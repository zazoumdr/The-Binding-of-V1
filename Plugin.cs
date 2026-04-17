using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using UnityEngine;
using HarmonyLib;
using GameConsole.pcon;

namespace ULTRAKILLBepInExTemplate
{
    /// <summary>
    /// Don't panic if you see errors here! Just follow the instructions in "README.md"
    /// </summary>
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        /// <summary>
        /// Make sure the modGUID is unique and is not used by any other mods!
        /// </summary>
        private const string modGUID = "modAuthor.modName";

        /// <summary>
        /// Make sure this is the same modName as the modGUID (without modAuthor)
        /// </summary>
        private const string modName = "modName";

        /// <summary>
        /// Self explainitory.
        /// </summary>
        private const string modVersion = "1.0.0";

        private static readonly Harmony Harmony = new Harmony(modGUID);

        /// <summary>
        /// Anything that goes in here will be executed the exact moment the mod is loaded.
        /// It is recommended that you log that the mod is loaded when this happens.
        /// </summary>
        private void Awake()
        {
            Debug.Log($"Mod {modName} version {modVersion} is loading...");
            Harmony.PatchAll();
            Debug.Log($"Mod {modName} version {modVersion} is loaded!");
        }

        /// <summary>
        /// Every tick this event will trigger (basically this will trigger every single frame)
        /// </summary>
        private void Update()
        {
            
        }

        /// Make sure to rename ExamplePatch to be the same class you're patching and make sure to rename Function to a function in that class!
        /// Example of a HarmonyPatch. (Feel free to delete!)
        // [HarmonyPatch(typeof(ExamplePatch), "Function")]
        // public static class ExamplePatch
        // {
        ///     // Either use Postfix or Prefix.
        //     [HarmonyPostfix]
        //     public static void Postfix()
        //     {
        //         // Code here.
        //     }
        // }
    }
}
