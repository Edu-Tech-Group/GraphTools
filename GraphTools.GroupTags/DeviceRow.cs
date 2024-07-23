using CsvHelper.Configuration.Attributes;

namespace GraphTools.GroupTags;

public class DeviceRow
{
    [Name("Serial number", "Serienummer")]
    public required string Serial { get; set; }
    
    [Name("Group tag", "Groepstag")]
    public string? Tag { get; set; }
    
    [Name("Devicename")]
    public string? Devicename { get; set; }
}