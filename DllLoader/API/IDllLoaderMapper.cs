using System.Collections.Generic;
using Oxide.Ext.DllLoader.Model;

namespace Oxide.Ext.DllLoader.API
{
    public interface IDllLoaderMapper
    {
        IEnumerable<string> ScanDirectoryPlugins(string directory);

        AssemblyInfo GetAssemblyInfoByName(string name);

        AssemblyInfo GetAssemblyInfoByPluginName(string name);

        PluginInfo GetPluginInfoByName(string pluginName);
    }
}