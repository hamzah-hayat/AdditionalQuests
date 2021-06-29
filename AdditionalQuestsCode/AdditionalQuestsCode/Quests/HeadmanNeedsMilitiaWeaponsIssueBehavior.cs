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
                    TextObject textObject = new TextObject("{ISSUE_SETTLEMENT} Needs Spears for Militia", null);
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
                    TextObject textObject = new TextObject("I can handle training my people into militia fighters, but I need someone to find spears to fight with. You can buy them in the surrounding towns. You can even make your own, if you are a decent smith. We will need {SPEARS_AMOUNT} spears in total. I will pay you {REWARD_AMOUNT}{GOLD_ICON} for the spears.", null);
                    textObject.SetTextVariable("SPEARS_AMOUNT", NeededSpearsNum);
                    textObject.SetTextVariable("REWARD_AMOUNT", RewardGold);
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
                    TextObject textObject = new TextObject("The young'uns of the village have all signed up to the newly forming militia. But I heard {QUEST_GIVER.NAME} doesnt have enough spears for even half of them men to train with, so they've been using wooden sticks to train with. I hope they dont end up having to use those same sticks in a real fight!.", null);
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
                return new HeadmanNeedsMilitiaWeaponsIssueBehavior.HeadmanNeedsMilitiaWeaponsQuest(questId, base.IssueOwner, CampaignTime.DaysFromNow(14f), this.RewardGold, NeededSpearsNum);
            }

            protected override void OnGameLoad()
            {
            }

            protected override int RewardGold
            {
                get
                {
                    return 500 + AdditionalQuestsHelperMethods.CalculateAveragePriceForWeaponClass(WeaponClass.OneHandedPolearm) * 20;
                }
            }

            protected int NeededSpearsNum
            {
                get
                {
                    return 20;
                }
            }

        }

        internal class HeadmanNeedsMilitiaWeaponsQuest : QuestBase
        {
            // Constructor with basic vars and any vars about the quest
            public HeadmanNeedsMilitiaWeaponsQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold,int spearsNumNeeded) : base(questId, questGiver, duration, rewardGold)
            {
                NeededSpears = spearsNumNeeded;
                this.SetDialogs();
                base.InitializeQuestOnCreation();
            }

            // All of our text/logs
            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{ISSUE_SETTLEMENT} needs spears for militia", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageOnePlayerAcceptsQuestLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{QUEST_GIVER.LINK}, a {?QUEST_GIVER.GENDER}headwomen{?}headman{\\?} of {QUEST_SETTLEMENT} asked you to deliver {SPEARS_AMOUNT} spears to {?QUEST_GIVER.GENDER}her{?}him{\\?} for the village militia. This will help boost the number of able militia in the village. \n \n You have agreed to bring them {SPEARS_AMOUNT} spears as soon as possible. You will be paid {REWARD_AMOUNT}{GOLD_ICON} for the spears. \n \n The spears should be one handed polearms.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    textObject.SetTextVariable("SPEARS_AMOUNT", this.NeededSpears);
                    textObject.SetTextVariable("REWARD_AMOUNT", RewardGold);
                    return textObject;
                }
            }

            private TextObject StageTwoPlayerHasSpearsLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You now have enough spears to complete the quest. Return to {QUEST_SETTLEMENT} to hand them over.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageTimeoutLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You have failed to deliver {SPEARS_AMOUNT} spears to the villagers. They wont be able to properly train their militia. The Headman is disappointed.", null);
                    textObject.SetTextVariable("SPEARS_AMOUNT", this.NeededSpears);
                    return textObject;
                }
            }

            private TextObject StageSuccessLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You have delivered {SPEARS_AMOUNT} spears to the villagers. Their militia is ready to train and defend the village. The Headman and the villagers are grateful.", null);
                    textObject.SetTextVariable("SPEARS_AMOUNT", this.NeededSpears);
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
                this.PlayerAcceptedQuestLog.UpdateCurrentProgress(this.GetRequiredSpearsCountOnPlayer());
                this.CheckIfPlayerReadyToReturnSpears();
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
                TextObject thankYouText = new TextObject("Thank you, {?PLAYER.GENDER}milady{?}sir{\\?}! We will make good use of these spears.", null);
                thankYouText.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject);
                TextObject waitingText = new TextObject("We await those spears, {?PLAYER.GENDER}milady{?}sir{\\?}.", null);
                waitingText.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject);


                this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine(thankYouText,null,null).Condition(() => CharacterObject.OneToOneConversationCharacter == base.QuestGiver.CharacterObject).Consequence(new ConversationSentence.OnConsequenceDelegate(this.QuestAcceptedConsequences)).CloseDialog();
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
                AdditionalQuestsHelperMethods.RemoveWeaponsWithTypeFromPlayer(WeaponClass.OneHandedPolearm,NeededSpears);
                base.QuestGiver.AddPower(25f);
                Settlement.CurrentSettlement.Prosperity += 50f;
                Settlement.CurrentSettlement.Militia += 20f;
                this.RelationshipChangeWithQuestGiver = 10;
                ChangeRelationAction.ApplyPlayerRelation(base.QuestGiver, this.RelationshipChangeWithQuestGiver, true, true);
            }

            private void Fail()
            {
                base.CompleteQuestWithFail();
                base.QuestGiver.AddPower(-15f);
                base.QuestGiver.CurrentSettlement.Prosperity += -25f;
                this.RelationshipChangeWithQuestGiver = -5;
                ChangeRelationAction.ApplyPlayerRelation(base.QuestGiver, this.RelationshipChangeWithQuestGiver, true, true);
            }


            // Saved vars/logs
            [SaveableField(1)]
            private readonly int NeededSpears;

            [SaveableField(2)]
            private JournalLog PlayerAcceptedQuestLog;

            [SaveableField(3)]
            private JournalLog PlayerHasNeededSpearsLog;
        }

        // Save data goes into this class
        public class HeadmanNeedsMilitiaWeaponsIssueTypeDefiner : SaveableTypeDefiner
        {
            public HeadmanNeedsMilitiaWeaponsIssueTypeDefiner() : base(585820)
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
