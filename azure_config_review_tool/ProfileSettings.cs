using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace azure_administration_tool1
{
    public class ProfileSettingsYmlFile
    {
        public int Windowleft;
        public int Windowtop;
        public int Windowwidth;
        public int Windowheight;
    }

    public class ProfileSettings
    {
        public string ProfileSettingsFileLocation { get { return @".\profile_settings.yml"; } }
        private ProfileSettingsYmlFile profileSettingsYml;

        public int WindowLeft { get{ return profileSettingsYml.Windowleft; } }
        public int WindowTop { get{ return profileSettingsYml.Windowtop; } }
        public int WindowWidth { get{ return profileSettingsYml.Windowwidth; } }
        public int WindowHeight { get { return profileSettingsYml.Windowheight; } }

        public ProfileSettings()
        {
            try
            {
                var ymlString = File.ReadAllText(ProfileSettingsFileLocation);
                var deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
                profileSettingsYml = deserializer.Deserialize<ProfileSettingsYmlFile>(ymlString);
            }
            catch
            {
                MessageBox.Show("Error can't load profile settings from file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show("Profile settings yml file location: " + Directory.GetCurrentDirectory() + @"\profile_settings.yml", "Debug info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                profileSettingsYml = new ProfileSettingsYmlFile()
                {
                    Windowleft = 160,
                    Windowtop = 40,
                    Windowwidth = 1600,
                    Windowheight = 870
                };
            }
        }

        public void Save(int left, int top, int width, int height)
        {
            profileSettingsYml.Windowleft = left;
            profileSettingsYml.Windowtop = top;
            profileSettingsYml.Windowwidth = width;
            profileSettingsYml.Windowheight = height;

            var serializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
            var ymlString = serializer.Serialize(profileSettingsYml);
            File.WriteAllText(ProfileSettingsFileLocation, ymlString);
        }
    }
}
