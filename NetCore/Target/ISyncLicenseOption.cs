using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target.Impl
{
    public abstract class BaseSyncLicenseOption
    {
        public abstract void SetLicenseText(IDictionary<string, string> licenseText);
        public abstract void SetName(IDictionary<string, string> name);
    }
}
