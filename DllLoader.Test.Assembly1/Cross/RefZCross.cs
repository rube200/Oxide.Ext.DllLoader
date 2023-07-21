namespace Oxide.Plugins
{
    [Info("RefZCross", "Rube200", "1.0.0")]
    [Description("RefZCross is for testing")]
    public class RefZCross : RustPlugin
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