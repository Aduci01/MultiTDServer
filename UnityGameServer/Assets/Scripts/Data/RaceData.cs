using System.Collections;
using System.Collections.Generic;
using PlayFab.ClientModels;
using UnityEngine;

namespace TD.Server {
    public class RaceData {
        public string displayName;
        public string description;

        public string id;

        public RaceJson stats;

        public List<BuildingData> buildings;
        public List<UnitData> units;

        public RaceData(string id, CatalogItem ci) {
            this.id = id;

            buildings = new List<BuildingData>();
            units = new List<UnitData>();

            displayName = ci.DisplayName;
            description = ci.Description;

            stats = JsonUtility.FromJson<RaceJson>(ci.CustomData);
        }
    }

    public class RaceJson {

    }
}