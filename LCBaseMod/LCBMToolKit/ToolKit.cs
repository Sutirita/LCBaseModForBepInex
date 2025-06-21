using LCBaseMod.LCBMConsole;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static CreatureMaxObserve;
using static System.Net.Mime.MediaTypeNames;

namespace LCBaseMod.LCBMToolKit
{
    class ToolKit
    {
        public static byte[] StreamToBytes(Stream stream)
        {

            byte[] bytes = new byte[stream.Length];

            stream.Read(bytes, 0, bytes.Length);

            stream.Seek(0, SeekOrigin.Begin);

            return bytes;

        }
        public static Sprite CreateSprite(Stream stream,int Width=256,int Height =256)
        {
            Texture2D val = new Texture2D(Width, Height);


            val.LoadImage(StreamToBytes(stream));


            return Sprite.Create(val, new Rect(0f, 0f, val.width, val.height), new Vector2(0.5f, 0.5f));
        }

        public static Sprite CreateSprite(byte[] bytes,int Width = 256, int Height = 256)
        {
            Texture2D val = new Texture2D(Width, Height);
            val.LoadImage(bytes);
            return Sprite.Create(val, new Rect(0f, 0f, val.width, val.height), new Vector2(0.5f, 0.5f));
        }




        //存档备份函数

        public static void BackUPSaveData(string _SaveBackUpDirPath)
        {
            LCBaseMod.Instance.MakeMessageLog("SaveBackUp Start...");
            // 源文件夹路径
            string sourceFolderPath = UnityEngine.Application.persistentDataPath;

            // 获取当前时间并格式化为文件夹名称
            string currentTimeFolderName = "SaveBackup_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss");

            // 创建目标文件夹路径
            string BKFolderPath = Path.Combine(_SaveBackUpDirPath, currentTimeFolderName);

            // 如果目标文件夹不存在，则创建它
            if (!Directory.Exists(BKFolderPath))
            {
                Directory.CreateDirectory(BKFolderPath);
            }

            // 获取源文件夹中的所有文件
            string[] files = Directory.GetFiles(sourceFolderPath, "*.*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                // 获取目标文件的完整路径
                string targetFilePath = file.Replace(sourceFolderPath, BKFolderPath);

                // 确保目标文件夹存在
                string targetDirectory = Path.GetDirectoryName(targetFilePath);
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                // 复制文件
                File.Copy(file, targetFilePath, true); // true表示如果目标文件已存在，则覆盖它
            }
            // 获取源文件夹中的所有子文件夹（包括空文件夹）
            string[] subDirectories = Directory.GetDirectories(sourceFolderPath, "*.*", SearchOption.AllDirectories);

            foreach (string subDirectory in subDirectories)
            {
                // 获取目标子文件夹的完整路径
                string targetSubDirectory = subDirectory.Replace(sourceFolderPath, BKFolderPath);

                // 确保目标子文件夹存在
                if (!Directory.Exists(targetSubDirectory))
                {
                    Directory.CreateDirectory(targetSubDirectory);
                }
            }

