using System;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System.Text;
using System.Collections;

//using GooglePlayGames;
//using GooglePlayGames.BasicApi;

namespace TD.Server {
    /// <summary>
    /// Handle login window events
    /// </summary>
    public class PlayfabLogin : MonoBehaviour {
        public static GetPlayerCombinedInfoRequestParams loginInfoParams =
            new GetPlayerCombinedInfoRequestParams {
                GetUserAccountInfo = false,
                GetUserData = false,
                GetUserInventory = false,
                GetUserVirtualCurrency = false,
                GetUserReadOnlyData = false,
            };

        public void Start() {
            DeviceIDLogin();
        }

        void DeviceIDLogin() {
            var request = new LoginWithCustomIDRequest {
                CustomId = "ADMIN",
                CreateAccount = true,
                InfoRequestParameters = loginInfoParams,
            };
            PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
        }


        private void OnLoginSuccess(LoginResult result) {
            Debug.Log("Login Succes!");

            DataCollection.GetData();
        }

        private void OnLoginFailure(PlayFabError obj) {
            throw new NotImplementedException();
        }
    }
}