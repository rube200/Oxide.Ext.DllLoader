using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAOAssemblyDeep", "Rube200", "1.0.0")]
    [Description("RefAOAssemblyDeep is for testing")]
    public class RefAOAssemblyDeep : DepTestPlugin
    {
        [PluginReference]
        Plugin RefAPAssemblyDeep;

        protected override Plugin DepPlugin => RefAPAssemblyDeep;
        protected override string PuginName => nameof(RefAPAssemblyDeep);

        protected override void Init()
        {
            base.Init();
        }

        protected override void Loaded()
        {
            base.Loaded();
        }

        protected override void OnServerInitialized()
        {
            base.OnServerInitialized();
        }

        protected override void Unload()
        {
            base.Unload();
        }

        protected override void Shutdown()
        {
            base.Shutdown();
        }

        protected override void Hotloading()
        {
            base.Hotloading();
        }
    }
}