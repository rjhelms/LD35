using UnityEngine;
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
    public SensorState sensorState = SensorState.BASIC;
    public ChassisState chassisState = ChassisState.BASIC;
    public ToolState toolState = ToolState.NONE;

    public void Reset()
    {
        HitPoints = 100;
        CurrentLevel = 0;
        ActivePartCount = 0;
        MaxPartCount = 1;
        sensorState = SensorState.BASIC;
        chassisState = ChassisState.BASIC;
        toolState = ToolState.NONE;
        SensorsAvailable = new bool[] { true, false, false, false };
        ChassisAvailable = new bool[] { true, false, false, false };
        ToolsAvailable = new bool[] { true, false, false, false };
    }
}
