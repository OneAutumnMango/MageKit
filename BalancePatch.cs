using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using System;

using System.Collections.Generic;
using System.Reflection.Emit;

[BepInPlugin("org.bepinex.plugins.balancepatch", "Balance Patch", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static ManualLogSource LoggerStatic;

    private void Awake()
    {
        LoggerStatic = Logger;
        LoggerStatic.LogInfo("Balance Plugin loaded!");

        var harmony = new Harmony("org.bepinex.plugins.balancepatch");
        harmony.PatchAll();
    }
}

// stop flashflood refreshing primary
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

// [HarmonyPatch(typeof(Chameleon), "Initialize")]
// public static class Patch_SpellInitialize
// {
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
// }

// act faster out of geyser (top of jump)
[HarmonyPatch(typeof(Geyser), "Initialize")]
public static class Patch_GeyserInitialize
{
    static void Postfix(Spell __instance)
    {
        if (__instance == null) return;
        __instance.windDown = 0.5f;
    }
}

// show hitboxes (dont use wormhole)
// [HarmonyPatch(typeof(GameUtility), "GetAllInSphere")]
// public static class Patch_GetAllInSphere_Debug
// {
//     static void Prefix(Vector3 center, float radius)
//     {
//         DrawDebugSphere(center, radius);
//     }

//     static void DrawDebugSphere(Vector3 pos, float radius)
//     {
//         var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//         go.transform.position = pos;
//         go.transform.localScale = Vector3.one * radius * 2f;

//         var col = go.GetComponent<Collider>();
//         if (col) col.enabled = false;

//         var mr = go.GetComponent<MeshRenderer>();
//         mr.material = new Material(Shader.Find("Sprites/Default"));
//         mr.material.color = new Color(1f, 0f, 0f, 0.25f);

//         GameObject.Destroy(go, 0.1f);
//     }
// }

// reduce flameleap offset, make it slighly closer to landing site
[HarmonyPatch(typeof(FlameLeapObject), "PrepareDestroy")]
public static class Patch_FlameLeapPrepareDestroy
{
    static void Prefix(FlameLeapObject __instance)
    {
        __instance.transform.position += __instance.transform.forward * -1f;

    }
}

[HarmonyPatch(typeof(ChainmailObject), "Update")]
public static class Patch_ChainmailObject_Update
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instr in instructions)
        {
            // Replace the 4.7f constant with 3f
            if (instr.opcode == OpCodes.Ldc_R4 && instr.operand is float f && f == 4.7f)
            {
                instr.operand = 3f;
            }
            yield return instr;
        }
    }
}


[HarmonyPatch(typeof(Chameleon), "Initialize")]
public static class Patch_ChameleonInitialize
{
    static void Prefix(Spell __instance)
    {
        if (__instance == null) return;
        __instance.cooldown = 9f;
    }
}