
class ServerSend {
    #region BASE
    private static void SendTCPData(int toClient, Packet packet) {
        packet.WriteLength();
        Server.clients[toClient].tcp.SendData(packet);
    }

    private static void SendUDPData(int toClient, Packet packet) {
        packet.WriteLength();
        Server.clients[toClient].udp.SendData(packet);
    }

    private static void SendTCPDataToAll(Packet packet) {
        packet.WriteLength();

        for (int i = 0; i < Server.MaxPlayers; i++) {
            Server.clients[i].tcp.SendData(packet);
        }
    }

    private static void SendTCPDataToAll(int excludedClient, Packet packet) {
        packet.WriteLength();

        for (int i = 0; i < Server.MaxPlayers; i++) {
            if (i != excludedClient)
                Server.clients[i].tcp.SendData(packet);
        }
    }

    private static void SendUCPDataToAll(Packet packet) {
        packet.WriteLength();

        for (int i = 0; i < Server.MaxPlayers; i++) {
            Server.clients[i].udp.SendData(packet);
        }
    }

    private static void SendUCPDataToAll(int excludedClient, Packet packet) {
        packet.WriteLength();

        for (int i = 0; i < Server.MaxPlayers; i++) {
            if (i != excludedClient)
                Server.clients[i].udp.SendData(packet);
        }
    }
    #endregion



    public static void Welcome(int toClient, string msg) {
        using (Packet packet = new Packet((int)ServerPackets.welcome)) {
            packet.Write(msg);
            packet.Write(toClient);

            SendTCPData(toClient, packet);
        }
    }

    public static void SendPort(int toClient, int port) {
        using (Packet packet = new Packet((int)ServerPackets.roomPort)) {
            packet.Write(port);
            SendTCPData(toClient, packet);
        }
    }

}
