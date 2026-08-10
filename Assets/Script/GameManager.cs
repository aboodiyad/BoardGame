using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject block;
    Board board;
    public GameObject piece;
    bool hasGameFinished, canMove;
    GameState gameState;
    Player currentPlayer;
    Dictionary<GamePiece, GameObject> pieceDictionary;
    GamePiece clickedPiece;

    public Action<Player, GameState> Message { get; internal set; }

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        fitCameraToBoard();
        spawnBlock();
        board = new Board();

        canMove = false;
        hasGameFinished = false;
        currentPlayer = Player.Red;
        gameState = GameState.Click;

        pieceDictionary = new Dictionary<GamePiece, GameObject>();

        foreach (var pair in board.playerPositions)
        {
            GameObject pieceObj = Instantiate(piece);
            pieceObj.transform.position = new Vector3(pair.Value.x, pair.Value.y, -2f);
            pieceObj.GetComponent<SpriteRenderer>().color = pair.Key.player == Player.Red ? Color.red : Color.blue;
            pieceDictionary[pair.Key] = pieceObj;
        }
    }

    void Update()
    {
        if (hasGameFinished) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 screenPos = Mouse.current.position.ReadValue();
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(screenPos);
            Grid clickedGrid = new Grid
            {
                x = Mathf.Clamp(Mathf.RoundToInt(mousePos.x), 0, 7),
                y = Mathf.Clamp(Mathf.RoundToInt(mousePos.y), 0, 7)
            };

            switch (gameState)
            {
                case GameState.Click:
                    canMove = false;
                    clickedPiece = board.GetPieceAtPosition(clickedGrid);
                    board.CalculateMoves(currentPlayer);
                    var moveDictionary = board.playerMoves;

                    if (moveDictionary.Count == 0)
                    {
                        hasGameFinished = true;
                        Player winner = currentPlayer == Player.Red ? Player.Blue : Player.Red;
                        Message?.Invoke(winner, GameState.Finished);
                        return;
                    }

                    if (clickedPiece.player != currentPlayer || clickedPiece.pieceNumber == -1) return;

                    foreach (var item in moveDictionary)
                        if (item.Key == clickedPiece) canMove = true;

                    if (!canMove) return;

                    Message?.Invoke(currentPlayer, GameState.Move);
                    gameState = GameState.Move;
                    break;

                case GameState.Move:
                    foreach (Moves currentMove in board.playerMoves[clickedPiece])
                    {
                        if (currentMove.end.x == clickedGrid.x && currentMove.end.y == clickedGrid.y)
                        {
                            pieceDictionary[clickedPiece].transform.position = new Vector3(currentMove.end.x, currentMove.end.y, -2f);

                            if (currentMove.isCapture)
                            {
                                pieceDictionary[currentMove.capturedPiece].SetActive(false);
                                pieceDictionary.Remove(currentMove.capturedPiece);
                            }

                            board.UpdateMove(currentMove);

                            if ((clickedPiece.player == Player.Red && currentMove.end.y == 7) ||
                                (clickedPiece.player == Player.Blue && currentMove.end.y == 0))
                            {
                                board.UpgradePiece(clickedPiece);
                                pieceDictionary[clickedPiece].transform.GetChild(0).gameObject.SetActive(true);
                            }

                            gameState = GameState.Click;
                            board.CalculateMoves(currentPlayer);

                            if (board.isCapture && currentMove.isCapture)
                            {
                                Message?.Invoke(currentPlayer, GameState.Click);
                                return;
                            }

                            Player mover = currentPlayer;
                            currentPlayer = currentPlayer == Player.Red ? Player.Blue : Player.Red;
                            board.CalculateMoves(currentPlayer);

                            if (board.playerMoves.Count == 0)
                            {
                                hasGameFinished = true;
                                Message?.Invoke(mover, GameState.Finished);
                            }
                            else
                            {
                                Message?.Invoke(currentPlayer, GameState.Click);
                            }

                            return;
                        }
                    }
                    break;
            }
        }
    }

    void spawnBlock()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GameObject temp = Instantiate(block);
                temp.transform.position = new Vector3(i, j, -1f);
                temp.GetComponent<SpriteRenderer>().color = (i + j) % 2 == 0 ? Color.grey : Color.black;
            }
        }
    }

    void fitCameraToBoard()
    {
        Camera cam = Camera.main;
        cam.transform.position = new Vector3(3.5f, 3.5f, -10f);

        float boardHalfSize = 4f;
        float verticalSize = boardHalfSize;
        float horizontalSize = boardHalfSize / cam.aspect;
        cam.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }

    public void GameQuit()
    {
        Application.Quit();
    }

    public void GameRestart()
    {
        SceneManager.LoadScene(0);
    }
}
