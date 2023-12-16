using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace AdditionalQuestsCode.Quests
{
    class SupplyMaterialsIssueBehavior : CampaignBehaviorBase
    {
        // Needs to be a town notable that owns a workshop of some kind
        private bool ConditionsHold(Hero issueGiver)
        {
            return issueGiver.IsNotable && issueGiver.OwnedWorkshops.Count > 0;
        }

        // If the conditions hold, start this quest, otherwise just add it as a possible quest
        public void OnCheckForIssue(Hero hero)
        {
            if (this.ConditionsHold(hero))
            {
                Workshop workshop = hero.OwnedWorkshops[0];
                Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new PotentialIssueData.StartIssueDelegate(this.OnIssueSelected), typeof(SupplyMaterialsIssueBehavior.SupplyMaterialsIssue), IssueBase.IssueFrequency.Common, workshop));
                return;
            }
            Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(SupplyMaterialsIssueBehavior.SupplyMaterialsIssue), IssueBase.IssueFrequency.Common));
        }

        private IssueBase OnIssueSelected(in PotentialIssueData pid, Hero issueOwner)
        {
            PotentialIssueData potentialIssueData = pid;
            return new SupplyMaterialsIssueBehavior.SupplyMaterialsIssue(issueOwner, potentialIssueData.RelatedObject as Workshop);
        }

        internal class SupplyMaterialsIssue : IssueBase
        {
            // Issue Vars and constructor
            [SaveableField(1)]
            Workshop questWorkshop;

            public SupplyMaterialsIssue(Hero issueOwner, Workshop workshop) : base(issueOwner, CampaignTime.DaysFromNow(20f))
            {
                IssueOwner = issueOwner;
                questWorkshop = workshop;
            }

            // Here we Store the TextObjects that are used by the Issue
            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQBDTitle}Beasthunter has kidnapped {NOTABLE_NAME}s Son!", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject Description
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQBDDescription}A Beasthunter has kidnapped the son of {NOTABLE_NAME}, demanding a ransom for services he gave in the past.", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    return new TextObject("{=AQBDIssueBrief}Woe is me, my son has been kidnapped! A Beasthunter who saved my life many years ago has come demanding payment. He took my son and says he will take him for good if I do not scrounge together enough money to pay him. You must help me stop him!", null);
                }
            }

            public override TextObject IssueAcceptByPlayer
            {
                get
                {
                    return new TextObject("{=AQBDIssueAccept}How much is the ransom?", null);
                }
            }

            public override TextObject IssueQuestSolutionExplanationByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQBDIssueSolution}He is demanding 2500{GOLD_ICON}, yet all I have is 500. You must help me provide the gold or beat him in a duel and take my son back by force!", null);
                    textObject.SetTextVariable("REWARD_AMOUNT", RewardGold);
                    return textObject;
                }
            }

            public override TextObject IssueQuestSolutionAcceptByPlayer
            {
                get
                {
                    return new TextObject("{=AQBDIssueSolutionAccept}I will see what I can do.", null);
                }
            }

            public override TextObject IssueAsRumorInSettlement
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQBDRumor}There was a massive argument over at {QUEST_GIVER.NAME}s house! I didn't hear the specifics but I saw a strangly dressed man leave afteward, demanding payment in \"gold or blood\". I hope nothing bad has befallen {QUEST_GIVER.NAME}!", null);
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
                return IssueBase.IssueFrequency.Common;
            }

            public override bool IssueStayAliveConditions()
            {
                return IssueOwner.OwnedWorkshops.Contains(questWorkshop);
            }

            protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill)
            {
                relationHero = null;
                flag = IssueBase.PreconditionFlags.None;
                if (issueGiver.GetRelationWithPlayer() < -10f)
                {
                    flag |= IssueBase.PreconditionFlags.Relation;
                    relationHero = issueGiver;
                }
                if (issueGiver.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
                {
                    flag |= IssueBase.PreconditionFlags.AtWar;
                }
                skill = null;
                return flag == IssueBase.PreconditionFlags.None;
            }

            protected override void CompleteIssueWithTimedOutConsequences()
            {
            }

            protected override QuestBase GenerateIssueQuest(string questId)
            {
                return null;
                //return new BeastHunterDuelIssueBehavior.BeastHunterDuelQuest(questId, base.IssueOwner, CampaignTime.DaysFromNow(14f), this.RewardGold);
            }

            protected override void OnGameLoad()
            {
            }

            protected override int RewardGold
            {
                get
                {
                    return 500;
                }
            }

            protected override void HourlyTick()
            {
            }
        }

        //internal class BeastHunterDuelQuest : QuestBase { }


        // Save data goes into this class
        public class SupplyMaterialsIssueTypeDefiner : SaveableTypeDefiner
        {
            public SupplyMaterialsIssueTypeDefiner() : base(585910)
            {
            }

            protected override void DefineClassTypes()
            {
                //base.AddClassDefinition(typeof(BeastHunterDuelIssueBehavior.BeastHunterDuelIssue), 1);
                //base.AddClassDefinition(typeof(BeastHunterDuelIssueBehavior.BeastHunterDuelQuest), 2);
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
