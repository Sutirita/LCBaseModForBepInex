using HarmonyLib;
using LCBaseMod.Extensions;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

using LCBaseMod.LCBMToolKit;

namespace LCBaseMod.LCBMHarmonyPatch

{

    class HP_Creature
    {


        //烦人东西
        [HarmonyPrefix, HarmonyPatch(typeof(CreatureGenerate.CreatureGenerateInfoManager), "Log")]
        public static bool Log(bool isError)
        {
            return isError;
        }





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
                LCBaseMod.Instance.MakeErrorLog($"CreatureMetaInfo is unavailable! MetaID:{id}");

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


            
            LCBaseMod.Instance.MakeMessageLog("Loading Vanilla CreatureData...");


           
            LCBM_Tools_CDL.LoadVanillaGameCreature(ref __CreatureTypeInfoList, ref __CreatureSpecialSkillTipTableList, ref __specialTipSize);

//            LCBMTool.CreatureDataLoader.LoadVanillaGameCreature(ref __CreatureTypeInfoList, ref __CreatureSpecialSkillTipTableList, ref __specialTipSize);

            int num1 = __CreatureTypeInfoList.Count;
            LCBaseMod.Instance.MakeMessageLog($"Vanilla CreatureData Loaded. (Total:{num1})");




            LCBaseMod.Instance.MakeMessageLog("Loading Mod CreatureData...");
            for (int i = 0; i < ExtensionManager.Instance.GetExtensionList().Count; i++)
            {

                Extension Extension = ExtensionManager.Instance.GetExtensionList()[i];
                if (!Extension.IsActive())
                {
                    continue;
                }

               LCBM_Tools_CDL.LoadExtentionCreature(Extension, ref __CreatureTypeInfoList, ref __CreatureSpecialSkillTipTableList, ref __specialTipSize);
              //  LCBMTool.CreatureDataLoader.LoadExtentionCreature(Extension, ref __CreatureTypeInfoList, ref __CreatureSpecialSkillTipTableList, ref __specialTipSize);

            }


            LCBaseMod.Instance.MakeMessageLog($"Mod CreatureData Loaded. (Total:{__CreatureTypeInfoList.Count - num1})");

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
            passageObjectModel = new PassageObjectModel(roomData.nodeId + "@creature", nodeById.GetAreaName(), "Map/Passage/PassageEmpty")
            {
                isDynamic = true
            };
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
                        }), "DoorIsolate", passageObjectModel, mapNode)
                        {
                            position = new Vector3(mapNode.GetPosition().x, mapNode.GetPosition().y, -0.01f)
                        };
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
            DoorObjectModel doorObjectModel2 = new DoorObjectModel(id, type2, attachedPassage, mapNode3)
            {
                position = new Vector3(mapNode3.GetPosition().x, mapNode3.GetPosition().y, -0.01f)
            };
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
                    MapNode mapNode5 = null;
                    if (!dictionary.TryGetValue(text2, out MapNode mapNode4) || !dictionary.TryGetValue(text3, out mapNode5))
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
            if (model.metaInfo is null)
            {
                LCBaseMod.Instance.MakeErrorLog("MeatInfo is Null");
                return false;
            }




            if (string.IsNullOrEmpty(model.metaInfo.animSrc))
            {
                LCBaseMod.Instance.MakeErrorLog($"Animation source is empty. MetaID:{model.metaInfo.id}");
                return false;
            }





            string[] animPathParts = model.metaInfo.animSrc.Split(new char[] { '/' });
            if (animPathParts[0] != "Custom")
            {
                return true;
            }


            CreatureUnit creatureUnit = ResourceCache.instance.LoadPrefab("Unit/CreatureBase").GetComponent<CreatureUnit>();

            if (creatureUnit != null)
            {
                creatureUnit.transform.SetParent(__instance.transform, false);
                creatureUnit.model = model;
                model.SetUnit(creatureUnit);
            }
            else
            {
                LCBaseMod.Instance.MakeErrorLog("Failed to load CreatureUnit prefab.");
                return false;
            }


            GameObject skeleAnimObj = null;

            CreatureAnimScript animScript = null;

            SkeletonDataAsset animSkeleData = LCBM_Tools_Anim.FindSkeletonDataAsset(animPathParts[1]);







            if (animSkeleData != null)
            {

                skeleAnimObj = SkeletonAnimation.NewSkeletonAnimationGameObject(animSkeleData).gameObject;
                skeleAnimObj.name = animPathParts[1];

            }
            else
            {
                //GameObject gameObject2 = Prefab.LoadPrefab(model.metaInfo.animSrc);
                LCBaseMod.Instance.MakeErrorLog($"SkeletonDataAsset '{animPathParts[1]}' not found. MetaID:{model.metaInfo.id}");
                // skeleAnimObj = Prefab.LoadPrefab("One");

            }


            if (skeleAnimObj != null)
            {
                skeleAnimObj.transform.SetParent(creatureUnit.transform, false);

                animScript = LCBM_Tools_Anim.AddCreatureAnimScript(skeleAnimObj, animPathParts[1]);

                if (animScript != null)
                {

                    creatureUnit.animTarget = animScript;

                }
                else
                {
                    LCBaseMod.Instance.MakeErrorLog($"Failed to add CreatureAnimScript for '{animPathParts[1]}'");
                }

            }
            else
            {
                LCBaseMod.Instance.MakeErrorLog($"Failed to create SkeletonAnimation for '{animPathParts[1]}'");
            }






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







        [HarmonyPrefix, HarmonyPatch(typeof(CreatureObserveInfoModel), "InitData")]


        public static bool HP_COIM(CreatureObserveInfoModel __instance)
        {
            CreatureTypeInfo MetaInfo = (CreatureTypeInfo)Traverse.Create(__instance).Field("_metaInfo").GetValue();
            if (MetaInfo != null)
            {
                __instance.InitObserveRegion(MetaInfo.observeData);
            }
            else
            {

            }
            return false;
        }






        [HarmonyPrefix, HarmonyPatch(typeof(CreatureObserveInfoModel), "IsMaxObserved")]

        public static bool HP_COIM_MO(CreatureObserveInfoModel __instance, ref bool __result)
        {
            CreatureTypeInfo MetaInfo = (CreatureTypeInfo)Traverse.Create(__instance).Field("_metaInfo").GetValue();
            if (MetaInfo != null)
            {
                return true;
            }
            else
            {
                __result = false;
                return false;
            }




        }





    }








}
