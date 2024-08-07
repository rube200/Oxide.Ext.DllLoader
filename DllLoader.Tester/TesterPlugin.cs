﻿using DllLoader.Test.Libs;
using HarmonyLib;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Random = Oxide.Core.Random;

namespace DllLoader.Tester
{
    [Info("TesterPlugin", "Rube200", "1.0.0")]
    [Description("Plugin to automatically test DllLoader")]
    public class TesterPlugin : RustPlugin
    {
        private const int NumberOfTestPlugins = 51;
        private const byte NumberOfPluginsToTest = 15;

        private int GetPluginsCount()
        {
            return Manager.GetPlugins().Count();
        }

        [ConsoleCommand("dllloader.test")]
        private void TestCommand()
        {
            Task.Run(async () =>
            {
                try
                {
                    Puts("=============== Initializing tests ===============");
                    for (var k = 0; k < 3; k++)
                        await ExecuteTest();
                    Puts("=============== Tests finished ===============");
                }
                catch (Exception ex)
                {
                    Interface.Oxide.LogException("Tests failed", ex);
                }
            });
        }

        private async Task ExecuteTest()
        {
            Puts("======== Checking plugins count ========");
            NumberOfTestPlugins.AssertEqual(GetPluginsCount());

            CheckLoaded();

            Puts("======== Choosing {0} to unload ========", NumberOfPluginsToTest);
            var pluginsToTest = GetRandomPlugins();

            Puts("======== Unloading plugins ========");
            foreach (var pluginName in pluginsToTest)
            {
                var success = Interface.Oxide.UnloadPlugin(pluginName);
                if (!success)
                {
                    Puts("Fail to unload plugin({0})", pluginName);
                }
            }

            await Task.Delay(100);
            CheckLoaded(pluginsToTest);

            Puts("======== Loading plugins ========");
            foreach (var pluginName in pluginsToTest)
            {
                var success = Interface.Oxide.LoadPlugin(pluginName);
                if (!success)
                {
                    Puts("Fail to load plugin({0})", pluginName);
                }
            }

            await CheckLoadedWithDelay();

            Puts("======== Choosing {0} to reload ========", NumberOfPluginsToTest);
            pluginsToTest = GetRandomPlugins();

            Puts("======== Reloading plugins ========");
            foreach (var pluginName in pluginsToTest)
            {
                var success = Interface.Oxide.ReloadPlugin(pluginName);
                if (!success)
                {
                    Puts("Fail to reload plugin({0})", pluginName);
                }
            }

            await CheckLoadedWithDelay();
        }

        private async Task CheckLoadedWithDelay()
        {
            await Task.Delay(5000);
            var time = CheckLoaded();
            await Task.Delay(2000 + (int)(time * 1000));
        }

        private float CheckLoaded(ICollection<string>? pluginsUnloaded = null)  
        {
            var maxTime = 3f;
            Puts("======== Checking plugins loaded ========"); 
            foreach (var plugin in Manager.GetPlugins().OfType<DepTestPlugin>())
            {
                var isDepUnloaded = pluginsUnloaded?.Contains(plugin.PluginName) ?? false;
                plugin.CheckPluginLoaded(isDepUnloaded);
                if (isDepUnloaded)
                    AssertUtils.AssertNull(plugin.DepPlugin);

                maxTime = Math.Max(maxTime, plugin.DelayTime);
            }

            return maxTime;
        }

        private ICollection<string> GetRandomPlugins()
        {
            var pluginsCollection = Manager.GetPlugins();
            var pluginsToTest = new List<string>();
            var pluginsDep = new List<string>();
            for (int i = 0; i < NumberOfPluginsToTest; i++)
            { 
                var pluginIndex = Random.Range(pluginsCollection.Count());
                var plugin = pluginsCollection.ElementAt(pluginIndex);
                if (plugin == this || plugin.Author != "Rube200" || pluginsToTest.Contains(plugin.Name))
                {
                    i--;
                    continue;
                }

                pluginsToTest.Add(plugin.Name);
                var pluginType = pluginsToTest.GetType();
                AccessTools
                    .GetDeclaredFields(pluginType)
                    .OfType<MemberInfo>()
                    .Union(AccessTools.GetDeclaredProperties(pluginType))
                    .Do(m =>
                    {
                        var attribute = m.GetCustomAttribute<PluginReferenceAttribute>(true);
                        if (attribute == null)
                            return;

                        var memberType = m.GetUnderlyingType();
                        if (!memberType!.IsSubclassOf(typeof(Plugin)))
                            return;

                        pluginsDep.Add(memberType.Name);
                    });
            }
            pluginsToTest.AddRange(pluginsDep);
            return pluginsToTest;
        }
    }

    public static class AssertUtils
    {
        public static bool AssertEqual<T>(this T expected, T? current)
        {
            return AssertEqual("Assertion failed. Expected: {0} | Current: {1}", expected, current);
        }

        public static bool AssertEqual<T>(this string msg, T? expected, T? current)
        {
            if (expected != null && expected.Equals(current))
                return true;

            throw new Exception(string.Format(msg, expected, current));
        }

        internal static void AssertNull(Plugin? plugin)
        {
            if (plugin != null)
                throw new Exception(string.Format("Plugin ({0}) is not null", plugin.Name));
        }
    }
}
