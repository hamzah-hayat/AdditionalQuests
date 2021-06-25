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
    public class StarvingTownNeedsFoodIssueBehavior : CampaignBehaviorBase
    {
        // Needs to be starving town, also merchant notable
        private bool ConditionsHold(Hero issueGiver)
        {
            return issueGiver.IsMerchant && issueGiver.CurrentSettlement != null && issueGiver.CurrentSettlement.IsTown && issueGiver.CurrentSettlement.IsStarving;
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
            return new StarvingTownNeedsFoodIssueBehavior.StarvingTownNeedsFoodIssue(issueOwner);
        }

        // Now the Issue
        internal class StarvingTownNeedsFoodIssue : IssueBase
        {
            public StarvingTownNeedsFoodIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(20f))
            {
            }

            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("Food crisis in {ISSUE_SETTLEMENT}", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject Description
            {
                get
                {
                    TextObject textObject = new TextObject("The town of {SETTLEMENT} is starving, {MERCHANT_NAME} needs help to restock the towns food supplies.", null);
                    textObject.SetTextVariable("SETTLEMENT", base.IssueSettlement.Name);
                    textObject.SetTextVariable("MERCHANT_NAME", base.IssueOwner.Name);
                    return textObject;
                }
            }

            public override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    return new TextObject("The food situation is bad, the town is starving. We've had to ration out all the food coming into the city but its getting harder and harder each day to put meals in front of people. If this keeps up for much longer, we will either all starve to death or the people will riot.", null);
                }
            }

            public override TextObject IssueAcceptByPlayer
            {
                get
                {
                    return new TextObject("Can I help? I have some food to spare.", null);
                }
            }

            public override TextObject IssueQuestSolutionExplanationByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("I thank you for your kindness, but we will require a larger amount of food to restock the city, enough to keep us going while we sort out the source of our food shortage. If you can supply Grain, Meat and Fish, I will pay you triple the average price for each. We will need at least 300 units of food in total to keep the town supplied.", null);
                    return textObject;
                }
            }

            public override TextObject IssueQuestSolutionAcceptByPlayer
            {
                get
                {
                    return new TextObject("I will head out to find food for the town.", null);
                }
            }

            public override TextObject IssueAsRumorInSettlement
            {
                get
                {
                    TextObject textObject = new TextObject("I'm so hungry... oh, sorry, Did I walk into you? Its been so hard lately... I haven't eaten in so long... If only someone could help {QUEST_GIVER.NAME} bring more food into town...", null);
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
                return IssueSettlement.IsStarving;
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
                return new StarvingTownNeedsFoodQuest(questId, base.IssueOwner, CampaignTime.DaysFromNow(14f), this.RewardGold);
            }

            protected override void OnGameLoad()
            {
            }
        }

        internal class StarvingTownNeedsFoodQuest : QuestBase
        {
            // Constructor with basic vars and any vars about the quest
            public StarvingTownNeedsFoodQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
            {
                this.SetDialogs();
                base.InitializeQuestOnCreation();
                NeededFood = 300;
            }


            // All of our text/logs
            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("Food crisis in {ISSUE_SETTLEMENT}", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageOnePlayerAcceptsQuestLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{QUEST_GIVER.LINK}, a merchant in the town of {QUEST_SETTLEMENT} asked you to deliver {FOOD_AMOUNT} food to the town, to help fulfil the current food crisis. The food can either be Grain, Meat or Fish. \n \n You will be paid triple the average price of each good on delivery.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    textObject.SetTextVariable("FOOD_AMOUNT", this.NeededFood);
                    return textObject;
                }
            }

            private TextObject StageTwoPlayerHasFoodLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You now have enough food to complete the quest. Return to {QUEST_SETTLEMENT} to hand it over.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageTimeoutLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You have failed to deliver {FOOD_AMOUNT} food to {QUEST_SETTLEMENT}. The people continue to starve and {QUEST_GIVER.LINK} is disappointed.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    textObject.SetTextVariable("FOOD_AMOUNT", this.NeededFood);
                    return textObject;
                }
            }

            private TextObject StageSuccessLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You have delivered {FOOD_AMOUNT} food to {QUEST_SETTLEMENT}. The people rejoice at the delivery of food. You have saved the people from starvation.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    textObject.SetTextVariable("FOOD_AMOUNT", this.NeededFood);
                    return textObject;
                }
            }

            private TextObject StageCancelDueToWarLogText
            {
                get
                {
                    TextObject textObject = new TextObject("Your clan is now at war with {ISSUE_GIVER.LINK}'s lord. Your agreement with {ISSUE_GIVER.LINK} was canceled.", null);
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
                CampaignEvents.MapEventStarted.AddNonSerializedListener(this, new Action<MapEvent, PartyBase, PartyBase>(this.OnMapEventStarted));
            }

            private void OnPlayerInventoryExchange(List<ValueTuple<ItemRosterElement, int>> purchasedItems, List<ValueTuple<ItemRosterElement, int>> soldItems, bool isTrading)
            {
                this.PlayerAcceptedQuestLog.UpdateCurrentProgress(this.GetFoodCountOnPlayer());
                this.CheckIfPlayerReadyToReturnFood();
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
                TextObject thankYouText = new TextObject("Thank you, {?PLAYER.GENDER}milady{?}sir{\\?}! This food will help us all.", null);
                thankYouText.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject);
                TextObject waitingText = new TextObject("We await the food supplies, {?PLAYER.GENDER}milady{?}sir{\\?}.", null);
                waitingText.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject);


                this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine(thankYouText, null, null).Condition(() => CharacterObject.OneToOneConversationCharacter == base.QuestGiver.CharacterObject).Consequence(new ConversationSentence.OnConsequenceDelegate(this.QuestAcceptedConsequences)).CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine(new TextObject("Have you brought {FOOD_AMOUNT} food?", null), null, null).Condition(delegate
                {
                    MBTextManager.SetTextVariable("FOOD_AMOUNT", this.NeededFood);
                    return CharacterObject.OneToOneConversationCharacter == base.QuestGiver.CharacterObject;
                }).BeginPlayerOptions().PlayerOption(new TextObject("Yes. Here is is.", null), null).ClickableCondition(new ConversationSentence.OnClickableConditionDelegate(this.ReturnFoodClickableConditions)).NpcLine(thankYouText, null, null).Consequence(delegate
                {
                    Campaign.Current.ConversationManager.ConversationEndOneShot += this.Success;
                }).CloseDialog().PlayerOption(new TextObject("I'm working on it.", null), null).NpcLine(waitingText, null, null).CloseDialog().EndPlayerOptions().CloseDialog();
            }

            private bool ReturnFoodClickableConditions(out TextObject explanation)
            {
                if (this.PlayerAcceptedQuestLog.CurrentProgress >= this.NeededFood)
                {
                    explanation = TextObject.Empty;
                    return true;
                }
                explanation = new TextObject("You don't have enough food.", null);
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
                this.PlayerAcceptedQuestLog = base.AddDiscreteLog(this.StageOnePlayerAcceptsQuestLogText, new TextObject("Collect Grain/Fish/Meat.", null), GetFoodCountOnPlayer(), this.NeededFood, null, false);
            }

            private int GetFoodCountOnPlayer()
            {
                int itemNumber = 0;
                foreach (ItemRosterElement itemRosterElement in PartyBase.MainParty.ItemRoster)
                {
                    if (itemRosterElement.EquipmentElement.Item != null)
                    {
                        if (itemRosterElement.EquipmentElement.Item.ItemCategory == DefaultItemCategories.Grain || itemRosterElement.EquipmentElement.Item.ItemCategory == DefaultItemCategories.Fish || itemRosterElement.EquipmentElement.Item.ItemCategory == DefaultItemCategories.Meat)
                        {
                            itemNumber += itemRosterElement.Amount;
                        }
                    }
                }

                if (itemNumber > this.NeededFood)
                {
                    TextObject textObject = new TextObject("You have enough food to complete the quest. Return to {QUEST_SETTLEMENT} to hand them over.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    InformationManager.AddQuickInformation(textObject, 0, null, "");
                }
                return itemNumber;
            }

            private void CheckIfPlayerReadyToReturnFood()
            {
                if (this.PlayerHasNeededFoodLog == null && this.PlayerAcceptedQuestLog.CurrentProgress >= this.NeededFood)
                {
                    this.PlayerHasNeededFoodLog = base.AddLog(this.StageTwoPlayerHasFoodLogText, false);
                    return;
                }
                if (this.PlayerHasNeededFoodLog != null && this.PlayerAcceptedQuestLog.CurrentProgress < this.NeededFood)
                {
                    base.RemoveLog(this.PlayerHasNeededFoodLog);
                    this.PlayerHasNeededFoodLog = null;
                }
            }

            private void Success()
            {
                base.CompleteQuestWithSuccess();
                base.AddLog(this.StageSuccessLogText, false);

                // Sell Meat first, then Fish, then Grain
                // Incrase foodstocks for town
                int foodSellingNum = NeededFood;
                int priceMultiplier = 3;


                int numSold = AdditionalQuestsHelperMethods.SellQuestItemForPlayer(DefaultItems.Meat, priceMultiplier, foodSellingNum); //Returns numSold, which is how many we were able to sell
                foodSellingNum -= numSold;

                TextObject sellMeatText = new TextObject("You gave {FOOD_AMOUNT} meat. In return you got {GOLD_AMOUNT}{GOLD_ICON}.", null);
                sellMeatText.SetTextVariable("FOOD_AMOUNT", numSold);
                sellMeatText.SetTextVariable("GOLD_AMOUNT", numSold * AdditionalQuestsHelperMethods.GetAveragePriceOfItem(DefaultItems.Meat) * priceMultiplier);
                base.AddLog(sellMeatText);

                // Use this foreach to find fish, not a default item
                foreach (var item in Items.AllTradeGoods)
                {
                    if (item.GetItemCategory() == DefaultItemCategories.Fish)
                    {
                        numSold = AdditionalQuestsHelperMethods.SellQuestItemForPlayer(item, priceMultiplier, foodSellingNum);
                        foodSellingNum -= numSold;
                        TextObject sellFishText = new TextObject("You gave {FOOD_AMOUNT} fish. In return you got {GOLD_AMOUNT}{GOLD_ICON}.", null);
                        sellFishText.SetTextVariable("FOOD_AMOUNT", numSold);
                        sellFishText.SetTextVariable("GOLD_AMOUNT", numSold * AdditionalQuestsHelperMethods.GetAveragePriceOfItem(item) * priceMultiplier);
                        base.AddLog(sellFishText);
                        break;
                    }
                }

                numSold = AdditionalQuestsHelperMethods.SellQuestItemForPlayer(DefaultItems.Grain, priceMultiplier, foodSellingNum);
                foodSellingNum -= numSold;
                TextObject sellGrainText = new TextObject("You gave {FOOD_AMOUNT} grain. In return you got {GOLD_AMOUNT}{GOLD_ICON}.", null);
                sellGrainText.SetTextVariable("FOOD_AMOUNT", numSold);
                sellGrainText.SetTextVariable("GOLD_AMOUNT", numSold * AdditionalQuestsHelperMethods.GetAveragePriceOfItem(DefaultItems.Grain) * priceMultiplier);
                base.AddLog(sellGrainText);



                QuestGiver.CurrentSettlement.Town.FoodStocks += 300;

                // Now do player effects eg add reknown
                Clan.PlayerClan.AddRenown(3f, false);

                // Now add power to notable and give relationship bonus
                base.QuestGiver.AddPower(25f);
                this.RelationshipChangeWithQuestGiver = 15;
                ChangeRelationAction.ApplyPlayerRelation(QuestGiver, this.RelationshipChangeWithQuestGiver, false, true);

                // also increase settlement prosperity
                Settlement.CurrentSettlement.Prosperity += 100f;
            }

            private void Fail()
            {
                base.QuestGiver.AddPower(-25f);
                base.QuestGiver.CurrentSettlement.Prosperity += -50f;
                this.RelationshipChangeWithQuestGiver = -10;
                ChangeRelationAction.ApplyPlayerRelation(base.QuestGiver, this.RelationshipChangeWithQuestGiver, false, true);
            }


            // Saved vars/logs
            [SaveableField(1)]
            private readonly int NeededFood;

            [SaveableField(2)]
            private JournalLog PlayerAcceptedQuestLog;

            [SaveableField(3)]
            private JournalLog PlayerHasNeededFoodLog;
        }

        // Save data goes into this class
        public class StarvingTownNeedsFoodIssueTypeDefiner : SaveableTypeDefiner
        {
            public StarvingTownNeedsFoodIssueTypeDefiner() : base(585850)
            {
            }

            protected override void DefineClassTypes()
            {
                base.AddClassDefinition(typeof(StarvingTownNeedsFoodIssueBehavior.StarvingTownNeedsFoodIssue), 1);
                base.AddClassDefinition(typeof(StarvingTownNeedsFoodIssueBehavior.StarvingTownNeedsFoodQuest), 2);
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
