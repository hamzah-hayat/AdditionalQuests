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
    public class StarvingTownNeedsFoodIssueBehavior : CampaignBehaviorBase
    {
        // Needs to be village notable with a bandit base in range of 1225
        private bool ConditionsHold(Hero issueGiver)
        {
            return issueGiver.IsMerchant && issueGiver.CurrentSettlement != null && issueGiver.CurrentSettlement.IsTown && issueGiver.CurrentSettlement.IsStarving;
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
            return new StarvingTownNeedsFoodIssueBehavior.StarvingTownNeedsFoodIssue(issueOwner);
        }


        // Now the Issue
        internal class StarvingTownNeedsFoodIssue : IssueBase
        {
            public StarvingTownNeedsFoodIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(20f))
            {
            }

            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{SETTLEMENT} needs food", null);
                    textObject.SetTextVariable("SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject Description
            {
                get
                {
                    TextObject textObject = new TextObject("The Town of {SETTLEMENT} is starving, {MERCHANT_NAME} needs help to restock food and help the people.", null);
                    textObject.SetTextVariable("SETTLEMENT", base.IssueSettlement.Name);
                    textObject.SetTextVariable("MERCHANT_NAME", base.IssueOwner.Name);
                    return textObject;
                }
            }

            public override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    return new TextObject("The situation is bad, the town is starving, we've had to ration out all the food coming into the city but its getting harder and harder each day for the people. If this keeps up for much longer, we will either all starve to death or the people will riot.", null);
                }
            }

            public override TextObject IssueAcceptByPlayer
            {
                get
                {
                    return new TextObject("Can I help? I have some food to spare.", null);
                }
            }

            public override TextObject IssueQuestSolutionExplanationByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("I thank you for your kindness, but we require a much larger amount of food to restock the city, enough to keep us going while we sort out the source of our food shortage. If you can supply Grain, Meat and Fish, I will pay you double for each you can bring us. Don't worry, the nobles are paying for this, so it won't be out of the peoples pocket.", null);
                    //textObject.SetTextVariable("SPEARS_AMOUNT", this.NeededHardWoodAmount);
                    return textObject;
                }
            }

            public override TextObject IssueQuestSolutionAcceptByPlayer
            {
                get
                {
                    return new TextObject("I will head out to find food for the town.", null);
                }
            }

            public override TextObject IssueAsRumorInSettlement
            {
                get
                {
                    TextObject textObject = new TextObject("I'm so hungry... oh, sorry, Did I walk into you? Its been so hard lately... I haven't eaten in so long... If only someone was helping {QUEST_GIVER.NAME} to bring more food into town...", null);
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

        internal class StarvingTownNeedsFoodQuest : QuestBase
        {
            public StarvingTownNeedsFoodQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
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
        public class StarvingTownNeedsFoodIssueTypeDefiner : SaveableTypeDefiner
        {
            public StarvingTownNeedsFoodIssueTypeDefiner() : base(1000501)
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
