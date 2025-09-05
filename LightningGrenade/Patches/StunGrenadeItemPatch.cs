using HarmonyLib;
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

            // Lightning strike position
            Vector3 strikePosition = __instance.transform.position;

            // Cutoff for lightning bolt if hitting something above
            float lightningCutoff = 0f;
            
            if (GameNetworkManager.Instance.localPlayerController.isInsideFactory)
            {
                RoundManager roundManager = Object.FindObjectOfType<RoundManager>();
                roundManager.SwitchPower(on: false);
                lightningCutoff =  80f; // Move up out of sight theoretically
            }
            else if (StartOfRound.Instance.shipBounds.bounds.Contains(strikePosition))
            {
                lightningCutoff =  9f; // Move up out of sight theoretically
            }
            else
            {
                EZDamage.API.KillEverything(strikePosition, LightningGrenadeMod.configLightningDamageRadius.Value, CauseOfDeath.Electrocution);
            }
            // Fire lightning bolt
            EZLightning.API.Strike(strikePosition + Vector3.up * lightningCutoff, LightningGrenadeMod.configLightningVolume.Value);

            // __instance.itemAudio.PlayOneShot(__instance.explodeSFX); Don't play normal explosion sound
            // WalkieTalkie.TransmitOneShotAudio(__instance.itemAudio, __instance.explodeSFX); Also don't play over walkie
            if (__instance.DestroyGrenade)
            {
                __instance.DestroyObjectInHand(__instance.playerThrownBy);
            }

            // Replacing the original method
            return false;
        }
    }
}
