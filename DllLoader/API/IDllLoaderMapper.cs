#region

using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Oxide.Ext.DllLoader.Model;

#endregion

namespace Oxide.Ext.DllLoader.API
{
    public interface IDllLoaderMapper : IAssemblyResolver
    {
        AssemblyInfo GetAssemblyInfoByPlugin(string pluginName);

        bool LoadAssembly(AssemblyInfo assemblyInfo);

        bool RegisterAssemblyFromFile(FileInfo fileInfo);

        bool RemoveAssemblyInfo(AssemblyNameReference assemblyNameReference);

        IEnumerable<string> ScanDirectoryPlugins(string directory);
    }
}