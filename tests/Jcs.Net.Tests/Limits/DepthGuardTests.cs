using System.Text;
using System.Text.Json;

namespace Jcs.Net.Tests.Limits;

public class DepthGuardTests
{
    private const int MaxNestingDepth = 64;

    private static string NestedArrays(int depth) =>
        new string('[', depth) + new string(']', depth);

    private static JsonElement ParseAtDepth(string json)
    {
        var options = new JsonDocumentOptions { MaxDepth = 100_000 };
        var document = JsonDocument.Parse(json, options);
        return document.RootElement.Clone();
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1_000)]
    [InlineData(5_000)]
    public void Canonicalize_element_throws_catchable_exception_for_deep_nesting(int depth)
    {
        var element = ParseAtDepth(NestedArrays(depth));

        var exception = Assert.Throws<JcsException>(() => JsonCanonicalizer.Canonicalize(element));
        Assert.Contains(MaxNestingDepth.ToString(), exception.Message);
    }

    [Fact]
    public void Deep_element_exception_is_caught_by_json_exception_handler()
    {
        var element = ParseAtDepth(NestedArrays(5_000));

        var caught = false;
        try
        {
            JsonCanonicalizer.Canonicalize(element);
        }
        catch (JsonException)
        {
            caught = true;
        }

        Assert.True(caught);
    }

    [Fact]
    public void CanonicalizeToUtf8_element_throws_catchable_exception_for_deep_nesting()
    {
        var element = ParseAtDepth(NestedArrays(5_000));

        Assert.Throws<JcsException>(() => JsonCanonicalizer.CanonicalizeToUtf8(element));
    }

    [Fact]
    public void TryCanonicalize_returns_false_and_does_not_throw_for_deep_input()
    {
        var deep = NestedArrays(5_000);

        var result = JsonCanonicalizer.TryCanonicalize(deep, out var canonical);

        Assert.False(result);
        Assert.Null(canonical);
    }

    [Fact]
    public void Depth_at_limit_canonicalizes_identically_through_both_paths()
    {
        var atLimit = NestedArrays(MaxNestingDepth);

        var fromText = JsonCanonicalizer.Canonicalize(atLimit);
        var fromElement = JsonCanonicalizer.Canonicalize(ParseAtDepth(atLimit));

        Assert.Equal(atLimit, fromText);
        Assert.Equal(fromText, fromElement);
    }

    [Fact]
    public void Depth_one_beyond_limit_is_rejected_on_both_paths()
    {
        var beyondLimit = NestedArrays(MaxNestingDepth + 1);

        Assert.ThrowsAny<JsonException>(() => JsonCanonicalizer.Canonicalize(beyondLimit));
        Assert.Throws<JcsException>(() => JsonCanonicalizer.Canonicalize(ParseAtDepth(beyondLimit)));
    }

    [Fact]
    public void Guard_does_not_alter_shallow_output()
    {
        const string input = "{ \"b\": 2, \"a\": [1.0, \"x\"] }";
        var expected = Encoding.UTF8.GetBytes("{\"a\":[1,\"x\"],\"b\":2}");

        using var document = JsonDocument.Parse(input);

        Assert.Equal(expected, JsonCanonicalizer.CanonicalizeToUtf8(input));
        Assert.Equal(expected, JsonCanonicalizer.CanonicalizeToUtf8(document.RootElement));
    }
}
