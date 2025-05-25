using BepInEx;
using HarmonyLib;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;

namespace LCBaseMod
{
    [BepInPlugin("com.Sutirita.LobotomyCropBaseMod", "LCBaseMod", "0.1.5")]
    [BepInProcess("LobotomyCorp.exe")]
    public class LCBaseMod : BaseUnityPlugin
    {
        private string _PluginVer = "0.1.5";

        private static LCBaseMod _instance;

        //基本文件夹
        private static string _PluginDirPath= Path.Combine(BepInEx.Paths.PluginPath, "Sutirita-LCBaseMod");
        private static string _SaveBackUpDirPath=Path.Combine(_PluginDirPath, "backup");
     
        private static string _ModDirPath= Path.Combine(_PluginDirPath, "Mods");




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
            if (!BMConfigManager.instance.cfg_EnableBaseMod.Value)
            {
                MakeMessageLog("LCBaseMod  is currently disabled..");
                return;
            }
            BMInit();
            //备份存档文件
            if (BMConfigManager.instance.cfg_AutoSaveBackUp.Value)
            {
                try
                {
                    BackUPSaveData();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString() + "\n" + ex.StackTrace);
                }

            }





            // 获取当前程序集
            /*
             
     
             
             */



        }

        // 在所有插件全部启动完成后会调用Start()方法，执行顺序在Awake()后面；
        void Start()
        {



        }

        // 插件启动后会一直循环执行Update()方法，可用于监听事件或判断键盘按键，执行顺序在Start()后面
        void Update()
        {


        }

        // 在插件关闭时会调用OnDestroy()方法

        void OnDestroy()
        {
            BMConfigManager.instance.Save();

            Debug.Log("OnDestroyed");
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

        public static void GenerateLogFile()
        {

        }


        private void BMInit()
        {



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
                    Logger.LogWarning(string.Format("Directory:{0} No Found", _ModDirPath));
                    System.IO.Directory.CreateDirectory(_ModDirPath);
                }
                if (!System.IO.Directory.Exists(_SaveBackUpDirPath))
                {
                    Logger.LogWarning(string.Format("Directory:{0} No Found", _SaveBackUpDirPath));
                    System.IO.Directory.CreateDirectory(_SaveBackUpDirPath);
                }
            }
            //加载Mod文件夹
            if (System.IO.Directory.Exists(GetModDirPath()))
            {
                Logger.LogMessage("Loading Mods...");
                ExtensionManager.Instance.Init();

            }
           
            //开始Patch
            Harmony.CreateAndPatchAll(typeof(BMHP_Basic));
            Harmony.CreateAndPatchAll(typeof(BMHP_GamePlay));
            Harmony.CreateAndPatchAll(typeof(BMHP_Equipment));
            Harmony.CreateAndPatchAll(typeof(BMHP_Creature));
            Harmony.CreateAndPatchAll(typeof(BMHP_CreaturePortrait));
            Harmony.CreateAndPatchAll(typeof(BMHP_ConsoleScript));

            ExtensionManager.Instance.CreatePatch();

         

        }








        public string GetModDirPath()
        {
            return _ModDirPath;
        }


        //存档备份函数

        public void BackUPSaveData()
        {
            MakeMessageLog("SaveBackUp Start...");
            // 源文件夹路径
            string sourceFolderPath = Application.persistentDataPath;

            // 获取当前时间并格式化为文件夹名称
            string currentTimeFolderName ="SaveBackup_"+ DateTime.Now.ToString("yyyy-MM-dd_HHmmss");

            // 创建目标文件夹路径
            string BKFolderPath = Path.Combine(_SaveBackUpDirPath, currentTimeFolderName);

            // 如果目标文件夹不存在，则创建它
            if (!Directory.Exists(BKFolderPath))
            {
                Directory.CreateDirectory(BKFolderPath);
            }

            // 获取源文件夹中的所有文件
            string[] files = Directory.GetFiles(sourceFolderPath, "*.*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                // 获取目标文件的完整路径
                string targetFilePath = file.Replace(sourceFolderPath, BKFolderPath);

                // 确保目标文件夹存在
                string targetDirectory = Path.GetDirectoryName(targetFilePath);
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                // 复制文件
                File.Copy(file, targetFilePath, true); // true表示如果目标文件已存在，则覆盖它
            }
            // 获取源文件夹中的所有子文件夹（包括空文件夹）
            string[] subDirectories = Directory.GetDirectories(sourceFolderPath, "*.*", SearchOption.AllDirectories);

            foreach (string subDirectory in subDirectories)
            {
                // 获取目标子文件夹的完整路径
                string targetSubDirectory = subDirectory.Replace(sourceFolderPath, BKFolderPath);

                // 确保目标子文件夹存在
                if (!Directory.Exists(targetSubDirectory))
                {
                    Directory.CreateDirectory(targetSubDirectory);
                }
            }




            MakeMessageLog(string.Format("SaveData has been Copied to : \n{0}", BKFolderPath));
        }

    }


}

