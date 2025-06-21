using HarmonyLib;
using LCBaseMod.LCBMConfig;
using LCBaseMod.LCBMConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace LCBaseMod.LCBMHarmonyPatch
{
     class HP_ConsoleScript
    {
        //重写控制台逻辑

        [HarmonyPrefix, HarmonyPatch(typeof(ConsoleScript), "OnExitEdit")]
        public static bool HP_OnExitEdit(string command, ConsoleScript __instance)
        {
            ConsoleScript.instance.ConsoleWnd.gameObject.SetActive(value: false);
            char[] separator = new char[] { ' ', '.' };
            if (BMConfigManager.Instance.EnableLCBMConsole.Value)
            {

                string[] array = command.Split(separator);
                LCBaseMod.Instance.MakeMessageLog($"Execute command:{array[0]}");
                try
                {

                    BMConsoleManager.instance.ExecuteCommand(array);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                return false;
            }
            else
            {
                return true;
            }

        }

    }
}
