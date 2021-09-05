using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace AdditionalQuestsCode.Quests
{
    static class AdditionalQuestsHelperMethods
    {
        // FindSuitableHideout finds a hideout that is near to the issueOwner, use the distanceFromHideout to choose a distance
        public static Settlement? FindSuitableHideout(Hero issueOwner, float distanceFromHideout)
        {
            foreach (Settlement settlement in from t in Settlement.All where t.IsHideout() && t.Hideout.IsInfested select t)
            {
                float distancetoHideout = settlement.GatePosition.DistanceSquared(issueOwner.GetMapPoint().Position2D);
                if (distancetoHideout <= distanceFromHideout)
                {
                    return settlement;
                }
            }
            return null;
        }

        // FindSuitableAssassinationTarget returns a random "target" hero that is considered to be an "Enemy" of the questGiver
        // If it returns null, this Quest Giver has no Enemies!
        // The minRelationForTarget is used to compare, for example a -20 value would mean that the quest giver and target have -20 relation with each other
        // Also we need to make sure the target is not in same faction as Quest giver, and that the two factions are at war with each other
        public static Hero? FindSuitableAssassinationTarget(Hero questGiver, int minRelationForTarget)
        {
            List<Hero> targets = new List<Hero>();

            foreach (Hero target in Hero.AllAliveHeroes)
            {
                if (questGiver.GetRelation(target) <= minRelationForTarget && questGiver.MapFaction != target.MapFaction && questGiver.MapFaction.IsAtWarWith(target.MapFaction))
                {
                    targets.Add(target);
                }
            }

            if (targets.Count == 0)
            {
                return null;
            }
            else
            {
                return targets.GetRandomElement();
            }
        }

        // FindSuitableHideout finds a hideout that is near to the issueOwner, default of 1225 distance
        public static Settlement? FindSuitableHideout(Hero issueOwner)
        {
            foreach (Settlement settlement in from t in Settlement.All where t.IsHideout() && t.Hideout.IsInfested select t)
            {
                float distancetoHideout = settlement.GatePosition.DistanceSquared(issueOwner.GetMapPoint().Position2D);
                if (distancetoHideout <= 1225f)
                {
                    return settlement;
                }
            }
            return null;
        }

        public static int GetRequiredWeaponWithTypeCountOnPlayer(WeaponClass weaponType)
        {
            int num = 0;
            foreach (ItemRosterElement itemRosterElement in PartyBase.MainParty.ItemRoster)
            {
                if (itemRosterElement.EquipmentElement.Item != null && itemRosterElement.EquipmentElement.Item.WeaponComponent != null && itemRosterElement.EquipmentElement.Item.WeaponComponent.PrimaryWeapon.WeaponClass == weaponType)
                {
                    num += itemRosterElement.Amount;
                }
            }
            return num;
        }

        public static void RemoveWeaponsWithTypeFromPlayer(WeaponClass weaponType, int numWeaponsToDelete)
        {
            int num = numWeaponsToDelete;
            for (int i = PartyBase.MainParty.ItemRoster.Count - 1; i >= 0; i--)
            {
                ItemRosterElement itemRosterElement = PartyBase.MainParty.ItemRoster[i];
                if (itemRosterElement.EquipmentElement.Item != null && itemRosterElement.EquipmentElement.Item.WeaponComponent != null && itemRosterElement.EquipmentElement.Item.WeaponComponent.PrimaryWeapon.WeaponClass == weaponType && itemRosterElement.Amount > 0)
                {
                    if (num < itemRosterElement.Amount)
                    {
                        PartyBase.MainParty.ItemRoster.AddToCounts(itemRosterElement.EquipmentElement, -num);
                        return;
                    }
                    PartyBase.MainParty.ItemRoster.AddToCounts(itemRosterElement.EquipmentElement, -itemRosterElement.Amount);
                    num -= itemRosterElement.Amount;
                }
                if (num == 0)
                {
                    break;
                }
            }
        }

        public static int CalculateAveragePriceForWeaponClass(WeaponClass weaponClass)
        {
            int num = 0;
            int num2 = 0;
            foreach (Settlement settlement in Settlement.All)
            {
                if (settlement.IsTown)
                {
                    foreach (ItemRosterElement itemRosterElement in settlement.ItemRoster)
                    {
                        WeaponComponent weaponComponent = itemRosterElement.EquipmentElement.Item.WeaponComponent;
                        if (weaponComponent != null && weaponComponent.PrimaryWeapon.WeaponClass == weaponClass)
                        {
                            num2 += itemRosterElement.Amount;
                            num += itemRosterElement.EquipmentElement.ItemValue;
                        }
                    }
                }
            }
            return num / ((num2 == 0) ? 1 : num2);
        }

        // SellQuestItemOfCategoryForPlayer tries to sell an item (using the item category) for the Average price * sellMultipler
        // If there is not enough to sell, it will return an integer indicating how much is left to sell.
        // This will remove items from main players inventory
        public static int SellQuestItemForPlayer(ItemObject item, int sellMultiplier, int numToSell)
        {
            int averagePrice = GetAveragePriceOfItem(item);
            int numSold = 0;

            for (int i = PartyBase.MainParty.ItemRoster.Count - 1; i >= 0; i--)
            {
                ItemRosterElement itemRosterElement = PartyBase.MainParty.ItemRoster[i];
                if (itemRosterElement.EquipmentElement.Item != null && itemRosterElement.EquipmentElement.Item == item)
                {
                    if (itemRosterElement.Amount < numToSell)
                    {
                        // We can only sell the amount here
                        GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, averagePrice * itemRosterElement.Amount * sellMultiplier, false);
                        PartyBase.MainParty.ItemRoster.AddToCounts(itemRosterElement.EquipmentElement, -itemRosterElement.Amount);
                        numSold += itemRosterElement.Amount;
                    }
                    else
                    {
                        // We can sell max amount
                        GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, averagePrice * numToSell * sellMultiplier, false);
                        PartyBase.MainParty.ItemRoster.AddToCounts(itemRosterElement.EquipmentElement, -numToSell);
                        numSold += numToSell;
                    }
                }
            }

            return numSold;
        }

        public static int GetAveragePriceOfItem(ItemObject item)
        {
            int fiefNum = 0;
            int itemPrice = 0;
            foreach (Settlement settlement in Settlement.All)
            {
                if (settlement.IsTown)
                {
                    itemPrice += settlement.Town.GetItemPrice(item);
                    fiefNum++;
                }
                else if (settlement.IsVillage)
                {
                    itemPrice += settlement.Village.GetItemPrice(item);
                    fiefNum++;
                }
            }
            return itemPrice / fiefNum;
        }
    }
}
