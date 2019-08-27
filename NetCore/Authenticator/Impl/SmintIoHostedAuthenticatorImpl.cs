using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Models;
using SmintIo.CLAPI.Consumer.Integration.Core.Database;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Impl
{
    public class SmintIoHostedAuthenticatorImpl : SmintIoAuthenticatorImpl
    {
        private readonly ISettingsDatabaseProvider _settingsDatabaseProvider;
        private readonly ITokenDatabaseProvider _tokenDatabaseProvider;

        private readonly ILogger _logger;

        public SmintIoHostedAuthenticatorImpl(
            ISettingsDatabaseProvider settingsDatabaseProvider,
            ITokenDatabaseProvider tokenDatabaseProvider,
            ILogger<SmintIoHostedAuthenticatorImpl> logger)
            : base(settingsDatabaseProvider, tokenDatabaseProvider, logger)
        {
            _settingsDatabaseProvider = settingsDatabaseProvider;
            _tokenDatabaseProvider = tokenDatabaseProvider;

            _logger = logger;
        }

#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        public override async Task<InitAuthenticationResultModel> InitSmintIoAuthenticationAsync()
#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        {
            throw new NotImplementedException();
        }

#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        public override async Task FinalizeSmintIoAuthenticationAsync(string authorizationCode)
#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        {
            throw new NotImplementedException();
        }
    }
}
