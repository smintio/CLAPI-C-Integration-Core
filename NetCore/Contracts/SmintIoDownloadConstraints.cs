namespace SmintIo.CLAPI.Consumer.Integration.Core.Contracts
{
    public class SmintIoDownloadConstraints
    {
        public int? EffectiveMaxDownloads { get; set; }
        public int? EffectiveMaxUsers { get; set; }
        public int? EffectiveMaxReuses { get; set; }
    }
}
