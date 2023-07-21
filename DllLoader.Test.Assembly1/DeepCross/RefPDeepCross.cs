namespace Oxide.Plugins
{
    [Info("RefPDeepCross", "Rube200", "1.0.0")]
    [Description("RefPDeepCross is for testing")]
    public class RefPDeepCross : RustPlugin
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