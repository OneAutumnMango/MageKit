using System.Collections.Generic;
using HarmonyLib;
using MageQuitModFramework.Utilities;
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

            if (!SpellRainSpawner.oneTimeSpells.TryGetValue(owner, out var playerSpells)) return;
            if (!playerSpells.TryGetValue(button, out var oneTime)) return;

            // if (Globals.spell_manager.lastSpells.ContainsKey(owner))
            // {
            //     oneTime.spell = Globals.spell_manager.lastSpells[owner];
            // }

            if (oneTime.remainingCasts > 0)
            {
                oneTime.remainingCasts--;
                oneTime.used = true; // Keep for backwards compatibility
                Plugin.Log.LogInfo($"Player {owner} used one-time spell: {oneTime.spellName} (Remaining casts: {oneTime.remainingCasts})");
            }
        }

        [HarmonyPatch(typeof(SpellHandler), "Update")]
        [HarmonyPostfix]
        static void RemoveUsedOneTimeSpells(SpellHandler __instance)
        {
            Identity id = __instance.GetComponent<Identity>();
            if (id == null) return;

            int owner = id.owner;

            if (!SpellRainSpawner.oneTimeSpells.TryGetValue(owner, out var playerSpells)) return;

            var spellStateEnum = GameModificationHelpers.GetPrivateField<int>(__instance, "spellState");

            if (spellStateEnum == 2)  // complete
            {
                List<SpellButton> toRemove = [];

                foreach (var kvp in playerSpells)
                {
                    // if (kvp.Value.spell.deathTimer > Time.time)  DOESNYT WORK NEED SPELLOBJECT INSTANCE
                    // {
                    //     continue; // Skip removing spell if it has a death timer that hasn't expired
                    // }
                    // Only remove spell when all casts have been used
                    if (kvp.Value.remainingCasts <= 0)
                    {
                        toRemove.Add(kvp.Key);
                    }
                }

                foreach (SpellButton spellButton in toRemove)
                {
                    if (!playerSpells.TryGetValue(spellButton, out var spell))
                        continue;

                    if (PlayerManager.players.TryGetValue(owner, out var player))
                    {
                        if (player.cooldowns.ContainsKey(spell.spellName))
                        {
                            if (player.cooldowns[spell.spellName] is Cooldown cooldown
                                && cooldown.IsCooldownAvailable() != 1)
                                continue; // skip if additional not on cooldown

                            player.cooldowns.Remove(spell.spellName);
                        }

                        if (player.spell_library.ContainsKey(spellButton))
                        {
                            player.spell_library.Remove(spellButton);
                        }
                    }

                    playerSpells.Remove(spellButton);

                    SpellRainHelper.HideHudButton(spellButton);

                    Plugin.Log.LogInfo($"Removed one-time spell {spell.spellName} from player {owner} slot {spellButton}");
                }
            }
        }
    }
}

