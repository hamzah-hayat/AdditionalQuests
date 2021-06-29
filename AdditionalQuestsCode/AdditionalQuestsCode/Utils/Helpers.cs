using System.Linq;
using TaleWorlds.Engine;

namespace AdditionalQuestsCode.Utils
{   
    public class Helpers
    {
        public static bool IsMCMLoaded()
        {
            bool loaded = false;
            var modnames = Utilities.GetModulesNames().ToList();
            if (modnames.Contains("Bannerlord.MBOptionScreen"))// && !overrideSettings
            {
                Statics.MCMModuleLoaded = true;
                loaded = true;
                Logging.MessageDebug("MCM Module is loaded");
            }
            return loaded;
        }

        public static bool IsHarmonyLoaded()
        {
            bool loaded = false;
            var modnames = Utilities.GetModulesNames().ToList();
            if (modnames.Contains("Bannerlord.Harmony"))
            {
                loaded = true;
                Logging.MessageDebug("Harmony Module is loaded");
            }
            else
            {
                Logging.MessageError("Requires Harmony please install the Harmony mod");
            }
            return loaded;
        }
    }
}
