using System.Globalization;
using CsvHelper;
using GraphTools.Common;
using GraphTools.GroupTags;
using Microsoft.Graph;
using Microsoft.Graph.Beta.DeviceManagement.WindowsAutopilotDeviceIdentities.Item.UpdateDeviceProperties;
using Microsoft.Graph.Beta.Models;

Console.Write("CSV path:");
var path = Console.ReadLine();

if (path == null)
{
    Console.WriteLine("Invalid path");
    return;
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

using var reader = new StreamReader(path);
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

await foreach (var deviceRow in csv.GetRecordsAsync<DeviceRow>())
{
    foreach (var matchingDeviceIdentity in deviceIdentities.Where(x => x.SerialNumber == deviceRow.Serial))
    {
        await graphClient.DeviceManagement.WindowsAutopilotDeviceIdentities[matchingDeviceIdentity.Id]
            .UpdateDeviceProperties.PostAsync(
                new UpdateDevicePropertiesPostRequestBody
                {
                    DisplayName = deviceRow.Devicename,
                    GroupTag = deviceRow.Tag
                });
    }
}

Console.WriteLine("Done");