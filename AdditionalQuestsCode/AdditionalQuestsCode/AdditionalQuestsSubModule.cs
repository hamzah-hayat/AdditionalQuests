using AdditionalQuestsCode.Quests;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace AdditionalQuestsCode
{
    public class AdditionalQuestsSubModule : MBSubModuleBase
    {

        public override void OnCampaignStart(Game game, object starterObject)
        {
            CampaignGameStarter gameStarter = (CampaignGameStarter)starterObject;
            this.AddBehaviors(gameStarter);
        }

        private void AddBehaviors(CampaignGameStarter gameStarter)
        {
            gameStarter.AddBehavior(new HeadmanNeedsMilitiaWeaponsIssueBehavior());
            gameStarter.AddBehavior(new StarvingTownNeedsFoodIssueBehavior());
            gameStarter.AddBehavior(new VillageBanditArmyRaidIssueBehavior());
            //gameStarter.AddBehavior(new NobleWantsTrainingBattleIssueBehavior());
            //gameStarter.AddBehavior(new TownUprisingIssueBehavior());
        }
    }
}