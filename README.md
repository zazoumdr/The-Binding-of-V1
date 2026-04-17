# ULTRAKILL MODDING TEMPLATE
Created by "thebluenebula" and made possible with the help of BepInEx.
# How to Setup The Template
- First of all, install the template and move it to "[LocalUserFolder]\Documents
- Navigate to the folder of your version of Visual Studio you would to use the template in. (Visual Studio 20XX)
- Find the "My Exported Templates" folder and open it.
- Paste the previously obtained .zip there.
- When opening Visual Studio, you will find the template as a project template.

# How to Setup References
**MANDATORY**
- Install ULTRAKILL with BepInEx.
- Right click the "lib" folder and then click "Add Existing Item" from the dropdown.
- Change the filter mode to "All Files" in the new pop-up.
- Navigate to ULTRAKILL's root.

**FOR SETTING UP THE LIB FOLDER (BEPINEX)**
- Find "BepInEx.dll" and "0Harmony.dll" in "BepInEx/Core".
- Select both by holding CTRL and then click Add.

**FOR SETTING UP THE LIB FOLDER (ULTRAKILL)**
Depending on what your mod is and what it does, some .dll files may not be needed. You will have to experiment with what .dll(s) you need. **However some .DLL(s) are mandatory so do not skip this step.**
- Navigate to "ULTRAKILL_Data/Managed".
- Select "Assembly-CSharp.dll" and "UnityEngine.dll" **(MANDATORY)**
- From there, select any other .DLL(s) that are/is needed for your mod.

**FOR SETTING UP THE REFERENCES**
- Right click the template and click "Copy Full Path" from the dropdown. (Optional)
- Right click "References" in the project and select "Add Reference..." from the dropdown.
- Click on "Browse" on the new pop-up.
- Navigate to the project's "lib" folder. (If you copied the project path earlier, go there.)
- Select all ".dll" files inside of the lib folder by hitting CTRL + A.
- Click Add on the pop-up.
- Click Ok on the "Reference Manager" window.

# RECOMMENDED PROGRAMS
- [dnSpyEx](https://github.com/dnSpyEx) is a continuation of dnSpy which allows you to view .NET dlls. Useful for looking at ULTRAKILL's code for modding.
- [Spite](https://discord.com/invite/envy-spite-1227272001719111750) is a level editor that allows you to look at how ULTRAKILL levels are made and create bundle files for mods. (Rude can also be used as an alternative.)

