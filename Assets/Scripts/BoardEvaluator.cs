using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardEvaluator : IEvaluator
{
    public int GetEvaluation(IRepresentation representation)
    {
        GameOutcome outcome = representation.GetGameOutcome();

        if(outcome == GameOutcome.PLAYER1){
            return 1;
        }
        if(outcome == GameOutcome.PLAYER2){
            return -1;
        }

        return 0;
    }

    public float GetCurrentWinner(IRepresentation representation)
    {
        float currentWinner = representation.GetCurrentWinner();

        if (currentWinner == 1)
        {
            return 1;
        }
        
        if (currentWinner == -1)
        {
            return -1;
        }
        
        if (currentWinner == 0.8f)
        {
            return 0.8f;
        }

        if (currentWinner == -0.8f)
        {
            return -0.8f;
        }

        if (currentWinner == 0.5f)
        {
            return 0.5f;
        }

        if (currentWinner == -0.5f)
        {
            return -0.5f;
        }

        if (currentWinner == 0.2f)
        {
            return 0.2f;
        }

        if (currentWinner == -0.2f)
        {
            return -0.2f;
        }

        return 0;
    }
}
