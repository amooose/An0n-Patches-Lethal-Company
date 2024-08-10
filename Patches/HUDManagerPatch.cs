using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using BepInEx;
using JetBrains.Annotations;
using GameNetcodeStuff;
using TMPro;
using BepInEx.Configuration;
using System.Reflection.Emit;
using BepInEx.Logging;


namespace An0n_Patches.Patches
{
    [HarmonyPatch]
    internal class HUDManagerPatch : MonoBehaviour
    {
        public static TMPro.TextMeshProUGUI HPSP_HUDText;
        public static bool instantiating = true;

        [HarmonyPatch(typeof(StartOfRound), "SceneManager_OnLoadComplete1")]
        [HarmonyPostfix()]
        public static void CreateHPSP_HUD()
        {
            if (instantiating)
            {
                GameObject wUIa = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/WeightUI");
                GameObject topLefta = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner");
                GameObject hud2 = UnityEngine.Object.Instantiate(wUIa, topLefta.transform);
                hud2.name = "HPSP";
                GameObject wg = hud2.transform.GetChild(0).gameObject;
                RectTransform ra2 = wg.GetComponent<RectTransform>();
                ra2.anchoredPosition = new Vector2(-45.0f, 10f);
                HPSP_HUDText = wg.GetComponent<TMPro.TextMeshProUGUI>();
                HPSP_HUDText.faceColor = new Color(255, 0, 0, 255);
                HPSP_HUDText.fontSize = 12f;
                HPSP_HUDText.margin = new Vector4(0, -36, 100, 0);
                HPSP_HUDText.alignment = TMPro.TextAlignmentOptions.TopRight;
                HPSP_HUDText.text = string.Format("{0}\n\n\n{1}%", 100, 100);
                instantiating = false;

            }
        }



            [HarmonyPatch(typeof(GameNetworkManager), "Disconnect")]
        [HarmonyPrefix()]
        public static void unInstantiate()
        {
            instantiating = true;
        }

        //Add hook for quitting to main menu to set instantiating to true, as pingElements and pingContainer are destroyed on exit to main menu

        [HarmonyPatch(typeof(HUDManager), "Update")]
        [HarmonyPostfix()]
        public static void Update()
        {
            if(GameNetworkManager.Instance.localPlayerController == null)
            {
                return;
            }
            if (!instantiating)
            {
                float hptxt = Mathf.RoundToInt(GameNetworkManager.Instance.localPlayerController.health);

                float sptxt = Mathf.RoundToInt((((GameNetworkManager.Instance.localPlayerController.sprintMeter * 100) - 10) / 90) * 100);
                if (sptxt < 0)
                {
                    sptxt = 0;
                }
                if (An0n_Patch_Plugin.showHPSP.Value)
                {
                    HPSP_HUDText.text = string.Format("{0}\n\n\n\n{1}%", hptxt, sptxt);
                }
                
            }
        }
    }
}
