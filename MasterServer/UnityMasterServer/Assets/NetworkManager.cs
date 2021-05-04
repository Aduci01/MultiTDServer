using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
    public static NetworkManager _instance;

    public static Dictionary<int, Room> rooms = new Dictionary<int, Room>();

    private int Port = 26955;

    private void Awake() {
        if (_instance == null) {
            _instance = this;

            return;
        }

        Destroy(gameObject);
    }

    private void Start() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 10;

        Server.Start(10000, Port);
    }

    private void OnApplicationQuit() {
        Server.Stop();

        foreach (Room r in rooms.Values) {
            r.roomProcess.CloseMainWindow();
            r.roomProcess.Close();
        }
    }


    float timer = 0;
    private void Update() {
        timer += Time.deltaTime;

        if (timer > 5f) { //Checking in every 5 seconds if a room was closed
            foreach (Room r in rooms.Values) {
                if (r.roomProcess.HasExited) {
                    rooms.Remove(r.port);
                }
            }
        }
    }

    public int GetFreePort() {
        int i = Port + 1;

        while (rooms.ContainsKey(i)) {
            i++;
        }

        return i;
    }
}
