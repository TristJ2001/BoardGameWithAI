using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance { get; private set; }

    private BoardRepresentation _boardRepresentation;
    public BoardRepresentation _BoardRepresentation => _boardRepresentation;

    private BoardEvaluator _boardEvaluator;

    private GameOutcome outcome = GameOutcome.UNDETERMINED;

    public GameOutcome Outcome
    {
        get { return outcome; }
    }

    private MainMenuController _mainMenuController;

    private GameObject[,] GameTiles;
    private int[,] gameBoard;
    
    [SerializeField] public int xLength = 4;
    [SerializeField] public int yLength = 4;

    [SerializeField] private GameObject gameTile;
    [SerializeField] private GameObject player1Piece;
    [SerializeField] private GameObject player2Piece;
    [SerializeField] private GameObject jumpPiece;

    private Vector2Int playerPiece1PreviousPos;

    public Vector2Int PlayerPiece1PreviousPos
    {
        get { return playerPiece1PreviousPos; }
        set => playerPiece1PreviousPos = value;
    }
    
    private Vector2Int playerPiece2PreviousPos;
    public Vector2Int PlayerPiece2PreviousPos
    {
        get { return playerPiece2PreviousPos; }
    }
    
    public Vector2Int jumpPiece1PreviousPos;
    public Vector2Int jumpPiece2PreviousPos;
    public Vector2Int jumpPiece3PreviousPos;
    public Vector2Int jumpPiece4PreviousPos;

    [SerializeField] private TMP_Text playerTurnText;
    [SerializeField] private GameObject rulesObject;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button restartButton;

    private bool showingRules;
    public bool isChoosing;
    
    int playerTurn;
    IAIPlayer randomAIPlayer = new RandomAIPlayer();
    private IAIPlayer miniMaxAIPlayer = new MiniMaxAIPlayer(new BoardEvaluator(), 2);
    private IAIPlayer hardMiniMaxAIPlayer = new HardMiniMaxAIPlayer(new BoardEvaluator(), 2);

    private QLRepresentation qlRepresentation;
    private QLAI qlAgent;
    [SerializeField] private int numTrainingSessions = 100;
    [Range(0,1)] [SerializeField] private float learningRate = 0.5f;
    [Range(0,1)] [SerializeField] private float discountFactor = 0.5f;
    
    private bool player1Turn;
    public bool Player1Turn
    {
        get { return player1Turn; }
        set => player1Turn = value;
    }

    private bool hasMovedJump;
    public bool HasMovedJump
    {
        get { return hasMovedJump; }
        set => hasMovedJump = value;
    }

    private bool hasMovedPlayerPiece;

    public bool HasMovedPlayerPiece
    {
        get { return hasMovedPlayerPiece; }
        set => hasMovedPlayerPiece = value;
    }

    private bool gameOver;
    public bool GameOver => gameOver;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }
    
    void Start()
    {
        //Declarations
        GameTiles = new GameObject[xLength,yLength];
        gameBoard = new int[4, 4];
        _boardRepresentation = new BoardRepresentation(gameBoard);
        _mainMenuController = MainMenuController.Instance;
        
        qlRepresentation = new QLRepresentation(gameBoard);
        qlAgent = new QLAI(qlRepresentation, 30);
        
        //Hiding rules and buttons
        rulesObject.gameObject.SetActive(false);
        mainMenuButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        
        player1Turn = true;
        
        //Filling GameBoard Array
        PopulateBoard();
    
        //Placing GameTiles on board
        for (int x = 0; x < xLength; x++)
        {
            for (int y = 0; y < yLength; y++)
            { 
                SpawnTile(x, y, GameTiles[x, y]);
            }
        }
    
        ColourTiles();
        PlacePlayerPieces();
    }

    private void Update()
    {
        UpdateBoard();
        outcome = _boardRepresentation.GetGameOutcome();
        
        //Hide / Show Rules
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (showingRules)
            {
                rulesObject.gameObject.SetActive(false);
                showingRules = false;
            }
            else
            {
                rulesObject.gameObject.SetActive(true);
                showingRules = true;
            }
        }
        
        //Display which player's turn it is
        if (!gameOver)
        {
            ChangePlayerTurnText();
        }

        //End game if player is in win tile
        if (outcome != GameOutcome.UNDETERMINED)
        {
            if (outcome == GameOutcome.PLAYER1)
            {
                GameWinner(1);
            }
            else
            {
                GameWinner(2);
            }
        }
    }

    public void OnMainMenuButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnRestartButtonClicked()
    {
        SceneManager.LoadScene("Game");
    }

    private void OnEnable()
    {
        BoardRepresentation.OnPreviousPositionChangedAction += OnPreviousPositionChangedAction;
        QLRepresentation.OnPreviousPositionChangedAction += OnPreviousPositionChangedAction;
    }
    
    private void OnDisable()
    {
        BoardRepresentation.OnPreviousPositionChangedAction -= OnPreviousPositionChangedAction;
        QLRepresentation.OnPreviousPositionChangedAction -= OnPreviousPositionChangedAction;
    }
    
    private void OnPreviousPositionChangedAction(int piece, Vector2Int prevPos)
    {
        if (piece == 1)
        {
            playerPiece1PreviousPos = prevPos;
        }
        
        if (piece == 2)
        {
            playerPiece2PreviousPos = prevPos;
        }
        
        if (piece == 4)
        {
            jumpPiece1PreviousPos = prevPos;
        }
        
        if (piece == 5)
        {
            jumpPiece2PreviousPos = prevPos;
        }
        
        if (piece == 6)
        {
            jumpPiece3PreviousPos = prevPos;
        }
        
        if (piece == 7)
        {
            jumpPiece4PreviousPos = prevPos;
        }
    }

    //Updates position of gameObjects based on their position in representation
    private void UpdateBoard()
    {
        for (int x = 0; x < _boardRepresentation.GameBoard.GetLength(0); x++)
        {
            for (int y = 0; y < _boardRepresentation.GameBoard.GetLength(1); y++)
            {
                if (_boardRepresentation.GameBoard[x, y] == 1)
                {
                    GameObject playerPiece1 = GameObject.FindGameObjectWithTag("Player1");
                    playerPiece1.transform.position = new Vector3(x, y, -1);
                }

                if (_boardRepresentation.GameBoard[x, y] == 2)
                {
                    GameObject playerPiece2 = GameObject.FindGameObjectWithTag("Player2");
                    playerPiece2.transform.position = new Vector3(x, y, -1);
                }
                
                if (_boardRepresentation.GameBoard[x, y] == 4)
                {
                    GameObject jumpPiece1 = GameObject.FindGameObjectWithTag("JumpPiece1");
                    jumpPiece1.transform.position = new Vector3(x, y, -1);
                }
                
                if (_boardRepresentation.GameBoard[x, y] == 5)
                {
                    GameObject jumpPiece2 = GameObject.FindGameObjectWithTag("JumpPiece2");
                    jumpPiece2.transform.position = new Vector3(x, y, -1);
                }
                
                if (_boardRepresentation.GameBoard[x, y] == 6)
                {
                    GameObject jumpPiece3 = GameObject.FindGameObjectWithTag("JumpPiece3");
                    jumpPiece3.transform.position = new Vector3(x, y, -1);
                }
                
                if (_boardRepresentation.GameBoard[x, y] == 7)
                {
                    GameObject jumpPiece4 = GameObject.FindGameObjectWithTag("JumpPiece4");
                    jumpPiece4.transform.position = new Vector3(x, y, -1);
                }
            }
        }
    }
    
    //Placing pieces on the board
    private void PlacePlayerPieces()
    {
        //Player 1 Piece 
        InstantiatePiece(1, 0, 0);

        //Player 2 Piece 
        InstantiatePiece(2, xLength - 1, yLength - 1);
        
        //Jump Piece
        InstantiatePiece(3, 1, 1);
        
        //Jump Piece 2
        InstantiatePiece(4, 2, 2);
        
        //JumpPiece 3
        InstantiatePiece(5, 1, yLength - 1);
        
        //Jump Piece 4
        InstantiatePiece(6, 2, 0);
    }
    
    private void InstantiatePiece(int player, int xPos, int yPos)
    {
        //Create new gameObject and set its position to the tile at xPos, yPos
        GameObject prefab = new GameObject();

        Vector3 piecePos = new Vector3(GameTiles[xPos, yPos].transform.position.x,
            GameTiles[xPos, yPos].transform.position.y, -1);

        //Player 1
        if (player == 1)
        {
            prefab = player1Piece;
            
            //Place piece in the array representation of the board
            _boardRepresentation.PlacePiece(1, xPos, yPos);
            
            GameObject newPiece = Instantiate(prefab, piecePos, Quaternion.identity);
            newPiece.name = $"Player{player}";
            newPiece.tag = newPiece.name;
        }
        //Player 2
        else if(player == 2)
        {
            prefab = player2Piece;
            _boardRepresentation.PlacePiece(2, xPos, yPos);
            
            GameObject newPiece = Instantiate(prefab, piecePos, Quaternion.identity);
            newPiece.name = $"Player{player}";
            newPiece.tag = newPiece.name;
        }
        //Jump Piece 1
        else if (player == 3)
        {
            prefab = jumpPiece;
            _boardRepresentation.PlacePiece(4, xPos, yPos);
            
            GameObject newPiece = Instantiate(prefab, piecePos, Quaternion.identity);
            newPiece.name = "JumpPiece1";
            newPiece.tag = newPiece.name;
        }
        //Jump Piece 2
        else if (player == 4)
        {
            prefab = jumpPiece;
            _boardRepresentation.PlacePiece(5, xPos, yPos);
            
            GameObject newPiece = Instantiate(prefab, piecePos, Quaternion.identity);
            newPiece.name = "JumpPiece2";
            newPiece.tag = newPiece.name;
        }
        //Jump Piece 3
        else if (player == 5)
        {
            prefab = jumpPiece;
            _boardRepresentation.PlacePiece(6, xPos, yPos);
            
            GameObject newPiece = Instantiate(prefab, piecePos, Quaternion.identity);
            newPiece.name = "JumpPiece3";
            newPiece.tag = newPiece.name;
        }
        //Jump Piece 4
        else if (player == 6)
        {
            prefab = jumpPiece;
            _boardRepresentation.PlacePiece(7, xPos, yPos);
            
            GameObject newPiece = Instantiate(prefab, piecePos, Quaternion.identity);
            newPiece.name = "JumpPiece4";
            newPiece.tag = newPiece.name;
        }
    }
    
    //Placing empty and win tiles
    private void PopulateBoard()
    {
        for (int x = 0; x < xLength; x++)
        {
            for (int y = 0; y < yLength; y++)
            {
                GameTiles[x, y] = gameTile;
                if (x == 0 && y == yLength - 1 || x == xLength - 1 && y == 0)
                {
                    _boardRepresentation.PlacePiece( 3, x, y);
                }
                else
                {
                    _boardRepresentation.PlacePiece(0, x, y);
                }
            }
        }
    }

    private void SpawnTile(int x, int y, GameObject tile)
    {
        GameTiles[x, y] = Instantiate(tile, new Vector3(x, y , 0), Quaternion.identity);
        GameTiles[x, y].name = $"GameTile[{x}, {y}]";
    }
    
    private void ColourTiles()
    {
        //Creating checkerboard pattern
        for (int i = 0; i < yLength; i++)
        {
            for (int j = 0; j < xLength; j++)
            {
                if (IsEven(i) && IsEven(j))
                {
                    GameTiles[i, j].GetComponent<Renderer>().material.color = Color.black;
                } 
                if(!IsEven(i) && !IsEven(j))
                {
                    GameTiles[i, j].GetComponent<Renderer>().material.color = Color.black;
                }
            }
        }
        
        //Setting colour of goal tiles
        GameTiles[0, yLength - 1].GetComponent<Renderer>().material.color = Color.green;
        GameTiles[xLength - 1, 0].GetComponent<Renderer>().material.color = Color.green;
    }
    
    private bool IsEven(int num)
    {
        if (num % 2 == 0)
        {
            return true;
        } 
        return false;
    }

    //Method to update board representation 
    //player: 1 = player1, 2 = player2, 4 = jump1, 5 = jump2, 6 = jump3, 7 = jump4
    public void MovePiece(int player, int oldXPos, int oldYPos, int newXPos, int newYPos)
    {
        if (player == 1)
        {
            if (_boardRepresentation.GetPieceAtTile(newXPos, newYPos) == /*4*/ 3)
            {
                GameWinner(1);
            }

            //Set previous position of piece
            playerPiece1PreviousPos = new Vector2Int(oldXPos, oldYPos);
            _boardRepresentation.SetPreviousPosition(1, playerPiece1PreviousPos);
            
            //Set piece's original position to 0 and new position to its integer representation
            _boardRepresentation.PlacePiece(0, oldXPos, oldYPos);
            _boardRepresentation.PlacePiece(1, newXPos, newYPos);

            outcome = _boardRepresentation.GetGameOutcome();
            
            //Starts AI coroutine if player's turn has ended
            if (outcome == GameOutcome.UNDETERMINED && hasMovedJump)
            {
                if (_mainMenuController.singleplayer && !_mainMenuController.qLearning)
                {
                    if (_mainMenuController.hardDifficulty == 2)
                    {
                        StartCoroutine(HardMiniMaxAITurnCoroutine());
                    }
                    else if (_mainMenuController.hardDifficulty == 1)
                    {
                        StartCoroutine(MiniMaxAITurnCoroutine());
                    }
                    else
                    {
                        StartCoroutine(RandomAITurnCoroutine());
                    }
                }
                else if (!_mainMenuController.singleplayer && _mainMenuController.qLearning)
                {
                    StartCoroutine(QlaiTurnCoroutine(1f));
                }
            }
        }
        else if (player == 2)
        {
            if (_boardRepresentation.GetPieceAtTile(newXPos, newYPos) == /*4*/ 3)
            {
                Debug.Log("Player 2 Wins!");
                GameWinner(2);
            }
            
            playerPiece2PreviousPos = new Vector2Int(oldXPos, oldYPos);
            _boardRepresentation.SetPreviousPosition(2, playerPiece2PreviousPos);
            
            _boardRepresentation.PlacePiece(0, oldXPos, oldYPos);
            _boardRepresentation.PlacePiece(2, newXPos, newYPos);
            
            outcome = _boardRepresentation.GetGameOutcome();
            
            if (outcome == GameOutcome.UNDETERMINED && hasMovedJump)
            {
                if (_mainMenuController.singleplayer && !_mainMenuController.qLearning)
                {
                    if (_mainMenuController.hardDifficulty == 2)
                    {
                        StartCoroutine(HardMiniMaxAITurnCoroutine());
                    }
                    else if (_mainMenuController.hardDifficulty == 1)
                    {
                        StartCoroutine(MiniMaxAITurnCoroutine());
                    }
                    else
                    {
                        StartCoroutine(RandomAITurnCoroutine());
                    }
                }
                else if (!_mainMenuController.singleplayer && _mainMenuController.qLearning)
                {
                    StartCoroutine(QlaiTurnCoroutine(1f));
                }
            }
        }
        else if (player == 4)
        {
            jumpPiece1PreviousPos = new Vector2Int(oldXPos, oldYPos);
            _boardRepresentation.SetPreviousPosition(4, jumpPiece1PreviousPos);
            
            _boardRepresentation.PlacePiece(0, oldXPos, oldYPos);
            _boardRepresentation.PlacePiece(/*3*/ 4, newXPos, newYPos);
            
            if (outcome == GameOutcome.UNDETERMINED && hasMovedPlayerPiece)
            {
                if (_mainMenuController.singleplayer && !_mainMenuController.qLearning)
                {
                    if (_mainMenuController.hardDifficulty == 2)
                    {
                        StartCoroutine(HardMiniMaxAITurnCoroutine());
                    }
                    else if (_mainMenuController.hardDifficulty == 1)
                    {
                        StartCoroutine(MiniMaxAITurnCoroutine());
                    }
                    else
                    {
                        StartCoroutine(RandomAITurnCoroutine());
                    }
                }
                else if (!_mainMenuController.singleplayer && _mainMenuController.qLearning)
                {
                    StartCoroutine(QlaiTurnCoroutine(1f));
                }
            }
        }
        else if (player == 5)
        {
            jumpPiece2PreviousPos = new Vector2Int(oldXPos, oldYPos);
            _boardRepresentation.SetPreviousPosition(5, jumpPiece2PreviousPos);
            
            _boardRepresentation.PlacePiece(0, oldXPos, oldYPos);
            _boardRepresentation.PlacePiece(5, newXPos, newYPos);
            
            if (outcome == GameOutcome.UNDETERMINED && hasMovedPlayerPiece)
            {
                if (_mainMenuController.singleplayer && !_mainMenuController.qLearning)
                {
                    if (_mainMenuController.hardDifficulty == 2)
                    {
                        StartCoroutine(HardMiniMaxAITurnCoroutine());
                    }
                    else if (_mainMenuController.hardDifficulty == 1)
                    {
                        StartCoroutine(MiniMaxAITurnCoroutine());
                    }
                    else
                    {
                        StartCoroutine(RandomAITurnCoroutine());
                    }
                }
                else if (!_mainMenuController.singleplayer && _mainMenuController.qLearning)
                {
                    StartCoroutine(QlaiTurnCoroutine(1f));
                }
            }
        }
        else if (player == 6)
        {
            jumpPiece3PreviousPos = new Vector2Int(oldXPos, oldYPos);
            _boardRepresentation.SetPreviousPosition(6, jumpPiece3PreviousPos);
            
            _boardRepresentation.PlacePiece(0, oldXPos, oldYPos);
            _boardRepresentation.PlacePiece(6, newXPos, newYPos);
            
            if (outcome == GameOutcome.UNDETERMINED && hasMovedPlayerPiece)
            {
                if (_mainMenuController.singleplayer && !_mainMenuController.qLearning)
                {
                    if (_mainMenuController.hardDifficulty == 2)
                    {
                        StartCoroutine(HardMiniMaxAITurnCoroutine());
                    }
                    else if (_mainMenuController.hardDifficulty == 1)
                    {
                        StartCoroutine(MiniMaxAITurnCoroutine());
                    }
                    else
                    {
                        StartCoroutine(RandomAITurnCoroutine());
                    }
                }
                else if (!_mainMenuController.singleplayer && _mainMenuController.qLearning)
                {
                    StartCoroutine(QlaiTurnCoroutine(1f));
                }
            }
        }
        else if (player == 7)
        {
            jumpPiece4PreviousPos = new Vector2Int(oldXPos, oldYPos);
            _boardRepresentation.SetPreviousPosition(7, jumpPiece4PreviousPos);
            
            _boardRepresentation.PlacePiece(0, oldXPos, oldYPos);
            _boardRepresentation.PlacePiece(7, newXPos, newYPos);
            
            if (outcome == GameOutcome.UNDETERMINED && hasMovedPlayerPiece)
            {
                if (_mainMenuController.singleplayer && !_mainMenuController.qLearning)
                {
                    if (_mainMenuController.hardDifficulty == 2)
                    {
                        StartCoroutine(HardMiniMaxAITurnCoroutine());
                    }
                    else if (_mainMenuController.hardDifficulty == 1)
                    {
                        StartCoroutine(MiniMaxAITurnCoroutine());
                    }
                    else
                    {
                        StartCoroutine(RandomAITurnCoroutine());
                    }
                }
                else if (!_mainMenuController.singleplayer && _mainMenuController.qLearning)
                {
                    StartCoroutine(QlaiTurnCoroutine(1f));
                }
            }
        }
    }
    
    private void GameWinner(int player)
    {
        playerTurnText.SetText($"Player {player} Wins!");
        gameOver = true;

        mainMenuButton.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
    }

    private void ChangePlayerTurnText()
    {
        if (player1Turn)
        {
            if (!hasMovedJump && !hasMovedPlayerPiece)
            {
                playerTurnText.SetText("Player1's Turn \n\nMove Jump Piece \n\nMove Player Piece");
            }
            else if(hasMovedJump && !hasMovedPlayerPiece)
            {
                playerTurnText.SetText("Player1's Turn \n\nMove Player Piece");
            }
            else if (!hasMovedJump && hasMovedPlayerPiece)
            {
                playerTurnText.SetText("Player1's Turn \n\nMove Jump Piece");
            }
        }
        else
        {
            if (!hasMovedJump && !hasMovedPlayerPiece)
            {
                playerTurnText.SetText("Player2's Turn \n\nMove Jump Piece \n\nMove Player Piece");
            }
            else if(hasMovedJump && !hasMovedPlayerPiece)
            {
                playerTurnText.SetText("Player2's Turn \n\nMove Player Piece");
            }
            else if (!hasMovedJump && hasMovedPlayerPiece)
            {
                playerTurnText.SetText("Player2's Turn \n\nMove Jump Piece");
            }
        }
    }

    private void MakeMove(Move move)
    {
        Vector2Int jumpPos = _boardRepresentation.FindPositionOfPiece(move.JumpPiece);
        Vector2Int playerPos = _boardRepresentation.FindPositionOfPiece(move.PlayerPiece);

        if (move.PlayerPiece == 1)
        {
            playerPiece1PreviousPos = playerPos;
        }
        if (move.PlayerPiece == 2)
        {
            playerPiece2PreviousPos = playerPos;
        }

        if (move.JumpPiece == 4)
        {
            jumpPiece1PreviousPos = jumpPos;
        }
        if (move.JumpPiece == 5)
        {
            jumpPiece2PreviousPos = jumpPos;
        }
        if (move.JumpPiece == 6)
        {
            jumpPiece3PreviousPos = jumpPos;
        }
        if (move.JumpPiece == 7)
        {
            jumpPiece4PreviousPos = jumpPos;
        }

        _boardRepresentation.MakeMove(move, move.JumpPiece, move.PlayerPiece);
    }
    
    IEnumerator HardMiniMaxAITurnCoroutine(){
        yield return new WaitForSeconds(1f);
        Move move = hardMiniMaxAIPlayer.GetMove(_boardRepresentation, playerTurn);

        if(move != null){
            MakeMove(move);
        }

        player1Turn = true;
        _boardRepresentation.ToString();
    }
    
    IEnumerator MiniMaxAITurnCoroutine(){
        Debug.Log("Minimax Turn");
        yield return new WaitForSeconds(1f);
        Move move = miniMaxAIPlayer.GetMove(_boardRepresentation, playerTurn);

        if(move != null){
            MakeMove(move);
        }

        player1Turn = true;
        _boardRepresentation.ToString();
    }
    
    IEnumerator RandomAITurnCoroutine(){
        Debug.Log("Random Turn");
        yield return new WaitForSeconds(1f);
        Move move = randomAIPlayer.GetMove(_boardRepresentation, playerTurn);

        if(move != null){
            MakeMove(move);
        }

        player1Turn = true;
        _boardRepresentation.ToString();
    }
    
    private IEnumerator QlaiTurnCoroutine(float delay = 1f)
    {
        Debug.Log("QLearning Turn");
        qlAgent.Train(numTrainingSessions, learningRate, discountFactor);
        
        yield return new WaitForSeconds(delay);
        Move move = qlAgent.GetMove(qlRepresentation, 2);

        if (move == null)
        {
            
        }
        else if (qlRepresentation.MakeMove(move, move.JumpPiece, move.PlayerPiece))
        {
            MakeMove(move);
        }

        player1Turn = true;
    }
    }

