using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.Server {
    public class SummonBehaviour : MonoBehaviour {
        Stats stats;
        Entity entity;

        bool isEnemy;

        float timer;

        public void Init(Stats stats, Entity entity, bool isEnemy) {
            this.stats = stats;
            this.entity = entity;
            this.isEnemy = isEnemy;

            if (stats.summonRate == 0) this.enabled = false;
            else {
                this.enabled = true;

                stats.summonRate *= Random.Range(0.9f, 1.1f);
            }
        }

        void FixedUpdate() {
            if (GameManager._instance.state != GameManager.GameState.InWave) return;

            timer += Time.fixedDeltaTime;

            if (timer > stats.summonRate) {
                timer = 0;

                Summon();
            }
        }

        void Summon() {
            Player player = entity.GetOwner();

            if (isEnemy) {
                WaveManager._instance.SpawnEnemy(stats.summonId, player);
            } else {
                player.Summon(DataCollection.unitDb[stats.summonId], GetPos());
            }
        }

        Vector3 GetPos() {
            Vector3 vec;
            int n = 0;

            do {
                n++;

                vec = Random.insideUnitSphere * 2.0f + entity.transform.position;
                vec = new Vector3(vec.x, 0.5f, vec.z);

            } while (Physics.OverlapSphere(vec, 0.3f).Length <= 1 && n < 100);

            return vec;
        }
    }
}