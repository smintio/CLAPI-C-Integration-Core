using SmintIo.CLAPI.Consumer.Integration.Core.Database.Models;
using System.Threading.Tasks;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Database
{
    public interface ISettingsDatabaseProvider
    {
        Task<SettingsDatabaseModel> GetSettingsDatabaseModelAsync();
    }
}
