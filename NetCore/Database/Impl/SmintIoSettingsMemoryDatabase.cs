using System.Threading.Tasks;
using Client.Options;
using SmintIo.CLAPI.Consumer.Integration.Core.Database.Models;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Database.Impl
{
    public class SmintIoSettingsMemoryDatabase : ISmintIoSettingsDatabaseProvider
    {
        private readonly SmintIoSettingsDatabaseModel _smintIoSettingsDatabaseModel;

        public SmintIoSettingsMemoryDatabase(
            SmintIoAppOptions appOptions,
            SmintIoAuthOptions authOptions)
        {
            _smintIoSettingsDatabaseModel = new SmintIoSettingsDatabaseModel()
            {
                TenantId = appOptions.TenantId,
                ChannelId = appOptions.ChannelId,
                ClientId = authOptions.ClientId,
                ClientSecret = authOptions.ClientSecret,
                RedirectUri = authOptions.RedirectUri,
                ImportLanguages = appOptions.ImportLanguages,
                RefreshToken = authOptions.RefreshToken
            };
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<SmintIoSettingsDatabaseModel> GetSmintIoSettingsDatabaseModelAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return _smintIoSettingsDatabaseModel;
        }
    }
}