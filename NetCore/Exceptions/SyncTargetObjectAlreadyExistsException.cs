namespace SmintIo.CLAPI.Consumer.Integration.Core.Exceptions
{
    public class SyncTargetObjectAlreadyExistsException : SyncTargetException
    {
        public string ObjectName { get; set; }

        public SyncTargetObjectAlreadyExistsException(string objectName)
            : base($"{objectName} already exists")
        {
            ObjectName = objectName;
        }
    }
}
