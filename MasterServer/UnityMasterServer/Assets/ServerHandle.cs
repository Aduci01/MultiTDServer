using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

class ServerHandle {
    public static void WelcomeReceived(int fromClient, Packet packet) {
        int clientIdCheck = packet.ReadInt();
        string userName = packet.ReadString();

        Debug.Log($"[CLIENT] : {Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully, NAME: {userName}");

        if (fromClient != clientIdCheck) {
            Debug.Log($"Player: {userName} _ ID: {fromClient}  wrong client id");
        }
    }

    public static void JoinRoom(int fromClient, Packet packet) {
        string id = packet.ReadString();

        foreach (Room r in NetworkManager.rooms.Values) {
            if (r.matchId == id) {
                ServerSend.SendPort(fromClient, r.port);

                return;
            }
        }

        Debug.Log($"[SERVER] : New Game Room started: {id}");

        Room newRoom = new Room(id);
        NetworkManager.rooms.Add(newRoom.port, newRoom);

        ServerSend.SendPort(fromClient, newRoom.port);
    }
}