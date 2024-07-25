#region

using System.Collections.Generic;
using Mono.Cecil;
using Oxide.Ext.DllLoader.Model;

#endregion

namespace Oxide.Ext.DllLoader.API
{
    public interface IDllLoaderMapper
    {
        AssemblyInfo GetAssemblyInfoByPlugin(string pluginName);

        bool LoadAssembly(AssemblyInfo assemblyInfo);

        void OnModLoad();

        void OnShutdown();

        bool RemoveAssemblyInfo(AssemblyNameReference assemblyNameReference);

        IEnumerable<string> ScanDirectoryPlugins(string directory);
    }
}