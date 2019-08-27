using System.Collections.Generic;
using System.Threading.Tasks;
using SmintIo.CLAPI.Consumer.Integration.Core.Contracts;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Providers
{
    internal interface ISmintIoApiClientProvider
    {
        Task<SmintIoGenericMetadata> GetGenericMetadataAsync();

        Task<(IList<SmintIoAsset>, string)> GetAssetsAsync(string continuationUuid);
    }
}
