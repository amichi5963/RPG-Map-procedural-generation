using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mass
{
    public Mass()
    {
        Pos = new Position(0, 0);
        Height = 0f;
        Threshold = new List<float>();
        HasBridge = false;
        IsRoad = false;
        HasEvent=false;
    }
    public Mass(int x, int y)
    {
        Pos = new Position(x, y);
        Height = 0f;
        Threshold = new List<float>();
        HasBridge = false;
        IsRoad = false;
        HasEvent = false;
    }
    public Position Pos { get; set; }
    public float Height { get; set; }
    public List<float> Threshold { get; set; }
    public bool HasBridge { get; set; }
    public bool IsRoad { get; set; }
    public bool HasEvent { get; set; }
    public void setThreshold(List<float> _threshold)
    {
        Threshold = _threshold;
    }
    public int getTerrainType()
    {
        for (int i = 0; i < Threshold.Count; i++)
        {
            if (Height > Threshold[i]) return i;
        }
        return Threshold.Count - 1;
    }
    public bool CanWalk()
    {
        return ((getTerrainType() < Threshold.Count-1 && getTerrainType() > 0) || HasBridge||HasEvent);
    }
    public int Walkability()
    {
        return getTerrainType()+1;
    }
}