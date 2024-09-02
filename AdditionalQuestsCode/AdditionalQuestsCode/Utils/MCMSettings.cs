using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Base.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime;

namespace AdditionalQuestsCode.Utils
{
    internal sealed class MCMAQSettings : AttributeGlobalSettings<MCMAQSettings> // AttributePerSaveSettings<MCMUISettings> AttributePerCampaignSettings<MCMUISettings>
    {

        public override string Id => "Additional_Quests";
        public override string DisplayName => $"AdditionalQuestsCode {typeof(MCMAQSettings).Assembly.GetName().Version.ToString(3)}";
        public override string FolderName => "Additional Quests";
        public override string FormatType => "json";

        // Settings for each kind of Quest
        // Bandit Army
        [SettingPropertyBool("Quest Enabled", Order = 0, RequireRestart = false, HintText = "Enables the Bandit Army quest.")]
        [SettingPropertyGroup("Quests/Bandit Army")]
        public bool BanditArmy_Enabled { get; set; } = true;

        [SettingPropertyBool("Remove minimum troop requirement", Order = 1, RequireRestart = false, HintText = "Removes the minimum requirement of 50 troops to start the Bandit Army quest.")]
        [SettingPropertyGroup("Quests/Bandit Army")]
        public bool BanditArmy_MinTroopReqDisabled { get; set; } = false;

        [SettingPropertyFloatingInteger("Bandit Army Size", 0.5f, 5f, "#0%", Order = 2, RequireRestart = false, HintText = "Changes size of Bandit Army party.")]
        [SettingPropertyGroup("Quests/Bandit Army")]
        public float BanditArmy_SizeMultiplier { get; set; } = 1;

        // Militia Spears
        [SettingPropertyBool("Quest Enabled", Order = 0, RequireRestart = false, HintText = "Enables the Militia Spears quest.")]
        [SettingPropertyGroup("Quests/Militia Spears")]
        public bool MilitiaSpears_Enabled { get; set; } = true;

        // Starving Town
        [SettingPropertyBool("Quest Enabled", Order = 0, RequireRestart = false, HintText = "Enables the Starving Town quest.")]
        [SettingPropertyGroup("Quests/Starving Town")]
        public bool StarvingTown_Enabled { get; set; } = true;

        // Town Uprising
        [SettingPropertyBool("Quest Enabled", Order = 0, RequireRestart = false, HintText = "Enables the Town Uprising quest.")]
        [SettingPropertyGroup("Quests/Town Uprising")]
        public bool TownUprising_Enabled { get; set; } = true;
    }
}