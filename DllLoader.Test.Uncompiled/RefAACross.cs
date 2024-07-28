namespace Oxide.Plugins
{
    [Info("RefAACross", "Rube200", "1.0.0")]
    [Description("RefAACross is for testing")]
    public class RefAACross : RustPlugin
    {
        protected string SelfName => GetType().Name;

        protected void Init()
        {
            Puts($"{SelfName} - Init");
        }

        protected void Loaded()
        {
            Puts($"{SelfName} - Loaded");
        }

        protected void Unload()
        {
            Puts($"{SelfName} - Unload");
        }

        protected void Shutdown()
        {
            Puts($"{SelfName} - Shutdown");
        }

        protected void Hotloading()
        {
            Puts($"{SelfName} - Hotloading");
        }

        protected void CallRef(string callerMsg)
        {
            Puts($"CallRef from: '{callerMsg}'");
        }
    }
}