using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using XNALib;
using System;

namespace QTD
{
    public class SettingsMgr
    {
        public static SettingsMgr Instance;
        const string Path = "Config/Settings.xml";
#if XBOX
        IsolatedStorageFile FileStorage = IsolatedStorageFile.GetUserStoreForApplication();
#endif

        #region Settings
        public bool IsFullScreen = true;
        public bool AlwaysShowHPBars = true;
        public bool ShowBountyValues = true;
        #endregion

        public SettingsMgr()
        {
#if WINDOWS
            if (File.Exists(Path))
#endif
#if XBOX
            if(FileStorage.FileExists(Path))
#endif
                Load();
            else
                Save();
        }

        
        public void Save()
        {
            try
            {
                XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement("root"));

                XElement settingsNode = new XElement("Settings");
                doc.Root.Add(settingsNode);

                settingsNode.Add(
                    new XElement("IsFullScreen", IsFullScreen.ToString()),
                    new XElement("AlwaysShowHPBars", AlwaysShowHPBars.ToString()),
                    new XElement("ShowBountyValues", AlwaysShowHPBars.ToString())
                    );


                // Save
#if WINDOWS
            doc.Save(Path, SaveOptions.None);
#endif
#if XBOX
                if (FileStorage.FileExists(Path))
                    FileStorage.DeleteFile(Path);
                IsolatedStorageFileStream stream = FileStorage.CreateFile(Path);
                doc.Save(stream);
                stream.Close();
#endif
            }
            catch { }
        }

        public void Load()
        {
            try
            {
#if WINDOWS
            XDocument doc = XDocument.Load(Path);
#endif
#if XBOX
                StreamReader stream = new StreamReader(new IsolatedStorageFileStream(Path, FileMode.Open, FileStorage));
                XDocument doc = XDocument.Load(stream);
                stream.Close();
#endif

                XElement settingsNode = doc.Root.SelectChildElement("Settings");
                IsFullScreen = bool.Parse(settingsNode.SelectChildElement("IsFullScreen").Value);
                AlwaysShowHPBars = bool.Parse(settingsNode.SelectChildElement("AlwaysShowHPBars").Value);
                ShowBountyValues = bool.Parse(settingsNode.SelectChildElement("ShowBountyValues").Value);
            }
            catch { }
        }
    }
}
