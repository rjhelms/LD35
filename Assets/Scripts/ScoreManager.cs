using UnityEngine;
using System.Collections;

public class ScoreManager : Singleton<ScoreManager>
{
    protected ScoreManager() { }

    public int HitPoints = 100;
    public bool[] SensorsAvailable = new bool[] { true, false, false, false };
    public bool[] ChassisAvailable = new bool[] { true, false, false, false };
    public bool[] ToolsAvailalbe = new bool[] { true, false, false, false };
}
