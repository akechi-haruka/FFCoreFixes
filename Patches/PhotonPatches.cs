using BepInEx.Logging;
using HarmonyLib;
using Siva.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FFCoreFixes.Patches {
    internal class PhotonPatches {

        public static ManualLogSource Log;

        [HarmonyPrefix, HarmonyPatch(typeof(NetworkCheckSequence), "OnMatchingServerInitialized")]
        public static bool OnMatchingServerInitialized(PhotonStatus status) {
            Log.LogInfo("Photon: " + status);
            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PhotonManager), "Status", MethodType.Setter)]
        public static void Status_set() {
            Log.LogInfo("Photon Status: " + PhotonManager.Status);
            if (PhotonManager.Status == PhotonStatus.Error) {
                Log.LogError(new System.Diagnostics.StackTrace().ToString());
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PhotonNetwork), "ConnectUsingSettings")]
        public static bool ConnectUsingSettings(ref bool __result, string gameVersion) {
            Log.LogInfo("Photon:ConnectUsingSettings");
            PhotonNetwork.logLevel = PhotonLogLevel.Full;
            PhotonNetwork.PhotonServerSettings.AppID = CoreFixesBehaviour.PhotonApplicationId.Value;
            if (CoreFixesBehaviour.UsePhotonCloud.Value) {
                Log.LogInfo("Photon:UseCloud");
                PhotonNetwork.PhotonServerSettings.UseCloud(CoreFixesBehaviour.PhotonApplicationId.Value);
                PhotonHandler.BestRegionCodeInPreferences = CloudRegionCode.eu;
                PhotonNetwork.PhotonServerSettings.Protocol = ExitGames.Client.Photon.ConnectionProtocol.Tcp;
                __result = PhotonNetwork.ConnectToBestCloudServer(gameVersion);
                return false;
            }
            return true;
        }

    }
}
