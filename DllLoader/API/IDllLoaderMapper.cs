#region

using System.Collections.Generic;
using Mono.Cecil;
using Oxide.Ext.DllLoader.Model;

#endregion

namespace Oxide.Ext.DllLoader.API
{
    public interface IDllLoaderMapper : IAssemblyResolver
    {
        AssemblyInfo GetAssemblyInfoByFilename(string fileName);

        AssemblyInfo GetAssemblyInfoByPlugin(string pluginName);

        bool RemoveAssemblyInfo(string assemblyName);

        IEnumerable<string> ScanDirectoryPlugins(string directory);

        void ScanAndRegisterAssemblies(string directory);
    }
}