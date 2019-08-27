using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Contracts
{
    public class SmintIoMetadataElement
    {
        public string Key { get; set; }

        public IDictionary<string, string> Values { get; set; }
    }
}
