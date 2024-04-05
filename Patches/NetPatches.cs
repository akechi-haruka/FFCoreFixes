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
    public class NetPatches {

        public static ManualLogSource Log;

        // lmao square
        // ReSharper disable once StringLiteralTypo
        [HarmonyPatch(typeof(ServerManager), "GetProtcol")]
        [HarmonyPrefix]
        public static bool GetProtocol(ref String __result) {
            if (CoreFixesBehaviour.DisableGameSrvHttps.Value) {
                __result = "http://";
                return false;
            } else {
                return true;
            }
        }

        [HarmonyPatch(typeof(PhotonManager), "Connect")]
        [HarmonyPostfix]
        public static void Connect(string serverAddress, string cabinetID, string devLobbyName, string option = null) {
            if (CoreFixesBehaviour.ForceOfflineMatching.Value) {
                PhotonManager.Status = PhotonStatus.Online;
            }
        }

        [HarmonyPatch(typeof(PhotonManager), "Status", MethodType.Getter)]
        [HarmonyPostfix]
        public static void Status(ref PhotonStatus __result) {
            if (CoreFixesBehaviour.ForceOfflineMatching.Value) {
                __result = PhotonStatus.Online;
            }
        }

        [HarmonyPatch(typeof(TheaterController), "IsStandAloneMode", MethodType.Getter)]
        [HarmonyPostfix]
        public static void IsStandAloneMode(ref bool __result) {
            if (CoreFixesBehaviour.ForceOfflineMatching.Value) {
                __result = true;
            }
        }

        [HarmonyPatch(typeof(TheaterController), "CheckPhotonDisconnect")]
        [HarmonyPrefix]
        public static bool CheckPhotonDisconnect(TheaterController __instance) {
            if (CoreFixesBehaviour.ForceOfflineMatching.Value) {
                //__instance.isDisconnected = true;
                return false;
            }
            return true;
        }

        /*[HarmonyPatch(typeof(ApplicationManager), "get_NesysOnline")]
        [HarmonyPrefix]
        public static bool NesysOnline(ref bool __result) {
            if (CoreFixesBehaviour.ForceOfflineMatching.Value) {
                log.LogDebug(ApplicationManager.Instance.ServerState.CardServer +"|" +ApplicationManager.Instance.ServerState.NesysServer);
                //__result = true;
                return true;
            } else {
                return true;
            }
        }*/

        /*[HarmonyPatch(typeof(ServerConnectionState), "set_CardServer")]
        [HarmonyPostfix]
        public static void set_CardServer(ServerConnectionState __instance) {
            log.LogDebug(__instance.CardServer +"|" + __instance.nesysServer + "|" + __instance.matchingServer);
            if (!__instance.CardServer || !__instance.nesysServer) {
                log.LogWarning("network disconnected");
                log.LogDebug(new StackTrace().ToString());
            }
        }*/

        [HarmonyPatch(typeof(NetworkCheckSequence), "OnCardServerInitialized")]
        [HarmonyPrefix]
        public static bool OnCardServerInitialized(bool isSuccess) {
            Log.LogDebug("OnCardServerInitialized(isSuccess=" + isSuccess + ")");
            return true;
        }

        [HarmonyPatch(typeof(NesysClient), "UpdateMaintenaceTime")]
        [HarmonyPrefix]
        public static bool UpdateMaintenaceTime() {
            NesysClient.maintenanceTime = -3;
            return false;
        }

        // weird ass thing I can't figure out but HealthCheck succeedes if response=200 and response=null??
        [HarmonyPatch(typeof(CardHealthCheck), "OnResponseHealthCheck")]
        [HarmonyPrefix]
        public static bool OnResponseHealthCheck(ServerCommand.ResponseResult result, ServerCommand.ServiceEchoResponseBody response, CardHealthCheck __instance) {
            Log.LogDebug("OnResponseHealthCheck(result="+result?.success+"/"+result?.statusCode+",response="+response?.result+"/"+response?.message+") -> Status = " + __instance.Status);
            if (response != null) {
                __instance.Status = response.code.CompareTo("#007") == 0 ? ServerStatus.VersionMismatch : ServerStatus.Online;
            } else {
                __instance.Status = ServerStatus.Online;
            }

            __instance.result = result;
            return false;
        }

    }
}
