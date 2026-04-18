using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using UnityEngine;
using HarmonyLib;
using GameConsole.pcon;

namespace TheBindingOfV1
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
        private const string modGUID = "killi.TheBindingOfV1";

        /// <summary>
        /// Make sure this is the same modName as the modGUID (without modAuthor)
        /// </summary>
        private const string modName = "TheBindingOfV1";

        /// <summary>
        /// Self explainitory.
        /// </summary>
        private const string modVersion = "0.1.0";

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


        //[HarmonyPatch(typeof(Revolver), "Shoot")]
        //public static class RevolverFireRatePatch
        //{
        //    public static float fireRateMultiplier = 2f;

        //    [HarmonyPostfix]
        //    public static void Postfix(Revolver __instance)
        //    {
        //        if (fireRateMultiplier <= 1f) return;

        //        // Après le tir, shootCharge est à 0 et shootReady à false
        //        // On le remonte artificiellement selon le multiplicateur
        //        __instance.shootCharge = 100f * (fireRateMultiplier - 1f);
        //    }
        //}

        public class ItemPickup : MonoBehaviour
        {
            private void OnTriggerEnter(Collider other)
            {
                NewMovement player = other.GetComponent<NewMovement>();
                if (player == null) return;

                player.walkSpeed += 20f;
                //RevolverFireRatePatch.fireRateMultiplier = 2f;

                Debug.Log("Soda Quelconque ramassé !");
                Destroy(gameObject);
            }
        }

        // Patch — drop item à la mort d'un Filth
        [HarmonyPatch(typeof(EnemyIdentifier), "Death", new Type[] { })]
        public static class FilthDropPatch
        {
            [HarmonyPostfix]
            public static void Postfix(EnemyIdentifier __instance)
            {
                if (__instance.enemyType != EnemyType.Filth) return;
                if (UnityEngine.Random.value > 0.15f) return;

                GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
                item.transform.position = __instance.transform.position + Vector3.up;
                item.transform.localScale = Vector3.one * 0.5f;
                item.GetComponent<Collider>().isTrigger = true;
                item.AddComponent<ItemPickup>();

                Debug.Log("Item dropped !");
            }
        }




    }
    //[HarmonyPatch(typeof(NewMovement), "Start")]
    //public static class TestMoveSpeedPatch
    //{
    //    [HarmonyPostfix]
    //    public static void Postfix(NewMovement __instance)
    //    {
    //        __instance.walkSpeed *= 5f;
    //    }
    //}
}
