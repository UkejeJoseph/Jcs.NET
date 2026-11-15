namespace Jcs.Net.Tests.Numbers;

public class NumberVectorFileTests
{
    private const int MinimumVectorCount = 10000;
    private const char FieldSeparator = ',';

    private static string VectorFilePath =>
        Path.Combine(AppContext.BaseDirectory, "fixtures", "numbers", "es6-number-vectors.txt");

    [Fact]
    public void Every_v8_generated_vector_formats_identically()
    {
        var lines = File.ReadAllLines(VectorFilePath).Where(line => line.Length > 0).ToArray();
        Assert.True(lines.Length >= MinimumVectorCount, $"Expected at least {MinimumVectorCount} vectors, found {lines.Length}.");

        var failures = new List<string>();
        foreach (var line in lines)
        {
            var separatorIndex = line.IndexOf(FieldSeparator);
            var bitsHex = line[..separatorIndex];
            var expected = line[(separatorIndex + 1)..];
            var value = BitConverter.UInt64BitsToDouble(
                ulong.Parse(bitsHex, System.Globalization.NumberStyles.HexNumber));

            var actual = EcmaScriptNumberFormatter.Format(value);
            if (actual != expected)
            {
                failures.Add($"{bitsHex}: expected {expected}, got {actual}");
            }
        }

        Assert.True(failures.Count == 0, $"{failures.Count} mismatches:\n{string.Join('\n', failures.Take(20))}");
    }

    [Fact]
    public void Every_vector_round_trips_through_full_canonicalization()
    {
        var lines = File.ReadAllLines(VectorFilePath).Where(line => line.Length > 0).ToArray();
        foreach (var line in lines)
        {
            var expected = line[(line.IndexOf(FieldSeparator) + 1)..];
            Assert.Equal(expected, JsonCanonicalizer.Canonicalize(expected));
        }
    }
}
