using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Line 
{
    //Public Variables:
    public Vector3 start;
    public Vector3 end;

    //Constructors:
    public Line(Vector3 start, Vector3 end)
    {
        this.start = start;
        this.end = end;
    }
}