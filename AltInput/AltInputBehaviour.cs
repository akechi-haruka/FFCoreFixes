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
        private static ConfigEntry<String> AltInputDevId;

        internal static ManualLogSource Log;

        private static DirectInput directInput = new DirectInput();
        private static AltDirectInputDevice diDevice;
        private static bool altEnabled;

        void Awake() {
            Log = Logger;
            Logger.LogDebug("AltInput starting");

            ConfigEnabled = Config.Bind("General", "Enable Plugin", true, "Enables AltInput");
            OSDEnabled = Config.Bind("General", "OSD", true, "Enables display of controller GUIDs on game start");
            AltInputDevId = Config.Bind("General", "AltInput DirectInput device GUID", "", "The device ID if you want to use AltInput");

            if (!ConfigEnabled.Value) {
                Logger.LogInfo("Plugin is disabled");
                return;
            }

            Logger.LogDebug("Listing devices");
            directInput = new DirectInput();
            foreach (var dev in directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AllDevices)) {
                Joystick js = new Joystick(directInput, dev.InstanceGuid);
                String msg = "AltInput: Detected Controller '" + dev.InstanceGuid + "' (" + dev.ProductName + "): " +
                    js.Capabilities.AxeCount + " Axes, " + js.Capabilities.ButtonCount + " Buttons, " +
                    js.Capabilities.PovCount + " POV(s)";
                if (OSDEnabled.Value) {
                    Logger.LogMessage(msg);
                } else {
                    Logger.LogInfo(msg);
                }
                if (dev.InstanceGuid.ToString() == AltInputDevId.Value) {
                    diDevice = new AltDirectInputDevice(directInput, DeviceClass.GameControl, dev.InstanceGuid);
                    if (OSDEnabled.Value) {
                        Logger.LogMessage("Selected DirectInput device: " + dev.ProductName);
                    } else {
                        Logger.LogInfo("Selected DirectInput device: " + dev.ProductName);
                    }
                    altEnabled = true;
                    break;
                }
            }
            if (altEnabled) {
                try {
                    Logger.LogDebug("Opening device");
                    diDevice.OpenDevice();
                    if (OSDEnabled.Value) {
                        Logger.LogMessage("Controller OK");
                    }
                } catch (Exception ex) {
                    Logger.LogMessage("AltInput: Failed to open device");
                    Logger.LogError(ex);
                }
            }
        }

        void Update() {
            if (altEnabled) {
                diDevice.ProcessInput();
            }
        }

        public static bool IsEnabled() {
            return altEnabled;
        }

        public static AltDirectInputDevice GetDevice() {
            return diDevice;
        }

    }
}
