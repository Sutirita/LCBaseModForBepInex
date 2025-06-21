using GlobalBullet;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using LCBaseMod.LCBMToolKit;

namespace LCBaseMod.LCBMConsole
{

    class CmdHelp : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            foreach (string str in BMConsoleManager.instance.GetCommandList())
            {
                LCBaseMod.Instance.MakeMessageLog(str);
            }
        }
    }



    class BackupSave : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            ToolKit.BackUPSaveData(LCBaseMod.Instance.GetSaveBackUpDirPath());
        }
    }


    class StoryTest : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            ConsoleCommand.instance.BetaStoryTester();
        }
    }


    class GainEnergy : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            float num = 0f;
            try
            {
                num = float.Parse(args[0]);
                EnergyModel.instance.AddEnergy(num);
            }
            catch (Exception)
            {
                Debug.LogError("ERR");
            }
        }
    }


    class GainLobPoint : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            int num = 0;
            try
            {
                num = int.Parse(args[0]);
                MoneyModel.instance.Add(num);
            }
            catch (Exception)
            {
                Debug.LogError("ERR");
            }
        }
    }


    class FillAmmo : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            GlobalBulletManager.instance.currentBullet = GlobalBulletManager.instance.maxBullet;

        }
    }


    class CreateEquipment : ILCBMCommand
    {
        public void Execute(string[] arg)
        {
            int id;
            id = int.Parse(arg[0]);

            Notice.instance.Send(NoticeName.MakeEquipment, new object[]
            {
                     InventoryModel.Instance.CreateEquipment(id)
        });
            Debug.Log($"Equipment:{id} Created.");

        }

    }


    class DestroyEquipments : ILCBMCommand
    {
        public void Execute(string[] arg)
        {
            int id;
            id = int.Parse(arg[0]);
            InventoryModel im = InventoryModel.Instance;
            List<EquipmentModel> _EL = Traverse.Create(im).Field("_equipList").GetValue<List<EquipmentModel>>();
            _EL.RemoveAll((EquipmentModel x) => x.metaInfo.id == id);
            Traverse.Create(im).Field("_equipList").SetValue(_EL);
        }

    }
    class DestroySingalEquipment : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            int id;
            id = int.Parse(args[0]);
            IList<EquipmentModel> _EL = InventoryModel.Instance.GetAllEquipmentList();
            for (int i = 0; i < _EL.Count;)
            {
                if (_EL[i].metaInfo.id == id)
                {
                    InventoryModel.Instance.RemoveEquipment(_EL[i]);
                    return;
                }
                else
                {
                    i++;
                }
            }
        }
    }





    class AgentList : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            foreach (Sefira sefira in SefiraManager.instance.sefiraList)
            {
                foreach (AgentModel model in sefira.agentList)
                {
                    LCBaseMod.Instance.MakeMessageLog($"Agent:{model.name}({sefira.name}) Id:{model.instanceId}");
                }
            }

            foreach (AgentModel model in AgentManager.instance.GetAgentSpareList())
            {
                LCBaseMod.Instance.MakeMessageLog($"Agent:{model.name}(NO Sefira) Id:{model.instanceId}");
            }

        }
    }

    class MakeDamage : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            int _id = int.Parse(args[0]);
            RwbpType _type = (RwbpType)int.Parse(args[1]);
            DamageInfo _dmg = new DamageInfo(_type, float.Parse(args[2]));
            foreach (OfficerModel model in OfficerManager.instance.GetOfficerList())
            {
                if (model.instanceId == _id)
                {
                    model.TakeDamage(_dmg);
                }
            }


            foreach (AgentModel model in AgentManager.instance.GetAgentList())
            {
                if (model.instanceId == _id)
                {
                    model.TakeDamage(_dmg);
                }
            }
            foreach (CreatureModel model in SefiraManager.instance.GetEscapedCreatures())
            {
                if (model.metaInfo.id == _id)
                {
                    model.TakeDamage(_dmg);
                }
            }
        }
    }






    class CreatureList : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            foreach (Sefira sefira in SefiraManager.instance.sefiraList)
            {
                foreach (CreatureModel model in sefira.creatureList)
                {
                    LCBaseMod.Instance.MakeMessageLog($"Creature:{model.metaInfo.name} Metaid:{model.metaInfo.id}");
                }
            }
        }
    }





    class CreatureSwarp : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            int id1 = int.Parse(args[0]);
            int id2 = int.Parse(args[0]);
            Debug.Log("WIP");
        }
    }

    class CreatureEscape : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            int id = int.Parse(args[0]);
            CreatureModel _model = CreatureManager.instance.FindCreature(id);
            _model.Escape(); ;

        }
    }















    class AddQliphothCounter : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            int id = int.Parse(args[0]);
            CreatureModel _model = CreatureManager.instance.FindCreature(id);
            _model.AddQliphothCounter(); ;

        }
    }

    class SubQliphothCounter : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            int id = int.Parse(args[0]);
            CreatureModel _model = CreatureManager.instance.FindCreature(id);
            _model.SubQliphothCounter(); ;

        }
    }

    class ZeroQliphothCounter : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            int id = int.Parse(args[0]);
            CreatureModel _model = CreatureManager.instance.FindCreature(id);
            if (_model.metaInfo.id == id)
            {
                while (_model.qliphothCounter > 0)
                {
                    _model.SubQliphothCounter(); ;
                }

            }
            return;
        }

    }





    class OverLoadGagueIncrease : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            int num = int.Parse(args[0]);
            ConsoleCommand.instance.OverloadInvoke(num);
        }
    }


    class OverLoadLVIncrease : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            ConsoleCommand.instance.BetaMeltdown();
        }
    }


    class OverloadCreature : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            int id = int.Parse(args[0]);
            int num = int.Parse(args[1]);
            int _lv = CreatureOverloadManager.instance.GetQliphothOverloadLevel();
            foreach (Sefira sefira in SefiraManager.instance.sefiraList)
            {
                foreach (CreatureModel model in sefira.creatureList)
                {
                    if (model.metaInfo.id == id)
                    {

                        model.ActivateOverload(_lv);
                    }
                }
            }
        }
    }


    class ClearOverload : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            throw new NotImplementedException();
        }
    }


    class SefiraBossDesc : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            Color color;
            try
            {
                float r, g, b, a;
                r = float.Parse(args[1]);
                g = float.Parse(args[2]);
                b = float.Parse(args[3]);
                a = float.Parse(args[4]);
                color = new Color(r, g, b, a);
            }
            catch (Exception)
            {
                color = Color.red;
            }
            try
            {
                SefiraBossDescUI sefiraBossDescUI = SefiraBossDescUI.GenDesc(args[0], color, color, 0.3f);
            }
            catch (Exception message)
            {
                Debug.Log(message);
            }
        }
    }



    class BossInvoke : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            ConsoleCommand.instance.SefiraBossInvoke(args[0]);
        }
    }


    class AddCreature : ILCBMCommand
    {
        public void Execute(string[] args)
        {
            try
            {
                long id = long.Parse(args[0]);
                PlayerModel.instance.AddWaitingCreature(id);
            }
            catch
            {

            }

        }
    }




}
