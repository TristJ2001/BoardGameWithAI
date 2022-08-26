using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceController : MonoBehaviour
{
    private static PieceController instance;
    
    public delegate void OnFinishedChoosing();
    public static event OnFinishedChoosing OnFinishedChoosingAction;

    [SerializeField] private GameObject markerPrefab;

    private Vector3 position;
    private int xPos;
    private int yPos;

    private int previousXPos;
    private int previousYPos;

    private GameManager _gameManager;
    private BoardRepresentation _boardRepresentation;

    void Start()
    {
        //Declarations
        _gameManager = GameManager._instance;
        _boardRepresentation = _gameManager._BoardRepresentation;
        
        //Setting the position of the gameObject
        position = gameObject.transform.position;
        xPos = (int)position.x;
        yPos = (int)position.y;

        previousXPos = xPos;
        previousYPos = yPos;
    }

    private void OnEnable()
    {
        MarkerController.OnMarkerClickedAction += OnMarkerClickedAction;
    }

    private void OnDisable()
    {
        MarkerController.OnMarkerClickedAction -= OnMarkerClickedAction;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePosition2D = new Vector2(mousePosition.x, mousePosition.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePosition2D, Vector2.zero);

            if (hit.collider == null)
            {
                OnFinishedChoosingAction?.Invoke();
            }
            else
            {
                //Check if the player clicked on this gameObject
                if (hit.transform.name == name)
                {
                    //Delete markers attached to any other gameObject
                    if (_gameManager.isChoosing)
                    {
                        OnFinishedChoosingAction?.Invoke();
                    }

                    //Will not spawn markers if the game is over
                    if (_gameManager.GameOver)
                    {
                        return;
                    }
                    
                    SpawnMarkers();
                }
            }
        }
    }

    private void SpawnMarkers()
    {
        _gameManager.isChoosing = true;
        
        position = gameObject.transform.position;
        xPos = (int)position.x;
        yPos = (int)position.y;

        int xStart = xPos - 1;
        int xEnd = xPos + 1;

        int yStart = yPos - 1;
        int yEnd = yPos + 1;

        //Will not spawn player piece markers if player has already moved player piece
        if (tag.Substring(0, 6) == "Player")
        {
            if (_gameManager.HasMovedPlayerPiece)
            {
                return;
            }
        }

        //Will not spawn jump piece markers if player has already moved jump
        if (tag.Substring(0, 4) == "Jump")
        {
            if (_gameManager.HasMovedJump)
            {
                return;
            }
        }

        for (int i = xStart; i <= xEnd; i++)
        {
            //Check if markers will spawn outside left or right of map
            if (i < 0 || i >= _gameManager.xLength)
            {
                continue;
            }
            
            for(int j = yStart; j <= yEnd; j++)
            {
                //Check if markers will spawn outside top or bottom of map
                if (j < 0 || j >= _gameManager.yLength)
                {
                    continue;
                }
                
                //Skip the tile that the piece is currently in
                if (Math.Abs(i - gameObject.transform.position.x) < 1 && Math.Abs(j - gameObject.transform.position.y) < 1)
                {
                    continue;
                }

                //Skip diagonal tiles
                if (i == xStart && j == yEnd || i == xEnd && j == yEnd ||
                    i == xStart && j == yStart || i == xEnd && j == yStart)
                {
                    continue;
                }

                //Skip tiles that already have a piece in them
                if (_boardRepresentation.GetPieceAtTile(i, j) == 1 
                    || _boardRepresentation.GetPieceAtTile(i, j) == 2 
                    || _boardRepresentation.GetPieceAtTile(i, j) == 4
                    || _boardRepresentation.GetPieceAtTile(i, j) == 5
                    || _boardRepresentation.GetPieceAtTile(i, j) == 6
                    || _boardRepresentation.GetPieceAtTile(i, j) == 7)
                {
                    continue;
                }

                //Skip piece's previous position
                if(CompareTag("Player1") && i == _gameManager.PlayerPiece1PreviousPos.x 
                                         && j == _gameManager.PlayerPiece1PreviousPos.y)
                {
                    continue;
                }
                
                if(CompareTag("Player2") && i == _gameManager.PlayerPiece2PreviousPos.x 
                                         && j == _gameManager.PlayerPiece2PreviousPos.y)
                {
                    continue;
                }
                
                if(CompareTag("JumpPiece1") && i == _gameManager.jumpPiece1PreviousPos.x 
                                            && j == _gameManager.jumpPiece1PreviousPos.y)
                {
                    continue;
                }
                
                if(CompareTag("JumpPiece2") && i == _gameManager.jumpPiece2PreviousPos.x 
                                            && j == _gameManager.jumpPiece2PreviousPos.y)
                {
                    continue;
                }
                
                if(CompareTag("JumpPiece3") && i == _gameManager.jumpPiece3PreviousPos.x 
                                            && j == _gameManager.jumpPiece3PreviousPos.y)
                {
                    continue;
                }
                
                if(CompareTag("JumpPiece4") && i == _gameManager.jumpPiece4PreviousPos.x 
                                            && j == _gameManager.jumpPiece4PreviousPos.y)
                {
                    continue;
                }
                
                //Make sure jump pieces can't go into goal tiles
                if (tag.Substring(0, 4) == "Jump")
                {
                    if (_boardRepresentation.GetPieceAtTile(i, j) == /*4*/ 3)
                    {
                        continue;
                    }
                }

                PlaceMarker(i, j);
            }
            
            //Check if jump piece is in adjacent tile. If true, place marker on the opposite side
            CheckIfNextToJump();
        }
    }

    private void CheckIfNextToJump()
    {
         if (CompareTag("Player1"))
         {
             if (yPos + 1 < _gameManager.yLength && 
                 _boardRepresentation.IsJumpPieceAtPos(xPos, yPos+ 1))
             { 
                 if (xPos == _gameManager.PlayerPiece1PreviousPos.x && yPos + 2 == _gameManager.PlayerPiece1PreviousPos.y 
                   || yPos + 2 >= _gameManager.yLength
                   || _boardRepresentation.IsJumpPieceAtPos(xPos, yPos + 2)
                   || _boardRepresentation.GetPieceAtTile(xPos, yPos + 2) == 1
                   || _boardRepresentation.GetPieceAtTile(xPos, yPos + 2) == 2)
                 {
                     
                 }
                 else
                 {
                     PlaceMarker(xPos, yPos + 2);
                     // CheckIfPLayerCanJumpAgain(xPos, yPos + 2);
                 }
             }
                
             if (yPos - 1 > 0 && 
                 _boardRepresentation.IsJumpPieceAtPos(xPos, yPos - 1))
             { 
                 if (xPos == _gameManager.PlayerPiece1PreviousPos.x && yPos - 2 == _gameManager.PlayerPiece1PreviousPos.y
                   || yPos - 2 < 0
                   || _boardRepresentation.IsJumpPieceAtPos(xPos, yPos - 2)
                   || _boardRepresentation.GetPieceAtTile(xPos, yPos - 2) == 1 
                   || _boardRepresentation.GetPieceAtTile(xPos, yPos - 2) == 2)
                 {
                     
                 }
                 else
                 {
                     PlaceMarker(xPos, yPos - 2);
                     // CheckIfPLayerCanJumpAgain(xPos, yPos - 2);
                 }
             }
                
             if (xPos - 1 > 0 && 
                 _boardRepresentation.IsJumpPieceAtPos(xPos - 1, yPos))
             { 
                 if (xPos - 2 == _gameManager.PlayerPiece1PreviousPos.x && yPos == _gameManager.PlayerPiece1PreviousPos.y 
                   || xPos - 2 < 0
                   || _boardRepresentation.IsJumpPieceAtPos(xPos - 2, yPos)
                   ||_boardRepresentation.GetPieceAtTile(xPos - 2, yPos) == 1
                   ||_boardRepresentation.GetPieceAtTile(xPos - 2, yPos) == 2)
                 { 
                     
                 }
                 else
                 {
                     PlaceMarker(xPos - 2, yPos);
                     // CheckIfPLayerCanJumpAgain(xPos - 2, yPos);
                 }
             }
                
             if (xPos + 1 < _gameManager.xLength && 
                 _boardRepresentation.IsJumpPieceAtPos(xPos + 1, yPos))
             { 
                 if (xPos + 2 == _gameManager.PlayerPiece1PreviousPos.x && yPos == _gameManager.PlayerPiece1PreviousPos.y 
                   || xPos + 2 >= _gameManager.xLength
                   || _boardRepresentation.IsJumpPieceAtPos(xPos + 2, yPos)
                   || _boardRepresentation.GetPieceAtTile(xPos + 2, yPos) == 1
                   || _boardRepresentation.GetPieceAtTile(xPos + 2, yPos) == 2)
                 {
                     
                 }
                 else
                 {
                     PlaceMarker(xPos + 2, yPos);
                     // CheckIfPLayerCanJumpAgain(xPos + 2, yPos);
                 }
             }
         }
         
         if (CompareTag("Player2"))
         {
             if (yPos + 1 < _gameManager.yLength && 
                 _boardRepresentation.IsJumpPieceAtPos(xPos, yPos+ 1))
             { 
                 if (xPos == _gameManager.PlayerPiece2PreviousPos.x && yPos + 2 == _gameManager.PlayerPiece2PreviousPos.y 
                   || yPos + 2 >= _gameManager.yLength
                   || _boardRepresentation.IsJumpPieceAtPos(xPos, yPos + 2)
                   || _boardRepresentation.GetPieceAtTile(xPos, yPos + 2) == 1
                   || _boardRepresentation.GetPieceAtTile(xPos, yPos + 2) == 2)
                 {
                     
                 }
                 else
                 {
                     PlaceMarker(xPos, yPos + 2);
                     // CheckIfPLayerCanJumpAgain(xPos, yPos + 2);
                 }
             }
                
             if (yPos - 1 > 0 && 
                 _boardRepresentation.IsJumpPieceAtPos(xPos, yPos - 1))
             { 
                 if (xPos == _gameManager.PlayerPiece2PreviousPos.x && yPos - 2 == _gameManager.PlayerPiece2PreviousPos.y
                   || yPos - 2 < 0
                   || _boardRepresentation.IsJumpPieceAtPos(xPos, yPos - 2)
                   || _boardRepresentation.GetPieceAtTile(xPos, yPos - 2) == 1 
                   || _boardRepresentation.GetPieceAtTile(xPos, yPos - 2) == 2)
                 {
                     
                 }
                 else
                 {
                     PlaceMarker(xPos, yPos - 2);
                     // CheckIfPLayerCanJumpAgain(xPos, yPos - 2);
                 }
             }
                
             if (xPos - 1 > 0 && 
                 _boardRepresentation.IsJumpPieceAtPos(xPos - 1, yPos))
             { 
                 if (xPos - 2 == _gameManager.PlayerPiece2PreviousPos.x && yPos == _gameManager.PlayerPiece2PreviousPos.y 
                   || xPos - 2 < 0
                   || _boardRepresentation.IsJumpPieceAtPos(xPos - 2, yPos)
                   ||_boardRepresentation.GetPieceAtTile(xPos - 2, yPos) == 1
                   ||_boardRepresentation.GetPieceAtTile(xPos - 2, yPos) == 2)
                 { 
                     
                 }
                 else
                 {
                     PlaceMarker(xPos - 2, yPos);
                     // CheckIfPLayerCanJumpAgain(xPos - 2, yPos);
                 }
             }
                
             if (xPos + 1 < _gameManager.xLength && 
                 _boardRepresentation.IsJumpPieceAtPos(xPos + 1, yPos))
             { 
                 if (xPos + 2 == _gameManager.PlayerPiece2PreviousPos.x && yPos == _gameManager.PlayerPiece2PreviousPos.y 
                   || xPos + 2 >= _gameManager.xLength
                   || _boardRepresentation.IsJumpPieceAtPos(xPos + 2, yPos)
                   || _boardRepresentation.GetPieceAtTile(xPos + 2, yPos) == 1
                   || _boardRepresentation.GetPieceAtTile(xPos + 2, yPos) == 2)
                 {
                     
                 }
                 else
                 {
                     PlaceMarker(xPos + 2, yPos);
                     // CheckIfPLayerCanJumpAgain(xPos + 2, yPos);
                 }
             }
         }
    }
    
    private void CheckIfPLayerCanJumpAgain(int xPosition, int yPosition)
    {
        if (yPosition + 1 < _gameManager.yLength && 
            _boardRepresentation.IsJumpPieceAtPos(xPosition, yPosition + 1))
        { 
            if (xPosition == previousXPos && yPosition + 2 == previousYPos 
                || yPosition + 2 >= _gameManager.yLength
                || _boardRepresentation.IsJumpPieceAtPos(xPosition, yPosition + 2)
                || _boardRepresentation.GetPieceAtTile(xPosition, yPosition + 2) == 1
                || _boardRepresentation.GetPieceAtTile(xPosition, yPosition + 2) == 2)
            {
                     
            }
            else
            {
                PlaceMarker(xPosition, yPosition + 2);
            }
        }
                
        if (yPosition - 1 > 0 && 
            _boardRepresentation.IsJumpPieceAtPos(xPosition, yPosition - 1))
        { 
            if (xPosition == previousXPos && yPosition - 2 == previousYPos
                || yPosition - 2 < 0
                || _boardRepresentation.IsJumpPieceAtPos(xPosition, yPosition - 2)
                || _boardRepresentation.GetPieceAtTile(xPosition, yPosition - 2) == 1
                || _boardRepresentation.GetPieceAtTile(xPosition, yPosition - 2) == 2)
            {
                     
            }
            else
            {
                PlaceMarker(xPosition, yPosition - 2);
            }
        }
                
        if (xPosition - 1 > 0 && 
            _boardRepresentation.IsJumpPieceAtPos(xPosition - 1, yPosition))
        { 
            if (xPosition - 2 == previousXPos && yPosition == previousYPos 
                || xPosition - 2 < 0
                || _boardRepresentation.IsJumpPieceAtPos(xPosition - 2, yPosition)
                ||_boardRepresentation.GetPieceAtTile(xPosition - 2, yPosition) == 1
                ||_boardRepresentation.GetPieceAtTile(xPosition - 2, yPosition) == 2)
            { 
                     
            }
            else
            {
                PlaceMarker(xPosition - 2, yPosition);
            }
        }
                
        if (xPosition + 1 < _gameManager.xLength && 
            _boardRepresentation.IsJumpPieceAtPos(xPosition + 1, yPosition))
        { 
            if (xPosition + 2 == previousXPos && yPosition == previousYPos 
                || xPosition + 2 >= _gameManager.xLength
                || _boardRepresentation.IsJumpPieceAtPos(xPosition + 2, yPosition)
                || _boardRepresentation.GetPieceAtTile(xPosition + 2, yPosition) == 1
                || _boardRepresentation.GetPieceAtTile(xPosition + 2, yPosition) == 2)
            {
                     
            }
            else
            {
                PlaceMarker(xPosition + 2, yPosition);
            }
        }
    }

    private void PlaceMarker(int xPosition, int yPosition)
    {
        GameObject newMarker = Instantiate(markerPrefab, new Vector3(xPosition, yPosition, -1), Quaternion.identity);

        if (name == "Player1")
        {
            newMarker.tag = "Player1Marker";
        }
        else if (name == "Player2")
        {
            newMarker.tag = "Player2Marker";
        }
        else if (name == "JumpPiece1")
        {
            newMarker.tag = "JumpPiece1Marker";
        }
        else if (name == "JumpPiece2")
        {
            newMarker.tag = "JumpPiece2Marker";
        }
        else if (name == "JumpPiece3")
        {
            newMarker.tag = "JumpPiece3Marker";
        }
        else if (name == "JumpPiece4")
        {
            newMarker.tag = "JumpPiece4Marker";
        }

        newMarker.name = $"{newMarker.tag}[{xPosition}, {yPosition}]";
    }
    
    private void OnMarkerClickedAction(int xPosition, int yPosition, string piece)
    {
        previousXPos = xPos;
        previousYPos = yPos;
        
        if (piece == "Player1Marker" && CompareTag("Player1"))
        {
            _gameManager.MovePiece(1, xPos, yPos, xPosition, yPosition);
            
            //Switch turns if player has already moved a jump piece
            if (_gameManager.Player1Turn && _gameManager.HasMovedJump)
            {
                _gameManager.Player1Turn = false;
                _gameManager.HasMovedJump = false;
                _gameManager.HasMovedPlayerPiece = false;
            }
            else if(!_gameManager.Player1Turn && _gameManager.HasMovedJump)
            {
                _gameManager.Player1Turn = true;
                _gameManager.HasMovedJump = false;
                _gameManager.HasMovedPlayerPiece = false;
            }
            else
            {
                _gameManager.HasMovedPlayerPiece = true;
            }
            
            OnFinishedChoosingAction?.Invoke();
        }
        else if (piece == "Player2Marker" && CompareTag("Player2"))
        {
            _gameManager.MovePiece(2, xPos, yPos, xPosition, yPosition);
            
            if (_gameManager.Player1Turn && _gameManager.HasMovedJump)
            {
                _gameManager.Player1Turn = false;
                _gameManager.HasMovedJump = false;
                _gameManager.HasMovedPlayerPiece = false;
            }
            else if(!_gameManager.Player1Turn && _gameManager.HasMovedJump)
            {
                _gameManager.Player1Turn = true;
                _gameManager.HasMovedJump = false;
                _gameManager.HasMovedPlayerPiece = false;
            }
            else
            {
                _gameManager.HasMovedPlayerPiece = true;
            }
            
            OnFinishedChoosingAction?.Invoke();
        }
        else if (piece == "JumpPiece1Marker" && CompareTag("JumpPiece1"))
        {
            _gameManager.MovePiece(4, xPos, yPos, xPosition, yPosition);
            
            if (_gameManager.Player1Turn && _gameManager.HasMovedPlayerPiece)
            {
                _gameManager.Player1Turn = false;
                _gameManager.HasMovedJump = false;
                _gameManager.HasMovedPlayerPiece = false;
            }
            else if(!_gameManager.Player1Turn && _gameManager.HasMovedPlayerPiece)
            {
                _gameManager.Player1Turn = true;
                _gameManager.HasMovedJump = false;
                _gameManager.HasMovedPlayerPiece = false;
            }
            else
            {
                _gameManager.HasMovedJump = true;
            }
            
            OnFinishedChoosingAction?.Invoke();
        }
        else if (piece == "JumpPiece2Marker" && CompareTag("JumpPiece2"))
        {
            _gameManager.MovePiece(5, xPos, yPos, xPosition, yPosition);
            
            if (_gameManager.Player1Turn && _gameManager.HasMovedPlayerPiece)
            {
                _gameManager.Player1Turn = false;
                _gameManager.HasMovedJump = false;
                _gameManager.HasMovedPlayerPiece = false;
            }
            else if(!_gameManager.Player1Turn && _gameManager.HasMovedPlayerPiece)
            {
                _gameManager.Player1Turn = true;
                _gameManager.HasMovedJump = false;
                _gameManager.HasMovedPlayerPiece = false;
            }
            else
            {
                _gameManager.HasMovedJump = true;
            }
            
            OnFinishedChoosingAction?.Invoke();
        }
        else if (piece == "JumpPiece3Marker" && CompareTag("JumpPiece3"))
        {
            _gameManager.MovePiece(6, xPos, yPos, xPosition, yPosition);
            
            if (_gameManager.Player1Turn && _gameManager.HasMovedPlayerPiece)
            {
                _gameManager.Player1Turn = false;
                _gameManager.HasMovedJump = false;
                _gameManager.HasMovedPlayerPiece = false;
            }
            else if(!_gameManager.Player1Turn && _gameManager.HasMovedPlayerPiece)
            {
                _gameManager.Player1Turn = true;
                _gameManager.HasMovedJump = false;
                _gameManager.HasMovedPlayerPiece = false;
            }
            else
            {
                _gameManager.HasMovedJump = true;
            }
            
            OnFinishedChoosingAction?.Invoke();
        }
        else if (piece == "JumpPiece4Marker" && CompareTag("JumpPiece4"))
        {
            _gameManager.MovePiece(7, xPos, yPos, xPosition, yPosition);
            
            if (_gameManager.Player1Turn && _gameManager.HasMovedPlayerPiece)
            {
                _gameManager.Player1Turn = false;
                _gameManager.HasMovedJump = false;
                _gameManager.HasMovedPlayerPiece = false;
            }
            else if(!_gameManager.Player1Turn && _gameManager.HasMovedPlayerPiece)
            {
                _gameManager.Player1Turn = true;
                _gameManager.HasMovedJump = false;
                _gameManager.HasMovedPlayerPiece = false;
            }
            else
            {
                _gameManager.HasMovedJump = true;
            }
            
            OnFinishedChoosingAction?.Invoke();
        }
    }
}
