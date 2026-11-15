namespace Jcs.Net.Tests.Sorting;

public class PropertySortingTests
{
    [Fact]
    public void Rfc_section_3_2_3_sample_orders_by_utf16_code_units()
    {
        const string input = """
            {
              "€": "Euro Sign",
              "\r": "Carriage Return",
              "דּ": "Hebrew Letter Dalet With Dagesh",
              "1": "One",
              "😀": "Emoji: Grinning Face",
              "\u0080": "Control",
              "ö": "Latin Small Letter O With Diaeresis"
            }
            """;

        var canonical = JsonCanonicalizer.Canonicalize(input);

        var expectedValueOrder = new[]
        {
            "Carriage Return",
            "One",
            "Control",
            "Latin Small Letter O With Diaeresis",
            "Euro Sign",
            "Emoji: Grinning Face",
            "Hebrew Letter Dalet With Dagesh"
        };
        var actualValueOrder = System.Text.Json.JsonDocument.Parse(canonical)
            .RootElement.EnumerateObject().Select(property => property.Value.GetString()).ToArray();
        Assert.Equal(expectedValueOrder, actualValueOrder);
    }

    [Fact]
    public void Surrogate_pair_keys_sort_before_higher_basic_plane_keys()
    {
        const string input = """{"דּ":"dalet","😀":"emoji"}""";
        Assert.Equal(
            "{\"😀\":\"emoji\",\"דּ\":\"dalet\"}",
            JsonCanonicalizer.Canonicalize(input));
    }

    [Fact]
    public void Shorter_prefix_sorts_first()
    {
        const string input = """{"ab":3,"aa":2,"a":1,"":0}""";
        Assert.Equal("{\"\":0,\"a\":1,\"aa\":2,\"ab\":3}", JsonCanonicalizer.Canonicalize(input));
    }

    [Fact]
    public void Sorting_uses_unescaped_property_names()
    {
        const string input = """{"b":2,"a":1}""";
        Assert.Equal("{\"a\":1,\"b\":2}", JsonCanonicalizer.Canonicalize(input));
    }

    [Fact]
    public void Digits_sort_as_strings_not_numbers()
    {
        const string input = """{"10":null,"1":[],"d":true}""";
        Assert.Equal("{\"1\":[],\"10\":null,\"d\":true}", JsonCanonicalizer.Canonicalize(input));
    }

    [Fact]
    public void Arrays_keep_element_order_while_nested_objects_sort()
    {
        const string input = """[3,1,{"b":2,"a":1},2]""";
        Assert.Equal("[3,1,{\"a\":1,\"b\":2},2]", JsonCanonicalizer.Canonicalize(input));
    }
}
