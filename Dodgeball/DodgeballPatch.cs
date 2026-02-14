using MageQuitModFramework.Spells;
using System.Collections.Generic;

namespace MageKit.Dodgeball
{
    public static class DodgeballPatch
    {
        public static void ApplyDodgeballModifiers(SpellModifierTable table)
        {
            foreach (var spell in GetDodgeballSpells())
            {
                table.TryAddToModifier(spell, "initialVelocity", -0.5f);
                table.TrySetBase(spell, "DAMAGE", 50f);
            }
        }

        private static readonly HashSet<SpellName> ExcludedSpells =
        [
            SpellName.FrogOfLife,
            SpellName.WaterCannon,
            SpellName.FlameLeash,
            SpellName.CyClone,
            SpellName.Sunder,
        ];

        private static readonly HashSet<SpellButton> ExcludedButtons =
        [
            SpellButton.Melee,
            SpellButton.Movement,
            SpellButton.Defensive,
        ];

        private static List<SpellName> GetDodgeballSpells()
        {
            var allSpells = System.Enum.GetValues(typeof(SpellName));
            var result = new List<SpellName>();
            foreach (SpellName spell in allSpells)
            {
                if (ExcludedSpells.Contains(spell))
                    continue;

                if (Globals.spell_manager.spell_table.TryGetValue(spell, out var spellObj) &&
                    ExcludedButtons?.Contains(spellObj.spellButton) == true)
                    continue;

                result.Add(spell);
            }
            return result;
        }
    }
}
