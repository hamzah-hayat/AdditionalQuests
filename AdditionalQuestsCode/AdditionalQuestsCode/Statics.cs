using System.Reflection;

namespace AdditionalQuestsCode
{
    public static class Statics
    {
        public const string ModuleFolder = "AdditionalQuestsCode";
        public const string InstanceID = ModuleFolder;
        public const string DisplayName = "AdditionalQuestsCode";
        public const string FormatType = "json";
        public const string logPath = @"..\\..\\Modules\\" + ModuleFolder + "\\AdditionalQuestsCodeLogs.txt";
        public const string ConfigFilePath = @"..\\..\\Modules\\" + ModuleFolder + "\\config.json";
        public static string PrePrend { get; set; } = DisplayName;
        public static string ModVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        //~DEBUG
        //public static bool Debug { get; set; } = true;
        //public static bool LogToFile { get; set; } = true;
    }
}
