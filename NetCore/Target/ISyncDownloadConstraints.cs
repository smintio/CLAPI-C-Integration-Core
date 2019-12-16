namespace SmintIo.CLAPI.Consumer.Integration.Core.Target
{
    public interface ISyncDownloadConstraints
    {
        void SetMaxUsers(int maxUsers);
        void SetMaxDownloads(int maxDownloads);
        void SetMaxReuses(int maxReuses);
    }
}
