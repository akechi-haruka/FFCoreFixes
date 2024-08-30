using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace AltInput {
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    [BepInPlugin("eu.harukakyoubate.altinput", "AltInput", "1.0")]
    public class AltInputBehaviour : BaseUnityPlugin {

        private static ConfigEntry<bool> ConfigEnabled;
        private static ConfigEntry<bool> OSDEnabled;
        private static ConfigEntry<String> AltInputDevId1;
        private static ConfigEntry<bool> AutoUseOnSingle;
        private static ConfigEntry<bool> AutoMerge;
        private static ConfigEntry<String> AltInputDevId2;

        internal static ManualLogSource Log;

        private static bool[] diActive = new bool[2];
        private static AltDirectInputDevice diDevice1;
        private static AltDirectInputDevice diDevice2;
        private static bool altEnabled;

        void Awake() {
            Log = Logger;
            Logger.LogDebug("AltInput starting");

            ConfigEnabled = Config.Bind("General", "Enable Plugin", true, "Enables AltInput");
            OSDEnabled = Config.Bind("General", "OSD", true, "Enables display of controller GUIDs on game start");
            AltInputDevId1 = Config.Bind("General", "AltInput DirectInput device GUID (1)", "", "The device ID if you want to use AltInput");
            AltInputDevId2 = Config.Bind("General", "AltInput DirectInput device GUID (2)", "", "The device ID if you want to use AltInput");
            AutoUseOnSingle = Config.Bind("General", "Auto-Use Single Controller", true, "Automatically use controller if only one is present");

            if (!ConfigEnabled.Value) {
                Logger.LogInfo("Plugin is disabled");
                return;
            }

            Logger.LogDebug("Listing devices");
            for (int p = 1; p <= 2; p++) {
                DirectInput directInput = new DirectInput();
                IList<DeviceInstance> devList = directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AllDevices);
                foreach (DeviceInstance dev in devList) {
                    Joystick js = new Joystick(directInput, dev.InstanceGuid);
                    String msg = "AltInput: Detected Controller '" + dev.InstanceGuid + "' (" + dev.ProductName + "): " +
                        js.Capabilities.AxeCount + " Axes, " + js.Capabilities.ButtonCount + " Buttons";
                    if (OSDEnabled.Value) {
                        Logger.LogMessage(msg);
                    } else {
                        Logger.LogInfo(msg);
                    }
                    if ((p == 1 && (dev.InstanceGuid.ToString() == AltInputDevId1.Value || (devList.Count == 1 && AutoUseOnSingle.Value))) || (p == 2 && dev.InstanceGuid.ToString() == AltInputDevId2.Value)) {
                        if (p == 1) {
                            diDevice1 = new AltDirectInputDevice(directInput, DeviceClass.GameControl, dev.InstanceGuid);
                        } else {
                            diDevice2 = new AltDirectInputDevice(directInput, DeviceClass.GameControl, dev.InstanceGuid);
                        }
                        diActive[p - 1] = true;
                        Logger.LogInfo("Selected DirectInput device (P"+p+"): " + dev.ProductName);
                        altEnabled = true;
                    }
                }
            }
            if (altEnabled) {
                try {
                    Logger.LogDebug("Opening device P1");
                    diDevice1.OpenDevice();
                    if (OSDEnabled.Value) {
                        Logger.LogMessage("Controller P1 OK");
                    }
                } catch (Exception ex) {
                    Logger.LogMessage("AltInput: Failed to open device");
                    Logger.LogError(ex);
                }
                if (diActive[1]) {
                    try {
                        Logger.LogDebug("Opening device P2");
                        diDevice2.OpenDevice();
                        if (OSDEnabled.Value) {
                            Logger.LogMessage("Controller P2 OK");
                        }
                    } catch (Exception ex) {
                        Logger.LogMessage("AltInput: Failed to open device");
                        Logger.LogError(ex);
                    }
                }
            }
        }

        void Update() {
            if (altEnabled) {
                if (diActive[0]) {
                    diDevice1.ProcessInput();
                }
                if (diActive[1]) {
                    diDevice2.ProcessInput();
                }
            }
        }

        public static bool IsEnabled() {
            return altEnabled;
        }

        public static bool IsDevice1Active() {
            return diActive[0];
        }

        public static bool IsDevice2Active() {
            return diActive[1];
        }

        public static AltDirectInputDevice GetDevice1() {
            return diDevice1;
        }

        public static AltDirectInputDevice GetDevice2() {
            return diDevice2;
        }

    }
}
