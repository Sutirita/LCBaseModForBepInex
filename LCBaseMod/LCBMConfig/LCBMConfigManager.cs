using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;



namespace LCBaseMod.LCBMConfig
{
    public class BMConfigManager
    {
        private static BMConfigManager __instance;

        private string ConfigDescRes = "LCBaseMod.LCBMConfig.LCBMConfigDesc.xml";


        private static readonly string ConfigFilePath = Path.Combine(BepInEx.Paths.ConfigPath, "com.Sutirita.LobotomyBaseMod.cfg");

        private ConfigFile _BMConfigFile = new ConfigFile(ConfigFilePath, true);


        private string __lang = "cn";

        private Dictionary<string, Dictionary<string, string>> _ConfigDesc = 
            
            new Dictionary<string, Dictionary<string, string>> 
            {
            
                {"cn", new Dictionary<string, string>()},

                {"en", new Dictionary<string, string>()},

                {"jp", new Dictionary<string, string>()},

                {"ru", new Dictionary<string, string>()},

                {"kr", new Dictionary<string, string>()}

            };


        //启用BM
        public ConfigEntry<bool> EnableBaseMod;

        //自动备份存档
        public ConfigEntry<bool> EnableAutoSaveBackUp;


        //BM控制台
        public ConfigEntry<bool> EnableLCBMConsole;

       
        //-----------------设置---------------

        public ConfigEntry<bool> AllowCreatureOverwrite;



        //数值精度显示
        public ConfigEntry<int> Precision_Dmg;
        public ConfigEntry<int> Precision_WorkSuccess;
        public ConfigEntry<int> Precision_Defense;
        public ConfigEntry<int> Precision_CreatureHP;
        public ConfigEntry<int> Precision_AgentHPandMP;

        //------------------------------------
        //BUG修复

        public ConfigEntry<bool> ChesedReserachFix;

        public ConfigEntry<bool> GeburahThirdPhaseFix;

        public ConfigEntry<bool> BigBirdOverloadFix;












        //-------------------------------------





        public static BMConfigManager Instance
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



        //加载设置
        //section 和 key 为 .cfg 文件内配置 
        private void LoadConfig<T>(string section, string key, out ConfigEntry<T> ConfigEntry, T DefaultValue = default, string Desc = "")
        {
            T _Value = DefaultValue;
            if (this._BMConfigFile.TryGetEntry<T>(section, key, out ConfigEntry<T> entry))
            {
                _Value = entry.Value;
            }
            ConfigEntry = this._BMConfigFile.Bind<T>(section, key, _Value, Desc);
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



        private void LoadConfigDesc()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            try
            {
                Stream stream = assembly.GetManifestResourceStream(ConfigDescRes);
                XmlDocument ConfigxmlDocument = new XmlDocument();
                ConfigxmlDocument.Load(stream);
                foreach (XmlNode cfgnode in ConfigxmlDocument.SelectNodes("ConfigDesc/Config"))
                {
                    string cfgname = cfgnode.Attributes.GetNamedItem("name").InnerText;
                    foreach (XmlNode DescNode in cfgnode)
                    {
                        string lang = DescNode.Attributes.GetNamedItem("lang").InnerText;
                        string desc = DescNode.InnerText;
                        if (_ConfigDesc.ContainsKey(lang))
                        {
                            _ConfigDesc[lang].Add(cfgname, desc);
                        }


                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

        }



        public void Save()
        {
            this._BMConfigFile.Save();
        }


        private void Init()
        {

            LoadConfig<bool>("LCBaseMod", "EnableBaseMod", out EnableBaseMod, true, GetConfigDesc("EnableBaseMod"));

            LoadConfig<bool>("LCBaseMod", "AllowCreatureOverwrite", out AllowCreatureOverwrite, true, GetConfigDesc("AllowCreatureOverwrite"));

            LoadConfig<bool>("LCBaseMod", "AutoSaveBackUp", out EnableAutoSaveBackUp, true, GetConfigDesc("AutoSaveBackUp"));

            LoadConfig<bool>("LCBaseMod", "BaseModConsole", out EnableLCBMConsole, true, GetConfigDesc("BaseModConsole"));

            LoadConfig<int>("MoreDetails", "DmgPrecision", out Precision_Dmg, 2, GetConfigDesc("DmgPrecision"));

            LoadConfig<int>("MoreDetails", "WorkSuccess", out Precision_WorkSuccess, 2, GetConfigDesc("WorkSuccessPrecision"));

            LoadConfig<int>("MoreDetails", "AgentSoltPrecision", out Precision_AgentHPandMP, 2, GetConfigDesc("AgentSoltPrecision"));

            LoadConfig<int>("MoreDetails", "CreatureHpPrecision", out Precision_CreatureHP, 2, GetConfigDesc("CreatureHpPrecision"));

            LoadConfig<int>("MoreDetails", "DefensePrecision", out Precision_Defense, 3, GetConfigDesc("DefensePrecision"));

            LoadConfig<bool>("BugFix", "ChesedReserachFix", out ChesedReserachFix, true,"");

            LoadConfig<bool>("BugFix", "GeburahThirdPhaseFix", out GeburahThirdPhaseFix, true, "");


        }















    }




}
