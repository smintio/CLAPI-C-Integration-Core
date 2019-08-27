using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Database.Models
{
    public class SettingsDatabaseModel
    {
        public string TenantId { get; set; }
        public int? ChannelId { get; set; }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public string RedirectUri { get; set; }

        public string[] ImportLanguages { get; set; }

        internal void ValidateForAuthenticator()
        {
            if (string.IsNullOrEmpty(TenantId))
                throw new SmintIoAuthenticatorException(SmintIoAuthenticatorException.AuthenticatorError.SmintIoIntegrationWrongState, "The tenant ID is missing");

            if (string.IsNullOrEmpty(ClientId))
                throw new SmintIoAuthenticatorException(SmintIoAuthenticatorException.AuthenticatorError.SmintIoIntegrationWrongState, "The client ID is missing");

            if (string.IsNullOrEmpty(ClientSecret))
                throw new SmintIoAuthenticatorException(SmintIoAuthenticatorException.AuthenticatorError.SmintIoIntegrationWrongState, "The client secret is missing");

            if (string.IsNullOrEmpty(RedirectUri))
                throw new SmintIoAuthenticatorException(SmintIoAuthenticatorException.AuthenticatorError.SmintIoIntegrationWrongState, "The redirect URI is missing");
        }

        internal void ValidateForSync()
        {
            if (string.IsNullOrEmpty(TenantId))
                throw new SmintIoAuthenticatorException(SmintIoAuthenticatorException.AuthenticatorError.SmintIoIntegrationWrongState, "The tenant ID is missing");

            if (ImportLanguages == null || ImportLanguages.Length == 0)
                throw new SmintIoAuthenticatorException(SmintIoAuthenticatorException.AuthenticatorError.SmintIoIntegrationWrongState, "The import languages are missing");
        }

        internal void ValidateForPusher()
        {
            ValidateForSync();

            if (ChannelId == null)
                throw new SmintIoAuthenticatorException(SmintIoAuthenticatorException.AuthenticatorError.SmintIoIntegrationWrongState, "The channel ID is missing");
        }
    }
}
