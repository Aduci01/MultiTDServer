using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.Server {
    public class ShootBehaviour : MonoBehaviour {
        public Entity target;

        [Header("General")]
        public Stats stats;
        public DamageType damageType;

        private float atkCountdown = 0f;

        public bool isShooting;

        void FixedUpdate() {
            if (target == null || stats.range < Vector3.Distance(target.transform.position, transform.position)) {
                isShooting = false;
                return;
            }

            isShooting = true;

            if (atkCountdown <= 0f) {
                Shoot();

                atkCountdown = stats.atkSpeed;
            }

            atkCountdown -= Time.fixedDeltaTime;
        }

        void Shoot() {
            if (stats.splashRadius > 0) {
                DoSplashDamage();
            } else {
                target.TakeDamage(stats.damage, damageType);
            }

            if (stats.slowMovement != 0) {
                target.AddSlow(stats.slowMovement, 1.5f);
            }
        }

        void DoSplashDamage() {
            Collider[] hitColliders = Physics.OverlapSphere(target.transform.position, stats.splashRadius);
            foreach (var hitCollider in hitColliders) {
                Entity e = hitCollider.GetComponent<Entity>();

                if (e != null)
                    e.TakeDamage(stats.damage, damageType);
            }
        }
    }
}