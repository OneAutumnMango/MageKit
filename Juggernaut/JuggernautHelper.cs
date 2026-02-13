using UnityEngine;
using DG.Tweening;
using MageQuitModFramework.Spells;
using System;
using System.Collections.Generic;

namespace MageKit.Juggernaut
{
    public static class JuggernautHelper
    {
        private static bool JuggernautModsApplied = false;
        private static SpellModifierTable _juggernautTable;
        private static string _previouslyLoadedTableKey;

        public static void ApplyJuggernautSpellModifications(bool isJuggernaut)
        {
            if (!isJuggernaut)
                return;

            if (JuggernautModsApplied)
            {
                Plugin.Log.LogWarning("Juggernaut mods already applied this round");
                return;
            }

            _previouslyLoadedTableKey = SpellModificationSystem.LoadedTableKey;
            _juggernautTable = SpellModificationSystem.GetTable(_previouslyLoadedTableKey).Copy();

            Dictionary<string, float> modifiers = new()
            {
                [ "DAMAGE"          ] = 2.0f,
                [ "RADIUS"          ] = 2.5f,
                [ "POWER"           ] = 2.0f,
                [ "Y_POWER"         ] = 1.5f,
                [ "HEAL"            ] = 1.25f,
                [ "initialVelocity" ] = 1.5f,
                [ "cooldown"        ] = 1.0f,
                [ "windUp"          ] = 1.25f,
                [ "windDown"        ] = 1.5f
            };

            foreach (SpellName spellName in Enum.GetValues(typeof(SpellName)))
            {
                foreach (var attribute in modifiers.Keys)
                {
                    _juggernautTable.TryMultiplyModifier(spellName, attribute, modifiers[attribute]);
                }
            }

            SpellModificationSystem.RegisterTable("juggernaut", _juggernautTable);
            JuggernautModsApplied = true;
        }

        public static void RevertJuggernautSpellModifications()
        {
            if (!JuggernautModsApplied)
                return;

            SpellModificationSystem.ClearTable("juggernaut");
            _juggernautTable = null;

            SpellModificationSystem.Load(_previouslyLoadedTableKey);

            JuggernautModsApplied = false;
        }

        public static void ApplyJuggernautVisuals(WizardController wc)
        {
            wc.transform.DOKill(); // prevent stacking if triggered again

            wc.transform  // make big
                .DOScale(1.3f, 0.7f)
                .SetDelay(3f)
                .SetEase(Ease.OutBack);

            // Make wizard bright/glowing
            Color[] customColors = [new Color(1f, 0.9f, 0.2f), Color.white];
            GameUtility.SetWizardColor(customColors, wc.gameObject, true); // true = extraBloom
            ApplyJuggernautGlow(wc.gameObject);
        }

        private static void ApplyJuggernautGlow(GameObject wizardObject)
        {
            foreach (var renderer in wizardObject.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.materials;
                for (var i = 0; i < materials.Length; i++)
                {
                    var material = materials[i];
                    if (material == null)
                    {
                        continue;
                    }

                    if (material.HasProperty(MaterialHashes.EmissionColor))
                    {
                        material.EnableKeyword("_EMISSION");
                        var emission = material.GetColor(MaterialHashes.EmissionColor);
                        material.SetColor(MaterialHashes.EmissionColor, emission * 2.25f);
                    }
                }
            }

            foreach (var light in wizardObject.GetComponentsInChildren<Light>(true))
            {
                light.intensity = Mathf.Max(light.intensity, 2.5f);
                light.range = Mathf.Max(light.range, 8f);
            }
        }
    }
}
