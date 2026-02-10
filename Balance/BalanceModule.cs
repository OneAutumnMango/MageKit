using HarmonyLib;
using MageQuitModFramework.Loading;

namespace BalancePatch.Balance
{
    public class BalanceModule : BaseModModule
    {
        public override string ModuleName => "Balance";

        protected override void OnLoad(Harmony harmony)
        {
            PatchGroup(harmony, typeof(BalancePatches));
        }

        protected override void OnUnload(Harmony harmony)
        {
            harmony.UnpatchSelf();
        }
    }
}
