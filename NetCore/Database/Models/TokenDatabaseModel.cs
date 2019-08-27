using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;
using System;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Database.Models
{
    public class TokenDatabaseModel
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdentityToken { get; set; }

        public DateTimeOffset? Expiration { get; set; }

        internal void ValidateForTokenRefresh()
        {
            if (!Success || string.IsNullOrEmpty(RefreshToken))
                throw new SmintIoAuthenticatorException(SmintIoAuthenticatorException.AuthenticatorError.SmintIoIntegrationWrongState, "The refresh token is missing");
        }

        internal void ValidateForSync()
        {
            if (!Success || string.IsNullOrEmpty(AccessToken))
                throw new SmintIoAuthenticatorException(SmintIoAuthenticatorException.AuthenticatorError.SmintIoIntegrationWrongState, "The access token is missing");
        }

        internal void ValidateForPusher()
        {
            ValidateForSync();
        }
    }
}
