using GeburahBoss;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LCBaseMod.LCBMHarmonyPatch
{
    class HP_GamePlay
    {

        //重写研究加载,修复ChesedBUG
        [HarmonyPrefix, HarmonyPatch(typeof(GameStaticDataLoader), "LoadResearchDescData")]
        public static bool HP_LoadResearchDescData(List<ResearchItemTypeInfo> research)
        {


            foreach (ResearchItemTypeInfo researchItemTypeInfo in research)
            {
                if (researchItemTypeInfo.id == 803)
                {
                    researchItemTypeInfo.upgradeInfos[0].specialAbility = new ResearchSpecialAbility
                    {
                        name = "upgrade_recover_bullet"
                    };
                    researchItemTypeInfo.upgradeInfos[0].bulletAility = null;
                }
            }










            // GameStaticDataLoader

            XmlDocument xmlDocument = AssetLoader.LoadExternalXML("Language/ResearchDesc");
            Dictionary<int, Dictionary<string, ResearchItemDesc>> dictionary = new Dictionary<int, Dictionary<string, ResearchItemDesc>>();
            List<string> list = new List<string>();
            XmlNodeList xmlNodeList = xmlDocument.SelectNodes("root/supportLanguage/ln");
            IEnumerator enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object obj = enumerator.Current;
                    XmlNode xmlNode = (XmlNode)obj;
                    list.Add(xmlNode.InnerText);
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
            XmlNodeList xmlNodeList2 = xmlDocument.SelectNodes("root/node");
            IEnumerator enumerator2 = xmlNodeList2.GetEnumerator();
            try
            {
                while (enumerator2.MoveNext())
                {
                    object obj2 = enumerator2.Current;
                    XmlNode xmlNode2 = (XmlNode)obj2;
                    int key = (int)float.Parse(xmlNode2.Attributes.GetNamedItem("id").InnerText);
                    Dictionary<string, ResearchItemDesc> dictionary2 = new Dictionary<string, ResearchItemDesc>();
                    foreach (string text in list)
                    {
                        XmlNode xmlNode3 = xmlNode2.SelectSingleNode(text);
                        string innerText = xmlNode3.SelectSingleNode("name").InnerText;
                        // Debug.Log(innerText);
                        string innerText2 = xmlNode3.SelectSingleNode("current").InnerText;
                        string innerText3 = xmlNode3.SelectSingleNode("short").InnerText;
                        innerText.Trim();
                        innerText2.Trim();
                        dictionary2.Add(text, new ResearchItemDesc
                        {
                            name = innerText,
                            desc = innerText2,
                            shortDesc = innerText3
                        });
                    }
                    dictionary.Add(key, dictionary2);
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
            foreach (ResearchItemTypeInfo researchItemTypeInfo in research)
            {
                if (dictionary.TryGetValue(researchItemTypeInfo.id, out Dictionary<string, ResearchItemDesc> desc))
                {
                    researchItemTypeInfo.desc = desc;
                }
            }



            return false;
        }






        //修复Geburah三阶段索敌

        [HarmonyPrefix, HarmonyPatch(typeof(GeburahBoss.ThirdPhase), "GetNextAction")]
        public static bool HP_GeburahThirdPhaseFix(GeburahBoss.ThirdPhase __instance, ref GeburahAction __result, List<UnitModel> near)
        {

            float moveProb = Traverse.Create(__instance).Field("moveProb").GetValue<float>();
            float spearProb = Traverse.Create(__instance).Field("spearProb").GetValue<float>();
            GeburahAction result;
            if (near.Count == 0 || __instance.geburah.currentPassage == null)
            {
                if (__instance.geburah.currentPassage == null)
                {
                    result = new MoveNodeAction(__instance.geburah, __instance.GetRandomNode());
                }
                else if (UnityEngine.Random.value <= moveProb)
                {
                    result = new MoveNodeAction(__instance.geburah, __instance.GetRandomNode());
                }
                else
                {
                    result = new GeburahIdle(__instance.geburah, UnityEngine.Random.Range(3f, 5f), false);
                }
            }
            else if (__instance.isPrevAttack)
            {
                __instance.isPrevAttack = false;
                result = new GeburahIdle(__instance.geburah, false, GeburahStaticInfo.AttackDelay.GetRandomFloat());
            }
            else
            {
                __instance.isPrevAttack = true;
                bool BloodyTreeFlag = __instance.geburah.CanStartBloodyTree();
                if (BloodyTreeFlag && UnityEngine.Random.value <= spearProb)
                {
                    result = new BloodyTreeThrow(__instance.geburah, false);
                }
                else
                {
                    UnitModel unitModel = near[UnityEngine.Random.Range(0, near.Count)];
                    if (__instance.geburah.IsInRange(unitModel, 10f))
                    {
                        __instance.geburah.LookTarget(unitModel);
                        if (__instance.geburah.IsInRange(unitModel, 6f) && UnityEngine.Random.value <= spearProb + (BloodyTreeFlag ? spearProb : 0f))
                        {
                            result = new DangoAttackAction(__instance.geburah, false);
                        }
                        else
                        {
                            result = new DefaultAttack(__instance.geburah, GeburahStaticInfo.P3_LongBirdAttack.front, GeburahStaticInfo.P3_LongBirdAttack.rear, 1);
                        }
                    }
                    else
                    {
                        result = new ChaseAction(__instance.geburah, unitModel.GetMovableNode(), 8f, true, false);
                    }
                }
            }

            __result = result;

            return false;
        }













    }
}
