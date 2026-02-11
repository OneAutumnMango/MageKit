using HarmonyLib;
using MageQuitModFramework.Modding;
using MageQuitModFramework.Data;

namespace BalancePatch.Boosted
{
    public class BoostedModule : BaseModule
    {
        public override string ModuleName => "Boosted";

        protected override void OnLoad(Harmony harmony)
        {
            BoostedPatch.PopulateManualModifierRejections();

            if (GameDataInitializer.IsLoaded)
                ApplyBoostedPatches(harmony);
            else
                GameDataInitializer.OnGameDataLoaded += () => ApplyBoostedPatches(harmony);
        }

        private void ApplyBoostedPatches(Harmony harmony)
        {
            Plugin.Log.LogInfo("Applying Boosted patches");
            BoostedPatch.PopulateSpellModifierTable();
            BoostedPatch.PatchAll(harmony);
        }

        protected override void OnUnload(Harmony harmony)
        {
            BoostedPatch.ResetSpellModifierTableMults();
            Plugin.CurrentUpgradeOptions.Clear();
            harmony.UnpatchSelf();
        }
    }
}
