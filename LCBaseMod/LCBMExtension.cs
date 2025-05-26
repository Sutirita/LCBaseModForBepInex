using Spine.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using UnityEngine;

namespace LCBaseMod
{
    class ExtensionManager
    {

        private List<Extension> ExtensionList= new List<Extension>();

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
           // Assemblylist.Add(Assembly.LoadFile(Path.Combine(ModRootDir, "OHarmony.dll")));
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
                if(Lib.TryGetValue(src,out Sprite sprite))
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

   public class Extension
    {

        private bool __Active = true;

        private string __ExtensionID="";

        private string __ExtensionDir="";

        private string __ExtensionDirName = "";

        private Dictionary<string,XmlDocument> __LangExtensionInfoLib =

         new Dictionary<string, XmlDocument>
         {
                    { "bg",new XmlDocument()},

                    { "cn",new XmlDocument()},

                    { "cn_tr",new XmlDocument()},

                    { "en",new XmlDocument() },

                    { "es",new XmlDocument() },

                    { "jp",new XmlDocument() },

                    { "kr",new XmlDocument() },

                    { "ru",new XmlDocument() },

                    { "vn",new XmlDocument() },

                    { "fr",new XmlDocument() }
         };



        private List<Assembly> __AssemblyList = new List<Assembly>();




        //
        private List<XmlDocument> __CreatureListLib = new List<XmlDocument>();

        private List<XmlDocument> __CreatureGenLib = new List<XmlDocument>();

        private Dictionary<string, XmlDocument> __CreatureStatLib = new Dictionary<string, XmlDocument>();




        //
        private Dictionary<string, Sprite> __PortraitLib = new Dictionary<string, Sprite>();


        private Dictionary<string,SkeletonDataAsset> __CreatureAnimSkeleDataLib = new Dictionary<string,SkeletonDataAsset>();




        //
        private Dictionary<string, Dictionary<string, XmlDocument>> __LangCreatureInfoLib =

            new Dictionary<string, Dictionary<string, XmlDocument>> 
            {
                { "bg",new Dictionary<string, XmlDocument>()},
                
                { "cn",new Dictionary<string, XmlDocument>()},
                
                { "cn_tr",new Dictionary<string, XmlDocument>()},
                
                { "en",new Dictionary<string, XmlDocument>() },
                
                { "es",new Dictionary<string, XmlDocument>() },
                
                { "jp",new Dictionary<string, XmlDocument>() },
                
                { "kr",new Dictionary<string, XmlDocument>() },
                
                { "ru",new Dictionary<string, XmlDocument>() },
                
                { "vn",new Dictionary<string, XmlDocument>() },
                                
                { "fr",new Dictionary<string, XmlDocument>() }
            };




        public Extension(string path)
        {
            __ExtensionDir = path;
            if (Directory.Exists(__ExtensionDir))
            {
                DirectoryInfo info = new DirectoryInfo(__ExtensionDir);
                __ExtensionDirName = info.Name;
                __ExtensionID = BitConverter.ToString(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(__ExtensionDirName))).Replace("-", "");
                LCBaseMod.Instance.MakeMessageLog($"Detected mod:{__ExtensionDirName}"); //({__ExtensionID})");

                Init(info);
                        
            }

                
        }





        public bool IsActive()
        {
            return __Active;
        }
        public void ActiveMod()
        {
             __Active =true;
        }
        public void ExpireMod()
        {
            __Active = false;
        }

        public string GetName()
        {
            return __ExtensionDirName;
        }

        public XmlDocument GetExtensionInfo(string lang)
        {
            return __LangExtensionInfoLib[lang];

        }




        public List<Assembly> GetAssemblyList()
        {
            return __AssemblyList;
        }




        public List<XmlDocument> GetCreatureListLib()
        {
            return __CreatureListLib;
        }

        public List<XmlDocument> GetCreatureGenLib()
        {
            return __CreatureGenLib;
        }


        public XmlDocument GetCreatureInfo(string lang, string id)
        {
            return __LangCreatureInfoLib[lang][id];

        }

        public XmlDocument GetCreatureStat(string TheCreatureStatName)
        {
            return __CreatureStatLib[TheCreatureStatName];
        }


