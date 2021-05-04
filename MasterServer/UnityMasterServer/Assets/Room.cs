using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class Room {
    public Process roomProcess;

    public int port;

    public string matchId;

    public Room(string id) {
        matchId = id;
        port = NetworkManager._instance.GetFreePort();

        UnityEngine.Debug.Log("Room started at " + port + " port");

        CreateRoom();
    }

    public void CreateRoom() {
        string path = "/home/ec2-user/Build/GameRoom/Build.x86_64";

        roomProcess = new Process();
        roomProcess.StartInfo.UseShellExecute = false;
        roomProcess.StartInfo.Arguments = "portNum" + port.ToString();
        roomProcess.StartInfo.RedirectStandardOutput = true;
        roomProcess.StartInfo.RedirectStandardError = true;

        roomProcess.StartInfo.FileName = path;

        roomProcess.OutputDataReceived += (sender, data) => {
            UnityEngine.Debug.Log(data.Data);

            if (data.Data.Contains("exit")) {

            }
        };

        roomProcess.ErrorDataReceived += (sender, data) => {
            UnityEngine.Debug.Log(data.Data);
        };

        try {
            roomProcess.Start();
            roomProcess.BeginOutputReadLine();
            roomProcess.BeginErrorReadLine();
        } catch (Exception ex) {
            UnityEngine.Debug.LogError(ex);
        }
    }
}
