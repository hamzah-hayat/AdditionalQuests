using AdditionalQuestsCode.Quests;
using AdditionalQuestsCode.Utils;
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

            if (MCMAQSettings.Instance.BanditArmy_Enabled) {
                gameStarter.AddBehavior(new BanditArmyIssueBehavior());
                Logging.MessageDebug("Adding Bandit Army Quest");
            }

            if (MCMAQSettings.Instance.MilitiaSpears_Enabled)
            {
                gameStarter.AddBehavior(new MilitiaSpearsIssueBehavior());
                Logging.MessageDebug("Adding Militia Spears Quest");
            }

            if (MCMAQSettings.Instance.StarvingTown_Enabled)
            {
                gameStarter.AddBehavior(new StarvingTownIssueBehavior());
                Logging.MessageDebug("Adding Starving Town Quest");
            }

            if (MCMAQSettings.Instance.TownUprising_Enabled)
            {
                gameStarter.AddBehavior(new TownUprisingIssueBehavior());
                Logging.MessageDebug("Adding Town Uprising Quest");
            }

            Logging.MessageDebug("AdditionalQuests loaded sucessfully!");
        }
    }
}