using HarmonyLib;
using SpellcastModFramework.Loading;

namespace BalancePatch.Debug
{
    public class DebugModule : BaseModModule
    {
        public override string ModuleName => "Debug";

        protected override void OnLoad(Harmony harmony)
        {
            harmony.PatchAll(typeof(DebugPatches));
        }

        protected override void OnUnload(Harmony harmony)
        {
            harmony.UnpatchSelf();
        }
    }
}
