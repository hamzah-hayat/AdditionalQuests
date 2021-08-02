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

        public override void OnGameLoaded(Game game, object initializerObject)
        {
            CampaignGameStarter gameStarter = (CampaignGameStarter)initializerObject;
            AddBehaviors(gameStarter);
        }

        private void AddBehaviors(CampaignGameStarter gameStarter)
        {
            // Custom Battle calls CampaignStart on submodules but gameStarter is null??
            if (gameStarter == null)
            {
                return;
            }

            // Not using MCM atm, so load all Issue Behaviours
            gameStarter.AddBehavior(new HeadmanNeedsMilitiaWeaponsIssueBehavior());
            gameStarter.AddBehavior(new StarvingTownNeedsFoodIssueBehavior());
            gameStarter.AddBehavior(new VillageBanditArmyRaidIssueBehavior());
            gameStarter.AddBehavior(new TownUprisingIssueBehavior());
        }
    }
}