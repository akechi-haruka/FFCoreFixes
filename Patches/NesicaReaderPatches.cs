using BepInEx.Logging;
using HarmonyLib;
using Siva;
using System.Text;
using UnityEngine;
using static NESiCAReader.Dll;

namespace FFCoreFixes {
    class NesicaReaderPatches {

        private static readonly RFID_ID BLANK_CARD = new RFID_ID();

        public static ManualLogSource log;
        public static bool AllowReading;
        public static bool NesicaPlaced;
        public static bool BlockContinuousReads;

        [HarmonyPatch(typeof(NESiCAReader.Dll), "NESiCAReaderOpen")]
        [HarmonyPrefix]
        public static bool NESiCAReaderOpen(ref uint __result) {
            log.LogDebug("NESiCAReaderOpen");
            __result = 0;
            return false;
        }

        [HarmonyPatch(typeof(NESiCAReader.Dll), "NESiCAReaderClose")]
        [HarmonyPrefix]
        public static bool NESiCAReaderClose(ref uint __result) {
            log.LogDebug("NESiCAReaderClose");
            __result = 0;
            return false;
        }

        [HarmonyPatch(typeof(NESiCAReader.Dll), "NESiCAReaderUpdate")]
        [HarmonyPrefix]
        public static bool NESiCAReaderUpdate(ref uint __result) {
            //log.LogDebug("NESiCAReaderUpdate");
            __result = 0;
            return false;
        }

        [HarmonyPatch(typeof(NESiCAReader.Dll), "NESiCAReaderGetXioStatus")]
        [HarmonyPrefix]
        public static bool NESiCAReaderGetXioStatus(ref int __result) {
            //log.LogDebug("NESiCAReaderGetXioStatus");
            __result = 1;
            return false;
        }

        [HarmonyPatch(typeof(NESiCAReader.Dll), "NESiCAReaderRead")]
        [HarmonyPrefix]
        public static bool NESiCAReaderRead(uint timeout, ref uint __result) {
            log.LogDebug("NESiCAReaderRead(timeout=" + timeout + ")");
            AllowReading = true;
            if (BlockContinuousReads) {
                __result = 3;
            } else {
                __result = 0;
            }
            return false;
        }

        [HarmonyPatch(typeof(NESiCAReader.Dll), "NESiCAReaderCancelRead")]
        [HarmonyPrefix]
        public static bool NESiCAReaderCancel(ref uint __result) {
            log.LogDebug("NESiCAReaderCancelRead");
            AllowReading = false;
            BlockContinuousReads = false;
            __result = 0;
            return false;
        }

        [HarmonyPatch(typeof(NESiCAReader.Dll), "NESiCAReaderGetStatus")]
        [HarmonyPrefix]
        public static bool NESiCAReaderGetStatus(ref uint __result) {
            //log.LogDebug("NESiCAReaderGetStatus");
            if (BlockContinuousReads) {
                __result = 1;
            } else {
                __result = 0;
            }
            return false;
        }

        [HarmonyPatch(typeof(NESiCAReader.Dll), "NESiCAReaderGetResult")]
        [HarmonyPrefix]
        public static bool NESiCAReaderGetResult(ref uint __result) {
            //log.LogDebug("NESiCAReaderGetResult");
            if (NesicaPlaced) {
                __result = 0;
            } else {
                __result = 3;
            }

            return false;
        }

        [HarmonyPatch(typeof(NESiCAReader.Dll), "NESiCAReaderGetID")]
        [HarmonyPrefix]
        public static bool NESiCAReaderGetID(out RFID_ID id, ref uint __result) {
            if (AllowReading && NesicaPlaced) {
                log.LogDebug("NESiCAReaderGetID");
                log.LogInfo("Scanned NESiCA card with ID: " + CoreFixesBehaviour.CardId.Value);
                id = new RFID_ID {
                    m_ucID = Encoding.ASCII.GetBytes(CoreFixesBehaviour.CardId.Value)
                };
                BlockContinuousReads = true;
                __result = 1;
            } else {
                __result = 0;
                id = BLANK_CARD;
            }
            return false;
        }

        [HarmonyPatch(typeof(GameTitle), "CertStateChanged")]
        [HarmonyPrefix]
        public static bool CertStateChanged(NesicaCertController.CertStateChangedEventArgs e) {
            log.LogDebug("GameTitle.CertStateChanged(e="+e.Status+")");
            return true;
        }

        /*[HarmonyPatch(typeof(NesicaCertController), "ShowWindowFinished")]
        [HarmonyPrefix]
        private bool ShowWindowFinished(NesicaCertController __instance, object sender, NesicaWindowController.ShowWindowFinishedEventArgs e) {
            NesicaCertController.Status status = e.Status;
            if (status != NesicaCertController.Status.Idle) {
                if (status == NesicaCertController.Status.Removal || status == NesicaCertController.Status.EventRemoval) {
                    __instance.nesicaRemovalCheck.CheckStart();
                }
            } else {
                if (!__instance.isClosed && __instance.isNesicaRead) {
                    __instance.isNesicaRead = false;
                    __instance.NesicaCertStart();
                }
                SingletonMonoBehaviour<ApplicationManager>.Instance.LoginStatus = ApplicationManager.UserLoginStatus.Logout;
                __instance.isFirstPlay = false;
                log.LogDebug("keeping carddata workaround");
                //__instance.cardData = null;
            }
            return false;
        }*/

    }
}
