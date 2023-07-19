#region

using Oxide.Core;
using Oxide.Core.Extensions;
using Oxide.Ext.DllLoader.Manager;

#endregion

namespace Oxide.Ext.DllLoader
{
    // ReSharper disable once UnusedMember.Global
    public sealed class DllLoaderExtension : Extension
    {
        public override string Name => "DllLoader";
        public override string Author => "Rube200";
        public override VersionNumber Version => new VersionNumber(1, 1, 1);

        private DllLoaderPluginManager _pluginsLoader;


        public DllLoaderExtension(ExtensionManager manager) : base(manager)
        { }


        public override void Load()
        {
            _pluginsLoader = new DllLoaderPluginManager();
            Manager.RegisterPluginLoader(_pluginsLoader);

            Interface.Oxide.OnFrame(OnFrame);
        }

        public override void LoadPluginWatchers(string pluginDirectory)
        {
 
        }


        public override void OnModLoad()
        {
            _pluginsLoader.OnModLoaded();
        }

        public override void OnShutdown()
        {
            _pluginsLoader.OnShutdown();
        }

        private void OnFrame(float delta)
        {
            foreach (var plugin in _pluginsLoader.OnFramePlugins)
                plugin.CallHook("OnFrame", new [] { delta });
        }
    }
}