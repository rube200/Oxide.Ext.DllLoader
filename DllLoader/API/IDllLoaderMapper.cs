#region

using System.Collections.Generic;
using Mono.Cecil;
using Oxide.Ext.DllLoader.Model;

#endregion

namespace Oxide.Ext.DllLoader.API
{
    public interface IDllLoaderMapper : IAssemblyResolver
    {
        IReadOnlyCollection<PluginInfo> GetAllPlugins();

        AssemblyInfo? GetAssemblyInfoByFilename(string fileName);

        AssemblyInfo? GetAssemblyInfoByPlugin(string pluginName);

        IEnumerable<string> ScanDirectoryPlugins(string directory);

        void RemoveAssembly(AssemblyInfo assemblyInfo);

        void ScanAndRegisterAssemblies(string directory);
    }
}