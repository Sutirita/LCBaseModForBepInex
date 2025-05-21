
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;
using UnityEngine.Serialization;
using Spine;
using Spine.Unity;



namespace LCBaseModForBepinEx{
    
    public  class LCBM_Tools_CDL
    {
        public static bool  GetBooleanData(string b)
        {
            string text = b.ToLower();
            if (b != null)
            {
                if (b == "true")
                {
                    return true;
                }
                if (b == "false")
                {
                    return false;
                }
            }
            return false;
        }

        public static DamageInfo ConvertToDamageInfo(XmlNode damageNode)
        {
            RwbpType type = ConvertToRWBP(damageNode.Attributes.GetNamedItem("type").InnerText);
            int min = int.Parse(damageNode.Attributes.GetNamedItem("min").InnerText);
            int max = int.Parse(damageNode.Attributes.GetNamedItem("max").InnerText);
            return new DamageInfo(type, min, max);
        }
        public static RwbpType ConvertToRWBP(string text)
        {
            if (text != null)
            {
                if (text == "R")
                {
                    return RwbpType.R;
                }
                if (text == "W")
                {
                    return RwbpType.W;
                }
                if (text == "B")
                {
                    return RwbpType.B;
                }
                if (text == "P")
                {
                    return RwbpType.P;
                }
            }
            return RwbpType.N;
        }

        private static string LoadCollectionStringItem(XmlNode node, ref int level)
        {
            XmlNode namedItem = node.Attributes.GetNamedItem("openLevel");
            if (namedItem == null)
            {
                UnityEngine.Debug.Log("openLevel not found : " + node.Name);
                level = 0;
                return node.InnerText;
            }
            level = (int)float.Parse(namedItem.InnerText);
            return node.InnerText;
        }
        private static int LoadCollectionIntegerItem(XmlNode node, ref int level)
        {
            XmlNode namedItem = node.Attributes.GetNamedItem("openLevel");
            if (namedItem == null)
            {
                 UnityEngine.Debug.LogError("openLevel not found : " + node.Name);
                return 0;
            }
            level = (int)float.Parse(namedItem.InnerText);
            return (int)float.Parse(node.InnerText);
        }

