## What is Aetherium?
Aetherium is a mod for Risk of Rain 2 that adds in a few items of varying tiers.
Most of the item ideas listed were sourced from the ideas channel on the official discord,
and as such the ones that were I will list the person that came up with the idea next to them.

## The mod so far has the following items implemented:
----
- **Feathered Plume** (Original idea by Ajarl, altered a tiny bit)
	- **White Rarity** - Gain a temporary 7% speed boost upon taking damage that stacks 3 times for 5 seconds. (+3 stacks and +0.5 second duration per additional Feathered Plume.)
----
 - **Shielding Core** (Original idea by NepNep)
	- **Green Rarity** - You gain 10  (+5 per stack) armor while BLUE shields are active. The first stack of this item will grant 4% of your max health as shield on pickup.
----
- **Shark Teeth** (Original idea by FancyFrenc)
	- **Green Rarity** - 25% of damage taken (+25% per stack, hyperbolically) is distributed to you over 5 seconds as bleed damage.
----
- **Blood Soaked Shield** (Original Idea by Kaboose, altered quite a fair bit)
	- **Green Rarity** - Killing an enemy restores 10% max shield (+10% per stack hyperbolically.) The first of the stack of this item will grant 8% of your max health as shield on pickup.
----
- **Blaster Sword** (Original idea by Bord, altered a bit by me.)  
	- **Red Rarity** - At full health, most attacks will fire out a sword beam that has 200% of your damage (+50% per stack) when it explodes after having impaled an enemy for a short duration.  
	- **Known Issues?** - MUL-T's nailgun fires an absurd amount of swords. They aren't super powerful (as they're based on MUL-T's base damage), but they are a bit spammy on him.)
----
- **Inspiring Drone** (Original Idea by Kaboose "Aftermarket Spare Parts", altered a ton from that.)  
	- **Red Rarity** - When a bot is purchased, it gains a 50% boost to each of its stats based on yours (+50% per stack, linearly).  
	Some bots gain more ammo for their attacks based on the bonus to their attack speed, and have their ammo replenished twice as fast per additional Inspiring Drone.  
    Finally, if an inspired bot is too far away from you, it is teleported to you after a delay (40 seconds for Turrets, 30 seconds for Drones).  
	- **Big Credits To** - Chen of the Chen's Classic Items and Chen's Gradius mods for putting a ton of work in to improve the Inspiring Drone's code as of 0.3.2.
----
- **Accursed Potion** (Original idea by Shadokuro, slightly modified by me.)
	- **Lunar Rarity** - Every 30 seconds (reduced by 25% per stack) you are forced to drink a strange potion, sharing its effects with enemies in a  
	20m radius (+5m per stack) around you. Max: 8 buffs or debuffs can be applied at any time.  
----
- **Alien Magnet** (Original Idea, if you call a vacuum original)
	- **Lunar Rarity** - Enemies hit by your attacks will be pulled towards you, starting at 6x force (+1 force multiplier, up to 10x total force. The effect is more noticeable on higher health enemies.)
----
- **Weighted Anklet** (Original Idea)
	- **Lunar Rarity** - Gain a 25% reduction to knockback from attacks (+25% per stack (up to 100%) linearly). Lose 10% move speed (+10% per stack (up to 40%) linearly).
----
- **Unstable Design** (Original idea by Spooky Boogie, altered a bit to improve)
	- **Lunar Rarity** - "Every 30 seconds you are compelled to create a very 'FRIENDLY' Lunar Chimera, if one of your creations does not already exist. It has a 400% base damage boost (+100% per stack), a 10% base HP boost (+10% per stack), a 300% base attack speed boost, and finally a 24% base movement speed boost (+24% per stack). This monstrosity can level up from kills."
----
- **Heart of the Void** (Original idea by Hyperion_21, altered a bit to improve)
	- **Lunar Rarity** - "On death, cause a void implosion with a radius of 15m (+7.5m per stack) that revives you if an enemy is killed by it BUT at 30% health (+5% per stack, max 99%) or lower, all kinds of healing are converted to damage.
	- **Known Issues** - With PlayableVoidcrab mod, all runs (including the current) after the Voidcrab has died will cause every Voidcrab explosion to occur instantly when triggered, including this. This is fixed by restarting.
----
- **Jar of Reshaping** (Original idea)
	- **Equipment** - On activation, absorb projectiles in a 20m radius for 3 second(s).  
	Upon success, fire all of the projectiles out of the jar upon next activation.  
	The damage traits of each projectiles fired from the jar depends on the bullets you absorbed.  
	After all the projectiles have been fired from the jar, it will need to cool down.
  
----

## Upcoming
----
- Version 1.0.0 will include a new survivor, we're building up to that.  

- ItemStatsMod integration soon, possibly 0.4.3.  

- The Key of Solomon idea is being brainstormed, but will still likely focus on stealing skills from enemies. However, those skills will likely be custom made by me, who knows.  

- Weighted Anklet will be reworked soon to be more desirable. How I'll do that? I'll have to think on it.  

- Make kills by the Lunar Chimera give half XP/Half Gold to the Unstable Design wielder. Make its spawn behavior configurable between individual per player or similar to halcyon seed.  

