using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefADCircularCross", "Rube200", "1.0.0")]
    [Description("RefADCircularCross is for testing")]
    public class RefADCircularCross : DepTestPlugin
    {
        [PluginReference]
        Plugin RefACCircularCross;

        protected override Plugin DepPlugin => RefACCircularCross;
        protected override string PuginName => nameof(RefACCircularCross);

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