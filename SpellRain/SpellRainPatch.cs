using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace MageKit.SpellRain
{
    [HarmonyPatch]
    public static class SpellRainPatches
    {
        [HarmonyPatch(typeof(SpellHandler), nameof(SpellHandler.StartSpell))]
        [HarmonyPostfix]
        static void MarkOneTimeSpellAsUsed(SpellHandler __instance, SpellButton button)
        {
            Identity id = __instance.GetComponent<Identity>();
            if (id == null) return;

            int owner = id.owner;

            if (!SpellRainSpawner.oneTimeSpells.ContainsKey(owner)) return;
            if (!SpellRainSpawner.oneTimeSpells[owner].ContainsKey(button)) return;

            OneTimeSpell oneTime = SpellRainSpawner.oneTimeSpells[owner][button];

            if (!oneTime.used)
            {
                oneTime.used = true;
                Plugin.Log.LogInfo($"Player {owner} used one-time spell: {oneTime.spellName}");
            }
        }

        [HarmonyPatch(typeof(SpellHandler), "Update")]
        [HarmonyPostfix]
        static void RemoveUsedOneTimeSpells(SpellHandler __instance)
        {
            Identity id = __instance.GetComponent<Identity>();
            if (id == null) return;

            int owner = id.owner;

            if (!SpellRainSpawner.oneTimeSpells.ContainsKey(owner)) return;

            var spellStateField = AccessTools.Field(typeof(SpellHandler), "spellState");
            var spellStateEnum = spellStateField.GetValue(__instance);

            if (spellStateEnum.ToString() == "Complete")
            {
                List<SpellButton> toRemove = [];

                foreach (var kvp in SpellRainSpawner.oneTimeSpells[owner])
                {
                    if (kvp.Value.used)
                    {
                        toRemove.Add(kvp.Key);
                    }
                }

                foreach (SpellButton button in toRemove)
                {
                    OneTimeSpell spell = SpellRainSpawner.oneTimeSpells[owner][button];

                    if (PlayerManager.players[owner].spell_library.ContainsKey(button))
                    {
                        PlayerManager.players[owner].spell_library.Remove(button);
                    }

                    if (PlayerManager.players[owner].cooldowns.ContainsKey(spell.spellName))
                    {
                        PlayerManager.players[owner].cooldowns.Remove(spell.spellName);
                    }

                    SpellRainSpawner.oneTimeSpells[owner].Remove(button);

                    Plugin.Log.LogInfo($"Removed one-time spell {spell.spellName} from player {owner} slot {button}");
                }
            }
        }
    }
}

