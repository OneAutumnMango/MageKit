using HarmonyLib;
using MageQuitModFramework.Loading;

namespace BalancePatch.Randomiser
{
    public class RandomiserModule : BaseModModule
    {
        public override string ModuleName => "Randomiser";

        protected override void OnLoad(Harmony harmony)
        {
            RandomiserPatch.PrecomputeSpellAttributes();
            RandomiserPatch.PatchAll(harmony);
        }

        protected override void OnUnload(Harmony harmony)
        {
            harmony.UnpatchSelf();
        }
    }
}
