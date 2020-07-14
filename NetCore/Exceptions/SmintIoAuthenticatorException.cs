namespace SmintIo.CLAPI.Consumer.Integration.Core.Exceptions
{
    public class SmintIoAuthenticatorException : AuthenticatorException
    {
        public SmintIoAuthenticatorException(AuthenticatorError error, string message) 
            : base(error, message)
        { }
    }
}
