using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.Server {
    public class HealBehaviour : MonoBehaviour {
        public Player pm;

        public int healAmount;
        public float healRate;
        float healTimer;

        Entity target;

        // Start is called before the first frame update
        void Start() {
            if (healAmount <= 0) {
                return;
            }

            InvokeRepeating("UpdateTarget", 0f, 1f);
        }

        void UpdateTarget() {
            Entity lowestHpEntity = null;
            float hpRate = 1;

            foreach (Building b in pm.buildings) {
                if (b.isDead) continue;

                float percent = b.health / b.maxHealth;
                if (percent < hpRate) {
                    hpRate = percent;
                    lowestHpEntity = b;
                }
            }

            foreach (Unit u in pm.units) {
                if (u.isDead) continue;

                float percent = u.health / u.maxHealth;
                if (percent < hpRate) {
                    hpRate = percent;
                    lowestHpEntity = u;
                }
            }

            target = lowestHpEntity;
        }

        void FixedUpdate() {
            if (target == null) return;

            if (healTimer <= 0f) {
                Heal();

                healTimer = healRate;
            }

            healTimer -= Time.deltaTime;
        }

        void Heal() {
            target.TakeDamage(-healAmount, DamageType.Pure);
        }
    }
}
