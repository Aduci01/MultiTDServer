using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class PlayFabScript : MonoBehaviour {
    public static PlayFabScript _instance;

    private void Awake() {
        _instance = this;
    }

    public void UpdateTrophies(string id, int val, bool isWinner) {
        PlayFabServerAPI.UpdatePlayerStatistics(new PlayFab.ServerModels.UpdatePlayerStatisticsRequest {
            // request.Statistics is a list, so multiple StatisticUpdate objects can be defined if required.
            Statistics = new List<PlayFab.ServerModels.StatisticUpdate> {
                new PlayFab.ServerModels.StatisticUpdate { StatisticName = "Ranked", Value = val },
            },

            PlayFabId = id
        },
        result => { Debug.Log("User statistics updated"); },
        error => { Debug.LogError(error.GenerateErrorReport()); });

        UpdateMatchData(id, val, isWinner);
    }

    public void UpdateBattleRoadWin(string id) {
        PlayFabServerAPI.GetUserData(new PlayFab.ServerModels.GetUserDataRequest {

        },
        result => {
            var jsonData = JsonUtility.FromJson<BattleRoadJson>(result.Data["battleRoad"].Value);
            jsonData.winCount++;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("battleRoad", JsonUtility.ToJson(jsonData));

            PlayFabServerAPI.UpdateUserData(new PlayFab.ServerModels.UpdateUserDataRequest {
                PlayFabId = id,
                Data = data,
                Permission = PlayFab.ServerModels.UserDataPermission.Public
            },
            result2 => {
                //Debug.Log("Successfully updated BR");
            },
            error2 => {
                Debug.LogError(error2.GenerateErrorReport());
            });
        },
        error => {
            Debug.LogError(error.GenerateErrorReport());
        });
    }

    public void UpdateMatchData(string id, int trophies, bool isWinner) {
        string reward = "";
        if (isWinner)
            reward = GetRandomReward();

        MatchDataJson md = new MatchDataJson();
        md.isWinner = isWinner;
        md.winReward = reward;
        md.gainedTrophies = trophies;

        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("matchData", JsonUtility.ToJson(md));

        PlayFabServerAPI.UpdateUserReadOnlyData(new PlayFab.ServerModels.UpdateUserDataRequest {
            PlayFabId = id,
            Data = data

        },
        result => { Debug.Log("User statistics updated"); },
        error => { Debug.LogError(error.GenerateErrorReport()); });
    }

    private string GetRandomReward() {
        int r = Random.Range(0, 100);

        if (r < 10) return "gold_chest";
        if (r < 30) return "silver_chest";
        if (r < 55) return "bronze_chest";

        return "";
    }
}

public class BattleRoadJson {
    public int winCount = 0;
    public string resetDate;
    public bool reward1 = false, reward2 = false, reward3 = false;
}

public class MatchDataJson {
    public bool isWinner;
    public string winReward;
    public int gainedTrophies;
}
