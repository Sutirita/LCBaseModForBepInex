using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LCBaseMod.Extensions
{
    public class ExtensionManager
    {

        private List<Extension> ExtensionList = new List<Extension>();

        private List<Assembly> Assemblylist = new List<Assembly>();

        private static ExtensionManager _instance;

        public static ExtensionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ExtensionManager();
                }

                return _instance;
            }
        }



        public void Init()
        {

            string ModRootDir = LCBaseMod.Instance.GetModDirPath();
            DirectoryInfo[] ModDirInfo = new DirectoryInfo(ModRootDir).GetDirectories();

            for (int i = 0; i < ModDirInfo.Length; i++)
            {
                try
                {
                    Extension ex = new Extension(ModDirInfo[i].FullName);
                    ExtensionList.Add(ex);

                }
                catch (Exception e)
                {
                    LCBaseMod.Instance.MakeErrorLog($"Failed to load mod {ModDirInfo[i].Name},skping...");
                    UnityEngine.Debug.LogException(e);
                    continue;
                }
            }
            LoadAssmblies();
        }

        public List<Extension> GetExtensionList()
        {
            return ExtensionList;
        }




        private void LoadAssmblies()
        {
            foreach (Extension ex in ExtensionList)
            {
                if (!ex.IsActive())
                {
                    continue;
                }
                foreach (Assembly _as in ex.GetAssemblyList())
                {
                    Assemblylist.Add(_as);
                }
            }
            LCBaseMod.Instance.MakeMessageLog($"{Assemblylist.Count} AssemblyFile loaded.");
        }


        public List<Assembly> GetAssembliesList()
        {
            return Assemblylist;
        }


        public Sprite GetPortraitSrc(string src)
        {
            foreach (Extension ex in ExtensionList)
            {
                if (!ex.IsActive())
                {
                    continue;
                }
                Dictionary<string, Sprite> Lib = ex.GetPortraitLib();
                if (Lib.TryGetValue(src, out Sprite sprite))
                {
                    return sprite;
                }
                else
                {
                    continue;
                }
            }
            LCBaseMod.Instance.MakeErrorLog($"PortraitSrc {src} No Found.");
            return Resources.Load<Sprite>("Sprites/Unit/creature/NoData");
        }




        public void CreatePatch()
        {
            foreach (Assembly assembly in GetAssembliesList())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == "Harmony_Patch")
                    {
                        try
                        {
                            Activator.CreateInstance(type);
                        }
                        catch (Exception e)

                        {

                            LCBaseMod.Instance.MakeErrorLog($"HarmonyPatchError in Assembly:{assembly.FullName},skping...");

                            UnityEngine.Debug.LogException(e);

                            continue;

                        }
                    }

                }
            }



        }


    }

    //extention
}
