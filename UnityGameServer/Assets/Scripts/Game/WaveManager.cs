using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace TD.Server {
    public class WaveManager : MonoBehaviour {
        public static WaveManager _instance;

        bool isWaveSpawning = false;
        float waveEndCheckTimer;

        public WaveParent waves;

        public Enemy enemyPrefab;

        private void Awake() {
            _instance = this;
        }

        void FixedUpdate() {
            if (isWaveSpawning || GameManager._instance.state != GameManager.GameState.InWave) return;

            waveEndCheckTimer += Time.fixedDeltaTime;

            if (waveEndCheckTimer > 1) {
                waveEndCheckTimer = 0;

                CheckWaveOver();
            }
        }

        public void StartWave(int currentWaveId) {
            isWaveSpawning = true;
            waveEndCheckTimer = 0;

            StartCoroutine(SpawnCoroutine(waves.waves[currentWaveId % waves.waves.Length]));
        }

        IEnumerator SpawnCoroutine(WaveData currenWave) {
            //Spawning base wave
            foreach (WaveType w in currenWave.wavePart) {
                for (int i = 0; i < w.amount; i++) {
                    foreach (Client c in Server.clients.Values) {
                        if (c.player == null || c.player.isDead) continue;

                        SpawnEnemy(w.enemyType, c.player);
                    }

                    yield return new WaitForSeconds(0.8f);
                }
            }


            //Spawning mercenaries
            for (int i = 0; i < Player.maxMercenaryLimitPerRound; i++) {
                foreach (Client c in Server.clients.Values) {
                    if (c.player == null) continue;

                    Player p = c.player.GetPlayerMercenaryFrom();

                    if (p.mercenaries.Count == 0) continue;

                    List<EnemyData> edList = p.mercenaries;
                    SpawnEnemy(edList[0].id, c.player);

                    p.mercenaries.RemoveAt(0);
                }

                yield return new WaitForSeconds(0.7f);
            }

            isWaveSpawning = false;
        }

        void CheckWaveOver() {
            foreach (Client c in Server.clients.Values) {
                if (c.player == null) continue;

                if (c.player.enemies.Count > 0) return;
            }

            GameManager._instance.StartPreGame(60);
        }

        public void SpawnEnemy(string enemy, Player player) {
            var e = Instantiate(enemyPrefab, player.transform);
            e.Init(DataCollection.enemyDb[enemy], player);

            StartCoroutine(DelayedActivation(e));

            player.SpawnEnemy(e);


            ServerSend.SendEnemy(e, player);
        }

        /// <summary>
        /// To decrease latency on Client
        /// </summary>
        /// <param name="enemy"></param>
        /// <returns></returns>
        IEnumerator DelayedActivation(Enemy enemy) {
            enemy.gameObject.SetActive(false);
            enemy.isDead = true;

            yield return new WaitForSeconds(0.8f);

            enemy.gameObject.SetActive(true);
            enemy.isDead = false;
        }

        void SpawnEnemyToAll(string enemy) {
            /*Enemy firstEnemy = null;
            foreach (Client c in Server.clients.Values) {
                if (c.player == null) continue;

                var e = Instantiate(enemyPrefab, c.player.transform);
                e.Init(DataCollection.enemyDb[enemy]);

                c.player.SpawnEnemy(e);

                if (firstEnemy == null)
                    firstEnemy = e;
            }

            ServerSend.SendEnemy(firstEnemy);*/
        }
    }
}