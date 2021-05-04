using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.Server {
    public class Unit : Entity {
        public UnitData data;

        Player player;

        public ShootBehaviour shooter;
        public MovementBehaviour movement;
        public HealBehaviour healer;

        [HideInInspector] public Vector3 startPos;

        public bool isSummon;

        public void Init(UnitData ud, Player p) {
            Init();
            OnDeathEvent += OnDestroyed;

            data = ud;
            player = p;

            shooter.damageType = data.stats.damageType;
            SetStats();

            startPos = transform.position;
        }

        public override void SetStats() {
            Stats stats = data.stats.levels[level];

            if (shooter != null) {
                shooter.stats = stats;
            }

            if (healer != null) {
                healer.healAmount = stats.healAmount;
                healer.healRate = stats.healRate;

                healer.pm = player;
            }

            maxHealth = health = stats.hp;
            movement.speed = stats.movementSpeed;

            SetCurrentStats(stats);
        }

        // Use this for initialization
        void Start() {
            if (shooter != null)
                InvokeRepeating("UpdateTarget", 0f, 1f);
        }

        void UpdateTarget() {
            float shortestDistance = Mathf.Infinity;
            Entity nearestT = null;

            foreach (Enemy e in player.enemies) {
                if (e.isDead) continue;

                float distanceToEnemy = Vector3.Distance(transform.position, e.transform.position);
                if (distanceToEnemy < shortestDistance) {
                    shortestDistance = distanceToEnemy;
                    nearestT = e;
                }
            }


            if (nearestT != null) {
                movement.target = nearestT.transform;
                shooter.target = nearestT;
            } else {
                movement.target = null;
                shooter.target = null;
            }
        }

        private void FixedUpdate() {
            if (shooter.isShooting) {
                movement.canMove = false;
            } else {
                movement.canMove = true;
            }
        }

        public override void OnDestroyed() {
            base.OnDestroyed();
        }

        public override void TryUpgrade() {
            if (GameManager._instance.state == GameManager.GameState.InWave) return;
            if (level >= data.stats.levels.Length - 1) return; //Already at max level

            int cost = data.stats.levels[level + 1].price;
            if (!player.AddGold(-cost)) return;

            level++;
            SetStats();

            ServerSend.UpgradeEntity(serverId);
        }

        public override void AddSlow(int value, float time) {
            movement.AddSlow(value, time);
        }

        public override Player GetOwner() {
            return player;
        }
    }
}