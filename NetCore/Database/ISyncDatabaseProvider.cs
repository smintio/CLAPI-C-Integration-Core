using SmintIo.CLAPI.Consumer.Integration.Core.Database.Models;
using System.Threading.Tasks;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Database
{
    public interface ISyncDatabaseProvider
    {
        Task<SyncDatabaseModel> GetSyncDatabaseModelAsync();

        Task SetSyncDatabaseModelAsync(SyncDatabaseModel syncDatabaseModel);
    }
}
