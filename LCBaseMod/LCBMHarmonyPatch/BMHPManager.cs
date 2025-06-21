using HarmonyLib;
using LCBaseMod.LCBMConfig;
using LCBaseMod.LCBMConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCBaseMod.LCBMHarmonyPatch
{
    class BMHPManager
    {
       private static  BMHPManager _instance;
        public static BMHPManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BMHPManager();
                }

                return _instance;
            }
        }

        public void Init()
        {
            StartPatch();
        }

        private void StartPatch()
        {
            Harmony.CreateAndPatchAll(typeof(HP_Basic));
            if (BMConfigManager.Instance.EnableLCBMConsole.Value)
            {
                Harmony.CreateAndPatchAll(typeof(HP_ConsoleScript));
            }

            Harmony.CreateAndPatchAll(typeof(HP_Creature));
            Harmony.CreateAndPatchAll(typeof(HP_Creature_Portrait));
            Harmony.CreateAndPatchAll(typeof(HP_GamePlay));
            Harmony.CreateAndPatchAll(typeof(HP_InfoDetails));

        }













    }
}
