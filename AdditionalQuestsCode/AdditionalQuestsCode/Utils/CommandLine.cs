using AdditionalQuestsCode.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static AdditionalQuestsCode.Quests.MilitiaSpearsIssueBehavior;

namespace AdditionalQuestsCode.Utils
{
    class CommandLine
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("uninstall", "additional_quests")]
        public static string UninstallAdditionalQuests(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            {
                return CampaignCheats.ErrorType;
            }

            // Remove the Addidtional Quests Campaign Behaviours
            // Also remove all current Issues as well
            Campaign.Current.CampaignBehaviorManager.RemoveBehavior<MilitiaSpearsIssueBehavior>();
            Campaign.Current.CampaignBehaviorManager.RemoveBehavior<BanditArmyIssueBehavior>();
            Campaign.Current.CampaignBehaviorManager.RemoveBehavior<StarvingTownIssueBehavior>();
            Campaign.Current.CampaignBehaviorManager.RemoveBehavior<TownUprisingIssueBehavior>();

            foreach (var issue in Campaign.Current.IssueManager.Issues.ToList())
            {
                if (issue.Value.GetType().Namespace.Contains("AdditionalQuestsCode"))
                {
                    Campaign.Current.IssueManager.DeactivateIssue(issue.Value);
                }
            }



            return "Uninstall Sucessfull, You may now Save and Exit and uninstall AdditionalQuests";
        }


    }
}