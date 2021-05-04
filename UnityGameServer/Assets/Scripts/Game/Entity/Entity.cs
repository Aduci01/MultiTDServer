using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.Server {
    public abstract class Entity : MonoBehaviour {
        public static short entityId = Int16.MinValue;
        public short serverId;

        public int level;

        public int health, maxHealth;
        public delegate void DeathDelegates();
        public event DeathDelegates OnDeathEvent;
        public bool isDead;

        private Stats currentStats;

        protected void Init() {
            serverId = entityId++;
        }

        public void TakeDamage(int dmg, DamageType dt) {
            float modifier = 1;
            if (dt == DamageType.Magic) modifier = 1 - currentStats.magicResist / 100f;
            if (dt == DamageType.Physical) modifier = 1 - currentStats.armor / 100f;

            health -= (int)(dmg * modifier);

            if (health <= 0) {
                OnDeathEvent?.Invoke();
            }
        }

        public virtual void OnDestroyed() {
            ServerSend.EntityHp(serverId, 0);
            isDead = true;

            gameObject.SetActive(false);
        }

        public void SetCurrentStats(Stats s) {
            currentStats = s;
        }

        public abstract void SetStats();

        public abstract void TryUpgrade();

        public abstract void AddSlow(int value, float time);

        public abstract Player GetOwner();

        //public abstract void RemoveEntity();
    }
}