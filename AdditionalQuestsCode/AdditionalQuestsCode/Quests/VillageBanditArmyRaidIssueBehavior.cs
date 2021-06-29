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
                    Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new PotentialIssueData.StartIssueDelegate(this.OnIssueSelected), typeof(VillageBanditArmyRaidIssueBehavior.VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.Common, settlement));
                    return;
                }
                Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(VillageBanditArmyRaidIssueBehavior.VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.Common));
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
            // Issue Vars and constructor
            [SaveableField(1)]
            Settlement BanditSettlement;

            public VillageBanditArmyRaidIssue(Hero issueOwner, Settlement banditSettlement) : base(issueOwner, CampaignTime.DaysFromNow(20f))
            {
                this.BanditSettlement = banditSettlement;
            }

            // Issue TextObjects
            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("Bandit Army Heading to {ISSUE_SETTLEMENT}!", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject Description
            {
                get
                {
                    TextObject textObject = new TextObject("A Bandit army is heading to {ISSUE_SETTLEMENT} for a raid. The people of {ISSUE_SETTLEMENT} need help defending themselves.", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    return new TextObject("Dire news has reached me, my scouts say that an army of rabble led by a self-proclaimed bandit \"king\" are making their way to our village right now, intent on taking everything we have. I've sent word to our nobles for help but I fear it may be to late by the time they get here.", null);
                }
            }

            public override TextObject IssueAcceptByPlayer
            {
                get
                {
                    return new TextObject("That is troubling news, how will you defend yourselves?", null);
                }
            }

            public override TextObject IssueQuestSolutionExplanationByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("Our militia will fight for our homes, but the more people we have the better. The enemy will be mostly comprised of rabble, just looters looking for easy money but they will have some better trained bandits as well. Will you help us defend the village?", null);
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
                    TextObject textObject = new TextObject("I heard that an army of bandits are heading their way here right now! We need to run, or hide or-or anything but stay here! Please, you must help us!", null);
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
                return new VillageBanditArmyRaidQuest(questId, base.IssueOwner, CampaignTime.DaysFromNow(10f), 3000, BanditSettlement);
            }

            protected override void OnGameLoad()
            {
            }
        }

        internal class VillageBanditArmyRaidQuest : QuestBase
        {
            public VillageBanditArmyRaidQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold, Settlement banditSettlement) : base(questId, questGiver, duration, rewardGold)
            {
                BanditSettlement = banditSettlement;
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
                    TextObject textObject = new TextObject("Bandit army heading to {ISSUE_SETTLEMENT}!", null);
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

            private TextObject StageTwoBanditArmyRaidingSettlement
            {
                get
                {
                    TextObject textObject = new TextObject("The Bandit army is raiding {QUEST_SETTLEMENT}! Protect them!.", null);
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
                CampaignEvents.VillageLooted.AddNonSerializedListener(this, new Action<Village>(this.OnVillageLooted));
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
                                SetPartyAiAction.GetActionForRaidingSettlement(BanditArmyMobileParty, QuestGiver.CurrentSettlement);
                            }
                            break;
                        case BanditArmyRaidQuestState.BanditArmyDefeatedPlayer:
                            if (!BanditArmyMobileParty.IsCurrentlyGoingToSettlement)
                            {
                                SetPartyAiAction.GetActionForRaidingSettlement(BanditArmyMobileParty, QuestGiver.CurrentSettlement);
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
                        SetPartyAiAction.GetActionForRaidingSettlement(BanditArmyMobileParty, QuestGiver.CurrentSettlement);
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
                if(village.Settlement == QuestGiver.CurrentSettlement && village.Settlement.LastAttackerParty == BanditArmyMobileParty)
                {
                    // Send notification to player
                    AddLog(StageTwoBanditArmyRaidingSettlement, false);
                    return;
                }

                if (village.Settlement == QuestGiver.CurrentSettlement)
                {
                    CompleteQuestWithCancel();
                    FinishQuest(BanditArmyRaidQuestFinish.CancelDueToVillageRaid);
                }
            }

            private void OnVillageLooted(Village village)
            {
                if (village.Settlement == QuestGiver.CurrentSettlement && village.Settlement.LastAttackerParty == BanditArmyMobileParty)
                {
                    base.CompleteQuestWithFail();
                    FinishQuest(BanditArmyRaidQuestFinish.BanditArmyDefeatPlayer);
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
                BanditSettlement.Hideout.IsSpotted = true;
                BanditSettlement.IsVisible = true;
                QuestHelper.AddMapArrowFromPointToTarget(new TextObject("Direction to Bandits", null), QuestGiver.CurrentSettlement.Position2D, BanditSettlement.Position2D, 5f, 0.1f, 1056732);
                TextObject textObject = new TextObject("{QUEST_GIVER.NAME} has marked the bandit hideout. The bandit army will head from there to the village", null);
                StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                InformationManager.AddQuickInformation(textObject, 0, null, "");
            }

            private void FinishQuest(BanditArmyRaidQuestFinish finishState)
            {
                switch (finishState)
                {
                    case BanditArmyRaidQuestFinish.PlayerDefeatBanditArmy:
                        AddLog(StageSuccessLogText, false);
                        // Now do player effects eg add reknown
                        Clan.PlayerClan.AddRenown(5f, false);
                        GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, this.RewardGold, false);
                        // Now add power to notable and give relationship bonus
                        base.QuestGiver.AddPower(25f);
                        this.RelationshipChangeWithQuestGiver = 20;
                        ChangeRelationAction.ApplyPlayerRelation(QuestGiver, this.RelationshipChangeWithQuestGiver, false, true);
                        // also increase settlement prosperity
                        // bonus to relationship with other notables as well
                        foreach (var hero in QuestGiver.CurrentSettlement.Notables)
                        {
                            if (hero == QuestGiver)
                            {
                                continue;
                            }
                            ChangeRelationAction.ApplyPlayerRelation(hero, this.RelationshipChangeWithQuestGiver / 2, false, false);
                        }
                        QuestGiver.CurrentSettlement.Prosperity += 100f;
                        CompleteQuestWithSuccess();
                        break;
                    case BanditArmyRaidQuestFinish.BanditArmyDefeatPlayer:
                        AddLog(StageFailureRaidLogText, false);
                        base.QuestGiver.AddPower(-25f);
                        this.RelationshipChangeWithQuestGiver = -10;
                        CompleteQuestWithFail();
                        break;
                    case BanditArmyRaidQuestFinish.CancelDueToVillageRaid:
                        AddLog(StageCancelDueToRaidLogText, false);
                        CompleteQuestWithCancel();
                        break;
                    case BanditArmyRaidQuestFinish.CancelDueToFactionWar:
                        AddLog(StageCancelDueToWarLogText, false);
                        CompleteQuestWithCancel();
                        break;
                    case BanditArmyRaidQuestFinish.CancelDueToTimeout:
                        AddLog(StageFailureTimeoutLogText, false);
                        CompleteQuestWithTimeOut();
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
                // Get our Bandit Settlement Culture
                Clan clan = null;
                if (BanditSettlement != null)
                {
                    CultureObject banditCulture = BanditSettlement.Culture;
                    clan = Clan.BanditFactions.FirstOrDefault((Clan x) => x.Culture == banditCulture);
                }
                if (clan == null)
                {
                    clan = Clan.All.GetRandomElementWithPredicate((Clan x) => x.IsBanditFaction);
                }

                // Build the Bandit Party
                // 50% looters, 50% bandit culture specific, with 30% being tier one, 20% being tier two bandit then one leader
                // Plus bandit leader
                // Party size is 50 at player clan tier one, plus 25 per player clan tier
                //this._banditParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse"), this.BanditPartyTroopCount / 4);
                PartyTemplateObject defaultPartyTemplate = BanditSettlement.Culture.BanditBossPartyTemplate;
                BanditArmyMobileParty = BanditPartyComponent.CreateBanditParty("bandit_army_party_1", clan, BanditSettlement.Hideout, false);
                TextObject customName = new TextObject("{BANDIT_CULTURE} Army", null);
                customName.SetTextVariable("BANDIT_CULTURE", BanditSettlement.Culture.Name);
                this.BanditArmyMobileParty.Party.Owner = ((clan != null) ? clan.Leader : null);
                this.BanditArmyMobileParty.InitializeMobileParty(defaultPartyTemplate, BanditSettlement.GetPosition2D, 0.1f, 0.2f);
                this.BanditArmyMobileParty.SetCustomName(customName);
                BanditArmyMobileParty.MemberRoster.Clear();

                int banditPartySize = 25 + Hero.MainHero.Clan.Tier * 25;
                BanditArmyMobileParty.AddElementToMemberRoster(CharacterObject.All.FirstOrDefault((CharacterObject t) => t.StringId == "looter"),((banditPartySize*50)/100) + MBRandom.RandomInt(-5,5));
                BanditArmyMobileParty.AddElementToMemberRoster(CharacterObject.All.FirstOrDefault((CharacterObject t) => t.Culture == BanditSettlement.Culture && t.Tier == 2), ((banditPartySize * 30) / 100) + MBRandom.RandomInt(-3, 3));
                BanditArmyMobileParty.AddElementToMemberRoster(CharacterObject.All.FirstOrDefault((CharacterObject t) => t.Culture == BanditSettlement.Culture && t.Tier == 3), ((banditPartySize * 20) / 100) + MBRandom.RandomInt(-2, 2));
                BanditArmyMobileParty.AddElementToMemberRoster(CharacterObject.All.FirstOrDefault((CharacterObject t) => t.Culture == BanditSettlement.Culture && t.Tier == 4), 1,true);

                // Add some food to party
                float foodChange = MBMath.Absf(this.BanditArmyMobileParty.FoodChange);
                int num3 = MBMath.Ceiling(base.QuestDueTime.RemainingDaysFromNow * foodChange);
                int num4 = num3 / 2;
                BanditArmyMobileParty.ItemRoster.AddToCounts(DefaultItems.Grain, num4);
                int number = num3 - num4;
                BanditArmyMobileParty.ItemRoster.AddToCounts(DefaultItems.Meat, number);

                // Set bandit army quest variables
                BanditArmyMobileParty.SetPartyUsedByQuest(true);
                BanditArmyMobileParty.IgnoreByOtherPartiesTill(QuestDueTime);
                BanditArmyMobileParty.Aggressiveness = 0f;
                BanditArmyMobileParty.Ai.SetDoNotMakeNewDecisions(true);
                BanditArmyMobileParty.Party.Visuals.SetMapIconAsDirty();
                SetPartyAiAction.GetActionForRaidingSettlement(BanditArmyMobileParty, QuestGiver.CurrentSettlement);
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
            Settlement BanditSettlement;

            [SaveableField(3)]
            private BanditArmyRaidQuestState CurrentQuestState;

            [SaveableField(4)]
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
