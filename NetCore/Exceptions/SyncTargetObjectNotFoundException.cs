namespace SmintIo.CLAPI.Consumer.Integration.Core.Exceptions
{
    public class SyncTargetObjectNotFoundException : SyncTargetException
    {
        public string ObjectId { get; set; }

        public SyncTargetObjectNotFoundException(string objectId)
            : base($"Object with ID {objectId} was not found")
        {
            ObjectId = objectId;
        }
    }
}
