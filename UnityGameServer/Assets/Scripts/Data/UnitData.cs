using System.Collections;
using System.Collections.Generic;
using PlayFab.ClientModels;
using UnityEngine;

namespace TD.Server {
    [System.Serializable]
    public class UnitData {
        public string displayName;
        public string description;

        public string id;

        public BuildingJson stats;


        public UnitData(string id, CatalogItem ci) {
            this.id = id;

            displayName = ci.DisplayName;
            description = ci.Description;

            stats = JsonUtility.FromJson<BuildingJson>(ci.CustomData);
        }
    }

    public class UnitJson {
        public string raceId;

        public DamageType damageType = DamageType.Physical;
        public Stats[] levels;
    }
}