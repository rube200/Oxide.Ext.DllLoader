using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAUAssemblyCircular", "Rube200", "1.0.0")]
    [Description("RefAUAssemblyCircular is for testing")]
    public class RefAUAssemblyCircular : DepTestPlugin
    {
        [PluginReference] 
        Plugin RefATAssemblyCircular;

        protected override Plugin DepPlugin => RefATAssemblyCircular;
        protected override string PuginName => nameof(DepPlugin);

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