using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAIPlayer
{
    Move GetMove(IRepresentation representation, int player);
}
