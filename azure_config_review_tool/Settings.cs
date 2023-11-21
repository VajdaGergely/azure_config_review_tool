using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.Serialization.NamingConventions;

namespace azure_administration_tool1
{
    public enum YmlRepoSource
    {
        None = 0,
        Azsk = 1,
        Azts = 2
    }

    public class SettingsYmlFile
    {
        public string Dbpath;
        public string Azskymlrepolocation;
        public string Aztsymlrepolocation;
        public string Ymlreposource;
        public string Notepadpluspluslocation;
    }

    public class Settings
    {
        public string SettingsFileLocation { get { return @".\settings.yml"; } }
        private SettingsYmlFile settingsYmlFile;

        public string DbPath { get { return Environment.ExpandEnvironmentVariables(settingsYmlFile.Dbpath); } }
        public string AzskYmlRepoLocation { get { return Environment.ExpandEnvironmentVariables(settingsYmlFile.Azskymlrepolocation); } }
        public string AztsYmlRepoLocation { get { return Environment.ExpandEnvironmentVariables(settingsYmlFile.Aztsymlrepolocation); } }
        private YmlRepoSource YmlRepoSource
        {
            get
            {
                try
                {
                    return (YmlRepoSource)Enum.Parse(typeof(YmlRepoSource), settingsYmlFile.Ymlreposource, true);
                }
                catch
                {
                    return YmlRepoSource.None;
                }
            }
        }

        public string YmlRepoLocation 
        { 
            get 
            { 
                switch(YmlRepoSource)
                {
                    case YmlRepoSource.Azsk:
                        return AzskYmlRepoLocation;
                    case YmlRepoSource.Azts:
                        return AztsYmlRepoLocation;
                    case YmlRepoSource.None:
                    default:
                        return "";
                }
            } 
        }

        public string NotepadPlusPlusLocation { get { return Environment.ExpandEnvironmentVariables(settingsYmlFile.Notepadpluspluslocation); } }

        public Settings()
        {
            var ymlString = File.ReadAllText(SettingsFileLocation);
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();

            try
            {
                settingsYmlFile = deserializer.Deserialize<SettingsYmlFile>(ymlString);
            }
            catch
            {
                MessageBox.Show("Error can't load settings from settings file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show("Settings yml file location: " + Directory.GetCurrentDirectory() + @"\settings.yml", "Debug info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                settingsYmlFile = new SettingsYmlFile()
                {
                    Dbpath = "",
                    Azskymlrepolocation = "",
                    Aztsymlrepolocation = "",
                    Ymlreposource = "",
                    Notepadpluspluslocation = ""
                };
            }

            //print debug info
            string debugDbPath = Environment.ExpandEnvironmentVariables(settingsYmlFile.Dbpath);
            string debugAzskYmlRepo = Environment.ExpandEnvironmentVariables(settingsYmlFile.Azskymlrepolocation);
            string debugAztsYmlRepo = Environment.ExpandEnvironmentVariables(settingsYmlFile.Aztsymlrepolocation);
            string debugNotepadPP = Environment.ExpandEnvironmentVariables(settingsYmlFile.Notepadpluspluslocation);
            
            if (!File.Exists(debugDbPath))
            {
                MessageBox.Show("File not exists: " + debugDbPath, "Debug info", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (!Directory.Exists(debugAzskYmlRepo))
            {
                MessageBox.Show("Folder not exists: " + debugAzskYmlRepo, "Debug info", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (!Directory.Exists(debugAztsYmlRepo))
            {
                MessageBox.Show("Folder not exists: " + debugAztsYmlRepo, "Debug info", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (!File.Exists(debugNotepadPP))
            {
                MessageBox.Show("File not exists: " + debugNotepadPP, "Debug info", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Save()
        {

        }
    }
}
