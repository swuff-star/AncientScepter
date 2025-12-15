
## Changelog
`1.1.37`
- 🛠️ Temporary Ancient Scepters should now properly reroll into a new temporary legendary item when more than one is acquired.
- ❗ Known issue: If already carrying a Temporary Ancient Scepter, picking up a permanent Ancient Scepter will reroll neither on the first stack of each. It's intended that the temporary would reroll while the permanent is kept - sorry to anyone who misses out on a temporary legendary for the time being! Note that all other scenarios involving picking up/mixing permanent/temporary Scepters should be A-OK.

`1.1.36`
- ➕ New default stacking style (RoRR): Extra stacks will give -30% CDR to the affected skill. Old behaviors are still available through config!
- ➕ New skills: Seeker, CHEF, Drifter.
- 🛠️ Ancient Scepter is now capable of being a Temporary Item, like Retruns.

`1.1.35`
- 🩹 fixed for AC

`1.1.34`
- 🩹 Recompiled mod for SOTS

`1.1.33`
- 🩹 Hotfixed edge case when replacing scepter skills defined through indexes using the skilldef based overload.Not the best possible fix,but incoming rewrite will invalidate the issue.

`1.1.32`
- 🛠️Fixed any inventory change restocking scepter skills,this time for every character. (Don't forget to change up characters and loadouts between test runs,everyone!)

`1.1.31`
- 🛠️Fixed any inventory change restocking scepter skills.
- Batched together transformation notifications to the same item to stop notification flood when given multiple scepters at once.
- 🛠️Fixed Heretic's Nevermore not having description text.

`1.1.30`
- 🛠️Improved handling of SkillOverrides,fixing multiple cases of unexpected behavior.
- Polished Void Fiend's scepter ability.
- Moved mod dependencies over to singular r2api modules instead of the legacy one.

`1.1.23`
- 🛠️Fixed inverted check on Artificer Flamethrower

`1.1.22`
- 🛠️Fixed Handling of cases where upgraded skill states are reused by other bodies.
  - Specifically,fixes Incinerator/Flamethrower Drones erroring out on attack.
- 🛠️Fixed non-existent/old bodies causing issues when inside Rex's Chaotic Growth

`1.1.21`
- Better handling of broken defs from mods + logging
- 🛠️Added nullcheck to removeclassicitemsscepter method to  try to fix out of range exception

`1.1.2`
- Updated to latest libraries
- ➕Added IDR to Nemesis Commando¹
- 🛠️Make registration of scepter skills slot agnostic²
- Adds an overload for RegisterScepterSkill that takes (replacementdef,bodyname,targetdef)²
- 🛠️BetterUI Compat Fixed²
	- Now loads properly if BetterUI is not installed
	- Fixed BetterUI Mobile Turret compat
- 🛠️Fixed scepter affecting skills that inherit from their targeted skillstate²
	- moved the responsibility of executing scepter skills from possession of the item to possession of the skilldef.
- 😎Thanks to Zenithrium¹ and RandomlyAwesome² for changes
- ➕Added config setting to show transformation notification on reroll
- 🛠️Fixed Tinker's Satchel Mostly Tame Mimic from duplicating the item, causing a reroll if reroll was enabled.
- 🛠️"COMMAND: Reap": Increased number of buff fruits from 1-8 to 2-10
- 🛠️Updated readme to use tables for readability
- ➕Added new icons for:
	- Captain: PHN-8300 ‘Lilith’ Strike
	- Commando: Death Blossom
	- Heretic: Perish Song
	- Mercenary: Massacre
	- Railgunner: Hypercharge
	- Railgunner: Permafrosted Cryocharge
	- REX: Chaotic Growth

`1.1.1`
- 🛠️fixed BetterUI compat having issues when BetterUI isn't installed

`1.1.0`
- 🛠️Updated for SOTV
	- Flamethrower: Copied updated IL code from ClassicItems
- ➕New Config:
	- "Remove Classic Items' Ancient Scepter From Droplist": Enabled by default.
- ➕Added Item Displays for Heretic, Railgunner, and Void FIend
- ➕New Scepters (WIP)
	- Railgunner:
		- Supercharge -> Hypercharge: Permanently removes 10 armor, +0.5 proc coeff
		- Cryocharge -> Permafrosted Cryocharge: Slows on hit, ice explosion
	- Void Fiend
		- Crush -> Reclaimed Crush: Heals nearby allies within 25m
		- Corrupted Crush -> Reclaimed Crush (Corrupted): Damages nearby enemies within 25m. +2 stocks
- BetterUI Compat
- 🛠️Captain: PHN-8300 'Lilith' Strike
	- Reduced blight duration (30s -> 20s) and stacks (10 -> 5)

`1.0.9`
- ➕Added new config setting: UnusedMode
	-   Keep: Non-sceptered characters keep scepter when picked up
	-   Reroll: Characters reroll according to the Reroll on pickup config
	-   Metamorphosis: characters without scepter upgrades will not reroll if metamorphosis is active  
		- Allowing you to play metamorphosis runs with sceptered characters
- ➕Added alternate model
- 🛠️Fixed the weird itemdef.modelpickupprefab warning thing
- ➕Added item displays to [Enforcer and Nemforcer](https://thunderstore.io/package/EnforcerGang/Enforcer/)

`1.0.8`
- 🛠️Fixed Captain's Diablo Scepter Skill still inflicting Blight on allies when Captain Nuke Friendly Fire is disabled.
- 🛠️Recategorized the config. Requires refreshing your config.

`1.0.7`
- 🛠️Temporarily replaced the texture used for the glow on the Ancient Scepter to the purple fire texture used by the Ancient Wisp, while waiting on the next material fix.

`1.0.6`
- 🛠️Config setting `StridesTakesPrecedence` changed to `HeresyTakesPrecedence` in accordance of RoR2 possessing more than one Heresy item
- ➕Added configuration setting to blacklist AncientScepter from turrets, incorrectly causing the turrets to receive a reroll due to having no skill coded. (Disabled by default to maintain original behavior)
- 🛠️Fixed Unity Error complaining about trying to register it to network despite missing a networkidentity
- ➕Added configuration setting to Captain's Nuke skill whether or not it blights allies (Disabled by default)
- 🛠️(For Devs) Adding a ScepterReplacer to a character that already has an existing ScepterReplacer for that slot and variant will replace it, instead of throwing an error.
	- This change allows developers that modify or outright change existing skills to have their own Scepter skill for existing characters.
- 🛠️(For Devs) All icons for Scepter Skills in this mod are now publicly accessible. See the static class `Assets.SpriteAssets`.

`1.0.5`
 - 🛠️Restored the property accessor on the ItemDef of the ItemBase, which was preventing other mods from accessing the item properly.

`1.0.4`
- ❌Blacklisted from being copied by TinkersSatchel's "Mostly-Tame Mimic" to prevent accidental rerolls.
- 🛠️Merged config option to reroll duplicates into: Disabled, Random, and Scrap.
	- Scrap option allows extra to reroll into red scrap.
- 🛠️Increased radius of Commando's "Death Blossom".
	- Holding down primary fire while using the skill switches back to the vanilla aim radius.
- 🛠️Added option to Mercernary's "Massacre", allowing the user to exit early to prevent softlocks; especially with with SkillsPlusPlus.
	- Hold down your special ability to exit early.
- ➕Added missing scepter skills for Captain's "Diablo Strike", Bandit's "Lights Out" and "Desperado", and Heretic's "Nevermore".
- ➕Added new skills for Aurelionite's Laser, and Alloy Vulture's Windblade. 
- ➕Updated Assets and Lore
- ➕Updated Readme
- 🛠️Updated internal names to be more independent of ClassicItems

`1.0.3`
- 🛠️Made ItemDef public so other mods can access it for their display rules

`1.0.2`
- 🛠️Fixed for latest RoR2 version

`1.0.1`
- 🛠️Fixed REX's upgrade not applying debuffs
- 🛠️Fixed Captain's upgrade not having a skill icon
- ➕Added item display for Bandit

`1.0.0`
- Initial release
