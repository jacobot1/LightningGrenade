using GameNetcodeStuff;
using HarmonyLib;
using LightningGrenade.Scripts;
using System.Reflection;
using UnityEngine;

namespace LightningGrenade.Patches
{
    [HarmonyPatch(typeof(StunGrenadeItem))]
    internal class StunGrenadeItemPatch
    {
        [HarmonyPatch("ExplodeStunGrenade")]
        [HarmonyPrefix]
        static bool LightningStrikePatch(StunGrenadeItem __instance)
        {
            // Set up AudioSource
            AudioSource thunderOrigin;
            GameObject audioObject = new GameObject("LightningGrenade_ThunderAudioSource");
            audioObject.SetActive(true);
            UnityEngine.Object.DontDestroyOnLoad(audioObject); // persist across scenes
            thunderOrigin = audioObject.AddComponent<AudioSource>();
            thunderOrigin.spatialBlend = 0f; // 2D sound
            thunderOrigin.minDistance = 5f;
            thunderOrigin.maxDistance = 200f;
            thunderOrigin.rolloffMode = AudioRolloffMode.Logarithmic;
            thunderOrigin.playOnAwake = false;
            thunderOrigin.loop = false;
            thunderOrigin.enabled = true;

            // Do normal sequence
            if (__instance.hasExploded)
            {
                return false;
            }
            if ((__instance.chanceToExplode < 100f && !__instance.explodeOnThrow) || (__instance.explodeOnCollision && !StartOfRound.Instance.currentLevel.spawnEnemiesAndScrap && __instance.parentObject == UnityEngine.Object.FindObjectOfType<DepositItemsDesk>().deskObjectsContainer))
            {
                if (__instance.playerThrownBy != null)
                {
                    __instance.playerThrownBy.activatingItem = false;
                }
                return false;
            }
            __instance.hasExploded = true;
            Transform parent = ((!__instance.isInElevator) ? RoundManager.Instance.mapPropsContainer.transform : StartOfRound.Instance.elevatorTransform);

            // Fire lightning bolt
            LightningGrenade.Scripts.LightningStrikeScript.SpawnLightningBolt(__instance.transform.position, thunderOrigin);
            DamageAndKillScript.DamageOrKillInRadius(__instance.transform.position, LightningGrenadeMod.configLightningDamageRadius.Value, 0, CauseOfDeath.Electrocution);

            __instance.itemAudio.PlayOneShot(__instance.explodeSFX);
            WalkieTalkie.TransmitOneShotAudio(__instance.itemAudio, __instance.explodeSFX);
            if (__instance.DestroyGrenade)
            {
                __instance.DestroyObjectInHand(__instance.playerThrownBy);
            }

            // Replacing the original method
            return false;
        }
    }
}
