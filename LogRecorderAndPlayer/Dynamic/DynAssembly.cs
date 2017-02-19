using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public enum AssemblyPathType
    {
        RelativePath,
        AbsolutePath
    }

    public static class DynamicAssembly
    {
        public static IEnumerable<T> LoadAssemblyInstances<T>(string assemblyFilename, AssemblyPathType assemblyPathType = AssemblyPathType.RelativePath)
        {
            if (assemblyPathType == AssemblyPathType.RelativePath)
                assemblyFilename = AppDomain.CurrentDomain.RelativeSearchPath.TrimEnd(System.IO.Path.DirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar + assemblyFilename;

            AssemblyName an = AssemblyName.GetAssemblyName(assemblyFilename);
            var hmm3 = Assembly.Load(an);

            Type[] iLoadTypes = (from t in hmm3.GetExportedTypes()
                                 where !t.IsInterface && !t.IsAbstract
                                 where typeof(T).IsAssignableFrom(t)
                                 select t).ToArray();

            return iLoadTypes.Select(t => (T)Activator.CreateInstance(t));
        }
    }
}
