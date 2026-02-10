using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SpellcastModFramework.Loading;
using SpellcastModFramework.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BalancePatch
{
    [BepInPlugin("org.bepinex.plugins.balancepatch", "Balance Patch", "1.0.0")]
    [BepInDependency("com.spellcast.modframework", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        public static ManualLogSource Log;
        public static System.Random Random = new();
        public static System.Random RandomiserRng;
        public static List<Boosted.BoostedPatch.UpgradeOption> CurrentUpgradeOptions = [];
        public static HashSet<(SpellName, string)> BannedUpgrades = [];

        private int upgradesSelected = 0;
        private readonly int MaxUpgrades = 3;
        private int freeBans = 1;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            Log.LogInfo("Balance Patch plugin loading...");

            ModuleManager.RegisterModule(new Balance.BalanceModule());
            ModuleManager.RegisterModule(new Debug.DebugModule());
            ModuleManager.RegisterModule(new Boosted.BoostedModule());
            ModuleManager.RegisterModule(new Randomiser.RandomiserModule());
            
            RegisterWithFramework();

            Log.LogInfo("Balance Patch plugin loaded!");
        }

        private void RegisterWithFramework()
        {
            ModUIRegistry.RegisterMod(
                "Balance Patch",
                "Core balance changes, boosted upgrades, and randomiser",
                BuildModUI,
                priority: 10
            );
        }

        private void BuildModUI(Transform parent)
        {
            UIComponents.CreateModuleToggleButton(parent, "Balance");
            UIComponents.CreateModuleToggleButton(parent, "Debug");
            UIComponents.CreateModuleToggleButton(parent, "Boosted");
            UIComponents.CreateModuleToggleButton(parent, "Randomiser");
            
            var inputField = UIComponents.CreateInputField(parent, "SeedInput", "Enter seed...", 200, 40);
            
            var setSeedButton = UIComponents.CreateButton(parent, "SetSeedButton", "Set Seed", 200, 40);
            setSeedButton.onClick.AddListener(() =>
            {
                int seedInt = Randomiser.RandomiserHelpers.HashSeed(inputField.text);
                RandomiserRng = new System.Random(seedInt);
                Log.LogInfo($"[Randomiser] Set seed to '{inputField.text}' (hash: {seedInt})");
            });
        }

        private void OnGUI()
        {
            if (CurrentUpgradeOptions.Count > 0)
            {
                DrawUpgradeOptions();
            }
        }

        private void DrawUpgradeOptions()
        {
            int upgradeX = 20;
            int upgradeY = Screen.height / 2 - 100;
            int optionHeight = 35;
            int buttonWidth = 50;
            int labelWidth = 180;
            int banButtonWidth = 40;

            GUI.Box(new Rect(upgradeX - 5, upgradeY - 20, labelWidth + buttonWidth * 2 + banButtonWidth + 15, CurrentUpgradeOptions.Count * optionHeight + 20), "Upgrades");

            for (int i = 0; i < CurrentUpgradeOptions.Count; i++)
            {
                var option = CurrentUpgradeOptions[i];
                int yPos = upgradeY + i * optionHeight;

                GUI.Label(new Rect(upgradeX, yPos, labelWidth, 30), option.GetDisplayText(), GetTierStyle(option.Tier));

                if (!Boosted.BoostedPatch.TryGetUpDownMultFromOption(option, out float upMult, out float downMult))
                    continue;

                if (GUI.Button(new Rect(upgradeX + labelWidth, yPos, buttonWidth, 30), $"{upMult * 100:F0}%", StyleManager.Green))
                {
                    SelectUpgrade(option, true);
                }

                if (GUI.Button(new Rect(upgradeX + labelWidth + buttonWidth, yPos, buttonWidth, 30), $"{downMult * 100:F0}%", StyleManager.Red))
                {
                    SelectUpgrade(option, false);
                }

                if (GUI.Button(new Rect(upgradeX + labelWidth + buttonWidth * 2 + 5, yPos, banButtonWidth, 30), "Ban"))
                {
                    BanUpgrade(option);
                }
            }
        }

        private void SelectUpgrade(Boosted.BoostedPatch.UpgradeOption option, bool isUp)
        {
            Boosted.BoostedPatch.ApplyUpgrade(option, isUp);
            upgradesSelected++;
            CurrentUpgradeOptions.Remove(option);
            MaybeResetUpgrades();
        }

        private void BanUpgrade(Boosted.BoostedPatch.UpgradeOption option)
        {
            Log.LogInfo($"Banned: {option.Spell} + {option.Attribute}");
            BannedUpgrades.Add((option.Spell, option.Attribute));
            CurrentUpgradeOptions.Remove(option);

            if (freeBans <= 0)
                upgradesSelected++;
            else
                freeBans--;

            MaybeResetUpgrades();
        }

        private void MaybeResetUpgrades()
        {
            if (upgradesSelected < MaxUpgrades) return;
            CurrentUpgradeOptions.Clear();
            upgradesSelected = 0;
            freeBans = 1;
        }

        private static GUIStyle GetTierStyle(Boosted.Upgrades.Tier tier)
        {
            if (tier.Equals(Boosted.Upgrades.Legendary)) return StyleManager.Gold;
            if (tier.Equals(Boosted.Upgrades.Rare))      return StyleManager.Purple;
            return StyleManager.White;
        }
    }
}