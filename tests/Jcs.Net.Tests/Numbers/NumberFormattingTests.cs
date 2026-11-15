namespace Jcs.Net.Tests.Numbers;

public class NumberFormattingTests
{
    private static double FromBits(string ieee754Hex) =>
        BitConverter.UInt64BitsToDouble(ulong.Parse(ieee754Hex, System.Globalization.NumberStyles.HexNumber));

    [Theory]
    [InlineData("0000000000000000", "0")]
    [InlineData("8000000000000000", "0")]
    [InlineData("0000000000000001", "5e-324")]
    [InlineData("8000000000000001", "-5e-324")]
    [InlineData("7fefffffffffffff", "1.7976931348623157e+308")]
    [InlineData("ffefffffffffffff", "-1.7976931348623157e+308")]
    [InlineData("4340000000000000", "9007199254740992")]
    [InlineData("c340000000000000", "-9007199254740992")]
    [InlineData("4430000000000000", "295147905179352830000")]
    [InlineData("44b52d02c7e14af5", "9.999999999999997e+22")]
    [InlineData("44b52d02c7e14af6", "1e+23")]
    [InlineData("44b52d02c7e14af7", "1.0000000000000001e+23")]
    [InlineData("444b1ae4d6e2ef4e", "999999999999999700000")]
    [InlineData("444b1ae4d6e2ef4f", "999999999999999900000")]
    [InlineData("444b1ae4d6e2ef50", "1e+21")]
    [InlineData("3eb0c6f7a0b5ed8c", "9.999999999999997e-7")]
    [InlineData("3eb0c6f7a0b5ed8d", "0.000001")]
    [InlineData("41b3de4355555553", "333333333.3333332")]
    [InlineData("41b3de4355555554", "333333333.33333325")]
    [InlineData("41b3de4355555555", "333333333.3333333")]
    [InlineData("41b3de4355555556", "333333333.3333334")]
    [InlineData("41b3de4355555557", "333333333.33333343")]
    [InlineData("becbf647612f3696", "-0.0000033333333333333333")]
    [InlineData("43143ff3c1cb0959", "1424953923781206.2")]
    public void Format_matches_rfc8785_appendix_b(string ieee754Hex, string expected)
    {
        Assert.Equal(expected, EcmaScriptNumberFormatter.Format(FromBits(ieee754Hex)));
    }

    [Theory]
    [InlineData("3ff0000000000000", "1")]
    [InlineData("bff0000000000000", "-1")]
    [InlineData("4024000000000000", "10")]
    [InlineData("4059000000000000", "100")]
    [InlineData("3fe0000000000000", "0.5")]
    [InlineData("3fd0000000000000", "0.25")]
    [InlineData("4025000000000000", "10.5")]
    [InlineData("3f60624dd2f1a9fc", "0.002")]
    [InlineData("4012000000000000", "4.5")]
    [InlineData("46293e5939a08cea", "1e+30")]
    [InlineData("3a53ce9a36f23c10", "1e-27")]
    [InlineData("4415af1d78b58c40", "100000000000000000000")]
    [InlineData("430c6bf526340000", "1000000000000000")]
    [InlineData("4341c37937e08000", "10000000000000000")]
    [InlineData("419d6f34547e6b75", "123456789.12345679")]
    [InlineData("3fb999999999999a", "0.1")]
    [InlineData("3fc999999999999a", "0.2")]
    [InlineData("3fd3333333333333", "0.3")]
    [InlineData("4330000000000000", "4503599627370496")]
    [InlineData("433fffffffffffff", "9007199254740991")]
    [InlineData("c33fffffffffffff", "-9007199254740991")]
    [InlineData("000000000000000a", "5e-323")]
    [InlineData("0000000000000003", "1.5e-323")]
    [InlineData("0000000000002710", "4.9407e-320")]
    [InlineData("0010000000000000", "2.2250738585072014e-308")]
    [InlineData("000fffffffffffff", "2.225073858507201e-308")]
    [InlineData("3eb4b6231abfd271", "0.0000012345")]
    [InlineData("3e8091b5aeffdb8e", "1.2345e-7")]
    [InlineData("4ac07f99e523818a", "1.23456e+52")]
    [InlineData("41fad27480000000", "7200000000")]
    [InlineData("400921fb54442d18", "3.141592653589793")]
    [InlineData("3e7ad7f29abcaf48", "1e-7")]
    [InlineData("3eb07111652d2b5c", "9.8e-7")]
    [InlineData("4450bb448ec2f608", "1.2345678901234568e+21")]
    [InlineData("4480f0cf064dd592", "1e+22")]
    [InlineData("3eb0c6f7a0b5ed92", "0.000001000000000000001")]
    [InlineData("390b860bde023111", "6.62607015e-34")]
    [InlineData("44dfe185ca57c517", "6.02214076e+23")]
    public void Format_matches_ecmascript_tostring(string ieee754Hex, string expected)
    {
        Assert.Equal(expected, EcmaScriptNumberFormatter.Format(FromBits(ieee754Hex)));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Format_rejects_values_outside_json(double value)
    {
        Assert.Throws<JcsException>(() => EcmaScriptNumberFormatter.Format(value));
    }
}
