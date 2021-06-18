README

Config is located in the config folder

Quests (Also known as Issues)

Noble Quests:
    - {NobleName} Needs New Weapon
        - Noble needs a new weapon to replace current one, Takes nobles current primary weapon (Weapon0?)
          Give in weapon of same type over 3000 gold value (make this dynamic?)
    - {NobleName} wants training battle
        - Noble wants to train his troops (has to many tier 1-2 troops), offers a sparring battle with player, player chooses troops to send into battle, so does Noble
          Small battle, ~50 troops per side (change based on difficulty?)
          No fatalties during combat, only Knock-outs
          Bonus party experience and gold reward for player if they win the fight
          upon quest completion, levels up all tier 1-2 troops for player and noble

Merchant Quests:
    - {Settlement} Needs Food
        - Settlement needs food to combat starvation, give food to merchant to complete
          Accepts Grain/Fish/Meat, pays double the current average price for each
          Mission is completed after 300 food given, but can give extra for more money (Limit 500 or 1000?)
          Gives food to settlement upon quest completion (stops starvation)


Artisan:
    - Rebel Uprising
        - In a low loyalty town, the people are going to rise up!
          Fight a small battle against the garrinson alongside the militia of the town, on success, force a rebellion in that town
          Upon quest sucess, force rebellion in town, also increase Artisan power
          Make war between player and faction that controlled town? Not sure

Gang Leader Quests:

Village Quests:
    - Bandit Army Raiding {Settlement}
        - Bandit Army is spawned based on player Reknown level, heading to nearby Village, Save village from Bandit army to complete quest
          Bandit army consists of 40% bandits based on hideout culture (eg sea raiders, forest bandits etc), and 60% looters
          If bandits reach village quest is failed instantly (also village is raided, need to figure out how to do that)
    - {Settlement} Needs Militia Weapons
        - Simple quest, return X number of Militia Spears to village to complete
          Increases number of militia in village, also quest only happens in village with low militia count



Modding notes

When creating Quest, had to choose an unused save ID that didnt clash with existing ones:
Eg
<!--
public class HeadmanNeedsHardWoodIssueTypeDefiner : SaveableTypeDefiner
{
    // Ensure the number in the base() is unique
    public HeadmanNeedsHardWoodIssueTypeDefiner() : base(1000500)
    {
    }
    
    protected override void DefineClassTypes()
    {
        base.AddClassDefinition(typeof(HeadmanNeedsHardWoodIssueBehavior.HeadmanNeedsHardWoodIssue), 1);
        base.AddClassDefinition(typeof(HeadmanNeedsHardWoodIssueBehavior.HeadmanNeedsHardWoodIssueQuest), 2);
    }
} 
-->