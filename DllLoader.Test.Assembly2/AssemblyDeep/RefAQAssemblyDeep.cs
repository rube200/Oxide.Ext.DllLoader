using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAQAssemblyDeep", "Rube200", "1.0.0")]
    [Description("RefAQAssemblyDeep is for testing")]
    public class RefAQAssemblyDeep : DepTestPlugin
    {
        [PluginReference]
        Plugin RefALAssemblyOrder;

        protected override Plugin DepPlugin => RefALAssemblyOrder;
        protected override string PuginName => nameof(RefALAssemblyOrder);

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