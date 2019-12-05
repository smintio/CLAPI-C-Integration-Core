namespace SmintIo.CLAPI.Consumer.Integration.Core.Target.Impl
{
    public abstract class BaseSyncDownloadConstraints
    {
        public abstract void SetMaxUsers(int maxUsers);
        public abstract void SetMaxDownloads(int maxDownloads);
        public abstract void SetMaxReuses(int maxReuses);
    }
}
