using ULTRAKILL.Enemy;
using UnityEngine;

public class TargetPredictionTest : MonoBehaviour
{
	public bool predictPlayerPosition = true;

	public bool throughPortals;

	public float time;

	public Transform previewPosition;

	public Transform previewPredictedPosition;

	public Transform previewGravityPosition;

	public Transform previewAssumeGroundMovementPosition;

	public Transform previewGravityAndGroundMovementPosition;

	[Header("Preview Toggles")]
	public bool showPosition = true;

	public bool showPredicted = true;

	public bool showGravity = true;

	public bool showGroundMovement = true;

	public bool showGravityAndGroundMovement = true;

	private Vision vision;

	private VisionQuery query;

	private void Start()
	{
		vision = new Vision(base.transform.position, new VisionTypeFilter(TargetType.PLAYER));
		query = new VisionQuery("Player", (TargetDataRef td) => td.target.isPlayer && ((!throughPortals) ? td.portals.IsEmpty : (!td.portals.IsEmpty)));
	}

	private void Update()
	{
		if (!predictPlayerPosition)
		{
			return;
		}
		vision.UpdateSourcePos(base.transform.position);
		_ = MonoSingleton<CameraController>.Instance.cam;
		if (vision.TrySee(query, out var data))
		{
			if (showPosition)
			{
				previewPosition.position = data.position;
			}
			if (showPredicted)
			{
				previewPredictedPosition.position = data.PredictTargetPosition(time);
			}
			if (showGravity)
			{
				previewGravityPosition.position = data.PredictTargetPosition(time, includeGravity: true);
			}
			if (showGroundMovement)
			{
				previewAssumeGroundMovementPosition.position = data.PredictTargetPosition(time, includeGravity: false, assumeGroundMovement: true);
			}
			if (showGravityAndGroundMovement)
			{
				previewGravityAndGroundMovementPosition.position = data.PredictTargetPosition(time, includeGravity: true, assumeGroundMovement: true);
			}
		}
		else
		{
			Debug.Log("No see");
		}
	}
}
