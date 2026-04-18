The Binding of V1
A roguelike mod for ULTRAKILL. Run through handcrafted rooms, collect bizarre items dropped by defeated enemies, and spend your hard-earned souls at the shop between floors. Your combat style directly impacts your economy — the more stylish your kills, the more souls you earn.

Status: Early development — not yet playable


Requirements

ULTRAKILL (Steam)
r2modman
Visual Studio 2022+ with .NET desktop development workload
Unity 2019.4.40f1 (for map editing only)


Setting up the project
1. Clone the repo
bashgit clone https://github.com/killi/The-Biding-of-V1.git
2. Install required mods via r2modman
Create a profile for ULTRAKILL and install :

BepInExPack
AngryLevelLoader
PluginConfigurator

3. Set up the lib/ folder
Create a lib/ folder at the root of the project and copy the following files into it :
FileLocationBepInEx.dll[r2modman profile]\BepInEx\core\0Harmony.dll[r2modman profile]\BepInEx\core\Assembly-CSharp.dll[ULTRAKILL install]\ULTRAKILL_Data\Managed\UnityEngine.dll[ULTRAKILL install]\ULTRAKILL_Data\Managed\

These files are excluded from version control (.gitignore). Every contributor must supply their own copies.

Finding your r2modman profile folder :
Open r2modman → select your ULTRAKILL profile → Browse profile folder in the left menu.
Finding your ULTRAKILL install folder :
Steam → right click ULTRAKILL → Manage → Browse local files
4. Add references in Visual Studio

Right click References in the Solution Explorer
Add Reference → Browse
Navigate to the lib/ folder
Select all .dll files → Add → OK

5. Build the project
Ctrl+Shift+B or Build → Build Solution
The compiled .dll will be in bin/Debug/.

Testing in game

Copy bin/Debug/TheBindingOfV1.dll to your r2modman profile's BepInEx/plugins/ folder
Launch ULTRAKILL via r2modman (Start modded)
The BepInEx console should show :

Mod TheBindingOfV1 version 0.1.0 is loading...
Mod TheBindingOfV1 version 0.1.0 is loaded!

Project structure
The-Biding-of-V1/
├── Plugin.cs          ← main plugin entry point
├── Properties/
│   └── AssemblyInfo.cs
├── lib/               ← DLL references (gitignored, fill manually)
└── bin/               ← build output (gitignored)

Contributing

Join the UltraModding Discord for modding help
Join the Envy & Spite Discord for map editing help
Check the conception doc and item list for design reference


License
MIT