using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.Server {
    public class Enemy : Entity {
        public EnemyData data;
        Player player;

        public ShootBehaviour shooter;
        public MovementBehaviour movement;
        public SummonBehaviour summoner;

        public void Init(EnemyData ed, Player p) {
            Init();
            OnDeathEvent += OnDestroyed;

            data = ed;
            player = p;

            SetStats();
        }

        public override void SetStats() {
            Stats stats = data.stats.levels[level];

            shooter.stats = stats;

            movement.speed = stats.movementSpeed;

            maxHealth = health = stats.hp;

            shooter.damageType = data.stats.damageType;
            SetCurrentStats(stats);


            if (summoner != null) {
                summoner.Init(stats, this, true);
            }
        }

        // Use this for initialization
        void Start() {
            InvokeRepeating("UpdateTarget", 0f, 1f);
        }

        void UpdateTarget() {
            float shortestDistance = Mathf.Infinity;
            Entity nearestT = null;

            foreach (Building b in player.buildings) {
                if (b.isDead) continue;

                float distanceToEnemy = Vector3.Distance(transform.position, b.transform.position);
                if (distanceToEnemy < shortestDistance) {
                    shortestDistance = distanceToEnemy;
                    nearestT = b;
                }
            }

            foreach (Unit u in player.units) {
                if (u.isDead) continue;

                float distanceToEnemy = Vector3.Distance(transform.position, u.transform.position);
                if (distanceToEnemy < shortestDistance) {
                    shortestDistance = distanceToEnemy;
                    nearestT = u;
                }
            }


            if (nearestT != null) {
                if (movement.target == nearestT.transform) return;

                if (nearestT != shooter.target) {
                    ServerSend.SyncTarget(serverId, nearestT.serverId);
                }

                movement.target = nearestT.transform;
                shooter.target = nearestT;
            } else {
                if (movement.target == player.castleTransform) return;

                movement.target = player.castleTransform;
                shooter.target = null;

                ServerSend.SyncTarget(serverId, 0, true);
            }
        }

        private void FixedUpdate() {
            if (shooter.isShooting) {
                movement.canMove = false;
            } else {
                movement.canMove = true;
            }

            if (Vector3.Distance(transform.position + Vector3.down * transform.position.y, player.castleTransform.position + Vector3.down * player.castleTransform.position.y) < 0.6f) {
                player.AddHp((short)-data.stats.hpConsume);

                OnDestroyed();
            }
        }

        public override void OnDestroyed() {
            if (Vector3.Distance(transform.position + Vector3.down * transform.position.y, player.castleTransform.position + Vector3.down * player.castleTransform.position.y) > 0.65f)
                player.AddGold(data.stats.levels[0].price / 10);
            else player.AddGold(Mathf.Max(1, data.stats.levels[0].price / 25));

            player.enemies.Remove(this);

            base.OnDestroyed();
            Destroy(gameObject);
        }

        public override void TryUpgrade() {

        }

        public override void AddSlow(int value, float time) {
            movement.AddSlow(value, time);
        }

        public override Player GetOwner() {
            return player;
        }
    }
}