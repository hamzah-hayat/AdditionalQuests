using Bannerlord.BUTR.Shared.Helpers;
using AdditionalQuestsCode.Utils;
using System.Reflection;

namespace AdditionalQuestsCode
{
    public static class Statics
    {
        public static MCMSettings? _settings;
        public const string ModuleFolder = "AdditionalQuestsCode";
        public const string InstanceID = ModuleFolder;
        public const string DisplayName = "AdditionalQuestsCode";
        public const string FormatType = "json";
        public const string logPath = @"..\\..\\Modules\\" + ModuleFolder + "\\AdditionalQuestsCodeLogs.txt";
        public const string ConfigFilePath = @"..\\..\\Modules\\" + ModuleFolder + "\\config.json";
        public static string PrePrend { get; set; } = DisplayName;
        public const string HarmonyId = ModuleFolder + ".harmony";
        public static string GameVersion = ApplicationVersionHelper.GameVersionStr();
        public static string ModVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        #region MCMConfigValues
        public static string? MCMConfigFolder { get; set; }
        public static string? MCMConfigFile { get; set; }
        public static bool MCMConfigFileExists { get; set; } = false;
        public static bool MCMModuleLoaded { get; set; } = false;
        public static bool ModConfigFileExists { get; set; } = false;
        #endregion

        //~DEBUG
        //public static bool Debug { get; set; } = true;
        //public static bool LogToFile { get; set; } = true;
    }
}
