using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class QLRepresentation : IRepresentation
{
    public delegate void OnPreviousPositionChanged(int piece, Vector2Int prevPos);
    public static event OnPreviousPositionChanged OnPreviousPositionChangedAction;
    
    private int[,] gameBoard = new int[4,4];

    private int[,] rewardBoard = new int[4, 4];
    
    private int xLength = 4;
    private int yLength = 4;

    private Vector2Int previousPlayerPiece1Pos;
    private Vector2Int previousPlayerPiece2Pos;
    private Vector2Int previousJumpPiece1Pos;
    private Vector2Int previousJumpPiece2Pos;
    private Vector2Int previousJumpPiece3Pos;
    private Vector2Int previousJumpPiece4Pos;

    private Vector2Int agentPos;
    
    public QLRepresentation(int[,] gameBoard){
        this.gameBoard = gameBoard;

        agentPos = FindPositionOfPiece(2);
    }
    
    public int[,] GetAs2DArray()
    {
        return gameBoard;
    }

    public Vector2Int GetPlayer2Pos()
    {
        for (int y = 0; y < gameBoard.GetLength(1); y++)
        {
            for (int x = 0; x < gameBoard.GetLength(0); x++)
            {
                if (gameBoard[x, y] == 2)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return new Vector2Int();
    }

    public void PlacePiece(int player, int xPos, int yPos)
    {
        gameBoard[xPos, yPos] = player;
    }

    public void PopulateRewardBoard()
    {
        // win tile reward
        rewardBoard[0, 3] = 6;
        rewardBoard[3, 0] = 6;
        
        // adjacent to win tile reward
        rewardBoard[2, 0] = 5;
        rewardBoard[3, 1] = 5;
        rewardBoard[0, 2] = 5;
        rewardBoard[1, 3] = 5;
        
        // 2 tiles away from win tile reward
        rewardBoard[1, 0] = 4;
        rewardBoard[3, 2] = 4;
        rewardBoard[0, 1] = 4;
        rewardBoard[2, 3] = 4;
        
        // diagonal to win tile reward
        rewardBoard[2, 1] = 3;
        rewardBoard[1, 2] = 3;
        
        // adjacent to diagonal to win tile reward
        rewardBoard[1, 1] = 2;
        rewardBoard[2, 2] = 2;
        
        // starting tiles reward
        rewardBoard[0, 0] = 1;
        rewardBoard[3, 3] = 1;
    }

    public void PlaceTiles()
    {
        for (int x = 0; x < xLength; x++)
        {
            for (int y = 0; y < yLength; y++)
            {
                if (x == 0 && y == yLength - 1 || x == xLength - 1 && y == 0)
                {
                    gameBoard[x, y] = 3;
                }
                else
                {
                    gameBoard[x, y] = 0;
                }
            }
        }
    }
    
    public int GetReward(int x, int y)
    {
        PopulateRewardBoard();

        return rewardBoard[x, y];
    }

    public int GetPieceAtTile(int xPos, int yPos)
    {
        return gameBoard[xPos, yPos];
    }

    public IRepresentation Duplicate()
    {
        return new QLRepresentation(gameBoard.Clone() as int[,]);
    }

    public List<Move> GetPossibleMoves(int player)
    {
        List<Move> moves = new List<Move>();
        
        List<Vector2Int> movesForPlayerPiece1 = GetEmptySpacesAroundPiece(1);
        List<Vector2Int> movesForPlayerPiece2 = GetEmptySpacesAroundPiece(2);
        List<Vector2Int> movesForJumpPiece1 = GetEmptySpacesAroundPiece(4);
        List<Vector2Int> movesForJumpPiece2 = GetEmptySpacesAroundPiece(5);
        List<Vector2Int> movesForJumpPiece3 = GetEmptySpacesAroundPiece(6);
        List<Vector2Int> movesForJumpPiece4 = GetEmptySpacesAroundPiece(7);

        foreach (Vector2Int emptyJump1Tile in movesForJumpPiece1)
        {
            foreach (Vector2Int emptyPlayer1Tile in movesForPlayerPiece1)
            {
                //Make sure that AI can't move 2 pieces into same tile
                if (emptyJump1Tile == emptyPlayer1Tile)
                {
                    continue;
                }

                if (IsValidMove(new Move(4, 1, emptyJump1Tile, emptyPlayer1Tile), 4, 1))
                {
                    moves.Add(new Move(4, 1, emptyJump1Tile, emptyPlayer1Tile));
                }
            }
            foreach (Vector2Int emptyPlayer2Tile in movesForPlayerPiece2)
            {
                if (emptyJump1Tile == emptyPlayer2Tile)
                {
                    continue;
                }

                if (IsValidMove(new Move(4, 2, emptyJump1Tile, emptyPlayer2Tile), 4, 2))
                {
                    moves.Add(new Move(4, 2, emptyJump1Tile, emptyPlayer2Tile));
                }
            }
        }
        foreach (Vector2Int emptyJump2Tile in movesForJumpPiece2)
        {
            foreach (Vector2Int emptyPlayer1Tile in movesForPlayerPiece1)
            {
                if (emptyJump2Tile == emptyPlayer1Tile)
                {
                    continue;
                }

                if (IsValidMove(new Move(5, 1, emptyJump2Tile, emptyPlayer1Tile), 5, 1))
                {
                    moves.Add(new Move(5, 1, emptyJump2Tile, emptyPlayer1Tile));
                }
            }
            foreach (Vector2Int emptyPlayer2Tile in movesForPlayerPiece2)
            {
                if (emptyJump2Tile == emptyPlayer2Tile)
                {
                    continue;
                }

                if (IsValidMove(new Move(5, 2, emptyJump2Tile, emptyPlayer2Tile), 5, 2))
                {
                    moves.Add(new Move(5, 2, emptyJump2Tile, emptyPlayer2Tile));
                }
            }
        }
        foreach (Vector2Int emptyJump3Tile in movesForJumpPiece3)
        {
            foreach (Vector2Int emptyPlayer1Tile in movesForPlayerPiece1)
            {
                if (emptyJump3Tile == emptyPlayer1Tile)
                {
                    continue;
                }

                if (IsValidMove(new Move(6, 1, emptyJump3Tile, emptyPlayer1Tile), 6, 1))
                {
                    moves.Add(new Move(6, 1, emptyJump3Tile, emptyPlayer1Tile));
                }
            }
            foreach (Vector2Int emptyPlayer2Tile in movesForPlayerPiece2)
            {
                if (emptyJump3Tile == emptyPlayer2Tile)
                {
                    continue;
                }

                if (IsValidMove(new Move(6, 2, emptyJump3Tile, emptyPlayer2Tile), 6, 2))
                {
                    moves.Add(new Move(6, 2, emptyJump3Tile, emptyPlayer2Tile));
                }
            }
        }
        foreach (Vector2Int emptyJump4Tile in movesForJumpPiece4)
        {
            foreach (Vector2Int emptyPlayer1Tile in movesForPlayerPiece1)
            {
                if (emptyJump4Tile == emptyPlayer1Tile)
                {
                    continue;
                }

                if (IsValidMove(new Move(7, 1, emptyJump4Tile, emptyPlayer1Tile), 7, 1))
                {
                    moves.Add(new Move(7, 1, emptyJump4Tile, emptyPlayer1Tile));
                }
            }
            foreach (Vector2Int emptyPlayer2Tile in movesForPlayerPiece2)
            {
                if (emptyJump4Tile == emptyPlayer2Tile)
                {
                    continue;
                }

                if (IsValidMove(new Move(7, 2, emptyJump4Tile, emptyPlayer2Tile), 7, 2))
                {
                    moves.Add(new Move(7, 2, emptyJump4Tile, emptyPlayer2Tile));
                }
            }
        }
        
        return moves;
    }
    
    private List<Vector2Int> GetEmptySpacesAroundPiece(int piece)
    {
        List<Vector2Int> emptySpaces = new List<Vector2Int>();
        
        int xPos = FindPositionOfPiece(piece).x;
        int yPos = FindPositionOfPiece(piece).y;
        
        for (int x = xPos - 1; x <= xPos + 1; x++)
        {
            //x out of bounds
            if (x <= 0 || x >= gameBoard.GetLength(0))
            {
                continue;
            }
            
            for (int y = yPos - 1; y <= yPos + 1; y++)
            {
                //y out of bounds
                if (y <= 0 || y >= gameBoard.GetLength(1))
                {
                    continue;
                }

                //skip diagonal
                if (x == xPos - 1 && y == yPos + 1
                    || x == xPos - 1 && y == yPos - 1
                    || x == xPos + 1 && y == yPos + 1
                    || x == xPos + 1 && y == yPos - 1)
                {
                    continue;
                }

                //skip tile with piece already in it
                if (gameBoard[x, y] == 1
                    || gameBoard[x, y] == 2
                    || gameBoard[x, y] == 4
                    || gameBoard[x, y] == 5
                    || gameBoard[x, y] == 6
                    || gameBoard[x, y] == 7)
                {
                    continue;
                }
                
                Vector2Int position = new Vector2Int();

                position.x = x;
                position.y = y;
                
                emptySpaces.Add(position);
            }
        }

        if (piece == 1 || piece == 2)
        {
            
            //Check for jump piece
            if (xPos + 1 < gameBoard.GetLength(0) &&
                IsJumpPieceAtPos(xPos + 1, yPos))
            {
                if (xPos + 2 >= gameBoard.GetLength(0)
                    || IsJumpPieceAtPos(xPos + 2, yPos)
                    || gameBoard[xPos + 2, yPos] == 1
                    || gameBoard[xPos + 2, yPos] == 2)
                {
                    
                }
                else
                {
                    Vector2Int position = new Vector2Int();
        
                    position.x = xPos + 2;
                    position.y = yPos;
                    
                    emptySpaces.Add(position);
                }
            }
            
            if (xPos - 1 > 0 &&
                IsJumpPieceAtPos(xPos - 1, yPos))
            {
                if (xPos - 2 < 0
                    || IsJumpPieceAtPos(xPos - 2, yPos)
                    || gameBoard[xPos - 2, yPos] == 1
                    || gameBoard[xPos - 2, yPos] == 2)
                {
                    
                }
                else
                {
                    Vector2Int position = new Vector2Int();
        
                    position.x = xPos - 2;
                    position.y = yPos;
                    
                    emptySpaces.Add(position);
                }
            }
            
            if (yPos + 1 < gameBoard.GetLength(1) &&
                IsJumpPieceAtPos(xPos, yPos + 1))
            {
                if (yPos + 2 >= gameBoard.GetLength(1)
                    || IsJumpPieceAtPos(xPos, yPos + 2)
                    || gameBoard[xPos, yPos + 2] == 1
                    || gameBoard[xPos, yPos + 2] == 2)
                {
                    
                }
                else
                {
                    Vector2Int position = new Vector2Int();
        
                    position.x = xPos;
                    position.y = yPos + 2;
                    
                    emptySpaces.Add(position);
                }
            }
            
            if (yPos - 1 > 0 &&
                IsJumpPieceAtPos(xPos, yPos - 1))
            {
                if (yPos - 2 < 0
                    || IsJumpPieceAtPos(xPos, yPos - 2)
                    || gameBoard[xPos, yPos - 2] == 1
                    || gameBoard[xPos, yPos - 2] == 2)
                {
                    
                }
                else
                {
                    Vector2Int position = new Vector2Int();
        
                    position.x = xPos;
                    position.y = yPos - 2;
                    
                    emptySpaces.Add(position);
                }
            }
        }

        return emptySpaces;
    }

    public bool MakeMove(Move move, int jumpPiece, int playerPiece)
    {
        Vector2Int jumpPiecePos = FindPositionOfPiece(jumpPiece);
        Vector2Int playerPiecePos = FindPositionOfPiece(playerPiece);

        gameBoard[jumpPiecePos.x, jumpPiecePos.y] = 0;
        gameBoard[move.JumpPosition.x, move.JumpPosition.y] = jumpPiece;
        
        gameBoard[playerPiecePos.x, playerPiecePos.y] = 0;
        gameBoard[move.PlayerPosition.x, move.PlayerPosition.y] = playerPiece;

        agentPos = FindPositionOfPiece(2);
        return true;
    }
    
    public Vector2Int FindPositionOfPiece(int piece)
    {
        Vector2Int position = new Vector2Int();

        int w = gameBoard.GetLength(0);
        int h = gameBoard.GetLength(1);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (gameBoard[x, y] == piece)
                {
                    position.x = x;
                    position.y = y;
                }
            }
        }

        return position;
    }

    public bool IsValidMove(Move move, int jumpPiece, int playerPiece)
    {
        previousPlayerPiece1Pos = GameManager._instance.PlayerPiece1PreviousPos;
        previousPlayerPiece2Pos = GameManager._instance.PlayerPiece2PreviousPos;
        previousJumpPiece1Pos = GameManager._instance.jumpPiece1PreviousPos;
        previousJumpPiece2Pos = GameManager._instance.jumpPiece2PreviousPos;
        previousJumpPiece3Pos = GameManager._instance.jumpPiece3PreviousPos;
        previousJumpPiece4Pos = GameManager._instance.jumpPiece4PreviousPos;
        
       //not valid x coord
        if(move.JumpPosition.x < 0 || move.JumpPosition.x >= gameBoard.GetLength(0))
        {
            return false;
        }
        if(move.PlayerPosition.x < 0 || move.PlayerPosition.x >= gameBoard.GetLength(0))
        {
            return false;
        }
        
        //not valid y coord
        if(move.JumpPosition.y < 0 || move.JumpPosition.y >= gameBoard.GetLength(1))
        {
            return false;
        }
        if(move.PlayerPosition.y < 0 || move.PlayerPosition.y >= gameBoard.GetLength(1))
        {
            return false;
        }
        
        //tile already occupied
        if(gameBoard[move.JumpPosition.x, move.JumpPosition.y] == 1
        || gameBoard[move.JumpPosition.x, move.JumpPosition.y] == 2
        || gameBoard[move.JumpPosition.x, move.JumpPosition.y] == 4
        || gameBoard[move.JumpPosition.x, move.JumpPosition.y] == 5
        || gameBoard[move.JumpPosition.x, move.JumpPosition.y] == 6
        || gameBoard[move.JumpPosition.x, move.JumpPosition.y] == 7)
        {
            return false;
        }
        if(gameBoard[move.PlayerPosition.x, move.PlayerPosition.y] == 1
           || gameBoard[move.PlayerPosition.x, move.PlayerPosition.y] == 2
           || gameBoard[move.PlayerPosition.x, move.PlayerPosition.y] == 4
           || gameBoard[move.PlayerPosition.x, move.PlayerPosition.y] == 5
           || gameBoard[move.PlayerPosition.x, move.PlayerPosition.y] == 6
           || gameBoard[move.PlayerPosition.x, move.PlayerPosition.y] == 7)
        {
            return false;
        }

        //cannot move piece into its previous position
        if (playerPiece == 1 && move.PlayerPosition.Equals(previousPlayerPiece1Pos))
        {
            return false;
        }
        
        if (playerPiece == 2 && move.PlayerPosition.Equals(previousPlayerPiece2Pos))
        {
            return false;
        }
        
        if (jumpPiece == 4 && move.JumpPosition.Equals(previousJumpPiece1Pos))
        {
            return false;
        }
        
        if (jumpPiece == 5 && move.JumpPosition.Equals(previousJumpPiece2Pos))
        {
            return false;
        }
        
        if (jumpPiece == 6 && move.JumpPosition.Equals(previousJumpPiece3Pos))
        {
            return false;
        }
        
        if (jumpPiece == 7 && move.JumpPosition.Equals(previousJumpPiece4Pos))
        {
            return false;
        }
        
        return true;
    }
    
    public bool IsJumpPieceAtPos(int xPos, int yPos)
    {
        if (xPos >= gameBoard.GetLength(0)
            || xPos < 0
            || yPos >= gameBoard.GetLength(1)
            || yPos < 0)
        {
            return false;
        }
        
        if (gameBoard[xPos, yPos] == 4
            || gameBoard[xPos, yPos] == 5
            || gameBoard[xPos, yPos] == 6
            || gameBoard[xPos, yPos] == 7)
        {
            return true;
        }

        return false;
    }

    public int PlayerCanWinNextTurn()
    {
        //Player 2 is adjacent to win tile
        if (gameBoard[2, 0] == 2 || gameBoard[3, 1] == 2 || gameBoard[0, 2] == 2 || gameBoard[1, 3] == 2)
        {
            return -1;
        }
        
        //Player 2 can jump over jump piece into win tile
        if (gameBoard[1, 0] == 2 && IsJumpPieceAtPos(2, 0) 
            || gameBoard[3, 2] == 2 && IsJumpPieceAtPos(3, 1)
            || gameBoard[0, 1] == 2 && IsJumpPieceAtPos(0, 2)
            || gameBoard[2, 3] == 2 && IsJumpPieceAtPos(1, 3))
        {
            return -1;
        }
        
        //Player 2 can move jump piece and then jump over it into win tile
        if (gameBoard[1, 0] == 2 && IsJumpPieceAtPos(2, 1)
            || gameBoard[3, 2] == 2 && IsJumpPieceAtPos(2, 1)
            || gameBoard[0, 1] == 2 && IsJumpPieceAtPos(1, 2)
            || gameBoard[2, 3] == 2 && IsJumpPieceAtPos(1, 2))
        {
            return -1;
        }
        
        //Player 1 is adjacent to win tile
        if (gameBoard[2, 0] == 1 || gameBoard[3, 1] == 1 || gameBoard[0, 2] == 1 || gameBoard[1, 3] == 1)
        {
            return 1;
        }
        
        //Player 1 can jump over jump piece into win tile
        if (gameBoard[1, 0] == 1 && IsJumpPieceAtPos(2, 0) 
            || gameBoard[3, 2] == 1 && IsJumpPieceAtPos(3, 1)
            || gameBoard[0, 1] == 1 && IsJumpPieceAtPos(0, 2)
            || gameBoard[2, 3] == 1 && IsJumpPieceAtPos(1, 3))
        {
            return 1;
        }

        //Player 1 can move jump piece and then jump over it into win tile
        if (gameBoard[1, 0] == 1 && IsJumpPieceAtPos(2, 1)
            || gameBoard[3, 2] == 1 && IsJumpPieceAtPos(2, 1)
            || gameBoard[0, 1] == 1 && IsJumpPieceAtPos(1, 2)
            || gameBoard[2, 3] == 1 && IsJumpPieceAtPos(1, 2))
        {
            return 1;
        }

        return 0;
    }

    public bool MovePlayerPieceAwayFromWin()
    {
        if (gameBoard[2, 0] == 1)
        {
            if (gameBoard[2, 1] == 0 && previousPlayerPiece1Pos.x != 2 && previousPlayerPiece1Pos.y != 1)
            {
                gameBoard[2, 0] = 0;
                gameBoard[2, 1] = 1;

                Vector2Int oldPos = new Vector2Int(2, 0);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count - 1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (gameBoard[1, 0] == 0 && previousPlayerPiece1Pos.x != 1 && previousPlayerPiece1Pos.y != 0)
            {
                gameBoard[2, 0] = 0;
                gameBoard[1, 0] = 1;
                
                Vector2Int oldPos = new Vector2Int(2, 0);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);

                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count - 1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (IsJumpPieceAtPos(2, 1) && gameBoard[2, 2] == 0 && previousPlayerPiece1Pos.x != 2 && previousPlayerPiece1Pos.y != 2)
            {
                gameBoard[2, 0] = 0;
                gameBoard[2, 2] = 1;
                
                Vector2Int oldPos = new Vector2Int(2, 0);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count - 1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (IsJumpPieceAtPos(1, 0) && gameBoard[0, 0] == 0 && previousPlayerPiece1Pos.x != 0 && previousPlayerPiece1Pos.y != 0)
            {
                gameBoard[2, 0] = 0;
                gameBoard[0, 0] = 1;
                
                Vector2Int oldPos = new Vector2Int(2, 0);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);

                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count - 1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
        }

        if (gameBoard[3, 1] == 1)
        {
            if (gameBoard[2, 1] == 0 && previousPlayerPiece1Pos.x != 2 && previousPlayerPiece1Pos.y != 1)
            {
                gameBoard[3, 1] = 0;
                gameBoard[2, 1] = 1;
                
                Vector2Int oldPos = new Vector2Int(3, 1);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count - 1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (gameBoard[3, 2] == 0 && previousPlayerPiece1Pos.x != 3 && previousPlayerPiece1Pos.y != 2)
            {
                gameBoard[3, 1] = 0;
                gameBoard[3, 2] = 1;
                
                Vector2Int oldPos = new Vector2Int(3, 1);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count - 1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (IsJumpPieceAtPos(2, 1) && gameBoard[1, 1] == 0 && previousPlayerPiece1Pos.x != 1 && previousPlayerPiece1Pos.y != 1)
            {
                gameBoard[3, 1] = 0;
                gameBoard[1, 1] = 1;
                
                Vector2Int oldPos = new Vector2Int(3, 1);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count - 1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (IsJumpPieceAtPos(3, 2) && gameBoard[3, 3] == 0 && previousPlayerPiece1Pos.x != 3 && previousPlayerPiece1Pos.y != 3)
            {
                gameBoard[3, 1] = 0;
                gameBoard[3, 3] = 1;
                
                Vector2Int oldPos = new Vector2Int(3, 1);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count - 1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
        }

        if (gameBoard[0, 2] == 1)
        {
            if (gameBoard[1, 2] == 0 && previousPlayerPiece1Pos.x != 1 && previousPlayerPiece1Pos.y != 2)
            {
                gameBoard[0, 2] = 0;
                gameBoard[1, 2] = 1;
                
                Vector2Int oldPos = new Vector2Int(0, 2);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count - 1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (gameBoard[0, 1] == 0 && previousPlayerPiece1Pos.x != 0 && previousPlayerPiece1Pos.y != 1)
            {
                gameBoard[0, 2] = 0;
                gameBoard[0, 1] = 1;
                
                Vector2Int oldPos = new Vector2Int(0, 2);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count - 1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (IsJumpPieceAtPos(1, 2) && gameBoard[2, 2] == 0 && previousPlayerPiece1Pos.x != 2 && previousPlayerPiece1Pos.y != 2)
            {
                gameBoard[0, 2] = 0;
                gameBoard[2, 2] = 1;
                
                Vector2Int oldPos = new Vector2Int(0, 2);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count - 1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (IsJumpPieceAtPos(0, 1) && gameBoard[0, 0] == 0 && previousPlayerPiece1Pos.x != 0 && previousPlayerPiece1Pos.y != 0)
            {
                gameBoard[0, 2] = 0;
                gameBoard[0, 0] = 1;
                
                Vector2Int oldPos = new Vector2Int(0, 2);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count - 1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
        }

        if (gameBoard[1, 3] == 1)
        {
            if (gameBoard[1, 2] == 0 && previousPlayerPiece1Pos.x != 1 && previousPlayerPiece1Pos.y != 2)
            {
                gameBoard[1, 3] = 0;
                gameBoard[1, 2] = 1;
                
                Vector2Int oldPos = new Vector2Int(1, 3);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count - 1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (gameBoard[2, 3] == 0 && previousPlayerPiece1Pos.x != 2 && previousPlayerPiece1Pos.y != 3)
            {
                gameBoard[1, 3] = 0;
                gameBoard[2, 3] = 1;
                
                Vector2Int oldPos = new Vector2Int(1, 3);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count - 1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (IsJumpPieceAtPos(1, 2) && gameBoard[1, 1] == 0 && previousPlayerPiece1Pos.x != 1 && previousPlayerPiece1Pos.y != 1)
            {
                gameBoard[1, 3] = 0;
                gameBoard[1, 1] = 1;
                
                Vector2Int oldPos = new Vector2Int(1, 3);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count - 1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (IsJumpPieceAtPos(2, 3) && gameBoard[3, 3] == 0 && previousPlayerPiece1Pos.x != 3 && previousPlayerPiece1Pos.y != 3)
            {
                gameBoard[1, 3] = 0;
                gameBoard[3, 3] = 1;
                
                Vector2Int oldPos = new Vector2Int(1, 3);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
        }

        if (gameBoard[1, 0] == 1)
        {
            if (gameBoard[0, 0] == 0 && previousPlayerPiece1Pos.x != 0 && previousPlayerPiece1Pos.y != 0)
            {
                gameBoard[1, 0] = 0;
                gameBoard[0, 0] = 1;
                
                Vector2Int oldPos = new Vector2Int(1, 0);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (gameBoard[1, 1] == 0 && previousPlayerPiece1Pos.x != 1 && previousPlayerPiece1Pos.y != 1)
            {
                gameBoard[1, 0] = 0;
                gameBoard[1, 1] = 1;
                
                Vector2Int oldPos = new Vector2Int(1, 0);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (IsJumpPieceAtPos(1, 1) && gameBoard[1, 2] == 0 && previousPlayerPiece1Pos.x != 1 && previousPlayerPiece1Pos.y != 2)
            {
                gameBoard[1, 0] = 0;
                gameBoard[1, 2] = 1;
                
                Vector2Int oldPos = new Vector2Int(1, 0);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
        }
        
        if (gameBoard[3, 2] == 1)
        {
            if (gameBoard[3, 3] == 0 && previousPlayerPiece1Pos.x != 3 && previousPlayerPiece1Pos.y != 3)
            {
                gameBoard[3, 2] = 0;
                gameBoard[3, 3] = 1;
                
                Vector2Int oldPos = new Vector2Int(3, 2);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (gameBoard[2, 2] == 0 && previousPlayerPiece1Pos.x != 2 && previousPlayerPiece1Pos.y != 2)
            {
                gameBoard[3, 2] = 0;
                gameBoard[2, 2] = 1;
                
                Vector2Int oldPos = new Vector2Int(3, 2);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (IsJumpPieceAtPos(2, 2) && gameBoard[1, 2] == 0 && previousPlayerPiece1Pos.x != 1 && previousPlayerPiece1Pos.y != 2)
            {
                gameBoard[3, 2] = 0;
                gameBoard[1, 2] = 1;
                
                Vector2Int oldPos = new Vector2Int(3, 2);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
        }
        
        if (gameBoard[0, 1] == 1)
        {
            if (gameBoard[0, 0] == 0 && previousPlayerPiece1Pos.x != 0 && previousPlayerPiece1Pos.y != 0)
            {
                gameBoard[0, 1] = 0;
                gameBoard[0, 0] = 1;
                
                Vector2Int oldPos = new Vector2Int(0, 1);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (gameBoard[1, 1] == 0 && previousPlayerPiece1Pos.x != 1 && previousPlayerPiece1Pos.y != 1)
            {
                gameBoard[0, 1] = 0;
                gameBoard[1, 1] = 1;
                
                Vector2Int oldPos = new Vector2Int(0, 1);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (IsJumpPieceAtPos(1, 1) && gameBoard[2, 1] == 0 && previousPlayerPiece1Pos.x != 2 && previousPlayerPiece1Pos.y != 1)
            {
                gameBoard[0, 1] = 0;
                gameBoard[2, 1] = 1;
                
                Vector2Int oldPos = new Vector2Int(0, 1);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
        }
        
        if (gameBoard[2, 3] == 1)
        {
            if (gameBoard[3, 3] == 0 && previousPlayerPiece1Pos.x != 3 && previousPlayerPiece1Pos.y != 3)
            {
                gameBoard[2, 3] = 0;
                gameBoard[3, 3] = 1;
                
                Vector2Int oldPos = new Vector2Int(2, 3);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (gameBoard[2, 2] == 0 && previousPlayerPiece1Pos.x != 2 && previousPlayerPiece1Pos.y != 2)
            {
                gameBoard[2, 3] = 0;
                gameBoard[2, 2] = 1;
                
                Vector2Int oldPos = new Vector2Int(2, 3);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);
                
                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
            if (IsJumpPieceAtPos(2, 2) && gameBoard[2, 1] == 0 && previousPlayerPiece1Pos.x != 2 && previousPlayerPiece1Pos.y != 1)
            {
                gameBoard[2, 3] = 0;
                gameBoard[2, 1] = 1;
                
                Vector2Int oldPos = new Vector2Int(2, 3);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);

                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
        }

        //If player can move jump piece and then jump over 
        if (gameBoard[1, 0] == 1 && IsJumpPieceAtPos(2, 1))
        {
            int JumpPiece = GetPieceAtTile(2, 1);
            if (gameBoard[1, 1] == 0  && gameBoard[1, 2] == 0 && previousPlayerPiece1Pos.x != 1 && previousPlayerPiece1Pos.y != 2)
            {
                gameBoard[2, 1] = 0;
                gameBoard[1, 1] = JumpPiece;
                
                gameBoard[1, 0] = 0;
                gameBoard[1, 2] = 1;
                
                Vector2Int oldPlayerPos = new Vector2Int(1, 0);
                OnPreviousPositionChangedAction?.Invoke(1, oldPlayerPos);

                Vector2Int oldJumpPos = new Vector2Int(2, 1);
                OnPreviousPositionChangedAction?.Invoke(JumpPiece, oldJumpPos);
                
                return true;
            }

            if (gameBoard[0, 0] == 0 && previousPlayerPiece1Pos.x != 0 && previousPlayerPiece1Pos.y != 0)
            {
                gameBoard[1, 0] = 0;
                gameBoard[0, 0] = 1;
                
                Vector2Int oldPos = new Vector2Int(1, 0);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);

                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }

            if (gameBoard[1, 1] == 0 && previousPlayerPiece1Pos.x != 1 && previousPlayerPiece1Pos.y != 0)
            {
                gameBoard[1, 0] = 0;
                gameBoard[1, 1] = 1;
                
                Vector2Int oldPos = new Vector2Int(1, 0);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);

                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
        }

        if (gameBoard[3, 2] == 1 && IsJumpPieceAtPos(2, 1))
        {
            int JumpPiece = GetPieceAtTile(2, 1);
            if (gameBoard[1, 2] == 0  && gameBoard[2, 2] == 0 && previousPlayerPiece1Pos.x != 1 && previousPlayerPiece1Pos.y != 2)
            {
                gameBoard[2, 1] = 0;
                gameBoard[2, 2] = JumpPiece;
                
                gameBoard[3, 2] = 0;
                gameBoard[1, 2] = 1;
                
                Vector2Int oldPlayerPos = new Vector2Int(3, 2);
                OnPreviousPositionChangedAction?.Invoke(1, oldPlayerPos);

                Vector2Int oldJumpPos = new Vector2Int(2, 1);
                OnPreviousPositionChangedAction?.Invoke(JumpPiece, oldJumpPos);
                
                return true;
            }

            if (gameBoard[3, 3] == 0 && previousPlayerPiece1Pos.x != 3 && previousPlayerPiece1Pos.y != 3)
            {
                gameBoard[3, 2] = 0;
                gameBoard[3, 3] = 1;
                
                Vector2Int oldPos = new Vector2Int(3, 2);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);

                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }

            if (gameBoard[2, 2] == 0 && previousPlayerPiece1Pos.x != 2 && previousPlayerPiece1Pos.y != 2)
            {
                gameBoard[3, 2] = 0;
                gameBoard[2, 2] = 1;
                
                Vector2Int oldPos = new Vector2Int(3, 2);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);

                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
        }

        if (gameBoard[0, 1] == 1 && IsJumpPieceAtPos(1, 2))
        {
            int JumpPiece = GetPieceAtTile(1, 2);
            if (gameBoard[1, 1] == 0  && gameBoard[2, 1] == 0 && previousPlayerPiece1Pos.x != 2 && previousPlayerPiece1Pos.y != 1)
            {
                gameBoard[1, 2] = 0;
                gameBoard[1, 1] = JumpPiece;
                
                gameBoard[0, 1] = 0;
                gameBoard[2, 1] = 1;
                
                Vector2Int oldPlayerPos = new Vector2Int(1, 2);
                OnPreviousPositionChangedAction?.Invoke(1, oldPlayerPos);

                Vector2Int oldJumpPos = new Vector2Int(1, 2);
                OnPreviousPositionChangedAction?.Invoke(JumpPiece, oldJumpPos);
                
                return true;
            }

            if (gameBoard[0, 0] == 0 && previousPlayerPiece1Pos.x != 0 && previousPlayerPiece1Pos.y != 0)
            {
                gameBoard[0, 1] = 0;
                gameBoard[0, 0] = 1;
                
                Vector2Int oldPos = new Vector2Int(0, 1);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);

                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }

            if (gameBoard[1, 1] == 0 && previousPlayerPiece1Pos.x != 1 && previousPlayerPiece1Pos.y != 1)
            {
                gameBoard[0, 1] = 0;
                gameBoard[1, 1] = 1;
                
                Vector2Int oldPos = new Vector2Int(1, 1);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);

                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
        }

        if (gameBoard[2, 3] == 1 && IsJumpPieceAtPos(1, 2))
        {
            int JumpPiece = GetPieceAtTile(1, 2);
            if (gameBoard[2, 2] == 0  && gameBoard[2, 1] == 0 && previousPlayerPiece1Pos.x != 2 && previousPlayerPiece1Pos.y != 1)
            {
                gameBoard[1, 2] = 0;
                gameBoard[2, 2] = JumpPiece;
                
                gameBoard[2, 3] = 0;
                gameBoard[2, 1] = 1;
                
                Vector2Int oldPlayerPos = new Vector2Int(2, 3);
                OnPreviousPositionChangedAction?.Invoke(1, oldPlayerPos);

                Vector2Int oldJumpPos = new Vector2Int(1, 2);
                OnPreviousPositionChangedAction?.Invoke(JumpPiece, oldJumpPos);
                
                return true;
            }

            if (gameBoard[3, 3] == 0 && previousPlayerPiece1Pos.x != 3 && previousPlayerPiece1Pos.y != 3)
            {
                gameBoard[2, 3] = 0;
                gameBoard[3, 3] = 1;
                
                Vector2Int oldPos = new Vector2Int(2, 3);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);

                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }

            if (gameBoard[2, 2] == 0 && previousPlayerPiece1Pos.x != 2 && previousPlayerPiece1Pos.y != 2)
            {
                gameBoard[2, 3] = 0;
                gameBoard[2, 2] = 1;
                
                Vector2Int oldPos = new Vector2Int(2, 3);
                OnPreviousPositionChangedAction?.Invoke(1, oldPos);

                int jumpPiece = Random.Range(4, 7);
                List<Vector2Int> emptySpacesAroundJump = GetEmptySpacesAroundPiece(jumpPiece);

                int index = Random.Range(0, emptySpacesAroundJump.Count -1);
                Vector2Int currentPos = FindPositionOfPiece(jumpPiece);

                gameBoard[currentPos.x, currentPos.y] = 0;
                gameBoard[emptySpacesAroundJump[index].x, emptySpacesAroundJump[index].y] = jumpPiece;
                
                return true;
            }
        }
        
        return false;
    }

    public void MovePlayerPieceIntoWin()
    {
        if (gameBoard[2, 0] == 2)
        {
            gameBoard[2, 0] = 0;
            gameBoard[3, 0] = 2;
        }

        if (gameBoard[3, 1] == 2)
        {
            gameBoard[3, 1] = 0;
            gameBoard[3, 0] = 2;
        }

        if (gameBoard[0, 2] == 2)
        {
            gameBoard[0, 2] = 0;
            gameBoard[0, 3] = 2;
        }

        if (gameBoard[1, 3] == 2)
        {
            gameBoard[1, 3] = 0;
            gameBoard[0, 3] = 2;
        }

        if (gameBoard[2, 3] == 2)
        {
            gameBoard[2, 3] = 0;
            gameBoard[0, 3] = 2;
        }

        if (gameBoard[0, 1] == 2)
        {
            gameBoard[0, 1] = 0;
            gameBoard[0, 3] = 2;
        }

        if (gameBoard[1, 0] == 2)
        {
            gameBoard[1, 0] = 0;
            gameBoard[3, 0] = 2;
        }

        if (gameBoard[3, 2] == 2)
        {
            gameBoard[3, 2] = 0;
            gameBoard[3, 0] = 2;
        }

        if (gameBoard[1, 0] == 2 && IsJumpPieceAtPos(2, 1))
        {
            int jumpPiece = GetPieceAtTile(2, 1);
            gameBoard[2, 1] = 0;
            gameBoard[2, 0] = jumpPiece;

            gameBoard[1, 0] = 0;
            gameBoard[3, 0] = 2;
        }
        
        if (gameBoard[3, 2] == 2 && IsJumpPieceAtPos(2, 1))
        {
            int jumpPiece = GetPieceAtTile(2, 1);
            gameBoard[2, 1] = 0;
            gameBoard[3, 1] = jumpPiece;

            gameBoard[3, 2] = 0;
            gameBoard[3, 0] = 2;
        }
        
        if (gameBoard[0, 1] == 2 && IsJumpPieceAtPos(1, 2))
        {
            int jumpPiece = GetPieceAtTile(1, 2);
            gameBoard[1, 2] = 0;
            gameBoard[0, 2] = jumpPiece;

            gameBoard[0, 1] = 0;
            gameBoard[0, 3] = 2;
        }
        
        if (gameBoard[2, 3] == 2 && IsJumpPieceAtPos(1, 2))
        {
            int jumpPiece = GetPieceAtTile(1, 2);
            gameBoard[1, 2] = 0;
            gameBoard[1, 3] = jumpPiece;

            gameBoard[2, 3] = 0;
            gameBoard[0, 3] = 2;
        }
    }

    public GameOutcome GetGameOutcome()
    {
        int winner = GetWinner();
        
        if(winner == 1)
        {
            return GameOutcome.PLAYER1;
        }
        if(winner == -1)
        {
            return GameOutcome.PLAYER2;
        }

        return GameOutcome.UNDETERMINED;
    }
    
    private int GetWinner(){
        if (gameBoard[0, 3] == 1 || gameBoard[3, 0] == 1)
        {
            return 1;
        }
        else if (gameBoard[0, 3] == 2 || gameBoard[3, 0] == 2)
        {
            return -1;
        }
        
        return 0;
    } 

    public float GetCurrentWinner()
    {
         //return 1 if player 1 piece is in win tile
        if (gameBoard[3, 0] == 1 || gameBoard[0, 3] == 1)
        {
            return 1;
        }
        
        //return -1 if player 2 piece is in win tile
        if (gameBoard[3, 0] == 2 || gameBoard[0, 3] == 2)
        {
            return -1;
        }
        
        //return 0.8 if player 1 piece is adjacent to win tile and player 2 piece isn't in a similar or better position
        if (gameBoard[2, 0] == 1 || gameBoard[3, 1] == 1 || gameBoard[0, 2] == 1 || gameBoard[1, 3] == 1
        && gameBoard[2, 0] != 2 && gameBoard[3, 1] != 2 && gameBoard[0, 2] != 2 && gameBoard[1, 3] != 2)
        {
            return 0.8f;
        }
        
        //return -0.8 if player 2 piece is adjacent to win tile and player 1 piece isn't in a similar or better position
        if (gameBoard[2, 0] == 2 || gameBoard[3, 1] == 2 || gameBoard[0, 2] == 2 || gameBoard[1, 3] == 2
        && gameBoard[2, 0] != 1 && gameBoard[3, 1] != 1 && gameBoard[0, 2] != 1 && gameBoard[1, 3] != 1)
        {
            return -0.8f;
        }
        
        //return 0.5 if player 1 piece is 2 tiles away from win tile (then can jump over a piece to win) and player 2
        //isn't in a better position
        if (gameBoard[1, 0] == 1 || gameBoard[0, 1] == 1 || gameBoard[2, 3] == 1 || gameBoard[3, 2] == 1
        && gameBoard[1, 0] != 2 && gameBoard[0, 1] != 2 && gameBoard[2, 3] != 2 && gameBoard[3, 2] != 2
        && gameBoard[2, 0] != 2 && gameBoard[3, 1] != 2 && gameBoard[0, 2] != 2 && gameBoard[1, 3] != 2)
        {
            return 0.5f;
        }
        
        //return -0.5 if player 2 piece is 2 tiles away from win tile (then can jump over a piece to win) and player 1
        //isn't in a better position
        if (gameBoard[1, 0] == 2 || gameBoard[0, 1] == 2 || gameBoard[2, 3] == 2 || gameBoard[3, 2] == 2
        && gameBoard[1, 0] != 1 && gameBoard[0, 1] != 1 && gameBoard[2, 3] != 1 && gameBoard[3, 2] != 1
        && gameBoard[2, 0] != 1 && gameBoard[3, 1] != 1 && gameBoard[0, 2] != 1 && gameBoard[1, 3] != 1)
        {
            return -0.5f;
        }
        
        //return 0.2 if player 1 is in a tile diagonal to win tile and player 2 isn't in a better position
        if (gameBoard[1, 2] == 1 || gameBoard[2, 1] == 1
        && gameBoard[1, 2] != 2 && gameBoard[2, 1] != 2
        && gameBoard[1, 0] != 2 && gameBoard[0, 1] != 2 && gameBoard[2, 3] != 2 && gameBoard[3, 2] != 2
        && gameBoard[2, 0] != 2 && gameBoard[3, 1] != 2 && gameBoard[0, 2] != 2 && gameBoard[1, 3] != 2)
        {
            return 0.2f;
        }
        
        //return -0.2 if player 2 is in a tile diagonal to win tile and player 1 isn't in a better position
        if (gameBoard[1, 2] == 2 || gameBoard[2, 1] == 2
        && gameBoard[1, 2] != 1 && gameBoard[2, 1] != 1
        && gameBoard[1, 0] != 1 && gameBoard[0, 1] != 1 && gameBoard[2, 3] != 1 && gameBoard[3, 2] != 1
        && gameBoard[2, 0] != 1 && gameBoard[3, 1] != 1 && gameBoard[0, 2] != 1 && gameBoard[1, 3] != 1)
        {
            return -0.2f;
        }
        
        return 0;
    }
}
