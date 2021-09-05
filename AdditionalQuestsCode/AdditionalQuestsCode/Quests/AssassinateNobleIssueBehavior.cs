using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace AdditionalQuestsCode.Quests
{
    class AssassinateNobleIssueBehavior : CampaignBehaviorBase
    {
        // Needs to be a noble with one "enemy" character
        // Also during a war between the two factions
        private bool ConditionsHold(Hero issueGiver)
        {
            return issueGiver.IsNoble;
        }

        // If the conditions hold, start this quest, otherwise just add it as a possible quest
        public void OnCheckForIssue(Hero hero)
        {
            if (this.ConditionsHold(hero))
            {
                Hero target = AdditionalQuestsHelperMethods.FindSuitableAssassinationTarget(hero, -30);
                if (target != null)
                {
                    Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new PotentialIssueData.StartIssueDelegate(this.OnIssueSelected), typeof(AssassinateNobleIssueBehavior.AssassinateNobleIssue), IssueBase.IssueFrequency.Rare, target));
                    return;
                }
            }
            Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(AssassinateNobleIssueBehavior.AssassinateNobleIssue), IssueBase.IssueFrequency.Rare));
        }

        private IssueBase OnIssueSelected(in PotentialIssueData pid, Hero issueOwner)
        {
            PotentialIssueData potentialIssueData = pid;
            return new AssassinateNobleIssueBehavior.AssassinateNobleIssue(issueOwner, potentialIssueData.RelatedObject as Hero);
        }

        internal class AssassinateNobleIssue : IssueBase
        {
            // Issue Vars and constructor
            [SaveableField(1)]
            Hero targetHero;

            public AssassinateNobleIssue(Hero issueOwner, Hero target) : base(issueOwner, CampaignTime.DaysFromNow(30f))
            {
                IssueOwner = issueOwner;
                targetHero = target;
            }

            // Here we Store the TextObjects that are used by the Issue
            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQANTitle}{QUEST_GIVER} wants {TARGET} Assassinated!", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", IssueOwner.CharacterObject, textObject);
                    StringHelpers.SetCharacterProperties("TARGET", targetHero.CharacterObject, textObject);
                    return textObject;
                }
            }

            public override TextObject Description
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQANDescription}{QUEST_GIVER} wants to use the current war to get their enemy, {TARGET} killed.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", IssueOwner.CharacterObject, textObject);
                    StringHelpers.SetCharacterProperties("TARGET", targetHero.CharacterObject, textObject);
                    return textObject;
                }
            }

            public override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQANIssueBrief}You may know that I have had a long running fued with {TARGET}, their crimes are to long to list, yet I have not had an oppurtunity to deal with them myself. The current war however, is a perfect time to have them \"dealt\" with permanently, I trust you can do this for me?", null);
                    StringHelpers.SetCharacterProperties("TARGET", targetHero.CharacterObject, textObject);
                    return textObject;
                }
            }

            public override TextObject IssueAcceptByPlayer
            {
                get
                {
                    return new TextObject("{=AQANIssueAccept}How do you want them dealt with?", null);
                }
            }

            public override TextObject IssueQuestSolutionExplanationByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQANIssueSolution}Find them and kill them, either in battle or afterwards, if they survive. If killing prisoners is beneath you, deliver {TARGET} to me and I will finish them myself.", null);
                    StringHelpers.SetCharacterProperties("TARGET", targetHero.CharacterObject, textObject);
                    return textObject;
                }
            }

            public override TextObject IssueQuestSolutionAcceptByPlayer
            {
                get
                {
                    return new TextObject("{=AQANIssueSolutionAccept}I will see what I can do.", null);
                }
            }

            public override TextObject IssueAsRumorInSettlement
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQANRumor}I heard {QUEST_GIVER} was recently heard shouting \"Will no one rid me of {TARGET}?!\" to their soliders. I hear a fued between the two has claimed the lives of many, I wonder what they plan on doing next?", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", IssueOwner.CharacterObject, textObject);
                    StringHelpers.SetCharacterProperties("TARGET", targetHero.CharacterObject, textObject);
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
                return targetHero.IsAlive;
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
        }

        //internal class BeastHunterDuelQuest : QuestBase { }


        // Save data goes into this class
        public class BeastHunterDuelIssueTypeDefiner : SaveableTypeDefiner
        {
            public BeastHunterDuelIssueTypeDefiner() : base(585920)
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
