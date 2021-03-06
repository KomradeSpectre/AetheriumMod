## What is Aetherium?
Aetherium is a mod for Risk of Rain 2 that adds in a few items of varying tiers.
Most of the item ideas listed were sourced from the ideas channel on the official discord,
and as such the ones that were I will list the person that came up with the idea next to them.

Discord: https://www.discord.gg/WNap5qcPf6

----
## Items (so far):
----

| Icon | Item | Description | Rarity | Original Idea by |
|:-|-|------|-|-|
|![](https://i.imgur.com/0utgQzB.png) | **Feathered Plume** | Gain a temporary `7%` speed boost upon taking damage that stacks `3` times for `5 seconds`. (`+3` stacks and `+0.5` second duration per additional Feathered Plume.) | White | Ajarl |
|![](https://i.imgur.com/Q6gtbBS.png) | **Blood Soaked Shield** |  Killing an enemy restores `10%` max shield (`+10%` per stack hyperbolically.) The first of the stack of this item will grant `8%` of your max health as shield on pickup. | Green | Kaboose |
|![](https://i.imgur.com/ip04Lin.png) | **Engineers Toolbelt** | You have a `20%` chance (`+20%` hyperbolically up to a maximum of `100%` chance) to duplicate drones and turrets on purchase. Additionally, you have a `10%` chance (`+10%` hyperbolically up to a maximum of `100%` chance) to revive drones and turrets when they die. | Green | Insane (originally) Me (Modified) |
|![](https://i.imgur.com/b5QsaP1.png) | **Shark Teeth** | `25%` of damage taken (`+25%` per stack hyperbolically) is distributed to you over `5` seconds as `bleed damage.` | Green | FancyFrenc |
|![](https://i.imgur.com/dIRNo04.png) | **Shielding Core** | You gain `10`  (`+5` per stack) armor while `BLUE shields` are active. The first stack of this item will grant `4%` of your max health as shield on pickup. |  Green | NepNep |
|![](https://i.imgur.com/5aCDrfv.png)|  **Blaster Sword** |  At `full health`, most attacks will `fire out a sword beam` that has `200%` of your damage (`+50%` per stack) when it explodes after having impaled an enemy for a short duration. | Red | Bord |
|![](https://i.imgur.com/6zG0MC9.png)| **Inspiring Drone** | Any bots you own gain a `50%` boost to each of its stats based on yours (`+50%` per stack). Some bots `gain more ammo` for their attacks based on the bonus to their attack speed, and have their `ammo replenished twice as fast` per additional Inspiring Drone. Finally, if an inspired bot is too far away from you, it is `teleported to you` after a delay (`40` seconds for Turrets, `30` seconds for Drones). | Red | Kaboose (original) Me (modification) |
|![](https://i.imgur.com/Z6gbypJ.png)| **Witches Ring** | Hits that deal `500%` damage or more will trigger `On Kill` effects. Upon success, the target hit is `granted immunity to this effect` for `5` seconds (`-10%` duration per stack, hyperbolically.)| Red | Me |
|![](https://i.imgur.com/tmXCtML.png)| **Accursed Potion** | Every `30` seconds (reduced by `25%` per stack) you are `forced` to drink a strange potion, sharing its effects with enemies in a `20m` radius (`+5m` per stack) around you. Max: `8` buffs or debuffs can be applied at any time. |  Lunar |  Shadokuro |
|![](https://i.imgur.com/QQY1j4w.png)| **Alien Magnet** | Enemies hit by your attacks will be `pulled towards you`, starting at `6x` force (`+1` force multiplier, up to `10x` total force. The effect is more noticeable on higher health enemies. | Lunar | Me |
|![](https://i.imgur.com/Pex4nKP.png)|  **Heart of the Void** | `On death`, cause a void implosion with a radius of `15m` (+`7.5m` per stack) that `revives you` if an enemy is killed by it **BUT** at `30%` health (`+5%` per stack, max `99%`) or lower, all kinds of healing are `converted to damage`. | Lunar | Hyperion_21 |
|![](https://i.imgur.com/MxqLea7.png)| **Unstable Design** | Every `30` seconds you are compelled to create a very '**FRIENDLY**' Lunar Chimera, if one of your creations does not already exist. It has a `400%` base damage boost (`+100%` per stack), a `10%` base HP boost (`+10%` per stack), a `300%` base attack speed boost, and finally a `24%` base movement speed boost (`+24%` per stack). This monstrosity can level up from kills. | Lunar | SpookyBoogie |
|![](https://i.imgur.com/o78EHhU.png)| **Weighted Anklet** | A collection of weights will slow your attack speed by `10%` (to a minimum of `10%` total) and your movement speed by `10%` (to a minimum of `10%` total). If you find a way to remove them, you are granted `0.25` attack speed `flat scaling`, `1` movement speed `flat scaling`, and `5%` damage per removal. Additionally, removing an anklet grants you a stack of `Limiter Release Dodge`. `Dodge` will allow you to dodge one `overlap`, or `blast` attack before depleting. Once all stacks of dodge are depleted, they will need to recharge (`10` seconds for the first stack, `5` seconds per each additional stack) before fully replenishing. | Lunar | Me |

----
## Equipment (so far):
----

| Icon | Equipment | Description | Type | Original Idea by |
|:-|-|------|-|-|
|![](https://i.imgur.com/RUky7N5.png)| **Jar of Reshaping** | On activation, `absorb projectiles` in a `20m` radius for `3` second(s). Upon success, `fire all of the projectiles` out of the jar upon next activation. The damage traits of each projectiles fired from the jar `depends on the bullets you absorbed`. After all the projectiles have been fired from the jar, it will need to cool down. | Normal | Me |

----
## Upcoming
----
- Version 0.6.0 or 0.7.0 will include a new survivor, we're building up to that.  

- Make Unstable Design's spawn behavior configurable between individual per player or similar to halcyon seed.  

- Will be adding more custom VFX/SFX to the Jar of Reshaping now that I'm more familiar with effect networking.  

- Implement Active Dodging for Weighted Anklet.

----

## Changelog
----
[0.5.5]
+ Added ItemStatsMod support.
+ Added a homing swords configuration option for Blaster Sword, disabled by default.
+ Added a config option to allow Blaster Sword to be used with any level of Barrier, disabled by default.
+ Added a config option to stop Shark Teeth from being able to kill you, disabled by default.
+ Added Engineers Toolbelt, a new Green Item. With it, you have a small chance to duplicate drones or turrets on purchase, and a small chance for them to revive themselves on death.
+ Added a Targeting Indicator config option to Unstable Design. If there is an unstable design out, it places one of these above whatever it is targeting's head. An arrow on it points to the one targeting you.
+ Added an Aggression Pulling config option to Unstable Design. If it is targeting any AI, it will draw their aggression until it loses sight of them, changes target, or either party dies.
+ Fixed a Heart of the Void bug where Dios would activate during heart of the void, and lead to a game over regardless of living status. Now, dios get used first before Heart of the Void activates.
+ Fixed a Heart of the Void bug where the forced camera sometimes would not delete and kept spinning.
+ Added Artifact of the Tyrant, when enabled, any time Mithrix spawns, they will have a random elite affix (how many they have is configurable).
+ Added Artifact of Leonids, when enabled, after a duration a meteor shower will occur.
+ Added Artifact of Regression, when enabled, any enemy monster that is in an evolved form will split into a group of its lesser form on death. E.g. Beetle Queen into 2 Beetle Guards into 5 Beetles each Guard.
- Removed all item display default rules. Any more reports of big item displays will be ignored.

[0.5.4]
+ Jar of Reshaping now fires the original projectiles it absorbed out. If it cannot find the original projectile it absorbed in the catalog, it fires my crystal jar bullet instead. This is config toggleable, but is now the default functionality.
+ The voidheart camera spin bug has been now truly defeated. It should never happen again.

[0.5.3]
+ Fixed Heart of the Void cinematic spinning endlessly
+ Heart of the Void will now no longer leave a used dio in your inventory. It gets removed when the instability debuff is added.
+ Fixed a longstanding bug with Heart of the Void where having a huge health change happen would kill you instantly.
+ Added compatibility for Tinkers Satchel, whenever it updates. The mimic should no longer grant free Limiter Releases.
+ Witches Ring Global Cooldown option now actually works. Before, I never actually checked for the buff on the person's body while its enabled.
+ Unstable Design now has a config option to replace its primary if Artifact of the King is installed. It's false by default.
- Remove Weighted Anklet and Limiter Releases from Deployables until they fix turrets just ignoring gravity. I can't be bothered to patch this every single time.

[0.5.2]
+ Added more visual touches to items in the mod.
+ Added a Cinematic Heart of the Void revival.
+ Added ItemDisplayRules for Bandit.
+ Updated to make everything work for post-anniversary update changes.
+ Added Knockback Resistance back to the Weighted Anklet, this goes away when you remove them and get a limiter release instead.
+ Other changes that I forgot to document.

[0.5.0]
+ Massive visuals update for the mod. Can't list every change here, but I have converted a great deal of it to the Hopoo Games shader and redesigned the look of some items.
+ Added some nice particle effects for a few items.
+ Weighted Anklet Redux: The previously worthless Lunar is probably now one of my strongest items.
+ Added an alternate model for the Blaster Sword. I based it loosely off of Kusanagi-no-Tsurugi.
+ Added a config to turn Inspiring Drone into a Green instead of a Red. Icon and all.
+ Made Unstable Design give half of gold and xp from its kills to its user. It gains the full value of both, but directly this time instead of pumping it into the Neutral team.
+ Fixed Blaster Sword dealing 2x the damage it should.
+ Buffed Shielding Core's default first armor stack to be 15, with additional stacks giving 10.
+ Buffed Shielding Core's base shield grant to be 8 percent, as opposed to 4 percent.
- Removed the DOT from Blaster Sword's projectile. It wasn't supposed to be there in the first place.
- Removed the knockback reduction from Weighted Anklet for the redesign.

[0.4.7]
+ GRRRR, thunderstore markdown.

[0.4.6]
+ Added configs to enable/disable items and equipment.
+ Added configs to allow blacklisting items so AI may never use/acquire them.
+ Retouched icons for buffs.
+ Made this readme more legible.
+ Made preparations for Pheromone Sac (0.5.0), will be releasing with another item.
- Removed obscure feature.

----

## Special Thanks  
**ThinkInvis** - I learned a whole lot of things from their Classic Items mod. Thanks to them for authorizing me to use TILER2's stathooks in my itembase, it was integral in getting it off the ground.  
**Chen** - Fixed turrets teleporting into the ground with Inspiring Drone. Provided me a snazzy build event to let me test things faster now. Put in a ton of work to highly improve the Inspiring Drone's code as well, can't thank them enough for it.  
**KingEnderBrine** - Helping out with some questions regarding Item Displays, and providing a useful helper mod to speed up my IDRS creation.  
**Harb** - For providing the ILHook from their HarbCrate mod, which allows Bloodsoaked Shield and Shielding Core to be able to provide some base shields, and also providing a reflection autodiscovery feature for my items and equipment.  
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
**Lorc (Https://lorcblog.blogspot.com/)** - For the base feather icon I did a small edit to for Feathered Plume's speed buff.  
**FilterForge (Https://www.filterforge.com/)** - For providing a good deal of the base for my normals. Awesome plugin library, I recommend it.