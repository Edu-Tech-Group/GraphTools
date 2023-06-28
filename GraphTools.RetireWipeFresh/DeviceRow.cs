using CsvHelper.Configuration.Attributes;

namespace GraphTools.RetireWipeFresh;

public enum Action
{
    Wipe,
    Retire,
    Fresh
}

public class DeviceRow
{
    [Name("Serial number")] public required string Serial { get; set; }

    [Name("Action")] [EnumIgnoreCase] public Action? Action { get; set; }
    [Name("KeepEnrollmentData")] public bool? KeepEnrollmentData { get; set; }
    [Name("KeepUserData")] public bool? KeepUserData { get; set; }
}