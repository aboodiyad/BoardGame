public enum Player { Red, Blue }

public struct Grid
{
    public int x, y;
}

public struct Moves
{
    public Grid start, end;
    public bool isCapture;
    public GamePiece capturedPiece;
}

public enum GameState { Click, Move, Finished }

public enum PieceType { Normal, King }
