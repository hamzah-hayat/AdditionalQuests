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
                this.SetDialogs();
                base.InitializeQuestOnCreation();
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
                    return new TextObject("You've failed to stop the bandit army. The bandits ravaged {QUEST_SETTLEMENT} and left.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
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
                CampaignEvents.PlayerInventoryExchangeEvent.AddNonSerializedListener(this, new Action<List<ValueTuple<ItemRosterElement, int>>, List<ValueTuple<ItemRosterElement, int>>, bool>(this.OnPlayerInventoryExchange));
                CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction>(this.OnWarDeclared));
                CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, new Action<Clan, Kingdom, Kingdom, ChangeKingdomAction.ChangeKingdomActionDetail, bool>(this.OnClanChangedKingdom));
                CampaignEvents.RaidCompletedEvent.AddNonSerializedListener(this, new Action<BattleSideEnum, MapEvent>(this.OnRaidCompleted));
                CampaignEvents.MapEventStarted.AddNonSerializedListener(this, new Action<MapEvent, PartyBase, PartyBase>(this.OnMapEventStarted));
            }

            private void OnPlayerInventoryExchange(List<ValueTuple<ItemRosterElement, int>> purchasedItems, List<ValueTuple<ItemRosterElement, int>> soldItems, bool isTrading)
            {
                bool flag = false;
                foreach (ValueTuple<ItemRosterElement, int> valueTuple in purchasedItems)
                {
                    ItemRosterElement item = valueTuple.Item1;
                    if (item.EquipmentElement.Item.WeaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.OneHandedPolearm)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    foreach (ValueTuple<ItemRosterElement, int> valueTuple2 in soldItems)
                    {
                        ItemRosterElement item = valueTuple2.Item1;
                        if (item.EquipmentElement.Item.WeaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.OneHandedPolearm)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    this.PlayerAcceptedQuestLog.UpdateCurrentProgress(this.GetRequiredSpearsCountOnPlayer());
                    this.CheckIfPlayerReadyToReturnSpears();
                }
            }

            private void OnMercenaryClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom)
            {
                this.CheckWarDeclaration();
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
                if (base.QuestGiver.CurrentSettlement.OwnerClan.IsAtWarWith(Clan.PlayerClan))
                {
                    base.CompleteQuestWithCancel(this.StageCancelDueToWarLogText);
                }
            }

            private void OnRaidCompleted(BattleSideEnum battleSide, MapEvent mapEvent)
            {
                if (mapEvent.MapEventSettlement == base.QuestGiver.CurrentSettlement)
                {
                    base.CompleteQuestWithCancel(this.StageCancelDueToRaidLogText);
                }
            }

            private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
            {
                QuestHelper.CheckMinorMajorCoercionAndFailQuest(this, mapEvent, attackerParty);
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
                this.SetDialogs();
            }

            protected override void SetDialogs()
            {
                TextObject thankYouText = new TextObject("Thank you, {?PLAYER.GENDER}milady{?}sir{\\?}! We will make good use of these weapons.", null);
                thankYouText.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject);
                TextObject waitingText = new TextObject("We await those weapons, {?PLAYER.GENDER}milady{?}sir{\\?}.", null);
                waitingText.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject);


                this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine(thankYouText, null, null).Condition(() => CharacterObject.OneToOneConversationCharacter == base.QuestGiver.CharacterObject).Consequence(new ConversationSentence.OnConsequenceDelegate(this.QuestAcceptedConsequences)).CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine(new TextObject("Have you brought {SPEARS_AMOUNT} spears?", null), null, null).Condition(delegate
                {
                    MBTextManager.SetTextVariable("SPEARS_AMOUNT", this.NeededSpears);
                    return CharacterObject.OneToOneConversationCharacter == base.QuestGiver.CharacterObject;
                }).BeginPlayerOptions().PlayerOption(new TextObject("Yes. Here they are.", null), null).ClickableCondition(new ConversationSentence.OnClickableConditionDelegate(this.ReturnSpearsClickableConditions)).NpcLine(thankYouText, null, null).Consequence(delegate
                {
                    Campaign.Current.ConversationManager.ConversationEndOneShot += this.Success;
                }).CloseDialog().PlayerOption(new TextObject("I'm working on it.", null), null).NpcLine(waitingText, null, null).CloseDialog().EndPlayerOptions().CloseDialog();
            }

            private bool ReturnSpearsClickableConditions(out TextObject explanation)
            {
                if (this.PlayerAcceptedQuestLog.CurrentProgress >= this.NeededSpears)
                {
                    explanation = TextObject.Empty;
                    return true;
                }
                explanation = new TextObject("You don't have enough spears.", null);
                return false;
            }

            protected override void OnTimedOut()
            {
                base.AddLog(this.StageTimeoutLogText, false);
                this.Fail();
            }

            private void QuestAcceptedConsequences()
            {
                base.StartQuest();
                int requiredSpearsCountOnPlayer = this.GetRequiredSpearsCountOnPlayer();
                this.PlayerAcceptedQuestLog = base.AddDiscreteLog(this.StageOnePlayerAcceptsQuestLogText, new TextObject("Collect one handed polearms", null), requiredSpearsCountOnPlayer, this.NeededSpears, null, false);
            }

            private int GetRequiredSpearsCountOnPlayer()
            {
                int itemNumber = AdditionalQuestsHelperMethods.GetRequiredWeaponWithTypeCountOnPlayer(WeaponClass.OneHandedPolearm);
                if (itemNumber > this.NeededSpears)
                {
                    TextObject textObject = new TextObject("You have enough spears to complete the quest. Return to {QUEST_SETTLEMENT} to hand them over.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    InformationManager.AddQuickInformation(textObject, 0, null, "");
                }
                return itemNumber;
            }

            private void CheckIfPlayerReadyToReturnSpears()
            {
                if (this.PlayerHasNeededSpearsLog == null && this.PlayerAcceptedQuestLog.CurrentProgress >= this.NeededSpears)
                {
                    this.PlayerHasNeededSpearsLog = base.AddLog(this.StageTwoPlayerHasSpearsLogText, false);
                    return;
                }
                if (this.PlayerHasNeededSpearsLog != null && this.PlayerAcceptedQuestLog.CurrentProgress < this.NeededSpears)
                {
                    base.RemoveLog(this.PlayerHasNeededSpearsLog);
                    this.PlayerHasNeededSpearsLog = null;
                }
            }

            private void Success()
            {
                base.CompleteQuestWithSuccess();
                base.AddLog(this.StageSuccessLogText, false);
                TraitLevelingHelper.OnIssueSolvedThroughQuest(base.QuestGiver, new Tuple<TraitObject, int>[]
                {
                    new Tuple<TraitObject, int>(DefaultTraits.Mercy, 50),
                    new Tuple<TraitObject, int>(DefaultTraits.Generosity, 30)
                });
                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, this.RewardGold, false);
                // Remove spears
                AdditionalQuestsHelperMethods.RemoveWeaponsWithTypeFromPlayer(WeaponClass.OneHandedPolearm, NeededSpears);
                base.QuestGiver.AddPower(25f);
                Settlement.CurrentSettlement.Prosperity += 50f;
                Settlement.CurrentSettlement.Militia += 20f;
                this.RelationshipChangeWithQuestGiver = 10;
                ChangeRelationAction.ApplyPlayerRelation(base.QuestGiver, this.RelationshipChangeWithQuestGiver, true, true);
            }

            private void Fail()
            {
                base.QuestGiver.AddPower(-5f);
                base.QuestGiver.CurrentSettlement.Prosperity += -10f;
                this.RelationshipChangeWithQuestGiver = -5;
                ChangeRelationAction.ApplyPlayerRelation(base.QuestGiver, this.RelationshipChangeWithQuestGiver, true, true);
            }


            // Saved vars/logs
            [SaveableField(10)]
            private readonly int NeededSpears;

            [SaveableField(20)]
            private JournalLog PlayerAcceptedQuestLog;

            [SaveableField(30)]
            private JournalLog PlayerHasNeededSpearsLog;
        }

        internal class ExtortionByDesertersIssueQuest : QuestBase
        {
            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{=vbiA31xT}Extortion by Deserters at {SETTLEMENT}", null);
                    textObject.SetTextVariable("SETTLEMENT", this.QuestSettlement.Name);
                    return textObject;
                }
            }

            private ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult _questResultSuccess1
            {
                get
                {
                    return new ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult(0, 20, 1f, 4, 5, 10, 0, true);
                }
            }
            private ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult _questResultSuccess2
            {
                get
                {
                    return new ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult(0, 40, 0.5f, 6, 10, 10, 25, true);
                }
            }
            private ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult _questResultSuccess3
            {
                get
                {
                    return new ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult(2, 60, 0f, 8, 15, 10, 100, true);
                }
            }
            private ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult _questResultFail1
            {
                get
                {
                    return new ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult(0, -20, 0f, -10, -10, -10, -50, false);
                }
            }
            private ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult _questResultFail2
            {
                get
                {
                    return new ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult(0, -20, 0f, -10, -10, -10, -50, false);
                }
            }
            private ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult _questResultTimeOut
            {
                get
                {
                    return new ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult(0, -20, 0f, -10, 0, 0, 0, false);
                }
            }
            private ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult _questResultCancel1
            {
                get
                {
                    return new ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult(0, 0, 0f, 0, 0, 0, 0, false);
                }
            }
            private ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult _questResultCancel2
            {
                get
                {
                    return new ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult(0, 0, 0f, 0, 0, 0, 0, false);
                }
            }

            // Token: 0x17000EAC RID: 3756
            // (get) Token: 0x060047F4 RID: 18420 RVA: 0x00132EDC File Offset: 0x001310DC
            private TextObject _onQuestStartedLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=pWxXvtXP}{QUEST_GIVER.LINK}, told you that a group of deserters have been raiding their village regularly. {?QUEST_GIVER.GENDER}She{?}He{\\?} asked you to wait in {QUEST_SETTLEMENT} until the deserters arrive...", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", this.QuestSettlement.Name);
                    return textObject;
                }
            }

            // Token: 0x17000EAD RID: 3757
            // (get) Token: 0x060047F5 RID: 18421 RVA: 0x00132F23 File Offset: 0x00131123
            private TextObject _onQuestSucceededLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=IXkgPlKR}You have defeated the deserters and saved the people of {QUEST_SETTLEMENT}.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", this.QuestSettlement.Name);
                    return textObject;
                }
            }

            // Token: 0x17000EAE RID: 3758
            // (get) Token: 0x060047F6 RID: 18422 RVA: 0x00132F47 File Offset: 0x00131147
            private TextObject _onQuestFailed1LogText
            {
                get
                {
                    return new TextObject("{=bdWc1VEl}You've lost track of the deserter party.", null);
                }
            }

            // Token: 0x17000EAF RID: 3759
            // (get) Token: 0x060047F7 RID: 18423 RVA: 0x00132F54 File Offset: 0x00131154
            private TextObject _onQuestFailed2LogText
            {
                get
                {
                    return new TextObject("{=oYJCP3mt}You've failed to stop the deserters. The deserters ravaged the village and left.", null);
                }
            }

            // Token: 0x17000EB0 RID: 3760
            // (get) Token: 0x060047F8 RID: 18424 RVA: 0x00132F64 File Offset: 0x00131164
            private TextObject _onQuestTimedOutLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=YjxCkglX}You've failed to complete this quest in time. Your agreement with {QUEST_GIVER.LINK} was canceled.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            // Token: 0x17000EB1 RID: 3761
            // (get) Token: 0x060047F9 RID: 18425 RVA: 0x00132F94 File Offset: 0x00131194
            private TextObject _onQuestCancel1LogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=x346Rqle}Your clan is now at war with the {QUEST_GIVER.LINK}'s lord. Your agreement was canceled.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            // Token: 0x17000EB2 RID: 3762
            // (get) Token: 0x060047FA RID: 18426 RVA: 0x00132FC4 File Offset: 0x001311C4
            private TextObject _onQuestCancel2LogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=wTx2MNIJ}{QUEST_SETTLEMENT} was raided. {QUEST_GIVER.LINK} can no longer fulfill your contract. Agreement was canceled.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", this.QuestSettlement.Name);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            // Token: 0x17000EB3 RID: 3763
            // (get) Token: 0x060047FB RID: 18427 RVA: 0x0013300C File Offset: 0x0013120C
            private TextObject _onDeserterPartyDefeatedLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=sRBvUj6U}The deserter party is defeated. Return back to {QUEST_GIVER.LINK} to claim your rewards.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            // Token: 0x17000EB4 RID: 3764
            // (get) Token: 0x060047FC RID: 18428 RVA: 0x0013303C File Offset: 0x0013123C
            private TextObject _onPlayerLeftQuestSettlementNotificationText
            {
                get
                {
                    TextObject textObject = new TextObject("{=qjuiWN4K}{PLAYER.NAME}, you should wait with us in the village to ambush the deserters!", null);
                    StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, textObject);
                    return textObject;
                }
            }

            // Token: 0x17000EB5 RID: 3765
            // (get) Token: 0x060047FD RID: 18429 RVA: 0x00133068 File Offset: 0x00131268
            private TextObject _onPlayerDefeatedDesertersNotificationText
            {
                get
                {
                    TextObject textObject = new TextObject("{=EfZaCzb0}{PLAYER.NAME}, please return back to {QUEST_SETTLEMENT} to claim your rewards.", null);
                    StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, textObject);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", this.QuestSettlement.Name);
                    return textObject;
                }
            }

            // Token: 0x17000EB6 RID: 3766
            // (get) Token: 0x060047FE RID: 18430 RVA: 0x001330A9 File Offset: 0x001312A9
            private TextObject _onDesertersNoticedPlayerNotificationText
            {
                get
                {
                    return new TextObject("{=9vzm2j5T}Deserters have noticed our presence, they are running away!", null);
                }
            }

            // Token: 0x17000EB7 RID: 3767
            // (get) Token: 0x060047FF RID: 18431 RVA: 0x001330B8 File Offset: 0x001312B8
            private DialogFlow QuestCompletionDialogFlow
            {
                get
                {
                    return DialogFlow.CreateDialogFlow("start", 125).NpcLine("{=SCaWkKF1}Here is what we've promised, {GOLD_REWARD}{GOLD_ICON} denars. I hope this makes it worth the blood spilled.", null, null).Condition(delegate
                    {
                        MBTextManager.SetTextVariable("GOLD_REWARD", this.RewardGold);
                        return Hero.OneToOneConversationHero == base.QuestGiver && this._currentState == ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestState.DesertersAreDefeated;
                    }).BeginPlayerOptions().PlayerOption("{=Bb3oHQNa}Thanks, this will come in handy.", null).NpcLine("{=khIuyBAi}Thank you for your help. Farewell.", null, null).Consequence(delegate
                    {
                        ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult questResultSuccess = this._questResultSuccess1;
                        this.ApplyQuestResult(questResultSuccess);
                        base.AddLog(this._onQuestSucceededLogText, false);
                        base.CompleteQuestWithSuccess();
                    }).CloseDialog().PlayerOption("{=xcyr5Oq2}Half of the coin is enough for our needs.", null).NpcLine("{=SVrCpvpZ}Thank you {PLAYER.NAME}. Our people are grateful to you for what you have done today. Farewell.", null, null).Condition(() => true).Consequence(delegate
                    {
                        ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult questResultSuccess = this._questResultSuccess2;
                        this.ApplyQuestResult(questResultSuccess);
                        base.AddLog(this._onQuestSucceededLogText, false);
                        base.CompleteQuestWithSuccess();
                    }).CloseDialog().PlayerOption("{=52GFRUTE}Keep your coin, headman. Your people's blessings are enough.", null).NpcLine("{=1l5dKk1c}Oh, thank you {PLAYER.NAME}. You will always be remembered by our people. Farewell.", null, null).Condition(() => true).Consequence(delegate
                    {
                        ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult questResultSuccess = this._questResultSuccess3;
                        this.ApplyQuestResult(questResultSuccess);
                        base.AddLog(this._onQuestSucceededLogText, false);
                        base.CompleteQuestWithSuccess();
                    }).CloseDialog().EndPlayerOptions().CloseDialog();
                }
            }

            // Token: 0x17000EB8 RID: 3768
            // (get) Token: 0x06004800 RID: 18432 RVA: 0x001331CC File Offset: 0x001313CC
            private DialogFlow DeserterPartyAmbushedDialogFlow
            {
                get
                {
                    return DialogFlow.CreateDialogFlow("start", 125).NpcLine("{=s2btPjJf}Who the hell are you? If you live in this village, you'd better rustle up some silver and wine. Look lively, eh?", null, null).Condition(() => this._deserterMobileParty != null && this._deserterMobileParty.IsActive && CharacterObject.OneToOneConversationCharacter == this._deserterMobileParty.Leader && this._deserterMobileParty.Position2D.Distance(this.QuestSettlement.Position2D) <= 5f).BeginPlayerOptions().PlayerOption("{=Pp3koSqA}This time you'll have to fight for it!", null).CloseDialog().EndPlayerOptions().CloseDialog();
                }
            }

            // Token: 0x17000EB9 RID: 3769
            // (get) Token: 0x06004801 RID: 18433 RVA: 0x00133221 File Offset: 0x00131421
            private int DeserterPartyMenCount
            {
                get
                {
                    return 24 + MBMath.Ceiling(24f * this._questDifficultyMultiplier);
                }
            }

            // Token: 0x17000EBA RID: 3770
            // (get) Token: 0x06004802 RID: 18434 RVA: 0x00133237 File Offset: 0x00131437
            private int DefenderPartyMenCount
            {
                get
                {
                    return 16 + MBMath.Ceiling(16f * this._questDifficultyMultiplier);
                }
            }

            // Token: 0x17000EBB RID: 3771
            // (get) Token: 0x06004803 RID: 18435 RVA: 0x0013324D File Offset: 0x0013144D
            public Settlement QuestSettlement
            {
                get
                {
                    return base.QuestGiver.CurrentSettlement;
                }
            }

            // Token: 0x17000EBC RID: 3772
            // (get) Token: 0x06004804 RID: 18436 RVA: 0x0013325A File Offset: 0x0013145A
            public override bool IsRemainingTimeHidden
            {
                get
                {
                    return false;
                }
            }

            // Token: 0x06004805 RID: 18437 RVA: 0x00133260 File Offset: 0x00131460
            public ExtortionByDesertersIssueQuest(string questId, Hero questGiver, float difficultyMultiplier, int rewardGold, CampaignTime duration) : base(questId, questGiver, duration, rewardGold)
            {
                StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, null);
                this._questDifficultyMultiplier = difficultyMultiplier;
                this._defenderMobileParty = null;
                this._deserterBattleFinalizedForTheFirstTime = false;
                this._playerAwayFromSettlementNotificationSent = false;
                this.CreateDeserterParty();
                this._currentState = ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestState.DesertersMovingToSettlement;
                base.AddTrackedObject(this._deserterMobileParty);
                this.SetDialogs();
                base.InitializeQuestOnCreation();
            }

            // Token: 0x06004806 RID: 18438 RVA: 0x001332CC File Offset: 0x001314CC
            protected override void InitializeQuestOnGameLoad()
            {
                StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, null);
                this._playerAwayFromSettlementNotificationSent = false;
                if (this._currentState == ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestState.DesertersMovingToSettlement)
                {
                    float num = this._deserterMobileParty.Position2D.Distance(MobileParty.MainParty.Position2D);
                    bool flag = PlayerEncounter.Current != null && PlayerEncounter.EncounterSettlement == this.QuestSettlement;
                    bool flag2 = num <= this._deserterMobileParty.SeeingRange * 0.8f;
                    this._playerAwayFromSettlementNotificationSent = (!flag && !flag2);
                }
                this.SetDialogs();
                Campaign.Current.ConversationManager.AddDialogFlow(this.QuestCompletionDialogFlow, this);
                Campaign.Current.ConversationManager.AddDialogFlow(this.DeserterPartyAmbushedDialogFlow, this);
                if (this._defenderMobileParty != null && this._defenderMobileParty.IsActive)
                {
                    this._defenderMobileParty.Ai.SetDoNotMakeNewDecisions(true);
                }
                if (this._deserterMobileParty != null && this._deserterMobileParty.IsActive)
                {
                    this._deserterMobileParty.Ai.SetDoNotMakeNewDecisions(true);
                }
            }

            // Token: 0x06004807 RID: 18439 RVA: 0x001333D4 File Offset: 0x001315D4
            protected override void SetDialogs()
            {
                this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine("{=PnRVabwv}Thank you. Just wait in the village. We'll stand lookout and lure them into your ambush. Just wait for the signal.", null, null).Condition(() => Hero.OneToOneConversationHero == base.QuestGiver).Consequence(new ConversationSentence.OnConsequenceDelegate(this.OnQuestAccepted)).CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine("{=iN1kBsac}I don't think they'll be long now. Our hunters have spotted them making ready. Keep waiting.", null, null).Condition(() => Hero.OneToOneConversationHero == base.QuestGiver).BeginPlayerOptions().PlayerOption("{=IJihRdfF}Don't worry, we'll be ready for the fight.", null).NpcLine("{=U0UoayfA}Good, good. Thank you again.", null, null).CloseDialog().PlayerOption("{=bcGzpFSg}Are you sure about what your hunters saw? My men are starting to run out of patience.", null).NpcLine("{=YsASaPKq}I'm sure they'll be here soon. Please don't leave the village, or we'll stand no chance...", null, null).CloseDialog().EndPlayerOptions().CloseDialog();
                this.QuestCharacterDialogFlow = DialogFlow.CreateDialogFlow("start", 125).NpcLine("{=rAqyKcKZ}Who the hell are you? What do you want from us?", null, null).Condition(() => this._deserterMobileParty != null && this._deserterMobileParty.IsActive && CharacterObject.OneToOneConversationCharacter == this._deserterMobileParty.Leader && this._deserterMobileParty.Position2D.Distance(this.QuestSettlement.Position2D) >= 5f).BeginPlayerOptions().PlayerOption("{=Ljs9ahMk}I know your intentions. I will not let you steal from those poor villagers!", null).CloseDialog().EndPlayerOptions().CloseDialog();
            }

            // Token: 0x06004808 RID: 18440 RVA: 0x001334EC File Offset: 0x001316EC
            private void OnQuestAccepted()
            {
                base.StartQuest();
                Campaign.Current.ConversationManager.AddDialogFlow(this.QuestCompletionDialogFlow, this);
                Campaign.Current.ConversationManager.AddDialogFlow(this.DeserterPartyAmbushedDialogFlow, this);
                base.AddLog(this._onQuestStartedLogText, false);
            }

            // Token: 0x06004809 RID: 18441 RVA: 0x0013353C File Offset: 0x0013173C
            private void ApplyQuestResult(in ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult result)
            {
                int num = (int)(result.GoldMultiplier * (float)this.RewardGold);
                if (num > 0)
                {
                    GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, num, false);
                }
                if (result.QuestGiverPowerChange != 0)
                {
                    base.QuestGiver.AddPower((float)result.QuestGiverPowerChange);
                }
                if (result.TownSecurityChange != 0)
                {
                    this.QuestSettlement.Village.Bound.Town.Security += (float)result.TownSecurityChange;
                }
                if (result.TownProsperityChange != 0)
                {
                    this.QuestSettlement.Village.Bound.Town.Settlement.Prosperity += (float)result.TownProsperityChange;
                }
                if (result.HonorChange != 0)
                {
                    if (result.IsSuccess)
                    {
                        TraitLevelingHelper.OnIssueSolvedThroughQuest(base.QuestGiver, new Tuple<TraitObject, int>[]
                        {
                    new Tuple<TraitObject, int>(DefaultTraits.Honor, result.HonorChange)
                        });
                    }
                    else
                    {
                        TraitLevelingHelper.OnIssueFailed(base.QuestGiver, new Tuple<TraitObject, int>[]
                        {
                    new Tuple<TraitObject, int>(DefaultTraits.Honor, result.HonorChange)
                        });
                    }
                }
                if (result.QuestGiverRelationChange != 0)
                {
                    this.RelationshipChangeWithQuestGiver = result.QuestGiverRelationChange;
                }
                if (result.RenownChange > 0)
                {
                    GainRenownAction.Apply(Hero.MainHero, (float)result.RenownChange, false);
                }
            }

            // Token: 0x0600480A RID: 18442 RVA: 0x00133674 File Offset: 0x00131874
            protected override void RegisterEvents()
            {
                CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, new Action(this.OnHourlyTickEvent));
                CampaignEvents.MapEventEnded.AddNonSerializedListener(this, new Action<MapEvent>(this.MapEventEnded));
                CampaignEvents.GameMenuOpened.AddNonSerializedListener(this, new Action<MenuCallbackArgs>(this.GameMenuOpened));
                CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction>(this.OnWarDeclared));
                CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, new Action<Clan, Kingdom, Kingdom, ChangeKingdomAction.ChangeKingdomActionDetail, bool>(this.OnClanChangedKingdom));
                CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.SettlementEntered));
                CampaignEvents.VillageBeingRaided.AddNonSerializedListener(this, new Action<Village>(this.OnVillageBeingRaided));
                CampaignEvents.MapEventStarted.AddNonSerializedListener(this, new Action<MapEvent, PartyBase, PartyBase>(this.OnMapEventStarted));
            }

            // Token: 0x0600480B RID: 18443 RVA: 0x00133739 File Offset: 0x00131939
            private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
            {
                QuestHelper.CheckMinorMajorCoercionAndFailQuest(this, mapEvent, attackerParty);
            }

            // Token: 0x0600480C RID: 18444 RVA: 0x00133743 File Offset: 0x00131943
            protected override void OnFinalize()
            {
                this.DestroyDefenderParty();
                this.ReleaseDeserterParty();
            }

            // Token: 0x0600480D RID: 18445 RVA: 0x00133754 File Offset: 0x00131954
            private void OnHourlyTickEvent()
            {
                if (this._deserterMobileParty == null || this._deserterMobileParty.MapEvent == null)
                {
                    switch (this._currentState)
                    {
                        case ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestState.DesertersMovingToSettlement:
                            {
                                float num = this._deserterMobileParty.Position2D.Distance(MobileParty.MainParty.Position2D);
                                bool flag = PlayerEncounter.Current != null && PlayerEncounter.EncounterSettlement == this.QuestSettlement;
                                bool flag2 = num <= this._deserterMobileParty.SeeingRange * 0.8f;
                                if (!flag)
                                {
                                    if (flag2)
                                    {
                                        InformationManager.AddQuickInformation(this._onDesertersNoticedPlayerNotificationText, 0, Hero.MainHero.CharacterObject, "");
                                        this.HandleDesertersRunningAway();
                                        this._currentState = ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestState.DesertersRunningAwayFromPlayer;
                                        return;
                                    }
                                    if (!this._playerAwayFromSettlementNotificationSent)
                                    {
                                        InformationManager.AddQuickInformation(this._onPlayerLeftQuestSettlementNotificationText, 0, base.QuestGiver.CharacterObject, "");
                                        this._playerAwayFromSettlementNotificationSent = true;
                                        return;
                                    }
                                }
                                else if (!this._deserterMobileParty.IsCurrentlyGoingToSettlement)
                                {
                                    SetPartyAiAction.GetActionForVisitingSettlement(this._deserterMobileParty, this.QuestSettlement);
                                    return;
                                }
                                break;
                            }
                        case ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestState.DesertersRunningAwayFromPlayer:
                            if (this._deserterMobileParty.Position2D.Distance(MobileParty.MainParty.Position2D) > MobileParty.MainParty.SeeingRange + 3f)
                            {
                                ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult questResultFail = this._questResultFail1;
                                this.ApplyQuestResult(questResultFail);
                                base.CompleteQuestWithFail(this._onQuestFailed1LogText);
                                return;
                            }
                            this.HandleDesertersRunningAway();
                            return;
                        case ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestState.DesertersDefeatedPlayer:
                            if (!this._deserterMobileParty.IsCurrentlyGoingToSettlement)
                            {
                                SetPartyAiAction.GetActionForVisitingSettlement(this._deserterMobileParty, this.QuestSettlement);
                            }
                            break;
                        default:
                            return;
                    }
                }
            }

            // Token: 0x0600480E RID: 18446 RVA: 0x001338D8 File Offset: 0x00131AD8
            private void MapEventEnded(MapEvent mapEvent)
            {
                if (mapEvent.IsPlayerMapEvent && this._deserterMobileParty != null && mapEvent.InvolvedParties.Contains(this._deserterMobileParty.Party))
                {
                    this._deserterBattleFinalizedForTheFirstTime = true;
                    if (mapEvent.WinningSide == mapEvent.PlayerSide)
                    {
                        base.AddLog(this._onDeserterPartyDefeatedLogText, false);
                        base.AddTrackedObject(base.QuestGiver);
                        base.AddTrackedObject(this.QuestSettlement);
                        InformationManager.AddQuickInformation(this._onPlayerDefeatedDesertersNotificationText, 0, base.QuestGiver.CharacterObject, "");
                        this._currentState = ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestState.DesertersAreDefeated;
                        return;
                    }
                    if (!this._deserterMobileParty.IsCurrentlyGoingToSettlement)
                    {
                        SetPartyAiAction.GetActionForVisitingSettlement(this._deserterMobileParty, this.QuestSettlement);
                    }
                    this._currentState = ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestState.DesertersDefeatedPlayer;
                }
            }

            // Token: 0x0600480F RID: 18447 RVA: 0x0013399C File Offset: 0x00131B9C
            private void GameMenuOpened(MenuCallbackArgs mArgs)
            {
                if (mArgs.MenuContext.GameMenu.StringId == "encounter" && this._deserterBattleFinalizedForTheFirstTime)
                {
                    this._deserterBattleFinalizedForTheFirstTime = false;
                    if (this._currentState == ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestState.DesertersAreDefeated)
                    {
                        this.DestroyDeserterParty();
                    }
                    this.DestroyDefenderParty();
                }
            }

            // Token: 0x06004810 RID: 18448 RVA: 0x001339E9 File Offset: 0x00131BE9
            private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification = true)
            {
                this.CheckWarDeclaration();
            }

            // Token: 0x06004811 RID: 18449 RVA: 0x001339F1 File Offset: 0x00131BF1
            private void OnWarDeclared(IFaction faction1, IFaction faction2)
            {
                this.CheckWarDeclaration();
            }

            // Token: 0x06004812 RID: 18450 RVA: 0x001339FC File Offset: 0x00131BFC
            private void CheckWarDeclaration()
            {
                if (base.QuestGiver.CurrentSettlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
                {
                    ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult questResultCancel = this._questResultCancel1;
                    this.ApplyQuestResult(questResultCancel);
                    base.CompleteQuestWithCancel(this._onQuestCancel1LogText);
                }
            }

            // Token: 0x06004813 RID: 18451 RVA: 0x00133A48 File Offset: 0x00131C48
            private void OnVillageBeingRaided(Village village)
            {
                if (village.Settlement == this.QuestSettlement)
                {
                    ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult questResultCancel = this._questResultCancel2;
                    this.ApplyQuestResult(questResultCancel);
                    base.CompleteQuestWithCancel(this._onQuestCancel2LogText);
                }
            }

            // Token: 0x06004814 RID: 18452 RVA: 0x00133A80 File Offset: 0x00131C80
            private void SettlementEntered(MobileParty party, Settlement settlement, Hero hero)
            {
                if (party == this._deserterMobileParty && settlement == this.QuestSettlement)
                {
                    bool flag = PlayerEncounter.Current != null && PlayerEncounter.EncounterSettlement == this.QuestSettlement;
                    if (this._currentState != ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestState.DesertersDefeatedPlayer && flag)
                    {
                        this.StartAmbushEncounter();
                        return;
                    }
                    if (PlayerCaptivity.IsCaptive && PlayerCaptivity.CaptorParty == this._deserterMobileParty.Party)
                    {
                        EndCaptivityAction.ApplyByEscape(Hero.MainHero, null);
                    }
                    ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult questResultFail = this._questResultFail2;
                    this.ApplyQuestResult(questResultFail);
                    base.CompleteQuestWithFail(this._onQuestFailed2LogText);
                }
            }

            // Token: 0x06004815 RID: 18453 RVA: 0x00133B14 File Offset: 0x00131D14
            protected override void OnTimedOut()
            {
                ExtortionByDesertersIssueBehavior.ExtortionByDesertersIssueQuest.ExtortionByDesertersQuestResult questResultTimeOut = this._questResultTimeOut;
                this.ApplyQuestResult(questResultTimeOut);
                base.AddLog(this._onQuestTimedOutLogText, false);
            }

            // Token: 0x06004816 RID: 18454 RVA: 0x00133B40 File Offset: 0x00131D40
            private void CreateDeserterParty()
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
                PartyTemplateObject defaultPartyTemplate = this.QuestSettlement.Culture.DefaultPartyTemplate;
                this._deserterMobileParty = BanditPartyComponent.CreateBanditParty("ebdi_deserters_party_1", clan, settlement.Hideout, false);
                TextObject customName = new TextObject("{=zT2b0v8y}Deserters Party", null);
                this._deserterMobileParty.Party.Owner = ((clan != null) ? clan.Leader : null);
                Vec2 position = this.FindBestSpawnPositionForDeserterParty();
                this._deserterMobileParty.InitializeMobileParty(defaultPartyTemplate, position, 0f, 0f, this.DeserterPartyMenCount);
                this._deserterMobileParty.SetCustomName(customName);
                int num = 0;
                foreach (TroopRosterElement troopRosterElement in this._deserterMobileParty.MemberRoster.GetTroopRoster())
                {
                    if (!troopRosterElement.Character.HasMount())
                    {
                        num += troopRosterElement.Number;
                    }
                }
                ItemObject itemObject = Items.All.GetRandomElementWithPredicate((ItemObject x) => x.IsMountable && x.Culture == this.QuestSettlement.Culture && !x.NotMerchandise && x.Tier == ItemObject.ItemTiers.Tier2);
                if (itemObject == null)
                {
                    itemObject = (MBObjectManager.Instance.GetObject<ItemObject>("vlandia_horse") ?? MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse"));
                }
                if (itemObject != null)
                {
                    this._deserterMobileParty.ItemRoster.AddToCounts(itemObject, num);
                }
                float num2 = MBMath.Absf(this._deserterMobileParty.FoodChange);
                int num3 = MBMath.Ceiling(base.QuestDueTime.RemainingDaysFromNow * num2);
                int num4 = num3 / 2;
                this._deserterMobileParty.ItemRoster.AddToCounts(DefaultItems.Grain, num4);
                int number = num3 - num4;
                this._deserterMobileParty.ItemRoster.AddToCounts(DefaultItems.Meat, number);
                this._deserterMobileParty.SetPartyUsedByQuest(true);
                this._deserterMobileParty.IgnoreByOtherPartiesTill(CampaignTime.Never);
                this._deserterMobileParty.Aggressiveness = 0f;
                this._deserterMobileParty.Ai.SetDoNotMakeNewDecisions(true);
                this._deserterMobileParty.Party.Visuals.SetMapIconAsDirty();
                SetPartyAiAction.GetActionForVisitingSettlement(this._deserterMobileParty, this.QuestSettlement);
            }

            // Token: 0x06004817 RID: 18455 RVA: 0x00133DE0 File Offset: 0x00131FE0
            private Vec2 FindBestSpawnPositionForDeserterParty()
            {
                MobileParty mainParty = MobileParty.MainParty;
                Vec2 getPosition2D = mainParty.GetPosition2D;
                float seeingRange = mainParty.SeeingRange;
                float num = seeingRange + 3f;
                float num2 = num * 1.25f;
                float maximumDistance = num2 * 3f;
                Vec2 result = getPosition2D;
                float num3 = float.MaxValue;
                int num4 = 0;
                MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
                do
                {
                    Vec2 vec = MobilePartyHelper.FindReachablePointAroundPosition(this._deserterMobileParty.Party, getPosition2D, num, seeingRange, false);
                    float num5;
                    if (mapDistanceModel.GetDistance(mainParty, vec, maximumDistance, out num5))
                    {
                        if (num5 < num3)
                        {
                            result = vec;
                            num3 = num5;
                        }
                        if (num5 < num2)
                        {
                            break;
                        }
                    }
                    num4++;
                }
                while (num4 < 16);
                return result;
            }

            // Token: 0x06004818 RID: 18456 RVA: 0x00133E82 File Offset: 0x00132082
            private void DestroyDeserterParty()
            {
                if (this._deserterMobileParty != null && this._deserterMobileParty.IsActive)
                {
                    DestroyPartyAction.Apply(null, this._deserterMobileParty);
                    this._deserterMobileParty = null;
                }
            }

            // Token: 0x06004819 RID: 18457 RVA: 0x00133EAC File Offset: 0x001320AC
            private void ReleaseDeserterParty()
            {
                if (this._deserterMobileParty != null && this._deserterMobileParty.IsActive)
                {
                    this._deserterMobileParty.SetPartyUsedByQuest(false);
                    this._deserterMobileParty.IgnoreByOtherPartiesTill(CampaignTime.HoursFromNow(0f));
                    this._deserterMobileParty.Aggressiveness = 1f;
                    if (this._deserterMobileParty.CurrentSettlement != null)
                    {
                        LeaveSettlementAction.ApplyForParty(this._deserterMobileParty);
                    }
                    this._deserterMobileParty.Ai.SetDoNotMakeNewDecisions(false);
                }
            }

            // Token: 0x0600481A RID: 18458 RVA: 0x00133F28 File Offset: 0x00132128
            private void CreateDefenderParty()
            {
                PartyTemplateObject militiaPartyTemplate = this.QuestSettlement.Culture.MilitiaPartyTemplate;
                this._defenderMobileParty = MobileParty.CreateParty("ebdi_defender_party_1", null, null);
                TextObject textObject = new TextObject("{=dPU8UbKA}{QUEST_GIVER}'s Party", null);
                textObject.SetTextVariable("QUEST_GIVER", base.QuestGiver.Name);
                this._defenderMobileParty.InitializeMobileParty(militiaPartyTemplate, this.QuestSettlement.GetPosition2D, 1f, 0.5f, this.DefenderPartyMenCount);
                this._defenderMobileParty.SetCustomName(textObject);
                this._defenderMobileParty.SetPartyUsedByQuest(true);
                this._defenderMobileParty.Party.Owner = base.QuestGiver;
                this._defenderMobileParty.Aggressiveness = 1f;
                this._defenderMobileParty.ShouldJoinPlayerBattles = true;
            }

            // Token: 0x0600481B RID: 18459 RVA: 0x00133FEC File Offset: 0x001321EC
            private void DestroyDefenderParty()
            {
                if (this._defenderMobileParty != null && this._defenderMobileParty.IsActive)
                {
                    DestroyPartyAction.Apply(null, this._defenderMobileParty);
                    this._defenderMobileParty = null;
                }
            }

            // Token: 0x0600481C RID: 18460 RVA: 0x00134018 File Offset: 0x00132218
            private void HandleDesertersRunningAway()
            {
                Vec2 v = this._deserterMobileParty.Position2D - MobileParty.MainParty.Position2D;
                v.Normalize();
                float f = this._deserterMobileParty.GetCachedPureSpeed() * 1.5f;
                Vec2 moveGoToPoint = this._deserterMobileParty.Position2D + v * f;
                float num;
                moveGoToPoint = this.FindFreePositionBetweenPointAndParty(this._deserterMobileParty, moveGoToPoint, out num, 10f, 1E-05f, 1000f, 1.5f);
                if (num <= 1E-05f)
                {
                    v.RotateCCW(-1.5707964f + (float)MBRandom.RandomInt(0, 2) * 3.1415927f);
                    moveGoToPoint = this._deserterMobileParty.Position2D + v * f;
                    moveGoToPoint = this.FindFreePositionBetweenPointAndParty(this._deserterMobileParty, moveGoToPoint, out num, 10f, 1E-05f, 1000f, 1.5f);
                }
                this._deserterMobileParty.SetMoveGoToPoint(moveGoToPoint);
            }

            // Token: 0x0600481D RID: 18461 RVA: 0x00134103 File Offset: 0x00132303
            private void StartAmbushEncounter()
            {
                EncounterManager.StartPartyEncounter(this._deserterMobileParty.Party, MobileParty.MainParty.Party);
                this.CreateDefenderParty();
            }

            // Token: 0x0600481E RID: 18462 RVA: 0x00134128 File Offset: 0x00132328
            private Vec2 FindFreePositionBetweenPointAndParty(MobileParty party, in Vec2 point, out float distance, float maxIterations = 10f, float acceptThres = 1E-05f, float maxPathDistance = 1000f, float euclideanThressholdFactor = 1.5f)
            {
                IMapScene mapSceneWrapper = Campaign.Current.MapSceneWrapper;
                Vec2 position2D = party.Position2D;
                PathFaceRecord faceIndex = mapSceneWrapper.GetFaceIndex(position2D);
                Vec2 vec = position2D;
                distance = 0f;
                if (PartyBase.IsPositionOkForTraveling(position2D))
                {
                    Vec2 vec2 = point;
                    PathFaceRecord faceIndex2 = mapSceneWrapper.GetFaceIndex(vec2);
                    Vec2 v = point;
                    float num = acceptThres * acceptThres;
                    int num2 = 0;
                    while ((float)num2 < maxIterations && vec.DistanceSquared(point) > num)
                    {
                        float num3 = position2D.Distance(vec2);
                        float num4;
                        bool pathDistanceBetweenAIFaces = Campaign.Current.MapSceneWrapper.GetPathDistanceBetweenAIFaces(faceIndex, faceIndex2, position2D, vec2, 0.2f, maxPathDistance, out num4);
                        bool flag = num4 < num3 * euclideanThressholdFactor;
                        if (pathDistanceBetweenAIFaces && flag)
                        {
                            vec = vec2;
                            distance = num3;
                            vec2 = 0.5f * (vec2 + v);
                        }
                        else
                        {
                            v = vec2;
                            vec2 = 0.5f * (vec + v);
                        }
                        faceIndex2 = mapSceneWrapper.GetFaceIndex(vec2);
                        num2++;
                    }
                }
                return vec;
            }

            [SaveableField(1)]
            private MobileParty _deserterMobileParty;

            [SaveableField(2)]
            private BanditArmyRaidQuestState CurrentQuestState;

            [SaveableField(3)]
            private readonly float _questDifficultyMultiplier;

            [SaveableField(4)]
            private bool _deserterBattleFinalizedForTheFirstTime;

            private bool _playerAwayFromSettlementNotificationSent;

            private readonly struct ExtortionByDesertersQuestResult
            {
                // Token: 0x060050BD RID: 20669 RVA: 0x001523EF File Offset: 0x001505EF
                public ExtortionByDesertersQuestResult(int renownChange, int honorChange, float goldMultiplier, int questGiverRelationChange, int questGiverPowerChange, int townSecurityChange, int townProsperityChange, bool isSuccess)
                {
                    this.RenownChange = renownChange;
                    this.HonorChange = honorChange;
                    this.GoldMultiplier = goldMultiplier;
                    this.QuestGiverRelationChange = questGiverRelationChange;
                    this.QuestGiverPowerChange = questGiverPowerChange;
                    this.TownSecurityChange = townSecurityChange;
                    this.TownProsperityChange = townProsperityChange;
                    this.IsSuccess = isSuccess;
                }

                // Token: 0x04001D7D RID: 7549
                public readonly int RenownChange;

                // Token: 0x04001D7E RID: 7550
                public readonly int HonorChange;

                // Token: 0x04001D7F RID: 7551
                public readonly float GoldMultiplier;

                // Token: 0x04001D80 RID: 7552
                public readonly int QuestGiverRelationChange;

                // Token: 0x04001D81 RID: 7553
                public readonly int QuestGiverPowerChange;

                // Token: 0x04001D82 RID: 7554
                public readonly int TownSecurityChange;

                // Token: 0x04001D83 RID: 7555
                public readonly int TownProsperityChange;

                // Token: 0x04001D84 RID: 7556
                public readonly bool IsSuccess;
            }

            internal enum BanditArmyRaidQuestState
            {
                BanditArmyMovingToSettlement,
                BanditArmyRunningAwayFromPlayer,
                BanditArmyDefeatedPlayer,
                BanditArmyAreDefeated
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
