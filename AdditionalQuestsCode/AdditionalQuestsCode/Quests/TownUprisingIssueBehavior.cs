using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using System.Reflection;

namespace AdditionalQuestsCode.Quests
{
    public class TownUprisingIssueBehavior : CampaignBehaviorBase
    {
        // Needs to be a town notable with low loyalty and security rating
        // Also make sure it is not the same faction as the player
        private bool ConditionsHold(Hero issueGiver)
        {
            if (issueGiver.CurrentSettlement != null && issueGiver.IsArtisan)
            {
                Settlement currentSettlement = issueGiver.CurrentSettlement;
                if (currentSettlement.IsTown)
                {
                    Town town = currentSettlement.Town;
                    return town.Loyalty <= 50 && currentSettlement.OwnerClan.MapFaction != Hero.MainHero.MapFaction;
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
            return new TownUprisingIssueBehavior.TownUprisingIssue(issueOwner);
        }


        // Now the Issue
        internal class TownUprisingIssue : IssueBase
        {
            public TownUprisingIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(20f))
            {
            }

            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQTUTitle}Rebellion in {ISSUE_SETTLEMENT}!", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject Description
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQTUDescription}The people {ISSUE_SETTLEMENT} are angry at their mistreatment by their nobility. They are planning a rebellion!", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQTUIssueBrief}I trust you, so listen closely. The people of {ISSUE_SETTLEMENT} have been mistreated for far to long, so I have been organising a \"replacement\" of the clan that runs this town. Most of the milita are with me, and I have several captains willing to lead as our leaders. We just need help in convincing the garrison who are still loyal to our current rulers.", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject IssueAcceptByPlayer
            {
                get
                {
                    return new TextObject("{=AQTUIssueAccept}I can help with this rebellion, what do you need of me?", null);
                }
            }

            public override TextObject IssueQuestSolutionExplanationByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQTUIssueSolution}We want to takeover with as little blood spilled as possible, but we will still need to fight the remaining loyalist garrison here in the town. Meet me here with your meen at midnight, and we will start the coup. I will bring my men as well.", null);
                    return textObject;
                }
            }

            public override TextObject IssueQuestSolutionAcceptByPlayer
            {
                get
                {
                    return new TextObject("{=AQTUIssueSolutionAccept}Understood, I will meet with you soon.", null);
                }
            }

            public override TextObject IssueAsRumorInSettlement
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQTURumor}Theres talk around that {QUEST_GIVER.NAME} has convinced the town milita to overthrow the ruling clan. They've denied everything, but I'm not so sure...", null);
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
                return IssueSettlement.Town.Loyalty <= 60 && IssueSettlement.OwnerClan.MapFaction != Hero.MainHero.MapFaction;
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
                if (Clan.PlayerClan.Tier < 1)
                {
                    flag |= IssueBase.PreconditionFlags.ClanTier;
                }
                if (MobileParty.MainParty.MemberRoster.TotalHealthyCount < 15)
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
                return new TownUprisingQuest(questId, base.IssueOwner, CampaignTime.DaysFromNow(14f), this.RewardGold);
            }

            protected override void OnGameLoad()
            {
            }
        }

        internal class TownUprisingQuest : QuestBase
        {
            // Constructor with basic vars and any vars about the quest
            public TownUprisingQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
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
                    TextObject textObject = new TextObject("{=AQTUTitle}Rebellion in {ISSUE_SETTLEMENT}!", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageOnePlayerAcceptsQuestLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQTUQuestAcceptLog}{QUEST_GIVER.LINK}, an artisan in the town of {QUEST_SETTLEMENT} has asked you to help the town militia overthrow the garrison of the town. He has asked you to wait for midnight, which is when the fight will begin. \n \n Wait in {QUEST_SETTLEMENT} until midnight.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageTwoFightIsReadyLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQTUFightReady}{QUEST_GIVER.LINK}'s loyal soliders are ready for the aumbush at {SETTLEMENT}.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("SETTLEMENT", base.QuestGiver.CurrentSettlement.EncyclopediaLinkWithName);
                    return textObject;
                }
            }

            private TextObject StageSuccessLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQTUSuccessLog}You helped the rebels of {QUEST_SETTLEMENT} take over the town. A new kingdom has been formed!", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageFailureTimeoutLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQTUTimeoutLog}You have failed to join the rebels at {QUEST_SETTLEMENT}. The rebellion plan has been cancelled for now, {QUEST_GIVER.LINK} is disapointed at your lack of commitment.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageFailureRejectionLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQTURejectionLog}You decided to stay out of the fight. {QUEST_GIVER.LINK} has called off the aumbush and is disapointed at your refusal to fight.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            private TextObject StageFailureDefeatLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQTUFailure}You were unable to defeat the loyalist soldiers of {QUEST_SETTLEMENT}, the rebellion was a failure!", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageCancelDueToWarLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQTUCancelWarLog}Your clan is now at war with the rulers of {QUEST_SETTLEMENT}. It will be to difficult to help the rebels now.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageCancelDueToSiegeLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=AQTUCancelSiegeLog}{QUEST_GIVER.LINK} cancels your plans due to the siege of {SETTLEMENT}. The plans for the rebellion must now be put on hold.", null);
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
                TextObject textObject = new TextObject("{=AQTUPreparationComplete}{QUEST_GIVER.LINK} has finally sent a messenger telling you it's time to meet {?QUEST_GIVER.GENDER}her{?}him{\\?} and join the fight.", null);
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
                this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine("{=AQTUOfferDialog}Wait until midnight, I will send someone to you when we are ready. Try not to draw attention to yourself before then.", null, null).Condition(delegate
                {
                    return Hero.OneToOneConversationHero == base.QuestGiver;
                }).Consequence(new ConversationSentence.OnConsequenceDelegate(this.OnQuestAccepted)).CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine("{=AQTUDiscussDialog}It's not time yet. I'll send a runner for you when the time comes.", null, null).Condition(delegate
                {
                    return Hero.OneToOneConversationHero == base.QuestGiver && !this._isFinalStage && !this._preparationsComplete;
                }).BeginPlayerOptions().PlayerOption("{=AQTUWaitingForRunner}All right. I am waiting for your runner.", null).NpcLine("{=AQTUPrepNotFinished}You'll know right away once the preparations are complete. Just don't leave town.", null, null).CloseDialog().PlayerOption("{=AQTUWaitingImpatient}I can't just hang on here forever. Be quick about it.", null).NpcLine("{=AQTUPrepImpatient}I'm getting this together as quickly as I can.", null, null).CloseDialog().EndPlayerOptions().CloseDialog();
            }

            private DialogFlow GetQuestGiverPreparationCompletedDialogFlow()
            {
                return DialogFlow.CreateDialogFlow("start", 125).NpcLine("{=AQTUReadyDialog}Are you ready for the fight?", null, null).Condition(() => Hero.OneToOneConversationHero == base.QuestGiver && this._preparationsComplete && !this._isFinalStage).BeginPlayerOptions().PlayerOption("I am ready.", null).Condition(() => !Hero.MainHero.IsWounded).NpcLine("Let's finish this!", null, null).Consequence(delegate
                {
                    Campaign.Current.ConversationManager.ConversationEndOneShot += this.rival_gang_start_fight_on_consequence;
                }).CloseDialog().PlayerOption("{=AQTUNotReady}I need more time.", null).Condition(() => !Hero.MainHero.IsWounded).NpcLine("{=AQTUNotReadyHurryUp}You’d better hurry up!", null, null).Consequence(delegate
                {
                    Campaign.Current.ConversationManager.ConversationEndOneShot += this.rival_gang_need_more_time_on_consequence;
                }).CloseDialog().PlayerOption("{=AQTUNotReadyWounded}My wounds are still fresh. I need some time to recover.", null).Condition(() => Hero.MainHero.IsWounded).NpcLine("{=AQTUNotReadyHurryUpAlt}We must attack before the garrison hears about our plan. You'd better hurry up!", null, null).CloseDialog().EndPlayerOptions().CloseDialog();
            }

            // Game Menus
            public void AddGameMenus()
            {
                TextObject textObject = new TextObject("{=AQTUStartFightMenu}A milita soldier walks up to you and quietly informs you that the men are in position.", null);
                base.AddGameMenu("town_uprising_quest_before_fight", TextObject.Empty, new OnInitDelegate(town_uprising_quest_before_fight_init), GameOverlays.MenuOverlayType.SettlementWithBoth, GameMenu.MenuFlags.none);
                base.AddGameMenu("town_uprising_quest_after_fight", TextObject.Empty, new OnInitDelegate(town_uprising_quest_after_fight_init), GameOverlays.MenuOverlayType.SettlementWithBoth, GameMenu.MenuFlags.none);
                base.AddGameMenu("town_uprising_quest_wait_duration_is_over", textObject, new OnInitDelegate(town_uprising_wait_duration_is_over_menu_on_init), GameOverlays.MenuOverlayType.None, GameMenu.MenuFlags.none);
                base.AddGameMenuOption("town_uprising_quest_wait_duration_is_over", "town_uprising_quest_wait_duration_is_over_yes", new TextObject("{=AQTUStartFightOption}Follow the soldier", null), new GameMenuOption.OnConditionDelegate(this.town_uprising_quest_wait_duration_is_over_yes_condition), new GameMenuOption.OnConsequenceDelegate(this.rival_gang_quest_wait_duration_is_over_yes_consequence), false, -1, null);
                base.AddGameMenuOption("town_uprising_quest_wait_duration_is_over", "town_uprising_quest_wait_duration_is_over_no", new TextObject("{=AQTULeaveFightOption}Leave", null), new GameMenuOption.OnConditionDelegate(this.town_uprising_quest_wait_duration_is_over_no_condition), new GameMenuOption.OnConsequenceDelegate(this.rival_gang_quest_wait_duration_is_over_no_consequence), false, -1, null);
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
                Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 125).NpcLine("{=AQTUSuccessDialog}Its done! The remaining troops loyal to the noble have been rounded up and taken prisoner. This is the start of a new begining for the people.", null, null).Condition(delegate
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
                CampaignBehaviorBase rebellionBehaviour = CampaignBehaviorBase.GetCampaignBehavior<RebellionsCampaignBehavior>();
                MethodInfo dynMethod = rebellionBehaviour.GetType().GetMethod("StartRebellionEvent", BindingFlags.NonPublic | BindingFlags.Instance);
                dynMethod.Invoke(rebellionBehaviour, new object[] { settlement });

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
        public class TownUprisingIssueTypeDefiner : SaveableTypeDefiner
        {
            public TownUprisingIssueTypeDefiner() : base(585860)
            {
            }

            protected override void DefineClassTypes()
            {
                base.AddClassDefinition(typeof(TownUprisingIssueBehavior.TownUprisingIssue), 1);
                base.AddClassDefinition(typeof(TownUprisingIssueBehavior.TownUprisingQuest), 2);
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
