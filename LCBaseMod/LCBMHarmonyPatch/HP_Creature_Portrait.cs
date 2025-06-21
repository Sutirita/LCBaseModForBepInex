using CommandWindow;
using CreatureInfo;
using HarmonyLib;
using LCBaseMod.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LCBaseMod.LCBMHarmonyPatch
{
    class HP_Creature_Portrait
    {

        [HarmonyPostfix, HarmonyPatch(typeof(CreatureSuppressRegion), "SetData")]
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

            Sprite SpriteInResources = ExtensionManager.Instance.GetPortraitSrc(creatureModel.metaInfo.portraitSrc);

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
        public static void HP_WAL(WorkAllocateRegion __instance, CreatureModel creature)

        {
            if (__instance.Portrait.sprite != null)
            {
                return;
            }
            Sprite SpriteInResources = ExtensionManager.Instance.GetPortraitSrc(creature.metaInfo.portraitSrc);

            __instance.Portrait.sprite = SpriteInResources;
        }



        [HarmonyPostfix, HarmonyPatch(typeof(CreatureInfoCodex), "OnOeverlayEnter")]
        public static void HP_OnOeverlayEnter(CreatureInfoCodex __instance, CreatureTypeInfo typeInfo)
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




}
