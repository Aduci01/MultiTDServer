using System;
using System.Collections.Generic;
using UnityEngine;

namespace TD.Server {
    class ServerHandle {
        public static void WelcomeReceived(short fromClient, Packet packet) {
            short clientIdCheck = packet.ReadShort();
            string userName = packet.ReadString();

            Debug.Log($"[CLIENT] : {Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully, NAME: {userName}");

            if (fromClient != clientIdCheck) {
                Debug.Log($"Player: {userName} _ ID: {fromClient}  wrong client id");
            }

            string raceId = packet.ReadString();

            string playfabId = packet.ReadString();

            Server.clients[fromClient].SendIntoGame(userName, raceId, playfabId);

            if (!GameManager._instance.isGameStarted) {
                GameManager._instance.StartWaitingForPlayers(12);
            }
        }

        public static void ChatMessage(short fromClient, Packet packet) {
            ServerSend.SendChatMessage(fromClient, packet.ReadString());
        }

        public static void UnitPlacementRequest(short fromClient, Packet packet) {
            string id = packet.ReadString();
            Vector3 pos = packet.ReadVector3();

            UnitData ud = DataCollection.unitDb[id];

            Server.clients[fromClient].player.PlaceUnitRequest(ud, pos);
        }

        public static void BuildingPlacementRequest(short fromClient, Packet packet) {
            string id = packet.ReadString();
            Vector3 pos = packet.ReadVector3();

            BuildingData bd = DataCollection.buildingDb[id];

            Server.clients[fromClient].player.PlaceBuildingRequest(bd, pos);
        }

        public static void UpgradeRequest(short fromClient, Packet packet) {
            Player player = Server.clients[fromClient].player;
            player.TryUpgrade(packet.ReadShort());
        }

        public static void MercenaryRequest(short fromClient, Packet packet) {
            EnemyData ed = DataCollection.enemyDb[packet.ReadString()];
            Player player = Server.clients[fromClient].player;

            player.TryToBuyMercenary(ed);
        }

        public static void CauldronEvent(short fromClient, Packet packet) {
            Player player = Server.clients[fromClient].player;

            Entity e = player.SearchForEntity(packet.ReadShort());
            if (e != null) {
                Cauldron c = e.GetComponent<Cauldron>();
                if (c == null) return;

                c.Convert(packet.ReadInt());
            }
        }

        public static void SellEntityRequest(short fromClient, Packet packet) {
            Player player = Server.clients[fromClient].player;
            player.TrySellEntity(packet.ReadShort());
        }
    }
}