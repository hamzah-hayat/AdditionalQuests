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
    public class NobleNeedsNewWeaponIssueBehavior : CampaignBehaviorBase
    {
        // Needs to be a noble
        private bool ConditionsHold(Hero issueGiver)
        {
            return issueGiver.IsNoble && issueGiver.IsCommander && !issueGiver.Noncombatant;
        }

        // If the conditions hold, start this quest, otherwise just add it as a possible quest
        public void OnCheckForIssue(Hero hero)
        {
            if (this.ConditionsHold(hero))
            {
                Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new PotentialIssueData.StartIssueDelegate(this.OnIssueSelected), typeof(VillageBanditArmyRaidIssueBehavior.VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.Common));
                return;
            }
            Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(VillageBanditArmyRaidIssueBehavior.VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.Common));
        }

        private IssueBase OnIssueSelected(in PotentialIssueData pid, Hero issueOwner)
        {
            return new NobleNeedsNewWeaponIssueBehavior.NobleNeedsNewWeaponIssue(issueOwner);
        }


        // Now the Issue
        internal class NobleNeedsNewWeaponIssue : IssueBase
        {
            public NobleNeedsNewWeaponIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(20f))
            {
            }

            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{NOBLE_NAME} needs new weapon", null);
                    textObject.SetTextVariable("NOBLE_NAME", base.IssueOwner.Name);
                    return textObject;
                }
            }

            public override TextObject Description
            {
                get
                {
                    TextObject textObject = new TextObject("{NOBLE_NAME} wants to have a finely crafted weapon made for them, to replace their old one", null);
                    textObject.SetTextVariable("NOBLE_NAME", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    return new TextObject("My weapon of choice was broken in a recent battle and there seems to be little hope of fixing it anew. In the meantime I've decided to allow some merchants and smithys to provide me with a suitable replacement.", null);
                }
            }

            public override TextObject IssueAcceptByPlayer
            {
                get
                {
                    return new TextObject("Have you found a good weapon yet?", null);
                }
            }

            public override TextObject IssueQuestSolutionExplanationByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("No, I have not. If you find a good weapon of at least value X in your travels, please bring it to me, I will happily buy it from you for double the market rate. If you feel up to the challenge, you could even try smithing the weapon instead.", null);
                    //textObject.SetTextVariable("SPEARS_AMOUNT", this.NeededHardWoodAmount);
                    return textObject;
                }
            }

            public override TextObject IssueQuestSolutionAcceptByPlayer
            {
                get
                {
                    return new TextObject("I will find you a new weapon.", null);
                }
            }

            public override TextObject IssueAsRumorInSettlement
            {
                get
                {
                    TextObject textObject = new TextObject("{QUEST_GIVER.NAME} has been looking for a new weapon to replace their current one. I heard they broke it cutting right through both a shield and the solider behind it, all in one blow!", null);
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
                return IssueBase.IssueFrequency.Rare;
            }

            public override bool IssueStayAliveConditions()
            {
                return true;
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

        internal class NobleNeedsNewWeaponQuest : QuestBase
        {
            public NobleNeedsNewWeaponQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
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
        public class NobleNeedsNewWeaponIssueTypeDefiner : SaveableTypeDefiner
        {
            public NobleNeedsNewWeaponIssueTypeDefiner() : base(1000502)
            {
            }

            protected override void DefineClassTypes()
            {
                base.AddClassDefinition(typeof(NobleNeedsNewWeaponIssueBehavior.NobleNeedsNewWeaponIssue), 1);
                base.AddClassDefinition(typeof(NobleNeedsNewWeaponIssueBehavior.NobleNeedsNewWeaponQuest), 2);
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
