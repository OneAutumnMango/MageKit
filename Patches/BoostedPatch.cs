using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using BalancePatch;
using System;
using System.Linq;
using Patches.Util;


namespace Patches.Boosted
{
    public class AttributeModifier
    {
        public float Base { get; set; }
        public float Mult { get; set; }
        public float Value => Base * Mult;
        public AttributeModifier(float baseValue, float mult = 1f)
        {
            Base = baseValue;
            Mult = mult;
        }
        public static implicit operator float(AttributeModifier mod) => mod.Value;
    }
    public class SpellModifiers
    {
        public AttributeModifier DAMAGE { get; set; }
        public AttributeModifier RADIUS { get; set; }
        public AttributeModifier POWER { get; set; }
        public AttributeModifier Y_POWER { get; set; }
        public AttributeModifier cooldown { get; set; }
        public AttributeModifier windUp { get; set; }
        public AttributeModifier windDown { get; set; }
        public AttributeModifier initialVelocity { get; set; }
        public AttributeModifier spellRadius { get; set; }
    }

    public static class Upgrades
    {
        public readonly struct Tier
        {
            public float Rate { get; }
            public float Up { get; }
            public float Down { get; }

            public Tier(float rate, float up, float down)
            {
                Rate = rate;
                Up = up;
                Down = down;
            }
        }

        public static readonly Tier Common = new(1.00f, 0.25f, -0.10f);
        public static readonly Tier Rare = new(0.25f, 0.50f, -0.20f);
        public static readonly Tier Legendary = new(0.05f, 0.75f, -0.30f);

        // Optional: array for iteration
        public static readonly Tier[] AllTiers = { Common, Rare, Legendary };

        public static Tier GetRandom()
        {
            double roll = Plugin.Random.NextDouble();
            if (roll < Legendary.Rate)
                return Legendary;
            else if (roll < Rare.Rate)
                return Rare;
            else
                return Common;
        }
    }

    public static class BoostedPatch
    {
        private static readonly string[] ClassAttributeKeys = ["DAMAGE", "RADIUS", "POWER", "Y_POWER"];
        private static readonly string[] SpellTableKeys = ["cooldown", "windUp", "windDown", "initialVelocity", "spellRadius"];
        // private static Dictionary<SpellName, Dictionary<String, Dictionary<String, float>>> SpellModifierTable = [];
        public static Dictionary<SpellName, SpellModifiers> SpellModifierTable = [];

        public class UpgradeOption
        {
            public SpellName Spell { get; set; }
            public string Attribute { get; set; }
            public Upgrades.Tier Tier { get; set; }

            public string GetDisplayText()
            {
                string attrDisplay = Attribute switch
                {
                    "DAMAGE" => "Damage",
                    "RADIUS" => "Impact Radius",
                    "POWER" => "Knockback",
                    "Y_POWER" => "Knockup",
                    "cooldown" => "Cooldown",
                    "windUp" => "Wind Up",
                    "windDown" => "Wind Down",
                    "initialVelocity" => "Initial Velocity",
                    "spellRadius" => "Projectile Radius",
                    _ => Attribute
                };
                return $"{Spell}: {attrDisplay}";
            }
        }

        private static void TryUpdateModifier(SpellName name, string attribute, float mod)
        {
            if (!SpellModifierTable.TryGetValue(name, out var mods)) return;

            var prop = typeof(SpellModifiers).GetProperty(attribute);

            if (prop?.GetValue(mods) is AttributeModifier attrMod)
                attrMod.Mult += mod;
        }

        public static List<SpellName> GetPlayerSpells(Player player) =>
            player?.cooldowns?.Keys.ToList() ?? [];

        private static void ApplyTier(SpellName name, string attribute, Upgrades.Tier tier, bool up)
        {
            TryUpdateModifier(name, attribute, up ? tier.Up : tier.Down);
        }

