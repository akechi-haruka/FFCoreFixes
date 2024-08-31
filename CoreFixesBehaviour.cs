using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Linq;
using JetBrains.Annotations;
using Siva;
using Siva.Network;
using UnityEngine;
using Theatrhythm;
using System.IO;
using FFCoreFixes.Patches;

namespace FFCoreFixes
{
    [BepInPlugin("eu.harukakyoubate.ff.corefixes", "FF Core Fixes", "1.5.1")]
    [BepInDependency("eu.harukakyoubate.altinput")]
    public class CoreFixesBehaviour : BaseUnityPlugin {
        private static readonly string[] AxisNames = { "Off", "HorizontalLeft1", "VerticalLeft1", "HorizontalRight1", "VerticalRight1", "HorizontalLeft2", "VerticalLeft2", "HorizontalRight2", "VerticalRight2", "HorizontalDpad1", "VerticalDpad1", "HorizontalDpad2", "VerticalDpad2" };

        private static ManualLogSource log;
        private static Harmony h;

        public static ConfigEntry<KeyboardShortcut> KeyCoin;
        public static ConfigEntry<KeyboardShortcut> KeyEnter;
        public static ConfigEntry<KeyboardShortcut> KeyLUp;
        public static ConfigEntry<KeyboardShortcut> KeyLDown;
        public static ConfigEntry<KeyboardShortcut> KeyLLeft;
        public static ConfigEntry<KeyboardShortcut> KeyLRight;
        public static ConfigEntry<KeyboardShortcut> KeyLLampSwitch;
        public static ConfigEntry<KeyboardShortcut> KeyRUp;
        public static ConfigEntry<KeyboardShortcut> KeyRDown;
        public static ConfigEntry<KeyboardShortcut> KeyRLeft;
        public static ConfigEntry<KeyboardShortcut> KeyRRight;
        public static ConfigEntry<KeyboardShortcut> KeyRLampSwitch;
        public static ConfigEntry<KeyboardShortcut> KeySelect;
        public static ConfigEntry<KeyboardShortcut> KeyTest;
        public static ConfigEntry<KeyboardShortcut> KeyService;
        public static ConfigEntry<int> ControllerDeadzone;
        public static ConfigEntry<string> CardId;
        public static ConfigEntry<bool> InvertRX;
        public static ConfigEntry<bool> InvertRY;
        public static ConfigEntry<bool> DisableGameSrvHttps;
        public static ConfigEntry<bool> ForceOfflineMatching;
        public static ConfigEntry<KeyboardShortcut> KeyCardSwipe;
        public static ConfigEntry<bool> TimeFreeze;
        public static ConfigEntry<bool> AllCollecaAvailable;
        public static ConfigEntry<bool> DumpMusicDB;
        public static ConfigEntry<bool> UseOnlyLeftStick;
        public static ConfigEntry<AltInputMode> AltInputSetting;
        public static ConfigEntry<String> PhotonApplicationId;
        public static ConfigEntry<bool> ConfigShowCursor;
        public static ConfigEntry<bool> AltInputP2Setting;

        private bool photonIsPatched;

        public enum AltInputMode {
            Off,
            Ps,
            Xbox
        }

