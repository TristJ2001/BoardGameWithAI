using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEvaluator
{
    int GetEvaluation(IRepresentation representation);
    float GetCurrentWinner(IRepresentation representation);
}
