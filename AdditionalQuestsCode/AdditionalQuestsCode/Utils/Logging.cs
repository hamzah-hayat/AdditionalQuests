using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace AdditionalQuestsCode.Utils
{
    public static class Logging
    {
        public static bool logToFile = true;
        public static bool Debug = true;
        public static string PrePrend = "AdditionalQuests";

        private static void ShowMessage(string message, Color messageColor, bool logToFile = false)
        {
            InformationManager.DisplayMessage(new InformationMessage(PrePrend + " : " + message, messageColor));
            if (logToFile)
            {
                logMessage(message);
            }
        }
        private static void logMessage(string message)
        {
            LogToFile(message + "; ModVersion: " + Statics.ModVersion);
        }

        public static void SendMessage(string message)
        {
            ShowMessage(message, Color.ConvertStringToColor("#42FF00FF"), logToFile);
        }

        public static void MessageError(string message)
        {
            ShowMessage(message, Color.ConvertStringToColor("#FF000000"));
        }

        public static void DisplayModHarmonyErrorMessage()
        {
            SendMessage("Will not function properly with out Harmony");
        }

        public static void ColorRedMessage(string message)
        {
            ShowMessage(message, Color.ConvertStringToColor("#FF0042FF"), logToFile);
        }

        public static void ColorGreenMessage(string message)
        {
            ShowMessage(message, Color.ConvertStringToColor("#42FF00FF"), logToFile);
        }

        public static void ColorBlueMessage(string message)
        {
            ShowMessage(message, Color.ConvertStringToColor("#0042FFFF"), logToFile);
        }

        public static void ColorOrangeMessage(string message)
        {
            ShowMessage(message, Color.ConvertStringToColor("#00F16D26"), logToFile);
        }

        public static void QuickInformationMessage(string message)
        {
            InformationManager.AddQuickInformation(new TextObject(message, null), 0, null, "");
        }

        [Conditional("DEBUG")]
        public static void DebugMessage(string message)
        {
            MessageDebug(message);
        }

        public static void MessageDebug(string message)
        {
            if (Debug)
            {
                ShowMessage(message, Color.ConvertStringToColor("#E6FF00FF"), true);
            }
        }
        public static void ShowError(string message, string title = "", Exception? exception = null, bool ShowVersionsInfo = true)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                title = "";
            }
            message += "\n\n" + exception?.ToStringFull();
            if (ShowVersionsInfo)
            {
                message += "\n\nModVersion: " + Statics.ModVersion;
            }
            logMessage(title + "\n" + message);
        }
        public static string ToStringFull(this Exception ex) => ex != null ? GetString(ex) : "";

        private static string GetString(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            GetStringRecursive(ex, sb);
            sb.AppendLine();
            sb.AppendLine("Stack trace:");
            sb.AppendLine(ex.StackTrace);
            return sb.ToString();
        }

        private static void GetStringRecursive(Exception ex, StringBuilder sb)
        {
            while (true)
            {
                sb.AppendLine(ex.GetType().Name + ":");
                sb.AppendLine(ex.Message);
                if (ex.InnerException == null)
                {
                    return;
                }

                sb.AppendLine();
                ex = ex.InnerException;
            }
        }

        public static void LogToFile(string message)
        {
            try
            {
                using StreamWriter sw = File.AppendText(Statics.logPath);
                sw.WriteLine(PrePrend + " : " + DateTime.Now.ToString() + " : " + message + "\r\n");
            }
            catch
            {

            }
        }
    }
}
