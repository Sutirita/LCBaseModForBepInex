using System.Collections.Generic;
using System.Linq;



namespace LCBaseMod.LCBMConsole
{
    public class BMConsoleManager
    {
        private readonly Dictionary<string, ILCBMCommand> _commands;
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
            
            _commands = new Dictionary<string, ILCBMCommand>();
            Cmdinit();

        }


        public string[] GetCommandList()
        {
            return _instance._commands.Keys.ToArray();

        }
        public void RegisterCommand(string CommandName, ILCBMCommand Command)
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
            if (_commands.TryGetValue(name, out ILCBMCommand command))
            {
                command.Execute(args);
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




            RegisterCommand("addcreature", new AddCreature());
        }





    }













}
