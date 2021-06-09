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
    public class VillageBanditArmyRaidIssueBehavior : CampaignBehaviorBase
    {
        // Needs to be village notable with a bandit base in range of 1225
        private bool ConditionsHold(Hero issueGiver)
        {
            return issueGiver.IsRuralNotable && issueGiver.CurrentSettlement != null && issueGiver.CurrentSettlement.IsVillage;
        }

        // If the conditions hold, start this quest, otherwise just add it as a possible quest
        public void OnCheckForIssue(Hero hero)
        {
            if (hero.IsNotable)
            {
                Settlement settlement = AdditionalQuestsHelperMethods.FindSuitableHideout(hero);
                if (this.ConditionsHold(hero) && settlement != null)
                {
                    Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new PotentialIssueData.StartIssueDelegate(this.OnIssueSelected), typeof(VillageBanditArmyRaidIssueBehavior.VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.VeryCommon, settlement));
                    return;
                }
                Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(VillageBanditArmyRaidIssueBehavior.VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.VeryCommon));
            }
        }

        private IssueBase OnIssueSelected(in PotentialIssueData pid, Hero issueOwner)
        {
            PotentialIssueData potentialIssueData = pid;
            return new VillageBanditArmyRaidIssueBehavior.VillageBanditArmyRaidIssue(issueOwner, potentialIssueData.RelatedObject as Settlement);
        }

        // Now the Issue
        internal class VillageBanditArmyRaidIssue : IssueBase
        {
            // Issue Vars
            Settlement BanditSettlement;

            public VillageBanditArmyRaidIssue(Hero issueOwner, Settlement banditSettlement) : base(issueOwner, CampaignTime.DaysFromNow(20f))
            {
                this.BanditSettlement = banditSettlement;
            }

            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("Bandit army heading to {ISSUE_SETTLEMENT}!", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject Description
            {
                get
                {
                    TextObject textObject = new TextObject("A Bandit army is heading {ISSUE_SETTLEMENT} for a raid. The people of {ISSUE_SETTLEMENT} will need help defending themselves", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    return new TextObject("Grave news has reached me, scouts have informed me that an army of bandits led by a self-proclaimed bandit king are making their way to our village right now, intent on taking everything we have. We've sent word to our nobles but I fear they will not reach them in time.", null);
                }
            }

            public override TextObject IssueAcceptByPlayer
            {
                get
                {
                    return new TextObject("How about the militia, cant they stop the bandits?", null);
                }
            }

            public override TextObject IssueQuestSolutionExplanationByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("They will fight for their homes, but the more people we have the better, the enemy will be mostly comprised of rabble, but they will have some better trained bandits as well. Will you help us defend with your men?", null);
                    //textObject.SetTextVariable("SPEARS_AMOUNT", this.NeededHardWoodAmount);
                    return textObject;
                }
            }

            public override TextObject IssueQuestSolutionAcceptByPlayer
            {
                get
                {
                    return new TextObject("Yes, I will help defend the village.", null);
                }
            }

            public override TextObject IssueAsRumorInSettlement
            {
                get
                {
                    TextObject textObject = new TextObject("An army of bandits are heading their way here right now! We have to run, or hide, anything but stay here! Please, you must help us!", null);
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
                return BanditSettlement.Hideout.IsInfested;
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
                if (Clan.PlayerClan.Tier < 1)
                {
                    flag |= IssueBase.PreconditionFlags.ClanTier;
                }
                if (MobileParty.MainParty.MemberRoster.TotalHealthyCount < 50)
                {
                    flag |= IssueBase.PreconditionFlags.NotEnoughTroops;
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
            public VillageBanditArmyRaidIssueTypeDefiner() : base(80506)
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
