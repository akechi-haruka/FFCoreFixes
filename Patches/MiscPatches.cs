using System;
using System.Diagnostics;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Siva;
using System.Linq;
using Siva.Network;
using UnityEngine;

namespace FFCoreFixes {
    public class MiscPatches {

        public static ManualLogSource log;

        [HarmonyPatch(typeof(Error), "Start")]
        [HarmonyPrefix]
        public static bool ErrorStart() {
            log.LogError("System error displaying: " + ApplicationManager.Instance.ErrorCode);
            return true;
        }

        // To prevent BepInEx's keyboard locking we kinda need to do this
        [HarmonyPatch(typeof(KeyboardShortcut), "ModifierKeyTest")]
        [HarmonyPrefix]
        static bool ModifierKeyTest(KeyboardShortcut __instance, ref bool __result) {
            __result = __instance.Modifiers.All(c => c == __instance.MainKey || Input.GetKey(c));
            return false;
        }

    }
}
