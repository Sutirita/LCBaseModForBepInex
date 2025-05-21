using GlobalBullet;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using static AgentTitleTypeList;
using static CreatureMaxObserve;

namespace LCBaseModForBepinEx
{
    
    public interface IBaseModCommand
    {
       
        void Execute(string[] args);
    }



    public class BMConsoleManager
    {
        private readonly Dictionary<string, IBaseModCommand> _commands;
        private static BMConsoleManager _instance;

        public static BMConsoleManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BMConsoleManager();
                }

                return _instance;
            }
        }
        public BMConsoleManager()
        {
            
            _commands = new Dictionary<string, IBaseModCommand>();
            Cmdinit();

        }
        public void RegisterCommand(string CommandName, IBaseModCommand Command)
        {
            this._commands.Add(CommandName, Command);
        }

        public void ExecuteCommand(string[] inputs)
        {
            if (inputs.Length == 0)
            {
                return;
            }
            string name = inputs[0];
            string[] args = inputs.Skip(1).ToArray();
            if (_commands.TryGetValue(name, out IBaseModCommand command))
            {
                command.Execute(args);
            }

        }




        class CmdHelp : IBaseModCommand
        {
            public void Execute(string[] args)
            {
                foreach (string str in BMConsoleManager.instance._commands.Keys)
                {
                    LCBaseMod.Instance.MakeMessageLog(str);
                }
            }
        }



        class BackupSave : IBaseModCommand
        {
            public void Execute(string[] args)
            {
                LCBaseMod.Instance.BackUPSaveData();
            }
        }


        class StoryTest : IBaseModCommand
        {
            public void Execute(string[] args)
            {
                ConsoleCommand.instance.BetaStoryTester();
            }
        }


        class GainEnergy : IBaseModCommand
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


        class GainLobPoint : IBaseModCommand
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


        class FillAmmo : IBaseModCommand
        {
            public void Execute(string[] args)
            {
                GlobalBulletManager.instance.currentBullet = GlobalBulletManager.instance.maxBullet;

            }
        }


        class CreateEquipment : IBaseModCommand
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


        class DestroyEquipments : IBaseModCommand
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
        class DestroySingalEquipment : IBaseModCommand
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





        class AgentList : IBaseModCommand
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

        class MakeDamage : IBaseModCommand
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






        class CreatureList : IBaseModCommand
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





        class CreatureSwarp : IBaseModCommand
        {
            public void Execute(string[] args)
            {
                int id1 = int.Parse(args[0]);
                int id2 = int.Parse(args[0]);
                Debug.Log("WIP");
            }
        }

        class CreatureEscape : IBaseModCommand
        {
            public void Execute(string[] args)
            {
                int id = int.Parse(args[0]);
                CreatureModel _model = CreatureManager.instance.FindCreature(id);
                _model.Escape(); ;
                     
            }
        }

        class AddQliphothCounter : IBaseModCommand
        {
            public void Execute(string[] args)
            {
                int id = int.Parse(args[0]);
                CreatureModel _model = CreatureManager.instance.FindCreature(id);
                _model.AddQliphothCounter(); ;
                        
            }
        }

        class SubQliphothCounter : IBaseModCommand
        {
            public void Execute(string[] args)
            {
                int id = int.Parse(args[0]);
                CreatureModel _model = CreatureManager.instance.FindCreature(id);
                _model.SubQliphothCounter(); ;
                        
            }
        }

        class ZeroQliphothCounter : IBaseModCommand
        {
            public void Execute(string[] args)
            {
                int id = int.Parse(args[0]);
                CreatureModel _model =CreatureManager.instance.FindCreature(id);
                if (_model.metaInfo.id == id)
                {
                    while (_model.qliphothCounter>0)
                    {
                        _model.SubQliphothCounter(); ;
                    }

                }
                return;
            }
        
        }





        class OverLoadGagueIncrease : IBaseModCommand
        {
            public void Execute(string[] args)
            {
                int num = int.Parse(args[0]);
                ConsoleCommand.instance.OverloadInvoke(num);
            }
        }


        class OverLoadLVIncrease : IBaseModCommand
        {
            public void Execute(string[] args)
            {
                ConsoleCommand.instance.BetaMeltdown();
            }
        }


        class OverloadCreature : IBaseModCommand
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


        class ClearOverload : IBaseModCommand
        {
            public void Execute(string[] args)
            {
                throw new NotImplementedException();
            }
        }


        class SefiraBossDesc : IBaseModCommand
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
                    color = new Color(r,g,b,a);
                }
                catch (Exception)
                {
                    color = Color.red;
                }
                try
                {
                    SefiraBossDescUI sefiraBossDescUI = SefiraBossDescUI.GenDesc(args[0],color,color, 0.3f);
                }
                catch (Exception message)
                {
                    Debug.Log(message);
                }
            }
        }



        class BossInvoke : IBaseModCommand
        {
            public void Execute(string[] args)
            {
                ConsoleCommand.instance.SefiraBossInvoke(args[0]);
            }
        }


        private void Cmdinit()
        {
            RegisterCommand("help", new CmdHelp());
            RegisterCommand("backup" ,new BackupSave());



            RegisterCommand("energy", new GainEnergy());
            RegisterCommand("lob", new GainLobPoint());
            RegisterCommand("reload", new FillAmmo());

            RegisterCommand("forge", new CreateEquipment());
            RegisterCommand("destroyall", new DestroyEquipments());
            RegisterCommand("destory", new DestroySingalEquipment());

            RegisterCommand("storyview", new StoryTest());

            RegisterCommand("creaturelist", new CreatureList());
            RegisterCommand("agentlist", new AgentList());


            RegisterCommand("Damage", new MakeDamage());


            RegisterCommand("creatureswarp", new CreatureSwarp());

            RegisterCommand("escape", new CreatureEscape());
            RegisterCommand("addcounter", new AddQliphothCounter());
            RegisterCommand("subcounter", new SubQliphothCounter());
            RegisterCommand("zerocounter", new ZeroQliphothCounter());

            RegisterCommand("overloadcreature", new OverloadCreature());
            RegisterCommand("overloadgague", new OverLoadGagueIncrease());
            RegisterCommand("meltdown", new OverLoadLVIncrease());



            RegisterCommand("bossbettle", new BossInvoke());

            RegisterCommand("bossdesc", new SefiraBossDesc());




        }





    }


}
