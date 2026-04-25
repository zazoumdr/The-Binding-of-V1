# Game Design Document

Let's do a quick recap of all the basic Gamedesign in UltraKill, like every game, it's gameplay and gamedesign is based on differents axiomes and sub-axiomes, which need, and should be understood, reused, and meshed to create something new for an ambitious mod 
Our work here will be on items/perks, focusing( without being limited to) :

## Lexicon

**A/R** : Adding/Removing, represents the capacity of certain items/perks to add or remove a value in a variable, e.g **A/R** ground speed, represents the capacity for an item/perks to have an influence on the airspeed of the caracter following a jump, a short run, or a slide, depending on the situation or the intent of the object.

**INF** : Influence, Influence on other variables, some of the actions of the caracter or/and it's environnement will be able to aply other related or unrelated effects to the environnement, ennemies, physics etc... It's main purpose is to facilitate the creation of brand new and original mechanics.
I will give examples in the third part of this doc, see L

**WU** : Weapon Upgrades

Ex : 
- MultiJump
  - A/G Number of jumps, **INF**

**INF** being for example, a chance to make mines appear at random locations, at a minimum fixed distance from the Player, inside the current room.

In a sense, it's a way to implemeting ideas using the "if, then" formula, e.g : If speed is/is more than/is less than/, then give a Damage multiplier, or, "the more something, the more something" or "the more you have of something, the more something happens"

Dans un sens, c'est un moyen d'implementer des idées en utilisant le format "if, then", par exemple, si la vitesse est/est plus que/est moins que, alors le joueur obtient un multiplicateur de dégats, ou alors "au plus quelque chose, au plus quelque chose" ou "plus on a quelque chose, plus quelque chose arrive"

The purpose of this document is to serve as a baseline for the addition of new items/Perks, by indexing any and all already established mechanic of the base game (If you notice something missing, please inform me)

