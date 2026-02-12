using HarmonyLib;
using MageQuitModFramework.Modding;

namespace MageKit.Balance
{
    public class BalanceModule : BaseModule
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