- Will be adding more custom VFX/SFX to the Jar of Reshaping now that I'm more familiar with effect networking.  

----

## Changelog
----
[0.4.3]
+ AETHERIUM REWRITE. Lots of refactoring had to be done, but now my process of item creation should be the default R2API way to do it. This means others can probably learn from it easier now.
+ Nerfed Shark Teeth default # of ticks value from 10 to 5 (As a result, it has more damage per these ticks vs Before.) Suggestion by Breadguy.
+ Nerfed Alien Magnet default starting force 6->3, force per stack 1->2 (To make it worth picking up more than one.) Suggestion by Breadguy.
+ Fixed Alien Magnet's force calculations to now work properly on things without a character motor. (Thanks to Chen for this)
+ Assorted fixes for Inspiring Drone and Unstable Design. (Thanks to Chen for this)
+ Added Texture to Shark Teeth.
+ Added an animation to Alien Magnet.
+ Made Alien Magnet use the ItemFollower system.
+ Retouched Icons, still working on these as development goes on.
+ Made the non-impale Blaster Sword projectile speeeeeeeeeeeeeeeeeeeen.
- Removed TILER2 dependency, migrated to my own ItemBase now.

[0.4.2]
+ Added Witches Ring.  
+ Fixed DOT effects doing nothing on the Accursed Potion. Be wary of how many potions you have now.  
+ Fixed a weird bug involving loading due to TILER2.  

[0.4.1]
+ Quick hotfix by Chen. Bots that had stocks able to be modified by Inspiring Drone couldn't fire if their owner didn't have an Inspiring Drone, woops, fixed now.

[0.4.0]
+ BIG Inspiring Drone update. Code quality has improved greatly thanks to Chen. The drone by default is active now (config in a later update to make it able to be on purchase again if you want that), and a fair handful of bugfixes in my default logic by Chen to make this item fantastic.
+ Added Integration for Eviscerate (Mercenary's first R attack) into the Blaster Sword. Enjoy!
+ Added Integration for Rex's Root R move, it's a bit janky as it works at full health only and will not work on things already entangled if your health didn't meet the threshold prior to them being tangled up. Will make better soon.
+ Added Jar of Reshaping, my first equipment!
+ Migration to TILER2 3.0.4, everything should be stable, but if it's not, let me know.
+ Fixed Inspiring Drone being upside down on MUL-T, he was vibin' now he isn't.

[0.3.1]
+ Turrets teleporting into the ground with inspiring drone has been fixed by Chen (Chen's Classic Items/Chen's Gradius Mod). Thanks!
+ Fixed Blaster Sword's internal class being non-public. Woops!
+ Fixed mod description stating that we only have 9 items. We're up to 11 now.
+ Nothing else to see here yet folks!

[0.3.0]
+ Added Accursed Potion
+ Added Blaster Sword
+ The first pickup of Blood Soaked Shield and Shielding Core will now grant a small portion of shield thanks to an ILHook provided by Harb.
+ Tried to fix turrets being half in the ground, failed. Will try again soon.
+ Fixed an issue related to me forgetting basic PEMDAS with the Shielding Core's calculation. E.

[0.2.0]
+ Added Configs to all items currently in Aetherium. All major values can be configured to your liking now.
+ Buffed Shielding Core's base values to 15 armor for first pickup, and 10 for each additional pickup after that.
+ More icon art by WaltzingPhantom, and the other new ones retouched to look better ingame, this time Shielding Core and Shark Teeth. 

----

## Special Thanks  
**ThinkInvis** - I learned a whole lot of things from their Classic Items mod. Thanks to them for authorizing me to use TILER2's stathooks in my itembase, it was integral in getting it off the ground.  
**Chen** - Fixed turrets teleporting into the ground with Inspiring Drone. Provided me a snazzy build event to let me test things faster now. Put in a ton of work to highly improve the Inspiring Drone's code as well, can't thank them enough for it.  
**Harb** - For providing the ILHook from their HarbCrate mod, which allows Bloodsoaked Shield and Shielding Core to be able to provide some base shields.  
**RyanP** - We reference his void implosion (from his Flood Warning warden elites) as basis for our void implosion, albeit modified a bit to work for our item.  
**Rob** - For varied questions relating to coding on ROR2. Also for helping solve some issues regarding ItemDisplay rendering.  
**OkIGotIt (referencing Rico)** - For providing a code snippet for ItemDisplays which let me learn how to have them show up in the first place.  
**Rico** - For the above, and for creating a timer component for me to fix an annoying Voidheart issue that I couldn't solve.   
**Rolo** - For providing a projectile code example, which allowed us to use it as a basis to create Blaster Sword with heavy modifications.  
**BordListian** - Basically co-dev support in creating this and playtesting.  
**GrimTheWanderer** - Playtesting.  
**WaltzingPhantom** - For providing alternative icon art for Feathered Plume, Blood Soaked Shield, Shark Teeth, and Shielding Core!  
**Various ROR2 Modding Core Developers and Modders** - Answering questions related to modding for ROR2 at random stages during development of version 0.1.0.  
**753** - Providing a crystallization shader for me to use in the future.  
**Lorc (http://lorcblog.blogspot.com/)** - For the base feather icon I did a small edit to for Feathered Plume's speed buff.  
