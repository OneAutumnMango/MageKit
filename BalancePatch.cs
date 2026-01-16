using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using System;
using System.Reflection;
using System.Text;


[BepInPlugin("org.bepinex.plugins.balanceplugin", "Balance Plugin", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    // Static logger accessible from Harmony patches
    public static ManualLogSource LoggerStatic;

    private void Awake()
    {
        // Assign instance logger to static field
        LoggerStatic = Logger;

        LoggerStatic.LogInfo("Balance Plugin loaded!");

        // Create Harmony instance and patch all methods
        var harmony = new Harmony("org.bepinex.plugins.refreshprimarylogger");
        harmony.PatchAll();
    }
}

[HarmonyPatch(typeof(SpellHandler), "RefreshPrimary")]
public static class Patch_RefreshPrimary
{
    static bool Prefix(SpellHandler __instance)
    {
        // Only skip if the caller is Flash Flood
        if (Environment.StackTrace.Contains("FlashFloodObject.localSpellObjectStart"))
        {
            return false;
        }
        return true;
    }
}

//[HarmonyPatch(typeof(Geyser), "Initialize")]
//public static class Patch_SpellInitialize
//{
//    static void Postfix(Spell __instance, Identity identity, Vector3 position, Quaternion rotation, float curve, int spellIndex = -1, bool selfCast = false, SpellName spellNameForCooldown = SpellName.Fireball)
//    {
//        try
//        {
//            if (__instance == null) return;

//            var sb = new StringBuilder();
//            sb.AppendLine($"[SpellLogger] Initialized Spell: {__instance.spellName}");
//            sb.AppendLine($"  Description: {__instance.description}");
//            sb.AppendLine($"  AnimationName: {__instance.animationName}");
//            sb.AppendLine($"  Cooldown: {__instance.cooldown}");
//            sb.AppendLine($"  WindUp: {__instance.windUp}");
//            sb.AppendLine($"  WindDown: {__instance.windDown}");
//            sb.AppendLine($"  MinRange: {__instance.minRange}");
//            sb.AppendLine($"  MaxRange: {__instance.maxRange}");
//            sb.AppendLine($"  CurveMultiplier: {__instance.curveMultiplier}");
//            sb.AppendLine($"  InitialVelocity: {__instance.initialVelocity}");
//            sb.AppendLine($"  IgnorePath: {__instance.ignorePath}");
//            sb.AppendLine($"  Uses: {__instance.uses}");
//            sb.AppendLine($"  AdditionalCasts Count: {__instance.additionalCasts?.Length ?? 0}");
//            sb.AppendLine("  StackTrace:");

//            sb.AppendLine("  StackTrace:");
//            sb.AppendLine(Environment.StackTrace); // append entire stack trace at once

//            Debug.Log(sb.ToString());
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError($"[SpellLogger] Exception logging spell: {ex}");
//        }
//    }
//}

[HarmonyPatch(typeof(Geyser), "Initialize")]
public static class Patch_GeyserInitialize
{
    static void Postfix(Spell __instance)
    {
        if (__instance == null) return;

        float oldWindDown = __instance.windDown;
        __instance.windDown = 0.5f;
    }
}
