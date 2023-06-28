using Azure.Identity;
using Microsoft.Graph.Beta;

namespace GraphTools.Common;

public static class GraphHelpers
{
    public static GraphServiceClient GetGraphClient()
    {
        return new GraphServiceClient(new DeviceCodeCredential(new DeviceCodeCredentialOptions()
        {
            ClientId = "9743e9c0-d56a-4f27-84e5-d725213c354c",
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions()
            {
                Name = "graphtools"
            }
        }));
    }
}