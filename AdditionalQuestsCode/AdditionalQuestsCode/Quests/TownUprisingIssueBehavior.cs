using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace AdditionalQuestsCode.Quests
{
    public class TownUprisingIssueBehavior : CampaignBehaviorBase
    {
        // Needs to be a town notable with low loyalty and security rating
        private bool ConditionsHold(Hero issueGiver)
        {
            if (issueGiver.CurrentSettlement != null && issueGiver.IsArtisan)
            {
                Settlement currentSettlement = issueGiver.CurrentSettlement;
                if (currentSettlement.IsTown)
                {
                    Town town = currentSettlement.Town;
                    return town.Loyalty <= 30 && town.Security <= 50 && issueGiver.IsArtisan;
                }
            }
            return false;
        }

        // If the conditions hold, start this quest, otherwise just add it as a possible quest
        public void OnCheckForIssue(Hero hero)
        {
            if (this.ConditionsHold(hero))
            {
                Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new PotentialIssueData.StartIssueDelegate(this.OnIssueSelected), typeof(VillageBanditArmyRaidIssueBehavior.VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.VeryCommon));
                return;
            }
            Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(VillageBanditArmyRaidIssueBehavior.VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.VeryCommon));
        }

        private IssueBase OnIssueSelected(in PotentialIssueData pid, Hero issueOwner)
        {
            return new TownUprisingIssueBehavior.VillageBanditArmyRaidIssue(issueOwner);
        }


        // Now the Issue
        internal class VillageBanditArmyRaidIssue : IssueBase
        {
            public VillageBanditArmyRaidIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(20f))
            {
            }

            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("The people of {ISSUE_SETTLEMENT} are rebelling!", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject Description
            {
                get
                {
                    TextObject textObject = new TextObject("The people {ISSUE_SETTLEMENT} are angry at their mistreatment by their nobility. They are planning a rebellion!", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("I trust you, so listen closely. The people of {ISSUE_SETTLEMENT} have been mistreated for far to long, so I have been organising a \"replacement\" of the clan that runs this town. Most of the milita are with me, and I have several captains willing to lead the town. We just need help in convincing the garrinson who are still loyal.", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject IssueAcceptByPlayer
            {
                get
                {
                    return new TextObject("I can help with this rebellion, what do you need of me?", null);
                }
            }

            public override TextObject IssueQuestSolutionExplanationByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("We want to make this a bloodless affair, but we will still need to fight the remaining loyalist garrinson here in the town. Meet me here next midnight with your men, and we will start the coup. I will bring my men as well.", null);
                    //textObject.SetTextVariable("SPEARS_AMOUNT", this.NeededHardWoodAmount);
                    return textObject;
                }
            }

            public override TextObject IssueQuestSolutionAcceptByPlayer
            {
                get
                {
                    return new TextObject("Understood, I will meet with you shortly.", null);
                }
            }

            public override TextObject IssueAsRumorInSettlement
            {
                get
                {
                    TextObject textObject = new TextObject("Theres talk around that {QUEST_GIVER.NAME} has convinced the town milita to overthrow the ruling clan. They've denied everything, but I'm not so sure...", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.IssueOwner.CharacterObject, textObject);
                    return textObject;
                }
            }

            public override bool IsThereAlternativeSolution
            {
                get
                {
                    return false;
                }
            }

            public override bool IsThereLordSolution
            {
                get
                {
                    return false;
                }
            }

            public override IssueFrequency GetFrequency()
            {
                return IssueBase.IssueFrequency.VeryCommon;
            }

            public override bool IssueStayAliveConditions()
            {
                throw new NotImplementedException();
            }

            protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill)
            {
                throw new NotImplementedException();
            }

            protected override void CompleteIssueWithTimedOutConsequences()
            {
                throw new NotImplementedException();
            }

            protected override QuestBase GenerateIssueQuest(string questId)
            {
                throw new NotImplementedException();
            }

            protected override void OnGameLoad()
            {
                throw new NotImplementedException();
            }
        }

        internal class VillageBanditArmyRaidQuest : QuestBase
        {
            public VillageBanditArmyRaidQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
            {
            }

            public override TextObject Title => throw new NotImplementedException();

            public override bool IsRemainingTimeHidden => throw new NotImplementedException();

            protected override void InitializeQuestOnGameLoad()
            {
                throw new NotImplementedException();
            }

            protected override void SetDialogs()
            {
                throw new NotImplementedException();
            }
        }

        // Save data goes into this class
        public class VillageBanditArmyRaidIssueTypeDefiner : SaveableTypeDefiner
        {
            public VillageBanditArmyRaidIssueTypeDefiner() : base(1000501)
            {
            }

            protected override void DefineClassTypes()
            {
                base.AddClassDefinition(typeof(VillageBanditArmyRaidIssueBehavior.VillageBanditArmyRaidIssue), 1);
                base.AddClassDefinition(typeof(VillageBanditArmyRaidIssueBehavior.VillageBanditArmyRaidQuest), 2);
            }
        }


        // Register this event to check for issue event
        public override void RegisterEvents()
        {
            CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, new Action<Hero>(this.OnCheckForIssue));
        }

        // Unused Sync Data method?
        public override void SyncData(IDataStore dataStore)
        {
        }

    }
}
