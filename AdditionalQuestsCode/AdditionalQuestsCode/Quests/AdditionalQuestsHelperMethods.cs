using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace AdditionalQuestsCode.Quests
{
    static class AdditionalQuestsHelperMethods
    {
        // FindSuitableHideout finds a hideout that is near to the issueOwner, use the distanceFromHideout to choose a distance
        public static Settlement FindSuitableHideout(Hero issueOwner, float distanceFromHideout)
        {
            Settlement result = null;
            foreach (Settlement settlement in from t in Settlement.All where t.IsHideout() && t.Hideout.IsInfested select t)
            {
                float distancetoHideout = settlement.GatePosition.DistanceSquared(issueOwner.GetMapPoint().Position2D);
                if (distancetoHideout <= distanceFromHideout)
                {
                    result = settlement;
                    break;
                }
            }
            return result;
        }

        // FindSuitableHideout finds a hideout that is near to the issueOwner, default of 1225 distance
        public static Settlement FindSuitableHideout(Hero issueOwner)
        {
            Settlement result = null;
            foreach (Settlement settlement in from t in Settlement.All where t.IsHideout() && t.Hideout.IsInfested select t)
            {
                float distancetoHideout = settlement.GatePosition.DistanceSquared(issueOwner.GetMapPoint().Position2D);
                if (distancetoHideout <= 1225f)
                {
                    result = settlement;
                    break;
                }
            }
            return result;
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

        public static void RemoveWeaponsWithTypeFromPlayer(WeaponClass weaponType,int numWeaponsToDelete)
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


    }
}
