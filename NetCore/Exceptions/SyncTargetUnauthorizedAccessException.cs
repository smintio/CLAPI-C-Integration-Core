namespace SmintIo.CLAPI.Consumer.Integration.Core.Exceptions
{
    public class SyncTargetUnauthorizedAccessException : SyncTargetException
    {
        public SyncTargetUnauthorizedAccessException()
            : base("Authorization with sync target failed, e.g. because access token is not present or is expired")
        { }
    }
}
