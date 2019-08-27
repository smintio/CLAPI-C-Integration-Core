using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Contracts
{
    public class SmintIoBinary
    {
        public string Uuid { get; set; }

        public string ContentType { get; set; }
        public string BinaryType { get; set; }

        public IDictionary<string, string> Name { get; set; }
        public IDictionary<string, string> Description { get; set; }

        public IDictionary<string, string> Usage { get; set; }

        public string Category { get; set; }

        public string DownloadUrl { get; set; }

        public string RecommendedFileName { get; set; }
        
        public int Version { get; set; }
    }
}
