using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
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
            [SaveableField(1)]
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
                return new VillageBanditArmyRaidQuest(questId, base.IssueOwner, CampaignTime.DaysFromNow(14f), this.RewardGold);
            }

            protected override void OnGameLoad()
            {
            }
        }

        internal class VillageBanditArmyRaidQuest : QuestBase
        {
            public VillageBanditArmyRaidQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
            {
                BanditArmyMobileParty = null;
                CreateBanditArmyParty();
                CurrentQuestState = BanditArmyRaidQuestState.BanditArmyMovingToSettlement;
                AddTrackedObject(BanditArmyMobileParty);

                SetDialogs();
                InitializeQuestOnCreation();
            }

            // All of our text/logs
            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("Bandit army raiding {ISSUE_SETTLEMENT}!", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageOnePlayerAcceptsQuestLogText
            {
                get
                {
                    TextObject textObject = new TextObject("The village of {QUEST_SETTLEMENT} is about to be raided by a bandit army from a nearby hideout! {QUEST_GIVER.LINK} has asked you to stop the bandits from devastating the village. \n \n Stop the bandits from reaching {QUEST_SETTLEMENT}.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageTwoPlayerDefeatedBanditArmy
            {
                get
                {
                    TextObject textObject = new TextObject("You have defeated the bandit army. The village of {QUEST_SETTLEMENT} is safe.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageSuccessLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You have defeated the bandit army that was going to raid {QUEST_SETTLEMENT}. The villagers are very grateful. The faction ruling the village have awarded you a bounty for defending the village.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageFailureRaidLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You've failed to stop the bandit army. The bandits ravaged {QUEST_SETTLEMENT} and left.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageFailureTimeoutLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You have taken to long to defeat the bandit army, forunately, this means the nobles rulling {QUEST_SETTLEMENT} have been able to respond in time, your agreement was cancelled.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageCancelDueToWarLogText
            {
                get
                {
                    TextObject textObject = new TextObject("Your clan is now at war with the {ISSUE_GIVER.LINK}'s lord. Your agreement with {ISSUE_GIVER.LINK} was canceled.", null);
                    StringHelpers.SetCharacterProperties("ISSUE_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            private TextObject StageCancelDueToRaidLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{SETTLEMENT_NAME} was raided by someone else. Your agreement with {ISSUE_GIVER.LINK} was canceled.", null);
                    textObject.SetTextVariable("SETTLEMENT_NAME", base.QuestGiver.CurrentSettlement.Name);
                    StringHelpers.SetCharacterProperties("ISSUE_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            // Register Events
            protected override void RegisterEvents()
            {
                CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, new Action(this.OnHourlyTickEvent));
                CampaignEvents.MapEventEnded.AddNonSerializedListener(this, new Action<MapEvent>(this.MapEventEnded));
                CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction>(this.OnWarDeclared));
                CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, new Action<Clan, Kingdom, Kingdom, ChangeKingdomAction.ChangeKingdomActionDetail, bool>(this.OnClanChangedKingdom));
                CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.SettlementEntered));
                CampaignEvents.VillageBeingRaided.AddNonSerializedListener(this, new Action<Village>(this.OnVillageBeingRaided));
                CampaignEvents.MapEventStarted.AddNonSerializedListener(this, new Action<MapEvent, PartyBase, PartyBase>(this.OnMapEventStarted));
            }

            private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
            {
                QuestHelper.CheckMinorMajorCoercionAndFailQuest(this, mapEvent, attackerParty);
            }

            private void OnHourlyTickEvent()
            {
                if (BanditArmyMobileParty == null || BanditArmyMobileParty.MapEvent == null)
                {
                    switch (CurrentQuestState)
                    {
                        case BanditArmyRaidQuestState.BanditArmyMovingToSettlement:
                            if (!BanditArmyMobileParty.IsCurrentlyGoingToSettlement)
                            {
                                SetPartyAiAction.GetActionForVisitingSettlement(BanditArmyMobileParty, QuestGiver.CurrentSettlement);
                            }
                            break;
                        case BanditArmyRaidQuestState.BanditArmyDefeatedPlayer:
                            if (!BanditArmyMobileParty.IsCurrentlyGoingToSettlement)
                            {
                                SetPartyAiAction.GetActionForVisitingSettlement(BanditArmyMobileParty, QuestGiver.CurrentSettlement);
                            }
                            break;
                        default:
                            return;
                    }
                }
            }

            private void MapEventEnded(MapEvent mapEvent)
            {
                if (mapEvent.IsPlayerMapEvent && BanditArmyMobileParty != null && mapEvent.InvolvedParties.Contains(BanditArmyMobileParty.Party))
                {
                    // We won
                    if (mapEvent.WinningSide == mapEvent.PlayerSide)
                    {
                        CompleteQuestWithSuccess();
                        FinishQuest(BanditArmyRaidQuestFinish.PlayerDefeatBanditArmy);
                        CurrentQuestState = BanditArmyRaidQuestState.BanditArmyAreDefeated;
                        return;
                    }

                    // We must have lost so make bandits continue onto village
                    if (!BanditArmyMobileParty.IsCurrentlyGoingToSettlement)
                    {
                        SetPartyAiAction.GetActionForVisitingSettlement(BanditArmyMobileParty, QuestGiver.CurrentSettlement);
                    }
                    CurrentQuestState = BanditArmyRaidQuestState.BanditArmyDefeatedPlayer;
                }
            }

            private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification = true)
            {
                this.CheckWarDeclaration();
            }

            private void OnWarDeclared(IFaction faction1, IFaction faction2)
            {
                this.CheckWarDeclaration();
            }

            private void CheckWarDeclaration()
            {
                if (base.QuestGiver.CurrentSettlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
                {
                    CompleteQuestWithCancel();
                    FinishQuest(BanditArmyRaidQuestFinish.CancelDueToFactionWar);
                }
            }

            private void OnVillageBeingRaided(Village village)
            {
                if (village.Settlement == QuestGiver.CurrentSettlement)
                {
                    CompleteQuestWithCancel();
                    FinishQuest(BanditArmyRaidQuestFinish.CancelDueToVillageRaid);
                }
            }

            private void SettlementEntered(MobileParty party, Settlement settlement, Hero hero)
            {
                if (party == BanditArmyMobileParty && settlement == QuestGiver.CurrentSettlement)
                {
                    if (PlayerCaptivity.IsCaptive && PlayerCaptivity.CaptorParty == BanditArmyMobileParty.Party)
                    {
                        EndCaptivityAction.ApplyByEscape(Hero.MainHero, null);
                    }
                    base.CompleteQuestWithFail();
                    FinishQuest(BanditArmyRaidQuestFinish.BanditArmyDefeatPlayer);
                }
            }

            protected override void OnTimedOut()
            {
                base.CompleteQuestWithTimeOut();
                FinishQuest(BanditArmyRaidQuestFinish.CancelDueToTimeout);
            }

            // Quest logic, the dialogs and conditions for it be to success or failure
            public override bool IsRemainingTimeHidden
            {
                get
                {
                    return false;
                }
            }

            protected override void InitializeQuestOnGameLoad()
            {
                if (BanditArmyMobileParty != null && BanditArmyMobileParty.IsActive)
                {
                    BanditArmyMobileParty.Ai.SetDoNotMakeNewDecisions(true);
                }
                this.SetDialogs();
            }

            protected override void SetDialogs()
            {
                this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine("Thank you. The bandits are on their way from a hideout close by. My scouts will show you where it is. We will pray for your sucess.", null, null).Condition(() => Hero.OneToOneConversationHero == base.QuestGiver).Consequence(new ConversationSentence.OnConsequenceDelegate(this.QuestAcceptedConsequences)).CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine("I don't think they'll be long now. I hope you can stop them in time.", null, null).Condition(() => Hero.OneToOneConversationHero == base.QuestGiver).BeginPlayerOptions().PlayerOption("Don't worry, we will crush their \"army\".", null).NpcLine("Good, good. Thank you again.", null, null).CloseDialog().EndPlayerOptions().CloseDialog();
                this.QuestCharacterDialogFlow = DialogFlow.CreateDialogFlow("start", 125).NpcLine("Who the hell are you? This 'ere is an army, we're on our way to a village to collect some \"taxes\". If you know whats good for you, you'll stay outta our way!", null, null).Condition(() => BanditArmyMobileParty != null && BanditArmyMobileParty.IsActive && CharacterObject.OneToOneConversationCharacter == BanditArmyMobileParty.Leader).BeginPlayerOptions().PlayerOption("You wont be raiding around here anytime soon, We're here to stop you!", null).CloseDialog().EndPlayerOptions().CloseDialog();
            }

            private void QuestAcceptedConsequences()
            {
                base.StartQuest();
                base.AddLog(this.StageOnePlayerAcceptsQuestLogText, false);
            }

            private void FinishQuest(BanditArmyRaidQuestFinish finishState)
            {
                switch (finishState)
                {
                    case BanditArmyRaidQuestFinish.PlayerDefeatBanditArmy:
                        AddLog(StageSuccessLogText, false);
                        break;
                    case BanditArmyRaidQuestFinish.BanditArmyDefeatPlayer:
                        AddLog(StageFailureRaidLogText, false);
                        break;
                    case BanditArmyRaidQuestFinish.CancelDueToVillageRaid:
                        AddLog(StageCancelDueToRaidLogText, false);
                        break;
                    case BanditArmyRaidQuestFinish.CancelDueToFactionWar:
                        AddLog(StageCancelDueToWarLogText, false);
                        break;
                    case BanditArmyRaidQuestFinish.CancelDueToTimeout:
                        AddLog(StageFailureTimeoutLogText, false);
                        break;
                    default:
                        break;
                }
            }

            protected override void OnFinalize()
            {
                ReleaseBanditArmyParty();
            }

            private void CreateBanditArmyParty()
            {
                Settlement settlement = SettlementHelper.FindNearestSettlement((Settlement x) => x.IsHideout());
                Clan clan = null;
                if (settlement != null)
                {
                    CultureObject banditCulture = settlement.Culture;
                    clan = Clan.BanditFactions.FirstOrDefault((Clan x) => x.Culture == banditCulture);
                }
                if (clan == null)
                {
                    clan = Clan.All.GetRandomElementWithPredicate((Clan x) => x.IsBanditFaction);
                }
                PartyTemplateObject defaultPartyTemplate = settlement.Culture.BanditBossPartyTemplate;
                BanditArmyMobileParty = BanditPartyComponent.CreateBanditParty("bandit_army_party_1", clan, settlement.Hideout, false);
                TextObject customName = new TextObject("Bandit Army", null);
                this.BanditArmyMobileParty.Party.Owner = ((clan != null) ? clan.Leader : null);
                this.BanditArmyMobileParty.InitializeMobileParty(defaultPartyTemplate, settlement.GetPosition2D, 0f, 0f, 50);
                this.BanditArmyMobileParty.SetCustomName(customName);
                float foodChange = MBMath.Absf(this.BanditArmyMobileParty.FoodChange);
                int num3 = MBMath.Ceiling(base.QuestDueTime.RemainingDaysFromNow * foodChange);
                int num4 = num3 / 2;
                BanditArmyMobileParty.ItemRoster.AddToCounts(DefaultItems.Grain, num4);
                int number = num3 - num4;
                BanditArmyMobileParty.ItemRoster.AddToCounts(DefaultItems.Meat, number);
                BanditArmyMobileParty.SetPartyUsedByQuest(true);
                BanditArmyMobileParty.IgnoreByOtherPartiesTill(CampaignTime.Never);
                BanditArmyMobileParty.Aggressiveness = 0f;
                BanditArmyMobileParty.Ai.SetDoNotMakeNewDecisions(true);
                BanditArmyMobileParty.Party.Visuals.SetMapIconAsDirty();
                SetPartyAiAction.GetActionForVisitingSettlement(BanditArmyMobileParty, QuestGiver.CurrentSettlement);
            }

            private void DestroyBanditArmyParty()
            {
                if (BanditArmyMobileParty != null && BanditArmyMobileParty.IsActive)
                {
                    DestroyPartyAction.Apply(null, BanditArmyMobileParty);
                    BanditArmyMobileParty = null;
                }
            }

            private void ReleaseBanditArmyParty()
            {
                if (BanditArmyMobileParty != null && BanditArmyMobileParty.IsActive)
                {
                    BanditArmyMobileParty.SetPartyUsedByQuest(false);
                    BanditArmyMobileParty.IgnoreByOtherPartiesTill(CampaignTime.HoursFromNow(0f));
                    BanditArmyMobileParty.Aggressiveness = 1f;
                    if (BanditArmyMobileParty.CurrentSettlement != null)
                    {
                        LeaveSettlementAction.ApplyForParty(BanditArmyMobileParty);
                    }
                    BanditArmyMobileParty.Ai.SetDoNotMakeNewDecisions(false);
                }
            }

            // Saved vars/logs
            [SaveableField(1)]
            private MobileParty BanditArmyMobileParty;

            [SaveableField(2)]
            private BanditArmyRaidQuestState CurrentQuestState;

            [SaveableField(3)]
            private JournalLog PlayerAcceptedQuestLog;

            // The current state of the Quest
            internal enum BanditArmyRaidQuestState
            {
                BanditArmyMovingToSettlement,
                BanditArmyDefeatedPlayer,
                BanditArmyAreDefeated
            }

            // The Enum for how the quest finishes
            internal enum BanditArmyRaidQuestFinish
            {
                PlayerDefeatBanditArmy,
                BanditArmyDefeatPlayer,
                CancelDueToVillageRaid,
                CancelDueToFactionWar,
                CancelDueToTimeout,
            }
        }

        // Save data goes into this class
        public class VillageBanditArmyRaidIssueTypeDefiner : SaveableTypeDefiner
        {
            public VillageBanditArmyRaidIssueTypeDefiner() : base(585870)
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
