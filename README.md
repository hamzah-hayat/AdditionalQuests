# README

## Mod Information
Nexus Mods Link: https://www.nexusmods.com/mountandblade2bannerlord/mods/3066?tab=description&BH=0

## Implemented Quests
### Town Quests:
    - {Settlement} Needs Food
        - Town needs food to combat starvation, give food to merchant to complete
          Accepts Grain/Fish/Meat, pays triple the current average price for each
          Mission is completed after 300 food given
          Adds 300 food to town upon quest completion (stops starvation), plus relationship bonus with all notables in town

    - Rebellion in {ISSUE_SETTLEMENT}!
        - In a low loyalty town (<50 ), the people are going to rise up!
          Fight a small battle against the garrison alongside the militia of the town, on success, force a rebellion in that town
          Currently a 10vs10 due to alley fight limitation, might up it later if I figure out how to increase the size of the battle

### Village Quests:
    - Bandit Army Raiding {Settlement}
        - Bandit Army is spawned at hideout based on player Clan tier, heading to nearby Village, defeat bandit army
          Bandit army consists of 50% bandits based on hideout culture (eg sea raiders, forest bandits etc), and 50% looters
          If bandits reach village they will begin raiding, stop them from finishing the raid
          Rewards 3000 gold as "bounty" from nobles plus relationship bonus with all notables in village

    - {Settlement} Needs Militia Weapons
        - Simple quest, return 20 one handed polearms to village to complete
          Increases number of militia in village by 20
          Quest only occurs in villages with less then 15 militia


## Non-Implemented Quests

### Town Quests:
    - {MerchantName} needs Raw Materials
        - Merchant needs Raw Materials for their owned business in the town
          Quest is to collect Raw Material based on their business that they own
          Rewards manufactured goods (if applicable) or gold
          Also adds manufactured goods to town to marketplace

### Village Quests:
    - Beasthunter demands payment!
        - Beasthunter has taken payment from village notable in first bon son as payment for saving life time ago
          Can convince beasthunter to give back child or fight him in duel
          Can also use army, but this causes fight if army is small (<50 troops)
          Or he runs away if army is big (>=50 troops), but less reward and minus honour and renown


### Noble Quests:
    - {NobleName} wants Training Battle
        - Noble wants to train his troops (has to many tier 1-2 troops), offers a sparring battle with player.
          Small battle, ~50 troops per side (change based on difficulty?)
          No fatalities during combat, only Knock-outs
          Bonus party experience and gold reward for player if they win the fight
          upon quest completion, levels up all tier 1-2 troops for player and noble

    - {NobleName} needs Escort for Peace Envoy
        - Noble wants diplomat to be escorted to hostile faction town to stop war
          Player has "avoid" battle effect, will not be attached by hostile faction
          Chance of aumbush in town/field?
          Bring back diplomat to noble, causes peace agreement between two factions

    - {NobleName} wants runaway serfs caught!
        - Mean noble (negative honour/generosity) wants protesting villagers to be caught
          Villagers spawn outside town of quest start, running towards another town
          When talked to, they will agree to return to starting town
          If player moves to far away, they will retry running away (Max once per villager party)
          Catch all Villagers for max reward, catch more then half for good Reward, Less then half for little reward, Catch none for no reward
          Have choice of letting Villagers go for village rep increase
          On quest end, change hearths of villages depending on choices

    - {NobleName} wants new {FactionName} clan established
        - Help establish a new Clan by providing troops, higher tier troops provide more quest points
          Points used to determine tier of resulting clan
          Earn renown and large relations increase with new clan and quest clan
          Also creates a new clan for faction
          Ruling clan only? Maybe give one fief for new clan?

    - {NobleName} needs Assassin!
        - Noble wants enemy to be assassianted
          Can either capture + behead noble yourself to complete quest or capture and bring back to noble for them to kill
          Sneak into town and fight to caputure them? maybe Keep Battle?



## Modding notes

The code is included in the folder AdditionalQuestsCode (including solution for visual studio)
You will need to redo the references based on where you installed Bannerlord



# Saving Issue + Quest data
When creating Issues, had to choose an unused save ID that didnt clash with existing ones:
Eg
```
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
```