        public static void ApplyUpgrade(UpgradeOption option, bool isPositive)
        {
            ApplyTier(option.Spell, option.Attribute, option.Tier, isPositive);
            float change = isPositive ? option.Tier.Up : option.Tier.Down;
            Plugin.Log.LogInfo($"[BoostedPatch] Applied {(isPositive ? "+" : "")}{change * 100:F0}% to {option.GetDisplayText()}");
        }

        public static List<UpgradeOption> GenerateUpgradeOptions(Player player, int count = 3)
        {
            var options = new List<UpgradeOption>();
            var spells = GetPlayerSpells(player);
            if (spells.Count == 0) return options;
            var allAttributes = ClassAttributeKeys.Concat(SpellTableKeys).ToArray();
            var possibleUpgrades = new List<(SpellName spell, string attr)>();
            foreach (var spell in spells)
            {
                foreach (var attr in allAttributes)
                {
                    possibleUpgrades.Add((spell, attr));
                }
            }
            var rng = Plugin.Random;
            for (int i = 0; i < count && possibleUpgrades.Count > 0; i++)
            {
                int index = rng.Next(possibleUpgrades.Count);
                var (spell, attr) = possibleUpgrades[index];
                possibleUpgrades.RemoveAt(index);
                options.Add(new UpgradeOption
                {
                    Spell = spell,
                    Attribute = attr,
                    Tier = Upgrades.GetRandom()
                });
            }
            return options;
        }

        public static void PopulateSpellModifierTable()
        {
            foreach (SpellName name in Util.Util.DefaultSpellTable.Keys)
            {
                var spell = Util.Util.DefaultSpellTable[name];
                var classAttrs = Util.Util.DefaultClassAttributes[name];

                SpellModifierTable[name] = new SpellModifiers
                {
                    DAMAGE = new AttributeModifier(classAttrs["DAMAGE"]),
                    RADIUS = new AttributeModifier(classAttrs["RADIUS"]),
                    POWER = new AttributeModifier(classAttrs["POWER"]),
                    Y_POWER = new AttributeModifier(classAttrs["Y_POWER"]),
                    cooldown = new AttributeModifier(spell.cooldown),
                    windUp = new AttributeModifier(spell.windUp),
                    windDown = new AttributeModifier(spell.windDown),
                    initialVelocity = new AttributeModifier(spell.initialVelocity),
                    spellRadius = new AttributeModifier(spell.spellRadius)
                };
            }

            // string inline = "{" + string.Join(", ",
            // SpellModifierTable.Select(spellKvp =>
            //     $"\"{spellKvp.Key}\": {{" +
            //     string.Join(", ", spellKvp.Value.Select(classKvp =>
            //         $"\"{classKvp.Key}\": {{" +
            //         string.Join(", ", classKvp.Value.Select(statKvp => $"\"{statKvp.Key}\": {statKvp.Value}")) +
            //         "}"
            //     )) +
            //     "}"
            // )) + "}";

            // Plugin.Log.LogInfo(inline);
        }

        public static void ApplyModifiersToSpellTable(SpellManager spellManager)
        {
            foreach (var kvp in SpellModifierTable)
            {
                SpellName name = kvp.Key;
                SpellModifiers mods = kvp.Value;

                if (spellManager.spell_table.TryGetValue(name, out Spell spell))
                {
                    spell.cooldown = mods.cooldown;
                    spell.windUp = mods.windUp;
                    spell.windDown = mods.windDown;
                    spell.initialVelocity = mods.initialVelocity;
                    spell.spellRadius = mods.spellRadius;
                }
            }
        }

        public static void ApplyModifiersToPlayer(Player player)
        {
            foreach (var kvp in player.cooldowns)
            {
                SpellName name = kvp.Key;
                Cooldown playerCooldown = kvp.Value;

                if (SpellModifierTable.TryGetValue(name, out SpellModifiers spellMods))
                {
                    playerCooldown.cooldown = spellMods.cooldown;
                }
            }
        }

