using System.Collections;
using UnityEngine;

public class TubeManController : MonoBehaviour
{
	public SkinnedMeshRenderer meshRenderer;

	public Rigidbody[] bones;

	public Vector3[] originalPos;

	public Vector3[] originalRot;

	public float forceMax = 10f;

	public float forceMin = 10f;

	public float randomness = 5f;

	private float forceTimer;

	private float forceTimerMax = 3f;

	private float currentForce;

	private float desiredForce;

	private static WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();

	private void Start()
	{
		currentForce = forceMax;
		desiredForce = forceMax;
		if (bones == null || bones.Length == 0)
		{
			Debug.LogError("Bones not assigned in TubeManController");
		}
		originalPos = new Vector3[bones.Length];
		originalRot = new Vector3[bones.Length];
		for (int i = 0; i < bones.Length; i++)
		{
			originalPos[i] = bones[i].transform.localPosition;
			originalRot[i] = bones[i].transform.localEulerAngles;
		}
	}

	private void OnEnable()
	{
		StartCoroutine(WiggleManUpdate());
	}

	private void OnDisable()
	{
		ResetBones();
	}

	private IEnumerator WiggleManUpdate()
	{
		yield return new WaitForSeconds(0.5f);
		SetOriginalPosition();
		while (true)
		{
			if (meshRenderer.isVisible)
			{
				if (CameraController.control.IsCloseToCamera50(base.transform.position))
				{
					meshRenderer.quality = SkinQuality.Auto;
				}
				else
				{
					meshRenderer.quality = SkinQuality.Bone1;
				}
				ApplyRandomForces();
			}
			else
			{
				LockBones();
				while (!meshRenderer.isVisible)
				{
					yield return fixedWait;
				}
				UnlockLockBones();
			}
			yield return fixedWait;
		}
	}

	private void ApplyRandomForces()
	{
		for (int i = 1; i < bones.Length; i++)
		{
			Vector3 force = new Vector3(Random.Range(0f - randomness, randomness), Random.Range(0f, currentForce), Random.Range(0f - randomness, randomness));
			bones[i].AddForce(force, ForceMode.Force);
		}
		if (forceTimer == 0f)
		{
			desiredForce = Random.Range(forceMin, forceMax);
			forceTimer = Random.Range(1f, forceTimerMax);
		}
		forceTimer -= Time.deltaTime;
		currentForce = Mathf.Lerp(currentForce, desiredForce, Time.deltaTime);
	}

	public void SetOriginalPosition()
	{
		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].transform.localPosition = originalPos[i];
			bones[i].transform.localEulerAngles = originalRot[i];
			bones[i].velocity = Vector3.zero;
			bones[i].angularVelocity = Vector3.zero;
			bones[i].isKinematic = false;
		}
	}

	public void ResetBones()
	{
		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].transform.localPosition = originalPos[i];
			bones[i].transform.localEulerAngles = originalRot[i];
			bones[i].velocity = Vector3.zero;
			bones[i].angularVelocity = Vector3.zero;
			bones[i].isKinematic = true;
		}
	}

	public void LockBones()
	{
		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].isKinematic = true;
		}
	}

	public void UnlockLockBones()
	{
		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].isKinematic = false;
		}
	}
}
