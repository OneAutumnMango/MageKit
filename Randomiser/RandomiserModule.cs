using HarmonyLib;
using MageQuitModFramework.Modding;

namespace BalancePatch.Randomiser
{
    public class RandomiserModule : BaseModule
    {
        public override string ModuleName => "Randomiser";

        protected override void OnLoad(Harmony harmony)
        {
            Plugin.InitialiseRandomiserRng();
            RandomiserPatch.PrecomputeSpellAttributes();
            RandomiserPatch.PatchAll(harmony);
        }

        protected override void OnUnload(Harmony harmony)
        {
            harmony.UnpatchSelf();
        }
    }
}
