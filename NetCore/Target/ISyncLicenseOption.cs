using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target
{
    public interface ISyncLicenseOption
    {
        void SetName(IDictionary<string, string> name);

        void SetLicenseText(IDictionary<string, string> licenseText);
    }
}
