using System.Collections.Generic;
using UnityEngine;

namespace MageKit.SpellRain
{
    public class OneTimeSpell
    {
        public SpellName spellName;
        public SpellButton button;
        public bool used = false;
    }

    public static class SpellRainSpawner
    {
        public static Dictionary<int, Dictionary<SpellButton, OneTimeSpell>> oneTimeSpells = [];
        private static GameObject crystalPrefab;
        private static bool prefabInitialized = false;

        private static GameObject GetCrystalPrefab()
        {
            if (!prefabInitialized)
            {
                // Try to find an existing CrystalObject in the scene
                CrystalObject existingCrystal = Object.FindObjectOfType<CrystalObject>();
                if (existingCrystal != null)
                {
                    crystalPrefab = existingCrystal.gameObject;
                    Plugin.Log.LogInfo("Cached crystal prefab from existing CrystalObject in scene.");
                }
                else
                {
                    // Use GameUtility.Instantiate to spawn a CrystalObject from the game's prefab system
                    GameObject go = GameUtility.Instantiate("Units/Crystal", Vector3.zero, Quaternion.identity, 0);
                    var crystal = go.GetComponent<CrystalObject>();
                    if (crystal != null)
                    {
                        // Use dummy values for Init, just to ensure it's initialized
                        crystal.Init(null, 0, SpellName.Brrage, CrystalObject.CrystalState.Inert, null, false);
                        // Clone the prefab so the cached one is never destroyed
                        GameObject prefabCopy = Object.Instantiate(go);
                        prefabCopy.SetActive(false);
                        Object.DontDestroyOnLoad(prefabCopy);
                        crystalPrefab = prefabCopy;
                        Object.Destroy(go); // destroy the original temporary instance
                        Plugin.Log.LogInfo("Spawned, cloned, and cached a CrystalObject prefab using GameUtility.Instantiate.");
                    }
                    else
                    {
                        Plugin.Log.LogError("Failed to get CrystalObject from instantiated prefab!");
                    }
                }
                prefabInitialized = true;
            }
            return crystalPrefab;
        }

        public static GameObject SpawnPickupCrystal(Vector3 position, SpellName spell, SpellButton targetSlot = SpellButton.Secondary)
        {
            GameObject prefab = GetCrystalPrefab();
            if (prefab == null)
            {
                Plugin.Log.LogError("Could not find crystal prefab to spawn!");
                return null;
            }

            GameObject newCrystal = Object.Instantiate(prefab, position, Quaternion.identity);

            newCrystal.SetActive(true);

            // Disable original behavior
            CrystalObject crystalObj = newCrystal.GetComponent<CrystalObject>();
            if (crystalObj != null)
                crystalObj.enabled = false;

            SpellRainHelper pickup = newCrystal.AddComponent<SpellRainHelper>();
            pickup.spellToGive = spell;
            pickup.targetSlot  = targetSlot;

            SetupCrystalVisuals(newCrystal, spell);

            Plugin.Log.LogInfo($"Spawned pickup crystal at {position} with spell: {spell}");
            return newCrystal;
        }

        public static GameObject SpawnRandomPickupCrystal(Vector3 position, SpellButton targetSlot = SpellButton.Secondary)
        {
            var allSpells = System.Enum.GetValues(typeof(SpellName));
            SpellName randomSpell = (SpellName)allSpells.GetValue(Random.Range(0, allSpells.Length));
            return SpawnPickupCrystal(position, randomSpell, targetSlot);
        }

        public static GameObject SpawnPickupNearPlayer(int playerNumber, SpellName spell, float distance = 3f, SpellButton targetSlot = SpellButton.Secondary)
        {
            if (!PlayerManager.players.ContainsKey(playerNumber))
            {
                Plugin.Log.LogWarning($"Player {playerNumber} not found!");
                return null;
            }

            GameObject wizard = PlayerManager.players[playerNumber].wizard;
            if (wizard == null)
            {
                Plugin.Log.LogWarning($"Player {playerNumber} has no wizard!");
                return null;
            }

            Vector3 spawnPos = wizard.transform.position + wizard.transform.forward * distance;
            return SpawnPickupCrystal(spawnPos, spell, targetSlot);
        }

        public static GameObject SpawnRandomPickupNearPlayer(int playerNumber, float distance = 3f, SpellButton targetSlot = SpellButton.Secondary)
        {
            var allSpells = System.Enum.GetValues(typeof(SpellName));
            SpellName randomSpell = (SpellName)allSpells.GetValue(Random.Range(0, allSpells.Length));
            return SpawnPickupNearPlayer(playerNumber, randomSpell, distance, targetSlot);
        }

        public static List<GameObject> SpawnPickupCircle(Vector3 center, int count, float radius, SpellButton targetSlot = SpellButton.Secondary)
        {
            List<GameObject> spawned = [];

            for (int i = 0; i < count; i++)
            {
                float angle = (360f / count) * i;
                float rad = angle * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(rad) * radius, 0f, Mathf.Sin(rad) * radius);
                Vector3 spawnPos = center + offset;

                GameObject crystal = SpawnRandomPickupCrystal(spawnPos, targetSlot);
                if (crystal != null)
                {
                    spawned.Add(crystal);
                }
            }

            return spawned;
        }

        private static void SetupCrystalVisuals(GameObject crystal, SpellName spell)
        {
            try
            {
                if (Globals.spell_manager.spell_table.ContainsKey(spell))
                {
                    Spell spellData = Globals.spell_manager.spell_table[spell];
                    Sprite icon = spellData.icon;
                    var renderers = crystal.GetComponentsInChildren<Renderer>(true);
                    foreach (var renderer in renderers)
                    {
                        if (renderer.name.Contains("Preserved") || renderer.name.Contains("Icon"))
                        {
                            Material mat = renderer.material;
                            if (icon != null)
                            {
                                mat.mainTexture = icon.texture;
                                mat.SetTexture(MaterialHashes.EmissionMap, icon.texture);
                                mat.SetTexture(MaterialHashes.OcclusionMap, icon.texture);
                                mat.SetTexture(MaterialHashes.HeightMap, icon.texture);
                                mat.SetColor(MaterialHashes.EmissionColor, Globals.iconEmissionColors[(int)spellData.element]);
                            }
                        }
                    }
                    // // Optionally instantiate preserveTokenizePrefab if available
                    // if (crystal.GetComponent<CrystalObject>() is CrystalObject co && co.preserveTokenizePrefab != null)
                    // {
                    //     Object.Instantiate(co.preserveTokenizePrefab, crystal.transform.position, Globals.sideways);
                    // }
                    // // Optionally disable collisions
                    // if (crystal.GetComponent<Rigidbody>() is Rigidbody rb)
                    //     rb.detectCollisions = false;
                }
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogWarning($"Could not set up crystal visuals: {e.Message}");
            }
        }
    }
}