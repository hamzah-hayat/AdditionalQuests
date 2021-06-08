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
    public class HeadmanNeedsHardWoodIssueBehavior : CampaignBehaviorBase
    {
        // First, Start with our Conditions for this Quest to be selected
        // These Conditions are what set whether this Quest can be "Activated"
        private bool ConditionsHold(Hero issueGiver)
        {
            if (issueGiver.CurrentSettlement == null || !issueGiver.IsNotable || !issueGiver.CurrentSettlement.IsVillage)
            {
                return false;
            }

            if(issueGiver.CurrentSettlement.Village.GetProsperityLevel() < SettlementComponent.ProsperityLevel.Mid)
            {
                return true;
            }

            return false;
        }

        // If the conditions hold, start this quest, otherwise just add it as a possible quest
        public void OnCheckForIssue(Hero hero)
        {
            if (this.ConditionsHold(hero))
            {
                Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new PotentialIssueData.StartIssueDelegate(this.OnSelected), typeof(HeadmanNeedsHardWoodIssueBehavior.HeadmanNeedsHardWoodIssue), IssueBase.IssueFrequency.VeryCommon, null));
                return;
            }
            Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(HeadmanNeedsHardWoodIssueBehavior.HeadmanNeedsHardWoodIssue), IssueBase.IssueFrequency.VeryCommon));
        }

        private IssueBase OnSelected(in PotentialIssueData pid, Hero issueOwner)
        {
            return new HeadmanNeedsHardWoodIssueBehavior.HeadmanNeedsHardWoodIssue(issueOwner);
        }

        // Register any events we need
        public override void RegisterEvents()
        {
            CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, new Action<Hero>(this.OnCheckForIssue));
        }

        // This class stores the "Issue" which details how the quest is constructed
        // It has the starting conversation as well
        internal class HeadmanNeedsHardWoodIssue : IssueBase
        {

            private int NeededHardWoodAmount
            {
                get
                {
                    return (int)(30f + 50f * base.IssueDifficultyMultiplier);
                }
            }

            public override int AlternativeSolutionNeededMenCount
            {
                get
                {
                    return (int)(5f + 3f * base.IssueDifficultyMultiplier);
                }
            }

            protected override int AlternativeSolutionDurationInDays
            {
                get
                {
                    return (int)(10f + 7f * base.IssueDifficultyMultiplier);
                }
            }

            protected override int RewardGold
            {
                get
                {
                    return 0;
                }
            }

            private int CompanionTradeSkillLimit
            {
                get
                {
                    return (int)(75f * base.IssueDifficultyMultiplier);
                }
            }

            // Here we Store the TextObjects that are used by the Issue
            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{ISSUE_SETTLEMENT} Needs HardWood", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject Description
            {
                get
                {
                    TextObject textObject = new TextObject("The headman of {ISSUE_SETTLEMENT} needs Hardwood to rebuild a broken barn.", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    return new TextObject("The other day a barn collapsed, thankfully, no lives were lost but we will need to rebuild it for the coming harvest. Without a barn to store our food we will have trouble making it past the next winter.", null);
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
                    TextObject textObject = new TextObject("There are plenty of willing labourers in the village that could help rebuild, the main problem is finding the wood necessary. If you can find us {HARDWOOD_AMOUNT} logs of wood, we can handle the rebuilding work. The village would be extremely grateful.", null);
                    textObject.SetTextVariable("HARDWOOD_AMOUNT", this.NeededHardWoodAmount);
                    return textObject;
                }
            }

            public override TextObject IssueAlternativeSolutionExplanationByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("I know you're busy, but maybe you can ask some of your men to find us that wood? {MEN_COUNT} men should do the job, and I'd reckon the whole affair should take two weeks. \nI'm desperate here, {?PLAYER.GENDER}madam{?}sir{\\?}... We need that barn!", null);
                    textObject.SetTextVariable("MEN_COUNT", this.AlternativeSolutionNeededMenCount);
                    textObject.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject);
                    return textObject;
                }
            }

            public override TextObject IssueQuestSolutionAcceptByPlayer
            {
                get
                {
                    return new TextObject("I will find the wood you need.", null);
                }
            }

            public override TextObject IssueAlternativeSolutionAcceptByPlayer
            {
                get
                {
                    TextObject textObject = new TextObject("I can order one of my companions and {MEN_COUNT} men to find wood for you.", null);
                    textObject.SetTextVariable("MEN_COUNT", this.AlternativeSolutionNeededMenCount);
                    return textObject;
                }
            }

            public override TextObject IssueAsRumorInSettlement
            {
                get
                {
                    TextObject textObject = new TextObject("Did you hear? A barn fell down just a few days ago, was a miracle noone was hurt. I hear {QUEST_GIVER.NAME} is at their wits end handling the repairs for it. {?QUEST_GIVER.GENDER}She{?}He{\\?} says there's not enough wood in the village to fix it.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.IssueOwner.CharacterObject, textObject);
                    return textObject;
                }
            }

            public override TextObject IssueAlternativeSolutionResponseByIssueGiver
            {
                get
                {
                    return new TextObject("Thank you, {?PLAYER.GENDER}milady{?}sir{\\?}! I will remember your kindness.", null);
                }
            }

            public override bool IsThereAlternativeSolution
            {
                get
                {
                    return true;
                }
            }

            public override bool IsThereLordSolution
            {
                get
                {
                    return false;
                }
            }

            protected override TextObject AlternativeSolutionStartLog
            {
                get
                {
                    TextObject textObject = new TextObject("{ISSUE_OWNER.LINK}, the headman of {ISSUE_SETTLEMENT}, asked you to deliver {HARDWOOD_AMOUNT} logs of wood to {?QUEST_GIVER.GENDER}her{?}him{\\?} to use as construction materials. Otherwise the peasants will have nowhere to store their food for the upcoming winter. You have agreed to send your companion {COMPANION.NAME} along with {MEN_COUNT} men to find some wood and return to the village. Your men should return in {RETURN_DAYS} days.", null);
                    StringHelpers.SetCharacterProperties("ISSUE_OWNER", base.IssueOwner.CharacterObject, textObject);
                    StringHelpers.SetCharacterProperties("COMPANION", base.AlternativeSolutionHero.CharacterObject, textObject);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    textObject.SetTextVariable("HARDWOOD_AMOUNT", this.NeededHardWoodAmount);
                    textObject.SetTextVariable("RETURN_DAYS", this.AlternativeSolutionDurationInDays);
                    textObject.SetTextVariable("MEN_COUNT", this.AlternativeSolutionSentTroops.TotalManCount - 1);
                    return textObject;
                }
            }

            protected override ValueTuple<SkillObject, int> CompanionSkillAndRewardXP
            {
                get
                {
                    return new ValueTuple<SkillObject, int>(DefaultSkills.Trade, (int)(500f + 700f * base.IssueDifficultyMultiplier));
                }
            }

            public HeadmanNeedsHardWoodIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(30f))
            {
            }

            protected override Dictionary<IssueEffect, float> GetIssueEffectsAndAmountInternal()
            {
                return new Dictionary<IssueEffect, float>
                {
                    {
                        DefaultIssueEffects.SettlementProsperity,
                        -0.2f
                    },
                    {
                        DefaultIssueEffects.SettlementLoyalty,
                        -0.5f
                    }
                };
            }

            public override bool DoTroopsSatisfyAlternativeSolution(TroopRoster troopRoster, out TextObject explanation)
            {
                explanation = TextObject.Empty;
                return QuestHelper.CheckRosterForAlternativeSolution(troopRoster, this.AlternativeSolutionNeededMenCount, ref explanation, 0, false);
            }

            public override bool CompanionOrFamilyMemberClickableCondition(Hero companion, out TextObject explanation)
            {
                explanation = TextObject.Empty;
                return QuestHelper.CheckCompanionForAlternativeSolution(companion.CharacterObject, ref explanation, this.GetAlternativeSolutionRequiredCompanionSkills(), null);
            }

            private Dictionary<SkillObject, int> GetAlternativeSolutionRequiredCompanionSkills()
            {
                return new Dictionary<SkillObject, int>
                {
                    {
                        DefaultSkills.Trade,
                        this.CompanionTradeSkillLimit
                    }
                };
            }

            public override bool AlternativeSolutionCondition(out TextObject explanation)
            {
                Dictionary<SkillObject, int> alternativeSolutionRequiredCompanionSkills = this.GetAlternativeSolutionRequiredCompanionSkills();
                explanation = TextObject.Empty;
                return QuestHelper.CheckAllCompanionsCondition(MobileParty.MainParty.MemberRoster, ref explanation, alternativeSolutionRequiredCompanionSkills, null) && QuestHelper.CheckRosterForAlternativeSolution(MobileParty.MainParty.MemberRoster, this.AlternativeSolutionNeededMenCount, ref explanation, 0, false);
            }

            public override void AlternativeSolutionStartConsequence()
            {
                TextObject textObject = new TextObject("Your companion has set out to find {HARDWOOD_AMOUNT} logs of wood for the {ISSUE_OWNER.NAME}.", null);
                textObject.SetTextVariable("HARDWOOD_AMOUNT", this.NeededHardWoodAmount);
                StringHelpers.SetCharacterProperties("ISSUE_OWNER", base.IssueOwner.CharacterObject, textObject);
                InformationManager.AddQuickInformation(textObject, 0, null, "");
            }

            protected override void AlternativeSolutionEndConsequence()
            {
                TraitLevelingHelper.OnIssueSolvedThroughAlternativeSolution(base.IssueOwner, new Tuple<TraitObject, int>[]
                {
                    new Tuple<TraitObject, int>(DefaultTraits.Generosity, 20)
                });
                base.IssueOwner.AddPower(10f);
                base.IssueSettlement.Prosperity += 50f;
                this.RelationshipChangeWithIssueOwner = 6;
            }

            public override IssueBase.IssueFrequency GetFrequency()
            {
                return IssueBase.IssueFrequency.VeryCommon;
            }

            public override bool IssueStayAliveConditions()
            {
                return base.IssueOwner.CurrentSettlement.Village.GetProsperityLevel() < SettlementComponent.ProsperityLevel.Mid;
            }

            protected override void CompleteIssueWithTimedOutConsequences()
            {
            }

            protected override void OnGameLoad()
            {
            }

            protected override QuestBase GenerateIssueQuest(string questId)
            {
                return new HeadmanNeedsHardWoodIssueBehavior.HeadmanNeedsHardWoodQuest(questId, base.IssueOwner, CampaignTime.DaysFromNow(10f), base.IssueDifficultyMultiplier, this.RewardGold, this.NeededHardWoodAmount);
            }

            protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out IssueBase.PreconditionFlags flag, out Hero relationHero, out SkillObject skill)
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
        }

        internal class HeadmanNeedsHardWoodQuest : QuestBase
        {
            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{ISSUE_SETTLEMENT} Needs Wood", null);
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

            private TextObject _playerAcceptedQuestLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{QUEST_GIVER.LINK}, the headman of the {QUEST_SETTLEMENT} asked you to deliver {HARDWOOD_AMOUNT} logs of wood to {?QUEST_GIVER.GENDER}her{?}him{\\?} to use as building material. Otherwise the peasants will have nowhere to store their food for the upcoming winter. \n \n You have agreed to bring them {HARDWOOD_AMOUNT} logs of wood as soon as possible.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    textObject.SetTextVariable("HARDWOOD_AMOUNT", this._neededWoodAmount);
                    return textObject;
                }
            }

            private TextObject _playerHasNeededWoodLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You now have enough Hardwood to complete the quest. Return to {QUEST_SETTLEMENT} to hand them over.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject _questTimeoutLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You have failed to deliver {HARDWOOD_AMOUNT} logs of wood to the villagers. They won't be able to rebuild the barn before the coming winter. The Headman and the villagers are doomed.", null);
                    textObject.SetTextVariable("HARDWOOD_AMOUNT", this._neededWoodAmount);
                    return textObject;
                }
            }

            private TextObject _successLog
            {
                get
                {
                    TextObject textObject = new TextObject("You have delivered {HARDWOOD_AMOUNT} logs of wood to the villagers. They will be able to rebuild their barn before the coming winter. The Headman and the villagers are grateful.", null);
                    textObject.SetTextVariable("HARDWOOD_AMOUNT", this._neededWoodAmount);
                    return textObject;
                }
            }

            private TextObject _cancelLogOnWarDeclared
            {
                get
                {
                    TextObject textObject = new TextObject("Your clan is now at war with the {ISSUE_GIVER.LINK}'s lord. Your agreement with {ISSUE_GIVER.LINK} was canceled.", null);
                    StringHelpers.SetCharacterProperties("ISSUE_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            private TextObject _cancelLogOnVillageRaided
            {
                get
                {
                    TextObject textObject = new TextObject("{SETTLEMENT_NAME} is raided by someone else. Your agreement with {ISSUE_GIVER.LINK} was canceled.", null);
                    textObject.SetTextVariable("SETTLEMENT_NAME", base.QuestGiver.CurrentSettlement.Name);
                    StringHelpers.SetCharacterProperties("ISSUE_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            public HeadmanNeedsHardWoodQuest(string questId, Hero giverHero, CampaignTime duration, float difficultyMultiplier, int rewardGold, int neededWoodAmount) : base(questId, giverHero, duration, rewardGold)
            {
                this._neededWoodAmount = neededWoodAmount;
                this._rewardGold = rewardGold;
                this.SetDialogs();
                base.InitializeQuestOnCreation();
            }

            protected override void InitializeQuestOnGameLoad()
            {
                this.SetDialogs();
            }

            protected override void RegisterEvents()
            {
                CampaignEvents.PlayerInventoryExchangeEvent.AddNonSerializedListener(this, new Action<List<ValueTuple<ItemRosterElement, int>>, List<ValueTuple<ItemRosterElement, int>>, bool>(this.OnPlayerInventoryExchange));
                CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction>(this.OnWarDeclared));
                CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, new Action<Clan, Kingdom, Kingdom, bool, bool>(this.OnClanChangedKingdom));
                CampaignEvents.MercenaryClanChangedKingdom.AddNonSerializedListener(this, new Action<Clan, Kingdom, Kingdom>(this.OnMercenaryClanChangedKingdom));
                CampaignEvents.RaidCompletedEvent.AddNonSerializedListener(this, new Action<BattleSideEnum, MapEvent>(this.OnRaidCompleted));
                CampaignEvents.MapEventStarted.AddNonSerializedListener(this, new Action<MapEvent, PartyBase, PartyBase>(this.OnMapEventStarted));
            }

            private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
            {
                QuestHelper.CheckMinorMajorCoercionAndFailQuest(this, mapEvent, attackerParty);
            }

            protected override void OnTimedOut()
            {
                base.AddLog(this._questTimeoutLogText, false);
                this.Fail();
            }

            protected override void SetDialogs()
            {
                TextObject textObject = new TextObject("Thank you, {?PLAYER.GENDER}milady{?}sir{\\?}! You are a saviour.", null);
                TextObject textObject2 = new TextObject("We await your success, {?PLAYER.GENDER}milady{?}sir{\\?}.", null);
                textObject.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject);
                textObject2.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject);
                this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine(textObject, null, null).Condition(() => CharacterObject.OneToOneConversationCharacter == base.QuestGiver.CharacterObject).Consequence(new ConversationSentence.OnConsequenceDelegate(this.QuestAcceptedConsequences)).CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine(new TextObject("Have you brought {WOOD_AMOUNT} logs of wood?", null), null, null).Condition(delegate
                {
                    MBTextManager.SetTextVariable("WOOD_AMOUNT", this._neededWoodAmount);
                    return CharacterObject.OneToOneConversationCharacter == base.QuestGiver.CharacterObject;
                }).BeginPlayerOptions().PlayerOption(new TextObject("Yes. Here is your wood.", null), null).ClickableCondition(new ConversationSentence.OnClickableConditionDelegate(this.ReturnWoodClickableConditions)).NpcLine(textObject, null, null).Consequence(delegate
                {
                    Campaign.Current.ConversationManager.ConversationEndOneShot += this.Success;
                }).CloseDialog().PlayerOption(new TextObject("I'm working on it.", null), null).NpcLine(textObject2, null, null).CloseDialog().EndPlayerOptions().CloseDialog();
            }

            private bool ReturnWoodClickableConditions(out TextObject explanation)
            {
                if (this._playerAcceptedQuestLog.CurrentProgress >= this._neededWoodAmount)
                {
                    explanation = TextObject.Empty;
                    return true;
                }
                explanation = new TextObject("You don't have enough wood.", null);
                return false;
            }

            private void QuestAcceptedConsequences()
            {
                base.StartQuest();
                int requiredWoodCountOnPlayer = this.GetRequiredWoodCountOnPlayer();
                this._playerAcceptedQuestLog = base.AddDiscreteLog(this._playerAcceptedQuestLogText, new TextObject("Collect Wood", null), requiredWoodCountOnPlayer, this._neededWoodAmount, null, false);
            }

            private int GetRequiredWoodCountOnPlayer()
            {
                int itemNumber = PartyBase.MainParty.ItemRoster.GetItemNumber(DefaultItems.HardWood);
                if (itemNumber >= this._neededWoodAmount)
                {
                    TextObject textObject = new TextObject("You have enough wood to complete the quest. Return to {QUEST_SETTLEMENT} to hand it over.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    InformationManager.AddQuickInformation(textObject, 0, null, "");
                }
                if (itemNumber <= this._neededWoodAmount)
                {
                    return itemNumber;
                }
                return this._neededWoodAmount;
            }

            private void CheckIfPlayerReadyToReturnWood()
            {
                if (this._playerHasNeededWoodLog == null && this._playerAcceptedQuestLog.CurrentProgress >= this._neededWoodAmount)
                {
                    this._playerHasNeededWoodLog = base.AddLog(this._playerHasNeededWoodLogText, false);
                    return;
                }
                if (this._playerHasNeededWoodLog != null && this._playerAcceptedQuestLog.CurrentProgress < this._neededWoodAmount)
                {
                    base.RemoveLog(this._playerHasNeededWoodLog);
                    this._playerHasNeededWoodLog = null;
                }
            }

            private void OnPlayerInventoryExchange(List<ValueTuple<ItemRosterElement, int>> purchasedItems, List<ValueTuple<ItemRosterElement, int>> soldItems, bool isTrading)
            {
                bool flag = false;
                foreach (ValueTuple<ItemRosterElement, int> valueTuple in purchasedItems)
                {
                    ItemRosterElement item = valueTuple.Item1;
                    if (item.EquipmentElement.Item == DefaultItems.HardWood)
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
                        if (item.EquipmentElement.Item == DefaultItems.HardWood)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    this._playerAcceptedQuestLog.UpdateCurrentProgress(this.GetRequiredWoodCountOnPlayer());
                    this.CheckIfPlayerReadyToReturnWood();
                }
            }

            private void OnMercenaryClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom)
            {
                this.CheckWarDeclaration();
            }

            private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, bool byRebellion, bool showNotification = true)
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
                    base.CompleteQuestWithCancel(this._cancelLogOnWarDeclared);
                }
            }

            private void OnRaidCompleted(BattleSideEnum battleSide, MapEvent mapEvent)
            {
                if (mapEvent.MapEventSettlement == base.QuestGiver.CurrentSettlement)
                {
                    base.CompleteQuestWithCancel(this._cancelLogOnVillageRaided);
                }
            }

            private void Success()
            {
                base.CompleteQuestWithSuccess();
                base.AddLog(this._successLog, false);
                TraitLevelingHelper.OnIssueSolvedThroughQuest(base.QuestGiver, new Tuple<TraitObject, int>[]
                {
                    new Tuple<TraitObject, int>(DefaultTraits.Mercy, 50),
                    new Tuple<TraitObject, int>(DefaultTraits.Generosity, 30)
                });
                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, this._rewardGold, false);
                GiveItemAction.ApplyForParties(PartyBase.MainParty, Settlement.CurrentSettlement.Party, DefaultItems.HardWood, this._neededWoodAmount);
                base.QuestGiver.AddPower(10f);
                Settlement.CurrentSettlement.Prosperity += 50f;
                this.RelationshipChangeWithQuestGiver = 8;
                ChangeRelationAction.ApplyPlayerRelation(base.QuestGiver, this.RelationshipChangeWithQuestGiver, true, true);
            }

            private void Fail()
            {
                base.QuestGiver.AddPower(-5f);
                base.QuestGiver.CurrentSettlement.Prosperity += -10f;
                this.RelationshipChangeWithQuestGiver = -5;
                ChangeRelationAction.ApplyPlayerRelation(base.QuestGiver, this.RelationshipChangeWithQuestGiver, true, true);
            }

            [SaveableField(10)]
            private readonly int _neededWoodAmount;

            [SaveableField(20)]
            private int _rewardGold;

            [SaveableField(30)]
            private JournalLog _playerAcceptedQuestLog;

            [SaveableField(40)]
            private JournalLog _playerHasNeededWoodLog;
        }

        // Save data goes into this class
        public class HeadmanNeedsHardWoodIssueTypeDefiner : SaveableTypeDefiner
        {
            public HeadmanNeedsHardWoodIssueTypeDefiner() : base(1000500)
            {
            }

            protected override void DefineClassTypes()
            {
                base.AddClassDefinition(typeof(HeadmanNeedsHardWoodIssueBehavior.HeadmanNeedsHardWoodIssue), 1);
                base.AddClassDefinition(typeof(HeadmanNeedsHardWoodIssueBehavior.HeadmanNeedsHardWoodQuest), 2);
            }
        }

        // No idea what this does but we override it anyway to fulfil base class
        // Maybe Multiplayer related?
        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}
