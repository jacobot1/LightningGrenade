using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightningGrenade.Scripts
{
    public class DamageAndKillScript
    {
        public static void DamageOrKillInRadius(Vector3 center, float radius, int damage, CauseOfDeath causeOfDeath)
        {
            int combinedLayers = 1084754248; // Shovel mask: should hit players and enemies

            Collider[] hits = Physics.OverlapSphere(center, radius, combinedLayers, QueryTriggerInteraction.Collide);

            foreach (var hit in hits)
            {
                // Try player
                PlayerControllerB player = hit.GetComponent<PlayerControllerB>();
                if (player != null && !player.isPlayerDead && player == GameNetworkManager.Instance.localPlayerController)
                {
                    if (damage == 0)
                    {
                        player.KillPlayer(Vector3.down * 17f, spawnBody: true, causeOfDeath);
                    }
                    else
                    {
                        player.DamagePlayer(damage, hasDamageSFX: true, callRPC: true, causeOfDeath, 0, fallDamage: false, Vector3.down * 17f);
                    }
                    continue;
                }

                // Try enemy
                EnemyAICollisionDetect enemyCollision = hit.GetComponent<EnemyAICollisionDetect>();
                if (enemyCollision != null && enemyCollision.mainScript != null)
                {
                    var enemy = enemyCollision.mainScript;

                    if (enemy.IsOwner && !enemy.isEnemyDead && enemy.enemyType.canDie)
                    {
                        if (damage == 0)
                        {
                            enemy.KillEnemyOnOwnerClient();
                        }
                        else if (enemy is IHittable hittable)
                        {
                            int force = Mathf.Clamp(damage / 25, 1, 5); // Rough scaling (customize as needed)
                            hittable.Hit(force, Vector3.down, GameNetworkManager.Instance.localPlayerController, playHitSFX: true);
                        }
                    }
                }
            }
        }
    }
}
