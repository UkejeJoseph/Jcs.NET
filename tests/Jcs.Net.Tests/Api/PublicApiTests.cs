using System.Text;
using System.Text.Json;

namespace Jcs.Net.Tests.Api;

public class PublicApiTests
{
    private const string SampleInput = "{ \"b\": 2, \"a\": [1.0, \"x\"] }";
    private const string SampleCanonical = "{\"a\":[1,\"x\"],\"b\":2}";

    [Fact]
    public void Canonicalize_element_overload_matches_text_overload()
    {
        using var document = JsonDocument.Parse(SampleInput);
        Assert.Equal(SampleCanonical, JsonCanonicalizer.Canonicalize(document.RootElement));
        Assert.Equal(
            JsonCanonicalizer.Canonicalize(SampleInput),
            JsonCanonicalizer.Canonicalize(document.RootElement));
    }

    [Fact]
    public void CanonicalizeToUtf8_overloads_encode_the_canonical_string()
    {
        using var document = JsonDocument.Parse(SampleInput);
        var expected = Encoding.UTF8.GetBytes(SampleCanonical);
        Assert.Equal(expected, JsonCanonicalizer.CanonicalizeToUtf8(SampleInput));
        Assert.Equal(expected, JsonCanonicalizer.CanonicalizeToUtf8(document.RootElement));
    }

    [Fact]
    public void Undefined_element_throws_jcs_exception()
    {
        Assert.Throws<JcsException>(() => JsonCanonicalizer.Canonicalize(default(JsonElement)));
    }

    [Fact]
    public void Element_with_duplicate_members_throws_jcs_exception()
    {
        using var document = JsonDocument.Parse("{\"a\":1,\"a\":2}");
        Assert.Throws<JcsException>(() => JsonCanonicalizer.Canonicalize(document.RootElement));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("{")]
    [InlineData("{\"a\":1,\"a\":2}")]
    [InlineData("\"\\uDEAD\"")]
    [InlineData("1e999")]
    public void TryCanonicalize_returns_false_for_invalid_input(string? input)
    {
        Assert.False(JsonCanonicalizer.TryCanonicalize(input, out var canonical));
        Assert.Null(canonical);
    }

    [Fact]
    public void TryCanonicalize_returns_true_and_canonical_form_for_valid_input()
    {
        Assert.True(JsonCanonicalizer.TryCanonicalize(SampleInput, out var canonical));
        Assert.Equal(SampleCanonical, canonical);
    }
}
