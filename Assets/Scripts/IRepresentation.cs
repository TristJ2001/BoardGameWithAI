using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRepresentation
{
    int[,] GetAs2DArray();
    void PlacePiece(int player, int xPos, int yPos);
    void PlaceTiles();
    int GetPieceAtTile(int xPos, int yPos);
    IRepresentation Duplicate();
    List<Move> GetPossibleMoves(int player);
    bool MakeMove(Move move, int jumpPiece, int playerPiece);
    bool IsValidMove(Move move, int jumpPiece, int playerPiece);
    int PlayerCanWinNextTurn();
    bool MovePlayerPieceAwayFromWin();
    void MovePlayerPieceIntoWin();
    GameOutcome GetGameOutcome();
    float GetCurrentWinner();
}

public enum GameOutcome{
    PLAYER1,
    PLAYER2,
    DRAW,
    UNDETERMINED
}
