using System.Globalization;
using CsvHelper;
using GraphTools.Common;
using GraphTools.RetireWipeFresh;
using Microsoft.Graph;
using Microsoft.Graph.Beta.DeviceManagement.ManagedDevices.Item.CleanWindowsDevice;
using Microsoft.Graph.Beta.DeviceManagement.ManagedDevices.Item.Wipe;
using Microsoft.Graph.Beta.Models;
using Action = GraphTools.RetireWipeFresh.Action;

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
        if (deviceRow.Action != null)
        {
            foreach (var matchingDeviceIdentity in deviceIdentities.Where(x => x.SerialNumber == deviceRow.Serial))
            {
                if (deviceRow.Action == null)
                {
                    continue;
                }

                if (requestCount == 99)
                {
                    await Task.Delay(TimeSpan.FromSeconds(20));
                    requestCount = 0;
                }

                switch (deviceRow.Action)
                {
                    case Action.Wipe:
                        await graphClient.DeviceManagement.ManagedDevices[matchingDeviceIdentity.ManagedDeviceId].Wipe
                            .PostAsync(new WipePostRequestBody
                            {
                                KeepEnrollmentData = deviceRow.KeepEnrollmentData,
                                KeepUserData = deviceRow.KeepUserData
                            });
                        break;
                    case Action.Retire:
                        await graphClient.DeviceManagement.ManagedDevices[matchingDeviceIdentity.ManagedDeviceId].Retire
                            .PostAsync();
                        break;
                    case Action.Fresh:
                        await graphClient.DeviceManagement.ManagedDevices[matchingDeviceIdentity.ManagedDeviceId]
                            .CleanWindowsDevice.PostAsync(
                                new CleanWindowsDevicePostRequestBody
                                {
                                    KeepUserData = deviceRow.KeepUserData
                                });
                        break;
                    case null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                requestCount++;
            }
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