Le but de ce document est de servir de base à l'addition de n'importe quel nouvel Item/**WU**, en répertoriant toutes les méchaniques déja établis dans le jeu de bae (n'hésitez pas à m'informer si il manque quelque chose selon vous)

## Table of contents/mechanics :

### Mouvement

- Jumping
  - Multi-Jumping
    - **A/R** Jump Number, **INF**
  - Hauteur
    - **A/R** Jump Height **INF**
  - Longueur
    - **A/R** Jump Length 
  - Velocity
    - Conservation of speed from the ground 
    - **A/R** Groundspeed
- In the air
  - **A/R** AirSpeed, **INF**
  - Airtime, **INF**
- Slide
  - **A/R** Speed of the Slide **INF**
  - A/D Slide Lenght **INF**
- Slamming **INF**
  - Air Slamming **INF** 
    - Air Slam Damage ?
  - Power Slamming **INF**
    - AOE
      - Range **INF**
      - Damage
  - Bouncing
    - Height **INF**
- Dashing
  - Dash Number **INF**
  - Speed
  - Lenght
- Course
  - Speed **INF**
- Ennemy stepping **INF**
  - **A/R** Height
  - A Temporary Stamina Boost
- Stamina 
  - **A/R** Stamina Value **INF** 
- Wall
  - Wall Jumping 
    **A/R** Number of jumps **INF**
  - Wall Clinging 
- Whiplash 
  - **A/R** Minimum/Maximum Range
  - Grappling Speed
    - Dash Grappling **INF**
- Gasoline **INF**
  - Speed 
  - Time on Gasoline **INF**

### Style

- No Style(default)**INF**
- Style **INF**
  - **A/R** Style points 

### Ennemies

- Shots modifiers
  - Limb Shots **INF**
  - Headshots **INF**
  - Normal Shots **INF** 
- Weight **INF**
- Ennemy Types
  - Flamable **INF**
  - Husks **INF** (with all corresponding ennemies)
  - Machines **INF** (with all corresponding ennemies)
  - Demons **INF** (with all corresponding ennemies)
  - Angels **INF** (with all corresponding ennemies)

### V1 Status

- Health 
  **A/R** Health **INF**
  **A/R** Health passive Regen 
- Chasing Projectiles 
- Death Mechanics
  - Reviving **INF**
  
### Weapons

- Arms 
  - FeedBacker(Blue Arm)
    - Parrying **INF**
    - Parrying Projectile **INF**
    - Punching **INF**
  - KnuckleBlaster(Red Arm)
    - Heavy Punching **INF**
    - Shockwave **INF**
  - Ranged Weapons
    - Blue Weapons
      - Revolver
        - **A/R**
          - Accuracy
          - Projectile Speed 
          - Damage
          - Number of projectiles 
          - Rate of Fire
          - Projectile Penetration
          - Magazine Size
          - Charging Shots **INF** 
      - Shotgun
        - **A/R**
          - Accuracy
          - Projectile Speed 
          - Damage
          - Number of projectiles
          - Rate of Fire
          - Projectile Penetration
          - Reload time 
          - Magazine Size
          - Explosive Core
            - Range
            - Damage
      - Nailgun
        - **A/R**
          - Accuracy
          - Projectile Speed 
          - Damage
          - Number of projectiles 
          - Rate of Fire
          - Projectile Penetration
          - Magazine Size
          - Magnet
      - RailCannon
        - **A/R**
          - Accuracy
          - Projectile Speed 
          - Damage
          - Number of projectiles 
          - Rate of Fire
          - Projectile Penetration
          - Magazine Size
      - Rocket Launcher
        - **A/R**
          - Accuracy
          - Projectile Speed 
          - Damage
          - Number of projectiles 
          - Rate of Fire
          - Projectile Penetration
          - Magazine Size
          - Time Stop **INF**
      - Alternate Blue Weapons
        - Revolver
          - **A/R**
            - Accuracy
            - Projectile Speed 
            - Damage
            - Number of projectiles 
            - Rate of Fire
            - Projectile Penetration
            - Magazine Size
            - Charging **INF**
        - Shotgun
          - **A/R**
            - Accuracy
            - Projectile Speed 
            - Damage
            - Number of projectiles 
            - Rate of Fire
            - Projectile Penetration
            - Magazine Size
            - Core Eject
        - Nailgun
          - **A/R**
            - Accuracy
            - Projectile Speed 
            - Damage
            - Number of projectiles 
            - Rate of Fire
            - Projectile Penetration
            - Magazine Size
            - Magnet **INF**
            - Sawblade **INF**
      - Washer
    - Red Weapons
      - Revolver
        - **A/R**
          - Accuracy
          - Projectile Speed 
          - Damage
          - Number of projectiles 
          - Rate of Fire
          - Projectile Penetration
          - Magazine Size
          - Spinning Speed **INF**
          - Richochet **INF**
      - Shotgun
        - **A/R**
          - Accuracy
          - Projectile Speed 
          - Damage
          - Number of projectiles 
          - Rate of Fire
          - Projectile Penetration
          - Magazine Size
          - Saw
            - Range **INF**
            - Damage
      - Nailgun
        - **A/R**
          - Accuracy
          - Projectile Speed 
          - Damage
          - Number of projectiles 
          - Rate of Fire
          - Projectile Penetration
          - Magazine Size
          - Overheating
      - RailCannon
        - **A/R**
          - Accuracy
          - Projectile Speed 
          - Damage
          - Number of projectiles 
          - Rate of Fire
          - Projectile Penetration
          - Magazine Size
          - AOE
            - Damage
      - Rocket Launcher
        - **A/R**
          - Accuracy
          - Projectile Speed 
          - Damage
          - Number of projectiles 
          - Rate of Fire
          - Projectile Penetration
          - Magazine Size
          - Gasoline
            - Range
            - Splash Size
            - Fire Damage
      - Alternate Red Weapons
        - Revolver
          - **A/R**
            - Accuracy
            - Projectile Speed 
            - Damage
            - Number of projectiles 
            - Rate of Fire
            - Projectile Penetration
            - Magazine Size
            - Spinning Speed **INF**
            - Richochet **INF**
        - Shotgun
          - **A/R**
            - Accuracy
            - Projectile Speed 
            - Damage
            - Number of projectiles 
            - Rate of Fire
            - Projectile Penetration
            - Magazine Size
            - Saw
              - Range **INF**
              - Damage
              - Saw Punching **INF**
        - Nailgun
          - **A/R**
            - Accuracy
            - Projectile Speed 
            - Damage
            - Number of projectiles 
            - Rate of Fire
            - Projectile Penetration
            - Magazine Size
            - Saw
              - Damage
              - Saw Punching **INF**
              - Richochet **INF**
              - Durability
    - Green Weapons
      - Revolver
        - **A/R**
          - Accuracy
          - Projectile Speed 
          - Damage
          - Number of projectiles 
          - Rate of Fire
          - Projectile Penetration
          - Magazine Size
          - Coins
            - Number of coins **INF**
            - Throwing Speed 
            - Hit multiplier
            - Throwing Momentum
      - Shotgun
        - **A/R**
          - Accuracy
          - Projectile Speed 
          - Damage
          - Number of projectiles 
          - Rate of Fire
          - Projectile Penetration
          - Magazine Size
          - Number of pumping required for Explosion
            - Explosion
              - AOE
              - Damage
      - Nailgun  
        - **A/R**
          - Accuracy
          - Projectile Speed 
          - Damage
          - Number of projectiles 
          - Rate of Fire
          - Projectile Penetration
          - Magazine Size
          - Charging
            - Speed
            - Storage **INF**
            - Flaming volley
              - Speed
              - Damage
      - RailCannon
        - **A/R**
          - Accuracy
          - Projectile Speed 
          - Damage
          - Number of projectiles 
          - Rate of Fire
          - Projectile Penetration
          - Magazine Size
          - Drill
            - Grab time 
            - Bleeding
      - Rocket Launcher
        - **A/R**
          - Accuracy
          - Projectile Speed 
          - Damage
          - Number of projectiles 
          - Rate of Fire
          - Projectile Penetration
          - Magazine Size
          - Balls **INF**
            - Bouncing
      - Alternate Green Weapons
        - Revolver
          - **A/R**
            - Accuracy
            - Projectile Speed 
            - Damage
            - Number of projectiles 
            - Rate of Fire
            - Projectile Penetration
            - Magazine Size
            - Coins
              - Number of coins **INF**
              - Throwing Speed 
              - Hit multiplier
              - Throwing Momentum
        - Shotgun
          - **A/R**
            - Accuracy
            - Projectile Speed 
            - Damage
            - Number of projectiles 
            - Rate of Fire
            - Projectile Penetration
            - Magazine Size
            - Pumping
              - Number of pumping required for alt-shot
              - Storage
            - Explosion
              - AOE
              - Damage
        - Nailgun
          - **A/R**
            - Accuracy
            - Projectile Speed 
            - Damage
            - Number of projectiles 
            - Rate of Fire
            - Projectile Penetration
            - Magazine Size
            - Fire Saw
              - Number of shots
              - Fire Damage
              - Durability

### Examples (TBD)


