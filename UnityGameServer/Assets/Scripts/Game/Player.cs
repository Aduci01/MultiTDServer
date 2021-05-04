using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.Server {
    public class Player : MonoBehaviour {
        [Header("Player Infos")]
        public short id;
        public string playfabId;
        public string username;
        public bool isAi = false;

        public string raceId;

        public Transform castleTransform;

        public bool isDead;

        [Space(15)]
        [Header("Stats")]
        public RaceData raceData;
        public short health;

        public int goldCurrency, manaCurrency;
        public int goldIncomePerWave, manaIncomePerWave;

        protected virtual void Start() {
            AddGold(300);
            AddMana(50);

            health = 100;
        }

        public void Initialize(short id, string username, string raceId, string pfId) {
            this.id = id;
            this.username = username;
            this.raceId = raceId;

            playfabId = pfId;

            raceData = DataCollection.raceDb[raceId];
        }

        public bool AddGold(int n, bool force = false) {
            if (!force)
                if (goldCurrency + n < 0) return false;

            goldCurrency += n;
            goldCurrency = Mathf.Max(goldCurrency, 0);

            ServerSend.GoldChanged(this);

            return true;
        }

        public bool AddMana(int n, bool force = false) {
            if (!force)
                if (manaCurrency + n < 0) return false;

            manaCurrency += n;
            manaCurrency = Mathf.Max(manaCurrency, 0);

            ServerSend.ManaChanged(this);

            return true;
        }


        public void AddHp(short n) {
            if (health <= 0) return;

            health += n;

            if (health <= 0) {
                //Player is dead
                isDead = true;
                health = 0;

                GameManager._instance.CheckGameOver();
            }

            ServerSend.PlayerHp(id, health);
        }

        /// <summary>
        /// Called after each wave
        /// </summary>
        public void NewRound() {
            if (isDead) return;

            ResetEntities();

            AddGold(goldIncomePerWave);
            AddMana(manaIncomePerWave);

            enemies = new List<Enemy>();
            mercenaries = new List<EnemyData>();
        }

        #region Placement & Entities
        [Space(15)]
        [Header("Placement")]
        public Building buildingPrefab;
        public Unit unitPrefab;

        public List<Building> buildings;
        public List<Unit> units;

        public void Summon(UnitData ud, Vector3 pos) {
            var u = Instantiate(unitPrefab, transform);
            u.transform.localPosition = pos;
            u.isSummon = true;

            u.Init(ud, this);
            units.Add(u);

            ServerSend.UnitPlaced(this, u);
        }

        public void PlaceUnitRequest(UnitData ud, Vector3 pos) {
            if (GameManager._instance.state != GameManager.GameState.PreWave) return;

            int cost = ud.stats.levels[0].price;

            if (!AddGold(-cost)) return;

            var u = Instantiate(unitPrefab, transform);
            u.transform.localPosition = pos;

            u.Init(ud, this);
            units.Add(u);

            ServerSend.UnitPlaced(this, u);
        }

        public void PlaceBuildingRequest(BuildingData bd, Vector3 pos) {
            if (GameManager._instance.state != GameManager.GameState.PreWave) return;

            int cost = bd.stats.levels[0].price;

            if (!AddGold(-cost)) return;

            var b = Instantiate(buildingPrefab, transform);
            b.transform.localPosition = pos;

            b.Init(bd, this);
            buildings.Add(b);

            ServerSend.BuildingPlaced(this, b);
        }

        public void TrySellEntity(short serverId) {
            if (GameManager._instance.state != GameManager.GameState.PreWave) return;

            Entity e = SearchForEntity(serverId);
            if (e == null) return;

            if (e.GetComponent<Unit>() != null) {
                Unit u = e.GetComponent<Unit>();
                units.Remove(u);
                AddGold(u.data.stats.levels[u.level].price / 2);

                goldIncomePerWave -= u.data.stats.levels[u.level].goldIncome;
                manaIncomePerWave -= u.data.stats.levels[u.level].manaIncome;

                Destroy(u.gameObject);
            }

            if (e.GetComponent<Building>() != null) {
                Building b = e.GetComponent<Building>();
                buildings.Remove(b);
                AddGold(b.data.stats.levels[b.level].price / 2);

                goldIncomePerWave -= b.data.stats.levels[b.level].goldIncome;
                manaIncomePerWave -= b.data.stats.levels[b.level].manaIncome;

                Destroy(b.gameObject);
            }

            ServerSend.SellEntity(e.serverId, this);
        }

        public void TryUpgrade(int serverId) {
            Entity e = SearchForEntity(serverId);

            if (e.GetComponent<Unit>() != null)
                e.GetComponent<Unit>().TryUpgrade();

            if (e.GetComponent<Building>() != null)
                e.GetComponent<Building>().TryUpgrade();
        }

        public Entity SearchForEntity(int serverId) {
            foreach (Building b in buildings) {
                if (b.serverId == serverId) return b;
            }

            foreach (Unit u in units) {
                if (u.serverId == serverId) return u;
            }

            return null;
        }

        void ResetEntities() {
            foreach (Building b in buildings) {
                b.health = b.maxHealth;
                b.isDead = false;

                b.gameObject.SetActive(true);
            }

            foreach (Unit u in units) {
                if (u.isSummon) {
                    ServerSend.SellEntity(u.serverId, this);
                    Destroy(u.gameObject);

                    continue;
                }

                u.health = u.maxHealth;
                u.isDead = false;

                u.transform.position = u.startPos;

                u.gameObject.SetActive(true);
            }

            units.RemoveAll(x => x.isSummon);
        }

        #endregion

        #region Enemy Handling
        [Space(15)]
        [Header("Enemies")]
        public List<Enemy> enemies;

        public Transform[] spawnTransforms;
        int spawnId;

        public void SpawnEnemy(Enemy e) {
            enemies.Add(e);
            e.transform.position = spawnTransforms[spawnId++ % spawnTransforms.Length].position;
        }

        #endregion


        #region Mercenaries
        public static int maxMercenaryLimitPerRound = 10;
        public List<EnemyData> mercenaries; //List of mercenaries bought by this player (Will be sent next turn)

        /// <summary>
        /// Trying to buy mercenary. In case not enough mana, it returns
        /// </summary>
        /// <param name="ed"></param>
        public void TryToBuyMercenary(EnemyData ed) {
            if (isDead) return;

            if (GameManager._instance.state != GameManager.GameState.PreWave || maxMercenaryLimitPerRound <= mercenaries.Count) return;

            int cost = ed.stats.levels[0].price;
            if (!AddMana(-cost)) return;

            goldIncomePerWave += ed.stats.levels[0].goldIncome;

            mercenaries.Add(ed);
            ServerSend.PurchaseMercenary(id, ed.id);
        }

        /// <summary>
        /// Returns the player which sends mercenaries to this player
        /// </summary>
        /// <returns></returns>
        public Player GetPlayerMercenaryFrom() {
            short i = (short)(id + 1);
            while (Server.clients[(short)(i % Server.clients.Count)].player == null || Server.clients[(short)(i % Server.clients.Count)].player.isDead) {
                i++;
                if (i == id) return null;
            }


            return Server.clients[(short)(i % Server.clients.Count)].player;
        }

        #endregion
    }
}