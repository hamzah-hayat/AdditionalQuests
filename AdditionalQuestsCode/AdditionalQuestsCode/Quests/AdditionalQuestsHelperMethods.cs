using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

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

    }
}