        public Dictionary<string,Sprite> GetPortraitLib()
        {
            return __PortraitLib;
        }

        public Dictionary<string,SkeletonDataAsset> GetCreatureAnimLibLib()
        {
            return __CreatureAnimSkeleDataLib;
        }







         private void Init(DirectoryInfo ExtensionDirInfo)
        {
            DirectoryInfo[] __ExtensionSubDirInfo = ExtensionDirInfo.GetDirectories();

            foreach (FileInfo FileInfo in ExtensionDirInfo.GetFiles("*.dll", SearchOption.AllDirectories))
            {

                LoadModAssembly(FileInfo);
            }
            for (int i = 0; i < __ExtensionSubDirInfo.Length; i++)           
            {
                    
                switch (__ExtensionSubDirInfo[i].Name)
                    {
                        
                    
                    case ("Info"):

                            /*                 
                             TODO

                            }*/
                            break;


                        
                    case ("CustomEffect"):
                    //TODO
                        break;


                        
                    case ("Equipment"):
                    //TODO
                        break;
                        
                    case ("CreatureAnimation"):
                        LoadCreatureAnimation(__ExtensionSubDirInfo[i]);
                        break;
                    case ("Creature"):
                        LoadCreatureDir(__ExtensionSubDirInfo[i]);
                        break;


                    }
                  

                }






            
        }



