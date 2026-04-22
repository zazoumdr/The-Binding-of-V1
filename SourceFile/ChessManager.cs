using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ULTRAKILL.Cheats;
using Unity.Mathematics;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class ChessManager : MonoSingleton<ChessManager>
{
	public enum SpecialMove
	{
		None,
		ShortCastle,
		LongCastle,
		PawnTwoStep,
		PawnPromotion,
		EnPassantCapture
	}

	public struct MoveData(ChessPieceData pieceToMove, int2 startPosition, ChessPieceData capturePiece, int2 endPosition, int2 lastEPPos, SpecialMove specialMove = SpecialMove.None, ChessPieceData.PieceType promotionType = ChessPieceData.PieceType.Pawn)
	{
		public int2 StartPosition = startPosition;

		public ChessPieceData PieceToMove = pieceToMove;

		public int2 EndPosition = endPosition;

		public ChessPieceData CapturePiece = capturePiece;

		public SpecialMove SpecialMove = specialMove;

		public int2 LastEnPassantPos = lastEPPos;

		public ChessPieceData.PieceType PromotionType = promotionType;
	}

	public GameObject originalPieces;

	public GameObject originalExtras;

	public GameObject blackWinner;

	public GameObject whiteWinner;

	public GameObject blackOpponent;

	public GameObject whiteOpponent;

	public GameObject draw;

	public Transform helperTileGroup;

	private Renderer[] helperTiles = new Renderer[64];

	private MaterialPropertyBlock colorSetter;

	private Bounds colBounds;

	private GameObject clonedPieces;

	private ChessPieceData[] chessBoard = new ChessPieceData[64];

	private Dictionary<ChessPieceData, ChessPiece> allPieces = new Dictionary<ChessPieceData, ChessPiece>();

	private ChessPieceData whiteKing;

	private ChessPieceData blackKing;

	private int2 enPassantPos = new int2(-1, -1);

	private List<MoveData> legalMoves = new List<MoveData>(27);

	private List<MoveData> pseudoLegalMoves = new List<MoveData>(27);

	private List<MoveData> allLegalMoves = new List<MoveData>(27);

	private UciChessEngine chessEngine;

	private List<string> UCIMoves = new List<string>();

	[HideInInspector]
	public bool isWhiteTurn = true;

	[HideInInspector]
	public bool whiteIsBot;

	[HideInInspector]
	public bool blackIsBot = true;

	[HideInInspector]
	public bool gameLocked = true;

	private bool tutorialMessageSent;

	private int numMoves;

	public int elo = 1000;

	private static readonly int2[] pawnMoves = (int2[])(object)new int2[2]
	{
		new int2(0, 1),
		new int2(0, 2)
	};

	private static readonly int2[] pawnCaptures = (int2[])(object)new int2[2]
	{
		new int2(1, 1),
		new int2(-1, 1)
	};

	private static readonly int2[] rookDirections = (int2[])(object)new int2[4]
	{
		new int2(1, 0),
		new int2(-1, 0),
		new int2(0, 1),
		new int2(0, -1)
	};

	private static readonly int2[] bishopDirections = (int2[])(object)new int2[4]
	{
		new int2(1, 1),
		new int2(-1, 1),
		new int2(1, -1),
		new int2(-1, -1)
	};

	private static readonly int2[] queenDirections = rookDirections.Concat(bishopDirections).ToArray();

	private static readonly int2[] knightOffsets = (int2[])(object)new int2[8]
	{
		new int2(1, 2),
		new int2(2, 1),
		new int2(2, -1),
		new int2(1, -2),
		new int2(-1, -2),
		new int2(-2, -1),
		new int2(-2, 1),
		new int2(-1, 2)
	};

	private static readonly int2[] kingDirections = (int2[])(object)new int2[8]
	{
		new int2(1, 0),
		new int2(-1, 0),
		new int2(0, 1),
		new int2(0, -1),
		new int2(1, 1),
		new int2(-1, 1),
		new int2(1, -1),
		new int2(-1, -1)
	};

	private void Awake()
	{
		gameLocked = true;
		colBounds = GetComponent<Collider>().bounds;
		colorSetter = new MaterialPropertyBlock();
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				Renderer component = helperTileGroup.GetChild(i).GetChild(j).GetComponent<Renderer>();
				component.SetPropertyBlock(colorSetter);
				helperTiles[i + j * 8] = component;
			}
		}
	}

	private void Start()
	{
		ResetBoard();
	}

	public void SetupNewGame()
	{
		StopAllCoroutines();
		ResetBoard();
		gameLocked = false;
		if (!whiteIsBot || !blackIsBot)
		{
			MonoSingleton<CheatsManager>.Instance.GetCheatInstance<SummonSandboxArm>()?.TryCreateArmType(SpawnableType.MoveHand);
			if (!tutorialMessageSent)
			{
				MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Chess pieces can be moved with the <color=orange>mover arm</color>.");
				tutorialMessageSent = true;
			}
		}
	}

	public void ToggleWhiteBot(bool isBot)
	{
		whiteIsBot = isBot;
		whiteOpponent.SetActive(whiteIsBot);
	}

	public void ToggleBlackBot(bool isBot)
	{
		blackIsBot = isBot;
		blackOpponent.SetActive(blackIsBot);
	}

	public void ResetBoard()
	{
		numMoves = 0;
		blackWinner.SetActive(value: false);
		whiteWinner.SetActive(value: false);
		HideHelperTiles();
		UCIMoves.Clear();
		if (clonedPieces != null)
		{
			UnityEngine.Object.Destroy(clonedPieces);
		}
		foreach (ChessPiece value in allPieces.Values)
		{
			if (value != null)
			{
				UnityEngine.Object.Destroy(value.gameObject);
			}
		}
		clonedPieces = null;
		clonedPieces = UnityEngine.Object.Instantiate(originalPieces, base.transform.parent);
		clonedPieces.SetActive(value: true);
		originalPieces.SetActive(value: false);
		allPieces.Clear();
		for (int i = 0; i < chessBoard.Length; i++)
		{
			chessBoard[i] = null;
		}
		isWhiteTurn = true;
		whiteOpponent.SetActive(whiteIsBot);
		blackOpponent.SetActive(blackIsBot);
		if (whiteIsBot || blackIsBot)
		{
			StartEngine();
		}
	}

	public void UpdateGame(MoveData move)
	{
		gameLocked = false;
		string text = ChessStringHandler.UCIMove(move);
		if (UCIMoves.Count > 0 && UCIMoves[UCIMoves.Count - 1].Equals(text))
		{
			Debug.LogError("tried to perform the same move twice");
			return;
		}
		UCIMoves.Add(text);
		if (UCIMoves.Count == 3 && UCIMoves[0] == "e2e4" && UCIMoves[1] == "e7e5" && UCIMoves[2] == "e1e2")
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(420, "<color=green>BONGCLOUD</color>");
		}
		string newMoveData = string.Join(" ", UCIMoves);
		if (isWhiteTurn)
		{
			numMoves++;
		}
		isWhiteTurn = !isWhiteTurn;
		if (IsGameOver())
		{
			if (numMoves == 2)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(1, "<color=red>FOOLS MATE</color>");
			}
		}
		else if ((isWhiteTurn && whiteIsBot) || (!isWhiteTurn && blackIsBot))
		{
			StartCoroutine(SendToBotCoroutine(newMoveData));
		}
	}

	private bool IsGameOver()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (!IsSufficientMaterial())
		{
			WinTrigger(null);
			return true;
		}
		allLegalMoves.Clear();
		for (int i = 0; i < chessBoard.Length; i++)
		{
			ChessPieceData chessPieceData = chessBoard[i];
			if (chessPieceData != null && chessPieceData.isWhite == isWhiteTurn)
			{
				GetLegalMoves(new int2(i % 8, i / 8));
				allLegalMoves.AddRange(legalMoves);
			}
		}
		if (allLegalMoves.Count == 0)
		{
			if (IsSquareAttacked(GetPiecePos(isWhiteTurn ? whiteKing : blackKing), isWhiteTurn))
			{
				WinTrigger(!isWhiteTurn);
			}
			else
			{
				WinTrigger(null);
			}
			return true;
		}
		return false;
	}

	public bool IsSufficientMaterial()
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = 0;
		bool flag = false;
		bool flag2 = false;
		int2? val = null;
		int2? val2 = null;
		int2 value = default(int2);
		for (int i = 0; i < chessBoard.Length; i++)
		{
			ChessPieceData chessPieceData = chessBoard[i];
			if (chessPieceData == null || chessPieceData.type == ChessPieceData.PieceType.King)
			{
				continue;
			}
			((int2)(ref value))._002Ector(i % 8, i / 8);
			if (chessPieceData.isWhite)
			{
				num++;
				if (chessPieceData.type == ChessPieceData.PieceType.Bishop)
				{
					flag = true;
					val = value;
				}
			}
			else
			{
				num2++;
				if (chessPieceData.type == ChessPieceData.PieceType.Bishop)
				{
					flag2 = true;
					val2 = value;
				}
			}
			if (num > 1 || num2 > 1 || chessPieceData.type == ChessPieceData.PieceType.Pawn || chessPieceData.type == ChessPieceData.PieceType.Rook || chessPieceData.type == ChessPieceData.PieceType.Queen)
			{
				return true;
			}
		}
		if (flag && flag2)
		{
			return (val.Value.x + val.Value.y) % 2 != (val2.Value.x + val2.Value.y) % 2;
		}
		return false;
	}

	public void WinTrigger(bool? whiteWin)
	{
		gameLocked = true;
		StopEngine();
		if (!whiteWin.HasValue)
		{
			draw.GetComponent<AudioSource>().Play();
			return;
		}
		GameObject gameObject = (whiteWin.Value ? whiteWinner : blackWinner);
		gameObject.SetActive(value: true);
		AudioSource[] components = gameObject.GetComponents<AudioSource>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].Play();
		}
		gameObject.GetComponent<ParticleSystem>().Play();
		if ((whiteWin == true && !whiteIsBot) || (whiteWin == false && !blackIsBot))
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(5000, "<color=orange>" + ((whiteWin == true) ? "WHITE" : "BLACK") + " WINS</color>");
		}
		if ((whiteWin == true && !whiteIsBot && blackIsBot) || (whiteWin == false && !blackIsBot && whiteIsBot))
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(5000, "<color=red>ULTRAVICTORY</color>");
		}
	}

	public void SetElo(float newElo)
	{
		elo = Mathf.FloorToInt(newElo);
	}

	public void WhiteIsBot(bool isBot)
	{
		whiteIsBot = isBot;
	}

	public void BlackIsBot(bool isBot)
	{
		blackIsBot = isBot;
	}

	private async void StartEngine()
	{
		chessEngine = new UciChessEngine();
		await chessEngine.InitializeUciModeAsync(whiteIsBot, elo);
	}

	public async void StopEngine()
	{
		if (chessEngine != null)
		{
			await chessEngine.StopEngine();
			chessEngine = null;
		}
	}

	public void BotStartGame()
	{
		StartCoroutine(SendToBotCoroutine(""));
	}

	private IEnumerator SendToBotCoroutine(string newMoveData)
	{
		bool isResponseReceived = false;
		string response = "";
		if (elo < 1500)
		{
			int num = elo - 1000;
			chessEngine.SendPlayerMoveAndGetEngineResponseAsync(newMoveData, onReceivedResponse, 250 + num);
		}
		else
		{
			chessEngine.SendPlayerMoveAndGetEngineResponseAsync(newMoveData, onReceivedResponse);
		}
		yield return new WaitUntil(() => isResponseReceived);
		if (response.StartsWith("bestmove"))
		{
			string botMove = ParseBotMove(response);
			MakeBotMove(botMove);
		}
		void onReceivedResponse(string resp)
		{
			response = resp;
			isResponseReceived = true;
		}
	}

	private string ParseBotMove(string engineResponse)
	{
		string[] array = engineResponse.Split(' ');
		if (array.Length >= 2)
		{
			return array[1];
		}
		return string.Empty;
	}

	private IEnumerator LerpBotMove(ChessPiece piece, int2 endIndex, MoveData moveData)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Transform trans = piece.transform;
		Vector3 startPos = trans.position;
		Vector3 endPos = IndexToWorldPosition(endIndex, piece.boardHeight);
		float duration = UnityEngine.Random.Range(0.5f, 1f);
		float elapsed = 0f;
		if (UnityEngine.Random.Range(0, 1000) == 666)
		{
			duration = 15f;
		}
		piece.dragSound.SetPitch(UnityEngine.Random.Range(0.75f, 1.25f));
		piece.dragSound.Play();
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / duration);
			trans.position = Vector3.Lerp(startPos, endPos, t);
			yield return null;
		}
		piece.dragSound.Stop();
		UnityEngine.Object.Instantiate<AudioSource>(piece.snapSound, piece.transform.position, Quaternion.identity);
		yield return null;
		MakeMove(moveData, updateVisuals: true);
		yield return null;
	}

	private void MakeBotMove(string botMove)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		(int2, int2, ChessPieceData.PieceType) tuple = ChessStringHandler.ProcessFullMove(botMove);
		int2 item = tuple.Item1;
		int2 endPos = tuple.Item2;
		ChessPieceData.PieceType promotionType = tuple.Item3;
		ChessPieceData pieceAt = GetPieceAt(item);
		if (pieceAt == null)
		{
			Debug.LogError("found no piece for move " + botMove);
		}
		GetLegalMoves(item);
		MoveData moveData = legalMoves.FirstOrDefault((MoveData move) => ((int2)(ref move.EndPosition)).Equals(endPos) && move.PromotionType == promotionType);
		if (((int2)(ref moveData.EndPosition)).Equals(endPos))
		{
			ChessPiece piece = allPieces[pieceAt];
			StartCoroutine(LerpBotMove(piece, endPos, moveData));
		}
	}

	public int2 WorldPositionToIndex(Vector3 pos)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 min = colBounds.min;
		Vector3 max = colBounds.max;
		Vector3 vector = new Vector3((pos.x - min.x) / (max.x - min.x), 0f, (pos.z - min.z) / (max.z - min.z));
		int num = Mathf.FloorToInt(vector.x * 8f);
		int num2 = Mathf.FloorToInt(vector.z * 8f);
		return new int2(num, num2);
	}

	public Vector3 IndexToWorldPosition(int2 index, float height)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 min = colBounds.min;
		Vector3 max = colBounds.max;
		float num = (float)Mathf.Clamp(index.x, 0, 7) + 0.5f;
		float num2 = (float)Mathf.Clamp(index.y, 0, 7) + 0.5f;
		return new Vector3(min.x + num * (max.x - min.x) / 8f, height, min.z + num2 * (max.z - min.z) / 8f);
	}

	public void DisplayValidMoves()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		foreach (MoveData legalMove in legalMoves)
		{
			int x = legalMove.EndPosition.x;
			int y = legalMove.EndPosition.y;
			if (x >= 0 && x < 8 && y >= 0 && y < 8)
			{
				Renderer obj = helperTiles[x + y * 8];
				colorSetter.SetColor("_TintColor", (legalMove.CapturePiece != null) ? Color.green : Color.cyan);
				obj.SetPropertyBlock(colorSetter);
			}
			else
			{
				Debug.LogError("Trying to display a move out of range");
			}
		}
	}

	public void HideHelperTiles()
	{
		colorSetter.SetColor("_TintColor", Color.clear);
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				helperTiles[i + j * 8].SetPropertyBlock(colorSetter);
			}
		}
	}

	public void FindMoveAtWorldPosition(ChessPiece chessPiece)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = chessPiece.transform.position;
		int2 tileID = WorldPositionToIndex(position);
		ChessPieceData data = chessPiece.Data;
		if (legalMoves.Count == 0)
		{
			chessPiece.UpdatePosition(GetPiecePos(data));
		}
		else
		{
			MoveData moveData = legalMoves.FirstOrDefault((MoveData move) => ((int2)(ref move.EndPosition)).Equals(tileID));
			if (!((int2)(ref moveData.EndPosition)).Equals(tileID) || ((int2)(ref moveData.StartPosition)).Equals(moveData.EndPosition))
			{
				chessPiece.UpdatePosition(GetPiecePos(data));
			}
			else
			{
				MakeMove(moveData, updateVisuals: true);
			}
		}
		HideHelperTiles();
	}

	public void InitializePiece(ChessPiece piece)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		ChessPieceData data = piece.Data;
		allPieces.Add(data, piece);
		Vector3 position = piece.transform.position;
		int2 val = WorldPositionToIndex(position);
		if (data.type == ChessPieceData.PieceType.King)
		{
			if (piece.isWhite)
			{
				whiteKing = data;
			}
			else
			{
				blackKing = data;
			}
		}
		SetPieceAt(val, data);
		piece.UpdatePosition(val);
	}

	public ChessPieceData GetPieceAt(int2 index)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return chessBoard[index.x + index.y * 8];
	}

	public void SetPieceAt(int2 index, ChessPieceData piece)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		chessBoard[index.x + index.y * 8] = piece;
	}

	private int2 GetPiecePos(ChessPieceData piece)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		int num = Array.IndexOf(chessBoard, piece);
		return new int2(num % 8, num / 8);
	}

	public void MakeMove(MoveData moveData, bool updateVisuals = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		ChessPieceData pieceToMove = moveData.PieceToMove;
		int2 endPosition = moveData.EndPosition;
		if (moveData.SpecialMove == SpecialMove.EnPassantCapture)
		{
			SetPieceAt(endPosition + new int2(0, (!pieceToMove.isWhite) ? 1 : (-1)), null);
		}
		if (moveData.SpecialMove == SpecialMove.PawnTwoStep)
		{
			enPassantPos = endPosition + new int2(0, (!pieceToMove.isWhite) ? 1 : (-1));
		}
		else
		{
			enPassantPos = new int2(-1, -1);
		}
		pieceToMove.timesMoved++;
		SetPieceAt(moveData.StartPosition, null);
		SetPieceAt(endPosition, pieceToMove);
		if (moveData.SpecialMove == SpecialMove.ShortCastle || moveData.SpecialMove == SpecialMove.LongCastle)
		{
			int num = ((moveData.SpecialMove == SpecialMove.ShortCastle) ? 7 : 0);
			int num2 = ((moveData.SpecialMove == SpecialMove.ShortCastle) ? 5 : 3);
			int2 index = default(int2);
			((int2)(ref index))._002Ector(num, (!pieceToMove.isWhite) ? 7 : 0);
			int2 val = default(int2);
			((int2)(ref val))._002Ector(num2, (!pieceToMove.isWhite) ? 7 : 0);
			ChessPieceData pieceAt = GetPieceAt(index);
			pieceAt.timesMoved++;
			SetPieceAt(index, null);
			SetPieceAt(val, pieceAt);
			if (updateVisuals && allPieces.TryGetValue(pieceAt, out var value))
			{
				value.UpdatePosition(val);
			}
		}
		if (moveData.SpecialMove == SpecialMove.PawnPromotion)
		{
			pieceToMove.type = moveData.PromotionType;
			if (updateVisuals)
			{
				ChessPiece chessPiece = allPieces[pieceToMove];
				if (chessPiece.autoControl)
				{
					chessPiece.PromoteVisualPiece();
				}
				else
				{
					gameLocked = true;
					foreach (KeyValuePair<ChessPieceData, ChessPiece> allPiece in allPieces)
					{
						allPiece.Value.PieceCanMove(canMove: false);
					}
					chessPiece.ShowPromotionGUI(moveData);
				}
			}
		}
		if (updateVisuals && allPieces.TryGetValue(pieceToMove, out var value2))
		{
			value2.UpdatePosition(endPosition);
			if (moveData.SpecialMove == SpecialMove.LongCastle || moveData.SpecialMove == SpecialMove.ShortCastle)
			{
				UnityEngine.Object.Instantiate(value2.teleportEffect, value2.transform.position, Quaternion.identity);
			}
			if (moveData.SpecialMove == SpecialMove.PawnPromotion)
			{
				UnityEngine.Object.Instantiate(value2.promotionEffect, value2.transform.position, Quaternion.identity);
			}
			if (!pieceToMove.autoControl && moveData.SpecialMove != SpecialMove.PawnPromotion)
			{
				StylishMove(moveData);
			}
		}
		if (updateVisuals && moveData.CapturePiece != null && allPieces.TryGetValue(moveData.CapturePiece, out var value3))
		{
			value3.Captured();
		}
		if (updateVisuals && (moveData.SpecialMove != SpecialMove.PawnPromotion || moveData.PieceToMove.autoControl))
		{
			UpdateGame(moveData);
		}
	}

	public void StylishMove(MoveData move)
	{
		if (move.SpecialMove == SpecialMove.LongCastle || move.SpecialMove == SpecialMove.ShortCastle)
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(50, "<color=#00ffffff>CASTLED</color>");
		}
		if (move.SpecialMove == SpecialMove.PawnPromotion)
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(500, "<color=green>" + move.PromotionType.ToString().ToUpper() + " PROMOTION</color>");
		}
		int num = 0;
		string text = "<color=white>";
		if (move.CapturePiece != null)
		{
			switch (move.CapturePiece.type)
			{
			case ChessPieceData.PieceType.Knight:
				num = 100;
				text = "<color=green>";
				break;
			case ChessPieceData.PieceType.Bishop:
				num = 100;
				text = "<color=green>";
				break;
			case ChessPieceData.PieceType.Rook:
				num = 200;
				text = "<color=orange>";
				break;
			case ChessPieceData.PieceType.Queen:
				num = 400;
				text = "<color=red>";
				break;
			}
			if (move.SpecialMove == SpecialMove.EnPassantCapture)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(100, "<color=#00ffffff>EN PASSANT</color>");
			}
			else
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(100 + num, text + move.CapturePiece.type.ToString().ToUpper() + " CAPTURE</color>");
			}
		}
	}

	public void UnmakeMove(MoveData moveData, bool updateVisuals = false)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		enPassantPos = moveData.LastEnPassantPos;
		ChessPieceData pieceToMove = moveData.PieceToMove;
		SetPieceAt(moveData.StartPosition, moveData.PieceToMove);
		int2 val = moveData.EndPosition;
		if (moveData.SpecialMove == SpecialMove.EnPassantCapture)
		{
			SetPieceAt(val, null);
			val += new int2(0, (!pieceToMove.isWhite) ? 1 : (-1));
		}
		SetPieceAt(val, moveData.CapturePiece);
		pieceToMove.timesMoved--;
		if (moveData.SpecialMove == SpecialMove.ShortCastle || moveData.SpecialMove == SpecialMove.LongCastle)
		{
			int num = ((moveData.SpecialMove == SpecialMove.ShortCastle) ? 7 : 0);
			int num2 = ((moveData.SpecialMove == SpecialMove.ShortCastle) ? 5 : 3);
			int2 index = default(int2);
			((int2)(ref index))._002Ector(num, (!pieceToMove.isWhite) ? 7 : 0);
			int2 index2 = default(int2);
			((int2)(ref index2))._002Ector(num2, (!pieceToMove.isWhite) ? 7 : 0);
			ChessPieceData pieceAt = GetPieceAt(index2);
			pieceAt.timesMoved--;
			SetPieceAt(index2, null);
			SetPieceAt(index, pieceAt);
		}
		if (moveData.SpecialMove == SpecialMove.PawnPromotion)
		{
			pieceToMove.type = ChessPieceData.PieceType.Pawn;
		}
		if (updateVisuals && allPieces.TryGetValue(pieceToMove, out var value))
		{
			value.UpdatePosition(moveData.StartPosition);
		}
		if (updateVisuals && moveData.CapturePiece != null && allPieces.TryGetValue(moveData.CapturePiece, out var value2))
		{
			value2.UpdatePosition(val);
		}
	}

	private bool IsValidPosition(int2 index)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (index.x >= 0 && index.x < 8 && index.y >= 0)
		{
			return index.y < 8;
		}
		return false;
	}

	public unsafe void GetLegalMoves(int2 index)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		ChessPieceData pieceAt = GetPieceAt(index);
		if (pieceAt == null)
		{
			int2 val = index;
			Debug.LogError("Found no piece at " + ((object)(*(int2*)(&val))/*cast due to .constrained prefix*/).ToString());
		}
		pseudoLegalMoves.Clear();
		legalMoves.Clear();
		switch (pieceAt.type)
		{
		case ChessPieceData.PieceType.Pawn:
			GetPawnMoves(pieceAt, index, pseudoLegalMoves);
			break;
		case ChessPieceData.PieceType.Knight:
		case ChessPieceData.PieceType.King:
			GetKnightKingMoves(pieceAt, index, pseudoLegalMoves);
			break;
		case ChessPieceData.PieceType.Rook:
		case ChessPieceData.PieceType.Bishop:
		case ChessPieceData.PieceType.Queen:
			GetSlidingMoves(pieceAt, index, pseudoLegalMoves);
			break;
		}
		int2 position = GetPiecePos(pieceAt.isWhite ? whiteKing : blackKing);
		foreach (MoveData pseudoLegalMove in pseudoLegalMoves)
		{
			MakeMove(pseudoLegalMove);
			if (pseudoLegalMove.PieceToMove.type == ChessPieceData.PieceType.King)
			{
				position = pseudoLegalMove.EndPosition;
			}
			if (!IsSquareAttacked(position, pieceAt.isWhite))
			{
				legalMoves.Add(pseudoLegalMove);
			}
			UnmakeMove(pseudoLegalMove);
		}
	}

	private void GetPawnMoves(ChessPieceData pawn, int2 startPos, List<MoveData> validMoves)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		int num = (pawn.isWhite ? 1 : (-1));
		int2 val = startPos + pawnMoves[0] * num;
		if (GetPieceAt(val) == null)
		{
			if (val.y == (pawn.isWhite ? 7 : 0))
			{
				validMoves.Add(new MoveData(pawn, startPos, null, val, enPassantPos, SpecialMove.PawnPromotion, ChessPieceData.PieceType.Queen));
				validMoves.Add(new MoveData(pawn, startPos, null, val, enPassantPos, SpecialMove.PawnPromotion, ChessPieceData.PieceType.Rook));
				validMoves.Add(new MoveData(pawn, startPos, null, val, enPassantPos, SpecialMove.PawnPromotion, ChessPieceData.PieceType.Bishop));
				validMoves.Add(new MoveData(pawn, startPos, null, val, enPassantPos, SpecialMove.PawnPromotion, ChessPieceData.PieceType.Knight));
			}
			else
			{
				validMoves.Add(new MoveData(pawn, startPos, null, val, enPassantPos));
			}
			if (pawn.timesMoved == 0)
			{
				int2 val2 = startPos + pawnMoves[1] * num;
				if (GetPieceAt(val2) == null)
				{
					validMoves.Add(new MoveData(pawn, startPos, null, val2, enPassantPos, SpecialMove.PawnTwoStep));
				}
			}
		}
		int2[] array = pawnCaptures;
		int2 index = default(int2);
		foreach (int2 val3 in array)
		{
			int2 val4 = startPos + val3 * num;
			if (!IsValidPosition(val4))
			{
				continue;
			}
			ChessPieceData pieceAt = GetPieceAt(val4);
			if (pieceAt != null && pieceAt.isWhite != pawn.isWhite)
			{
				if (val.y == (pawn.isWhite ? 7 : 0))
				{
					validMoves.Add(new MoveData(pawn, startPos, pieceAt, val4, enPassantPos, SpecialMove.PawnPromotion, ChessPieceData.PieceType.Queen));
					validMoves.Add(new MoveData(pawn, startPos, pieceAt, val4, enPassantPos, SpecialMove.PawnPromotion, ChessPieceData.PieceType.Rook));
					validMoves.Add(new MoveData(pawn, startPos, pieceAt, val4, enPassantPos, SpecialMove.PawnPromotion, ChessPieceData.PieceType.Bishop));
					validMoves.Add(new MoveData(pawn, startPos, pieceAt, val4, enPassantPos, SpecialMove.PawnPromotion, ChessPieceData.PieceType.Knight));
				}
				else
				{
					validMoves.Add(new MoveData(pawn, startPos, pieceAt, val4, enPassantPos));
				}
			}
			if (((int2)(ref enPassantPos)).Equals(val4))
			{
				((int2)(ref index))._002Ector(enPassantPos.x, enPassantPos.y - num);
				ChessPieceData pieceAt2 = GetPieceAt(index);
				if (pieceAt2 != null && pieceAt2.isWhite != pawn.isWhite)
				{
					validMoves.Add(new MoveData(pawn, startPos, pieceAt2, enPassantPos, enPassantPos, SpecialMove.EnPassantCapture));
				}
			}
		}
	}

	private void GetSlidingMoves(ChessPieceData slidingPiece, int2 startPos, List<MoveData> validMoves)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		int2[] array;
		switch (slidingPiece.type)
		{
		case ChessPieceData.PieceType.Bishop:
			array = bishopDirections;
			break;
		case ChessPieceData.PieceType.Rook:
			array = rookDirections;
			break;
		case ChessPieceData.PieceType.Queen:
			array = queenDirections;
			break;
		default:
			Debug.LogError("Invalid piece type for sliding moves");
			array = (int2[])(object)new int2[1];
			break;
		}
		int2[] array2 = array;
		foreach (int2 val in array2)
		{
			int2 val2 = startPos;
			while (true)
			{
				val2 += val;
				if (!IsValidPosition(val2))
				{
					break;
				}
				ChessPieceData pieceAt = GetPieceAt(val2);
				if (pieceAt != null)
				{
					if (pieceAt.isWhite != slidingPiece.isWhite)
					{
						validMoves.Add(new MoveData(slidingPiece, startPos, pieceAt, val2, enPassantPos));
					}
					break;
				}
				validMoves.Add(new MoveData(slidingPiece, startPos, null, val2, enPassantPos));
			}
		}
	}

	private void GetKnightKingMoves(ChessPieceData piece, int2 startPos, List<MoveData> validMoves)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		int2[] array = ((piece.type == ChessPieceData.PieceType.Knight) ? knightOffsets : kingDirections);
		foreach (int2 val in array)
		{
			int2 val2 = startPos + val;
			if (IsValidPosition(val2))
			{
				ChessPieceData pieceAt = GetPieceAt(val2);
				if (pieceAt == null || pieceAt.isWhite != piece.isWhite)
				{
					validMoves.Add(new MoveData(piece, startPos, pieceAt, val2, enPassantPos));
				}
			}
		}
		if (piece.type == ChessPieceData.PieceType.King)
		{
			TryCastle(piece, startPos, isKingSide: true, validMoves);
			TryCastle(piece, startPos, isKingSide: false, validMoves);
		}
	}

	private void TryCastle(ChessPieceData king, int2 startPos, bool isKingSide, List<MoveData> validMoves)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if (king.timesMoved > 0 || IsSquareAttacked(startPos, king.isWhite))
		{
			return;
		}
		int num = (isKingSide ? 7 : 0);
		int2 index = default(int2);
		((int2)(ref index))._002Ector(num, startPos.y);
		ChessPieceData pieceAt = GetPieceAt(index);
		if (pieceAt == null || pieceAt.isWhite != king.isWhite || pieceAt.type != ChessPieceData.PieceType.Rook || pieceAt.timesMoved > 0)
		{
			return;
		}
		int num2 = (isKingSide ? 1 : (-1));
		int2 index2 = default(int2);
		for (int i = startPos.x + num2; i != num; i += num2)
		{
			((int2)(ref index2))._002Ector(i, startPos.y);
			if (GetPieceAt(index2) != null)
			{
				return;
			}
		}
		int2 position = default(int2);
		((int2)(ref position))._002Ector(startPos.x + num2, startPos.y);
		if (!IsSquareAttacked(position, king.isWhite))
		{
			SpecialMove specialMove = (isKingSide ? SpecialMove.ShortCastle : SpecialMove.LongCastle);
			validMoves.Add(new MoveData(king, startPos, null, new int2(isKingSide ? 6 : 2, startPos.y), enPassantPos, specialMove));
		}
	}

	public bool IsSquareAttacked(int2 position, bool isWhite)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		int2[] array = kingDirections;
		foreach (int2 val in array)
		{
			if (IsPieceAtPositionOfType(position + val, isWhite, ChessPieceData.PieceType.King))
			{
				return true;
			}
		}
		if (IsSlidingPieceAttacking(position, isWhite, isRookMovement: true))
		{
			return true;
		}
		if (IsSlidingPieceAttacking(position, isWhite, isRookMovement: false))
		{
			return true;
		}
		array = knightOffsets;
		foreach (int2 val2 in array)
		{
			if (IsPieceAtPositionOfType(position + val2, isWhite, ChessPieceData.PieceType.Knight))
			{
				return true;
			}
		}
		int num = (isWhite ? 1 : (-1));
		array = pawnCaptures;
		foreach (int2 val3 in array)
		{
			if (IsPieceAtPositionOfType(position + val3 * new int2(1, num), isWhite, ChessPieceData.PieceType.Pawn))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsSlidingPieceAttacking(int2 position, bool isWhite, bool isRookMovement)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		int2[] array = (isRookMovement ? rookDirections : bishopDirections);
		foreach (int2 val in array)
		{
			int2 val2 = position + val;
			while (IsValidPosition(val2))
			{
				ChessPieceData pieceAt = GetPieceAt(val2);
				if (pieceAt != null)
				{
					if (pieceAt.isWhite == isWhite)
					{
						break;
					}
					if (pieceAt.type == ChessPieceData.PieceType.Queen)
					{
						return true;
					}
					if ((isRookMovement ? 1 : 3) != (int)pieceAt.type)
					{
						break;
					}
					return true;
				}
				val2 += val;
			}
		}
		return false;
	}

	private bool IsPieceAtPositionOfType(int2 position, bool isWhite, ChessPieceData.PieceType type)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (IsValidPosition(position))
		{
			ChessPieceData pieceAt = GetPieceAt(position);
			if (pieceAt != null && pieceAt.isWhite != isWhite && pieceAt.type == type)
			{
				return true;
			}
		}
		return false;
	}
}
