using System;
using System.Net;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Exceptions
{
    public class SyncTargetException : Exception
    {
        public int? ErrorCode { get; set; }

        public SyncTargetException(string message) : base(message)
        { }

        public SyncTargetException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public SyncTargetException(HttpStatusCode httpStatusCode, string message)
            : base(message)
        {
            ErrorCode = (int)httpStatusCode;
        }
    }
}
