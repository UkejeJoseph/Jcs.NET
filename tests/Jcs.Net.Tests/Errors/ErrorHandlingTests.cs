using System.Text.Json;

namespace Jcs.Net.Tests.Errors;

public class ErrorHandlingTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{")]
    [InlineData("[1,]")]
    [InlineData("{\"a\":}")]
    [InlineData("{'a':1}")]
    [InlineData("undefined")]
    [InlineData("{\"a\":1} extra")]
    [InlineData("01")]
    [InlineData("+1")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-Infinity")]
    public void Invalid_json_throws_json_exception(string input)
    {
        Assert.ThrowsAny<JsonException>(() => JsonCanonicalizer.Canonicalize(input));
    }

    [Theory]
    [InlineData("1e400")]
    [InlineData("-1e400")]
    [InlineData("1.4e+9999")]
    [InlineData("[1e309]")]
    [InlineData("{\"giant\":2e308}")]
    public void Numbers_outside_double_range_throw_jcs_exception(string input)
    {
        Assert.Throws<JcsException>(() => JsonCanonicalizer.Canonicalize(input));
    }

    [Theory]
    [InlineData("{\"a\":1,\"a\":2}")]
    [InlineData("{\"a\":1,\"b\":{\"x\":1,\"x\":2}}")]
    [InlineData("[{\"dup\":true,\"dup\":true}]")]
    [InlineData("{\"a\":1,\"\\u0061\":2}")]
    public void Duplicate_member_names_throw_jcs_exception(string input)
    {
        Assert.Throws<JcsException>(() => JsonCanonicalizer.Canonicalize(input));
    }

    [Theory]
    [InlineData("\"\\uDEAD\"")]
    [InlineData("\"\\ud800\"")]
    [InlineData("\"a\\uD83Dz\"")]
    [InlineData("\"\\uDE00\\uD83D\"")]
    [InlineData("{\"\\uD800\":1}")]
    [InlineData("[\"ok\",\"\\uDC00\"]")]
    public void Lone_surrogate_escapes_throw_jcs_exception(string input)
    {
        Assert.Throws<JcsException>(() => JsonCanonicalizer.Canonicalize(input));
    }

    [Theory]
    [InlineData("\"\\uD83D\\uDE00\"")]
    [InlineData("{\"\\uD83D\\uDE00\":\"pair\"}")]
    [InlineData("\"\\\\uDEAD\"")]
    public void Valid_pairs_and_escaped_backslash_lookalikes_are_accepted(string input)
    {
        Assert.NotNull(JsonCanonicalizer.Canonicalize(input));
    }

    [Fact]
    public void Null_input_throws_argument_null_exception()
    {
        Assert.Throws<ArgumentNullException>(() => JsonCanonicalizer.Canonicalize((string)null!));
        Assert.Throws<ArgumentNullException>(() => JsonCanonicalizer.CanonicalizeToUtf8((string)null!));
    }

    [Fact]
    public void Jcs_exception_is_a_json_exception()
    {
        Assert.IsAssignableFrom<JsonException>(new JcsException("message"));
    }

    [Fact]
    public void Element_with_lone_surrogate_string_value_throws_jcs_exception()
    {
        using var document = JsonDocument.Parse("{\"k\":\"\\uDEAD\"}");
        var exception = Assert.Throws<JcsException>(() => JsonCanonicalizer.Canonicalize(document.RootElement));
        Assert.Contains("String value", exception.Message);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public void Element_with_lone_surrogate_property_name_throws_jcs_exception()
    {
        using var document = JsonDocument.Parse("{\"\\uD800\":1}");
        var exception = Assert.Throws<JcsException>(() => JsonCanonicalizer.Canonicalize(document.RootElement));
        Assert.Contains("Property name", exception.Message);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public void Element_with_lone_surrogate_value_inside_array_throws_jcs_exception()
    {
        using var document = JsonDocument.Parse("[\"ok\",\"\\uDC00\"]");
        Assert.Throws<JcsException>(() => JsonCanonicalizer.Canonicalize(document.RootElement));
    }

    [Fact]
    public void Literal_lone_surrogate_outside_string_body_is_malformed_json_not_a_surrogate_violation()
    {
        var input = "{" + (char)0xD800 + "\"a\":1}";
        var exception = Assert.ThrowsAny<JsonException>(() => JsonCanonicalizer.Canonicalize(input));
        Assert.DoesNotContain("3.2.2.2", exception.Message);
        Assert.False(JsonCanonicalizer.TryCanonicalize(input, out var canonical));
        Assert.Null(canonical);
    }

    [Theory]
    [InlineData(0xD83D, "\\uDE00")]
    [InlineData(0xDE00, "\\uD83D")]
    public void Surrogate_pairs_split_across_literal_and_escaped_representations_are_rejected(
        int literalCodeUnit, string escapeText)
    {
        var literalFirst = "\"" + (char)literalCodeUnit + escapeText + "\"";
        Assert.Throws<JcsException>(() => JsonCanonicalizer.Canonicalize(literalFirst));

        var escapeFirst = "\"" + escapeText + (char)literalCodeUnit + "\"";
        Assert.Throws<JcsException>(() => JsonCanonicalizer.Canonicalize(escapeFirst));
    }
}
