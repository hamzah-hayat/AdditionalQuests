README

Config is located in the config folder

Quests (Also known as Issues)

Noble Quests:
    - Bandit Army Raiding {Settlement}
        - Bandit Army is spawned based on player Reknown level, heading to nearby Village, Save village from Bandit army to complete quest
          If Village is raided quest is failed
    - {NobleName} Needs New Weapon
        - Noble needs a new weapon to replace current one, Randomly chosen between One Handed Sword/Axe, Two Handed Sword/Axe and Spear
          Must be of sufficent value to be given in
    - {NobleName} Training Battle
        - Noble wants to train his troops, offers a sparring battle with player, player chooses troops to send into battle, so does Noble
          No fatalties during combat, only Knock-outs
          Bonus Experience and gold reward for player if they win the fight

Merchant Quests:

Artisan:
    - Rebel Uprising
        - In a low loyalty town, the people are going to rise up!
          Fight a small battle against the garrinson alongside the militia of the town, on success, force a rebellion in that town

Gang Leader Quests:

Village Quests:
    - {Settlement} Needs Militia Weapons
        - Simple quest, return X number of Militia Spears to village to complete
    - {Settlement} Needs Hardwood
        - Super simple quest, return Hardwood to village to help repair barn



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