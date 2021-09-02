using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;

namespace AdditionalQuestsCode.Quests
{
    public class TrainingBattleIssueBehavior : CampaignBehaviorBase
    {
        // Needs to be a noble commander with at least 20% of their army being tier one/tier two units
        private bool ConditionsHold(Hero issueGiver)
        {
            if (issueGiver.IsNoble && issueGiver.IsPartyLeader)
            {
                double lowTierTroops = 0;
                double totalTroops = 0;

                foreach (var troop in issueGiver.PartyBelongedTo.MemberRoster.GetTroopRoster())
                {
                    if (troop.Character.Tier == 1 || troop.Character.Tier == 2)
                    {
                        lowTierTroops += troop.Number;
                    }
                    totalTroops += troop.Number;
                }

                if (lowTierTroops / totalTroops >= 0.2)
                {
                    return true;
                }
            }

            return false;
        }

        // If the conditions hold, start this quest, otherwise just add it as a possible quest
        public void OnCheckForIssue(Hero hero)
        {
            if (this.ConditionsHold(hero))
            {
                Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new PotentialIssueData.StartIssueDelegate(this.OnIssueSelected), typeof(BanditArmyIssueBehavior.VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.Common));
                return;
            }
            Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(BanditArmyIssueBehavior.VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.Common));
        }

        private IssueBase OnIssueSelected(in PotentialIssueData pid, Hero issueOwner)
        {
            return new TrainingBattleIssueBehavior.NobleWantsTrainingBattleIssue(issueOwner);
        }

        // Now the Issue
        internal class NobleWantsTrainingBattleIssue : IssueBase
        {
            public NobleWantsTrainingBattleIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(20f))
            {
            }

            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{NOBLE_NAME} Wants a Training Battle", null);
                    textObject.SetTextVariable("NOBLE_NAME", base.IssueOwner.Name);
                    return textObject;
                }
            }

            public override TextObject Description
            {
                get
                {
                    TextObject textObject = new TextObject("{NOBLE_NAME} wants to have {?QUEST_GIVER.GENDER}her{?}his{\\?} army trained in combat, via a miltary exercise.", null);
                    textObject.SetTextVariable("NOBLE_NAME", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    return new TextObject("I have far to many fresh faced recruits in my army, not ready for combat against anything more trained then looters. I believe a training exercise with some veteran soliders will be a good experience for them..", null);
                }
            }

            public override TextObject IssueAcceptByPlayer
            {
                get
                {
                    return new TextObject("I have some experienced troops, maybe I can help?", null);
                }
            }

            public override TextObject IssueQuestSolutionExplanationByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("That would be great, If you can wait a couple of days, I can prepare a training battle. Bring 50 of your men against 50 of mine. I will also provide suitably blunted weapons and medics. If you beat my troops, I'll even throw in a extra reward.", null);
                    return textObject;
                }
            }

            public override TextObject IssueQuestSolutionAcceptByPlayer
            {
                get
                {
                    return new TextObject("Sure, I will prepare my men and wait for your word.", null);
                }
            }

            public override TextObject IssueAsRumorInSettlement
            {
                get
                {
                    TextObject textObject = new TextObject("I heard {QUEST_GIVER.NAME} shouting at {?QUEST_GIVER.GENDER}her{?}his{\\?} troops today, {?QUEST_GIVER.GENDER}she{?}he{\\?} was shouting at a poor recruit who had tried to hold a sword by the wrong end. Yes, the pointy one. I hear they are looking for people with combat experience to help them train.", null);
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
                if (IssueOwner.IsNoble && IssueOwner.IsPartyLeader)
                {
                    double lowTierTroops = 0;
                    double totalTroops = 0;

                    foreach (var troop in IssueOwner.PartyBelongedTo.MemberRoster.GetTroopRoster())
                    {
                        if (troop.Character.Tier == 1 || troop.Character.Tier == 2)
                        {
                            lowTierTroops += troop.Number;
                        }
                        totalTroops += troop.Number;
                    }

                    if (lowTierTroops / totalTroops >= 0.2)
                    {
                        return true;
                    }
                }

                return false;
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
                return new NobleWantsTrainingBattleQuest(questId, base.IssueOwner, CampaignTime.DaysFromNow(10f), 1000);
            }

            protected override void OnGameLoad()
            {
            }
        }

        internal class NobleWantsTrainingBattleQuest : QuestBase
        {
            // Constructor with basic vars and any vars about the quest
            public NobleWantsTrainingBattleQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
            {
                this.SetDialogs();
                this.AddGameMenus();
                base.InitializeQuestOnCreation();
            }

            // All of our text/logs
            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{NOBLE_NAME} Wants a Training Battle", null);
                    textObject.SetTextVariable("NOBLE_NAME", base.QuestGiver.Name);
                    return textObject;
                }
            }

            private TextObject StageOnePlayerAcceptsQuestLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{QUEST_GIVER.LINK} has asked you to help organise a training battle. The fight will take place in a {QUEST_SETTLEMENT}. \n \n Wait in {QUEST_SETTLEMENT} until midday.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageTwoFightIsReadyLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=OIBiRTRP}{QUEST_GIVER.LINK} is waiting for you at {SETTLEMENT}.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("SETTLEMENT", base.QuestGiver.CurrentSettlement.EncyclopediaLinkWithName);
                    return textObject;
                }
            }

            private TextObject StageSuccessLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You helped the rebels of {QUEST_SETTLEMENT} take over the town. A new kingdom has been formed! ", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageFailureTimeoutLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You have failed to join the rebels at {QUEST_SETTLEMENT}. The rebellion plan has been cancelled for now, {QUEST_GIVER.LINK} is disapointed at your lack of commitment.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageFailureRejectionLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=aXMg9M7t}You decided to stay out of the fight. {?QUEST_GIVER.GENDER}She{?}He{\\?} will certainly lose to the rival gang without your help.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            private TextObject StageFailureDefeatLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=du3dpMaV}You were unable to defeat {RIVAL_GANG_LEADER.LINK}'s gang, and thus failed to fulfill your commitment to {QUEST_GIVER.LINK}.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            private TextObject StageCancelDueToWarLogText
            {
                get
                {
                    TextObject textObject = new TextObject("Your clan is now at war with the rulers of {QUEST_SETTLEMENT}. It will be to difficult to help the rebels now.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageCancelDueToSiegeLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=s1GWSE9Y}{QUEST_GIVER.LINK} cancels your plans due to the siege of {SETTLEMENT}. {?QUEST_GIVER.GENDER}She{?}He{\\?} has worse troubles than {?QUEST_GIVER.GENDER}her{?}his{\\?} quarrel with the rival gang.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("SETTLEMENT", base.QuestGiver.CurrentSettlement.EncyclopediaLinkWithName);
                    return textObject;
                }
            }

            // Register Events
            protected override void RegisterEvents()
            {
                CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, new Action(this.HourlyTick));
                CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction>(this.OnWarDeclared));
                CampaignEvents.OnSiegeEventStartedEvent.AddNonSerializedListener(this, new Action<SiegeEvent>(this.OnSiegeEventStarted));
                CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, new Action<Clan, Kingdom, Kingdom, ChangeKingdomAction.ChangeKingdomActionDetail, bool>(this.OnClanChangedKingdom));
            }

            private void HourlyTick()
            {
                if (PlayerEncounter.Current != null && PlayerEncounter.Current.IsPlayerWaiting && PlayerEncounter.EncounterSettlement == QuestGiver.CurrentSettlement && CampaignTime.Now.IsNightTime && !this._isReadyToBeFinalized)
                {
                    OnGuestGiverPreparationsCompleted();
                }
            }

            private void OnGuestGiverPreparationsCompleted()
            {
                this._preparationsComplete = true;
                if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement == base.QuestGiver.CurrentSettlement && Campaign.Current.CurrentMenuContext != null && Campaign.Current.CurrentMenuContext.GameMenu.StringId == "town_wait_menus")
                {
                    Campaign.Current.CurrentMenuContext.SwitchToMenu("town_uprising_quest_wait_duration_is_over");
                }
                TextObject textObject = new TextObject("{=DUKbtlNb}{QUEST_GIVER.LINK} has finally sent a messenger telling you it's time to meet {?QUEST_GIVER.GENDER}her{?}him{\\?} and join the fight.", null);
                StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                base.AddLog(StageTwoFightIsReadyLogText, false);
                InformationManager.AddQuickInformation(textObject, 0, null, "");
            }

            private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification)
            {
                this.CheckWarDeclaration();
            }

            private void OnWarDeclared(IFaction faction1, IFaction faction2)
            {
                this.CheckWarDeclaration();
            }

            private void CheckWarDeclaration()
            {
                if (QuestGiver.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
                {
                    base.AddLog(this.StageCancelDueToWarLogText, false);
                    base.CompleteQuestWithCancel();
                }
            }

            private void OnSiegeEventStarted(SiegeEvent siegeEvent)
            {
                if (siegeEvent.BesiegedSettlement == base.QuestGiver.CurrentSettlement)
                {
                    base.AddLog(this.StageCancelDueToSiegeLogText, false);
                    base.CompleteQuestWithCancel();
                }
            }


            // Quest Logic
            protected override void InitializeQuestOnGameLoad()
            {
                this.SetDialogs();
                this.AddGameMenus();
                Campaign.Current.ConversationManager.AddDialogFlow(this.GetQuestGiverPreparationCompletedDialogFlow(), this);
            }

            protected override void SetDialogs()
            {
                this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine("Wait until midnight, I will send someone to you when we are ready. Try not to draw attention to yourself before then.", null, null).Condition(delegate
                {
                    return Hero.OneToOneConversationHero == base.QuestGiver;
                }).Consequence(new ConversationSentence.OnConsequenceDelegate(this.OnQuestAccepted)).CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine("It's not time yet. I'll send a runner for you when the time comes.", null, null).Condition(delegate
                {
                    return Hero.OneToOneConversationHero == base.QuestGiver && !this._isFinalStage && !this._preparationsComplete;
                }).BeginPlayerOptions().PlayerOption("All right. I am waiting for your runner.", null).NpcLine("You'll know right away once the preparations are complete. Just don't leave town.", null, null).CloseDialog().PlayerOption("I can't just hang on here forever. Be quick about it.", null).NpcLine("I'm getting this together as quickly as I can.", null, null).CloseDialog().EndPlayerOptions().CloseDialog();
            }

            private DialogFlow GetQuestGiverPreparationCompletedDialogFlow()
            {
                return DialogFlow.CreateDialogFlow("start", 125).NpcLine("Are you ready for the fight?", null, null).Condition(() => Hero.OneToOneConversationHero == base.QuestGiver && this._preparationsComplete && !this._isFinalStage).BeginPlayerOptions().PlayerOption("I am ready.", null).Condition(() => !Hero.MainHero.IsWounded).NpcLine("Let's finish this!", null, null).Consequence(delegate
                {
                    Campaign.Current.ConversationManager.ConversationEndOneShot += this.rival_gang_start_fight_on_consequence;
                }).CloseDialog().PlayerOption("I need more time.", null).Condition(() => !Hero.MainHero.IsWounded).NpcLine("You’d better hurry up!", null, null).Consequence(delegate
                {
                    Campaign.Current.ConversationManager.ConversationEndOneShot += this.rival_gang_need_more_time_on_consequence;
                }).CloseDialog().PlayerOption("My wounds are still fresh. I need some time to recover.", null).Condition(() => Hero.MainHero.IsWounded).NpcLine("We must attack before the garrison hears about our plan. You'd better hurry up!", null, null).CloseDialog().EndPlayerOptions().CloseDialog();
            }

            // Game Menus
            public void AddGameMenus()
            {
                TextObject textObject = new TextObject("A milita soldier walks up to you and quietly informs you that the men are in position.", null);
                base.AddGameMenu("town_uprising_quest_before_fight", TextObject.Empty, new OnInitDelegate(town_uprising_quest_before_fight_init), GameOverlays.MenuOverlayType.SettlementWithBoth, GameMenu.MenuFlags.none);
                base.AddGameMenu("town_uprising_quest_after_fight", TextObject.Empty, new OnInitDelegate(town_uprising_quest_after_fight_init), GameOverlays.MenuOverlayType.SettlementWithBoth, GameMenu.MenuFlags.none);
                base.AddGameMenu("town_uprising_quest_wait_duration_is_over", textObject, new OnInitDelegate(town_uprising_wait_duration_is_over_menu_on_init), GameOverlays.MenuOverlayType.None, GameMenu.MenuFlags.none);
                base.AddGameMenuOption("town_uprising_quest_wait_duration_is_over", "town_uprising_quest_wait_duration_is_over_yes", new TextObject("Follow the soldier", null), new GameMenuOption.OnConditionDelegate(this.town_uprising_quest_wait_duration_is_over_yes_condition), new GameMenuOption.OnConsequenceDelegate(this.rival_gang_quest_wait_duration_is_over_yes_consequence), false, -1, null);
                base.AddGameMenuOption("town_uprising_quest_wait_duration_is_over", "town_uprising_quest_wait_duration_is_over_no", new TextObject("Leave", null), new GameMenuOption.OnConditionDelegate(this.town_uprising_quest_wait_duration_is_over_no_condition), new GameMenuOption.OnConsequenceDelegate(this.rival_gang_quest_wait_duration_is_over_no_consequence), false, -1, null);
            }

            public override bool IsRemainingTimeHidden
            {
                get
                {
                    return false;
                }
            }

            private void rival_gang_start_fight_on_consequence()
            {
                this._isFinalStage = true;
                if (Mission.Current != null)
                {
                    Mission.Current.EndMission();
                }
                Campaign.Current.GameMenuManager.SetNextMenu("town_uprising_quest_before_fight");
            }

            private void rival_gang_need_more_time_on_consequence()
            {
                if (Campaign.Current.CurrentMenuContext.GameMenu.StringId == "town_uprising_quest_wait_duration_is_over")
                {
                    Campaign.Current.GameMenuManager.SetNextMenu("town_wait_menus");
                }
            }

            private void AddQuestGiverGangLeaderOnSuccessDialogFlow()
            {
                Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 125).NpcLine("Its done! The remaining troops loyal to the noble have been rounded up and taken prisoner. This is the start of a new begining for the people.", null, null).Condition(delegate
                {
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, null);
                    return base.IsOngoing && Hero.OneToOneConversationHero == base.QuestGiver;
                }).Consequence(delegate
                {
                    Campaign.Current.ConversationManager.ConversationEndOneShot += this.OnQuestSucceeded;
                }).CloseDialog(), null);
            }

            /*
            private CharacterObject GetTroopTypeTemplateForDifficulty()
            {
                int difficultyRange = MBMath.ClampInt(MathF.Ceiling(1 / 0.1f), 1, 10);
                CharacterObject characterObject;
                if (difficultyRange == 1)
                {
                    characterObject = CharacterObject.All.FirstOrDefault((CharacterObject t) => t.StringId == "looter");
                }
                else if (difficultyRange == 10)
                {
                    characterObject = CharacterObject.All.FirstOrDefault((CharacterObject t) => t.StringId == "mercenary_8");
                }
                else
                {
                    characterObject = CharacterObject.All.FirstOrDefault((CharacterObject t) => t.StringId == "mercenary_" + (difficultyRange - 1));
                }
                if (characterObject == null)
                {
                    characterObject = CharacterObject.All.First((CharacterObject t) => t.IsBasicTroop && t.IsSoldier);
                }
                return characterObject;
            }
            */

            private void StartCommonAreaBattle()
            {
                // Create Garrison party
                PartyTemplateObject cultureGarrisonTemplate = QuestGiver.CurrentSettlement.Culture.DefaultPartyTemplate;
                this.HostileGarrisonParty = MobileParty.CreateParty("garrison_party", null, null);
                TextObject textObject = new TextObject("Garrison", null);
                this.HostileGarrisonParty.InitializeMobileParty(new TroopRoster(this.HostileGarrisonParty.Party), new TroopRoster(this.HostileGarrisonParty.Party), base.QuestGiver.CurrentSettlement.GatePosition, 1f, 0.5f);
                HostileGarrisonParty.InitializeMobileParty(cultureGarrisonTemplate, base.QuestGiver.CurrentSettlement.GatePosition, 1f, 0.5f, 10);
                this.HostileGarrisonParty.SetCustomName(textObject);
                EnterSettlementAction.ApplyForParty(this.HostileGarrisonParty, base.QuestGiver.CurrentSettlement);
                this.HostileGarrisonParty.SetPartyUsedByQuest(true);


                // Player party consists of 5 milita and 4 of players best troops + hero
                PartyTemplateObject militiaPartyTemplate = QuestGiver.CurrentSettlement.Culture.MilitiaPartyTemplate;
                FlattenedTroopRoster bestTroops = MobilePartyHelper.GetStrongestAndPriorTroops(PartyBase.MainParty.MobileParty, 4, false);


                // Store our existing troops in _playerTroops
                foreach (TroopRosterElement troopRosterElement in PartyBase.MainParty.MemberRoster.GetTroopRoster())
                {
                    if (!troopRosterElement.Character.IsPlayerCharacter)
                    {
                        this._playerTroops.Add(troopRosterElement);
                    }
                }

                PartyBase.MainParty.MemberRoster.RemoveIf((TroopRosterElement t) => !t.Character.IsPlayerCharacter);
                PartyBase.MainParty.MemberRoster.Add(bestTroops);
                PartyBase.MainParty.MobileParty.AddElementToMemberRoster(militiaPartyTemplate.Stacks[0].Character, 5);

                PlayerEncounter.RestartPlayerEncounter(this.HostileGarrisonParty.Party, PartyBase.MainParty, false);
                GameMenu.ActivateGameMenu("town_uprising_quest_after_fight");
                this._isReadyToBeFinalized = true;
                PlayerEncounter.Current.ForceAlleyFight = true;
                PlayerEncounter.StartBattle();
                PlayerEncounter.StartAlleyFightMission();
            }

            private bool town_uprising_quest_wait_duration_is_over_yes_condition(MenuCallbackArgs args)
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Continue;
                return true;
            }

            private bool town_uprising_quest_wait_duration_is_over_no_condition(MenuCallbackArgs args)
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }

            private void town_uprising_wait_duration_is_over_menu_on_init(MenuCallbackArgs args)
            {
                Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
            }

            private void rival_gang_quest_wait_duration_is_over_yes_consequence(MenuCallbackArgs args)
            {
                CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, null, true, true, false, false), new ConversationCharacterData(base.QuestGiver.CharacterObject, null, true, true, false, false));
            }

            private void rival_gang_quest_wait_duration_is_over_no_consequence(MenuCallbackArgs args)
            {
                Campaign.Current.CurrentMenuContext.SwitchToMenu("town_wait_menus");
            }

            private void town_uprising_quest_before_fight_init(MenuCallbackArgs args)
            {
                if (_isFinalStage)
                {
                    StartCommonAreaBattle();
                }
            }

            private void town_uprising_quest_after_fight_init(MenuCallbackArgs args)
            {
                if (_isReadyToBeFinalized)
                {
                    bool hasPlayerWon = PlayerEncounter.Battle.WinningSide == PlayerEncounter.Battle.PlayerSide;
                    PlayerEncounter.Current.FinalizeBattle();
                    HandlePlayerEncounterResult(hasPlayerWon);
                    _isReadyToBeFinalized = false;
                }
            }

            private void HandlePlayerEncounterResult(bool hasPlayerWon)
            {
                PlayerEncounter.Finish(false);
                EncounterManager.StartSettlementEncounter(MobileParty.MainParty, base.QuestGiver.CurrentSettlement);
                GameMenu.SwitchToMenu("town");
                PartyBase.MainParty.MemberRoster.RemoveIf((TroopRosterElement t) => !t.Character.IsPlayerCharacter);
                foreach (TroopRosterElement troopRosterElement in this._playerTroops)
                {
                    PartyBase.MainParty.MemberRoster.AddToCounts(troopRosterElement.Character, troopRosterElement.Number, false, troopRosterElement.WoundedNumber, troopRosterElement.Xp, true, -1);
                }
                if (hasPlayerWon)
                {
                    this.AddQuestGiverGangLeaderOnSuccessDialogFlow();
                    PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(LocationComplex.Current.GetLocationOfCharacter(base.QuestGiver), null, base.QuestGiver.CharacterObject, null);
                    return;
                }
                else
                {
                    this.OnQuestFailedWithDefeat();
                    return;
                }
            }

            protected override void OnTimedOut()
            {
                base.AddLog(this.StageFailureTimeoutLogText, false);
                TraitLevelingHelper.OnIssueFailed(base.QuestGiver, new Tuple<TraitObject, int>[]
                {
        new Tuple<TraitObject, int>(DefaultTraits.Honor, -20)
                });
                this.ApplyQuestFailConsequences();
            }

            private void OnQuestAccepted()
            {
                base.StartQuest();
                this.PlayerAcceptedQuestLog = base.AddLog(StageOnePlayerAcceptsQuestLogText, false);
                Campaign.Current.ConversationManager.AddDialogFlow(this.GetQuestGiverPreparationCompletedDialogFlow(), this);
            }

            private void OnQuestSucceeded()
            {
                this._onQuestSucceededLog = base.AddLog(this.StageSuccessLogText, false);
                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, RewardGold, false);
                this.RelationshipChangeWithQuestGiver = 15;
                base.QuestGiver.AddPower(50f);

                Settlement settlement = QuestGiver.CurrentSettlement;
                //CampaignBehaviorBase rebellionBehaviour = CampaignBehaviorBase.GetCampaignBehavior<RebellionsCampaignBehavior>();
                //MethodInfo dynMethod = rebellionBehaviour.GetType().GetMethod("StartRebellionEvent", BindingFlags.NonPublic | BindingFlags.Instance);
                //dynMethod.Invoke(rebellionBehaviour, new object[] { settlement });

                if (this.HostileGarrisonParty != null)
                {
                    if (this.HostileGarrisonParty.IsActive)
                    {
                        DestroyPartyAction.Apply(null, this.HostileGarrisonParty);
                    }
                }

                base.CompleteQuestWithSuccess();
            }

            private void OnQuestFailedWithRejectionOrTimeout()
            {
                base.AddLog(this.StageFailureRejectionLogText, false);
                TraitLevelingHelper.OnIssueFailed(base.QuestGiver, new Tuple<TraitObject, int>[]
                {
        new Tuple<TraitObject, int>(DefaultTraits.Honor, -20)
                });
                this.ApplyQuestFailConsequences();
            }

            private void OnQuestFailedWithDefeat()
            {
                base.AddLog(StageFailureDefeatLogText, false);
                this.ApplyQuestFailConsequences();
                base.CompleteQuestWithFail(null);
            }

            private void ApplyQuestFailConsequences()
            {
                this.RelationshipChangeWithQuestGiver = -15;
                base.QuestGiver.AddPower(-10f);
                base.QuestGiver.CurrentSettlement.Town.Security += -10f;
                if (this.HostileGarrisonParty != null && this.HostileGarrisonParty.IsActive)
                {
                    DestroyPartyAction.Apply(null, this.HostileGarrisonParty);
                }
            }

            [SaveableField(1)]
            private MobileParty HostileGarrisonParty;

            [SaveableField(2)]
            private bool _isFinalStage;

            [SaveableField(3)]
            private bool _isReadyToBeFinalized;

            [SaveableField(4)]
            private List<TroopRosterElement> _playerTroops = new List<TroopRosterElement>();

            [SaveableField(5)]
            private bool _preparationsComplete;

            [SaveableField(6)]
            private JournalLog PlayerAcceptedQuestLog;

            [SaveableField(7)]
            private JournalLog _onQuestSucceededLog;
        }

        // Save data goes into this class
        public class NobleWantsTrainingBattleIssueTypeDefiner : SaveableTypeDefiner
        {
            public NobleWantsTrainingBattleIssueTypeDefiner() : base(585840)
            {
            }

            protected override void DefineClassTypes()
            {
                base.AddClassDefinition(typeof(TrainingBattleIssueBehavior.NobleWantsTrainingBattleIssue), 1);
                base.AddClassDefinition(typeof(TrainingBattleIssueBehavior.NobleWantsTrainingBattleQuest), 2);
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
