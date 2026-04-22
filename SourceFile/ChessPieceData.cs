public class ChessPieceData
{
	public enum PieceType
	{
		Pawn,
		Rook,
		Knight,
		Bishop,
		Queen,
		King
	}

	public bool isWhite = true;

	public int timesMoved;

	public bool queenSide;

	public bool autoControl;

	public PieceType type;

	public ChessPieceData(PieceType type, bool isWhite, bool queenSide)
	{
		this.type = type;
		this.isWhite = isWhite;
		this.queenSide = queenSide;
	}
}
