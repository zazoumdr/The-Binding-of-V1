#  Rude Level Editor — Map Creation Guide

---

## 1. Level Structure

Every room must follow this hierarchy:

```
1 - RoomName
├── Non-Stuff        (static geometry — floors, walls, ceiling)
└── Stuff            (arenas, enemies — resets on checkpoint)
    ├── ArenaTrigger
    ├── Wave1
    └── Wave2
```

>  The **Stuff** object MUST have a **GoreZone** component attached.

- **Non-Stuff** → everything that doesn't move and doesn't reset
- **Stuff** → everything that resets if the player respawns from a checkpoint

---

## 2. Setting Up an Arena

### Step by step

1. Inside `Stuff`, create a **cube** for the trigger
   - Add a **Box Collider** set to **Is Trigger**
   - Set the material to **Enemy Trigger Material**
   - Set the layer to **Invisible**

2. Create your **Wave objects** (empty GameObjects) inside `Stuff`
   - Add the **Activate Next Wave** component to each wave
   - Set the **enemy count** to match the number of enemies in that wave

3. **Place your enemies** inside their respective wave objects
   -  **Disable all enemies** before exporting — otherwise they spawn instantly on room load

4. **Configure the Trigger**:
   - `Doors` → assign the room doors (they will lock when triggered)
   - `Enemies` → assign the enemies of Wave 1

5. **Configure each wave's Activate Next Wave**:
   - All waves except the last: set `Next Enemies` to the next wave's enemies
   - Last wave: leave `Next Enemies` empty, set `Doors` to unlock the room doors

### Example — 2 waves

```
Stuff [GoreZone]
├── ArenaTrigger
│   ├── Doors: [Door1, Door2]
│   └── Enemies: [Filth1, Filth2]
├── Wave1 [Activate Next Wave — count: 2]
│   ├── Next Enemies: [Schism1]
│   ├── Filth1 (DISABLED)
│   └── Filth2 (DISABLED)
└── Wave2 [Activate Next Wave — count: 1]
    ├── Next Enemies: (empty)
    ├── Doors: [Door1, Door2]
    └── Schism1 (DISABLED)
```

---

## 3. Checkpoints

**Prefab location:** `Assets/ULTRAKILL Assets/Prefabs/Levels/Checkpoint`

Two types:
- **Checkpoint** → one-time use → put Stuff in `Rooms`
- **CheckpointReusable** → multi-use → put Stuff in `Rooms To Inherit`

> Never fill both `Rooms` AND `Rooms To Inherit` at the same time.

### Setup

- `To Activate` → the room object this checkpoint belongs to
- `Rooms` → Stuff objects to clone on level load (one-time checkpoint)
- `Rooms To Inherit` → Stuff objects to clone when checkpoint is grabbed (reusable)
- `Doors To Unlock` → doors to unlock when player respawns here

### Placement in hierarchy

Checkpoints always go **between** rooms:

```
1 - RoomFirst
Checkpoint   ← clones Stuff from RoomSecond
2 - RoomSecond
```

>  The checkpoint's local **+Z direction** is where the player faces on respawn.

### Troubleshooting
- Checkpoint spits you into the void → something is wrong with the setup
- Check: `ToActivate` is filled, at least 1 Stuff in Rooms or RoomsToInherit, no missing references

---

## 4. Exporting / Compiling

1. Open **RUDE → Rude Exporter** in the Unity toolbar
2. First time only: click the **gear icon** → find `Output Path`
   - In ULTRAKILL: **Options → Plugin Config → Angry Level Loader Configure → Settings → Open Levels Folder**
   - Copy that path and paste it in the `Output Path` field
3. Click the **export button** (blue button with a box and arrow)
   - First export takes a while — subsequent exports are faster
4. Launch ULTRAKILL via r2modman → the map is automatically loaded

---

## 5. Quick Reference

| Component | Where to find |
|---|---|
| GoreZone | Add Component → search "GoreZone" |
| Activate Next Wave | Add Component → search "Activate Next Wave" |
| Checkpoint | `Assets/ULTRAKILL Assets/Prefabs/Levels/` |
| Enemy Trigger Material | Assets → search "Enemy Trigger" |
| Enemies prefabs | `Assets/ULTRAKILL Assets/Prefabs/Enemies/` |

---

## 6. Useful Links

- Full doc : https://envy-spite-team.github.io/ULTRAMappingDocs/
- Arenas : https://envy-spite-team.github.io/ULTRAMappingDocs/Tutorials/Arenas%20And%20Enemies
- Checkpoints : https://envy-spite-team.github.io/ULTRAMappingDocs/Tutorials/Checkpoints
- Compiling : https://envy-spite-team.github.io/ULTRAMappingDocs/Tutorials/Compiling
- Discord : https://discord.gg/KqK5yDsRjQ
