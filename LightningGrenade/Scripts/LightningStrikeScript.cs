using DigitalRuby.ThunderAndLightning;
using UnityEngine;

namespace LightningGrenade.Scripts
{
    // Credit to Zagster, I updated the code to work with the latest version of C# and made some optimizations.
    public class LightningStrikeScript
    {
        public static StormyWeather? stormyWeather = null;
        public static void SpawnLightningBolt(Vector3 strikePosition, AudioSource lightningAudio)
        {
            Vector3 offset = new Vector3(UnityEngine.Random.Range(-32, 32), 0f, UnityEngine.Random.Range(-32, 32));
            Vector3 vector = strikePosition + Vector3.up * 160f + offset;

            if (stormyWeather == null)
            {
                stormyWeather = UnityEngine.Object.FindObjectOfType<StormyWeather>(true);
                LightningGrenadeMod.mls.LogInfo("Getting stormy weather for the first time");
            }
            
            // Plugin.ExtendedLogging($"{vector} -> {strikePosition}");

            LightningBoltPrefabScript localLightningBoltPrefabScript = UnityEngine.Object.Instantiate(stormyWeather.targetedThunder);
            localLightningBoltPrefabScript.enabled = true;

            localLightningBoltPrefabScript.GlowWidthMultiplier = 2.5f;
            localLightningBoltPrefabScript.DurationRange = new RangeOfFloats { Minimum = 0.6f, Maximum = 1.2f };
            localLightningBoltPrefabScript.TrunkWidthRange = new RangeOfFloats { Minimum = 0.6f, Maximum = 1.2f };
            localLightningBoltPrefabScript.Camera = GameNetworkManager.Instance.localPlayerController.gameplayCamera;
            localLightningBoltPrefabScript.Source.transform.position = vector;
            localLightningBoltPrefabScript.Destination.transform.position = strikePosition;
            localLightningBoltPrefabScript.AutomaticModeSeconds = 0.2f;
            localLightningBoltPrefabScript.Generations = 8;
            localLightningBoltPrefabScript.CreateLightningBoltsNow();

            lightningAudio.transform.position = strikePosition;
            if (GameNetworkManager.Instance.localPlayerController.isInsideFactory)
            {
                lightningAudio.volume = LightningGrenadeMod.configLightningVolume.Value / 2;
            }
            else
            {
                lightningAudio.volume = LightningGrenadeMod.configLightningVolume.Value;
            }
            stormyWeather.PlayThunderEffects(strikePosition, lightningAudio);
        }
    }
}