using System;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Exceptions
{
    public class SmintIoSyncJobException : Exception
    {
        public enum SyncJobError
        {
            Generic
        }

        public SyncJobError Error { get; set; }

        public SmintIoSyncJobException(SyncJobError error, string message)
            : base(message)
        {
            Error = error;
        }
    }
}
