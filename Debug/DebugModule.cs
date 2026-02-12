using HarmonyLib;
using MageQuitModFramework.Modding;

namespace MageKit.Debug
{
    public class DebugModule : BaseModule
    {
        public override string ModuleName => "Debug";

        protected override void OnLoad(Harmony harmony)
        {
            PatchGroup(harmony, typeof(DebugPatches));
        }

        protected override void OnUnload(Harmony harmony)
        {
            harmony.UnpatchSelf();
        }
    }
}
