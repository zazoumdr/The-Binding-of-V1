Game Design Document

Let's do a quick recap of all the basic Gamedesign in UltraKill, like every game, it's gameplay and gamedesign is based on differents axiomes and sub-axiomes, which need, and should be understood, reused, and meshed to create something new for an ambitious mod Our work here will be on items/perks, focusing( without being limited to) :
Lexicon

| Term | Definition |
|---|---|
| **A/R** | Adding/Removing, represents the capacity of certain items/perks to add or remove a value in a variable, e.g A/R ground speed, represents the capacity for an item/perks to have an influence on the airspeed of the caracter following a jump, a short run, or a slide, depending on the situation or the intent of the object. |
| **INF** | Influence, Influence on other variables, some of the actions of the caracter or/and it's environnement will be able to aply other related or unrelated effects to the environnement, ennemies, physics etc... It's main purpose is to facilitate the creation of brand new and original mechanics. I will give examples in the third part of this doc, see L |
| **WU** | Weapon Upgrades |

Ex :

| Item | Mechanic | Notes |
|---|---|---|
| MultiJump | A/G Number of jumps | INF |

INF being for example, a chance to make mines appear at random locations, at a minimum fixed distance from the Player, inside the current room.

In a sense, it's a way to implemeting ideas using the "if, then" formula, e.g : If speed is/is more than/is less than/, then give a Damage multiplier, or, "the more something, the more something" or "the more you have of something, the more something happens"

Dans un sens, c'est un moyen d'implementer des idées en utilisant le format "if, then", par exemple, si la vitesse est/est plus que/est moins que, alors le joueur obtient un multiplicateur de dégats, ou alors "au plus quelque chose, au plus quelque chose" ou "plus on a quelque chose, plus quelque chose arrive"

The purpose of this document is to serve as a baseline for the addition of new items/Perks, by indexing any and all already established mechanic of the base game (If you notice something missing, please inform me)

Le but de ce document est de servir de base à l'addition de n'importe quel nouvel Item/WU, en répertoriant toutes les méchaniques déja établis dans le jeu de bae (n'hésitez pas à m'informer si il manque quelque chose selon vous)

---

## Table of contents/mechanics

### Mouvement

#### Jumping

| Mechanic | Modifiers |
|---|---|
| Multi-Jumping | A/R Jump Number, INF |
| Hauteur | A/R Jump Height, INF |
| Longueur | A/R Jump Length |
| Velocity — Conservation of speed from the ground | A/R Groundspeed |

#### In the air

| Mechanic | Modifiers |
|---|---|
| AirSpeed | A/R, INF |
| Airtime | INF |

#### Slide

| Mechanic | Modifiers |
|---|---|
| Speed of the Slide | A/R, INF |
| Slide Length | A/D, INF |

#### Slamming INF

| Mechanic | Modifiers |
|---|---|
| Air Slamming | INF |
| Air Slam Damage | ? |
| Power Slamming | INF |
| AOE Range | INF |
| AOE Damage | — |
| Bouncing Height | INF |

#### Dashing

| Mechanic | Modifiers |
|---|---|
| Dash Number | INF |
| Speed | — |
| Length | — |

#### Course

| Mechanic | Modifiers |
|---|---|
| Speed | INF |

#### Ennemy stepping INF

| Mechanic | Modifiers |
|---|---|
| A/R Height | — |
| A Temporary Stamina Boost | — |

#### Stamina

| Mechanic | Modifiers |
|---|---|
| A/R Stamina Value | INF |

#### Wall

| Mechanic | Modifiers |
|---|---|
| Wall Jumping | A/R Number of jumps, INF |
| Wall Clinging | — |

#### Whiplash

| Mechanic | Modifiers |
|---|---|
| A/R Minimum/Maximum Range | — |
| Grappling Speed | — |
| Dash Grappling | INF |

#### Gasoline INF

| Mechanic | Modifiers |
|---|---|
| Speed | — |
| Time on Gasoline | INF |

---

### Style

| Mechanic | Modifiers |
|---|---|
| No Style (default) | INF |
| Style | INF |
| A/R Style points | — |

