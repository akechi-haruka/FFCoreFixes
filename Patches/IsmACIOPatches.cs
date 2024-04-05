using AltInput;
using BepInEx.Logging;
using HarmonyLib;
using ismACIO;
using UnityEngine;

namespace FFCoreFixes {
    
    public class IsmACIOPatches {

        public static ManualLogSource Log;
        public static uint Credits = 1;

        [HarmonyPatch(typeof(Dll), "ismACIO_Init")]
        [HarmonyPrefix]
        public static bool Init(ref int __result) {
            Log.LogDebug("ismACIO_Init");
            __result = 0;
            return false;
        }

        [HarmonyPatch(typeof(Dll), "ismACIO_Update")]
        [HarmonyPrefix]
        public static bool Update(ref uint __result) {
            //log.LogDebug("ismACIO_Update");
            __result = 0;
            return false;
        }

        [HarmonyPatch(typeof(Dll), "ismACIO_GetStatus")]
        [HarmonyPrefix]
        public static bool GetStatus(ref int __result) {
            //log.LogDebug("ismACIO_GetStatus");
            __result = 2;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Dll), "ismACIO_GetInputD")]
        public static bool GetInputD(ref uint __result) {
            uint keys = 0;

            float dz = CoreFixesBehaviour.ControllerDeadzone.Value / 100F;

            if (AltInputBehaviour.IsEnabled()) {
                AltDirectInputDevice dev = AltInputBehaviour.GetDevice();
                if (dev.HasState()) {
                    if (CoreFixesBehaviour.AltInputSetting.Value == CoreFixesBehaviour.AltInputMode.Ps) {
                        float lx = norm(dev.GetX());
                        float ly = norm(dev.GetY());
                        float rx = norm(dev.GetZ());
                        float ry = norm(dev.GetRZ());
                        if (CoreFixesBehaviour.InvertRX.Value) {
                            rx *= -1;
                        }
                        if (CoreFixesBehaviour.InvertRY.Value) {
                            ry *= -1;
                        }
                        float lb = norm(dev.GetRX());
                        float rb = norm(dev.GetRY());

                        keys |= ly > dz ? 512U : 0;
                        keys |= ly < -dz ? 256U : 0;
                        keys |= lx > dz ? 2048U : 0;
                        keys |= lx < -dz ? 1024U : 0;
                        keys |= lb >= 0.5F ? 32U : 0;
                        keys |= rb >= 0.5F ? 65536U : 0;

                        if (!CoreFixesBehaviour.UseOnlyLeftStick.Value) {
                            keys |= ry > dz ? 2U : 0;
                            keys |= ry < -dz ? 1U : 0;
                            keys |= rx > dz ? 8U : 0;
                            keys |= rx < -dz ? 4U : 0;
                        }
                    } else if (CoreFixesBehaviour.AltInputSetting.Value == CoreFixesBehaviour.AltInputMode.Xbox) {
                        float lx = norm(dev.GetX());
                        float ly = norm(dev.GetY());
                        float rx = norm(dev.GetRX());
                        float ry = norm(dev.GetRY());
                        if (CoreFixesBehaviour.InvertRX.Value) {
                            rx *= -1;
                        }
                        if (CoreFixesBehaviour.InvertRY.Value) {
                            ry *= -1;
                        }

                        keys |= ly > dz ? 512U : 0;
                        keys |= ly < -dz ? 256U : 0;
                        keys |= lx > dz ? 2048U : 0;
                        keys |= lx < -dz ? 1024U : 0;

                        if (!CoreFixesBehaviour.UseOnlyLeftStick.Value) {
                            keys |= ry > dz ? 2U : 0;
                            keys |= ry < -dz ? 1U : 0;
                            keys |= rx > dz ? 8U : 0;
                            keys |= rx < -dz ? 4U : 0;
                        }
                    }
                }
            }
            
            keys |= CoreFixesBehaviour.KeyEnter.Value.IsPressed() ? 128U : 0;
            keys |= CoreFixesBehaviour.KeyLUp.Value.IsPressed() ? 256U : 0;
            keys |= CoreFixesBehaviour.KeyLDown.Value.IsPressed() ? 512U : 0;
            keys |= CoreFixesBehaviour.KeyLLeft.Value.IsPressed() ? 1024U : 0;
            keys |= CoreFixesBehaviour.KeyLRight.Value.IsPressed() ? 2048U : 0;
            keys |= CoreFixesBehaviour.KeyLLampSwitch.Value.IsPressed() ? 32U : 0;
            keys |= CoreFixesBehaviour.KeyRUp.Value.IsPressed() ? 1U : 0;
            keys |= CoreFixesBehaviour.KeyRDown.Value.IsPressed() ? 2U : 0;
            keys |= CoreFixesBehaviour.KeyRLeft.Value.IsPressed() ? 4U : 0;
            keys |= CoreFixesBehaviour.KeyRRight.Value.IsPressed() ? 8U : 0;
            keys |= CoreFixesBehaviour.KeyRLampSwitch.Value.IsPressed() ? 65536U : 0;
            keys |= CoreFixesBehaviour.KeySelect.Value.IsPressed() ? 4096U : 0;
            keys |= CoreFixesBehaviour.KeyTest.Value.IsPressed() ? 8192U : 0;
            keys |= CoreFixesBehaviour.KeyService.Value.IsPressed() ? 16384U : 0;
            keys |= CoreFixesBehaviour.KeyCoin.Value.IsPressed() ? 32768U : 0;

            __result = keys;
            return false;
        }

        private static float norm(float f) {
            return (f - 0.5F) * 2;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Dll), "ismACIO_GetInputA")]
        public static bool GetInputA(ref Dll.ismACIO_ANALOG_INPUT value) {
            //log.LogDebug("ismACIO_GetInputA");
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Dll), "ismACIO_SetOutputD")]
        public static bool SetOutputD(uint value) {
            //log.LogDebug("ismACIO_SetOutputD");
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Dll), "ismACIO_SetOutputA")]
        public static bool SetOutputA(ref Dll.ismACIO_OUTPUT value) {
            //log.LogDebug("ismACIO_SetOutputA");
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Dll), "ismACIO_GetCoin")]
        public static bool GetCoin(int port, ref uint __result) {
            //log.LogDebug("ismACIO_GetCoin");
            __result = Credits;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Dll), "ismACIO_DecreaseCoin")]
        public static bool DecreaseCoin(int port, int num) {
            //log.LogDebug("ismACIO_DecreaseCoin(port="+port+",num="+num+")");
            if (num > Credits) {
                Log.LogWarning("Trying to use " + num + " credit(s) while only having " + Credits);
                Credits = 0;
            } else {
                Credits -= (uint)num;
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Dll), "ismACIO_GetCoinStatus")]
        public static bool GetCoinStatus(int port, ref int __result) {
            //log.LogDebug("ismACIO_GetCoinStatus");
            __result = 0;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Dll), "ismACIO_Delete")]
        public static bool Delete(ref int __result) {
            Log.LogDebug("ismACIO_Delete");
            __result = 0;
            return false;
        }

    }
}
