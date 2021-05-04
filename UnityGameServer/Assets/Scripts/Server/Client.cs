using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace TD.Server {

    class Client {
        public static int dataBufferSize = 4096;

        public short id;
        public Player player;
        public bool isConnected;

        public TCP tcp;
        public UDP udp;


        public Client(short clientId) {
            id = clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP {
            public TcpClient socket;

            private readonly short id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(short _id) {
                id = _id;
            }

            public void Connect(TcpClient _socket) {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;


                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Successfully connected to the Server");
            }

            public void SendData(Packet packet) {
                try {
                    if (socket != null) {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                } catch (Exception ex) {
                    Debug.Log($"Error sending data to player {id} via TCP: {ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult result) {
                try {
                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0) {
                        Server.clients[id].Disconnect();

                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivedData.Reset(HandleData(data));


                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                } catch (Exception _ex) {
                    Server.clients[id].Disconnect();

                    Debug.Log(_ex);
                }
            }

            private bool HandleData(byte[] data) {
                int packetLength = 0;

                receivedData.SetBytes(data);

                if (receivedData.UnreadLength() >= 4) {
                    packetLength = receivedData.ReadInt();

                    if (packetLength <= 0) {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= receivedData.UnreadLength()) {
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);

                    ThreadManager.ExecuteOnMainThread(() => {
                        using (Packet packet = new Packet(packetBytes)) {
                            int packetId = packet.ReadInt();
                            Server.packetHandlers[packetId](id, packet);

                            PerformanceCounter._instance.AddTcpData(packet.Length());
                        }
                    });


                    packetLength = 0;
                    if (receivedData.UnreadLength() >= 4) {
                        packetLength = receivedData.ReadInt();

                        if (packetLength <= 0) {
                            return true;
                        }
                    }
                }


                if (packetLength <= 1) {
                    return true;
                }

                return false;
            }

            public void Disconnect() {
                socket.Close();

                stream = null;
                receivedData = null;
                receiveBuffer = null;

                socket = null;
            }
        }

        public class UDP {
            public IPEndPoint endPoint;

            private short id;

            public UDP(short _id) {
                id = _id;
            }

            public void Connect(IPEndPoint _endPoint) {
                endPoint = _endPoint;
            }

            public void SendData(Packet _packet) {
                Server.SendUDPData(endPoint, _packet);
            }

            public void HandleData(Packet packetData) {
                int packetLength = packetData.ReadInt();
                byte[] packetBytes = packetData.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet packet = new Packet(packetBytes)) {
                        short packetId = packet.ReadShort();
                        Server.packetHandlers[packetId](id, packet);
                    }
                });
            }

            public void Disconnect() {
                endPoint = null;
            }
        }

        public void SendIntoGame(string playerName, string raceId, string pfId, bool isAi = false) {
            player = NetworkManager._instance.InstatiatePlayer(isAi);
            player.transform.position = Vector3.right * 30 * id;

            player.Initialize(id, playerName, raceId, pfId);
            player.isAi = isAi;

            foreach (Client client in Server.clients.Values) {
                if (client.player != null) {
                    if (client.id != id) {
                        ServerSend.SpawnPlayer(id, client.player);
                    }
                }
            }

            foreach (Client client in Server.clients.Values) {
                if (client.player != null) {
                    ServerSend.SpawnPlayer(client.id, player);
                }
            }

            if (AgentListener._instance != null && !isAi)
                AgentListener._instance.OnPlayerAdded(pfId);

            isConnected = true;
        }

        public void Disconnect() {
            Debug.Log($"Disconnected: {player.username}");

            if (AgentListener._instance != null)
                AgentListener._instance.OnPlayerRemoved(player.playfabId);

            isConnected = false;
            tcp.Disconnect();
            udp.Disconnect();

            if (GameManager._instance.state != GameManager.GameState.WaitingForPlayers) {
                if (Server.GetConnectedClients() == 0)
                    NetworkManager._instance.Quit();
            }

            //ServerSend.PlayerDisconnected(id);
        }
    }
}