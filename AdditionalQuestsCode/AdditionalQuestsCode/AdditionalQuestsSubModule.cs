using HarmonyLib;
using AdditionalQuestsCode.Quests;
using AdditionalQuestsCode.Utils;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace AdditionalQuestsCode
{
    public class AdditionalQuestsSubModule : MBSubModuleBase
    {

        private Harmony? harmony;

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

            if (Statics.MCMModuleLoaded)
            {
                if (Statics._settings.HeadsmanNeedsMilitiaWeaponsActive)
                {
                    gameStarter.AddBehavior(new HeadmanNeedsMilitiaWeaponsIssueBehavior());
                    Logging.MessageDebug("Headsman Quest Enabled");
                }

                if (Statics._settings.StarvingTownNeedsFoodActive)
                {
                    gameStarter.AddBehavior(new StarvingTownNeedsFoodIssueBehavior());
                    Logging.MessageDebug("Starving Town Quest Enabled");
                }

                if (Statics._settings.VillageBanditArmyActive)
                {
                    gameStarter.AddBehavior(new VillageBanditArmyRaidIssueBehavior());
                    Logging.MessageDebug("Village Bandit Army Quest Enabled");
                }

                if (Statics._settings.TownUprisingIssueActive)
                {
                    gameStarter.AddBehavior(new TownUprisingIssueBehavior());
                    Logging.MessageDebug("Town Uprising Quest Enabled");
                }
            } else {
                Logging.MessageDebug("MCM Disabled - Loading All Quests");
                gameStarter.AddBehavior(new HeadmanNeedsMilitiaWeaponsIssueBehavior());
                gameStarter.AddBehavior(new StarvingTownNeedsFoodIssueBehavior());
                gameStarter.AddBehavior(new VillageBanditArmyRaidIssueBehavior());
                gameStarter.AddBehavior(new TownUprisingIssueBehavior());
            }
           

            //Disabled
            //gameStarter.AddBehavior(new NobleWantsTrainingBattleIssueBehavior());
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            try
            {
                ConfigLoader.LoadConfig();
                Logging.SendMessage("Loaded v" + Statics.ModVersion);
            }
            catch (Exception ex)
            {
                Logging.SendMessage("Failed to Load Mod");
            }
        }
    }
}