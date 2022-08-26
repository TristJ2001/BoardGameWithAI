using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MiniMaxAIPlayer : IAIPlayer
{
    private IEvaluator _evaluator;

    private Move bestMove;
    private int maxDepth;

    public MiniMaxAIPlayer(IEvaluator evaluator, int depth = 3)
    {
        _evaluator = evaluator;
        maxDepth = depth;
    }
    
    public Move GetMove(IRepresentation representation, int player)
    {
        Minimax(representation, maxDepth, int.MinValue, int.MaxValue, player > 0);
        return bestMove;
    }
    
    private float Minimax(IRepresentation representation, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        if (representation.GetGameOutcome() != GameOutcome.UNDETERMINED || depth == 0)
        {
            return _evaluator.GetCurrentWinner(representation);
        }

        float bestEvaluation = maximizingPlayer ? int.MinValue : Int32.MaxValue;
        int player = maximizingPlayer ? 1 : -1; 

        List<Move> possibleMoves = representation.GetPossibleMoves(player);

        List<IRepresentation> possibleStates = new List<IRepresentation>();

        foreach (Move move in possibleMoves)
        {
            IRepresentation newRep = representation.Duplicate();
            newRep.MakeMove(move, move.JumpPiece, move.PlayerPiece);
            possibleStates.Add(newRep);
        }

        int index = 0;

        foreach (IRepresentation state in possibleStates)
        {
            float evaluation = Minimax(state, depth - 1, alpha, beta, !maximizingPlayer);
           
            if (maximizingPlayer)
            {
                if (evaluation > bestEvaluation)
                {
                    bestEvaluation = evaluation;
                    if (depth == maxDepth)
                    {
                        bestMove = possibleMoves[index];
                    }
                }
            }
            else //minimizing player
            {
                if (evaluation < bestEvaluation)
                {
                    bestEvaluation = evaluation;
                    if (depth == maxDepth)
                    {
                        bestMove = possibleMoves[index];
                    }
                }

            }

            index++;
        }

        return bestEvaluation;
    }
}
