# Arthur Morgan: Beastmancer

Mod for RDR2.


## About


### The dead rise...


Arthur has been imbued with the ability to summon *Familiars*, and bring fallen animals back from the dead. Once summoned, these creatures are under Arthur's command. These abilities do cost Arthur some of his deadeye.

Command them to follow you, move to a location, or even fight alongside you. Be careful though, as animals that have been resurrected have a short life span, and you don't want to be standing near them when they die again. Once they die, they will release their energy back to you.

You'll need all the help you can get, as Arthur has been cursed. Any humans he kills will rise from beyond the grave. Once an enemy has risen, they are tougher than ever and won't be happy with you. However, they are particularly vunerable to fire and water. After an enemy has been slain again, they'll be dead for good.


## Installation


### Requirements


- [Script Hook RDR2](http://dev-c.com/rdr2/scripthookrdr2/)
- [ScriptHookRDR2 .NET](https://www.nexusmods.com/reddeadredemption2/mods/70)


### Install


If you haven't already, create a directory ``scripts`` in the RDR2 installation folder. Copy ``Beastmancer.dll`` and ``Beastmancer.ini`` into the ``scripts`` folder.


## Instructions


### Gameplay


#### Summoning Familiars


Press and hold the button for ``Whistling``. After a few seconds, if you have enough deadeye each *Familiar* defined in the ini fill will be summoned.


#### Resurrecting an Animal


Aim (with a weapon out) at an animal on the ground, and press the ``guard`` button. If you have enough deadeye, the creature will be resurrected.


#### Issuing commands


To command an individual animal, aim at them (with a weapon out) and press ``guard`` to select that animal. Then aim at another location and press guard again. If you are aiming at an enemy human or animal when you press it, it will be a command to attack. If you are aiming at the ground, the animal will move to that location.

- Ordering an Undead Animal to attack will cause them to run to the target and explode. Ordering a Familiar to attack will cause them to attack the target normally.


If you have already summoned or resurrected an animal, whistle again to order them to follow you. Press the guard button (without aiming at anything) to issue the command to attack any enemies in the nearby area.


#### Undead enemies


If Arthur kills a human, they will rise from the dead. 

**NOTE**: The time it takes to rise from the dead is based on if they've been *collected* by the mod. The mod will collect all nearby humans at a set interval (defined by ``collect_peds`` in the ini). Until the human has been collected by the mod, they won't rise. Also, the human has to have been killed originally by Arthur, not one of your animal friends.

These undead enemies have a lot more health than normal. Headshots will do the trick, as will fire or luring them into water (even small puddles).


### INI file

*Familiars* are defined in the ``ini`` file. In order to change and rename them, follow the example provided. Create a section for each Familiar, and add each Familiar name to the ``allies`` value under global, separated by a comma.

```
[global]
allies=Artemis,Teddy

...

[Artemis]
name=Artemis
model=A_C_Cougar_01
speed_multiplier=10

[Teddy]
name=Teddy
model=A_C_BearBlack_01
speed_multiplier=8
```

If you want to change the animal for the *Famliar*, change the ``model`` value from with something from [here](https://github.com/Saltyq/ScriptHookRDR2DotNet/blob/d296980f1a95cb871822ade6aec4298c0d41b272/source/scripting_v3/RDR2/Entities/Peds/PedHash.cs).


The other options in the ini file should hopefully be self-explanatory. I'd recommend **NOT** changing the value for ``collect_peds``. This determines the interval for the mod to collect all of the surrounding animals and humans (which is used by the mod to track the undead enemies). It seems to crash the game if it's too short.


## Gameplay Videos


- [Summoning Famliars](https://streamable.com/6w8wn)
- [Resurrecting an animal](https://streamable.com/1kxmd)
- [Undead animal attack](https://streamable.com/xpl85)
- [Undead enemy](https://streamable.com/lrzlx)
- [Famliar Attacking](https://streamable.com/gsgds)
