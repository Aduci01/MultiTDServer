using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.Profiling;
using System;

public class PerformanceCounter : MonoBehaviour {
    public static PerformanceCounter _instance;
    private void Awake() {
        _instance = this;
    }

    int tick;

    long sum;
    int count;
    long maxMemory = 0;

    int tcpDataIn = 0; //in bytes
    int tcpDataOut = 0; //in bytes

    private void FixedUpdate() {
        tick++;

        if (tick > 40) {
            tick = 0;
            Log();
        }
    }

    void Log() {
        long l = Profiler.GetTotalAllocatedMemoryLong() / 1000000;
        sum += l;
        count++;

        if (maxMemory < l) {
            maxMemory = l;
        }
    }

    public void OnQuit() {
        Debug.Log("Average: " + sum / count + "Mb");
        Debug.Log("Max: " + maxMemory + "Mb");
        Debug.Log("Tcp Data In: " + tcpDataIn + "byte");
        Debug.Log("Tcp Data Out: " + tcpDataIn + "byte");
    }

    public void AddTcpData(int length) {
        tcpDataIn += length;
    }

    public void AddTcpDataOut(int length) {
        tcpDataOut += length;
    }
}
