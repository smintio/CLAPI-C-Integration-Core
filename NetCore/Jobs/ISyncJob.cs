using System.Threading.Tasks;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Jobs
{
    internal interface ISyncJob
    {
        Task SynchronizeAsync(bool synchronizeGenericMetadata);
    }
}