---

### Ennemies

| Mechanic | Modifiers |
|---|---|
| Limb Shots | INF |
| Headshots | INF |
| Normal Shots | INF |
| Weight | INF |
| Flamable | INF |
| Husks | INF (with all corresponding ennemies) |
| Machines | INF (with all corresponding ennemies) |
| Demons | INF (with all corresponding ennemies) |
| Angels | INF (with all corresponding ennemies) |

---

### V1 Status

| Mechanic | Modifiers |
|---|---|
| Health | A/R Health, INF, A/R Health passive Regen |
| Chasing Projectiles | — |
| Death Mechanics — Reviving | INF |

---

### Weapons

#### Arms

##### FeedBacker (Blue Arm)

| Mechanic | Modifiers |
|---|---|
| Parrying | INF |
| Parrying Projectile | INF |
| Punching | INF |

##### KnuckleBlaster (Red Arm)

| Mechanic | Modifiers |
|---|---|
| Heavy Punching | INF |
| Shockwave | INF |

---

#### Ranged Weapons — Blue Weapons

##### Revolver

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Charging Shots | INF |

##### Shotgun

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Reload time | A/R |
| Magazine Size | A/R |
| Explosive Core Range | INF |
| Explosive Core Damage | A/R |

##### Nailgun

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Magnet | A/R |

##### RailCannon

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |

##### Rocket Launcher

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Time Stop | INF |

##### Alternate Blue Weapons — Revolver

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Charging | INF |

##### Alternate Blue Weapons — Shotgun

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Core Eject | A/R |

##### Alternate Blue Weapons — Nailgun

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Magnet | INF |
| Sawblade | INF |

##### Washer

---

#### Ranged Weapons — Red Weapons

##### Revolver

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Spinning Speed | INF |
| Richochet | INF |

##### Shotgun

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Saw Range | INF |
| Saw Damage | A/R |

##### Nailgun

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Overheating | A/R |

##### RailCannon

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| AOE Damage | A/R |

##### Rocket Launcher

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Gasoline Range | A/R |
| Gasoline Splash Size | A/R |
| Gasoline Fire Damage | A/R |

##### Alternate Red Weapons — Revolver

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Spinning Speed | INF |
| Richochet | INF |

##### Alternate Red Weapons — Shotgun

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Saw Range | INF |
| Saw Damage | A/R |
| Saw Punching | INF |

##### Alternate Red Weapons — Nailgun

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Saw Damage | A/R |
| Saw Punching | INF |
| Richochet | INF |
| Saw Durability | A/R |

---

#### Ranged Weapons — Green Weapons

##### Revolver

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Number of coins | INF |
| Throwing Speed | A/R |
| Hit multiplier | A/R |
| Throwing Momentum | A/R |

##### Shotgun

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Number of pumping required for Explosion | A/R |
| Explosion AOE | A/R |
| Explosion Damage | A/R |

##### Nailgun

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Charging Speed | A/R |
| Charging Storage | INF |
| Flaming Volley Speed | A/R |
| Flaming Volley Damage | A/R |

##### RailCannon

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Drill Grab time | A/R |
| Drill Bleeding | A/R |

##### Rocket Launcher

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Balls Bouncing | INF |

##### Alternate Green Weapons — Revolver

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Number of coins | INF |
| Throwing Speed | A/R |
| Hit multiplier | A/R |
| Throwing Momentum | A/R |

##### Alternate Green Weapons — Shotgun

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Number of pumping required for alt-shot | A/R |
| Storage | A/R |
| Explosion AOE | A/R |
| Explosion Damage | A/R |

##### Alternate Green Weapons — Nailgun

| Modifier | Type |
|---|---|
| Accuracy | A/R |
| Projectile Speed | A/R |
| Damage | A/R |
| Number of projectiles | A/R |
| Rate of Fire | A/R |
| Projectile Penetration | A/R |
| Magazine Size | A/R |
| Fire Saw Number of shots | A/R |
| Fire Saw Fire Damage | A/R |
| Fire Saw Durability | A/R |
