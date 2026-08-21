using Azure.Identity;
using Microsoft.Graph.Beta;

namespace GraphTools.Common;

public static class GraphHelpers
{
    private const string ClientId = "f5d69c35-966e-4219-9bf6-9b22f05d4e8a";
    private const string TenantId = "organizations";

    public static GraphServiceClient GetGraphClient()
    {
        return new GraphServiceClient(new DeviceCodeCredential(new DeviceCodeCredentialOptions()
        {
            ClientId = ClientId,
            TenantId = TenantId,
            DeviceCodeCallback = (code, _) =>
            {
                Console.WriteLine(code.Message);
                return Task.CompletedTask;
            },
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions()
            {
                Name = "graphtools"
            }
        }), ["DeviceManagementServiceConfig.ReadWrite.All"]);
    }
}
