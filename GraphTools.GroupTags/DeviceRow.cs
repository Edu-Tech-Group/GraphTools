using CsvHelper.Configuration.Attributes;

namespace GraphTools.GroupTags;

public class DeviceRow
{
    [Name("Serial number")]
    public required string Serial { get; set; }
    
    [Name("Group tag")]
    public string? Tag { get; set; }
}