        private static CreatureTypeInfo.CreatureDataList GetCreatureDataList(XmlNodeList nodes, string itemName, bool isInt)
        {
            CreatureTypeInfo.CreatureDataList creatureDataList = new CreatureTypeInfo.CreatureDataList();
            creatureDataList.itemName = itemName;
            IEnumerator enumerator = nodes.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object obj = enumerator.Current;
                    XmlNode node = (XmlNode)obj;
                    CreatureTypeInfo.CreatureData creatureData = new CreatureTypeInfo.CreatureData();
                    int openLevel = -1;
                    if (isInt)
                    {
                        int num =LoadCollectionIntegerItem(node, ref openLevel);
                        creatureData.data = num;
                    }
                    else
                    {
                        string text = LoadCollectionStringItem(node, ref openLevel);
                        string data = text.Trim();
                        creatureData.data = data;
                    }
                    creatureData.openLevel = openLevel;
                    creatureDataList.AddData(creatureData);
                }
            }
            finally
            {
                IDisposable disposable;
                if ((disposable = (enumerator as IDisposable)) != null)
                {
                    disposable.Dispose();
                }
            }
            return creatureDataList;
        }


        public static CreatureTypeInfo LoadCreatureTypeInfo(XmlDocument doc, ref List<CreatureSpecialSkillTipTable> creatureSpecialSkillTipList, ref Dictionary<long, int> specialTipSizeLib, out ChildCreatureData childData)
        {
            XmlNode xmlNode = doc.SelectSingleNode("/creature/info");
            XmlNode xmlNode2 = doc.SelectSingleNode("/creature/observe");
            XmlNode xmlNode3 = doc.SelectSingleNode("/creature/etc");
            XmlNode xmlNode4 = doc.SelectSingleNode("/creature/child");
            CreatureTypeInfo creatureTypeInfo = new CreatureTypeInfo();
            creatureTypeInfo.id = long.Parse(xmlNode.Attributes.GetNamedItem("id").InnerText);
            ChildCreatureData childCreatureData = new ChildCreatureData();
            if (xmlNode4 != null)
            {
                XmlNode xmlNode5 = xmlNode4.SelectSingleNode("name");
                if (xmlNode5 != null)
                {
                    childCreatureData.name = xmlNode5.InnerText;
                }
                XmlNode xmlNode6 = xmlNode4.SelectSingleNode("codeId");
                if (xmlNode6 != null)
                {
                    childCreatureData.codeId = xmlNode6.InnerText;
                }
            }
            childData = childCreatureData;
            CreatureSpecialSkillTipTable creatureSpecialSkillTipTable = new CreatureSpecialSkillTipTable(creatureTypeInfo.id);
            XmlNode xmlNode7 = xmlNode2.SelectSingleNode("specialTipSize");
            if (xmlNode7 != null)
            {
                int value = (int)float.Parse(xmlNode7.Attributes.GetNamedItem("size").InnerText);
                specialTipSizeLib.Add(creatureTypeInfo.id, value);
                XmlNodeList xmlNodeList = xmlNode7.SelectNodes("specialTip");
                int num = 0;
                IEnumerator enumerator = xmlNodeList.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        object obj = enumerator.Current;
                        XmlNode xmlNode8 = (XmlNode)obj;
                        CreatureSpecialSkillDesc creatureSpecialSkillDesc = new CreatureSpecialSkillDesc();
                        creatureSpecialSkillDesc.key = xmlNode8.Attributes.GetNamedItem("key").InnerText;
                        if (xmlNode8.Attributes.GetNamedItem("openLevel") != null)
                        {
                            creatureSpecialSkillDesc.openLevel = (int)float.Parse(xmlNode8.Attributes.GetNamedItem("openLevel").InnerText);
                        }
                        else
                        {
                            creatureSpecialSkillDesc.openLevel = 1;
                        }
                        creatureSpecialSkillDesc.index = num;
                        creatureSpecialSkillDesc.desc = xmlNode8.InnerText;
                        creatureSpecialSkillDesc.original = creatureSpecialSkillDesc.desc;
                        num++;
                        creatureSpecialSkillTipTable.descList.Add(creatureSpecialSkillDesc);
                    }
                }
                finally
                {
                    IDisposable disposable;
                    if ((disposable = (enumerator as IDisposable)) != null)
                    {
                        disposable.Dispose();
                    }
                }
                creatureSpecialSkillTipList.Add(creatureSpecialSkillTipTable);
            }
            XmlNode xmlNode9 = xmlNode2.SelectSingleNode("max");
            if (xmlNode9 != null)
            {
                CreatureMaxObserve maxObserveModule = creatureTypeInfo.maxObserveModule;
                XmlNodeList xmlNodeList2 = xmlNode9.SelectNodes("desc");
                if (xmlNodeList2 != null)
                {
                    IEnumerator enumerator2 = xmlNodeList2.GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                        {
                            object obj2 = enumerator2.Current;
                            XmlNode xmlNode10 = (XmlNode)obj2;
                            CreatureMaxObserve.Desc desc = new CreatureMaxObserve.Desc();
                            desc.id = (int)float.Parse(xmlNode10.Attributes.GetNamedItem("id").InnerText);
                            desc.selectId = (int)float.Parse(xmlNode10.Attributes.GetNamedItem("select").InnerText);
                            string format_text = xmlNode10.InnerText.Trim();
                            string[] textFromFormatProcessText = TextConverter.GetTextFromFormatProcessText(format_text);
                            desc.Init(textFromFormatProcessText);
                            maxObserveModule.descs.Add(desc);
                        }
                    }
                    finally
                    {
                        IDisposable disposable2;
                        if ((disposable2 = (enumerator2 as IDisposable)) != null)
                        {
                            disposable2.Dispose();
                        }
                    }
                }
                XmlNode xmlNode11 = xmlNode9.SelectSingleNode("desc");
                string format_text2 = xmlNode11.InnerText.Trim();
                string[] textFromFormatProcessText2 = TextConverter.GetTextFromFormatProcessText(format_text2);
                maxObserveModule.desc.Init(textFromFormatProcessText2);
                XmlNodeList xmlNodeList3 = xmlNode9.SelectNodes("select");
                if (xmlNodeList3 != null)
                {
                    IEnumerator enumerator3 = xmlNodeList3.GetEnumerator();
                    try
                    {
                        while (enumerator3.MoveNext())
                        {
                            object obj3 = enumerator3.Current;
                            XmlNode xmlNode12 = (XmlNode)obj3;
                            XmlNodeList xmlNodeList4 = xmlNode12.SelectNodes("node");
                            CreatureMaxObserve.Select select = new CreatureMaxObserve.Select();
                            select.id = (int)float.Parse(xmlNode12.Attributes.GetNamedItem("id").InnerText);
                            IEnumerator enumerator4 = xmlNodeList4.GetEnumerator();
                            try
                            {
                                while (enumerator4.MoveNext())
                                {
                                    object obj4 = enumerator4.Current;
                                    XmlNode xmlNode13 = (XmlNode)obj4;
                                    CreatureMaxObserve.Select.SelectNode selectNode = new CreatureMaxObserve.Select.SelectNode();
                                    selectNode.desc = xmlNode13.Attributes.GetNamedItem("desc").InnerText;
                                    selectNode.isAnswer = GetBooleanData(xmlNode13.Attributes.GetNamedItem("isAnswer").InnerText);
                                    if (selectNode.isAnswer)
                                    {
                                        XmlNode namedItem = xmlNode13.Attributes.GetNamedItem("message");
                                        if (namedItem != null)
                                        {
                                            selectNode.message = xmlNode13.Attributes.GetNamedItem("message").InnerText;
                                        }
                                        else
                                        {
                                            selectNode.message = null;
                                        }
                                    }
                                    if (xmlNode13.Attributes.GetNamedItem("target") != null && xmlNode13.Attributes.GetNamedItem("target").InnerText != string.Empty)
                                    {
                                        selectNode.targetId = (int)float.Parse(xmlNode13.Attributes.GetNamedItem("target").InnerText);
                                    }
                                    else
                                    {
                                        selectNode.targetId = -1;
                                    }
                                    maxObserveModule.select.list.Add(selectNode);
                                    select.list.Add(selectNode);
                                }
                            }
                            finally
                            {
                                IDisposable disposable3;
                                if ((disposable3 = (enumerator4 as IDisposable)) != null)
                                {
                                    disposable3.Dispose();
                                }
                            }
                            maxObserveModule.selects.Add(select);
                        }
                    }
                    finally
                    {
                        IDisposable disposable4;
                        if ((disposable4 = (enumerator3 as IDisposable)) != null)
                        {
                            disposable4.Dispose();
                        }
                    }
                }
                XmlNode xmlNode14 = xmlNode9.SelectSingleNode("angela");
                string format_text3 = xmlNode14.InnerText.Trim();
                string[] textFromFormatProcessText3 = TextConverter.GetTextFromFormatProcessText(format_text3);
                maxObserveModule.angela.Init(textFromFormatProcessText3);
                maxObserveModule.init = true;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            XmlNodeList xmlNodeList5 = xmlNode.SelectNodes("narration");
            IEnumerator enumerator5 = xmlNodeList5.GetEnumerator();
            try
            {
                while (enumerator5.MoveNext())
                {
                    object obj5 = enumerator5.Current;
                    XmlNode xmlNode15 = (XmlNode)obj5;
                    string innerText = xmlNode15.Attributes.GetNamedItem("action").InnerText;
                    string value2 = xmlNode15.InnerText.Trim();
                    dictionary.Add(innerText, value2);
                }
            }
            finally
            {
                IDisposable disposable5;
                if ((disposable5 = (enumerator5 as IDisposable)) != null)
                {
                    disposable5.Dispose();
                }
            }
            creatureTypeInfo.narrationTable = dictionary;
            creatureTypeInfo.MaxObserveLevel = (int)float.Parse(xmlNode2.Attributes.GetNamedItem("level").InnerText);
            XmlNode xmlNode16 = xmlNode2.SelectSingleNode("collection");



            CreatureTypeInfo.CreatureDataTable creatureDataTable = new CreatureTypeInfo.CreatureDataTable();
            foreach (string text in CreatureTypeInfo.stringData)
            {
                XmlNodeList nodes = xmlNode16.SelectNodes(text);
                CreatureTypeInfo.CreatureDataList creatureDataList = GetCreatureDataList(nodes, text, false);
                creatureDataTable.dictionary.Add(text, creatureDataList);
            }
            creatureTypeInfo.dataTable = creatureDataTable;




            XmlNode xmlNode17 = xmlNode16.SelectSingleNode("openText");
            if (xmlNode17 != null)
            {
                string openText = xmlNode17.InnerText.Trim();
                creatureTypeInfo.openText = openText;
            }
            XmlNodeList xmlNodeList6 = xmlNode2.SelectNodes("desc");
            IEnumerator enumerator6 = xmlNodeList6.GetEnumerator();
            try
            {
                while (enumerator6.MoveNext())
                {
                    object obj6 = enumerator6.Current;
                    XmlNode xmlNode18 = (XmlNode)obj6;
                    int item = (int)float.Parse(xmlNode18.Attributes.GetNamedItem("openLevel").InnerText);
                    string text = xmlNode18.InnerText.Trim();
                    string textFromFormatAlter = TextConverter.GetTextFromFormatAlter(text);
                    creatureTypeInfo.desc.Add(textFromFormatAlter);
                    creatureTypeInfo.observeTable.desc.Add(item);
                }
            }
            finally
            {
                IDisposable disposable6;
                if ((disposable6 = (enumerator6 as IDisposable)) != null)
                {
                    disposable6.Dispose();
                }
            }
            XmlNodeList xmlNodeList7 = xmlNode2.SelectNodes("record");
            IEnumerator enumerator7 = xmlNodeList7.GetEnumerator();
            try
            {
                while (enumerator7.MoveNext())
                {
                    object obj7 = enumerator7.Current;
                    XmlNode xmlNode19 = (XmlNode)obj7;
                    int item2 = (int)float.Parse(xmlNode19.Attributes.GetNamedItem("openLevel").InnerText);
                    string text2 = xmlNode19.InnerText.Trim();
                    string textFromFormatAlter2 = TextConverter.GetTextFromFormatAlter(text2);
                    creatureTypeInfo.observeRecord.Add(textFromFormatAlter2);
                    creatureTypeInfo.observeTable.record.Add(item2);
                }
            }
            finally
            {
                IDisposable disposable7;
                if ((disposable7 = (enumerator7 as IDisposable)) != null)
                {
                    disposable7.Dispose();
                }
            }
            if (xmlNode3 != null)
            {
                XmlNodeList xmlNodeList8 = xmlNode3.SelectNodes("param");
                IEnumerator enumerator8 = xmlNodeList8.GetEnumerator();
                try
                {
                    while (enumerator8.MoveNext())
                    {
                        object obj8 = enumerator8.Current;
                        XmlNode xmlNode20 = (XmlNode)obj8;
                        CreatureStaticData.ParameterData parameterData = new CreatureStaticData.ParameterData();
                        string innerText2 = xmlNode20.Attributes.GetNamedItem("key").InnerText;
                        int index = (int)float.Parse(xmlNode20.Attributes.GetNamedItem("index").InnerText);
                        string innerText3 = xmlNode20.InnerText;
                        parameterData.desc = innerText3;
                        parameterData.index = index;
                        parameterData.key = innerText2;
                        creatureTypeInfo.creatureStaticData.paramList.Add(parameterData);
                    }
                }
                finally
                {
                    IDisposable disposable8;
                    if ((disposable8 = (enumerator8 as IDisposable)) != null)
                    {
                        disposable8.Dispose();
                    }
                }
            }
            return creatureTypeInfo;
        }


        public static CreatureTypeInfo LoadChildMeta(string currentLn, string src, ref List<CreatureSpecialSkillTipTable> creatureSpecialSkillTipList, ref Dictionary<long, int> specialTipSizeLib)
        {
            XmlDocument xmlDocument = AssetLoader.LoadExternalXML(string.Format("Language/{0}/creatures/{1}_{0}", currentLn,src));
            if (xmlDocument == null)
            {
                xmlDocument = AssetLoader.LoadExternalXML(string.Format("Language/{0}/creatures/{1}_{0}", "en", src));
            }
            ChildCreatureData childCreatureData = null;
            return LoadCreatureTypeInfo(xmlDocument, ref creatureSpecialSkillTipList, ref specialTipSizeLib, out childCreatureData);
        }
        public static CreatureEventCallTime GetCreatureEventCallTime(string time)
        {
            CreatureEventCallTime result = CreatureEventCallTime.Immediately;
            string text = time.ToLower();
            if (text != null)
            {
                if (!(text == "onroomenter"))
                {
                    if (text == "onrelease")
                    {
                        result = CreatureEventCallTime.onRelease;
                    }
                }
                else
                {
                    result = CreatureEventCallTime.OnRoomEnter;
                }
            }
            return result;
        }

        public static SkillTrigger.ClearEvent SkillTriggerClearEvent(XmlNode node)
        {
            SkillTrigger.ClearEvent clearEvent = new SkillTrigger.ClearEvent();
            string innerText = node.Attributes.GetNamedItem("clear").InnerText;
            bool booleanData = GetBooleanData(innerText);
            string innerText2 = node.InnerText;
            if (innerText2 == null || innerText2 == string.Empty || innerText2 == string.Empty)
            {
                clearEvent.hasEvent = false;
            }
            else
            {
                XmlNode namedItem = node.Attributes.GetNamedItem("time");
                if (namedItem != null)
                {
                    clearEvent.eventTime = GetCreatureEventCallTime(namedItem.InnerText);
                }
                clearEvent.hasEvent = true;
            }
            clearEvent.clear = booleanData;
            return clearEvent;
        }


        public static void LoadSkillTrigger(XmlNode triggerRoot, CreatureTypeInfo typeinfo)
        {
            IEnumerator enumerator = triggerRoot.SelectNodes("useSkill").GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object obj = enumerator.Current;
                    XmlNode xmlNode = (XmlNode)obj;
                    UseSkillTrigger useSkillTrigger = new UseSkillTrigger();
                    string innerText = xmlNode.Attributes.GetNamedItem("checkTime").InnerText;
                    XmlNode xmlNode2 = xmlNode.SelectSingleNode("skillType");
                    if (xmlNode2 != null)
                    {
                        long skillId = (long)float.Parse(xmlNode2.InnerText);
                        int maxCount = (int)float.Parse(xmlNode2.Attributes.GetNamedItem("max").InnerText);
                        useSkillTrigger.skillId = skillId;
                        useSkillTrigger.maxCount = maxCount;
                    }
                    XmlNode xmlNode3 = xmlNode.SelectSingleNode("clear");
                    useSkillTrigger._ClearOnActivated = SkillTriggerClearEvent(xmlNode3.SelectSingleNode("activated"));
                    useSkillTrigger._ClearOnFalse = SkillTriggerClearEvent(xmlNode3.SelectSingleNode("onCheckFalse"));
                    IEnumerator enumerator2 = xmlNode.SelectNodes("calledEvent").GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                        {
                            object obj2 = enumerator2.Current;
                            XmlNode xmlNode4 = (XmlNode)obj2;
                            SkillTrigger.CalledEvent calledEvent = new SkillTrigger.CalledEvent();
                            calledEvent.eventName = xmlNode4.InnerText;
                            XmlNode namedItem;
                            if ((namedItem = xmlNode4.Attributes.GetNamedItem("time")) != null)
                            {
                                calledEvent.eventTime = GetCreatureEventCallTime(namedItem.InnerText);
                            }
                            if (!int.TryParse(xmlNode4.Attributes.GetNamedItem("count").InnerText, out calledEvent.calledCount))
                            {
                                calledEvent.calledCount = useSkillTrigger.maxCount;
                            }
                            useSkillTrigger.calledEvent.Add(calledEvent);
                        }
                    }
                    finally
                    {
                        IDisposable disposable;
                        if ((disposable = (enumerator2 as IDisposable)) != null)
                        {
                            disposable.Dispose();
                        }
                    }
                    if (innerText != null && innerText == "OnEnterRoom")
                    {
                        typeinfo.skillTriggerCheck.onEnterRoom.Add(useSkillTrigger);
                    }
                    typeinfo.skillTriggerCheck.total.Add(useSkillTrigger);
                }
            }
            finally
            {
                IDisposable disposable2;
                if ((disposable2 = (enumerator as IDisposable)) != null)
                {
                    disposable2.Dispose();
                }
            }
        }


        private static void LoadCreatureStat(XmlNode stat, XmlNode statCreature, CreatureTypeInfo model)
        {
            XmlNode xmlNode;
            if ((xmlNode = statCreature.SelectSingleNode("script")) != null)
            {
                model.script = xmlNode.InnerText;
            }
            XmlNode xmlNode2;
            if ((xmlNode2 = statCreature.SelectSingleNode("workAnim")) != null)
            {
                model.workAnim = xmlNode2.InnerText;
                XmlNode namedItem = xmlNode2.Attributes.GetNamedItem("face");
                if (namedItem != null)
                {
                    model.workAnimFace = namedItem.InnerText;
                }
            }
            XmlNode xmlNode3;
            if ((xmlNode3 = statCreature.SelectSingleNode("kitIcon")) != null)
            {
                model.kitIconSrc = xmlNode3.InnerText;
            }
            XmlNode xmlNode4;
            if ((xmlNode4 = stat.SelectSingleNode("workType")) != null)
            {
                string innerText = xmlNode4.InnerText;
                if (innerText != null)
                {
                    if (!(innerText == "normal"))
                    {
                        if (innerText == "kit")
                        {
                            model.creatureWorkType = CreatureWorkType.KIT;
                        }
                    }
                    else
                    {
                        model.creatureWorkType = CreatureWorkType.NORMAL;
                    }
                }
            }
            XmlNode xmlNode5;
            if ((xmlNode5 = stat.SelectSingleNode("kitType")) != null)
            {
                string innerText2 = xmlNode5.InnerText;
                if (innerText2 != null)
                {
                    if (!(innerText2 == "equip"))
                    {
                        if (!(innerText2 == "channel"))
                        {
                            if (innerText2 == "oneshot")
                            {
                                model.creatureKitType = CreatureKitType.ONESHOT;
                            }
                        }
                        else
                        {
                            model.creatureKitType = CreatureKitType.CHANNEL;
                        }
                    }
                    else
                    {
                        model.creatureKitType = CreatureKitType.EQUIP;
                    }
                }
            }
            XmlNode xmlNode6;
            if ((xmlNode6 = stat.SelectSingleNode("qliphoth")) != null)
            {
                model.qliphothMax = int.Parse(xmlNode6.InnerText);
            }
            XmlNode xmlNode7;
            if ((xmlNode7 = stat.SelectSingleNode("speed")) != null)
            {
                model.speed = float.Parse(xmlNode7.InnerText);
            }
            XmlNode xmlNode8 = stat.SelectSingleNode("escapeable");
            if (xmlNode8 != null)
            {
                bool booleanData = GetBooleanData(xmlNode8.InnerText.Trim());
                model.isEscapeAble = booleanData;
            }
            else
            {
                model.isEscapeAble = true;
            }
            XmlNode xmlNode9 = stat.SelectSingleNode("hp");
            if (xmlNode9 != null)
            {
                model.maxHp = (int)float.Parse(xmlNode9.InnerText);
            }
            else
            {
                model.maxHp = 5;
            }
            IEnumerator enumerator = stat.SelectNodes("workProb").GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object obj = enumerator.Current;
                    XmlNode xmlNode10 = (XmlNode)obj;
                    RwbpType type = ConvertToRWBP(xmlNode10.Attributes.GetNamedItem("type").InnerText);
                    IEnumerator enumerator2 = xmlNode10.SelectNodes("prob").GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                        {
                            object obj2 = enumerator2.Current;
                            XmlNode xmlNode11 = (XmlNode)obj2;
                            int level = int.Parse(xmlNode11.Attributes.GetNamedItem("level").InnerText);
                            float prob = float.Parse(xmlNode11.InnerText);
                            model.workProbTable.SetWorkProb(type, level, prob);
                        }
                    }
                    finally
                    {
                        IDisposable disposable;
                        if ((disposable = (enumerator2 as IDisposable)) != null)
                        {
                            disposable.Dispose();
                        }
                    }
                }
            }
            finally
            {
                IDisposable disposable2;
                if ((disposable2 = (enumerator as IDisposable)) != null)
                {
                    disposable2.Dispose();
                }
            }
            XmlNode xmlNode12 = stat.SelectSingleNode("workCooltime");
            if (xmlNode12 != null)
            {
                model.workCooltime = int.Parse(xmlNode12.InnerText);
            }
            XmlNode xmlNode13 = stat.SelectSingleNode("workSpeed");
            if (xmlNode13 != null)
            {
                model.cubeSpeed = float.Parse(xmlNode13.InnerText);
            }
            XmlNode xmlNode14 = statCreature.SelectSingleNode("skillTrigger");
            if (xmlNode14 != null)
            {
                LoadSkillTrigger(xmlNode14, model);
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            IEnumerator enumerator3 = statCreature.SelectNodes("sound").GetEnumerator();
            try
            {
                while (enumerator3.MoveNext())
                {
                    object obj3 = enumerator3.Current;
                    XmlNode xmlNode15 = (XmlNode)obj3;
                    string innerText3 = xmlNode15.Attributes.GetNamedItem("action").InnerText;
                    string innerText4 = xmlNode15.Attributes.GetNamedItem("src").InnerText;
                    dictionary.Add(innerText3, innerText4);
                }
            }
            finally
            {
                IDisposable disposable3;
                if ((disposable3 = (enumerator3 as IDisposable)) != null)
                {
                    disposable3.Dispose();
                }
            }
            model.soundTable = dictionary;
            model.nodeInfo = statCreature.SelectNodes("graph/node");
            model.edgeInfo = statCreature.SelectNodes("graph/edge");
            XmlNode xmlNode16 = statCreature.SelectSingleNode("anim");
            if (xmlNode16 != null)
            {
                model.animSrc = xmlNode16.Attributes.GetNamedItem("prefab").InnerText;
            }
            XmlNode xmlNode17 = statCreature.SelectSingleNode("returnImg");
            if (xmlNode17 != null)
            {
                model.roomReturnSrc = xmlNode17.Attributes.GetNamedItem("src").InnerText;
            }
            else
            {
                model.roomReturnSrc = string.Empty;
            }
            XmlNode xmlNode18 = stat.SelectSingleNode("feelingStateCubeBounds");
            if (xmlNode18 != null)
            {
                List<int> list = new List<int>();
                IEnumerator enumerator4 = xmlNode18.SelectNodes("cube").GetEnumerator();
                try
                {
                    while (enumerator4.MoveNext())
                    {
                        object obj4 = enumerator4.Current;
                        XmlNode xmlNode19 = (XmlNode)obj4;
                        list.Add(int.Parse(xmlNode19.InnerText));
                    }
                }
                finally
                {
                    IDisposable disposable4;
                    if ((disposable4 = (enumerator4 as IDisposable)) != null)
                    {
                        disposable4.Dispose();
                    }
                }
                model.feelingStateCubeBounds.upperBounds = list.ToArray();
            }
            XmlNode xmlNode20 = stat.SelectSingleNode("workDamage");
            if (xmlNode20 != null)
            {
                model.workDamage = ConvertToDamageInfo(xmlNode20);
            }
            XmlNode xmlNode21 = stat.SelectSingleNode("specialDamage");
            if (xmlNode21 != null)
            {
                Dictionary<string, EquipmentTypeInfo> dictionary2 = new Dictionary<string, EquipmentTypeInfo>();
                IEnumerator enumerator5 = xmlNode21.ChildNodes.GetEnumerator();
                try
                {
                    while (enumerator5.MoveNext())
                    {
                        object obj5 = enumerator5.Current;
                        XmlNode xmlNode22 = (XmlNode)obj5;
                        if (xmlNode22.Name == "damage")
                        {
                            string innerText5 = xmlNode22.Attributes.GetNamedItem("id").InnerText;
                            EquipmentTypeInfo value = EquipmentTypeInfo.MakeWeaponInfoByDamageInfo(ConvertToDamageInfo(xmlNode22));
                            dictionary2.Add(innerText5, value);
                        }
                        else if (xmlNode22.Name == "weapon")
                        {
                            string innerText6 = xmlNode22.Attributes.GetNamedItem("id").InnerText;
                            string innerText7 = xmlNode22.Attributes.GetNamedItem("weaponId").InnerText;
                            EquipmentTypeInfo data = EquipmentTypeList.instance.GetData(int.Parse(innerText7));
                            dictionary2.Add(innerText6, data);
                        }
                    }
                }
                finally
                {
                    IDisposable disposable5;
                    if ((disposable5 = (enumerator5 as IDisposable)) != null)
                    {
                        disposable5.Dispose();
                    }
                }
                model.creatureSpecialDamageTable.Init(dictionary2);
            }
            Dictionary<string, DefenseInfo> dictionary3 = new Dictionary<string, DefenseInfo>();
            IEnumerator enumerator6 = stat.SelectNodes("defense").GetEnumerator();
            try
            {
                while (enumerator6.MoveNext())
                {
                    object obj6 = enumerator6.Current;
                    XmlNode xmlNode23 = (XmlNode)obj6;
                    string innerText8 = xmlNode23.Attributes.GetNamedItem("id").InnerText;
                    DefenseInfo defenseInfo = new DefenseInfo();
                    IEnumerator enumerator7 = xmlNode23.SelectNodes("defenseElement").GetEnumerator();
                    try
                    {
                        while (enumerator7.MoveNext())
                        {
                            object obj7 = enumerator7.Current;
                            XmlNode xmlNode24 = (XmlNode)obj7;
                            string innerText9 = xmlNode24.Attributes.GetNamedItem("type").InnerText;
                            if (innerText9 != null)
                            {
                                if (!(innerText9 == "R"))
                                {
                                    if (!(innerText9 == "W"))
                                    {
                                        if (!(innerText9 == "B"))
                                        {
                                            if (innerText9 == "P")
                                            {
                                                defenseInfo.P = float.Parse(xmlNode24.InnerText);
                                            }
                                        }
                                        else
                                        {
                                            defenseInfo.B = float.Parse(xmlNode24.InnerText);
                                        }
                                    }
                                    else
                                    {
                                        defenseInfo.W = float.Parse(xmlNode24.InnerText);
                                    }
                                }
                                else
                                {
                                    defenseInfo.R = float.Parse(xmlNode24.InnerText);
                                }
                            }
                        }
                    }
                    finally
                    {
                        IDisposable disposable6;
                        if ((disposable6 = (enumerator7 as IDisposable)) != null)
                        {
                            disposable6.Dispose();
                        }
                    }
                    dictionary3.Add(innerText8, defenseInfo);
                }
            }
            finally
            {
                IDisposable disposable7;
                if ((disposable7 = (enumerator6 as IDisposable)) != null)
                {
                    disposable7.Dispose();
                }
            }
            model.defenseTable.Init(dictionary3);
            XmlNode xmlNode25 = stat.SelectSingleNode("observeInfo");
            if (xmlNode25 != null)
            {
                List<ObserveInfoData> list2 = new List<ObserveInfoData>();
                IEnumerator enumerator8 = xmlNode25.SelectNodes("observeElement").GetEnumerator();
                try
                {
                    while (enumerator8.MoveNext())
                    {
                        object obj8 = enumerator8.Current;
                        XmlNode xmlNode26 = (XmlNode)obj8;
                        string regionName = xmlNode26.Attributes.GetNamedItem("name").InnerText.Trim();
                        int observeCost = (int)float.Parse(xmlNode26.Attributes.GetNamedItem("cost").InnerText);
                        ObserveInfoData item = new ObserveInfoData
                        {
                            observeCost = observeCost,
                            regionName = regionName
                        };
                        list2.Add(item);
                    }
                }
                finally
                {
                    IDisposable disposable8;
                    if ((disposable8 = (enumerator8 as IDisposable)) != null)
                    {
                        disposable8.Dispose();
                    }
                }
                model.observeData = list2;
            }
            else
            {
                List<ObserveInfoData> list3 = new List<ObserveInfoData>();
                for (int i = 0; i < CreatureModel.regionName.Length; i++)
                {
                    ObserveInfoData item2 = new ObserveInfoData
                    {
                        observeCost = 0,
                        regionName = CreatureModel.regionName[i]
                    };
                    list3.Add(item2);
                }
                for (int j = 0; j < CreatureModel.careTakingRegion.Length; j++)
                {
                    ObserveInfoData item3 = new ObserveInfoData
                    {
                        observeCost = 0,
                        regionName = CreatureModel.careTakingRegion[j]
                    };
                    list3.Add(item3);
                }
                model.observeData = list3;
            }
            List<CreatureEquipmentMakeInfo> list4 = new List<CreatureEquipmentMakeInfo>();
            IEnumerator enumerator9 = stat.SelectNodes("equipment").GetEnumerator();
            try
            {
                while (enumerator9.MoveNext())
                {
                    object obj9 = enumerator9.Current;
                    XmlNode xmlNode27 = (XmlNode)obj9;
                    XmlNode namedItem2 = xmlNode27.Attributes.GetNamedItem("equipId");
                    XmlNode namedItem3 = xmlNode27.Attributes.GetNamedItem("level");
                    XmlNode namedItem4 = xmlNode27.Attributes.GetNamedItem("cost");
                    XmlNode namedItem5 = xmlNode27.Attributes.GetNamedItem("prob");
                    CreatureEquipmentMakeInfo creatureEquipmentMakeInfo = new CreatureEquipmentMakeInfo();
                    if (namedItem2 != null)
                    {
                        int id = int.Parse(namedItem2.InnerText);
                        creatureEquipmentMakeInfo.equipTypeInfo = EquipmentTypeList.instance.GetData(id);
                        if (creatureEquipmentMakeInfo.equipTypeInfo == null)
                        {
                            continue;
                        }
                    }
                    if (namedItem3 != null)
                    {
                        creatureEquipmentMakeInfo.level = int.Parse(namedItem3.InnerText);
                    }
                    if (namedItem4 != null)
                    {
                        creatureEquipmentMakeInfo.cost = int.Parse(namedItem4.InnerText);
                    }
                    if (namedItem5 != null)
                    {
                        creatureEquipmentMakeInfo.prob = float.Parse(namedItem5.InnerText);
                    }
                    list4.Add(creatureEquipmentMakeInfo);
                }
            }
            finally
            {
                IDisposable disposable9;
                if ((disposable9 = (enumerator9 as IDisposable)) != null)
                {
                    disposable9.Dispose();
                }
            }
            model.equipMakeInfos = list4;
            List<CreatureObserveBonusData> list5 = new List<CreatureObserveBonusData>();
            IEnumerator enumerator10 = stat.SelectNodes("observeBonus").GetEnumerator();
            try
            {
                while (enumerator10.MoveNext())
                {
                    object obj10 = enumerator10.Current;
                    XmlNode xmlNode28 = (XmlNode)obj10;
                    int level2 = int.Parse(xmlNode28.Attributes.GetNamedItem("level").InnerText);
                    string innerText10 = xmlNode28.Attributes.GetNamedItem("type").InnerText;
                    CreatureObserveBonusData creatureObserveBonusData = new CreatureObserveBonusData();
                    if (innerText10 != null)
                    {
                        if (!(innerText10 == "prob"))
                        {
                            if (innerText10 == "speed")
                            {
                                creatureObserveBonusData.bonus = CreatureObserveBonusData.BonusType.SPEED;
                            }
                        }
                        else
                        {
                            creatureObserveBonusData.bonus = CreatureObserveBonusData.BonusType.PROB;
                        }
                    }
                    creatureObserveBonusData.level = level2;
                    creatureObserveBonusData.value = int.Parse(xmlNode28.InnerText);
                    list5.Add(creatureObserveBonusData);
                }
            }
            finally
            {
                IDisposable disposable10;
                if ((disposable10 = (enumerator10 as IDisposable)) != null)
                {
                    disposable10.Dispose();
                }
            }
            model.observeBonus.Init(list5);
            XmlNode xmlNode29 = stat.SelectSingleNode("maxWorkCount");
            if (xmlNode29 != null)
            {
                model.maxWorkCount = int.Parse(xmlNode29.InnerText);
            }
            XmlNode xmlNode30 = stat.SelectSingleNode("maxProbReductionCounter");
            if (xmlNode30 != null)
            {
                model.maxProbReductionCounter = int.Parse(xmlNode30.InnerText);
            }
            XmlNode xmlNode31 = stat.SelectSingleNode("probReduction");
            if (xmlNode31 != null)
            {
                model.probReduction = float.Parse(xmlNode31.InnerText);
            }
        }






        public static void LoadOriginalGameCreature(

            CreatureDataLoader CDL,

            ref List<CreatureTypeInfo> CreatureTypeInfoList,

    
            ref List<CreatureSpecialSkillTipTable> CreatureSpecialSkillTipTableList ,

    
            ref Dictionary<long, int> specialTipSize

            )
        {
            LCBaseMod.Instance.MakeMessageLog("Loading Orginal data...");
            string __currentLn = GlobalGameManager.instance.GetCurrentLanguage();
            XmlDocument OrginalGameCreatureList = new XmlDocument();
            OrginalGameCreatureList.LoadXml(Resources.Load<TextAsset>("xml/CreatureList").text);
            XmlNodeList CreatureNodes = OrginalGameCreatureList.SelectNodes("/creature_list/creature");//提取节点列表
            System.Collections.IEnumerator CreatureNodesEnumerator = CreatureNodes.GetEnumerator();
            try
            {
                while (CreatureNodesEnumerator.MoveNext())
                {
                    XmlNode SingalCreatureNode = (XmlNode)CreatureNodesEnumerator.Current;
                    string TheCreatureSrc = SingalCreatureNode.Attributes.GetNamedItem("src").InnerText;

                    //xmlstat
                    XmlDocument CreatureStatsDataXmlDoc = new XmlDocument();

                    //读取stats数据

                    TextAsset CreatureStatsTextAsset = Resources.Load<TextAsset>("xml/creatureStats/" + SingalCreatureNode.SelectSingleNode("stat").InnerText);
                    CreatureStatsDataXmlDoc.LoadXml(CreatureStatsTextAsset.text);





                    XmlDocument CreatureLangDataXmlDoc = new XmlDocument();
                    //读取语言本地化文件

                    CreatureLangDataXmlDoc = AssetLoader.LoadExternalXML($"Language/{__currentLn}/creatures/{TheCreatureSrc}_{__currentLn}");

                    // string _metaid = SingalCreatureNode.Attributes.GetNamedItem("id").InnerText;
                    //  CreatureLangDataXmlDoc = ExtensionManager.instance.GetModModCreatureInfo(__currentLn, _metaid);



                    if (CreatureLangDataXmlDoc == null)
                    {
                        CreatureLangDataXmlDoc = AssetLoader.LoadExternalXML($"Language/en/creatures/{TheCreatureSrc}_en");
                    }
                    CreatureTypeInfo creatureTypeInfo = LCBM_Tools_CDL.LoadCreatureTypeInfo(CreatureLangDataXmlDoc, ref CreatureSpecialSkillTipTableList, ref specialTipSize, out ChildCreatureData data);


                    XmlNode Stat_creatureNode = CreatureStatsDataXmlDoc.SelectSingleNode("creature");
                    XmlNode Stat_statNode = CreatureStatsDataXmlDoc.SelectSingleNode("creature/stat");
                    XmlNode Stat_ChildCreatureNode = Stat_creatureNode.SelectSingleNode("child");

                   LoadCreatureStat(Stat_statNode, Stat_creatureNode, creatureTypeInfo);


                    //加载子单位Stat数据
                    if (Stat_ChildCreatureNode != null)
                    {
                        string ChildStatSrc = Stat_ChildCreatureNode.InnerText;
                        TextAsset ChildStatSrcTextAsset = Resources.Load<TextAsset>("xml/creatureStats/" + ChildStatSrc);
                        XmlDocument ChildStatXmlDoc = new XmlDocument();
                        ChildStatXmlDoc.LoadXml(ChildStatSrcTextAsset.text);
                        ChildCreatureTypeInfo childCreatureTypeInfo = new ChildCreatureTypeInfo();
                        XmlNode ChildStat_CreatureNode = ChildStatXmlDoc.SelectSingleNode("creature");
                        childCreatureTypeInfo.maxHp = (int)float.Parse(ChildStat_CreatureNode.SelectSingleNode("stat/hp").InnerText);
                        childCreatureTypeInfo.speed = float.Parse(ChildStat_CreatureNode.SelectSingleNode("stat/speed").InnerText);
                        XmlNode ChildStat_CreatureAnimNode = ChildStat_CreatureNode.SelectSingleNode("anim");
                        if (ChildStat_CreatureAnimNode != null)
                        {
                            childCreatureTypeInfo.animSrc = ChildStat_CreatureAnimNode.Attributes.GetNamedItem("prefab").InnerText;
                        }
                        XmlNode ChildStat_RiskInfo = ChildStat_CreatureNode.SelectSingleNode("riskLevel");
                        if (ChildStat_RiskInfo != null)
                        {
                            int riskLevelOpen = (int)float.Parse(ChildStat_RiskInfo.Attributes.GetNamedItem("openLevel").InnerText);
                            string innerText3 = ChildStat_RiskInfo.InnerText;
                            childCreatureTypeInfo.riskLevelOpen = riskLevelOpen;
                            childCreatureTypeInfo._riskLevel = innerText3;
                        }
                        XmlNode ChildStat_AttackInfo = ChildStat_CreatureNode.SelectSingleNode("attackType");
                        if (ChildStat_AttackInfo != null)
                        {
                            int attackTypeOpen = (int)float.Parse(ChildStat_AttackInfo.Attributes.GetNamedItem("openLevel").InnerText);
                            childCreatureTypeInfo.attackTypeOpen = attackTypeOpen;
                            childCreatureTypeInfo._attackType = ChildStat_AttackInfo.InnerText;
                        }
                        Dictionary<string, DefenseInfo> dictionary = new Dictionary<string, DefenseInfo>();
                        System.Collections.IEnumerator enumerator2 = ChildStat_CreatureNode.SelectNodes("stat/defense").GetEnumerator();
                        try
                        {
                            while (enumerator2.MoveNext())
                            {
                                object obj2 = enumerator2.Current;
                                XmlNode xmlNode8 = (XmlNode)obj2;
                                string innerText5 = xmlNode8.Attributes.GetNamedItem("id").InnerText;
                                DefenseInfo defenseInfo = new DefenseInfo();
                                System.Collections.IEnumerator enumerator3 = xmlNode8.SelectNodes("defenseElement").GetEnumerator();
                                try
                                {
                                    while (enumerator3.MoveNext())
                                    {
                                        object obj3 = enumerator3.Current;
                                        XmlNode xmlNode9 = (XmlNode)obj3;
                                        string innerText6 = xmlNode9.Attributes.GetNamedItem("type").InnerText;
                                        if (innerText6 != null)
                                        {
                                            if (!(innerText6 == "R"))
                                            {
                                                if (!(innerText6 == "W"))
                                                {
                                                    if (!(innerText6 == "B"))
                                                    {
                                                        if (innerText6 == "P")
                                                        {
                                                            defenseInfo.P = float.Parse(xmlNode9.InnerText);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        defenseInfo.B = float.Parse(xmlNode9.InnerText);
                                                    }
                                                }
                                                else
                                                {
                                                    defenseInfo.W = float.Parse(xmlNode9.InnerText);
                                                }
                                            }
                                            else
                                            {
                                                defenseInfo.R = float.Parse(xmlNode9.InnerText);
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    IDisposable disposable;
                                    if ((disposable = (enumerator3 as IDisposable)) != null)
                                    {
                                        disposable.Dispose();
                                    }
                                }
                                dictionary.Add(innerText5, defenseInfo);
                            }
                        }
                        finally
                        {
                            IDisposable disposable2;
                            if ((disposable2 = (enumerator2 as IDisposable)) != null)
                            {
                                disposable2.Dispose();
                            }
                        }
                        childCreatureTypeInfo.defenseTable.Init(dictionary);
                        XmlNode xmlNode10 = ChildStat_CreatureNode.SelectSingleNode("script");
                        if (xmlNode10 != null)
                        {
                            childCreatureTypeInfo.script = xmlNode10.InnerText;
                        }
                        XmlNode xmlNode11 = ChildStat_CreatureNode.SelectSingleNode("portrait");
                        if (xmlNode11 != null)
                        {
                            childCreatureTypeInfo._tempPortrait = xmlNode11.InnerText.Trim();
                            childCreatureTypeInfo._isChildAndHasData = true;
                        }
                        XmlNode xmlNode12 = ChildStat_CreatureNode.SelectSingleNode("metaInfo");
                        if (xmlNode12 != null)
                        {
                            string innerText7 = xmlNode12.InnerText;


                            XmlDocument c_xmlDocument = AssetLoader.LoadExternalXML($"Language/{__currentLn}/creatures/{innerText7}_{__currentLn}");
                            if (c_xmlDocument == null)
                            {
                                c_xmlDocument = AssetLoader.LoadExternalXML(string.Format("Language/{0}/creatures/{1}_{0}", "en", innerText7));
                            }

                            CreatureTypeInfo creatureTypeInfo2 = LCBM_Tools_CDL.LoadChildMeta(__currentLn, innerText7, ref CreatureSpecialSkillTipTableList, ref specialTipSize);



                            XmlNode statCreature = ChildStat_CreatureNode;
                            XmlNode stat2 = ChildStat_CreatureNode.SelectSingleNode("stat");
                            LoadCreatureStat(stat2, statCreature, creatureTypeInfo2);
                            CreatureTypeInfoList.Add(creatureTypeInfo2);
                            childCreatureTypeInfo.id = creatureTypeInfo2.id;
                            childCreatureTypeInfo.isHasBaseMeta = true;
                        }
                        XmlNodeList xmlNodeList2 = ChildStat_CreatureNode.SelectNodes("sound");
                        Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
                        System.Collections.IEnumerator enumerator4 = xmlNodeList2.GetEnumerator();
                        try
                        {
                            while (enumerator4.MoveNext())
                            {
                                object obj4 = enumerator4.Current;
                                XmlNode xmlNode13 = (XmlNode)obj4;
                                string innerText8 = xmlNode13.Attributes.GetNamedItem("action").InnerText;
                                string innerText9 = xmlNode13.Attributes.GetNamedItem("src").InnerText;
                                dictionary2.Add(innerText8, innerText9);
                            }
                        }
                        finally
                        {
                            IDisposable disposable3;
                            if ((disposable3 = (enumerator4 as IDisposable)) != null)
                            {
                                disposable3.Dispose();
                            }
                        }
                        childCreatureTypeInfo.soundTable = dictionary2;
                        creatureTypeInfo.childTypeInfo = childCreatureTypeInfo;
                        creatureTypeInfo.childTypeInfo.data = data;
                    }




                    CreatureTypeInfoList.Add(creatureTypeInfo);

                }


            }
            finally
            {
                IDisposable disposable4;
                if ((disposable4 = (CreatureNodesEnumerator as IDisposable)) != null)
                {
                    disposable4.Dispose();
                }
            }


        }





        public static void LoadExtentionCreature(

            Extension ex,


            CreatureDataLoader CDL,

            ref List<CreatureTypeInfo> CreatureTypeInfoList,


            ref List<CreatureSpecialSkillTipTable> CreatureSpecialSkillTipTableList,


            ref Dictionary<long, int> specialTipSize

    )
        {
            LCBaseMod.Instance.MakeMessageLog($"Loading Mod:{ex.GetName()} data...");
            string __currentLn = GlobalGameManager.instance.GetCurrentLanguage();
            foreach(XmlDocument CreatureListXmlDoc in ex.GetCreatureListLib())
            {
                XmlNodeList CreatureNodes = CreatureListXmlDoc.SelectNodes("/creature_list/creature");//提取节点列表
                System.Collections.IEnumerator CreatureNodesEnumerator = CreatureNodes.GetEnumerator();
                try
                {
                    while (CreatureNodesEnumerator.MoveNext())
                    {
                        bool StopLoadingFlag = false;
                        XmlNode SingalCreatureNode = (XmlNode)CreatureNodesEnumerator.Current;
                        string TheCreatureMetaID = SingalCreatureNode.Attributes.GetNamedItem("id").InnerText;
                        string TheCreatureStatName = SingalCreatureNode.SelectSingleNode("stat").InnerText;
                        XmlDocument CreatureStatsDataXmlDoc = new XmlDocument();
                        XmlDocument CreatureLangDataXmlDoc = new XmlDocument();
                        //读取stats数据
                        try
                        {
                             CreatureStatsDataXmlDoc = ex.GetCreatureStat(TheCreatureStatName);

                        }catch(Exception e)
                        {
                           
                            LCBaseMod.Instance.MakeErrorLog($"Exception in loading CreatureStatData(Src:{TheCreatureStatName})at mod:{ex.GetName()}");
                            UnityEngine.Debug.LogException(e);
                            ex.ExpireMod();
                            StopLoadingFlag = true;
                        }
                        //读取语言本地化文件
                        try 
                        { 
                            CreatureLangDataXmlDoc = ex.GetCreatureInfo(__currentLn, TheCreatureMetaID);
                        }
                        catch(Exception e)
                        { 
                            LCBaseMod.Instance.MakeErrorLog($"Cannot find  CreatureInfo({__currentLn}) of {TheCreatureMetaID} at mod:{ex.GetName()}.");
                            UnityEngine.Debug.LogException(e);
                            ex.ExpireMod();
                            StopLoadingFlag = true;
                        }
                        List<long> keysToRemove = new List<long>();
                        foreach (long id in specialTipSize.Keys )
                        {

                            if (TheCreatureMetaID == id.ToString())
                            {
                                LCBaseMod.Instance.MakeWarningLog($"TheCreatureMeta:{TheCreatureMetaID} already exsist. mod:{ex.GetName()}.");
                                   
                                keysToRemove.Add(id);
                            }
                        }
                        if (!BMConfigManager.instance.cfg_AllowCreatureOverwrite.Value)
                        {
                            ex.ExpireMod();
                            StopLoadingFlag = true;
                        }
                        else
                        {
                            foreach (long key in keysToRemove)
                            {
                                specialTipSize.Remove(key);
                                for (int j=0;j<CreatureTypeInfoList.Count; j++)
                                {
                                    CreatureTypeInfo CTIF = CreatureTypeInfoList[j];
                                    if (CTIF.id == key)
                                    {
                                        LCBaseMod.Instance.MakeWarningLog($"TheCreatureMeta:{key} removed.");
                                        CreatureTypeInfoList.RemoveAt(j);
                                    }
                                }
                                
                            }

                        }
                        if (StopLoadingFlag)
                        {
                            continue;
                        }

                
                        CreatureTypeInfo creatureTypeInfo = LCBM_Tools_CDL.LoadCreatureTypeInfo(CreatureLangDataXmlDoc, ref CreatureSpecialSkillTipTableList, ref specialTipSize, out ChildCreatureData data);


                        XmlNode Stat_creatureNode = CreatureStatsDataXmlDoc.SelectSingleNode("creature");
                        XmlNode Stat_statNode = CreatureStatsDataXmlDoc.SelectSingleNode("creature/stat");
                        XmlNode Stat_ChildCreatureNode = Stat_creatureNode.SelectSingleNode("child");

                        LoadCreatureStat(Stat_statNode, Stat_creatureNode, creatureTypeInfo);


     //加载子单位Stat数据
                        if (Stat_ChildCreatureNode != null)
                        {
                            string ChildStatSrc = Stat_ChildCreatureNode.InnerText;

                            XmlDocument ChildStatXmlDoc = ex.GetCreatureStat(ChildStatSrc);

                            ChildCreatureTypeInfo childCreatureTypeInfo = new ChildCreatureTypeInfo();
                            XmlNode ChildStat_CreatureNode = ChildStatXmlDoc.SelectSingleNode("creature");
                            childCreatureTypeInfo.maxHp = (int)float.Parse(ChildStat_CreatureNode.SelectSingleNode("stat/hp").InnerText);
                            childCreatureTypeInfo.speed = float.Parse(ChildStat_CreatureNode.SelectSingleNode("stat/speed").InnerText);
                            XmlNode ChildStat_CreatureAnimNode = ChildStat_CreatureNode.SelectSingleNode("anim");
                            if (ChildStat_CreatureAnimNode != null)
                            {
                                childCreatureTypeInfo.animSrc = ChildStat_CreatureAnimNode.Attributes.GetNamedItem("prefab").InnerText;
                            }
                            XmlNode ChildStat_RiskInfo = ChildStat_CreatureNode.SelectSingleNode("riskLevel");
                            if (ChildStat_RiskInfo != null)
                            {
                                int riskLevelOpen = (int)float.Parse(ChildStat_RiskInfo.Attributes.GetNamedItem("openLevel").InnerText);
                                string innerText3 = ChildStat_RiskInfo.InnerText;
                                childCreatureTypeInfo.riskLevelOpen = riskLevelOpen;
                                childCreatureTypeInfo._riskLevel = innerText3;
                            }
                            XmlNode ChildStat_AttackInfo = ChildStat_CreatureNode.SelectSingleNode("attackType");
                            if (ChildStat_AttackInfo != null)
                            {
                                int attackTypeOpen = (int)float.Parse(ChildStat_AttackInfo.Attributes.GetNamedItem("openLevel").InnerText);
                                childCreatureTypeInfo.attackTypeOpen = attackTypeOpen;
                                childCreatureTypeInfo._attackType = ChildStat_AttackInfo.InnerText;
                            }
                            Dictionary<string, DefenseInfo> dictionary = new Dictionary<string, DefenseInfo>();
                            System.Collections.IEnumerator enumerator2 = ChildStat_CreatureNode.SelectNodes("stat/defense").GetEnumerator();
                            try
                            {
                                while (enumerator2.MoveNext())
                                {
                                    object obj2 = enumerator2.Current;
                                    XmlNode xmlNode8 = (XmlNode)obj2;
                                    string innerText5 = xmlNode8.Attributes.GetNamedItem("id").InnerText;
                                    DefenseInfo defenseInfo = new DefenseInfo();
                                    System.Collections.IEnumerator enumerator3 = xmlNode8.SelectNodes("defenseElement").GetEnumerator();
                                    try
                                    {
                                        while (enumerator3.MoveNext())
                                        {
                                            object obj3 = enumerator3.Current;
                                            XmlNode xmlNode9 = (XmlNode)obj3;
                                            string innerText6 = xmlNode9.Attributes.GetNamedItem("type").InnerText;
                                            if (innerText6 != null)
                                            {
                                                if (!(innerText6 == "R"))
                                                {
                                                    if (!(innerText6 == "W"))
                                                    {
                                                        if (!(innerText6 == "B"))
                                                        {
                                                            if (innerText6 == "P")
                                                            {
                                                                defenseInfo.P = float.Parse(xmlNode9.InnerText);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            defenseInfo.B = float.Parse(xmlNode9.InnerText);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        defenseInfo.W = float.Parse(xmlNode9.InnerText);
                                                    }
                                                }
                                                else
                                                {
                                                    defenseInfo.R = float.Parse(xmlNode9.InnerText);
                                                }
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        IDisposable disposable;
                                        if ((disposable = (enumerator3 as IDisposable)) != null)
                                        {
                                            disposable.Dispose();
                                        }
                                    }
                                    dictionary.Add(innerText5, defenseInfo);
                                }
                            }
                            finally
                            {
                                IDisposable disposable2;
                                if ((disposable2 = (enumerator2 as IDisposable)) != null)
                                {
                                    disposable2.Dispose();
                                }
                            }
                            childCreatureTypeInfo.defenseTable.Init(dictionary);
                            XmlNode xmlNode10 = ChildStat_CreatureNode.SelectSingleNode("script");
                            if (xmlNode10 != null)
                            {
                                childCreatureTypeInfo.script = xmlNode10.InnerText;
                            }
                            XmlNode xmlNode11 = ChildStat_CreatureNode.SelectSingleNode("portrait");
                            if (xmlNode11 != null)
                            {
                                childCreatureTypeInfo._tempPortrait = xmlNode11.InnerText.Trim();
                                childCreatureTypeInfo._isChildAndHasData = true;
                            }
                            XmlNode xmlNode12 = ChildStat_CreatureNode.SelectSingleNode("metaInfo");
                            if (xmlNode12 != null)
                            {
                                string innerText7 = xmlNode12.InnerText;


                                XmlDocument c_xmlDocument = AssetLoader.LoadExternalXML($"Language/{__currentLn}/creatures/{innerText7}_{__currentLn}");
                                if (c_xmlDocument == null)
                                {
                                   //err
                                }

                                CreatureTypeInfo creatureTypeInfo2 = LCBM_Tools_CDL.LoadChildMeta(__currentLn, innerText7, ref CreatureSpecialSkillTipTableList, ref specialTipSize);



                                XmlNode statCreature = ChildStat_CreatureNode;
                                XmlNode stat2 = ChildStat_CreatureNode.SelectSingleNode("stat");
                                LoadCreatureStat( stat2, statCreature, creatureTypeInfo2);
                                CreatureTypeInfoList.Add(creatureTypeInfo2);
                                childCreatureTypeInfo.id = creatureTypeInfo2.id;
                                childCreatureTypeInfo.isHasBaseMeta = true;
                            }
                            XmlNodeList xmlNodeList2 = ChildStat_CreatureNode.SelectNodes("sound");
                            Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
                            System.Collections.IEnumerator enumerator4 = xmlNodeList2.GetEnumerator();
                            try
                            {
                                while (enumerator4.MoveNext())
                                {
                                    object obj4 = enumerator4.Current;
                                    XmlNode xmlNode13 = (XmlNode)obj4;
                                    string innerText8 = xmlNode13.Attributes.GetNamedItem("action").InnerText;
                                    string innerText9 = xmlNode13.Attributes.GetNamedItem("src").InnerText;
                                    dictionary2.Add(innerText8, innerText9);
                                }
                            }
                            finally
                            {
                                IDisposable disposable3;
                                if ((disposable3 = (enumerator4 as IDisposable)) != null)
                                {
                                    disposable3.Dispose();
                                }
                            }
                            childCreatureTypeInfo.soundTable = dictionary2;
                            creatureTypeInfo.childTypeInfo = childCreatureTypeInfo;
                            creatureTypeInfo.childTypeInfo.data = data;
                        }
 
                    




                        CreatureTypeInfoList.Add(creatureTypeInfo);

                    }


                }
                finally
                {
                    IDisposable disposable4;
                    if ((disposable4 = (CreatureNodesEnumerator as IDisposable)) != null)
                    {
                        disposable4.Dispose();
                    }
                }

            }
               
     


        }











    }

    public class LCBM_Tools_Anim
    {
        public static SkeletonDataAsset FindSkeletonDataAsset(string animName)
        {
            foreach (Extension ex in ExtensionManager.Instance.GetExtensionList())
            {
                Dictionary<string,SkeletonDataAsset> lib = ex.GetCreatureAnimLibLib();
                if (lib != null && lib.ContainsKey(animName))
                {
                    return lib[animName];
                }
            }
            return null;
        }

        public static CreatureAnimScript AddCreatureAnimScript(GameObject skeleAnimObj, string animName)
        {
            foreach (Assembly assembly in ExtensionManager.Instance.GetAssembliesList())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == animName)
                    {
                        try
                        {
                            skeleAnimObj.AddComponent(type);
                            CreatureAnimScript animScript = skeleAnimObj.GetComponent<CreatureAnimScript>();
                            if (animScript == null)
                            {
                                LCBaseMod.Instance.MakeErrorLog($"Null CreatureAnimScript for '{animName}'.");
                                return null;
                            }
                            return animScript;
                        }
                        catch (Exception e)
                        {
                            LCBaseMod.Instance.MakeErrorLog($"Failed to add CreatureAnimScript for '{animName}'.");
                            UnityEngine.Debug.LogException(e);
                            return null;
                        }
                    }
                }
            }
            LCBaseMod.Instance.MakeErrorLog($"CreatureAnimScript '{animName}' not found.");
            return null;
        }

    }
    public class LCBM_Tools_Spine
    {

























    }



    }





