using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Text;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using Unity.Netcode;
using System.Reflection;
using System.Numerics;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Harmony;
using BepInEx;
using System.Collections;
using BepInEx.Logging;
namespace An0n_Patches.Patches
{

    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerPatch
    {

        //Patch out jump delay
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(PlayerControllerB), "PlayerJump", MethodType.Enumerator)]
        public static IEnumerable<CodeInstruction> RemoveJumpDelay(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            if (!An0n_Patch_Plugin.instantJump.Value)
            {
                return list;
            }
            int j = 0;
            for (int i = 0; i < list.Count; i++)
            {
                CodeInstruction codeInstruction = list[i];
                
                if (!(codeInstruction.opcode != OpCodes.Newobj))
                {
                    ConstructorInfo constructorInfo = codeInstruction.operand as ConstructorInfo;
                    if (((constructorInfo != null) ? constructorInfo.DeclaringType : null) == typeof(WaitForSeconds))
                    {
                        Debug.Log("[An0nPatch] patched Instant-Jump");
                        list[i] = new CodeInstruction(OpCodes.Ldnull, null);
                        Debug.Log(j.ToString()+"-REM:"+list[i].ToString());
                        Debug.Log((j-1).ToString() + "REM:" + list[i-1].ToString());
                        list.RemoveAt(i - 1);
                        i--;
                    }
                }
                j++;
            }
            return list;
        }

        //Patch running slipperiness
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        public static IEnumerable<CodeInstruction> fixFloatyTurnAndRun(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
            for (int i = 0; i < list.Count - 3; i++)
            {
                CodeInstruction codeInstruction = list[i];

                    if (codeInstruction.opcode == OpCodes.Ldc_R4 && (float)codeInstruction.operand == 5f)
                    {

                        if (list[i + 2].LoadsField(typeof(PlayerControllerB).GetField("carryWeight")))
                        {

                            if (list[i + 3].opcode == OpCodes.Ldc_R4 && (float)list[i + 3].operand == 1.5f)
                            {
                                Debug.Log("[An0nPatch] patched slipperiness");
                                list[i] = new CodeInstruction(OpCodes.Ldc_R4, An0n_Patch_Plugin.slipperiness.Value);
                            }
                        }
                    }

            }

            //instant sprint
            for (int i = 1; i < list.Count - 2; i++)
            {
                CodeInstruction codeInstruction = list[i];

                if (codeInstruction.opcode == OpCodes.Ldc_R4 && (float)codeInstruction.operand == 2.25f)
                {
                    if (list[i - 1].opcode == OpCodes.Ldfld)
                    {
                        if (list[i-1].ToString() == "ldfld float GameNetcodeStuff.PlayerControllerB::sprintMultiplier")
                        {
                            if (list[i + 2].opcode == OpCodes.Ldc_R4 && (float)list[i + 2].operand == 1f)
                            {
                                Debug.Log("[An0nPatch] patched instant sprint");
                                list[i + 2] = new CodeInstruction(OpCodes.Ldc_R4, An0n_Patch_Plugin.instantSprint.Value);
                            }
                        }
                    }
                }

            }
            return list;
        }

        private static PlayerControllerB __mainPlayer;
        

        public static bool canJump(RaycastHit ___hit)
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            return !Physics.Raycast(player.gameplayCamera.transform.position, UnityEngine.Vector3.up, out ___hit, 0.72f, player.playersManager.collidersAndRoomMask, QueryTriggerInteraction.Ignore);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        public static void ReadInput(PlayerControllerB __instance, RaycastHit ___hit)
        {
            try
            {
                if (PlayerControllerPatch.stillInitializing) { return; }
                if ((object)__mainPlayer == (object)null)
                {
                    __mainPlayer = StartOfRound.Instance.localPlayerController;
                }
                PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
                if ((!__instance.IsOwner || !__instance.isPlayerControlled || (__instance.IsServer && !__instance.isHostPlayerObject)) && !__instance.isTestingPlayer && !__mainPlayer.inTerminalMenu && !__mainPlayer.isTypingChat && !__mainPlayer.isPlayerDead && !__mainPlayer.quickMenuManager.isMenuOpen)
                {
                    /*if (Keyboard.current.leftCtrlKey.wasReleasedThisFrame && !up && canJump(___hit) && !An0n_Patch_Plugin.crouchToggle.Value)
                    {
                        Debug.Log("ctrl Key released");
                        up = true;
                        GameNetworkManager.Instance.localPlayerController.Crouch(false);
                    }*/
                    if (!IngamePlayerSettings.Instance.playerInput.actions.FindAction("Crouch", true).IsPressed() && !Physics.Raycast(player.gameplayCamera.transform.position, UnityEngine.Vector3.up, out ___hit, 0.72f, player.playersManager.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
                    {
                        player.isCrouching = false;
                        player.playerBodyAnimator.SetBool("crouching", false);
                    }
                }

            }
            catch { }
        }


        public static bool tempcrouch = false;
        [HarmonyPatch(typeof(PlayerControllerB), "Jump_performed")]
        [HarmonyPrefix()]
        public static void Jump_performedA(PlayerControllerB __instance, RaycastHit ___hit)
        {
            if (__instance.isCrouching == true && canJump(___hit))
            {
                __instance.isCrouching = false;
                tempcrouch = true;
            }
        }
        [HarmonyPatch(typeof(PlayerControllerB), "Jump_performed")]
        [HarmonyPostfix()]
        public static void Jump_performedB(PlayerControllerB __instance)
        {
            if (tempcrouch)
            {
                __instance.isCrouching = true;
                tempcrouch = false;
            }
        }
        [HarmonyPatch(typeof(PlayerControllerB), "Crouch_performed")]
        [HarmonyPrefix()]
        public static void Crouch_performed()
        {

        }

        [HarmonyPatch(typeof(PlayerControllerB), "PlayerHitGroundEffects")]
        [HarmonyPostfix()]
        public static void PlayerHitGroundEffects()
        {
            if (GameNetworkManager.Instance.localPlayerController.isCrouching)
            {
                GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetTrigger("startCrouching");
                GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetBool("crouching", true);
            }
        }

        private static bool stillInitializing = true;
        [HarmonyPatch(typeof(GameNetworkManager), "Disconnect")]
        [HarmonyPostfix()]
        public static void Uninitialize()
        {
            stillInitializing = true;
        }

        [HarmonyPatch(typeof(StartOfRound), "SceneManager_OnLoadComplete1")]
        [HarmonyPostfix()]
        public static void Initialize()
        {
            stillInitializing = false;
        }


        

       
    }
}
