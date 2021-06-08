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
    public class HeadmanNeedsMilitiaWeaponsIssueBehavior : CampaignBehaviorBase
    {
        // Needs to be village notable with less then 20 militia
        private bool ConditionsHold(Hero issueGiver)
        {
            return issueGiver.IsRuralNotable && issueGiver.CurrentSettlement != null && issueGiver.CurrentSettlement.IsVillage && issueGiver.CurrentSettlement.Militia < 20f;
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
            return new HeadmanNeedsMilitiaWeaponsIssueBehavior.HeadmanNeedsMilitiaWeaponsIssue(issueOwner);
        }

        // Now the Issue
        internal class HeadmanNeedsMilitiaWeaponsIssue : IssueBase
        {
            public HeadmanNeedsMilitiaWeaponsIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(20f))
            {
                IssueOwner = issueOwner;
            }

            // Here we Store the TextObjects that are used by the Issue
            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{ISSUE_SETTLEMENT} needs spears for militia", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject Description
            {
                get
                {
                    TextObject textObject = new TextObject("The headman of {ISSUE_SETTLEMENT} needs spears for the militia.", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    return new TextObject("I'm currently organising the militia of the village, however we lack weapons to train and fight with. In particular, we have very few spears in the armoury.", null);
                }
            }

            public override TextObject IssueAcceptByPlayer
            {
                get
                {
                    return new TextObject("How can I help?", null);
                }
            }

            public override TextObject IssueQuestSolutionExplanationByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("I can handle training our men into militia fighters, but I need someone to find spears to fight with, you should be able to buy them in the surrounding towns. You can even make your own, if you are a decent smith. We will need {SPEARS_AMOUNT} spears in total.", null);
                    //textObject.SetTextVariable("SPEARS_AMOUNT", this.NeededHardWoodAmount);
                    return textObject;
                }
            }

            public override TextObject IssueQuestSolutionAcceptByPlayer
            {
                get
                {
                    return new TextObject("I will find the spears you need.", null);
                }
            }

            public override TextObject IssueAsRumorInSettlement
            {
                get
                {
                    TextObject textObject = new TextObject("The young lads of the village have all signed up to the newly forming militia. But I heard {QUEST_GIVER.NAME} doesnt have enough spears for even half of the men to train with, so they've been using wooden sticks to train with. I hope they dont end up having to use those same sticks in a real fight!.", null);
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
                return IssueSettlement.Militia <= 20f;
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
                throw new NotImplementedException();
            }

            protected override void OnGameLoad()
            {
            }
        }

        internal class HeadmanNeedsMilitiaWeaponsQuest : QuestBase
        {
            public HeadmanNeedsMilitiaWeaponsQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
            {
            }

            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{ISSUE_SETTLEMENT} needs spears for militia", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            public override bool IsRemainingTimeHidden
            {
                get
                {
                    return false;
                }
            }

            private TextObject StageOnePlayerAcceptsQuestLog
            {
                get
                {
                    TextObject textObject = new TextObject("{QUEST_GIVER.LINK}, the headman of the {QUEST_SETTLEMENT} asked you to deliver {SPEARS_AMOUNT} spears to {?QUEST_GIVER.GENDER}her{?}him{\\?} for the village militia. This will help boost the number of able militia in the village. \n \n You have agreed to bring them {SPEARS_AMOUNT} spears as soon as possible.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    textObject.SetTextVariable("SPEARS_AMOUNT", this.NeededSpears);
                    return textObject;
                }
            }

            private TextObject StageTwoPlayerHasSpearsLog
            {
                get
                {
                    TextObject textObject = new TextObject("You now have enough spears to complete the quest. Return to {QUEST_SETTLEMENT} to hand them over.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageTimeoutLog
            {
                get
                {
                    TextObject textObject = new TextObject("You have failed to deliver {SPEARS_AMOUNT} spears to the villagers. They wont be able to properly train their militia. The Headman is disappointed.", null);
                    textObject.SetTextVariable("SPEARS_AMOUNT", this.NeededSpears);
                    return textObject;
                }
            }

            private TextObject StageSuccessLog
            {
                get
                {
                    TextObject textObject = new TextObject("You have delivered {SPEARS_AMOUNT} spears to the villagers. Their militia is ready to train and defend the village. The Headman and the villagers are grateful.", null);
                    textObject.SetTextVariable("SPEARS_AMOUNT", this.NeededSpears);
                    return textObject;
                }
            }

            private TextObject StageCancelDueToWarLog
            {
                get
                {
                    TextObject textObject = new TextObject("Your clan is now at war with the {ISSUE_GIVER.LINK}'s lord. Your agreement with {ISSUE_GIVER.LINK} was canceled.", null);
                    StringHelpers.SetCharacterProperties("ISSUE_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            private TextObject StageCancelDueToRaidLog
            {
                get
                {
                    TextObject textObject = new TextObject("{SETTLEMENT_NAME} was raided by someone else. Your agreement with {ISSUE_GIVER.LINK} was canceled.", null);
                    textObject.SetTextVariable("SETTLEMENT_NAME", base.QuestGiver.CurrentSettlement.Name);
                    StringHelpers.SetCharacterProperties("ISSUE_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            protected override void InitializeQuestOnGameLoad()
            {
                this.SetDialogs();
            }

            protected override void SetDialogs()
            {
                throw new NotImplementedException();
            }

            [SaveableField(10)]
            private readonly int NeededSpears;

            [SaveableField(20)]
            private JournalLog PlayerAcceptedQuestLog;

            [SaveableField(30)]
            private JournalLog PlayerHasNeededSpears;
        }

        // Save data goes into this class
        public class HeadmanNeedsMilitiaWeaponsIssueTypeDefiner : SaveableTypeDefiner
        {
            public HeadmanNeedsMilitiaWeaponsIssueTypeDefiner() : base(1000501)
            {
            }

            protected override void DefineClassTypes()
            {
                base.AddClassDefinition(typeof(HeadmanNeedsMilitiaWeaponsIssueBehavior.HeadmanNeedsMilitiaWeaponsIssue), 1);
                base.AddClassDefinition(typeof(HeadmanNeedsMilitiaWeaponsIssueBehavior.HeadmanNeedsMilitiaWeaponsQuest), 2);
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
