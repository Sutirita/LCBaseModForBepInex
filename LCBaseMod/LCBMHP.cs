
using BepInEx.Core.Logging.Interpolation;
using CommandWindow;
using CreatureInfo;
using Credit;
using HarmonyLib;
using Inventory;
using nightowl.DistortionShaderPack;
using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace LCBaseMod
{
    class BMHP_ConsoleScript
    {
        //重写控制台逻辑

        [HarmonyPrefix, HarmonyPatch(typeof(ConsoleScript), "OnExitEdit")]
        public static bool HP_OnExitEdit(string command, ConsoleScript __instance)
        {
            ConsoleScript.instance.ConsoleWnd.gameObject.SetActive(value: false);
            char[] separator = new char[] { ' ', '.' };
            if (BMConfigManager.instance.cfg_RewriteConsole.Value)
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



    class BMHP_Basic
    {
        //BM版本号
        [HarmonyPostfix, HarmonyPatch(typeof(NewTitleScript), "Start")]
        public static void HP_NTCS(NewTitleScript __instance)
        {
            __instance.GameVersionChecker.text += LCBaseMod.Instance.GetVertionStr();
        }



        [HarmonyPostfix, HarmonyPatch(typeof(AlterTitleController), "Start")]
        public static void HP_ATCS(AlterTitleController __instance)
        {
            __instance.GameVersionChecker.text += LCBaseMod.Instance.GetVertionStr();
        }

















        //从设置菜单移除RO的名字
        [HarmonyPostfix, HarmonyPatch(typeof(OptionUI), "Awake")]
        public static void RemoveRO()
        {
            string[] _credit = (string[])Traverse.Create(OptionUI.Instance).Field("credit").GetValue();
            _credit[0] = _credit[0].Replace(" / Ro", "");
            Dictionary<string, string> dic = (Dictionary<string, string>)Traverse.Create(OptionUI.Instance).Field("creditText").GetValue();
            dic["cn"] = _credit[0];
            Traverse.Create(OptionUI.Instance).Field("creditText").SetValue(dic);
        }

        //从感谢名单移除RO的名字
        [HarmonyPostfix, HarmonyPatch(typeof(GameStaticDataLoader), "LoadCreditData")]
        public static void RemoveROInCredit()
        {

            List<CreditSection> creditSection = CreditManager.instance.list;

            for (int i = 0; i < creditSection.Count; i++)
            {
                List<CreditItem> list2 = creditSection[i].list;
                for (int j = 0; j < list2.Count; j++)
                {
                    if (list2[j].name == "Ro")
                    {
                        creditSection[i].list.Remove(list2[j]);
                        CreditManager.instance.Init(creditSection);
                        LCBaseMod.Instance.MakeMessageLog("Ro is Removed.");
                        return;
                    }
                }
            }

        }


    }


    class BMHP_GamePlay
    {

        //重写研究加载（因为我不想看月记的一堆LOG）
        [HarmonyPrefix, HarmonyPatch(typeof(GameStaticDataLoader), "LoadResearchDescData")]
        public static bool HP_LoadResearchDescData(List<ResearchItemTypeInfo> research)
        {
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
                Dictionary<string, ResearchItemDesc> desc = null;
                if (dictionary.TryGetValue(researchItemTypeInfo.id, out desc))
                {
                    researchItemTypeInfo.desc = desc;
                }
            }



            return false;
        }


        //------------显示小数-----------

        //伤害抗性显示小数
        [HarmonyPrefix, HarmonyPatch(typeof(UIUtil), "DefenseSetFactor")]
        public static bool HP_DefenseSetFactor(DefenseInfo defenseInfo, UnityEngine.UI.Text[] text, bool bracket = false)
        {
            int acc = BMConfigManager.instance.cfg_Precision_Defense.Value;
            string format = bracket ? "({0:F" + acc + "})" : "{0:F" + acc + "}";

            text[0].text = string.Format(format, defenseInfo.R);
            text[1].text = string.Format(format, defenseInfo.W);
            text[2].text = string.Format(format, defenseInfo.B);
            text[3].text = string.Format(format, defenseInfo.P);
            return false;
        }


        //图鉴工作成功率

        [HarmonyPrefix, HarmonyPatch(typeof(CreatureInfoWorkSlot), "SetWorkSuccess", new Type[] { })]
        public static bool HP_CreatureInfoWorkSlot_SWS(CreatureInfoWorkSlot __instance)
        {
            UnityEngine.UI.Text[] levelSuccessPercentage = (UnityEngine.UI.Text[])Traverse.Create(__instance).Field("levelSuccessPercentage").GetValue();
            RwbpType _type = (RwbpType)Traverse.Create(__instance).Field("_type").GetValue();
            for (int i = 0; i < levelSuccessPercentage.Length; i++)
            {
                UnityEngine.UI.Text text = levelSuccessPercentage[i];
                float num = new CreatureInfoController().MetaInfo.workProbTable.GetWorkProb(_type, i + 1);
                string percentText = UICommonTextConverter.GetPercentText(num);
                text.text = percentText;
            }
            return false;
        }


        //工作成功率小数格式
        [HarmonyPrefix, HarmonyPatch(typeof(UICommonTextConverter), "GetPercentText", new Type[] { typeof(float) })]
        public static bool HP_UICommonTextConverter(ref string __result, float rate)
        {
            int acc = BMConfigManager.instance.cfg_Precision_WorkSuccess.Value;
            __result = string.Format("{0:F" + acc + "}", (rate * 100f)) + "%";

            return false;

        }


        //工作时间显示小数
        [HarmonyPostfix, HarmonyPatch(typeof(WorkData), "CheckCurrentSkill")]

        public static void HP_WDCheckCurrentSkill(WorkData __instance)
        {

            SkillTypeInfo current = Traverse.Create(__instance).Field<SkillTypeInfo>("_current").Value;
            if (current == null)
            {
                return;
            }
            AgentModel currentAgent = Traverse.Create(__instance).Field<AgentModel>("_currentAgent").Value;
            if (currentAgent == null)
            {
                return;
            }
            CreatureModel currentCreature = Traverse.Create(__instance).Field<CreatureModel>("_currentCreature").Value;
            if (currentCreature == null)
            {
                return;
            }

            //_currentCreature
            string prefix = "work_";
            string[] region = new string[] { "r", "w", "b", "p" };
            int acc = BMConfigManager.instance.cfg_Precision_WorkSuccess.Value;
            int num = (int)current.id;
            if (num != 6 && num != 7)
            {
                if (currentCreature.observeInfo.GetObserveState(prefix + region[num - 1]))
                {
                    float ws = (float)currentCreature.metaInfo.feelingStateCubeBounds.GetLastBound() / (currentCreature.GetCubeSpeed() * (1f + (float)(currentCreature.GetObserveBonusSpeed() + currentAgent.workSpeed) / 100f));
                    __instance.WorkSpeed.text = string.Format("{0:F" + acc + "}s", ws);

                }
            }
            else if (num == 7)
            {
                if (currentCreature.observeInfo.GetObserveState(prefix + region[1]))
                {

                    float ws = (float)currentCreature.metaInfo.feelingStateCubeBounds.GetLastBound() / (currentCreature.GetCubeSpeed() * (1f + (float)(currentCreature.GetObserveBonusSpeed() + currentAgent.workSpeed) / 100f));
                    __instance.WorkSpeed.text = string.Format("{0:F" + acc + "}s", ws);

                }
            }
        }





        //---显示伤害效果的小数部分---


        //伤害特效显示小数
        [HarmonyPrefix, HarmonyPatch(typeof(UnitModel), "MakeDamageEffect")]
        public static bool HP_MakeDamageEffect(RwbpType type, float value, DefenseInfo.Type defense, UnitModel __instance)
        {
            if (ResearchDataModel.instance.IsUpgradedAbility("damage_text") || (GlobalGameManager.instance.gameMode == GameMode.TUTORIAL && GlobalGameManager.instance.tutorialStep > 1))
            {
                MovableObjectNode mov = __instance.GetMovableNode();
                DamageEffect effect = DamageEffect.Invoker(mov);
                Color inner = Color.white;
                Color outter = Color.white;
                Color white = Color.white;
                Color white2 = Color.white;
                if (defense == DefenseInfo.Type.NONE)
                {
                    effect.DefenseTypeInner.gameObject.SetActive(false);
                    effect.DamageCount.rectTransform.anchoredPosition =
                        new Vector2(-83f, effect.DamageCount.rectTransform.anchoredPosition.y);
                    effect.Icon.rectTransform.anchoredPosition =
                        new Vector2(5f, effect.Icon.rectTransform.anchoredPosition.y);
                    effect.IconOut.rectTransform.anchoredPosition = effect.Icon.rectTransform.anchoredPosition;
                    effect.DefenseTypeText.enabled = false;
                }
                else
                {
                    if (defense == DefenseInfo.Type.IMMUNE)
                    {
                        effect.DamageCount.gameObject.SetActive(false);
                    }
                    else
                    {
                        effect.DamageCount.gameObject.SetActive(true);
                    }

                    effect.DefenseTypeInner.gameObject.SetActive(true);
                    int num = 0;
                    switch (defense)
                    {
                        case DefenseInfo.Type.SUPER_WEAKNESS:
                            num = 0;
                            break;
                        case DefenseInfo.Type.WEAKNESS:
                            num = 1;
                            break;
                        case DefenseInfo.Type.RESISTANCE:
                            num = 3;
                            break;
                        case DefenseInfo.Type.IMMUNE:
                            num = 2;
                            break;
                        case DefenseInfo.Type.ENDURE:
                            num = 4;
                            break;
                    }
                    effect.DefenseTypeText.sprite = effect.DamageTextImage[num];
                    effect.DefenseTypeInner.sprite = effect.DamageFontTexture[num];
                    effect.DefenseTypeText.SetNativeSize();
                    effect.DefenseTypeInner.SetNativeSize();
                }
                UIColorManager.instance.GetRWBPTypeColor(type, out white, out white2);
                Sprite sprite = effect.DamageIcon[(int)type];
                Sprite sprite2 = effect.DamageIconOut[(int)type];
                if (__instance != null)
                {
                    int acc = BMConfigManager.instance.cfg_Precision_Dmg.Value;
                    if (type == RwbpType.P && __instance is WorkerModel)
                    {
                        effect.DamageCount.text = string.Format("{0:F" + acc + "}", value);
                    }
                    else
                    {
                        effect.DamageCount.text = string.Format("{0:F" + acc + "}", value);
                    }
                }
                effect.Icon.sprite = sprite;
                effect.IconOut.sprite = sprite2;
                Graphic frame = effect.Frame;
                Color color = white;
                effect.DamageCount.color = color;
                effect.DamageContext.color = color;
                frame.color = color;
                Graphic fill = effect.Fill;
                color = white2;
                effect.DamageCountOutline.effectColor = color;
                fill.color = color;
                effect.IconOut.color = white2;
                effect.DefenseTypeText.color = white;
                effect.DefenseTypeInner.color = white2;

                Vector2 b = Vector2.zero;
                switch (type)
                {
                    case RwbpType.N:
                        effect.IconOut.rectTransform.anchoredPosition = effect.Icon.rectTransform.anchoredPosition;
                        break;
                    case RwbpType.R:
                        b = new Vector2(-2.5f, 9.6f);
                        break;
                    case RwbpType.W:
                        b = new Vector2(-3f, 6.8f);
                        break;
                    case RwbpType.B:
                        b = new Vector2(-3.1f, 0f);
                        break;
                    case RwbpType.P:
                        b = new Vector2(-3.1f, 0f);
                        break;
                }
                effect.IconOut.rectTransform.anchoredPosition = effect.Icon.rectTransform.anchoredPosition + b;
                effect.Icon.color = Color.white;
                effect.Dettach();
                effect.transform.localPosition = new Vector3(effect.transform.localPosition.x + UnityEngine.Random.Range(-0.5f, 0.5f),
                    effect.transform.localPosition.y, effect.transform.localPosition.z);
            }
            return false;
        }


        //异想体单位
        [HarmonyPrefix, HarmonyPatch(typeof(CreatureModel), "TakeDamage")]
        public static bool HP_CreatureModelTkDmg_Pre(UnitModel actor, DamageInfo dmg, CreatureModel __instance)
        {
            dmg = dmg.Copy();
            if (!__instance.script.CanTakeDamage(actor, dmg))
            {
                return false;
            }
            if (__instance.Unit.model.Equipment.armor != null)
            {
                __instance.Unit.model.Equipment.armor.OnTakeDamage(actor, ref dmg);
            }
            if (__instance.Unit.model.Equipment.weapon != null)
            {
                __instance.Unit.model.Equipment.weapon.OnTakeDamage(actor, ref dmg);
            }
            __instance.Unit.model.Equipment.gifts.OnTakeDamage(actor, ref dmg);
            float num = 0f;
            float num2 = 1f;
            if (actor != null)
            {
                num2 = UnitModel.GetDmgMultiplierByEgoLevel(actor.GetAttackLevel(), __instance.GetDefenseLevel());
            }
            num2 *= __instance.GetBufDamageMultiplier(actor, dmg);
            num2 *= __instance.script.GetDamageFactor(actor, dmg);
            if (dmg.type == RwbpType.R || dmg.type == RwbpType.W)
            {
                num = dmg.GetDamageWithDefenseInfo(__instance.defense) * num2;
            }
            else if (dmg.type == RwbpType.B)
            {
                num = dmg.GetDamageWithDefenseInfo(__instance.defense) * num2;
            }
            else if (dmg.type == RwbpType.P)
            {
                num = dmg.GetDamageWithDefenseInfo(__instance.defense) * num2;
            }
            else if (dmg.type == RwbpType.N)
            {
                num = dmg.GetDamageWithDefenseInfo(__instance.defense) * num2;
            }
            if (__instance.hp > 0f)
            {
                if (num >= 0f)
                {
                    float hp = __instance.hp;
                    __instance.hp -= num;
                    __instance.MakeDamageEffect(dmg.type, num, __instance.defense.GetDefenseType(dmg.type));
                    if (dmg.type == RwbpType.R || dmg.type == RwbpType.B || num > 1f)
                    {
                        __instance.MakeSpatteredBlood();
                    }
                    __instance.script.OnTakeDamage(actor, dmg, num);
                }
                else if (num < 0f)
                {
                    float num4 = -num;
                    float num5 = (float)__instance.maxHp - __instance.hp;
                    if (num5 >= num4)
                    {
                        __instance.hp += num4;
                    }
                    else
                    {
                        __instance.hp = (float)__instance.maxHp;
                    }
                    GameObject gameObject = Prefab.LoadPrefab("Effect/RecoverHP");
                    gameObject.transform.SetParent(__instance.Unit.animTarget.transform);
                    gameObject.transform.localPosition = Vector3.zero;
                    gameObject.transform.localScale = Vector3.one;
                    gameObject.transform.localRotation = Quaternion.identity;
                }
            }
            __instance.hp = Mathf.Clamp(__instance.hp, 0f, (float)__instance.maxHp);
            if (__instance.state == CreatureState.ESCAPE && __instance.hp <= 0f)
            {
                __instance.Suppressed();
            }
            if (__instance.Unit.model.Equipment.armor != null)
            {
                __instance.Unit.model.Equipment.armor.OnTakeDamage_After(num, dmg.type);
            }
            if (__instance.Unit.model.Equipment.weapon != null)
            {
                __instance.Unit.model.Equipment.weapon.OnTakeDamage_After(num, dmg.type);
            }
            __instance.Unit.model.Equipment.gifts.OnTakeDamage_After(num, dmg.type);

            return false;

        }


        //职员单位
        [HarmonyPrefix, HarmonyPatch(typeof(WorkerModel), "TakeDamage")]

        public static bool HP_WorkerModelTkDmg_Pre(UnitModel actor, DamageInfo dmg, WorkerModel __instance)
        {
            dmg = dmg.Copy();

            List<BarrierBuf> barrierBufList = (List<BarrierBuf>)Traverse.Create(__instance).Field("_barrierBufList").GetValue();


            if (__instance.invincible && !__instance.HasUnitBuf(UnitBufType.OTHER_WORLD_PORTRAIT_VICTIM))
            {
                return false;
            }
            if (__instance.IsDead())
            {
                return false;
            }
            if (__instance.Equipment.armor != null)
            {
                __instance.Equipment.armor.OnTakeDamage(actor, ref dmg);
            }
            if (__instance.Equipment.weapon != null)
            {
                __instance.Equipment.weapon.OnTakeDamage(actor, ref dmg);
            }
            __instance.Equipment.gifts.OnTakeDamage(actor, ref dmg);
            float num = 1f;
            if (actor != null)
            {
                num = UnitModel.GetDmgMultiplierByEgoLevel(actor.GetAttackLevel(), __instance.GetDefenseLevel());
            }
            num *= __instance.GetBufDamageMultiplier(actor, dmg);
            float num2 = dmg.GetDamageWithDefenseInfo(__instance.defense) * num;
            if (dmg.type == RwbpType.R)
            {
                foreach (BarrierBuf barrierBuf in barrierBufList.ToArray())
                {
                    num2 = barrierBuf.UseBarrier(RwbpType.R, num2);
                }
                float hp = __instance.hp;
                __instance.hp -= num2;
                if (__instance.Equipment.kitCreature != null)
                {
                    __instance.Equipment.kitCreature.script.kitEvent.OnTakeDamagePhysical(__instance, num2);
                }
                __instance.MakeDamageEffect(RwbpType.R, num2, __instance.defense.GetDefenseType(RwbpType.R));
                float num3 = (float)__instance.maxHp;
                if (num3 > 0f && UnityEngine.Random.value < num2 * 2f / num3)
                {
                    __instance.MakeSpatteredBlood();
                }
            }
            else if (dmg.type == RwbpType.W)
            {
                foreach (BarrierBuf barrierBuf2 in barrierBufList.ToArray())
                {
                    num2 = barrierBuf2.UseBarrier(RwbpType.W, num2);
                }
                float num4 = __instance.mental;
                float value2;
                if (__instance.CannotControll() && __instance.unconAction is Uncontrollable_RedShoes)
                {
                    num4 = __instance.hp;
                    __instance.hp -= num2;
                    value2 = (float)((int)__instance.hp - (int)num4);
                    if (__instance.Equipment.kitCreature != null)
                    {
                        __instance.Equipment.kitCreature.script.kitEvent.OnTakeDamagePhysical(__instance, num2);
                    }
                }
                else if (__instance.IsPanic() && !(__instance is OfficerModel) && actor is WorkerModel && !((WorkerModel)actor).CannotControll() && !((WorkerModel)actor).IsPanic())
                {
                    __instance.mental += num2;
                    value2 = __instance.mental - num4;
                }
                else if (__instance is OfficerModel)
                {
                    __instance.mental -= num2;
                    __instance.hp -= num2;
                }
                else
                {
                    __instance.mental -= num2;
                    if (__instance.Equipment.kitCreature != null)
                    {
                        __instance.Equipment.kitCreature.script.kitEvent.OnTakeDamageMental(__instance, num2);
                    }
                }
                __instance.MakeDamageEffect(RwbpType.W, num2, __instance.defense.GetDefenseType(RwbpType.W));
            }
            else if (dmg.type == RwbpType.B)
            {
                foreach (BarrierBuf barrierBuf3 in barrierBufList.ToArray())
                {
                    num2 = barrierBuf3.UseBarrier(RwbpType.B, num2);
                }
                float hp2 = __instance.hp;
                __instance.hp -= num2;
                if (__instance.IsPanic() && !(__instance is OfficerModel) && actor is WorkerModel && !((WorkerModel)actor).CannotControll() && !((WorkerModel)actor).IsPanic())
                {
                    __instance.mental += num2;
                }
                else
                {
                    __instance.mental -= num2;
                    if (__instance.Equipment.kitCreature != null)
                    {
                        __instance.Equipment.kitCreature.script.kitEvent.OnTakeDamageMental(__instance, num2);
                    }
                }
                __instance.MakeDamageEffect(RwbpType.B, num2, __instance.defense.GetDefenseType(RwbpType.B));
                float num5 = (float)__instance.maxHp;
                if (num5 > 0f && UnityEngine.Random.value < num2 * 2f / num5)
                {
                    __instance.MakeSpatteredBlood();
                }
            }
            else if (dmg.type == RwbpType.P)
            {
                float num6 = num2 / 100f;
                num2 = (float)__instance.maxHp * num6;
                foreach (BarrierBuf barrierBuf4 in barrierBufList.ToArray())
                {
                    num2 = barrierBuf4.UseBarrier(RwbpType.P, num2);
                }
                float hp3 = __instance.hp;
                __instance.hp -= num2;
                if (__instance.Equipment.kitCreature != null)
                {
                    __instance.Equipment.kitCreature.script.kitEvent.OnTakeDamagePhysical(__instance, num2);
                }
                __instance.MakeDamageEffect(RwbpType.P, num2, __instance.defense.GetDefenseType(RwbpType.P));
            }
            else if (dmg.type == RwbpType.N)
            {
                float hp4 = __instance.hp;
                __instance.hp -= num2;
                if (__instance.Equipment.kitCreature != null)
                {
                    __instance.Equipment.kitCreature.script.kitEvent.OnTakeDamagePhysical(__instance, num2);
                }
                __instance.MakeDamageEffect(RwbpType.N, num2, DefenseInfo.Type.NONE);
            }
            if (num2 > 0f && __instance.defense.GetMultiplier(dmg.type) > 1f)
            {
                __instance.AddUnitBuf(new UnderAttackBuf());
            }
            if (__instance.IsPanic() && (__instance.CurrentPanicAction is PanicRoaming || __instance.CurrentPanicAction is PanicOpenIsolate) && actor is AgentModel && !((AgentModel)actor).CannotControll() && !((AgentModel)actor).IsPanic())
            {
                __instance.AddUnitBuf(new PanicUnderAttackBuf());
            }
            __instance.hp = Mathf.Clamp(__instance.hp, 0f, (float)__instance.maxHp);
            __instance.mental = Mathf.Clamp(__instance.mental, 0f, (float)__instance.maxMental);
            if (__instance.invincible)
            {
                return false;
            }
            if (__instance.hp <= 0f)
            {
                if (UnityEngine.Random.value <= 0.25f && __instance.isRealWorker)
                {

                    Traverse.Create(__instance).Field("_revivalHp").SetValue(true);
                }
                else
                {
                    Traverse.Create(__instance).Field("_revivalHp").SetValue(false);
                }
                if (!Traverse.Create(__instance).Field("_revivaledHp").GetValue<bool>() && Traverse.Create(__instance).Field("_revivalHp").GetValue<bool>()
                    && MissionManager.instance.ExistsFinishedBossMission(SefiraEnum.CHESED) && __instance.DeadType != DeadType.EXECUTION)
                {
                    __instance.hp = 1f;
                    Traverse.Create(__instance).Field("_revivaledHp").SetValue(true);
                    Traverse.Create(__instance).Field("_revivalHp").SetValue(false);
                    __instance.RecoverHP((float)__instance.maxHp / 2f);
                }
                else
                {
                    if (dmg.specialDeadSceneEnable)
                    {
                        __instance.SetSpecialDeadScene(dmg.specialDeadSceneName);
                    }
                    __instance.OnDie();
                }
            }
            else if (__instance.IsPanic())
            {
                if (__instance.mental >= (float)__instance.maxMental)
                {
                    __instance.StopPanic();
                }
            }
            else if (__instance.mental <= 0f)
            {
                if (UnityEngine.Random.value <= 0.25f && __instance.isRealWorker)
                {
                    Traverse.Create(__instance).Field("_revivalMental").SetValue(true);
                }

                else
                {
                    Traverse.Create(__instance).Field("_revivalMental").SetValue(false);
                }
                if (
                    !Traverse.Create(__instance).Field("_revivaledMental").GetValue<bool>() &&
                    Traverse.Create(__instance).Field("_revivalMental").GetValue<bool>()
                    && MissionManager.instance.ExistsFinishedBossMission(SefiraEnum.CHESED))
                {
                    Traverse.Create(__instance).Field("_revivaledMental").SetValue(true);
                    Traverse.Create(__instance).Field("_revivalMental").SetValue(false);
                    __instance.RecoverMental((float)__instance.maxMental / 2f);
                }
                else
                {
                    __instance.Panic();
                }
            }
            if (__instance.Equipment.armor != null)
            {
                __instance.Equipment.armor.OnTakeDamage_After(num2, dmg.type);
            }
            if (__instance.Equipment.weapon != null)
            {
                __instance.Equipment.weapon.OnTakeDamage_After(num2, dmg.type);
            }
            __instance.Equipment.gifts.OnTakeDamage_After(num2, dmg.type);
            return false;
        }


        //兔子单位
        [HarmonyPrefix, HarmonyPatch(typeof(RabbitModel), "TakeDamage")]
        public static bool HP_RabbitModelTkDmg(UnitModel actor, DamageInfo dmg, RabbitModel __instance)
        {
            dmg = dmg.Copy();
            List<BarrierBuf> barrierBufList = (List<BarrierBuf>)Traverse.Create(__instance).Field("_barrierBufList").GetValue();
            if (__instance.IsDead())
            {
                return false;
            }
            float num = 1f;
            if (actor != null)
            {
                num = UnitModel.GetDmgMultiplierByEgoLevel(actor.GetAttackLevel(), __instance.GetDefenseLevel());
            }
            num *= __instance.GetBufDamageMultiplier(actor, dmg);
            float num2 = dmg.GetDamageWithDefenseInfo(__instance.defense) * num;
            if (dmg.type == RwbpType.R)
            {
                foreach (BarrierBuf barrierBuf in barrierBufList.ToArray())
                {
                    num2 = barrierBuf.UseBarrier(RwbpType.R, num2);
                }
                float hp = __instance.hp;
                __instance.hp -= num2;
                __instance.MakeDamageEffect(RwbpType.R, num2, __instance.defense.GetDefenseType(RwbpType.R));
            }
            else if (dmg.type == RwbpType.W)
            {
                foreach (BarrierBuf barrierBuf2 in barrierBufList.ToArray())
                {
                    num2 = barrierBuf2.UseBarrier(RwbpType.W, num2);
                }
                float mental = __instance.mental;
                __instance.mental -= num2;
                __instance.MakeDamageEffect(RwbpType.W, num2, __instance.defense.GetDefenseType(RwbpType.W));
            }
            else if (dmg.type == RwbpType.B)
            {
                foreach (BarrierBuf barrierBuf3 in barrierBufList.ToArray())
                {
                    num2 = barrierBuf3.UseBarrier(RwbpType.B, num2);
                }
                float hp2 = __instance.hp;
                __instance.hp -= num2;
                __instance.mental -= num2;
                __instance.MakeDamageEffect(RwbpType.B, num2, __instance.defense.GetDefenseType(RwbpType.B));
            }
            else if (dmg.type == RwbpType.P)
            {
                float num3 = num2 / 100f;
                num2 = (float)__instance.maxHp * num3;
                foreach (BarrierBuf barrierBuf4 in barrierBufList.ToArray())
                {
                    num2 = barrierBuf4.UseBarrier(RwbpType.P, num2);
                }
                float hp3 = __instance.hp;
                __instance.hp -= num2;
                __instance.MakeDamageEffect(RwbpType.P, num2, __instance.defense.GetDefenseType(RwbpType.P));
            }
            else if (dmg.type == RwbpType.N)
            {
                float hp4 = __instance.hp;
                __instance.hp -= num2;
                __instance.MakeDamageEffect(RwbpType.N, num2, DefenseInfo.Type.NONE);
            }
            __instance.hp = Mathf.Clamp(__instance.hp, 0f, (float)__instance.maxHp);
            __instance.mental = Mathf.Clamp(__instance.mental, 0f, (float)__instance.maxMental);
            if (__instance.hp <= 0f)
            {
                __instance.OnDie();
            }
            else if (__instance.mental <= 0f)
            {
                __instance.OnDieByMental();
            }
            return false;
        }

        //------------------------



        //敌对单位hp显示小数

        [HarmonyPostfix, HarmonyPatch(typeof(CreatureUnit), "Update")]
        public static void HP_CreatureUnit(CreatureUnit __instance)
        {
            int acc = BMConfigManager.instance.cfg_Precision_CreatureHP.Value;
            if (__instance.hpSlider.gameObject.activeInHierarchy)
            {
                //移除之前的血量信息
                int startIndex = __instance.escapeCreatureName.text.IndexOf(' ');
                int endIndex = __instance.escapeCreatureName.text.IndexOf(']');
                if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
                {
                    __instance.escapeCreatureName.text = __instance.escapeCreatureName.text.Remove(startIndex, endIndex - startIndex + 1);
                }
                //更新血量
                string curhp = string.Format("{0:F" + acc + "}", __instance.model.hp);
                string _hp = $" [{curhp}/{__instance.model.maxHp}]";
                __instance.escapeCreatureName.text += _hp;
            }


        }


        [HarmonyPostfix, HarmonyPatch(typeof(ChildCreatureUnit), "Update")]
        public static void HP_ChildCreatureUnit(ChildCreatureUnit __instance)
        {
            HP_CreatureUnit(__instance);

        }



        //员工hp,mp显示小数

        [HarmonyPostfix, HarmonyPatch(typeof(AgentSlot), "UpdateUI")]
        public static void HP_AgentSlot(AgentSlot __instance)
        {
            int acc = BMConfigManager.instance.cfg_Precision_AgentHPMP.Value;
            string curhp = string.Format("{0:F" + acc + "}", __instance.CurrentAgent.hp);
            string curmp = string.Format("{0:F" + acc + "}", __instance.CurrentAgent.mental);
            __instance.HealthText.text = $"{curhp}/{__instance.CurrentAgent.maxHp}";
            __instance.MentalText.text = $"{curmp}/{__instance.CurrentAgent.maxMental}";
        }


    }





    class BMHP_Creature
    {

        //异想体池的后置补丁
        [HarmonyPostfix, HarmonyPatch(typeof(CreatureGenerateInfo), "GetAll")]

        public static void HP_CreatureGenerateInfor_GetAll(ref long[] __result)
        {

            List<long> tmplist = new List<long>();
            foreach (Extension ex in ExtensionManager.Instance.GetExtensionList())
            {
                if (!ex.IsActive())
                {
                    continue;
                }
                foreach (XmlDocument CreatureGenXmldoc in ex.GetCreatureGenLib())
                {

                    if (CreatureGenXmldoc.SelectNodes("/All/add") != null)
                    {
                        foreach (XmlNode node in CreatureGenXmldoc.SelectNodes("/All/add"))
                        {
                            try
                            {
                                long _id = long.Parse(node.InnerText);
                                if (!tmplist.Contains(_id) && !__result.Contains(_id))
                                {
                                    tmplist.Add(_id);
                                }
                                else
                                {
                                    LCBaseMod.Instance.MakeWarningLog($"MetaID:{_id} already exsists. mod:{ex.GetName()} ");
                                    continue;
                                }

                            }

                            catch (Exception e)

                            {

                                LCBaseMod.Instance.MakeErrorLog($"Exception in adding CreatureGen at mod:{ex.GetName()} ");

                                Debug.LogException(e);

                                continue;

                            }

                        }

                    }

                }


            }

            __result = __result.Concat(tmplist).ToArray();
        }





        [HarmonyPostfix, HarmonyPatch(typeof(CreatureTypeList), "GetData")]

        public static void HP_CreatureTypeList(ref CreatureTypeInfo __result, long id)
        {
            if (__result == null)
            {
                LCBaseMod.Instance.MakeMessageLog($"ERRid {id}");
            }

        }


        //重写异想体加载
        [HarmonyPrefix, HarmonyPatch(typeof(CreatureDataLoader), "Load")]
        public static bool HP_CreatureDataLoader_Load(CreatureDataLoader __instance)
        {
            if (!EquipmentTypeList.instance.loaded)
            {
                Debug.LogError("LoadCreatureList >> EquipmentTypeList must be loaded. ");
            }


            List<CreatureTypeInfo> __CreatureTypeInfoList = new List<CreatureTypeInfo>();

            List<CreatureSpecialSkillTipTable> __CreatureSpecialSkillTipTableList = new List<CreatureSpecialSkillTipTable>();

            Dictionary<long, int> __specialTipSize = new Dictionary<long, int>();



            LCBM_Tools_CDL.LoadOriginalGameCreature(__instance, ref __CreatureTypeInfoList, ref __CreatureSpecialSkillTipTableList, ref __specialTipSize);

            for (int i = 0; i < ExtensionManager.Instance.GetExtensionList().Count; i++)
            {

                Extension Extension = ExtensionManager.Instance.GetExtensionList()[i];
                if (!Extension.IsActive())
                {
                    continue;
                }

                LCBM_Tools_CDL.LoadExtentionCreature(Extension, __instance, ref __CreatureTypeInfoList, ref __CreatureSpecialSkillTipTableList, ref __specialTipSize);

            }

            CreatureTypeList.instance.Init(__CreatureTypeInfoList.ToArray(), __CreatureSpecialSkillTipTableList.ToArray(), __specialTipSize);

            return false;
        }



        //设置脚本
        [HarmonyPrefix, HarmonyPatch(typeof(CreatureManager), "BuildCreatureModel")]

        public static bool HP_CreatureManager_Bulid(CreatureManager __instance, CreatureModel model, long metadataId, SefiraIsolate roomData, string sefiraNum)
        {

            Dictionary<long, CreatureObserveInfoModel> observeInfoList = (Dictionary<long, CreatureObserveInfoModel>)Traverse.Create(__instance).Field("observeInfoList").GetValue();


            CreatureTypeInfo data = CreatureTypeList.instance.GetData(metadataId);
            if (data == null)
            {

                return false; ;
            }
            object obj = null;
            foreach (Assembly assembly in ExtensionManager.Instance.GetAssembliesList())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == data.script)
                    {
                        obj = Activator.CreateInstance(type);
                    }
                }
            }
            if (obj == null)
            {
                return true;
            }
            model.script = (CreatureBase)obj;
            if (observeInfoList.ContainsKey(metadataId))
            {
                observeInfoList.TryGetValue(metadataId, out model.observeInfo);
            }
            else
            {
                model.observeInfo = new CreatureObserveInfoModel(metadataId);
                observeInfoList.Add(metadataId, model.observeInfo);
            }
            model.sefira = (model.sefiraOrigin = SefiraManager.instance.GetSefira(sefiraNum));
            model.sefiraNum = sefiraNum;
            model.specialSkillPos = roomData.pos;
            model.isolateRoomData = roomData;
            model.metadataId = metadataId;
            model.metaInfo = data;
            if (CreatureTypeList.instance.GetSkillTipData(metadataId) != null)
            {
                model.metaInfo.specialSkillTable = CreatureTypeList.instance.GetSkillTipData(metadataId).GetCopy();
            }
            model.basePosition = new Vector2(roomData.x, roomData.y);
            model.script.SetModel(model);
            model.entryNodeId = roomData.nodeId;
            MapNode nodeById = MapGraph.instance.GetNodeById(roomData.nodeId);
            model.entryNode = nodeById;
            nodeById.connectedCreature = model;
            Dictionary<string, MapNode> dictionary = new Dictionary<string, MapNode>();
            List<MapEdge> list = new List<MapEdge>();
            MapNode mapNode = null;
            PassageObjectModel passageObjectModel = null;
            passageObjectModel = new PassageObjectModel(roomData.nodeId + "@creature", nodeById.GetAreaName(), "Map/Passage/PassageEmpty");
            passageObjectModel.isDynamic = true;
            passageObjectModel.Activate();
            passageObjectModel.scaleFactor = 0.75f;
            passageObjectModel.SetToIsolate();
            passageObjectModel.position = new Vector3(roomData.x, roomData.y, 0f);
            passageObjectModel.type = PassageType.ISOLATEROOM;
            IEnumerator enumerator2 = data.nodeInfo.GetEnumerator();
            try
            {
                while (enumerator2.MoveNext())
                {
                    object obj2 = enumerator2.Current;
                    XmlNode xmlNode = (XmlNode)obj2;
                    string text = roomData.nodeId + "@" + xmlNode.Attributes.GetNamedItem("id").InnerText;
                    float x = model.basePosition.x + float.Parse(xmlNode.Attributes.GetNamedItem("x").InnerText);
                    float y = model.basePosition.y + float.Parse(xmlNode.Attributes.GetNamedItem("y").InnerText);
                    XmlNode namedItem = xmlNode.Attributes.GetNamedItem("type");
                    MapNode mapNode2;
                    if (namedItem != null && namedItem.InnerText == "workspace")
                    {
                        mapNode2 = new MapNode(text, new Vector2(x, y), nodeById.GetAreaName(), passageObjectModel);
                        passageObjectModel.AddNode(mapNode2);
                        model.SetWorkspaceNode(mapNode2);
                    }
                    else if (namedItem != null && namedItem.InnerText == "custom")
                    {
                        mapNode2 = new MapNode(text, new Vector2(x, y), nodeById.GetAreaName(), passageObjectModel);
                        passageObjectModel.AddNode(mapNode2);
                        model.SetCustomNode(mapNode2);
                    }
                    else if (namedItem != null && namedItem.InnerText == "creature")
                    {
                        mapNode2 = new MapNode(text, new Vector2(x, y), nodeById.GetAreaName(), passageObjectModel);
                        passageObjectModel.AddNode(mapNode2);
                        model.SetRoomNode(mapNode2);
                        model.SetCurrentNode(mapNode2);
                    }
                    else
                    {
                        if (namedItem == null || !(namedItem.InnerText == "innerDoor"))
                        {
                            continue;
                        }
                        mapNode = (mapNode2 = new MapNode(text, new Vector2(x, y), nodeById.GetAreaName(), passageObjectModel));
                        passageObjectModel.AddNode(mapNode2);
                        DoorObjectModel doorObjectModel = new DoorObjectModel(string.Concat(new object[]
                        {
                        nodeById,
                        "@",
                        text,
                        "@inner"
                        }), "DoorIsolate", passageObjectModel, mapNode);
                        doorObjectModel.position = new Vector3(mapNode.GetPosition().x, mapNode.GetPosition().y, -0.01f);
                        passageObjectModel.AddDoor(doorObjectModel);
                        mapNode.SetDoor(doorObjectModel);
                        doorObjectModel.Close();
                    }
                    dictionary.Add(text, mapNode2);
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
            PassageObjectModel attachedPassage = nodeById.GetAttachedPassage();
            MapNode mapNode3 = new MapNode(roomData.nodeId + "@outter", new Vector2(nodeById.GetPosition().x, nodeById.GetPosition().y), nodeById.GetAreaName(), attachedPassage);
            string id = roomData.nodeId + "@outterDoor";
            string type2 = "MalkuthDoorMiddle";
            switch (model.sefira.sefiraEnum)
            {
                case SefiraEnum.MALKUT:
                    type2 = "MalkuthDoorMiddle";
                    break;
                case SefiraEnum.YESOD:
                    type2 = "YesodDoorMiddle";
                    break;
                case SefiraEnum.HOD:
                    type2 = "HodDoorMiddle";
                    break;
                case SefiraEnum.NETZACH:
                    type2 = "NetzachDoorMiddle";
                    break;
                case SefiraEnum.TIPERERTH1:
                case SefiraEnum.TIPERERTH2:
                    type2 = "TipherethDoorMiddle";
                    break;
                case SefiraEnum.GEBURAH:
                    type2 = "GeburahDoorMiddle";
                    break;
                case SefiraEnum.CHESED:
                    type2 = "ChesedDoorMiddle";
                    break;
                case SefiraEnum.BINAH:
                    type2 = "BinahDoorMiddle";
                    break;
                case SefiraEnum.CHOKHMAH:
                    type2 = "ChokhmahDoorMiddle";
                    break;
                case SefiraEnum.KETHER:
                    type2 = "KetherDoorMiddle";
                    break;
            }
            DoorObjectModel doorObjectModel2 = new DoorObjectModel(id, type2, attachedPassage, mapNode3);
            doorObjectModel2.position = new Vector3(mapNode3.GetPosition().x, mapNode3.GetPosition().y, -0.01f);
            attachedPassage.AddDoor(doorObjectModel2);
            mapNode3.SetDoor(doorObjectModel2);
            doorObjectModel2.Close();
            attachedPassage.AddNode(mapNode3);
            MapEdge mapEdge = new MapEdge(mapNode3, nodeById, "road");
            list.Add(mapEdge);
            mapNode3.AddEdge(mapEdge);
            nodeById.AddEdge(mapEdge);
            if (mapNode != null)
            {
                MapEdge mapEdge2 = new MapEdge(mapNode3, mapNode, "door", 0.01f);
                doorObjectModel2.Connect(mapNode.GetDoor());
                list.Add(mapEdge2);
                mapNode3.AddEdge(mapEdge2);
                mapNode.AddEdge(mapEdge2);
            }
            dictionary.Add(mapNode3.GetId(), mapNode3);
            if (model.GetCustomNode() == null)
            {
                model.SetCustomNode(model.GetCurrentNode());
            }
            IEnumerator enumerator3 = data.edgeInfo.GetEnumerator();
            try
            {
                while (enumerator3.MoveNext())
                {
                    object obj3 = enumerator3.Current;
                    XmlNode xmlNode2 = (XmlNode)obj3;
                    string text2 = roomData.nodeId + "@" + xmlNode2.Attributes.GetNamedItem("node1").InnerText;
                    string text3 = roomData.nodeId + "@" + xmlNode2.Attributes.GetNamedItem("node2").InnerText;
                    string innerText = xmlNode2.Attributes.GetNamedItem("type").InnerText;
                    MapNode mapNode4 = null;
                    MapNode mapNode5 = null;
                    if (!dictionary.TryGetValue(text2, out mapNode4) || !dictionary.TryGetValue(text3, out mapNode5))
                    {
                        Debug.Log(string.Concat(new string[]
                        {
                        "cannot create edge - (",
                        text2,
                        ", ",
                        text3,
                        ")"
                        }));
                    }
                    XmlNode namedItem2 = xmlNode2.Attributes.GetNamedItem("cost");
                    MapEdge mapEdge3;
                    if (namedItem2 != null)
                    {
                        mapEdge3 = new MapEdge(mapNode4, mapNode5, innerText, float.Parse(namedItem2.InnerText));
                    }
                    else
                    {
                        mapEdge3 = new MapEdge(mapNode4, mapNode5, innerText);
                    }
                    list.Add(mapEdge3);
                    mapNode4.AddEdge(mapEdge3);
                    mapNode5.AddEdge(mapEdge3);
                }
            }
            finally
            {
                IDisposable disposable2;
                if ((disposable2 = (enumerator3 as IDisposable)) != null)
                {
                    disposable2.Dispose();
                }
            }
            MapGraph.instance.RegisterPassage(passageObjectModel);











            return false;
        }





          
        




        [HarmonyPrefix, HarmonyPatch(typeof(CreatureLayer), "AddCreature")]
        public static bool HP_CLAC(CreatureLayer __instance, CreatureModel model)
        {
            if (model == null)
            {
                LCBaseMod.Instance.MakeErrorLog("Model is null.");
                return false;
            }

            if (string.IsNullOrEmpty(model.metaInfo.animSrc))
            {
                LCBaseMod.Instance.MakeErrorLog("Animation source is empty.");
                return false;
            }

            string[] animPathParts = model.metaInfo.animSrc.Split(new char[] { '/' });
            if (animPathParts[0] != "Custom")
            {
                return true;
            }

            CreatureUnit creatureUnit = ResourceCache.instance.LoadPrefab("Unit/CreatureBase").GetComponent<CreatureUnit>();

            if (creatureUnit == null)
            {
                LCBaseMod.Instance.MakeErrorLog("Failed to load CreatureUnit prefab.");
                return false;
            }

            creatureUnit.transform.SetParent(__instance.transform, false);
            creatureUnit.model = model;
            model.SetUnit(creatureUnit);

            SkeletonDataAsset animSkeleData = LCBM_Tools_Anim.FindSkeletonDataAsset(animPathParts[1]);
            if (animSkeleData == null)
            {
                LCBaseMod.Instance.MakeErrorLog($"SkeletonDataAsset '{animPathParts[1]}' not found.");
                return false;
            }

            GameObject skeleAnimObj = SkeletonAnimation.NewSkeletonAnimationGameObject(animSkeleData).gameObject;
            if (skeleAnimObj == null)
            {
                LCBaseMod.Instance.MakeErrorLog($"Failed to create SkeletonAnimation for '{animPathParts[1]}'");
                return false;
            }

            CreatureAnimScript animScript = LCBM_Tools_Anim.AddCreatureAnimScript(skeleAnimObj, animPathParts[1]);
            if (animScript == null)
            {
                LCBaseMod.Instance.MakeErrorLog($"Failed to add CreatureAnimScript for '{animPathParts[1]}'");
                return false;
            }

            creatureUnit.animTarget = animScript;
            skeleAnimObj.transform.SetParent(creatureUnit.transform, false);

            if (!string.IsNullOrEmpty(model.metaInfo.roomReturnSrc))
            {
                GameObject returnObject = Prefab.LoadPrefab(model.metaInfo.roomReturnSrc);
                if (returnObject == null)
                {
                    LCBaseMod.Instance.MakeErrorLog($"Return object prefab '{model.metaInfo.roomReturnSrc}' not found.");
                    return false;
                }

                returnObject.transform.SetParent(creatureUnit.transform);
                returnObject.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
                returnObject.transform.localPosition = new Vector3(0f, -0.2f, 0f);
                returnObject.SetActive(false);
                creatureUnit.returnObject = returnObject;
            }
            else
            {
                creatureUnit.returnObject = creatureUnit.returnSpriteRenderer.gameObject;
                creatureUnit.returnObject.SetActive(false);
            }

            GameObject isolateRoomObj = Prefab.LoadPrefab("IsolateRoom");
            if (isolateRoomObj == null)
            {
                LCBaseMod.Instance.MakeErrorLog("IsolateRoom prefab not found.");
                return false;
            }

            IsolateRoom isolateRoom = isolateRoomObj.GetComponent<IsolateRoom>();
            if (isolateRoom == null)
            {
                LCBaseMod.Instance.MakeErrorLog("IsolateRoom component not found.");
                return false;
            }

            isolateRoomObj.transform.SetParent(__instance.transform, false);
            isolateRoom.RoomSpriteRenderer.sprite = ResourceCache.instance.GetSprite($"Sprites/IsolateRoom/isolate_2");
            isolateRoom.SetCreature(creatureUnit);
            isolateRoom.Init();
            isolateRoomObj.transform.position = model.basePosition;
            creatureUnit.room = isolateRoom;

            List<CreatureUnit> creatureList = Traverse.Create(__instance).Field("creatureList").GetValue<List<CreatureUnit>>();
            creatureList.Add(creatureUnit);
            Traverse.Create(__instance).Field("creatureList").SetValue(creatureList);

            Dictionary<long, CreatureUnit> creatureDic = Traverse.Create(__instance).Field("creatureDic").GetValue<Dictionary<long, CreatureUnit>>();
            creatureDic.Add(model.instanceId, creatureUnit);
            Traverse.Create(__instance).Field("creatureDic").SetValue(creatureDic);



            return false;
   
        }



        private static void AddCreatureToLists(CreatureLayer __instance, CreatureUnit creatureUnit, CreatureModel model)
        {
            List<int> tempIntforSprite = Traverse.Create(__instance).Field("tempIntforSprite").GetValue<List<int>>();
            int roomSpriteIndex = UnityEngine.Random.Range(1, 4);
            tempIntforSprite.Add(roomSpriteIndex);
            Traverse.Create(__instance).Field("tempIntforSprite").SetValue(tempIntforSprite);

            List<CreatureUnit> creatureList = Traverse.Create(__instance).Field("creatureList").GetValue<List<CreatureUnit>>();
            creatureList.Add(creatureUnit);
            Traverse.Create(__instance).Field("creatureList").SetValue(creatureList);

            Dictionary<long, CreatureUnit> creatureDic = Traverse.Create(__instance).Field("creatureDic").GetValue<Dictionary<long, CreatureUnit>>();
            creatureDic.Add(model.instanceId, creatureUnit);
            Traverse.Create(__instance).Field("creatureDic").SetValue(creatureDic);
        }
         










    }




    class BMHP_CreaturePortrait
    {
		
        [HarmonyPostfix,HarmonyPatch(typeof(CreatureSuppressRegion), "SetData")]
        public static void HP_CSR(CreatureSuppressRegion __instance, UnitModel target)
        {
                       
            if (__instance.Portrait.sprite != null)
            {
                return;
            }
            if (!(target is CreatureModel))
            {
                Debug.LogError("Should be Creature");
                return;
            }
            CreatureModel creatureModel = target as CreatureModel;

            Sprite SpriteInResources = ExtensionManager.Instance.GetPortraitSrc(creatureModel.metaInfo._tempPortrait);

            if (SpriteInResources == null)
            {
                SpriteInResources = Resources.Load<Sprite>("Sprites/Unit/creature/NoData");
                //err nodata
            }



            if (creatureModel is ChildCreatureModel && !(creatureModel.script is BossBird) && !(creatureModel.script is WhiteNightSpace.DeathAngelApostle))
            {

                __instance.Portrait.sprite = SpriteInResources;

            }
            else
            {
                if (creatureModel.observeInfo.GetObserveState(CreatureModel.regionName[0]))
                {
                    __instance.Portrait.sprite = SpriteInResources;

                }

                __instance.Portrait.sprite = SpriteInResources;
            }

            if (creatureModel.script is GeburahCoreScript || creatureModel.script is BinahCoreScript)
            {
                __instance.Portrait.sprite = SpriteInResources;
            }
        }


        [HarmonyPostfix, HarmonyPatch(typeof(KitCreatureRegion), "SetData")]
        public static void HP_KCR(KitCreatureRegion __instance, UnitModel target)
        {
            if (__instance.TargetImage.sprite != null)
            {
                return;
            }
            if (!(target is CreatureModel))
            {
                Debug.LogError("Should be creatureModel");
                return;
            }
            CreatureModel creatureModel = target as CreatureModel;

            Sprite SpriteInResources = ExtensionManager.Instance.GetPortraitSrc(creatureModel.metaInfo.portraitSrc);

            int observeCost = creatureModel.observeInfo.GetObserveCost(CreatureModel.regionName[0]);
            if (creatureModel.metaInfo.creatureKitType == CreatureKitType.ONESHOT)
            {
                if (creatureModel.observeInfo.totalKitUseCount >= observeCost)
                {
                    __instance.TargetImage.sprite = SpriteInResources;
                    return;
                }
            }
            else if (creatureModel.observeInfo.totalKitUseTime >= (float)observeCost)
            {

                __instance.TargetImage.sprite = SpriteInResources;
            
                return;
            }

            __instance.TargetImage.sprite = Resources.Load<Sprite>("Sprites/Unit/creature/NoData");
        }


        [HarmonyPostfix, HarmonyPatch(typeof(WorkAllocateRegion), "OnObserved")]
        public static void HP_WAL(WorkAllocateRegion __instance,CreatureModel creature)
		
        {
            if (__instance.Portrait.sprite != null)
            {
                return;
            }
            Sprite SpriteInResources = ExtensionManager.Instance.GetPortraitSrc(creature.metaInfo.portraitSrc);

            __instance.Portrait.sprite = SpriteInResources;
        }



        [HarmonyPostfix, HarmonyPatch(typeof(CreatureInfoCodex), "OnOeverlayEnter")]
        public static void HP_OnOeverlayEnter(CreatureInfoCodex __instance,CreatureTypeInfo typeInfo)
        {
            if (__instance.Portrait.sprite != null)
            {
                return;
            }
            Sprite SpriteInResources = ExtensionManager.Instance.GetPortraitSrc(typeInfo.portraitSrc);
            __instance.Portrait.sprite = SpriteInResources;

        }


        
        [HarmonyPostfix, HarmonyPatch(typeof(CreatureInfoCodexSlot), "Init")]
        public static void HP_Init(CreatureInfoCodexSlot __instance, CreatureTypeInfo typeInfo, CreatureObserveInfoModel observeInfo)
        {
            if (__instance.Portrait.sprite != null)
            {
                return;
            }
            Sprite SpriteInResources = ExtensionManager.Instance.GetPortraitSrc(typeInfo.portraitSrcForcely);

            __instance.Portrait.sprite = SpriteInResources;

        }


        [HarmonyPostfix, HarmonyPatch(typeof(CreatureInfoKitStatRoot), "Initialize", new Type[] { typeof(CreatureModel) })]
        public static void HP_CIKS_Initialize(CreatureInfoKitStatRoot __instance, CreatureModel creature)
        {
            if (__instance.Portrait.sprite != null)
            {
                return;
            }
            Sprite SpriteInResources = ExtensionManager.Instance.GetPortraitSrc(creature.metaInfo.portraitSrcForcely);
            __instance.Portrait.sprite = SpriteInResources;

        }


        [HarmonyPostfix, HarmonyPatch(typeof(CreatureInfoKitStatRoot), "Initialize", new Type[] { })]
        public static void HP_CIKS_Initialize(CreatureInfoKitStatRoot __instance)
        {
            if (__instance.Portrait.sprite != null)
            {
                return;
            }
            Sprite SpriteInResources = ExtensionManager.Instance.GetPortraitSrc(__instance.MetaInfo.portraitSrcForcely);
            __instance.Portrait.sprite = SpriteInResources;
        }


        [HarmonyPostfix, HarmonyPatch(typeof(CreatureInfoStatRoot), "Initialize", new Type[] { typeof(CreatureModel) })]
        public static void HP_CIS_Initialize(CreatureInfoStatRoot __instance, CreatureModel creature)
        {
            if (__instance.Portrait.sprite != null)
            {
                return;
            }
            Sprite SpriteInResources = ExtensionManager.Instance.GetPortraitSrc(creature.metaInfo.portraitSrcForcely);
            __instance.Portrait.sprite = SpriteInResources;
        }


        [HarmonyPostfix, HarmonyPatch(typeof(CreatureInfoStatRoot), "Initialize", new Type[] { })]
        public static void HP_CIS_Initialize(CreatureInfoStatRoot __instance)
        {
            if (__instance.Portrait.sprite != null)
            {
                return;
            }

            Sprite SpriteInResources = ExtensionManager.Instance.GetPortraitSrc(__instance.MetaInfo.portraitSrcForcely);
            __instance.Portrait.sprite = SpriteInResources;
        }



        [HarmonyPostfix, HarmonyPatch(typeof(SefiraPanel.CreaturePortrait), "UpdateCheck")]
        public static void HP_SP_UpdateCheck(SefiraPanel.CreaturePortrait __instance)
        {
            if (!__instance.isInit)
            {
                return;
            }

            if (__instance.portrait.sprite != null)
            {
                return;
            }

            Sprite SpriteInResources = ExtensionManager.Instance.GetPortraitSrc(__instance.creature.metaInfo.portraitSrcForcely);

            if (__instance.creature.metaInfo.creatureWorkType == CreatureWorkType.KIT)
            {
                int observeCost = __instance.creature.observeInfo.GetObserveCost(CreatureModel.regionName[0]);
                if (__instance.creature.metaInfo.creatureKitType == CreatureKitType.ONESHOT)
                {
                    if (__instance.creature.observeInfo.totalKitUseCount >= observeCost)
                    {
                        __instance.portrait.sprite = SpriteInResources;
                    }
                    __instance.portrait.sprite = Resources.Load<Sprite>("Sprites/Unit/creature/NoData");
                }
                else
                {
                    if ((int)__instance.creature.observeInfo.totalKitUseTime >= observeCost)
                    {
                        __instance.portrait.sprite = SpriteInResources;
                    }
                    __instance.portrait.sprite = Resources.Load<Sprite>("Sprites/Unit/creature/NoData");
                }
            }
            else if (__instance.creature.observeInfo.GetObserveState(CreatureModel.regionName[0]))
            {
                __instance.portrait.sprite = SpriteInResources;
            }
            else
            {
                __instance.portrait.sprite = Resources.Load<Sprite>("Sprites/Unit/creature/NoData");
              
            }
        }




    }









    class BMHP_Equipment
    {
        [HarmonyPrefix, HarmonyPatch(typeof(EquipmentDataLoader), "Load")]
        public static bool HP_EquipmentDataLoader_Load(EquipmentDataLoader __instance)
        {
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(InventoryItemController), "Init")]
        public static bool HP_InventoryItemController_Init()
        {
            return true;
        }

    }

}
