using BepInEx.Logging;
using HarmonyLib;
using Siva;
using Siva.Device;
using System;
using System.IO;
using Theatrhythm;
using UnityEngine;

namespace FFCoreFixes {
    class GamePatches {

        public static ManualLogSource Log;

        [HarmonyPatch(typeof(FxCycle), "Update")]
        [HarmonyPrefix]
        public static bool FxCycleUpdate(FxCycle __instance) {
            __instance.CameraMovement();
            return false;
        }

		/*
		 * Reimplementation of ControlPad.Update to remove some debug code that clashes with custom controller keybindings.
		 */
        [HarmonyPatch(typeof(ControlPad), "Update")]
        [HarmonyPrefix]
        public static bool ControlPadUpdate(ControlPad __instance) {
			int num = 0;
			for (int i = 0; i < __instance.properties.Length; i++) {
				if (__instance.properties[i].Release) {
					__instance.properties[i].Clear();
				}
			}
			for (int j = 0; j < __instance.properties.Length - 1; j++) {
				if (!__instance.properties[j].Enabled) {
					for (int k = j + 1; k < __instance.properties.Length; k++) {
						if (__instance.properties[k].Enabled) {
							__instance.properties[j].Enabled = __instance.properties[k].Enabled;
							__instance.properties[j].Used = __instance.properties[k].Used;
							__instance.properties[j].TouchDown = __instance.properties[k].TouchDown;
							__instance.properties[j].Touch = __instance.properties[k].Touch;
							__instance.properties[j].Release = __instance.properties[k].Release;
							__instance.properties[j].Slide = __instance.properties[k].Slide;
							__instance.properties[j].TurnSlide = __instance.properties[k].TurnSlide;
							__instance.properties[j].PreSlide = __instance.properties[k].PreSlide;
							__instance.properties[j].SlideDegree = __instance.properties[k].SlideDegree;
							__instance.properties[j].Position = __instance.properties[k].Position;
							__instance.properties[j].FingerId = __instance.properties[k].FingerId;
							__instance.properties[j].Checked = __instance.properties[k].Checked;
							__instance.properties[k].Clear();
						}
					}
				}
			}
			for (int l = 0; l < __instance.properties.Length; l++) {
				if (__instance.properties[l].Enabled) {
					num++;
				}
			}
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			float num6 = 0f;
			float num7 = 0f;
			float num8 = 0f;
			float num9 = 0f;
			float num10 = 0f;
			float num11 = 0f;
			float num12 = 0f;
			float num13 = 0f;
			float num14 = 0f;
			float num15 = 0f;
			float num16 = 0f;
			float num17 = 0f;
			for (int m = 0; m < __instance.properties.Length; m++) {
				int num18 = m + 100;
				bool flag = false;
				bool flag2 = false;
				if (num18 == 101) {
					flag = (Siva.Device.Input.GetSwitch(SwitchCode.LLampSwitch));
				} else if (num18 == 102) {
					flag = (Siva.Device.Input.GetSwitch(SwitchCode.RLampSwitch));
				} else if (num18 >= 103 && num18 <= 112) {
					flag = false;
				} else if (num18 == 113) {
					/*num2 = UnityEngine.Input.GetAxis("HorizontalLeft1");
					num3 = UnityEngine.Input.GetAxis("VerticalLeft1");
					if (num2 >= __instance.Dead || num2 <= -__instance.Dead || num3 >= __instance.Dead || num3 <= -__instance.Dead) {
						flag2 = true;
					}*/
					if (Siva.Device.Input.GetSwitch(SwitchCode.LLeft)) {
						num14 = -1f;
					} else if (Siva.Device.Input.GetSwitch(SwitchCode.LRight)) {
						num14 = 1f;
					}
					if (Siva.Device.Input.GetSwitch(SwitchCode.LUp)) {
						num15 = -1f;
					} else if (Siva.Device.Input.GetSwitch(SwitchCode.LDown)) {
						num15 = 1f;
					}
					if (num14 >= __instance.Dead || num14 <= -__instance.Dead || num15 >= __instance.Dead || num15 <= -__instance.Dead) {
						flag2 = true;
					}
				} else if (num18 == 114) {
					/*num4 = UnityEngine.Input.GetAxis("HorizontalRight1");
					num5 = UnityEngine.Input.GetAxis("VerticalRight1");
					if (num4 >= __instance.Dead || num4 <= -__instance.Dead || num5 >= __instance.Dead || num5 <= -__instance.Dead) {
						flag2 = true;
					}*/
					if (Siva.Device.Input.GetSwitch(SwitchCode.RLeft)) {
						num16 = -1f;
					} else if (Siva.Device.Input.GetSwitch(SwitchCode.RRight)) {
						num16 = 1f;
					}
					if (Siva.Device.Input.GetSwitch(SwitchCode.RUp)) {
						num17 = -1f;
					} else if (Siva.Device.Input.GetSwitch(SwitchCode.RDown)) {
						num17 = 1f;
					}
					if (num16 >= __instance.Dead || num16 <= -__instance.Dead || num17 >= __instance.Dead || num17 <= -__instance.Dead) {
						flag2 = true;
					}
				}
				__instance.properties[m].Checked = false;
				int index = __instance.GetIndex(num18);
				if (index >= 0) {
					if (flag) {
						__instance.properties[index].Enabled = true;
						__instance.properties[index].TouchDown = false;
						__instance.properties[index].Touch = true;
						__instance.properties[index].Release = false;
						__instance.properties[index].Checked = true;
					} else if (flag2) {
						__instance.properties[index].Enabled = true;
						__instance.properties[index].TouchDown = false;
						__instance.properties[index].Touch = false;
						__instance.properties[index].Release = false;
						__instance.properties[index].Checked = true;
					}
				} else if (flag) {
					__instance.properties[num].Enabled = true;
					__instance.properties[num].TouchDown = true;
					__instance.properties[num].Touch = true;
					__instance.properties[num].Release = false;
					__instance.properties[num].FingerId = num18;
					__instance.properties[num].Checked = true;
					num++;
				} else if (flag2) {
					__instance.properties[num].Enabled = true;
					__instance.properties[num].TouchDown = false;
					__instance.properties[num].Touch = false;
					__instance.properties[num].Release = false;
					__instance.properties[num].FingerId = num18;
					__instance.properties[num].Checked = true;
					num++;
				}
			}
			for (int n = 0; n < __instance.properties.Length; n++) {
				int num19 = n + 100;
				bool joystickType = __instance.joystick1P;
				Vector2 position = default(Vector2);
				if (num19 == 113) {
					if (num2 >= __instance.Dead || num2 <= -__instance.Dead || num3 >= __instance.Dead || num3 <= -__instance.Dead) {
						joystickType = __instance.joystick1P;
						position = new Vector2(num2, num3);
					}
					if (num14 >= __instance.Dead || num14 <= -__instance.Dead || num15 >= __instance.Dead || num15 <= -__instance.Dead) {
						joystickType = __instance.joystick1P;
						position = new Vector2(num14, num15);
					}
				} else if (num19 == 114) {
					if (num4 >= __instance.Dead || num4 <= -__instance.Dead || num5 >= __instance.Dead || num5 <= -__instance.Dead) {
						joystickType = __instance.joystick1P;
						position = new Vector2(num4, num5);
					}
					if (num16 >= __instance.Dead || num16 <= -__instance.Dead || num17 >= __instance.Dead || num17 <= -__instance.Dead) {
						joystickType = __instance.joystick1P;
						position = new Vector2(num16, num17);
					}
				} else if (num19 == 115) {
					if (num6 >= __instance.Dead || num6 <= -__instance.Dead || num7 >= __instance.Dead || num7 <= -__instance.Dead) {
						joystickType = __instance.joystick1P;
						position = new Vector2(num6, num7);
					}
				} else if (num19 == 116) {
					if (num8 >= __instance.Dead || num8 <= -__instance.Dead || num9 >= __instance.Dead || num9 <= -__instance.Dead) {
						joystickType = __instance.joystick2P;
						position = new Vector2(num8, num9);
					}
				} else if (num19 == 117) {
					if (num10 >= __instance.Dead || num10 <= -__instance.Dead || num11 >= __instance.Dead || num11 <= -__instance.Dead) {
						joystickType = __instance.joystick2P;
						position = new Vector2(num10, num11);
					}
				} else if (num19 == 118 && (num12 >= __instance.Dead || num12 <= -__instance.Dead || num13 >= __instance.Dead || num13 <= -__instance.Dead)) {
					joystickType = __instance.joystick2P;
					position = new Vector2(num12, num13);
				}
				int index2 = __instance.GetIndex(num19);
				if (index2 >= 0) {
					__instance.CheckSlideDegree(ref __instance.properties[index2], position, 0.2f, joystickType);
				}
			}
			for (int num20 = 0; num20 < __instance.properties.Length; num20++) {
				if (__instance.properties[num20].Enabled && !__instance.properties[num20].Checked) {
					__instance.properties[num20].Enabled = true;
					__instance.properties[num20].TouchDown = false;
					__instance.properties[num20].Touch = false;
					__instance.properties[num20].Release = true;
				}
			}
			return false;
        }

		private static void CheckSaveDir() {
			if (!Directory.Exists("save")) {
				Directory.CreateDirectory("save");
            }
        }

		[HarmonyPatch(typeof(ApplicationManager), "GetDataPath")]
		[HarmonyPrefix]
		public static bool GetDataPath(string directoryName, ref string __result) {
			CheckSaveDir();
			__result = "./save/" + directoryName + "/";
			return false;
		}

		[HarmonyPatch(typeof(RemainTimeController), "IsInfinite", MethodType.Getter)]
		[HarmonyPrefix]
		public static bool IsInfinite(ref bool __result, RemainTimeController __instance) {
			if (CoreFixesBehaviour.TimeFreeze.Value && __instance.remainTime > 5) {
				__result = true;
				return false;
            } else {
				return true;
            }
        }

		public static bool CheckDeliveryTime(ref bool __result, string id, DateTime dateTime) {
			if (CoreFixesBehaviour.AllCollecaAvailable.Value) {
				__result = true;
				return false;
            } else {
				return true;
            }
        }
	}
}
