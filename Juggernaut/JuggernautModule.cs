using HarmonyLib;
using MageQuitModFramework.Modding;

namespace MageKit.Juggernaut
{
    public class JuggernautModule : BaseModule
    {
        public override string ModuleName => "Juggernaut";

        protected override void OnLoad(Harmony harmony)
        {
            PatchGroup(harmony, typeof(JuggernautPatches));
            JuggernautPatches.Initialize();
        }

        protected override void OnUnload(Harmony harmony)
        {
            harmony.UnpatchSelf();
        }
    }
}
