using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace MageKit.Debug
{
    [HarmonyPatch]
    public static class DebugPatches
    {
        private static void LogInstantiatedObject(Object obj)
        {
            if (obj is GameObject go)
            {
                Plugin.Log.LogInfo($"[Instantiate] GameObject: '{go.name}' (active: {go.activeSelf}, layer: {go.layer}, children: {go.transform.childCount})");
                foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                {
                    Plugin.Log.LogInfo($"  Renderer: '{renderer.name}' enabled={renderer.enabled} material={renderer.material?.name}");
                }
            }
            else
            {
                Plugin.Log.LogInfo($"[Instantiate] Object: '{obj?.GetType().Name}'");
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), [typeof(Object)])]
        public static class Patch_Object_Instantiate_Object
        {
            static void Postfix(Object __result, Object __0)
            {
                Plugin.Log.LogInfo($"[Instantiate] Input: '{__0?.GetType().Name}' name='{(__0 is GameObject go ? go.name : __0?.ToString())}'");
                LogInstantiatedObject(__result);
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), [typeof(Object), typeof(Vector3), typeof(Quaternion)])]
        public static class Patch_Object_Instantiate_Object_Position
        {
            static void Postfix(Object __result, Object __0, Vector3 __1, Quaternion __2)
            {
                Plugin.Log.LogInfo($"[Instantiate] Input: '{__0?.GetType().Name}' name='{(__0 is GameObject go ? go.name : __0?.ToString())}' pos={__1} rot={__2.eulerAngles}");
                LogInstantiatedObject(__result);
            }
        }
    }

    // Show damage hitboxes
    [HarmonyPatch(typeof(GameUtility), "GetAllInSphere")]
    public static class Patch_GetAllInSphere_Debug
    {
        static void Prefix(Vector3 center, float radius)
        {
            DrawDebugSphere(center, radius);
        }

        static void DrawDebugSphere(Vector3 pos, float radius)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * radius * 2f;

            var col = go.GetComponent<Collider>();
            if (col) col.enabled = false;

            var mr = go.GetComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Sprites/Default"));
            mr.material.color = new Color(1f, 0f, 0f, 0.25f);

            Object.Destroy(go, 0.1f);
        }
    }

    // Log damage
    [HarmonyPatch(typeof(WizardStatus), "rpcApplyDamage")]
    public static class Patch_WizardStatus_rpcApplyDamage
    {
        static void Prefix(WizardStatus __instance, float damage, int owner, int source)
        {
            var idField = typeof(WizardStatus).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic);
            var idValue = idField?.GetValue(__instance);

            int wizardOwner = -1;
            if (idValue != null)
            {
                var ownerField = idValue.GetType().GetField("owner", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (ownerField != null)
                    wizardOwner = (int)ownerField.GetValue(idValue);
            }

            Plugin.Log.LogInfo($"[Damage Log] Wizard {wizardOwner} is about to take {damage} damage from {owner}, source {source}");
        }

        static void Postfix(WizardStatus __instance, float damage, int owner, int source)
        {
            Plugin.Log.LogInfo($"[Damage Log] Wizard's remaining health: {__instance.health}, damage taken: {damage}");
        }
    }

    // Log healing
    [HarmonyPatch(typeof(WizardStatus), "rpcApplyHealing")]
    public static class Patch_WizardStatus_rpcApplyHealing
    {
        static void Prefix(WizardStatus __instance, float healing, int owner)
        {
            var idField = typeof(WizardStatus).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic);
            var idValue = idField?.GetValue(__instance);

            int wizardOwner = -1;
            if (idValue != null)
            {
                var ownerField = idValue.GetType().GetField("owner", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (ownerField != null)
                    wizardOwner = (int)ownerField.GetValue(idValue);
            }

            Plugin.Log.LogInfo($"[Healing Log] Wizard {wizardOwner} is about to heal {healing} health from {owner}");
        }

        static void Postfix(WizardStatus __instance, float healing, int owner)
        {
            Plugin.Log.LogInfo($"[Healing Log] Wizard's current health: {__instance.health}, healing applied: {healing}");
        }
    }
}
