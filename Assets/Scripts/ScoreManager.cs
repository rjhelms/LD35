﻿using UnityEngine;
using System.Collections;

public class ScoreManager : Singleton<ScoreManager>
{
    protected ScoreManager() { }

    public int HitPoints = 100;
    public int CurrentLevel = 0;
    public int MaxLevels = 2;
    public int ActivePartCount = 0;
    public int MaxPartCount = 1;
    public bool[] SensorsAvailable = new bool[] { true, false, false, false };
    public bool[] ChassisAvailable = new bool[] { true, false, false, false };
    public bool[] ToolsAvailable = new bool[] { true, false, false, false };
    public SensorState sensorState;
    public ChassisState chassisState;
    public ToolState toolState;
}
