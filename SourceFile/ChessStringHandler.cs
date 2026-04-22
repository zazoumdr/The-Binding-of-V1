using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

public static class ChessStringHandler
{
	public static void LogMatchHistory(List<string> matchHistory)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item in matchHistory)
		{
			stringBuilder.AppendLine(item);
		}
		Debug.Log(stringBuilder.ToString());
	}

	public static string GenerateFenString(ChessPieceData[] board, bool isWhiteTurn, string castlingAvailability, string enPassantTarget, int halfmoveClock, int fullmoveNumber)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int num = 7; num >= 0; num--)
		{
			int num2 = 0;
			for (int i = 0; i < 8; i++)
			{
				int num3 = num * 8 + i;
				ChessPieceData chessPieceData = board[num3];
				if (chessPieceData == null)
				{
					num2++;
					continue;
				}
				if (num2 != 0)
				{
					stringBuilder.Append(num2);
					num2 = 0;
				}
				char fenCharForPiece = GetFenCharForPiece(chessPieceData);
				stringBuilder.Append(fenCharForPiece);
			}
			if (num2 != 0)
			{
				stringBuilder.Append(num2);
			}
			if (num > 0)
			{
				stringBuilder.Append('/');
			}
		}
		stringBuilder.Append(isWhiteTurn ? " w " : " b ");
		stringBuilder.Append(castlingAvailability);
		stringBuilder.Append(" ");
		stringBuilder.Append(string.IsNullOrWhiteSpace(enPassantTarget) ? "-" : enPassantTarget);
		stringBuilder.Append(" ");
		stringBuilder.Append(halfmoveClock);
		stringBuilder.Append(" ");
		stringBuilder.Append(fullmoveNumber);
		return stringBuilder.ToString();
	}

	private static char GetFenCharForPiece(ChessPieceData piece)
	{
		char c = '0';
		switch (piece.type)
		{
		case ChessPieceData.PieceType.Pawn:
			c = 'p';
			break;
		case ChessPieceData.PieceType.Rook:
			c = 'r';
			break;
		case ChessPieceData.PieceType.Knight:
			c = 'n';
			break;
		case ChessPieceData.PieceType.Bishop:
			c = 'b';
			break;
		case ChessPieceData.PieceType.Queen:
			c = 'q';
			break;
		case ChessPieceData.PieceType.King:
			c = 'k';
			break;
		default:
			Debug.LogError("Received an invalid piece type from the chess engine");
			break;
		}
		if (!piece.isWhite)
		{
			return c;
		}
		return char.ToUpper(c);
	}

	public static string CalculateCastlingAvailability(ChessPieceData[] board)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = true;
		bool flag4 = true;
		bool flag5 = true;
		bool flag6 = true;
		foreach (ChessPieceData chessPieceData in board)
		{
			if (chessPieceData == null)
			{
				continue;
			}
			if (chessPieceData.type == ChessPieceData.PieceType.King)
			{
				if (chessPieceData.isWhite)
				{
					flag = chessPieceData.timesMoved != 0;
				}
				else
				{
					flag2 = chessPieceData.timesMoved != 0;
				}
			}
			else
			{
				if (chessPieceData.type != ChessPieceData.PieceType.Rook)
				{
					continue;
				}
				if (chessPieceData.isWhite)
				{
					if (chessPieceData.queenSide)
					{
						flag4 = chessPieceData.timesMoved == 0;
					}
					else
					{
						flag3 = chessPieceData.timesMoved == 0;
					}
				}
				else if (chessPieceData.queenSide)
				{
					flag6 = chessPieceData.timesMoved == 0;
				}
				else
				{
					flag5 = chessPieceData.timesMoved == 0;
				}
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (!flag)
		{
			if (flag3)
			{
				stringBuilder.Append("K");
			}
			if (flag4)
			{
				stringBuilder.Append("Q");
			}
		}
		if (!flag2)
		{
			if (flag5)
			{
				stringBuilder.Append("k");
			}
			if (flag6)
			{
				stringBuilder.Append("q");
			}
		}
		if (stringBuilder.Length <= 0)
		{
			return "-";
		}
		return stringBuilder.ToString();
	}

	public static string ConvertToChessNotation(int2 position)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		char c = (char)(97 + position.x);
		int num = position.y + 1;
		return $"{c}{num}";
	}

	public static int2 ConvertFromChessNotation(string notation)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (notation.Length < 2)
		{
			Debug.LogError("Invalid chess notation");
		}
		char num = notation[0];
		int num2 = int.Parse(notation.Substring(1));
		int num3 = num - 97;
		int num4 = num2 - 1;
		return new int2(num3, num4);
	}

	public static (int2 start, int2 end, ChessPieceData.PieceType promotionType) ProcessFullMove(string move)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(move) || move.Length < 4)
		{
			Debug.LogError("Got invalid move from bot");
		}
		string notation = move.Substring(0, 2);
		string notation2 = move.Substring(2, 2);
		if (move.Contains("none"))
		{
			Debug.LogError("Bot found move: " + move);
		}
		int2 item = ConvertFromChessNotation(notation);
		int2 item2 = ConvertFromChessNotation(notation2);
		ChessPieceData.PieceType item3 = ChessPieceData.PieceType.Pawn;
		if (move.Length > 4)
		{
			switch (move[4])
			{
			case 'q':
				item3 = ChessPieceData.PieceType.Queen;
				break;
			case 'r':
				item3 = ChessPieceData.PieceType.Rook;
				break;
			case 'b':
				item3 = ChessPieceData.PieceType.Bishop;
				break;
			case 'n':
				item3 = ChessPieceData.PieceType.Knight;
				break;
			}
		}
		return (start: item, end: item2, promotionType: item3);
	}

	public static string UCIMove(ChessManager.MoveData moveData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		string text = ConvertToChessNotation(moveData.StartPosition) + ConvertToChessNotation(moveData.EndPosition);
		char c = 'p';
		switch (moveData.PromotionType)
		{
		case ChessPieceData.PieceType.Knight:
			c = 'n';
			break;
		case ChessPieceData.PieceType.Rook:
			c = 'r';
			break;
		case ChessPieceData.PieceType.Bishop:
			c = 'b';
			break;
		case ChessPieceData.PieceType.Queen:
			c = 'q';
			break;
		}
		if (moveData.PromotionType != ChessPieceData.PieceType.Pawn)
		{
			text += c;
		}
		return text;
	}

	public static string LogPerft(ChessManager.MoveData moveData, int subsequentMoves = 0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		string text = ConvertToChessNotation(moveData.StartPosition) + ConvertToChessNotation(moveData.EndPosition);
		char c = 'p';
		switch (moveData.PromotionType)
		{
		case ChessPieceData.PieceType.Knight:
			c = 'n';
			break;
		case ChessPieceData.PieceType.Rook:
			c = 'r';
			break;
		case ChessPieceData.PieceType.Bishop:
			c = 'b';
			break;
		case ChessPieceData.PieceType.Queen:
			c = 'q';
			break;
		}
		if (moveData.PromotionType != ChessPieceData.PieceType.Pawn)
		{
			text += c;
		}
		return text + $": {subsequentMoves}";
	}

	public static void LogMoveData(ChessManager.MoveData moveData, int subsequentMoves = 0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		string text = ConvertToChessNotation(moveData.StartPosition) + ConvertToChessNotation(moveData.EndPosition);
		if (subsequentMoves > 0)
		{
			text += $" {subsequentMoves}";
		}
		Debug.Log(text + "\nMove Data:\n" + $"Piece Type: {moveData.PieceToMove.type}\n" + $"Start Position: {moveData.StartPosition}\n" + "Color: " + (moveData.PieceToMove.isWhite ? "White" : "Black") + "\n" + $"End Position: {moveData.EndPosition}\n" + "Capture Piece: " + ((moveData.CapturePiece != null) ? moveData.CapturePiece.type.ToString() : "None") + "\n" + $"Castle State: {moveData.SpecialMove}");
	}
}
