using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.Server {
    public class GameManager : MonoBehaviour {
        public static GameManager _instance;

        public enum GameState { WaitingForPlayers, InWave, PreWave }
        public GameState state;

        public float timer;
        public int currentWave;

        public bool isGameStarted;

        private void Awake() {
            _instance = this;
        }

        public void InitAis() {
            AgentListener._instance.SetPlayerNum();

            for (int i = 0; i < 8 - AgentListener._instance.playerNum; i++) {
                AddAiPlayer();
            }
        }

        public void StartWaitingForPlayers(int t) {
            isGameStarted = true;
            InitAis();

            StartCoroutine(WaitingForPlayersCoroutine(t));
        }

        IEnumerator WaitingForPlayersCoroutine(int t) {
            yield return new WaitForSeconds(t);


            StartPreGame(60);
        }

        public void StartPreGame(int t) {
            currentWave++;
            NewRound();

            timer = t;
            state = GameState.PreWave;

            StartCoroutine(PreWave());
        }

        IEnumerator PreWave() {
            while (timer >= 0) {
                yield return new WaitForSeconds(1f);

                timer -= 1;
                ServerSend.PreWaveTime((short)timer);
            }

            WaveManager._instance.StartWave(currentWave);
            state = GameState.InWave;
        }

        void NewRound() {
            foreach (Client c in Server.clients.Values) {
                if (c.player == null) continue;

                c.player.NewRound();

                c.player.AddGold(c.player.goldIncomePerWave, true);
                c.player.AddMana(c.player.manaIncomePerWave, true);
            }
        }

        public void CheckGameOver() {
            int n = 0;

            foreach (Client c in Server.clients.Values) {
                if (c.player == null) continue;

                if (c.player.health > 0) n++;
            }

            if (n <= 1) {
                NetworkManager._instance.Quit();
            }
        }

        public void AddAiPlayer() {
            foreach (Client c in Server.clients.Values) {
                if (c.player != null) continue;

                c.SendIntoGame(GetRandomAiName(), GetRandomRace(), "", true);

                break;
            }
        }

        public string GetRandomRace() {
            List<string> list = new List<string>();
            foreach (string s in DataCollection.raceDb.Keys) {
                list.Add(s);
            }

            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static string[] usernameStrings = { "Carl", "DareD33vil", "Sherlock", "Tyranner", "asdasd", "majom", "gamervil", "Vippo", "Lida", "Tresh55", "Hrgxh", "EmperorJax", "rtx", "Clement", "IBeatYa", "lovboy", "Martha", "Daddy", "kitkat", "vacuumCleaner", "D3velop3r", "poppy", "404", "Weed4Life", "Tigerrr", "ForTheHorde", "lol", "Monster", "lusty", "Sue1", "Misi", "Player24", "Player1", "Mr.Smith", "+-+", "I <3 You", "DckFace", "Deku", "Musketiir", "voldi", "lukeskyW", "12345", "Dragasag", "Jack the Heck" };
        public string GetRandomAiName() {
            return usernameStrings[UnityEngine.Random.Range(0, usernameStrings.Length)];
        }
    }
}