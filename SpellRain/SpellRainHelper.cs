using System.Collections.Generic;
using UnityEngine;

namespace MageKit.SpellRain
{
    public class SpellRainHelper : MonoBehaviour
    {
        public SpellName spellToGive;
        public SpellButton targetSlot = SpellButton.Secondary;
        private bool pickedUp = false;
        private SoundPlayer soundPlayer;

        void Start()
        {
            soundPlayer = GetComponent<SoundPlayer>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (pickedUp) return;

            Identity wizardId = other.transform.root.GetComponent<Identity>();
            if (wizardId != null)
            {
                PickupSpell(wizardId.owner);
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (pickedUp) return;

            Identity wizardId = collision.collider.transform.root.GetComponent<Identity>();
            if (wizardId != null)
            {
                PickupSpell(wizardId.owner);
            }
        }

        void PickupSpell(int pickerOwner)
        {
            pickedUp = true;

            if (!SpellRainSpawner.oneTimeSpells.ContainsKey(pickerOwner))
            {
                SpellRainSpawner.oneTimeSpells[pickerOwner] = [];
            }

            SpellRainSpawner.oneTimeSpells[pickerOwner][targetSlot] = new OneTimeSpell
            {
                spellName = spellToGive,
                button    = targetSlot,
                used      = false
            };

            Globals.spell_manager.AddSpellToPlayer(
                targetSlot,
                spellToGive,
                pickerOwner
            );

            if (soundPlayer != null)
            {
                soundPlayer.PlaySoundInstantiate("event:/sfx/ice/cryogenic-pick-up", 5f);
            }

            Plugin.Log.LogInfo($"Player {pickerOwner} picked up one-time spell: {spellToGive} in slot {targetSlot}");

            Destroy(gameObject, 0.1f);
        }
    }
}