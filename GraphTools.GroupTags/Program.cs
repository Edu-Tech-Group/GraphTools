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

    long requestCount = 0;
    await foreach (var deviceRow in csv.GetRecordsAsync<DeviceRow>())
    {
        foreach (var matchingDeviceIdentity in deviceIdentities.Where(x => x.SerialNumber == deviceRow.Serial))
        {
            if (requestCount == 99)
            {
                await Task.Delay(TimeSpan.FromSeconds(20));
                requestCount = 0;
            }

            try
            {
                await graphClient.DeviceManagement.WindowsAutopilotDeviceIdentities[matchingDeviceIdentity.Id]
                    .UpdateDeviceProperties.PostAsync(
                        new UpdateDevicePropertiesPostRequestBody
                        {
                            DisplayName = deviceRow.Devicename,
                            GroupTag = deviceRow.Tag
                        });
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