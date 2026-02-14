using HarmonyLib;
using MageQuitModFramework.Data;
using MageQuitModFramework.Modding;
using MageQuitModFramework.Spells;
using System.Collections.Generic;

namespace MageKit.Dodgeball
{
    public class DodgeballModule : BaseModule
    {
        public override string ModuleName => "Dodgeball";

        protected override void OnLoad(Harmony harmony)
        {
            if (GameDataInitializer.IsLoaded)
                ApplyDodgeballPatch();
            else
                GameDataInitializer.OnGameDataLoaded += ApplyDodgeballPatch;
        }

        private void ApplyDodgeballPatch()
        {
            Plugin.Log.LogInfo("Applying Dodgeball patch");
            var table = SpellModificationSystem.RegisterTable("dodgeball");
            DodgeballPatch.ApplyDodgeballModifiers(table);
            SpellModificationSystem.Load("dodgeball");
        }

        protected override void OnUnload(Harmony harmony)
        {
            // Unload the dodgeball table
            SpellModificationSystem.ClearTable("dodgeball");
        }
    }
}
