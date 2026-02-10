using HarmonyLib;
using MageQuitModFramework.Loading;

namespace BalancePatch.Boosted
{
    public class BoostedModule : BaseModModule
    {
        public override string ModuleName => "Boosted";

        protected override void OnLoad(Harmony harmony)
        {
            if (!GameDataInitializer.IsLoaded)
            {
                Plugin.Log.LogWarning("GameDataInitializer not loaded, patching SpellManager.Awake");
                harmony.PatchAll(typeof(GameDataInitializer));
            }

            BoostedPatch.PopulateManualModifierRejections();
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
