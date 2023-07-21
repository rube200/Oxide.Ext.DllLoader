#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Oxide.Core;
using Oxide.Ext.DllLoader.Controller;
using Oxide.Ext.DllLoader.Model;

#endregion

namespace Oxide.Ext.DllLoader.Mapper
{
    public sealed class DllLoaderMapper
    {
        private readonly ISet<AssemblyInfo> _assembliesInfo = new HashSet<AssemblyInfo>();


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
            return GetAssemblyByOriginalName(args.Name);
        }
        #endregion


        #region ScanDiretory
        public IEnumerable<string> ScanDirectoryPlugins(string directory)
        {
            ScanAndRegisterAssemblies(directory);
            Interface.Oxide.LogDebug("Total assemblies registered({0}).", _assembliesInfo.Count);

            foreach (var assemblyInfo in _assembliesInfo)
            {
                Interface.Oxide.LogDebug("Found {0} plugins for assembly({1}).", assemblyInfo.PluginsName.Count, assemblyInfo.OriginalName);
                foreach (var pluginName in assemblyInfo.PluginsName)
                    yield return pluginName;
            }
        }

        private void ScanAndRegisterAssemblies(string directory)
        {
            Interface.Oxide.LogDebug("Scanning directory({0}) for assemblies...", directory);
            if (!Directory.Exists(directory))
            {
                Interface.Oxide.LogDebug("Fail to scan directory({0}), directory not found.", directory);
                return;
            }

            var dirFiles = new DirectoryInfo(directory).GetFiles("*.dll", SearchOption.TopDirectoryOnly);
            foreach (var file in dirFiles)
            {
                Interface.Oxide.LogDebug("Found file({0}) in directory({1}).", file.Name, directory);
                if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    Interface.Oxide.LogDebug("Ignoring file({0}), marked as hidden.", file.Name);
                    continue;
                }

                var assemblyInfo = AssemblyController.LoadAssemblyInfo(file.FullName);
                if (assemblyInfo == null)
                {
                    Interface.Oxide.LogWarning("Fail to load assembly({0})", file.FullName);
                    continue;
                }

                if (_assembliesInfo.Contains(assemblyInfo))
                {
                    Interface.Oxide.LogDebug("Assembly({0}) already registered.", file.FullName);
                    continue;
                }

                Interface.Oxide.LogDebug("Assembly({0}) loaded from file({1}) in directory({2}).", assemblyInfo.OriginalName, file.Name, directory);
                _assembliesInfo.Add(assemblyInfo);
            }
        }
        #endregion


        #region Getters
        public Assembly GetAssemblyByOriginalName(string originalName)
        {
            var assemblyInfo = _assembliesInfo.FirstOrDefault(ai => ai.OriginalName.Equals(originalName, StringComparison.OrdinalIgnoreCase));
            if (assemblyInfo == null)
                return null;

            if (assemblyInfo.IsAssemblyLoaded || AssemblyController.LoadAssembly(assemblyInfo))
                return assemblyInfo.Assembly;

            RemoveAssemblyInfo(assemblyInfo);
            return null;
        }

        public AssemblyInfo GetAssemblyInfoByPluginName(string name)
        {
            return _assembliesInfo.FirstOrDefault(assemblyInfo => assemblyInfo.ContainsPlugin(name));
        }

        public PluginInfo GetPluginInfoByName(string pluginName)
        {
            return _assembliesInfo.Select(assemblyInfo => assemblyInfo.GetPluginInfo(pluginName)).FirstOrDefault(pluginInfo => pluginInfo != null);
        }
        #endregion


        public void RemoveAssemblyInfo(AssemblyInfo assemblyInfo)
        {
            _assembliesInfo.Remove(assemblyInfo);
        }
    }
}