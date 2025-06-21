using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using UnityEngine;


using LCBaseMod.LCBMConfig;
using LCBaseMod.Extensions;
using LCBaseMod.LCBMHarmonyPatch;
using LCBaseMod.LCBMToolKit;

namespace LCBaseMod
{
    [BepInPlugin("com.Sutirita.LobotomyCropBaseMod", "LCBaseMod", "0.2.1")]
    [BepInProcess("LobotomyCorp.exe")]
    public class LCBaseMod : BaseUnityPlugin
    {
        private string _PluginVer = "0.2.1";

        private static LCBaseMod _instance;

        //基本文件夹
        private static readonly string _PluginDirPath= Path.Combine(BepInEx.Paths.PluginPath, "Sutirita-LCBaseMod");


        private static readonly string _SaveBackUpDirPath=Path.Combine(_PluginDirPath, "backup");
     
        private static readonly string _ModDirPath= Path.Combine(_PluginDirPath, "Mods");


        public static LCBaseMod Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LCBaseMod();
                }

                return _instance;
            }
        }


        // 在插件启动时会直接调用Awake()方法
        void Awake()
        {

            if (!BMConfigManager.Instance.EnableBaseMod.Value)
            {
                MakeMessageLog("LCBaseMod  is currently disabled..");
                return;
            }
            LCBMInit();


            //备份存档文件

            if (BMConfigManager.Instance.EnableAutoSaveBackUp.Value)
            {
                try
                {
                    ToolKit.BackUPSaveData(_SaveBackUpDirPath);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString() + "\n" + ex.StackTrace);
                }

            }

        }

        // 在所有插件全部启动完成后会调用Start()方法，执行顺序在Awake()后面；
        void Start()
        {

        }

        // 插件启动后会一直循环执行Update()方法，可用于监听事件或判断键盘按键，执行顺序在Start()后面
        void Update()
        {


        }

        // 在插件关闭时

        void OnDestroy()
        {
            BMConfigManager.Instance.Save();
        }


        public string GetModDirPath()
        {
            return _ModDirPath;
        }

        public string GetSaveBackUpDirPath()
        {
            return _SaveBackUpDirPath;
        }



        public string GetVertionStr()
        {
            return $"\n{Instance.Info.Metadata.Name} {Instance._PluginVer}ver\nMade by Sutirita.";
        }


        public void MakeMessageLog(string message)
        {
            Logger.LogMessage(message);
        }


        public void MakeInfoLog(string message)
        {
            Logger.LogInfo(message);
        }

        public void MakeErrorLog(string message)
        {
            Logger.LogError(message);

        }
        public void MakeWarningLog(string message)
        {
            Logger.LogWarning(message);

        }


        private void LCBMInit()
        {
            MakeMessageLog($"LCBaseMod {_PluginVer} initializing...");
            //检查存在
            if (!System.IO.Directory.Exists(_PluginDirPath))
            {
                System.IO.Directory.CreateDirectory(_PluginDirPath);
                System.IO.Directory.CreateDirectory(_ModDirPath);
                System.IO.Directory.CreateDirectory(_SaveBackUpDirPath);
            }
            else
            {
                if (!System.IO.Directory.Exists(_ModDirPath))
                {
                    MakeWarningLog(string.Format("Directory:{0} No Found", _ModDirPath));
                    System.IO.Directory.CreateDirectory(_ModDirPath);
                }
                if (!System.IO.Directory.Exists(_SaveBackUpDirPath))
                {
                    MakeWarningLog(string.Format("Directory:{0} No Found", _SaveBackUpDirPath));
                    System.IO.Directory.CreateDirectory(_SaveBackUpDirPath);
                }
            }

            //加载Mod文件夹
            if (System.IO.Directory.Exists(GetModDirPath()))
            {
                MakeMessageLog("Loading Mods...");
                ExtensionManager.Instance.Init();

            }

            //开始Patch
            BMHPManager.Instance.Init();

            ExtensionManager.Instance.CreatePatch();

         

        }






    }


}

