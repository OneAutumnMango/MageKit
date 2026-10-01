using HarmonyLib;

namespace MageKit.ChameleonInvisibilityFix
{
    [HarmonyPatch(typeof(ChameleonObject), nameof(ChameleonObject.rpcCopy))]
    public static class ChameleonInvisibilityFixPatch
    {
        static void Postfix(ChameleonObject __instance, WizardController ___wizard, ref bool ___invisible)
        {
            if (!___invisible || ___wizard == null) return;

            // Only act if this client cast the spell (offline: always true)
            bool isCaster = !Globals.online
                || (__instance.photonView != null && __instance.photonView.isMine);
            if (!isCaster) return;

            ___invisible = false; // stops OnDestroy from decrementing again
            if (___wizard.invisibilityCount > 0)
                ___wizard.invisibilityCount--;

            if (___wizard.invisibilityCount == 0)
            {
                var status = ___wizard.GetComponent<WizardStatus>();
                status.HideStatusBar(false);
                status.HideWizard(false);
            }
        }
    }
}