using System;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Models
{
    internal class RefreshTokenResultModel
    {
        public bool Success { get; set; }

        public string ErrorMsg { get; set; }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdentityToken { get; set; }

        public DateTimeOffset? Expiration { get; set; }
    }
}
