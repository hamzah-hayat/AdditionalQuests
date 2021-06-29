using Bannerlord.BUTR.Shared.Helpers;
using System.IO;

namespace AdditionalQuestsCode.Utils
{
    class ConfigLoader
    {

        public static void LoadConfig()
        {
            BuildVariables();
            ChechMCMProvider();
            if (Statics._settings is null)
            {
                Logging.MessageError("Failed to load any config provider");
            }
        }
        private static void BuildVariables()
        {
            Helpers.IsMCMLoaded();
            CheckMcmConfig();
        }

        private static void ChechMCMProvider()
        {
            if (Statics.MCMModuleLoaded)
            {
                if (MCMSettings.Instance is not null)
                {
                    Statics._settings = MCMSettings.Instance;
                    Logging.MessageDebug("using MCM");
                } else {
                    Logging.MessageDebug("Problem loading MCM config");
                }
            } else {
                Logging.MessageError("MCM Module is not loaded");
            }
        }

        private static void CheckMcmConfig()
        {
            string RootFolder = System.IO.Path.Combine(FSIOHelper.GetConfigPath(), "ModSettings/Global/" + Statics.ModuleFolder);
            if (Directory.Exists(RootFolder))
            {
                Statics.MCMConfigFolder = RootFolder;
                string fileLoc = System.IO.Path.Combine(RootFolder, Statics.ModuleFolder + ".json");
                if (File.Exists(fileLoc))
                {
                    Statics.MCMConfigFileExists = true;
                    Statics.MCMConfigFile = fileLoc;
                    Logging.MessageDebug("MCM Module Config file found");
                }
            }
        }
    }
}
