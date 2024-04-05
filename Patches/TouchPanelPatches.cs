using BepInEx.Logging;
using HarmonyLib;
using Siva.Device;

namespace FFCoreFixes {

    class TouchPanelPatches {

        public static ManualLogSource log;

        [HarmonyPatch(typeof(TouchPanel), "Setup")]
        [HarmonyPrefix]
        public static bool Setup() {
            log.LogDebug("TouchPanel.Setup");
            TouchPanel.Calibrate();
            return false;
        }

        [HarmonyPatch(typeof(TouchPanel), "Calibrate")]
        [HarmonyPrefix]
        public static bool Calibrate() {
            log.LogDebug("TouchPanel.Calibrate");
            TouchPanel.CalibrationStatus = TouchPanel.Status.OK;
            return false;
        }

        [HarmonyPatch(typeof(TouchPanel), "State")]
        [HarmonyPrefix]
        public static bool State() {
            log.LogDebug("TouchPanel.State");
            TouchPanel.ConnectionStatus = TouchPanel.Status.OK;
            return false;
        }

    }

}
