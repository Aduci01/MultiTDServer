using System.Collections;
using System.Collections.Generic;
using PlayFab.ClientModels;
using UnityEngine;

namespace TD.Server {
    [System.Serializable]
    public class EnemyData {
        public string displayName;
        public string description;

        public string id;

        public EnemyJson stats;


        public EnemyData(string id, CatalogItem ci) {
            this.id = id;

            displayName = ci.DisplayName;
            description = ci.Description;

            stats = JsonUtility.FromJson<EnemyJson>(ci.CustomData);
        }
    }

    public class EnemyJson {
        public bool isPurchasable;
        public int hpConsume = 1;

        public DamageType damageType = DamageType.Physical;
        public Stats[] levels;
    }
}