            LCBaseMod.Instance.MakeMessageLog(string.Format("SaveData has been Copied to : \n{0}", BKFolderPath));
        }







    }

    class CustomConversation
    {
        private void SetConversation(Sprite PortraitSprite, Color FrameColor, string TheSpeakerName)
        {

            GameObject AngelaUI = GameObject.Find("AngelaUI(Conversation)");

            Transform FrameColored = AngelaUI.transform.GetChild(1).GetChild(0);

            UnityEngine.UI.Image ConversationImg = FrameColored.GetComponent<UnityEngine.UI.Image>();

            ConversationImg.color = FrameColor;


            Transform Portrait = FrameColored.GetChild(0);
            UnityEngine.UI.Image PortraiImg = Portrait.GetComponent<UnityEngine.UI.Image>();

            PortraiImg.sprite = PortraitSprite;

            Transform SpeakerName = FrameColored.GetChild(1);
            UnityEngine.UI.Text SpeakerNameText = SpeakerName.GetComponent<UnityEngine.UI.Text>();
            SpeakerNameText.text = TheSpeakerName;   // "Sutirita";

        }

        public void SefiraConverSaation(SefiraEnum sefiraenum)
        {
            Sefira sefira = SefiraManager.instance.GetSefira(sefiraenum);
            Sprite SefiraPortraitSprite = Resources.Load<Sprite>("sprites/story/portrait/angelaportrait");
            string __Name = sefira.name.Substring(0, 1).ToUpper() + sefira.name.Substring(1); ;
            Color color = UIColorManager.instance.GetSefiraColor(sefira).imageColor;
            switch (sefiraenum)
            {
                case (SefiraEnum.MALKUT):
                    SefiraPortraitSprite = Resources.Load<Sprite>("sprites/story/portrait/malkuthportrait");
                    break;
                case (SefiraEnum.YESOD):
                    SefiraPortraitSprite = Resources.Load<Sprite>("sprites/story/portrait/yesodportrait");
                    break;
                case (SefiraEnum.HOD):
                    SefiraPortraitSprite = Resources.Load<Sprite>("sprites/story/portrait/hodportrait");
                    break;
                case (SefiraEnum.NETZACH):
                    SefiraPortraitSprite = Resources.Load<Sprite>("sprites/story/portrait/netzachportrait");
                    break;
                case (SefiraEnum.TIPERERTH1):
                    SefiraPortraitSprite = Resources.Load<Sprite>("sprites/story/portrait/tipherethaportrait");
                    __Name = "Tiphereth";
                    break;
                case (SefiraEnum.TIPERERTH2):
                    SefiraPortraitSprite = Resources.Load<Sprite>("sprites/story/portrait/tipherethbportrait");
                    __Name = "Tiphereth";
                    break;
                case (SefiraEnum.GEBURAH):
                    SefiraPortraitSprite = Resources.Load<Sprite>("sprites/story/portrait/geburahportrait");
                    break;
                case (SefiraEnum.CHESED):
                    SefiraPortraitSprite = Resources.Load<Sprite>("sprites/story/portrait/chesedportrait");
                    break;
                case (SefiraEnum.BINAH):
                    SefiraPortraitSprite = Resources.Load<Sprite>("sprites/story/portrait/binahportrait");
                    break;
                case (SefiraEnum.CHOKHMAH):
                    SefiraPortraitSprite = Resources.Load<Sprite>("sprites/story/portrait/chokmahportrait");
                    __Name = "Hoknma";
                    break;
                case (SefiraEnum.KETHER):
                    SefiraPortraitSprite = Resources.Load<Sprite>("sprites/story/portrait/ayinportrait");
                    __Name = "Ayin";
                    break;
                case (SefiraEnum.DAAT):
                    SefiraPortraitSprite = Resources.Load<Sprite>("sprites/story/portrait/angelaportrait");
                    break;
                default:
                     SefiraPortraitSprite = Resources.Load<Sprite>("sprites/story/portrait/angelaportrait");
                    break;
            }
            SetConversation(SefiraPortraitSprite,color, __Name);
        }

        public void ResetConverSaation()
        {
            Sprite AngelaPortrait = Resources.Load<Sprite>("sprites/story/portrait/angelaportrait");

            Color FrameColor = Color.white;
            ColorUtility.TryParseHtmlString("#80CCFFFF", out FrameColor);
            SetConversation(AngelaPortrait, FrameColor, "Angela");
        }

        public void AddMessage(string Message)
        {
            AngelaConversationUI.instance.AddAngelaMessage(Message);

        }
    }


    class CustomReport
    {
        public void SendSefiraReport(SefiraEnum sefiraEnum ,string Desc)
        {
            Sefira sefira = SefiraManager.instance.GetSefira(sefiraEnum);
            string name = sefira.name;
            Sprite sefiraPortrait = CharacterResourceDataModel.instance.GetSefiraPortrait(sefira.sefiraEnum, false);
            Color color = CharacterResourceDataModel.instance.GetColor(name);
            SefiraConversationController.Instance.UpdateConversation(sefiraPortrait, color, Desc);
        }

        public void SendCustomReport(Sprite Portrait,string HtmlColor,string Desc)
        {
            Color _Color = Color.white;
            ColorUtility.TryParseHtmlString(HtmlColor ,out _Color);
            SefiraConversationController.Instance.UpdateConversation(Portrait, _Color, Desc);
        }

        

    }







}
