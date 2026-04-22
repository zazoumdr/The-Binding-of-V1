using UnityEngine;

public class GoreSplatter : MonoBehaviour
{
	public int bloodAbsorberCount;

	public BSType bloodSplatterType;

	private Rigidbody rb;

	private Vector3 direction;

	private float force;

	private bool goreOver;

	private int touchedCollisions;

	private Vector3 defaultScale;

	private bool freezeGore;

	private bool foundParent;

	private Vector3 startPos;

	private bool detectCollisions = true;

	[HideInInspector]
	public bool beenFlung;

	private void OnEnable()
	{
		Invoke("SlowUpdate", 1f);
		if (!beenFlung)
		{
			beenFlung = true;
			if (!rb)
			{
				TryGetComponent<Rigidbody>(out rb);
			}
			StopGore();
			if (!detectCollisions)
			{
				rb.detectCollisions = true;
			}
			defaultScale = base.transform.localScale;
			direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
			force = Random.Range(20, 60);
			startPos = base.transform.position;
			bloodAbsorberCount = 0;
			if (StockMapInfo.Instance.continuousGibCollisions)
			{
				rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
			}
			rb.AddForce(direction * force, ForceMode.VelocityChange);
			rb.rotation = Random.rotation;
			freezeGore = !MonoSingleton<BloodsplatterManager>.Instance.neverFreezeGibs && MonoSingleton<PrefsManager>.Instance.GetBoolLocal("freezeGore");
			if (freezeGore)
			{
				Invoke("Repool", 5f);
			}
			if (StockMapInfo.Instance.removeGibsWithoutAbsorbers)
			{
				Invoke("RepoolIfNoAbsorber", StockMapInfo.Instance.gibRemoveTime);
			}
		}
	}

	private void OnDisable()
	{
		CancelInvoke("SlowUpdate");
	}

	private void RepoolIfNoAbsorber()
	{
		if (bloodAbsorberCount <= 0)
		{
			Repool();
		}
		else if (StockMapInfo.Instance.removeGibsWithoutAbsorbers)
		{
			Invoke("RepoolIfNoAbsorber", StockMapInfo.Instance.gibRemoveTime);
		}
	}

	public void Repool()
	{
		beenFlung = false;
		MonoSingleton<BloodsplatterManager>.Instance.RepoolGore(base.gameObject, bloodSplatterType);
	}

	private void SlowUpdate()
	{
		if (freezeGore && goreOver && rb.velocity.y > -0.1f && rb.velocity.y < 0.1f)
		{
			StopGore();
		}
		if (Vector3.Distance(base.transform.position, startPos) > 1000f)
		{
			Repool();
		}
		if ((bool)rb && base.transform.position.y > 666f && rb.velocity.y > 0f)
		{
			rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
		}
		if (base.isActiveAndEnabled)
		{
			Invoke("SlowUpdate", 1f);
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if (freezeGore && LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment) && (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Floor")))
		{
			touchedCollisions++;
			goreOver = true;
		}
	}

	private void OnCollisionExit(Collision other)
	{
		if (freezeGore && LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment) && (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Floor")))
		{
			touchedCollisions--;
			if (touchedCollisions <= 0)
			{
				goreOver = false;
			}
		}
	}

	private void StopGore()
	{
		detectCollisions = false;
		rb.velocity = Vector3.zero;
		rb.detectCollisions = false;
	}
}
