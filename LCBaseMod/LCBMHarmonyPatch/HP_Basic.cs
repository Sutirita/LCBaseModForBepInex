using Credit;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCBaseMod.LCBMHarmonyPatch
{

    class HP_Basic
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

}
