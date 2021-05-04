using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.Server {
    public class NetworkManager : MonoBehaviour {
        public static NetworkManager _instance;

        public Player playerPrefab;
        public Player aiPrefab;

        private static int GetPort() {
            var args = System.Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; i++) {
                if (args[i].Contains("portNum")) {
                    string portString = args[i];
                    portString.Remove(0, 7);

                    return int.Parse(portString.Substring(7));
                }
            }

            return 26956;
        }

        private void Awake() {
            if (_instance == null) {
                _instance = this;

                return;
            }

            Destroy(gameObject);
        }

        private void Start() {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 20;

            Server.Start(8, GetPort());
        }

        private void OnApplicationQuit() {
            Server.Stop();
        }

        public Player InstatiatePlayer(bool isAi) {
            if (isAi) return Instantiate(aiPrefab, Vector3.zero, Quaternion.identity);

            return Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Quitting the server after 25 minutes
        /// </summary>
        float quitTimer;
        private void FixedUpdate() {
            quitTimer += Time.fixedDeltaTime;

            if (quitTimer > 60 * 35) {
                quitTimer = 0;
                Quit();
            }
        }

        public void Quit() {
            PerformanceCounter._instance.OnQuit();

            Application.Quit();
        }
    }
}