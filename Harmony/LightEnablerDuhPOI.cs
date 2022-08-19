using System.Reflection;
using HarmonyLib;
using UnityEngine;

public class OcbPrettyGrass : IModApi
{

    public void InitMod(Mod mod)
    {
        Debug.Log("Loading OCB POI Light Enabler Patch Duh: " + GetType().ToString());
        new Harmony(GetType().ToString()).PatchAll(mod.MainAssembly);
    }

    [HarmonyPatch(typeof(BlockLight))]
    [HarmonyPatch("GetBlockActivationCommands")]
    public class BlockLight_GetBlockActivationCommands
    {
        static bool Prefix(
            WorldBase _world,
            BlockActivationCommand[] ___cmds,
            ref BlockActivationCommand[] __result)
        {
            // Use regular code for editor
            if (_world.IsEditor()) return true;
            ___cmds[0].enabled = true;
            __result = ___cmds;
            return false;
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
            int _indexInBlockActivationCommands,
            WorldBase _world,
            int _cIdx,
            Vector3i _blockPos,
            BlockValue _blockValue,
            ref bool __result)
        {
            if (_indexInBlockActivationCommands == 0)
            {
                FnUpdateLightState.Invoke(__instance, new object[] { _world, _cIdx, _blockPos, _blockValue, true, false });
                __result = true;
                return false;
            }
            return true;
        }
    }

}
