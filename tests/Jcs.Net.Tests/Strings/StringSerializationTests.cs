using System.Text;

namespace Jcs.Net.Tests.Strings;

public class StringSerializationTests
{
    private static string Serialize(string value)
    {
        var builder = new StringBuilder();
        JsonStringSerializer.Serialize(builder, value);
        return builder.ToString();
    }

    [Theory]
    [InlineData("", "\"\"")]
    [InlineData("hello", "\"hello\"")]
    [InlineData("\"", "\"\\\"\"")]
    [InlineData("\\", "\"\\\\\"")]
    [InlineData("/", "\"/\"")]
    [InlineData("a\"b\\c", "\"a\\\"b\\\\c\"")]
    public void Quote_and_backslash_are_the_only_escaped_printable_characters(string value, string expected)
    {
        Assert.Equal(expected, Serialize(value));
    }

    [Theory]
    [InlineData("\b", "\"\\b\"")]
    [InlineData("\t", "\"\\t\"")]
    [InlineData("\n", "\"\\n\"")]
    [InlineData("\f", "\"\\f\"")]
    [InlineData("\r", "\"\\r\"")]
    public void Predefined_control_characters_use_short_escapes(string value, string expected)
    {
        Assert.Equal(expected, Serialize(value));
    }

    [Theory]
    [InlineData(0x0000, "\"\\u0000\"")]
    [InlineData(0x0001, "\"\\u0001\"")]
    [InlineData(0x000b, "\"\\u000b\"")]
    [InlineData(0x000e, "\"\\u000e\"")]
    [InlineData(0x000f, "\"\\u000f\"")]
    [InlineData(0x001a, "\"\\u001a\"")]
    [InlineData(0x001f, "\"\\u001f\"")]
    public void Remaining_control_characters_use_lowercase_hex_escapes(int codePoint, string expected)
    {
        Assert.Equal(expected, Serialize(((char)codePoint).ToString()));
    }

    [Theory]
    [InlineData(0x007f)]
    [InlineData(0x0080)]
    [InlineData(0x00f6)]
    [InlineData(0x20ac)]
    [InlineData(0xfb33)]
    [InlineData(0x1f600)]
    [InlineData(0x1f602)]
    public void Characters_outside_the_control_range_stay_literal(int codePoint)
    {
        var value = char.ConvertFromUtf32(codePoint);
        Assert.Equal("\"" + value + "\"", Serialize(value));
    }

    [Fact]
    public void Rfc_sample_string_serializes_exactly()
    {
        Assert.Equal(
            "\"€$\\u000f\\nA'B\\\"\\\\\\\\\\\"/\"",
            Serialize("€$\u000f\nA'B\"\\\\\"/"));
    }

    [Theory]
    [InlineData("D800")]
    [InlineData("DEAD")]
    [InlineData("DE00")]
    [InlineData("0061-D800-007A")]
    [InlineData("D83D-0078")]
    [InlineData("DE00-D83D")]
    public void Unpaired_surrogates_are_rejected(string utf16CodeUnitsHex)
    {
        var value = FromCodeUnits(utf16CodeUnitsHex);
        Assert.Throws<JcsException>(() => Serialize(value));
    }

    private static string FromCodeUnits(string dashSeparatedHex) =>
        new(Array.ConvertAll(
            dashSeparatedHex.Split('-'),
            unit => (char)ushort.Parse(unit, System.Globalization.NumberStyles.HexNumber)));
}
