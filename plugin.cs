using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using An0n_Patches.Patches;

namespace An0n_Patches
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    public class An0n_Patch_Plugin : BaseUnityPlugin
    {
        private const string pluginGUID = "com.an0n.patch";
        private const string pluginName = "An0n Patch";
        private const string pluginVersion = "1.0.4";
        public static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(pluginGUID);
        private Harmony harmony = new Harmony(pluginGUID);
        public static ConfigEntry<float> instantSprint;
        public static ConfigEntry<float> slipperiness;
        public static ConfigEntry<bool> instantJump;
        public static ConfigEntry<bool> showHPSP;
        private void Awake()
        {
            instantJump = Config.Bind("General",
                            "instantJump",
                            true,
                            "Enable/disable instant jump. Removes the delay with jumping when enabled.");
            instantSprint = Config.Bind("General",    
                             "instantSprint",  
                             2.25f, 
                             "How fast to accelerate to sprint value of 2.25. 2.25 is the max, so it's instant acceleration.");
            slipperiness = Config.Bind("General",
                             "slipperiness", 
                             10f,
                             "The amount of slipperiness when running and changing direction. 10-15f is a good value for little to no slippery feeling.");
            showHPSP = Config.Bind("General",
                             "showHealthStamina",
                             true,
                             "Show your health and sprint/stamina % on the HUD.");
            mls.LogInfo("[An0nPatch] Plugin Loaded");
            this.patcher = new Harmony(pluginGUID);
            this.patcher.PatchAll(typeof(PlayerControllerPatch));
            this.patcher.PatchAll(typeof(HUDManagerPatch));
        }
        private Harmony patcher;
    }
}