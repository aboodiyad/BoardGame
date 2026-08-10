public class GamePiece
{
    public Player player;
    public PieceType pieceType;
    public int pieceNumber;

    public GamePiece(Player tempPlayer, int tempNumber)
    {
        player = tempPlayer;
        pieceNumber = tempNumber;
        pieceType = PieceType.Normal;
    }
}
