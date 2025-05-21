using BepInEx.Configuration;
using GlobalBullet;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;



namespace LCBaseModForBepinEx
{
    public class BMConfigManager
    {
        private static BMConfigManager __instance;
        private static string cfgfilename = "com.Sutirita.LobotomyBaseMod.cfg";
        private static string cfgfilepath = Path.Combine(BepInEx.Paths.ConfigPath, cfgfilename);
        private ConfigFile _BMConfigFile = new ConfigFile(cfgfilepath, true);


        private string __lang = "cn";

        private Dictionary<string, Dictionary<string, string>> _ConfigDesc = 
            
            new Dictionary<string, Dictionary<string, string>> 
            {
            
                {"cn", new Dictionary<string, string>()},

                {"en", new Dictionary<string, string>()}

            };



        public ConfigEntry<bool> cfg_EnableBaseMod;


        public ConfigEntry<bool> cfg_AutoSaveBackUp;


        public ConfigEntry<bool> cfg_AllowCreatureOverwrite;


        //控制台
        public ConfigEntry<bool> cfg_RewriteConsole;
        public ConfigEntry<bool> cfg_Console_Enable;


        //数值精度显示
        public ConfigEntry<int> cfg_Precision_Dmg;
        public ConfigEntry<int> cfg_Precision_WorkSuccess;
        public ConfigEntry<int> cfg_Precision_Defense;
        public ConfigEntry<int> cfg_Precision_CreatureHP;
        public ConfigEntry<int> cfg_Precision_AgentHPMP;


        public static BMConfigManager instance
        {
            get
            {
                if (__instance == null)
                {
                    __instance = new BMConfigManager();
                }
                return __instance;
            }
        }



        // 尝试从配置文件中读取配置项
        BMConfigManager()
        {

            LoadConfigDesc();


            Init();




        }


        private void LoadConfigDesc()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            // 获取嵌入资源的名称
            string resourceName = "LobotomyBaseModForBepinEx.LCBMConfigDesc.xml";

            Stream stream = assembly.GetManifestResourceStream(resourceName);

            XmlDocument ConfigxmlDocument = new XmlDocument();

            ConfigxmlDocument.Load(stream);
            foreach(XmlNode cfgnode in ConfigxmlDocument.SelectNodes("ConfigDesc/Config"))
            {
                string cfgname = cfgnode.Attributes.GetNamedItem("name").InnerText;
                foreach (XmlNode DescNode in cfgnode)
                {
                    string lang = DescNode.Attributes.GetNamedItem("lang").InnerText;
                    string desc = DescNode.InnerText;
                    _ConfigDesc[lang].Add(cfgname, desc);
                   // LCBaseMod.Instance.MakeMessageLog($"Added Desc {cfgname}：{desc} （{lang}）");
                }
            }

        }

        private string GetConfigDesc(string cfgname)
        {
            if (_ConfigDesc[__lang].TryGetValue(cfgname ,out string _r))
            {
                return _r;
            }
            else
            {
                return "";
            }


        }


        private void Init()
        {



            LoadConfig<bool>("LCBaseMod", "EnableBaseMod", out cfg_EnableBaseMod, true, GetConfigDesc("EnableBaseMod"));

            LoadConfig<bool>("LCBaseMod", "AllowCreatureOverwrite", out cfg_AllowCreatureOverwrite, true,GetConfigDesc("AllowCreatureOverwrite"));

            LoadConfig<bool>("LCBaseMod", "AutoSaveBackUp", out cfg_AutoSaveBackUp, true,GetConfigDesc("AutoSaveBackUp"));

            LoadConfig<bool>("LCBaseMod", "BaseModConsole", out cfg_RewriteConsole, true, GetConfigDesc("BaseModConsole"));




            LoadConfig<int>("GamePlay", "DmgPrecision", out cfg_Precision_Dmg, 2,GetConfigDesc("DmgPrecision"));

            LoadConfig<int>("GamePlay", "AgentSoltPrecision", out cfg_Precision_AgentHPMP, 2,GetConfigDesc("AgentSoltPrecision"));

            LoadConfig<int>("GamePlay", "CreatureHpPrecision", out cfg_Precision_CreatureHP, 2, GetConfigDesc("CreatureHpPrecision"));

            LoadConfig<int>("GamePlay", "DefensePrecision", out cfg_Precision_Defense, 3, GetConfigDesc("DefensePrecision"));

            LoadConfig<int>("GamePlay", "WorkSuccessPrecision", out cfg_Precision_WorkSuccess, 2,GetConfigDesc("WorkSuccessPrecision"));





        }

        private void LoadConfig<T>(string section, string key, out ConfigEntry<T> ConfigEntry, T DefaultValue = default, string Desc = "")
        {
            T _Value = DefaultValue;
            if (this._BMConfigFile.TryGetEntry<T>(section, key, out ConfigEntry<T> entry))
            {
                _Value = entry.Value;
            }
            ConfigEntry = this._BMConfigFile.Bind<T>(section, key, _Value, Desc);
        }


        public void Save()
        {
            this._BMConfigFile.Save();
        }
    }




}
