using System;
using System.Linq;
using System.Reflection;

namespace UIForia {

    public static class AppDomainExtensions {

        public static Assembly GetAssemblyByName(this AppDomain domain, string assemblyName) {
            return domain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
        }

    }

}