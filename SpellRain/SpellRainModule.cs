using MageQuitModFramework.Modding;

namespace MageKit.SpellRain
{
    public class SpellRainModule : BaseModule
    {
        public override string ModuleName => "SpellRain";

        protected override void OnLoad(HarmonyLib.Harmony harmony)
        {
            PatchGroup(harmony, typeof(SpellRainPatches));
        }

        protected override void OnUnload(HarmonyLib.Harmony harmony)
        {
            harmony.UnpatchSelf();
        }
    }
}
