using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Contracts
{
    public class SmintIoLicenseOptions
    {
        public IDictionary<string, string> OptionName { get; set; }

        public IDictionary<string, string> LicenseText { get; set; }
    }
}
