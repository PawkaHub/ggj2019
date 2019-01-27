using System;
using UnityEngine;

[Serializable]
public struct CritterInputPacket
{
    public Quaternion headOrientation;
    public bool forward;
    public bool backward;
    public bool leftward;
    public bool rightward;
    public bool jump;
    public bool shoot;
    public float MouseX;
    public float MouseY;
}
