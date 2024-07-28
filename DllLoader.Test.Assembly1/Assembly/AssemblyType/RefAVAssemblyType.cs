using DllLoader.Test.Libs;
using Oxide.Core.Plugins;
using Oxide.Plugins;

namespace DllLoader.Test.Assembly1.Type
{
    [Info("RefAVAssemblyType", "Rube200", "1.0.0")]
    [Description("RefAVAssemblyType is for testing")]
    public class RefAVAssemblyType : DepTestPlugin
    {
        [PluginReference]
        RefAOAssemblyDeep? RefAOAssemblyDeep;

        public override Plugin? DepPlugin => RefAOAssemblyDeep;
        public override string PluginName => nameof(RefAOAssemblyDeep);

        protected override void Init()
        {
            base.Init();
        }

        protected override void Loaded()
        {
            base.Loaded();
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

        protected override void CallRef(string callerMsg)
        {
            base.CallRef(callerMsg);
        }
    }
}