        private void LoadModAssembly(FileInfo fileInfo)
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(fileInfo.FullName);
                __AssemblyList.Add(assembly);

            }
            catch (Exception e)
            {
                LCBaseMod.Instance.MakeErrorLog($"Failed to load Assembly:{fileInfo.Name} at{__ExtensionDirName} ,skping...");
                UnityEngine.Debug.LogException(e);
            }
        }




        private void LoadCustomEffect()
        {
            
        }












        //读取动画

        private void LoadCreatureAnimation(DirectoryInfo CreatureAnimDirInfo)
        {
            foreach (DirectoryInfo curdirinfo in CreatureAnimDirInfo.GetDirectories())
            {
                List<Texture2D> TextureList = new List<Texture2D>();

                foreach (FileInfo fileInfo in curdirinfo.GetFiles())
                {
                    // 处理.png文件，加载为Texture2D
                    if (fileInfo.Name.EndsWith(".png"))
                    {
                        byte[] fileData = File.ReadAllBytes(fileInfo.FullName);
                        Texture2D texture = new Texture2D(2, 2); // 创建一个临时纹理
                        if (texture.LoadImage(fileData)) // 加载图片数据
                        {
                           
                            texture.name = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                            TextureList.Add(texture);
                        }
                        else
                        {
                            LCBaseMod.Instance.MakeErrorLog($"Failed to load texture:{fileInfo.Name} at mod {__ExtensionDirName}");
                        }
                    }
                }
                string skeletonStringData = "";
                byte[] skeletonByteData = null;
                string atlasData = "";
                try
                {



                    if (File.Exists(curdirinfo.FullName + "/json.txt"))
                    {
                        //json形式1
                        skeletonStringData = File.ReadAllText(curdirinfo.FullName + "/json.txt");

                    }
                    else if (File.Exists(curdirinfo.FullName + "/skeleton.skel"))
                    {
                        //skel形式1
                        skeletonByteData = File.ReadAllBytes(curdirinfo.FullName + "/skeleton.skel");

                    }
                    else if (File.Exists(curdirinfo.FullName + $"/{curdirinfo.Name}.json"))
                    {
                        //json形式2
                        skeletonStringData = File.ReadAllText(curdirinfo.FullName + $"/{curdirinfo.Name}.json");

                    }
                    else if (File.Exists(curdirinfo.FullName + $"/{curdirinfo.Name}.skel"))
                    {
                        //skel形式2
                        skeletonByteData = File.ReadAllBytes(curdirinfo.FullName + $"/{curdirinfo.Name}.skel");

                    }




                    //两种atlas文件读取
                    if (File.Exists(curdirinfo.FullName + "/atlas.txt"))
                    {

                        atlasData = File.ReadAllText(curdirinfo.FullName + "/atlas.txt");

                    }
                    else if (File.Exists(curdirinfo.FullName + $"/{curdirinfo.Name}.atlas"))
                    {

                        atlasData = File.ReadAllText(curdirinfo.FullName + $"/{curdirinfo.Name}.atlas");
                    }


                    Shader shader = Shader.Find("Spine/Skeleton");

                    Material materialPropertySource = new Material(shader);


                    //atlasData

                    //   AtlasAsset atlasAsset = AtlasAsset.CreateRuntimeInstance(new TextAsset(), TextureList.ToArray(), materialPropertySource, true);

                    AtlasAsset atlasAsset = LCBM_Tools_Spine.CreateAtlaAssetRuntimeInstance(atlasData, TextureList.ToArray(), materialPropertySource, true);

                    //skeletonData
                    SkeletonDataAsset SkeleDataAsset = LCBM_Tools_Spine.CreateSDARuntimeInstanceByString(skeletonStringData, atlasAsset, true, 0.01f);



                    if (SkeleDataAsset != null)
                    {
                        __CreatureAnimSkeleDataLib[curdirinfo.Name] = SkeleDataAsset;

                        LCBaseMod.Instance.MakeInfoLog($"Anim:{"Custom/" + curdirinfo.Name} Loaded.");

                    }
                    else
                    {
                        LCBaseMod.Instance.MakeErrorLog($"Failed to load Anim:{"Custom/" + curdirinfo.Name}.");
                    }



                   

                }
                catch(Exception e)
                {
                    LCBaseMod.Instance.MakeErrorLog($"Failed to load Anim{"Custom/" + curdirinfo.Name}");
                    Debug.LogException(e);
                    continue;
                }




            }
        }













        private void LoadCreatureDir(DirectoryInfo CreatureDirInfo)
        {
            DirectoryInfo[] SubDirInfo = CreatureDirInfo.GetDirectories();
            for(int i = 0; i < SubDirInfo.Length; i++)
            {
                switch (SubDirInfo[i].Name)
                { 


                    case ("CreatureList"):
                        LoadCreatureList(SubDirInfo[i].GetFiles() );
                        break;

                    case ("CreatureGen"):
                        LoadCreatureGen(SubDirInfo[i].GetFiles()
                            );
                        break;

                    case ("CreatureInfo"):
                        LoadCreatureInfo(SubDirInfo[i]);
                        
                        break;

                    case ("Creatures"):
                        LoadCreatureStat(SubDirInfo[i].GetFiles());
                        break;


                    case ("Portrait"):
                        LoadPortraits( SubDirInfo[i].GetFiles());
                        break;

                }
            }

        }












        //读取CL
        private void LoadCreatureList(FileInfo[] FileList)
        {
            foreach (FileInfo _File in FileList)
            {
                if (_File.Name.EndsWith(".xml") || _File.Name.EndsWith(".txt") )
                {
                    XmlDocument doc = new XmlDocument();
                    try
                    {
                        doc.LoadXml(File.ReadAllText(_File.FullName));

                    }
                    catch (Exception e)
                    {
                        LCBaseMod.Instance.MakeErrorLog($"Exception in reading {_File.FullName.Replace(__ExtensionDir, "{ModsDir}:")}");
                        UnityEngine.Debug.LogException(e);
                        continue;
                    }
                    __CreatureListLib.Add(doc);
                }
            }
        }

        private void LoadCreatureGen(FileInfo[] FileList)
        {
            foreach (FileInfo _File in FileList)
            {
                if (_File.Name.EndsWith(".xml") )
                {

                    XmlDocument doc = new XmlDocument();
                    try
                    {
                        doc.LoadXml(File.ReadAllText(_File.FullName));
                        __CreatureGenLib.Add(doc);

                    }
                    catch (Exception e)
                    {
                        LCBaseMod.Instance.MakeErrorLog($"Exception in loading CreatureGen at Mod:{__ExtensionDirName}\n FileName:{_File.Name}");
                        UnityEngine.Debug.LogException(e);
                        continue;
                    }


                }
            }


        }




        //读取Stat数据
        private void LoadCreatureStat(FileInfo[] FileList)
        {
            foreach (FileInfo _File in FileList)
            {
                //识别.xml和.txt
                if (_File.Name.EndsWith(".xml")|| _File.Name.EndsWith(".txt") )
                {
                    //加载文件
                    XmlDocument doc = new XmlDocument();
                    string TheCreatureSrc = "";
                    try
                    {
                        doc.LoadXml(File.ReadAllText(_File.FullName));
                        TheCreatureSrc = Path.GetFileNameWithoutExtension(_File.FullName);

                    }
                    catch (Exception e)
                    {
                        LCBaseMod.Instance.MakeErrorLog($"Exception in loading Creatures at Mod:{__ExtensionDirName}\n FileName:{_File.Name}");
                        UnityEngine.Debug.LogException(e);
                        continue;
                    }

                    //不重复添加
                    if (!__CreatureStatLib.Keys.Contains(TheCreatureSrc))             
                    {
                            __CreatureStatLib[TheCreatureSrc] = doc;
                        
                    }
                    else           
                    {
                        LCBaseMod.Instance.MakeErrorLog($"The {TheCreatureSrc} already exists in CreatureStatLib. Mod:{__ExtensionDirName}");
                        continue;
                    }

                    
                }
            }


        }



        //加载所有语言对应的creatureinfo
        private void LoadCreatureInfo(DirectoryInfo CreatureInfoDir)
        {

            foreach (string L in __LangCreatureInfoLib.Keys)
            {
                if (Directory.Exists(Path.Combine(CreatureInfoDir.FullName,L)))
                {
                    
                    FileInfo[] cur_CreatureInfoFiles = new DirectoryInfo(Path.Combine(CreatureInfoDir.FullName, L)).GetFiles();
                    string moddir = LCBaseMod.Instance.GetModDirPath();
                    foreach (FileInfo _File in cur_CreatureInfoFiles)
                    {
                        if (_File.Extension==".xml")
                        {
                            try
                            {
                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(File.ReadAllText(_File.FullName));
                                
                                XmlNode InfoNode = doc.SelectSingleNode("/creature/info");

                                if (InfoNode.Attributes.GetNamedItem("id").InnerText!= null)
                                {
                                    string _metaid = InfoNode.Attributes.GetNamedItem("id").InnerText;
                                    if (!__LangCreatureInfoLib[L].ContainsKey(_metaid))
                                    { 
                                        __LangCreatureInfoLib[L][_metaid] = doc;
                                    }
                                    else
                                    {

                                        LCBaseMod.Instance.MakeErrorLog($"Exception in reading {_File.FullName.Replace(moddir, "{ModsDir}:")}.");
                                        LCBaseMod.Instance.MakeErrorLog($"metaid:{_metaid} already exsists.");

                                    }
                                }else
                                {
                                    LCBaseMod.Instance.MakeErrorLog($"Exception in reading {_File.FullName.Replace(moddir, "{ModsDir}:")}.");
                                    LCBaseMod.Instance.MakeErrorLog($"metaid no found");
                                }

                            }
                            catch (Exception e)
                            {

                                LCBaseMod.Instance.MakeErrorLog($"Exception in reading {_File.FullName.Replace(moddir, "{ModsDir}:")}");
                                UnityEngine.Debug.LogException(e);
                            }
                        }

                    }
                }

            }

        }





        //读取头图

        private void LoadPortraits(FileInfo[] PortraitFileList)
        {
            foreach (FileInfo F in PortraitFileList)
            {
                // 读取文件数据
                byte[] rawData = File.ReadAllBytes(F.FullName);

                // 创建纹理
                Texture2D T = new Texture2D(256, 256);

                string PortraitSrc = Path.GetFileNameWithoutExtension(F.Name);
                if (!T.LoadImage(rawData))
                {
                    LCBaseMod.Instance.MakeErrorLog($"Failed to load image from file {F.FullName}");
                    continue; // 跳过当前文件
                }

                // 创建 Sprite
                Sprite S = Sprite.Create(T, new Rect(0f, 0f, T.width, T.height), new Vector2(0.5f, 0.5f));
                __PortraitLib["Custom/" + PortraitSrc] = S;
                LCBaseMod.Instance.MakeInfoLog($"Image:{"Custom/" + PortraitSrc} Loaded.");
            }

        }











    }







}
