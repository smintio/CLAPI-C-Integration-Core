using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target
{
    public interface ISyncLicenseOption
    {
        void SetLicenseText(IDictionary<string, string> licenseText);
        void SetName(IDictionary<string, string> name);
    }
}
