namespace Oxide.Plugins
{
    [Info("RefODeepCross", "Rube200", "1.0.0")]
    [Description("RefODeepCross is for testing")]
    public class RefODeepCross : RustPlugin
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