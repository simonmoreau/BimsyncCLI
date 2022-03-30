using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BimsyncCLI.Models.Bimsync;
using System.Text.Json;
using System.Diagnostics;

namespace BimsyncCLI.Services
{
    public class SettingsService
    {
        private string settingsFileName = "settings.bimsync";
        private string settingsFilePath = "";

        public SettingsService()
        {
            Transforms = new Dictionary<string, Transform>();
        }

        public SettingsService(string fileName)
        {
            Transforms = new Dictionary<string, Transform>();
            settingsFileName = fileName;
            settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), settingsFileName);
            GetSavedSettings(settingsFilePath);
        }

        public bool StayOrthographic { get; set;}
        public bool AlwaysNewView { get; set;}
        public string ViewNameSuffix { get; set;}
        public Token Token { get; set;}
        public Dictionary<string, Transform> Transforms { get; set; }

        private void GetSavedSettings(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    // deserialize JSON directly from a file
                        string file = File.ReadAllText(settingsFilePath);
                        SettingsService deserializedSettings = JsonSerializer.Deserialize<SettingsService>(file);
                        SetValuesFromSettingsObject(deserializedSettings);
                }
                else
                {
                    SetDefaultValues();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Something went wrong while reading the settings. " + ex.Message);
                SetDefaultValues();
            }
        }

        private void SetDefaultValues()
        {
            StayOrthographic = false;
            AlwaysNewView = false;
            ViewNameSuffix = "";
            Token = null;
        }

        private void SetValuesFromSettingsObject(SettingsService settingsService)
        {
            StayOrthographic = settingsService.StayOrthographic;
            AlwaysNewView = settingsService.AlwaysNewView;
            ViewNameSuffix = settingsService.ViewNameSuffix;
            Transforms = settingsService.Transforms;

            Token = settingsService.Token;
        }

        private void WriteSettingsToFile()
        {

            string settingText = JsonSerializer.Serialize<SettingsService>(this);
            File.WriteAllText(settingsFilePath,settingText);
        }

        public void SaveSettings(bool stayOrthographic, bool alwaysNewView, string viewNameSuffix)
        {
            StayOrthographic = stayOrthographic;
            AlwaysNewView = alwaysNewView;
            ViewNameSuffix = viewNameSuffix;
            WriteSettingsToFile();
        }

        public void SaveSettings(bool stayOrthographic, bool alwaysNewView, string viewNameSuffix, Transform transform)
        {
            StayOrthographic = stayOrthographic;
            AlwaysNewView = alwaysNewView;
            ViewNameSuffix = viewNameSuffix;
            
            WriteSettingsToFile();
        }

        public void SetToken(Token token)
        {
            // I must be able to handle error here and around here
            Token = token;
            try
            {
                // This is ugly, but I admit that if I can't write the token to the text file, I just keep it in memory
                WriteSettingsToFile();
            }
            catch
            {
                // Do nothing
            }
        }

        public void ClearToken()
        {
            Token = null;
        }
    }
}
