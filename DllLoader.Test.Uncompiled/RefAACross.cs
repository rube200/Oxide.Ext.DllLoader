namespace Oxide.Plugins
{
    [Info("RefAACross", "Rube200", "1.0.0")]
    [Description("RefAACross is for testing")]
    public class RefAACross : RustPlugin
    {
        void Init()
        {
            var selfName = GetType().Name;
            Puts($"I am alive {selfName}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}