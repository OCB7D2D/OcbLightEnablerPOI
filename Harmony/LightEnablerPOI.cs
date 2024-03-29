﻿using System.Reflection;
using HarmonyLib;

public class OcbPrettyGrass : IModApi
{

    public void InitMod(Mod mod)
    {
        Log.Out("OCB Harmony Patch: " + GetType().ToString());
        Harmony harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    [HarmonyPatch(typeof(BlockLight))]
    [HarmonyPatch("GetBlockActivationCommands")]
    public class BlockLight_GetBlockActivationCommands
    {
        static void Postfix(
            WorldBase _world,
            // BlockActivationCommand[] ___cmds,
            ref BlockActivationCommand[] __result)
        {
            // Use regular code for editor
            if (_world.IsEditor()) return;
            if (__result.Length == 0) return;
            __result[0].enabled = true;
        }
    }

    [HarmonyPatch(typeof(BlockLight))]
    [HarmonyPatch("GetActivationText")]
    public class BlockLight_GetActivationText
    {
        static bool Prefix(
            WorldBase _world,
            BlockValue _blockValue,
            EntityAlive _entityFocusing,
            BlockActivationCommand[] ___cmds,
            ref string __result)
        {
            // Use regular code for editor
            if (_world.IsEditor()) return true;
            PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
            string str = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
            __result = (_blockValue.meta & 2) != 0 ? string.Format(Localization.Get("useSwitchLightOff"), str) : string.Format(Localization.Get("useSwitchLightOn"), str);
            return false;
        }
    }

    [HarmonyPatch(typeof(BlockLight))]
    [HarmonyPatch("OnBlockActivated")]
    public class BlockLight_OnBlockActivated
    {
        static MethodInfo FnUpdateLightState = AccessTools.Method(typeof(BlockLight), "updateLightState");

        static bool Prefix(
            BlockLight __instance,
            string _commandName,
            WorldBase _world,
            int _cIdx,
            Vector3i _blockPos,
            BlockValue _blockValue,
            ref bool __result)
        {
            if (_commandName == "light")
            {
                FnUpdateLightState.Invoke(__instance, new object[] { _world, _cIdx, _blockPos, _blockValue, true, false });
                __result = true;
                return false;
            }
            return true;
        }
    }

}
