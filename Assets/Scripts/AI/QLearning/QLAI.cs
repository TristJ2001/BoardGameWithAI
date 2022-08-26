using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QLAI : IAIPlayer
{
    private float[,] qTable;
    private QLRepresentation representation;

    private Move[,] moveTable;

    private int width;
    private int height;
    
    public QLAI(IRepresentation representation, int numActions)
    {
        this.representation = representation as QLRepresentation;
         
        int[,] playArea = representation.GetAs2DArray();

        width = playArea.GetLength(0);
        height = playArea.GetLength(1);
        
        int numStates = playArea.Length;

        qTable = new float[numActions, numStates];
        moveTable = new Move[numActions, numStates];
    }

    private float GetMaxQ(int state)
    {
        float maxQ = float.NegativeInfinity;
        
        for (int i = 0; i < qTable.GetLength(0); i++)
        {
            if (qTable[i, state] > maxQ)
            {
                maxQ = qTable[i, state];
            }
        }

        return maxQ;
    }
    
    private int GetStateNumber(Vector2Int pos)
    {
        return pos.x + pos.y * width;
    }
    
    public void Train(int numTrainingSessions, float learningRate, float discountFactor)
    {
        for (int i = 0; i < numTrainingSessions; i++)
        {
            QLRepresentation sessionRep = representation.Duplicate() as QLRepresentation;

            if (sessionRep is null)
            {
                Debug.Log("Session rep is null");
            }

            Vector2Int agentPosition = sessionRep.GetPlayer2Pos();

            List<Move> possibleMoves = sessionRep.GetPossibleMoves(2);
            int action = 0;

            float currentQ = 0;
            int currentState = GetStateNumber(agentPosition);

            foreach (Move move in possibleMoves)
            {
                int nextState = GetStateNumber(move.PlayerPosition);
                
                float maxQ = GetMaxQ(nextState);
                int rewardAtNextState = sessionRep.GetReward(move.PlayerPosition.x, move.PlayerPosition.y);
                
                float newQ = (1 - learningRate) * currentQ +
                             learningRate * (rewardAtNextState + discountFactor * maxQ);
                
                qTable[action, currentState] = newQ;
                moveTable[action, currentState] = move;
                currentQ = newQ;
                
                currentState = nextState;
                
                sessionRep.MakeMove(move, move.JumpPiece, move.PlayerPiece);

                action++;
            }
        }
    }

    
    public Move GetMove(IRepresentation representation, int player)
    {
        if (!(representation is QLRepresentation))
        {
            Debug.Log("Not a QL Representation");
            return new Move(0, 0, Vector2Int.zero, Vector2Int.zero);
        }
        
        if (representation.PlayerCanWinNextTurn() == -1)
        {
            representation.MovePlayerPieceIntoWin();
            return null;
        }
        
        if (representation.PlayerCanWinNextTurn() == 1)
        {
            if (representation.MovePlayerPieceAwayFromWin())
            {
                return null;
            }
        }
        
        QLRepresentation qLRep = (QLRepresentation) representation;
        int currentState = GetStateNumber(qLRep.GetPlayer2Pos());
        
        float maxQValue = float.NegativeInfinity;
        int actionIndex = -1;

        // Debug.Log(ToString());
        for (int i = 0; i < qTable.GetLength(0); i++)
        {
            if (qTable[i, currentState] > maxQValue)
            {
                maxQValue = qTable[i, currentState];
                actionIndex = i;
            }
        }

        return new Move(moveTable[actionIndex, currentState].JumpPiece,
            moveTable[actionIndex, currentState].PlayerPiece,
            moveTable[actionIndex, currentState].JumpPosition, 
            moveTable[actionIndex, currentState].PlayerPosition);
    }

    public override string ToString()
    {
        string s = "\t";

        for (int c = 0; c < qTable.GetLength(0); c++)
        {
            s += c + "\t";
        }

        s += "\n";
        
        for (int r = 0; r < qTable.GetLength(1); r++)
        {
            s += r + ") \t";
            
            for (int c = 0; c < qTable.GetLength(0); c++)
            {
                s += qTable[c, r] + "\t";
            }

            s += "\n";
        }

        return s;
    }
}