        [UsedImplicitly]
        public void Awake() {
            log = Logger;
            Debug.Log("FF Core Fixes loading");
            h = new Harmony("eu.harukakyoubate.ff.corefixes");

            KeyCoin = Config.Bind("General Input", "Coin Key", new KeyboardShortcut(KeyCode.F3), "Inserts a coin");
            KeyService = Config.Bind("General Input", "Service", new KeyboardShortcut(KeyCode.F4), "Inserts a service coin");
            KeyTest = Config.Bind("General Input", "Test", new KeyboardShortcut(KeyCode.F5), "Enters the game's test/config menu");
            KeySelect = Config.Bind("General Input", "Select Key", new KeyboardShortcut(KeyCode.Space), "Moves the cursor in the test menu");
            KeyEnter = Config.Bind("General Input", "Enter Key", new KeyboardShortcut(KeyCode.Return), "Selects the current highlighted option in the test menu");
            ConfigShowCursor = Config.Bind("General Input", "Show Mouse Cursor", true, "Shows the mouse cursor. Disable if using touchscreen.");

            KeyLLampSwitch = Config.Bind("Game Input", "Left Action Button", new KeyboardShortcut(KeyCode.LeftShift), "Game button");
            KeyRLampSwitch = Config.Bind("Game Input", "Right Action Button", new KeyboardShortcut(KeyCode.RightControl), "Game button");
            KeyLUp = Config.Bind("Game Input", "Left Stick Up", new KeyboardShortcut(KeyCode.W), "Game button");
            KeyLDown = Config.Bind("Game Input", "Left Stick Down", new KeyboardShortcut(KeyCode.S), "Game button");
            KeyLLeft = Config.Bind("Game Input", "Left Stick Left", new KeyboardShortcut(KeyCode.A), "Game button");
            KeyLRight = Config.Bind("Game Input", "Left Stick Right", new KeyboardShortcut(KeyCode.D), "Game button");
            KeyRUp = Config.Bind("Game Input", "Right Stick Up", new KeyboardShortcut(KeyCode.UpArrow), "Game button");
            KeyRDown = Config.Bind("Game Input", "Right Stick Down", new KeyboardShortcut(KeyCode.DownArrow), "Game button");
            KeyRLeft = Config.Bind("Game Input", "Right Stick Left", new KeyboardShortcut(KeyCode.LeftArrow), "Game button");
            KeyRRight = Config.Bind("Game Input", "Right Stick Right", new KeyboardShortcut(KeyCode.RightArrow), "Game button");
            AltInputSetting = Config.Bind("Game Input (Controller)", "Controller Mode (AltInput)", AltInputMode.Off, "Use the AltInput plugin for controller input.");
            UseOnlyLeftStick = Config.Bind("Game Input (Controller)", "Use Only Left Stick", false, "Use only left stick from AltInput.");
            AltInputP2Setting = Config.Bind("Game Input (Controller)", "Use AltInput P2 For Right Stick", false, "Uses the P2 Slot from AltInput for the right stick.");


            InvertRX = Config.Bind("Game Input (Controller)", "Invert Right X Axis", false, "Inverts the right stick's X-Axis.");
            InvertRY = Config.Bind("Game Input (Controller)", "Invert Right Y Axis", false, "Inverts the right stick's Y-Axis.");
            ControllerDeadzone = Config.Bind("Game Input (Controller)", "Stick Deadzone", 50, new ConfigDescription("The deadzone size for stick input", new AcceptableValueRange<int>(0, 100)));

            CardId = Config.Bind("Network", "NESiCA Card ID", RandomString(16), "The NESiCA card ID used for card swiping.\nMust be 16 characters.");
            DisableGameSrvHttps = Config.Bind("Network", "Disable secure game server communication", true, "If enabled, game server communication uses HTTP instead of HTTPS. Will only work with unpatched SimpleNesys.dll");
            ForceOfflineMatching = Config.Bind("Network", "Spoof matching server", true, "Disables the matching server. Must be turned off if using real photon server.");
            KeyCardSwipe = Config.Bind("Network", "Swipe card", new KeyboardShortcut(KeyCode.Return), "Key to swipe NESiCA card");
            TimeFreeze = Config.Bind("Other", "Freeze Time", false, new ConfigDescription("Freezes the timer (except some occasions where the timer is used for menu transitions)", null, new ConfigurationManagerAttributes() {
                IsAdvanced = true
            }));
            DumpMusicDB = Config.Bind("Other", "Dump Music DB", false, new ConfigDescription("", null, new ConfigurationManagerAttributes() {
                IsAdvanced = true
            }));

            AllCollecaAvailable = Config.Bind("Gameplay", "All colleca permanently available", true, "This allows all colleca cards to be permanently available, regardless of system time.\n(This prevents a crash when there are no more colleca available after the game's shutdown)");

            PhotonApplicationId = Config.Bind("Network", "Photon Application ID", "", new ConfigDescription("Application GUID for Photon", null, new ConfigurationManagerAttributes() {
                IsAdvanced = true
            }));


            IsmACIOPatches.Log = Logger;
            NesicaReaderPatches.log = Logger;
            TouchPanelPatches.log = Logger;
            MiscPatches.log = Logger;
            GamePatches.Log = Logger;
            NetPatches.Log = Logger;
            h.PatchAll(typeof(IsmACIOPatches));
            h.PatchAll(typeof(NesicaReaderPatches));
            h.PatchAll(typeof(TouchPanelPatches));
            h.PatchAll(typeof(MiscPatches));
            h.PatchAll(typeof(GamePatches));
            h.PatchAll(typeof(NetPatches));
            h.PatchAll(typeof(NetPatches));
        }

