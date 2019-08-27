using System;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Exceptions
{
    public class SmintIoAuthenticatorException : Exception
    {
        public enum AuthenticatorError
        {
            SmintIoIntegrationWrongState,
            CannotAcquireSmintIoToken,
            CannotRefreshSmintIoToken,
            Generic
        }

        public AuthenticatorError Error { get; set; }

        public SmintIoAuthenticatorException(AuthenticatorError error, string message)
            : base(message)
        {
            Error = error;
        }
    }
}
