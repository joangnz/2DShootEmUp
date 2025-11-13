using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public const byte MOUSEBUTTON0 = 0;
    public const byte NUMBER1 = 1;
    public const byte NUMBER2 = 2;
    public const byte NUMBER3 = 3;
    public const byte NUMBER4 = 4;
    public const byte NUMBER5 = 5;
    public const byte MOUSEBUTTON1 = 6;

    public NetworkButtons buttons;
    public Vector2 direction;
    public NetworkBool north, east, south, west;
}