using HarmonyLib;
using MageQuitModFramework.Modding;

namespace BalancePatch.Debug
{
    public class DebugModule : BaseModModule
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
