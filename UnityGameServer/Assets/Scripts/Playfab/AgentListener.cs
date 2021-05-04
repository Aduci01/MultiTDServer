using System.Collections;
using UnityEngine;
using PlayFab;
using System;

using System.Collections.Generic;
using PlayFab.MultiplayerAgent.Model;

public class AgentListener : MonoBehaviour {
    public static AgentListener _instance;

    public int playerNum = 1;

    private void Awake() {
        _instance = this;
    }

    private List<ConnectedPlayer> _connectedPlayers;
    public bool Debugging = true;
    // Use this for initialization
    void Start() {
        _connectedPlayers = new List<ConnectedPlayer>();
        PlayFabMultiplayerAgentAPI.Start();
        PlayFabMultiplayerAgentAPI.IsDebugging = Debugging;
        PlayFabMultiplayerAgentAPI.OnMaintenanceCallback += OnMaintenance;
        PlayFabMultiplayerAgentAPI.OnShutDownCallback += OnShutdown;
        PlayFabMultiplayerAgentAPI.OnServerActiveCallback += OnServerActive;
        PlayFabMultiplayerAgentAPI.OnAgentErrorCallback += OnAgentError;

        StartCoroutine(ReadyForPlayers());
    }

    public void SetPlayerNum() {
        IList<string> list = PlayFabMultiplayerAgentAPI.GetInitialPlayers();

        if (list != null) playerNum = list.Count;
    }

    IEnumerator ReadyForPlayers() {
        yield return new WaitForSeconds(.5f);
        PlayFabMultiplayerAgentAPI.ReadyForPlayers();
    }


    private void OnServerActive() {
        //UnityNetworkServer.Instance.StartListen();
        Debug.Log("Server Started From Agent Activation");
    }

    public void OnPlayerRemoved(string playfabId) {
        if (_connectedPlayers == null) return;

        ConnectedPlayer player = _connectedPlayers.Find(x => x.PlayerId.Equals(playfabId, StringComparison.OrdinalIgnoreCase));
        _connectedPlayers.Remove(player);
        PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_connectedPlayers);
    }

    public void OnPlayerAdded(string playfabId) {
        if (_connectedPlayers == null) return;

        _connectedPlayers.Add(new ConnectedPlayer(playfabId));
        PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_connectedPlayers);
    }

    private void OnAgentError(string error) {
        Debug.Log(error);
    }

    private void OnShutdown() {
        Debug.Log("Server is shutting down");
        //foreach (var conn in UnityNetworkServer.Instance.Connections) {
        //    conn.Connection.Send(CustomGameServerMessageTypes.ShutdownMessage, new ShutdownMessage());
        //}
        StartCoroutine(Shutdown());
    }

    IEnumerator Shutdown() {
        yield return new WaitForSeconds(5f);
        Application.Quit();
    }

    private void OnMaintenance(DateTime? NextScheduledMaintenanceUtc) {
        /*Debug.LogFormat("Maintenance scheduled for: {0}", NextScheduledMaintenanceUtc.Value.ToLongDateString());
        foreach (var conn in UnityNetworkServer.Instance.Connections) {
            conn.Connection.Send(CustomGameServerMessageTypes.ShutdownMessage, new MaintenanceMessage() {
                ScheduledMaintenanceUTC = (DateTime)NextScheduledMaintenanceUtc
            });
        }*/
    }
}