        void OnLevelWasLoaded(int level) {
            log.LogDebug(level);
            if (level == 16) {
                log.LogMessage("FF Core Fixes v1.5 by Haruka.");
                log.LogMessage("F1 for mod config, " + KeyTest.Value.MainKey + " for test menu, " + KeyCoin.Value.MainKey + " to insert coin.");
                if (DumpMusicDB.Value) {
                    GMGDumper();
                }
            }
        }

        [UsedImplicitly]
        public void Update() {
            if (!photonIsPatched) {
                PhotonPatches.Log = Logger;
                h.PatchAll(typeof(PhotonPatches));
                photonIsPatched = true;
            }
            if (KeyCoin.Value.IsDown()) {
                log.LogMessage("Coin inserted");
                IsmACIOPatches.Credits++;
            }
            if (KeyService.Value.IsDown()) {
                log.LogMessage("Service coin inserted");
                IsmACIOPatches.Credits++;
            }
            if (KeyCardSwipe.Value.IsDown()) {
                NesicaReaderPatches.NesicaPlaced = !NesicaReaderPatches.NesicaPlaced;
                if (!NesicaReaderPatches.NesicaPlaced) {
                    NesicaReaderPatches.BlockContinuousReads = false;
                }
                log.LogMessage("NESiCA " + (NesicaReaderPatches.NesicaPlaced ? "placed on" : "removed from") + " reader. Press again to " + (NesicaReaderPatches.NesicaPlaced ? "remove card" : "scan card") + ".");
            }
            if (ConfigShowCursor.Value && !Cursor.visible) {
                Cursor.visible = true;
            } else if (!ConfigShowCursor.Value && Cursor.visible) {
                Cursor.visible = false;
            }
        }

        private static string RandomString(int length) {
            System.Random r = new System.Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[r.Next(s.Length)]).ToArray());
        }

        private static void GMGDumper() {
            String str = "id,name,title,subtitle,diffindex,diffval,series,medleyid,medleymusictype,medleytype\n";
            foreach (var val in MedleyTable.entityMedleyTable.dictionary.Values) {
                for (int i = 0; i < 6; i++) {
                    str += val.musicID[0] + "," + val.name_jp + "," + val.title + "," + val.subtitle + "," + i + "," + val.difficulties[i] + "," + val.seriesID + "," + val.medleyID + "," + val.medleyMusicType + "," + val.medleyType + "\n";
                }
            }
            File.WriteAllText("mdb.csv", str);
            str = "MonsterID,NameJP,NameEN,HP,RaidBossScale\n";
            foreach (var val in MonsterTable.EntityMonsterTable.dictionary.Values) {
                str += val.monsterID + "," + val.name_JP + "," + val.name_EN + "," + val.hp + "," + val.raidBossScale + "\n";
            }
            File.WriteAllText("monster.csv", str);
            str = "ID,CharacterID,Rarity\n";
            foreach (var val in CollectionCardTable.CharacterTable.Dictionary.Values) {
                str += val.ID + "," + val.characterID + "," + val.rarity + "\n";
            }
            foreach (var val in CollectionCardTable.JacketTable.Dictionary.Values) {
                str += val.ID + "," + val.characterID + "," + val.rarity + "\n";
            }
            File.WriteAllText("colleca.csv", str);
            str = "ID,Name\n";
            foreach (var val in AirshipTable.EntityAirshipTable.dictionary.Values) {
                str += val.airshipId + "," + val.airshipName + "\n";
            }
            File.WriteAllText("airship.csv", str);
            str = "ID,Name\n";
            foreach (var val in ProficaIllustTable.table.dictionary.Values) {
                str += val.ID + "," + val.name + "\n";
            }
            File.WriteAllText("profica_illust.csv", str);
            str = "ID,Name\n";
            foreach (var val in ProficaDesignTable.table.dictionary.Values) {
                str += val.ID + "," + val.name + "\n";
            }
            File.WriteAllText("profica_design.csv", str);
            str = "ID,Name,Text,ItemID\n";
            foreach (var val in PlayerTitleTable.table.dictionary.Values) {
                str += val.ID + "," + val.name + "," + val.text + "," + val.ItemID + "\n";
            }
            File.WriteAllText("degress.csv", str);
            log.LogMessage("MDB dumped");
        }

    }
}
