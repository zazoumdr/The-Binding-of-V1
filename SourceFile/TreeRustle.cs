using UnityEngine;
using UnityEngine.Audio;

public class TreeRustle : MonoBehaviour
{
	public MeshRenderer leafRenderer;

	public AudioClip[] audioClips;

	public AudioMixerGroup audioGroup;

	public float rustleDuration = 1f;

	public float rustleStrengthScale = 2f;

	public float rustleSpeedScale = 2f;

	private float baseRustleStrength;

	private float baseRustleSpeed;

	private MaterialPropertyBlock propertyBlock;

	private float time = 1f;

	private void Start()
	{
		propertyBlock = new MaterialPropertyBlock();
		Material sharedMaterial = leafRenderer.sharedMaterial;
		baseRustleStrength = sharedMaterial.GetFloat("_VertexNoiseAmplitude");
		baseRustleSpeed = sharedMaterial.GetFloat("_VertexNoiseSpeed");
	}

	private void Update()
	{
		if (time <= 1f)
		{
			time += Time.deltaTime / rustleDuration;
			leafRenderer.GetPropertyBlock(propertyBlock);
			propertyBlock.SetFloat("_VertexNoiseAmplitude", baseRustleStrength * Mathf.Lerp(rustleStrengthScale, 1f, time));
			propertyBlock.SetFloat("_VertexNoiseSpeed", baseRustleSpeed * Mathf.Lerp(rustleSpeedScale, 1f, time));
			leafRenderer.SetPropertyBlock(propertyBlock);
		}
	}

	public void DoRustle()
	{
		time = 0f;
		audioClips[Random.Range(0, audioClips.Length)].PlayClipAtPoint(audioGroup, base.transform.position, 128, 1f, 0.5f, Random.Range(0.8f, 1.1f), (AudioRolloffMode)1);
	}
}
