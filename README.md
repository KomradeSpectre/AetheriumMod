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
	- **Green Rarity** - You gain 10  (+5 per stack) armor while BLUE shields are active.
----
- **Shark Teeth** (Original idea by FancyFrenc)
	- **Green Rarity** - 25% of damage taken (+25% per stack, hyperbolically) is distributed to you over 5 seconds as bleed damage.
----
- **Blood Soaked Shield** (Original Idea by Kaboose, altered quite a fair bit)
	- **Green Rarity** - Killing an enemy restores 10% max shield (+10% per stack hyperbolically.)
	- **Upcoming** - The first of the stack of this item will grant a small portion of shield on pickup.
----
- **Alien Magnet** (Original Idea, if you call a vacuum original)
	- **Lunar Rarity** - Enemies hit by your attacks will be pulled towards you, starting at 6x force (+1 force multiplier, up to 10x total force. The effect is more noticeable on higher health enemies.)
----
- **Weighted Anklet** (Original Idea)
	- **Lunar Rarity** - Gain a 25% reduction to knockback from attacks (+25% per stack (up to 100%) linearly). Lose 10% move speed (+10% per stack (up to 40%) linearly).
	- **Upcoming** - I may up the cap for the movement speed loss to allow a bigger downside when using randomizers or challenges.
----
- **Unstable Design** (Original idea by Spooky Boogie, altered a bit to improve)
	- **Lunar Rarity** - "Every 30 seconds you are compelled to create a very 'FRIENDLY' Lunar Chimera, if one of your creations does not already exist. It has a 400% base damage boost (+100% per stack), a 10% base HP boost (+10% per stack), a 300% base attack speed boost, and finally a 24% base movement speed boost (+24% per stack). This monstrosity can level up from kills."
	- **Upcoming** - Make kills by the Lunar Chimera give half XP/Half Gold to the Unstable Design wielder. Make it configurable between individual per player or similar to halcyon seed.
----
- **Heart of the Void** (Original idea by Hyperion_21, altered a bit to improve)
	- **Lunar Rarity** - "On death, cause a void implosion with a radius of 15m (+7.5m per stack) that revives you if an enemy is killed by it BUT at 30% health (+5% per stack, max 99%) or lower, all kinds of healing are converted to damage.
	- **Known Issues** - With PlayableVoidcrab mod, all runs (including the current) after the Voidcrab has died will cause every Voidcrab explosion to occur instantly when triggered, including this. This is fixed by restarting.

## Changelog
[0.1.4]
+ Fixed issue related to Dio's Best Friend revival adding another Unstable Design Chimera. Oops.
+ Made Blood Soaked Shield work with Forgive Me Please.
+ Added "Heart of the Void".

[0.1.3]  
+ Fixed issue related to turrets being able to summon individual Lunar Chimeras from Unstable Design.  
+ Fixed issue where Lunar Chimera would not teleport back up when falling into deathzones.  
+ Fixed Shielding Core model to actually use transparency for the gem (as I intended it to).  
+ Added a skill the Lunar Chimera will use on its enemy if it is in the air.  

[0.1.2]  
+ Added Unstable Design  
+ Fixed styletag issue with Shark Teeth displaying the style tags ingame as text.  

[0.1.1]  
+ Added Weighted Anklet.  

[0.1.0]  
+ Added Shielding Core  
+ Added Blood Soaked Shield  
+ Added Shark Teeth  
+ Added Feathered Plume  
+ Added Alien Magnet  
---
## Special Thanks  
**ThinkInvis** - I learned a whole lot of things from their Classic Items mod, utilize their config/logger system, and utilize their TILER2 API in this project.  
**RyanP** - We reference his void implosion (from his Flood Warning warden elites) as basis for our void implosion, albeit modified a bit to work for our item.  
**Rob** - For varied questions relating to coding on ROR2. Also for helping solve some issues regarding ItemDisplay rendering.  
**OkIGotIt (referencing Rico)** - For providing a code snippet for ItemDisplays which let me learn how to have them show up in the first place.   
**BordListian** - Basically co-dev support in creating this and playtesting.  
**GrimTheWanderer** - Playtesting.  
**Various ROR2 Modding Core Developers and Modders** - Answering questions related to modding for ROR2 at random stages during development of version 0.1.0.  
**Lorc (http://lorcblog.blogspot.com/)** - For the base feather icon I did a small edit to for Feathered Plume's speed buff.  
