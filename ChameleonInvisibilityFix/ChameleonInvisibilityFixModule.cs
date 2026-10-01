using HarmonyLib;
using MageQuitModFramework.Modding;

namespace MageKit.ChameleonInvisibilityFix
{
    public class ChameleonInvisibilityFixModule : BaseModule
    {
        public override string ModuleName => "ChameleonInvisibilityFix";

        protected override void OnLoad(Harmony harmony)
        {
            PatchGroup(harmony, typeof(ChameleonInvisibilityFixPatch));
        }

        protected override void OnUnload(Harmony harmony)
        {
            harmony.UnpatchSelf();
        }
    }
}