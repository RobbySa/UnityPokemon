using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public MoveBase MoveBase { get; set; }
    public int Pp { get; set; }

    public Move(MoveBase pMove)
    {
        MoveBase = pMove;
        Pp = pMove.Pp; 
    }
}
