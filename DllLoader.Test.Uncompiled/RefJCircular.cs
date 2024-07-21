using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefJCircular", "Rube200", "1.0.0")]
    [Description("RefJCircular is for testing")]
    public class RefJCircular : DepTestPlugin
    {
        [PluginReference]
        Plugin RefKCircular;

        protected override Plugin DepPlugin => RefKCircular;
        protected override string PuginName => nameof(RefKCircular);

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