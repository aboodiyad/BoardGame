using System.Collections.Generic;

public class Board
{
    public Dictionary<GamePiece, Grid> playerPositions;
    public Dictionary<GamePiece, List<Moves>> playerMoves;
    public bool isCapture;

    readonly List<Grid> kingMoves = new List<Grid>
    {
        new Grid { x = -1, y = -1 },
        new Grid { x = -1, y = 1 },
        new Grid { x = 1, y = -1 },
        new Grid { x = 1, y = 1 }
    };

    readonly List<Grid> redDirections = new List<Grid>
    {
        new Grid { x = -1, y = 1 },
        new Grid { x = 1, y = 1 }
    };

    readonly List<Grid> blueDirections = new List<Grid>
    {
        new Grid { x = -1, y = -1 },
        new Grid { x = 1, y = -1 }
    };

    public Board()
    {
        playerPositions = new Dictionary<GamePiece, Grid>();
        playerMoves = new Dictionary<GamePiece, List<Moves>>();
        Init();
    }

    public void Init()
    {
        playerPositions.Clear();

        // Red pieces on bottom rows.
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if ((x + y) % 2 == 1)
                {
                    playerPositions[new GamePiece(Player.Red, playerPositions.Count)] = new Grid { x = x, y = y };
                }
            }
        }

        // Blue pieces on top rows.
        for (int y = 5; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if ((x + y) % 2 == 1)
                {
                    playerPositions[new GamePiece(Player.Blue, playerPositions.Count)] = new Grid { x = x, y = y };
                }
            }
        }
    }

    public void CalculateMoves(Player currentPlayer)
    {
        playerMoves = new Dictionary<GamePiece, List<Moves>>();
        isCapture = false;

        foreach (var item in playerPositions)
        {
            GamePiece currentPiece = item.Key;
            Grid currentPosition = item.Value;

            if (currentPiece.player != currentPlayer)
            {
                continue;
            }

            List<Grid> directions = currentPiece.pieceType == PieceType.King ? kingMoves : (currentPiece.player == Player.Red ? redDirections : blueDirections);
            List<Moves> availableMoves = new List<Moves>();

            foreach (Grid direction in directions)
            {
                Grid toCheck = new Grid { x = currentPosition.x + direction.x, y = currentPosition.y + direction.y };
                if (!isValidGrid(toCheck))
                {
                    continue;
                }

                GamePiece targetPiece = GetPieceAtPosition(toCheck);

                if (targetPiece.pieceNumber != -1)
                {
                    if (targetPiece.player == currentPiece.player)
                    {
                        continue;
                    }

                    Grid doubleCheck = new Grid { x = toCheck.x + direction.x, y = toCheck.y + direction.y };
                    if (!isValidGrid(doubleCheck))
                    {
                        continue;
                    }

                    GamePiece landingPiece = GetPieceAtPosition(doubleCheck);
                    if (landingPiece.pieceNumber == -1)
                    {
                        availableMoves.Add(new Moves
                        {
                            start = currentPosition,
                            end = doubleCheck,
                            isCapture = true,
                            capturedPiece = targetPiece
                        });
                    }
                }
                else
                {
                    availableMoves.Add(new Moves
                    {
                        start = currentPosition,
                        end = toCheck,
                        isCapture = false,
                        capturedPiece = new GamePiece(Player.Red, -1)
                    });
                }
            }

            if (availableMoves.Count > 0)
            {
                playerMoves[currentPiece] = availableMoves;
            }
        }

        foreach (var moves in playerMoves.Values)
        {
            foreach (Moves move in moves)
            {
                if (move.isCapture)
                {
                    isCapture = true;
                    break;
                }
            }

            if (isCapture)
            {
                break;
            }
        }

        if (isCapture)
        {
            var filtered = new Dictionary<GamePiece, List<Moves>>();

            foreach (var entry in playerMoves)
            {
                List<Moves> captures = entry.Value.FindAll(move => move.isCapture);
                if (captures.Count > 0)
                {
                    filtered[entry.Key] = captures;
                }
            }

            playerMoves = filtered;
        }
    }

    public GamePiece GetPieceAtPosition(Grid position)
    {
        foreach (var pair in playerPositions)
        {
            if (pair.Value.x == position.x && pair.Value.y == position.y)
            {
                return pair.Key;
            }
        }

        return new GamePiece(Player.Red, -1);
    }

    public void UpdateMove(Moves moves)
    {
        GamePiece movingPiece = GetPieceAtPosition(moves.start);
        if (movingPiece.pieceNumber == -1)
        {
            return;
        }

        playerPositions[movingPiece] = moves.end;

        if (moves.isCapture && moves.capturedPiece.pieceNumber != -1)
        {
            playerPositions.Remove(moves.capturedPiece);
        }
    }

    public void UpgradePiece(GamePiece kingPiece)
    {
        if (kingPiece != null)
        {
            kingPiece.pieceType = PieceType.King;
        }
    }

    public static bool isValidGrid(Grid temp)
    {
        return temp.x >= 0 && temp.x < 8 && temp.y >= 0 && temp.y < 8;
    }
}
