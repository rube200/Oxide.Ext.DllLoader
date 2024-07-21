using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefKCircular", "Rube200", "1.0.0")]
    [Description("RefKCircular is for testing")]
    public class RefKCircular : DepTestPlugin
    {
        [PluginReference]
        Plugin RefJCircular;

        protected override Plugin DepPlugin => RefJCircular;
        protected override string PuginName => nameof(RefJCircular);

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