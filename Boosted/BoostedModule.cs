using HarmonyLib;
using MageQuitModFramework.Loading;

namespace BalancePatch.Boosted
{
    public class BoostedModule : BaseModModule
    {
        public override string ModuleName => "Boosted";

        protected override void OnLoad(Harmony harmony)
        {
            BoostedPatch.PopulateManualModifierRejections();

            // GameDataInitializer is already patched by the framework
            // Just check if it's loaded, and if not, wait for the event
            if (GameDataInitializer.IsLoaded)
            {
                Plugin.Log.LogInfo("GameDataInitializer already loaded, applying patches now");
                BoostedPatch.PopulateSpellModifierTable();
                BoostedPatch.PatchAll(harmony);
            }
            else
            {
                Plugin.Log.LogWarning("GameDataInitializer not loaded yet, waiting for game data");
                GameDataInitializer.OnGameDataLoaded += () =>
                {
                    Plugin.Log.LogInfo("Game data loaded, applying Boosted patches");
                    BoostedPatch.PopulateSpellModifierTable();
                    BoostedPatch.PatchAll(harmony);
                };
            }
        }

        protected override void OnUnload(Harmony harmony)
        {
            BoostedPatch.ResetSpellModifierTableMults();
            Plugin.CurrentUpgradeOptions.Clear();
            harmony.UnpatchSelf();
        }
    }
}
