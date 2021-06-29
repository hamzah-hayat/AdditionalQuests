using Bannerlord.BUTR.Shared.Helpers;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Settings.Base.Global;
using System.Collections.Generic;
using TaleWorlds.Localization;
namespace AdditionalQuestsCode.Utils
{
    public class MCMSettings : AttributeGlobalSettings<MCMSettings>
    {
        #region ModSettingsStandard
        public override string Id => Statics.InstanceID;

        // Build mod display name with name and version form the project properties version
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        string modName = Statics.DisplayName;
        public override string DisplayName => TextObjectHelper.Create("{=AdditionalQuestsModDisplayName}" + modName + " {VERSION}", new Dictionary<string, TextObject>()
        {
            { "VERSION", TextObjectHelper.Create(typeof(MCMSettings).Assembly.GetName().Version?.ToString(3) ?? "")! }
        })!.ToString();
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

        public override string FolderName => Statics.ModuleFolder;
        public override string FormatType => Statics.FormatType;

        public bool LoadMCMConfigFile { get; set; } = false;
        public string ModDisplayName { get { return DisplayName; } }
        #endregion

        //~ Mod Specific settings 

        //~ Headsman Needs Militia Weapons
        #region Headsman Needs Militia Weapons
        [SettingPropertyBool("{=AQC-HNMW}Headmans Needs Militia Weapons" + "*", Order = 0, RequireRestart = true, IsToggle = true,
            HintText = "{=AQC-HNMW}Enables the Headmans Needs Militia Weapons Quest."),
            SettingPropertyGroup("{=AQC-HNMW}Headsman Needs Militia Weapons")] //, GroupOrder = 0
        public bool HeadsmanNeedsMilitiaWeaponsActive { get; set; } = true;
        #endregion

        //~ Starving Town Needs Food
        #region Starving Town Needs Food
        [SettingPropertyBool("{=AQC-STNF}Starving Town Needs Food" + "*", Order = 1, RequireRestart = true, IsToggle = true,
            HintText = "{=AQC-STNF}Enables the Starving Town Needs Food Quest."),
            SettingPropertyGroup("{=AQC-STNF}Starving Town Needs Food")] //, GroupOrder = 1
        public bool StarvingTownNeedsFoodActive { get; set; } = true;
        #endregion

        //~ Town Uprising Issue
        #region Town Uprising Issue
        [SettingPropertyBool("{=AQC-TUI}Town Uprising Issue" + "*", Order = 2, RequireRestart = true, IsToggle = true,
            HintText = "{=AQC-TUI}Enables the Town Uprising Issue Quest."),
            SettingPropertyGroup("{=AQC-TUI}Town Uprising Issue")] //, GroupOrder = 2
        public bool TownUprisingIssueActive { get; set; } = true;
        #endregion

        //~ Village Bandit Army
        #region Village Bandit Army
        [SettingPropertyBool("{=AQC-VBA}Village Bandit Army" + "*", Order = 3, RequireRestart = true, IsToggle = true,
            HintText = "{=AQC-VBA}Enables the Village Bandit Army Quest."),
            SettingPropertyGroup("{=AQC-VBA}Village Bandit Army")] //, GroupOrder = 3
        public bool VillageBanditArmyActive { get; set; } = true;
        #endregion
    }
}