using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.Server {
    public class Building : Entity {
        public BuildingData data;


        [HideInInspector] public Player player;

        public ShootBehaviour shooter;
        public HealBehaviour healer;
        public SummonBehaviour summoner;

        public void Init(BuildingData bd, Player p) {
            Init();
            OnDeathEvent += OnDestroyed;

            data = bd;
            player = p;

            shooter.damageType = data.stats.damageType;
            SetStats();

            if (data.id == "Elves_Cauldron") {
                gameObject.AddComponent<Cauldron>();
            }
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

            if (summoner != null) {
                summoner.Init(stats, this, false);
            }

            maxHealth = health = stats.hp;


            //Setting income modifiers
            player.goldIncomePerWave += stats.goldIncome;
            player.manaIncomePerWave += stats.manaIncome;
            if (level > 0) {
                player.goldIncomePerWave -= data.stats.levels[level - 1].goldIncome;
                player.manaIncomePerWave -= data.stats.levels[level - 1].manaIncome;
            }

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
                shooter.target = nearestT;
            } else {
                shooter.target = null;
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

        }

        public override Player GetOwner() {
            return player;
        }
    }
}