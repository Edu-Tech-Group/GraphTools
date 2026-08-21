using System.Globalization;
using CsvHelper;
using GraphTools.Common;
using GraphTools.GroupTags;
using Microsoft.Graph;
using Microsoft.Graph.Beta.DeviceManagement.WindowsAutopilotDeviceIdentities.Item.UpdateDeviceProperties;
using Microsoft.Graph.Beta.Models;
using Microsoft.Graph.Beta.Models.ODataErrors;

try
{
    var dryRun = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
    var path = args.FirstOrDefault(argument => !argument.StartsWith("--", StringComparison.Ordinal));

    if (path == null)
    {
        Console.Write("CSV path:");
        path = Console.ReadLine();
    }

    if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
    {
        Console.WriteLine("CSV file not found");
        return;
    }

    if (dryRun)
    {
        Console.WriteLine("Dry run: Microsoft Graph and CSV data will be read, but no devices will be changed.");
    }

    var graphClient = GraphHelpers.GetGraphClient();

    var deviceIdentities = new List<WindowsAutopilotDeviceIdentity>();

    var pageIterator = PageIterator<WindowsAutopilotDeviceIdentity, WindowsAutopilotDeviceIdentityCollectionResponse>
        .CreatePageIterator(
            graphClient,
            (await graphClient.DeviceManagement.WindowsAutopilotDeviceIdentities.GetAsync())!,
            device =>
            {
                deviceIdentities.Add(device);
                return true;
            });

    await pageIterator.IterateAsync();

    using var reader = CreateCsvReader(path);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

    long requestCount = 0;
    await foreach (var deviceRow in csv.GetRecordsAsync<DeviceRow>())
    {
        foreach (var matchingDeviceIdentity in deviceIdentities.Where(x => x.SerialNumber == deviceRow.Serial))
        {
            if (dryRun)
            {
                Console.WriteLine($"Would update {deviceRow.Serial}: group tag '{deviceRow.Tag}'" +
                                  (deviceRow.Devicename == null ? string.Empty : $", device name '{deviceRow.Devicename}'"));
                continue;
            }

            if (requestCount == 99)
            {
                await Task.Delay(TimeSpan.FromSeconds(20));
                requestCount = 0;
            }

            try
            {
                var update = new UpdateDevicePropertiesPostRequestBody
                {
                    GroupTag = deviceRow.Tag
                };

                if (deviceRow.Devicename != null)
                {
                    update.DisplayName = deviceRow.Devicename;
                }

                await graphClient
                    .DeviceManagement
                    .WindowsAutopilotDeviceIdentities[matchingDeviceIdentity.Id]
                    .UpdateDeviceProperties.PostAsync(update);
            }
            catch (ODataError e)
            {
                Console.WriteLine($"Row with serial: {deviceRow.Serial} failed");
                Console.WriteLine(e);
            }

            requestCount++;
        }
    }

    Console.WriteLine("Done");
}
catch (Exception e)
{
    Console.WriteLine(e);
}

Console.WriteLine("Press ENTER to exit...");
Console.ReadLine();

static TextReader CreateCsvReader(string path)
{
    // Some Intune exports wrap an entire data row in quotes and double every
    // embedded quote. Unwrap only those rows so both standard CSV files and
    // current Intune portal exports can be consumed by CsvHelper.
    var lines = File.ReadLines(path).Select((line, index) =>
        index > 0 && line.Length >= 2 && line[0] == '"' && line[^1] == '"'
            ? line[1..^1].Replace("\"\"", "\"")
            : line);

    return new StringReader(string.Join(Environment.NewLine, lines));
}
