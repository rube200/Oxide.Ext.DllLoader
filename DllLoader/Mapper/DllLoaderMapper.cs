#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Oxide.Core;
using Oxide.Ext.DllLoader.API;
using Oxide.Ext.DllLoader.Controller;
using Oxide.Ext.DllLoader.Model;

#endregion

namespace Oxide.Ext.DllLoader.Mapper
{
    public sealed class DllLoaderMapper : IDllLoaderMapper
    {
        private readonly IDictionary<string, AssemblyInfo> _assembliesInfo = new Dictionary<string, AssemblyInfo>(StringComparer.OrdinalIgnoreCase);


        #region AssemblyResolver

        public void OnModLoad()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
        }

        public void OnShutdown()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
            _assembliesInfo.Clear();
        }

        private Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyInfo = GetAssemblyInfoByName(args.Name);
            if (assemblyInfo == null)
                return null;

            if (assemblyInfo.IsAssemblyLoaded || AssemblyController.LoadAssembly(assemblyInfo))
                return assemblyInfo.Assembly;

            RemoveAssemblyInfo(args.Name);
            return null;
        }

        #endregion


        #region ScanDiretory

        public IEnumerable<string> ScanDirectoryPlugins(string directory)
        {
            ScanAndRegisterAssemblies(directory);
            Interface.Oxide.LogDebug("Total assemblies registered({0}).", _assembliesInfo.Count);

            var plugins = _assembliesInfo.Values.SelectMany(ai => ai.PluginsName).ToArray();
            Interface.Oxide.LogDebug("Total plugins registered({0}).", plugins.Length);
            return plugins;
        }

        private void ScanAndRegisterAssemblies(string directory)
        {
            Interface.Oxide.LogDebug("Scanning directory({0}) for assemblies...", directory);
            if (!Directory.Exists(directory))
            {
                Interface.Oxide.LogWarning("Fail to scan directory({0}), directory not found.", directory);
                return;
            }

            var dirFiles = new DirectoryInfo(directory).GetFiles("*.dll", SearchOption.TopDirectoryOnly);
            foreach (var file in dirFiles)
            {
                Interface.Oxide.LogDebug("Found file({0}) in directory({1}).", file.Name, directory);
                if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    Interface.Oxide.LogInfo("Ignoring file({0}), marked as hidden.", file.Name);
                    continue;
                }

                var assemblyName = AssemblyController.GetAssemblyNameFromFile(file.FullName);
                if (_assembliesInfo.TryGetValue(assemblyName, out var assemblyInfo))
                {
                    if (assemblyInfo.LastWriteTimeUtc >= file.LastWriteTimeUtc)
                    {
                        Interface.Oxide.LogDebug("Assembly({0}) already registered.", file.FullName);
                        continue;
                    }

                    Interface.Oxide.LogDebug("Assembly({0}) already registered, but need to be reloaded.", file.FullName);
                }

                assemblyInfo = AssemblyController.LoadAssemblyInfo(file.FullName, file.LastWriteTimeUtc);
                if (assemblyInfo == null)
                {
                    Interface.Oxide.LogError("Fail to load assembly({0})", file.FullName);
                    continue;
                }


                Interface.Oxide.LogDebug("Assembly({0}) loaded from file({1}) in directory({2}).", assemblyInfo.OriginalName, file.Name, directory);
                _assembliesInfo[assemblyName] = assemblyInfo;
            }
        }

        #endregion


        #region Getters

        public AssemblyInfo GetAssemblyInfoByName(string name)
        {
            if (_assembliesInfo.TryGetValue(name, out var assemblyInfo))
                return assemblyInfo;
            
            return null;
        }

        public AssemblyInfo GetAssemblyInfoByFile(string filename)
        {
            return _assembliesInfo.Values.FirstOrDefault(assemblyInfo => assemblyInfo.IsFile(filename));
        }

        public AssemblyInfo GetAssemblyInfoByPluginName(string name)
        {
            return _assembliesInfo.Values.FirstOrDefault(assemblyInfo => assemblyInfo.ContainsPlugin(name));
        }

        public PluginInfo GetPluginInfoByName(string pluginName)
        {
            return _assembliesInfo.Values.Select(assemblyInfo => assemblyInfo.GetPluginInfo(pluginName))
                .FirstOrDefault(pluginInfo => pluginInfo != null);
        }

        #endregion

        public void RemoveAssemblyInfo(string assemblyName)
        {
            _assembliesInfo.Remove(assemblyName);
        }
    }
}