using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace TD.Server {
    class Server {
        public static int MaxPlayers { get; private set; }

        public static int Port { get; private set; }
        public static Dictionary<short, Client> clients = new Dictionary<short, Client>();

        public delegate void PacketHandler(short fromClient, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        public static int GetConnectedClients() {
            int n = 0;
            foreach (Client c in clients.Values) {
                if (c.isConnected) n++;
            }

            return n;
        }

        public static void Start(int maxPlayers, int port) {
            MaxPlayers = maxPlayers;
            Port = port;

            Debug.Log("Starting Server...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptSocket(new AsyncCallback(TCPConnectionCallback), null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Debug.Log($"Server Started on {Port}.");
        }

        public static void Stop() {
            tcpListener.Stop();
            udpListener.Close();
        }

        private static void UDPReceiveCallback(IAsyncResult result) {
            try {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4) {
                    return;
                }

                using (Packet packet = new Packet(data)) {
                    short clientId = packet.ReadShort();

                    /*if (clientId == 0) {
                        return;
                    }*/

                    if (clients[clientId].udp.endPoint == null) {
                        clients[clientId].udp.Connect(clientEndPoint);
                        return;
                    }

                    if (clients[clientId].udp.endPoint.ToString().Equals(clientEndPoint.ToString())) {
                        clients[clientId].udp.HandleData(packet);
                    }
                }
            } catch (Exception ex) {
                Debug.Log($"Error receiving UDP Data {ex}");
            }
        }

        private static void TCPConnectionCallback(IAsyncResult result) {
            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectionCallback), null);

            Debug.Log($"{client.Client.RemoteEndPoint} is trying to connect...");


            for (short i = 0; i < MaxPlayers; i++) {
                if (clients[i].player == null) {

                    clients[i].tcp.Connect(client);
                    return;
                }
            }

            Debug.Log($"{client.Client.RemoteEndPoint} failed to connect: Server Full");
        }

        public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet) {
            try {
                if (clientEndPoint != null) {
                    udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            } catch (Exception ex) {
                Debug.Log($"Error sending data to {clientEndPoint} via UDP: {ex}");
            }
        }

        private static void InitializeServerData() {
            for (short i = 0; i < MaxPlayers; i++) {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>() {
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                {(int)ClientPackets.chatMessage, ServerHandle.ChatMessage },

                {(int)ClientPackets.buildingPlacementRequest, ServerHandle.BuildingPlacementRequest },
                {(int)ClientPackets.unitPlacementRequest, ServerHandle.UnitPlacementRequest },
                {(int)ClientPackets.sellEntityRequest, ServerHandle.SellEntityRequest },

                {(int)ClientPackets.upgradeRequest, ServerHandle.UpgradeRequest },

                {(int)ClientPackets.mercenaryRequest, ServerHandle.MercenaryRequest },
                {(int)ClientPackets.cauldronEvent, ServerHandle.CauldronEvent },

            };
        }
    }
}