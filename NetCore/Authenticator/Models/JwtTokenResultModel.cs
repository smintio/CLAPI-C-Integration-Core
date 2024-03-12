using Newtonsoft.Json;
using System;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Models
{
    public class JwtRefreshTokenResultModel : RefreshTokenResultModel
    {
        public JwtRefreshTokenResultModel()
        {
        }

        [JsonProperty("access_token")]
        public string CustomAccessToken
        {
            set
            {
                AccessToken = value;
            }
        }

        [JsonProperty("refresh_token")]
        public string CustomRefreshToken
        {
            set
            {
                RefreshToken = value;
            }
        }

        [JsonProperty("token_type")]
        public string TokenType { set => string.Equals("Bearer", value); }

        [JsonProperty("error")]
        public string CustomErrorFlag { set => string.IsNullOrEmpty(value); }

        [JsonProperty("error_description")]
        private string ErrorDescription
        {
            set
            {
                ErrorMsg = value;
            }
        }

        [JsonProperty("expires_in")]
        private int ExpiresIn { set => Expiration = DateTimeOffset.Now.Add(TimeSpan.FromSeconds(value)); }
    }
}
