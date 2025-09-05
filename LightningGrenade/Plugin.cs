using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LightningGrenade.Patches;
using UnityEngine;

namespace LightningGrenade
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("com.jacobot5.EZLightning")]
    public class LightningGrenadeMod : BaseUnityPlugin
    {
        // Mod metadata
        public const string modGUID = "com.jacobot5.LightningGrenade";
        public const string modName = "LightningGrenade";
        public const string modVersion = "2.0.0";

        // Initalize Harmony
        private readonly Harmony harmony = new Harmony(modGUID);

        // Configuration
        public static ConfigEntry<float> configLightningVolume;
        public static ConfigEntry<float> configLightningDamageRadius;

        // Create static instance
        public static LightningGrenadeMod Instance;

        // Initialize logging
        public static ManualLogSource mls;

        private void Awake()
        {
            // Ensure static instance
            if (Instance == null)
            {
                Instance = this;
            }

            // Send alive message
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo("LightningGrenade has awoken.");

            // Bind configuration
            configLightningVolume = Config.Bind("Lightning.Thunder",
                                                "Volume",
                                                1f,
                                                "How loud thunder should be. Default is 1; ranges from 0-1");
            // Bind configuration
            configLightningDamageRadius = Config.Bind("Lightning.Damage",
                                                "Radius",
                                                3f,
                                                "How close players/enemies must be to a lighting bolt to be killed by it");

            // Do the patching
            harmony.PatchAll(typeof(LightningGrenadeMod));
            harmony.PatchAll(typeof(StunGrenadeItemPatch));
            harmony.PatchAll(typeof(KickIfModNotInstalled));
        }
    }
}
