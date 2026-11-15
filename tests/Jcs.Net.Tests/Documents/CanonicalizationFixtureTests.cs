using System.Text;

namespace Jcs.Net.Tests.Documents;

public class CanonicalizationFixtureTests
{
    public static TheoryData<string> FixtureNames => new()
    {
        "arrays", "french", "structures", "unicode", "values", "weird"
    };

    private static string FixturePath(string kind, string name) =>
        Path.Combine(AppContext.BaseDirectory, "fixtures", "documents", kind, name + ".json");

    private static string ReadInput(string name) => File.ReadAllText(FixturePath("input", name));

    private static byte[] ReadExpectedUtf8(string name)
    {
        var bytes = File.ReadAllBytes(FixturePath("expected", name));
        var length = bytes.Length;
        while (length > 0 && (bytes[length - 1] == (byte)'\n' || bytes[length - 1] == (byte)'\r'))
        {
            length--;
        }

        return bytes[..length];
    }

    [Theory]
    [MemberData(nameof(FixtureNames))]
    public void Canonicalize_matches_official_expected_output(string name)
    {
        var expected = Encoding.UTF8.GetString(ReadExpectedUtf8(name));
        Assert.Equal(expected, JsonCanonicalizer.Canonicalize(ReadInput(name)));
    }

    [Theory]
    [MemberData(nameof(FixtureNames))]
    public void CanonicalizeToUtf8_matches_official_expected_bytes(string name)
    {
        Assert.Equal(ReadExpectedUtf8(name), JsonCanonicalizer.CanonicalizeToUtf8(ReadInput(name)));
    }

    [Theory]
    [MemberData(nameof(FixtureNames))]
    public void Canonicalization_is_idempotent(string name)
    {
        var once = JsonCanonicalizer.Canonicalize(ReadInput(name));
        Assert.Equal(once, JsonCanonicalizer.Canonicalize(once));
    }

    [Fact]
    public void Rfc_section_3_2_4_utf8_bytes_match_exactly()
    {
        const string rfcExpectedHex =
            "7b226c69746572616c73223a5b6e756c6c2c747275652c66616c73655d2c226e756d62657273223a5b" +
            "3333333333333333332e333333333333332c31652b33302c342e352c302e3030322c31652d32375d2c" +
            "22737472696e67223a22e282ac245c75303030665c6e4127425c225c5c5c5c5c222f227d";
        var expected = Convert.FromHexString(rfcExpectedHex);
        Assert.Equal(expected, JsonCanonicalizer.CanonicalizeToUtf8(ReadInput("values")));
    }

    [Theory]
    [InlineData("null", "null")]
    [InlineData("true", "true")]
    [InlineData("false", "false")]
    [InlineData("  42  ", "42")]
    [InlineData("\"hi\"", "\"hi\"")]
    [InlineData("[]", "[]")]
    [InlineData("{}", "{}")]
    [InlineData("[ 1 , 2 , 3 ]", "[1,2,3]")]
    [InlineData("{ \"b\" : 2, \"a\" : 1 }", "{\"a\":1,\"b\":2}")]
    [InlineData("[true , false,null, \"x\" ]", "[true,false,null,\"x\"]")]
    [InlineData("{\"z\":{\"b\":[{\"y\":0,\"x\":1}],\"a\":9}}", "{\"z\":{\"a\":9,\"b\":[{\"x\":1,\"y\":0}]}}")]
    public void Whitespace_is_stripped_and_structure_preserved(string input, string expected)
    {
        Assert.Equal(expected, JsonCanonicalizer.Canonicalize(input));
    }
}
