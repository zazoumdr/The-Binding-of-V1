using Sandbox;
using Unity.Mathematics;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
	public ChessPieceData Data;

	public ChessPieceData.PieceType type;

	public bool isWhite = true;

	public bool queenSide;

	private bool positionDirty;

	private Quaternion startRot;

	private Rigidbody rb;

	private SandboxProp sbp;

	public AudioSource snapSound;

	[HideInInspector]
	public AudioSource dragSound;

	public GameObject breakEffect;

	public GameObject teleportEffect;

	public GameObject promotionEffect;

	public int timesMoved;

	public bool autoControl;

	public bool initialized;

	public GameObject promotionPanel;

	public float boardHeight = -900f;

	private ChessManager chessMan;

	private ChessManager.MoveData promotionMove;

	private void Awake()
	{
		chessMan = MonoSingleton<ChessManager>.Instance;
		sbp = GetComponent<SandboxProp>();
		if (!(Object)(object)dragSound)
		{
			dragSound = GetComponent<AudioSource>();
		}
	}

	private void Start()
	{
		if (promotionPanel != null)
		{
			promotionPanel.SetActive(value: false);
		}
		float y = chessMan.GetComponent<Collider>().bounds.max.y;
		float y2 = base.transform.GetChild(0).GetComponent<Collider>().bounds.min.y;
		boardHeight = base.transform.position.y + (y - y2);
		base.transform.position = new Vector3(base.transform.position.x, boardHeight, base.transform.position.z);
		if (!initialized)
		{
			rb = GetComponent<Rigidbody>();
			startRot = base.transform.rotation;
			Data = new ChessPieceData(type, isWhite, queenSide)
			{
				timesMoved = timesMoved,
				autoControl = autoControl
			};
			if (isWhite)
			{
				SetAutoControl(chessMan.whiteIsBot);
			}
			if (!isWhite)
			{
				SetAutoControl(chessMan.blackIsBot);
			}
			chessMan.InitializePiece(this);
			PieceCanMove(canMove: false);
			initialized = true;
		}
	}

	public void SetAutoControl(bool useAutoControl)
	{
		autoControl = useAutoControl;
		Data.autoControl = autoControl;
		sbp.disallowManipulation = autoControl;
	}

	private void Update()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (!autoControl)
		{
			if (chessMan.gameLocked)
			{
				PieceCanMove(canMove: false);
			}
			else
			{
				PieceCanMove(isWhite == chessMan.isWhiteTurn);
			}
			if (sbp.frozen && !positionDirty)
			{
				int2 index = chessMan.WorldPositionToIndex(base.transform.position);
				chessMan.GetLegalMoves(index);
				chessMan.DisplayValidMoves();
				positionDirty = true;
			}
			if (positionDirty && !sbp.frozen)
			{
				rb.isKinematic = false;
			}
		}
	}

	public void PieceCanMove(bool canMove)
	{
		sbp.disallowManipulation = !canMove;
	}

	private void OnCollisionEnter(Collision collider)
	{
		if (!autoControl && (!isWhite || !chessMan.whiteIsBot) && (isWhite || !chessMan.blackIsBot) && !sbp.frozen)
		{
			if (positionDirty && collider.gameObject.TryGetComponent<ChessManager>(out var _))
			{
				chessMan.FindMoveAtWorldPosition(this);
				Object.Instantiate<AudioSource>(snapSound, base.transform.position, Quaternion.identity);
			}
			if (positionDirty && collider.gameObject.TryGetComponent<ChessPiece>(out var _))
			{
				chessMan.FindMoveAtWorldPosition(this);
				Object.Instantiate<AudioSource>(snapSound, base.transform.position, Quaternion.identity);
			}
		}
	}

	public void UpdatePosition(int2 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position2 = chessMan.IndexToWorldPosition(position, boardHeight);
		base.transform.SetPositionAndRotation(position2, startRot);
		positionDirty = false;
		rb.isKinematic = true;
	}

	public void ShowPromotionGUI(ChessManager.MoveData move)
	{
		promotionMove = move;
		promotionPanel.SetActive(value: true);
	}

	public void PlayerPromotePiece(int type)
	{
		ChessPieceData.PieceType promotionType = ChessPieceData.PieceType.Pawn;
		switch (type)
		{
		case 0:
			promotionType = ChessPieceData.PieceType.Queen;
			break;
		case 1:
			promotionType = ChessPieceData.PieceType.Rook;
			break;
		case 2:
			promotionType = ChessPieceData.PieceType.Bishop;
			break;
		case 3:
			promotionType = ChessPieceData.PieceType.Knight;
			break;
		}
		Data.type = promotionType;
		promotionPanel.SetActive(value: false);
		PromoteVisualPiece();
		promotionMove.PromotionType = promotionType;
		chessMan.StylishMove(promotionMove);
		chessMan.UpdateGame(promotionMove);
	}

	public void PromoteVisualPiece()
	{
		base.transform.GetChild(0).gameObject.SetActive(value: false);
		int num = ((!Data.isWhite) ? 4 : 0);
		switch (Data.type)
		{
		case ChessPieceData.PieceType.Rook:
			num++;
			break;
		case ChessPieceData.PieceType.Bishop:
			num += 2;
			break;
		case ChessPieceData.PieceType.Knight:
			num += 3;
			break;
		}
		GameObject obj = Object.Instantiate(chessMan.originalExtras.transform.GetChild(num).gameObject, base.transform);
		obj.transform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
		obj.SetActive(value: true);
		CapsuleCollider componentInChildren = obj.GetComponentInChildren<CapsuleCollider>();
		Vector3 position = componentInChildren.transform.TransformPoint(componentInChildren.center);
		CapsuleCollider component = GetComponent<CapsuleCollider>();
		component.height = componentInChildren.height;
		component.radius = componentInChildren.radius;
		component.center = component.transform.InverseTransformPoint(position);
		Object.Destroy(componentInChildren);
	}

	public void Captured()
	{
		Object.Instantiate(breakEffect, base.transform.position, Quaternion.identity);
		base.gameObject.SetActive(value: false);
	}
}