        public static void PatchAllSpellObjects(Harmony harmony)
        {
            foreach (SpellName name in Enum.GetValues(typeof(SpellName)))
            {
                string fullTypeName = Util.Util.GetSpellObjectTypeName(name);
                Type spellType = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType(fullTypeName))
                    .FirstOrDefault(t => t != null);

                if (spellType == null) continue;

                MethodInfo initMethod = spellType.GetMethod("Init", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (initMethod == null) continue;

                MethodInfo prefixMethod = typeof(BoostedPatch).GetMethod(
                    nameof(Prefix_SpellObjectInit),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

                harmony.Patch(initMethod, prefix: new HarmonyMethod(prefixMethod));
            }
        }

        private static void Prefix_SpellObjectInit(object __instance)
        {
            Type t = __instance.GetType();
            var matchedSpell = Enum.GetValues(typeof(SpellName))
                .Cast<SpellName>()
                .FirstOrDefault(name => Util.Util.GetSpellObjectTypeName(name) == t.Name);

            if (!SpellModifierTable.TryGetValue(matchedSpell, out var mods)) return;

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            t.GetField("DAMAGE", flags)?.SetValue(__instance, (float)mods.DAMAGE);
            t.GetField("RADIUS", flags)?.SetValue(__instance, (float)mods.RADIUS);
            t.GetField("POWER", flags)?.SetValue(__instance, (float)mods.POWER);
            t.GetField("Y_POWER", flags)?.SetValue(__instance, (float)mods.Y_POWER);
        }
    }

    // [HarmonyPatch(typeof(Player), "RegisterCooldown")]
    // public static class Patch_Player_RegisterCooldown_SetDamage
    // {
    //     static void Prefix(ref float cooldown)
    //     {
    //         cooldown = 100f;
    //     }
    // }
    [HarmonyPatch(typeof(SpellManager), "Awake")]
    public static class Patch_SpellManager_GetManagerInstance
    {
        public static SpellManager mgr;

        static void Postfix(SpellManager __instance)
        {
            mgr = __instance ?? Globals.spell_manager;
        }
    }



    // ROUND WATCHER
    [HarmonyPatch(typeof(NetworkManager), "CombineRoundScores")]
    public static class NetworkManager_CombineRoundScores_RoundLogger
    {
        private static void Prefix()
        {
            Plugin.Log.LogInfo(
                $"[RoundWatcher] CombineRoundScores → round {PlayerManager.round}"
            );
        }

        private static void Postfix()
        {
            if (Patch_SpellManager_GetManagerInstance.mgr != null)
            {
                BoostedPatch.ApplyModifiersToSpellTable(Patch_SpellManager_GetManagerInstance.mgr);
                Plugin.Log.LogInfo("[RoundWatcher] Applied spell modifiers to spell table");
            }

            if (PlayerManager.round > 0)
            {
                Player player = PlayerManager.players.Values.FirstOrDefault(p => p.localPlayerNumber == 0);
                if (player == null) return;

                BoostedPatch.ApplyModifiersToPlayer(player);  // update player cooldowns

                var options = BoostedPatch.GenerateUpgradeOptions(player, 5);
                Plugin.CurrentUpgradeOptions.Clear();
                Plugin.CurrentUpgradeOptions.AddRange(options);  // thread safe

                Plugin.Log.LogInfo($"[RoundWatcher] Generated {options.Count} upgrade options:");
                foreach (var opt in options)
                {
                    Plugin.Log.LogInfo($"  {opt.GetDisplayText()}: +{opt.Tier.Up * 100:F0}% / {opt.Tier.Down * 100:F0}%");
                }
            }
        }
    }


    [HarmonyPatch(typeof(PlayerManager), "AddPlayer")]
    public static class Patch_PlayerManager_AddPlayer
    {
        static void Prefix(int number, InputType inputType)
        {
            Plugin.Log.LogInfo($"[PlayerManager] Adding Player: {number}, InputType: {inputType}");
            Plugin.Log.LogInfo($"[PlayerManager] Current Players: {string.Join(", ", PlayerManager.players.Keys)}");
        }
    }
}
