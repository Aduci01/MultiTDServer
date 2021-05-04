using System;

namespace TD.Server {
    class ServerSend {
        #region BASE
        private static void SendTCPData(short toClient, Packet packet) {
            packet.WriteLength();
            if (Server.clients[toClient].player != null && Server.clients[toClient].player.isAi) return;

            Server.clients[toClient].tcp.SendData(packet);

            PerformanceCounter._instance.AddTcpDataOut(packet.Length());
        }

        private static void SendUDPData(short toClient, Packet packet) {
            packet.WriteLength();
            Server.clients[toClient].udp.SendData(packet);
        }

        private static void SendTCPDataToAll(Packet packet) {
            packet.WriteLength();

            for (short i = 0; i < Server.MaxPlayers; i++) {
                if (Server.clients[i].player == null) continue;
                if (Server.clients[i].player.isAi) continue;

                Server.clients[i].tcp.SendData(packet);

                PerformanceCounter._instance.AddTcpDataOut(packet.Length());
            }
        }

        private static void SendTCPDataToAll(int excludedClient, Packet packet) {
            packet.WriteLength();

            for (short i = 0; i < Server.MaxPlayers; i++) {
                if (i != excludedClient)
                    Server.clients[i].tcp.SendData(packet);
            }
        }

        private static void SendUCPDataToAll(Packet packet) {
            packet.WriteLength();

            for (short i = 0; i < Server.MaxPlayers; i++) {
                Server.clients[i].udp.SendData(packet);
            }
        }

        private static void SendUCPDataToAll(int excludedClient, Packet packet) {
            packet.WriteLength();

            for (short i = 0; i < Server.MaxPlayers; i++) {
                if (i != excludedClient)
                    Server.clients[i].udp.SendData(packet);
            }
        }
        #endregion



        public static void Welcome(short toClient, string msg) {
            using (Packet packet = new Packet((int)ServerPackets.welcome)) {
                packet.Write(msg);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }

        public static void SpawnPlayer(short toClient, Player player) {
            using (Packet packet = new Packet((int)ServerPackets.spawnPlayer)) {
                packet.Write(player.id);
                packet.Write(player.username);

                packet.Write(player.raceId);

                SendTCPData(toClient, packet);
            }
        }


        public static void PlayerDisconnected(short playerId) {
            using (Packet packet = new Packet((int)ServerPackets.playerDisconnected)) {
                packet.Write(playerId);

                SendTCPDataToAll(packet);
            }
        }


        public static void Ping() {
            using (Packet packet = new Packet((int)ServerPackets.ping)) {
                SendTCPDataToAll(packet);
            }
        }

        public static void GameOver(int winnerTeam) {
            using (Packet packet = new Packet((int)ServerPackets.gameOver)) {
                packet.Write(winnerTeam);

                SendTCPDataToAll(packet);
            }
        }

        public static void SendChatMessage(short fromPlayer, string msg) {
            using (Packet packet = new Packet((int)ServerPackets.chatReceived)) {
                packet.Write(fromPlayer);
                packet.Write(msg);

                SendTCPDataToAll(packet);
            }
        }

        public static void GoldChanged(Player player) {
            using (Packet packet = new Packet((int)ServerPackets.goldChanged)) {
                packet.Write(player.goldCurrency);

                SendTCPData(player.id, packet);
            }
        }

        public static void ManaChanged(Player player) {
            using (Packet packet = new Packet((int)ServerPackets.manaChanged)) {
                packet.Write(player.manaCurrency);

                SendTCPData(player.id, packet);
            }
        }

        public static void UnitPlaced(Player player, Unit u) {
            using (Packet packet = new Packet((int)ServerPackets.unitPlaced)) {
                packet.Write(player.id);
                packet.Write(u.serverId);
                packet.Write(u.data.id);
                packet.Write(u.transform.localPosition);

                SendTCPDataToAll(packet);
            }
        }

        public static void BuildingPlaced(Player player, Building b) {
            using (Packet packet = new Packet((int)ServerPackets.buildingPlaced)) {
                packet.Write(player.id);
                packet.Write(b.serverId);
                packet.Write(b.data.id);
                packet.Write(b.transform.localPosition);

                SendTCPDataToAll(packet);
            }
        }

        public static void PreWaveTime(short time) {
            using (Packet packet = new Packet((int)ServerPackets.preWaveTime)) {
                packet.Write(time);

                SendTCPDataToAll(packet);
            }
        }

        public static void SendEnemy(Enemy enemy, Player player) {
            using (Packet packet = new Packet((int)ServerPackets.spawnEnemy)) {
                packet.Write(player.id);

                packet.Write(enemy.data.id);
                packet.Write(enemy.serverId);

                //packet.Write

                SendTCPDataToAll(packet);
            }
        }

        public static void EntityHp(short serverId, int hp) {
            using (Packet packet = new Packet((int)ServerPackets.entityHp)) {
                packet.Write(serverId);
                packet.Write(hp);

                SendTCPDataToAll(packet);
            }
        }

        public static void UpgradeEntity(short serverId) {
            using (Packet packet = new Packet((int)ServerPackets.upgradeEntity)) {
                packet.Write(serverId);

                SendTCPDataToAll(packet);
            }
        }

        public static void PlayerHp(short playerId, short hp) {
            using (Packet packet = new Packet((int)ServerPackets.playerHp)) {
                packet.Write(playerId);
                packet.Write(hp);

                SendTCPDataToAll(packet);
            }
        }

        public static void PurchaseMercenary(short playerId, string enemyId) {
            using (Packet packet = new Packet((int)ServerPackets.mercenaryPurchase)) {
                packet.Write(playerId);
                packet.Write(enemyId);

                SendTCPDataToAll(packet);
            }
        }

        public static void SellEntity(short serverId, Player player) {
            using (Packet packet = new Packet((int)ServerPackets.removeEntity)) {
                packet.Write(player.id);
                packet.Write(serverId);

                SendTCPDataToAll(packet);
            }
        }

        public static void SyncTarget(short serverId, short targetId, bool nullTarget = false) {
            using (Packet packet = new Packet((int)ServerPackets.syncTarget)) {
                packet.Write(serverId);
                packet.Write(nullTarget);
                packet.Write(targetId);

                SendTCPDataToAll(packet);
            }
        }
    }
}