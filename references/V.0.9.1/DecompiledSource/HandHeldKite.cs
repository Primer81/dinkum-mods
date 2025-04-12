using System.Collections;
using UnityEngine;

public class HandHeldKite : MonoBehaviour
{
	public Transform KiteToFly;

	private CharStatusEffects myChar;

	private NPCAI myAi;

	public Animator kiteAnimator;

	private float currentTurn;

	private Vector3 localKitePos;

	private float randomDistance = 35f;

	private float distanceTimer = 10f;

	private float distanceTimerMax = 3f;

	private float currentDistance = 20f;

	private Vector3 lastCharPos;

	private Vector3 targetPosition;

	public LayerMask collisionLayers;

	public AudioSource myAudio;

	private void Start()
	{
		myChar = GetComponentInParent<CharStatusEffects>();
		if ((bool)myChar)
		{
			targetPosition = KiteToFly.transform.position;
			lastCharPos = myChar.transform.position;
			myChar.ConnectKite(this);
			StartCoroutine(HandleKiteControls());
			return;
		}
		myAi = GetComponentInParent<NPCAI>();
		if ((bool)myAi)
		{
			targetPosition = KiteToFly.transform.position;
			lastCharPos = myAi.transform.position;
			StartCoroutine(HandleKiteForNPC());
		}
	}

	private void OnEnable()
	{
		SoundManager.Instance.onMasterChange.AddListener(SyncVolume);
		kiteAnimator.SetFloat("Offset", Random.Range(0f, 1f));
		SyncVolume();
	}

	private void OnDisable()
	{
		SoundManager.Instance.onMasterChange.RemoveListener(SyncVolume);
	}

	public void SyncVolume()
	{
		if ((bool)myAudio)
		{
			myAudio.volume = 0.1f * SoundManager.Instance.GetGlobalSoundVolume();
		}
	}

	private IEnumerator HandleKiteControls()
	{
		KiteToFly.parent = null;
		while (true)
		{
			if ((bool)myChar && myChar.isLocalPlayer)
			{
				if (Inventory.Instance.CanMoveCharacter() && InputMaster.input.UseHeld())
				{
					if (!CameraController.control.isInAimCam())
					{
						CameraController.control.enterAimCamera(turnOnCrossHeir: false);
					}
				}
				else if (CameraController.control.isInAimCam())
				{
					CameraController.control.exitAimCamera();
				}
			}
			HandleRandomDistance();
			if (Vector3.Distance(myChar.transform.position, lastCharPos) >= 0.01f)
			{
				targetPosition = Vector3.Lerp(targetPosition, CameraController.control.mainCamera.transform.position + Vector3.up * 5f + -myChar.transform.transform.forward * (currentDistance / 3f), Time.deltaTime * 2f);
			}
			else if (CameraController.control.isInAimCam())
			{
				targetPosition = Vector3.Lerp(targetPosition, CameraController.control.mainCamera.transform.position + Vector3.up * 2f + CameraController.control.mainCamera.transform.forward * currentDistance, Time.deltaTime * 3f);
			}
			else
			{
				targetPosition = Vector3.Lerp(targetPosition, CameraController.control.mainCamera.transform.position + Vector3.up * 5f + CameraController.control.mainCamera.transform.forward * currentDistance, Time.deltaTime * 3f);
			}
			lastCharPos = myChar.transform.position;
			if (WorldManager.Instance.isPositionOnMap(targetPosition))
			{
				targetPosition.y = Mathf.Clamp(targetPosition.y, (float)WorldManager.Instance.heightMap[Mathf.RoundToInt(targetPosition.x / 2f), Mathf.RoundToInt(targetPosition.z / 2f)] + 7f, 100f);
			}
			localKitePos = GetNewPosAfterRay(targetPosition);
			if (myChar.GetNetworkKitePosition() != Vector3.zero)
			{
				KiteToFly.position = Vector3.Lerp(KiteToFly.position, myChar.GetNetworkKitePosition(), Time.deltaTime * 2f);
			}
			Vector3 forward = KiteToFly.forward;
			KiteToFly.LookAt(base.transform);
			SetKiteTurning(forward, KiteToFly.forward);
			yield return null;
		}
	}

	private IEnumerator HandleKiteForNPC()
	{
		KiteToFly.parent = null;
		while (true)
		{
			HandleRandomDistance();
			if (Vector3.Distance(myAi.transform.position, lastCharPos) >= 0.01f)
			{
				targetPosition = Vector3.Lerp(targetPosition, myAi.transform.position + Vector3.up * 6f + -myAi.transform.transform.forward * (currentDistance / 5.5f), Time.deltaTime / 2f);
			}
			else
			{
				targetPosition = Vector3.Lerp(targetPosition, myAi.transform.position + Vector3.up * 6f + myAi.transform.forward * (currentDistance / 3f), Time.deltaTime / 2f);
			}
			lastCharPos = myAi.transform.position;
			if (WorldManager.Instance.isPositionOnMap(targetPosition))
			{
				targetPosition.y = Mathf.Clamp(targetPosition.y, (float)WorldManager.Instance.heightMap[Mathf.RoundToInt(targetPosition.x / 2f), Mathf.RoundToInt(targetPosition.z / 2f)] + 7f, 100f);
			}
			localKitePos = GetNewPosAfterRay(targetPosition);
			KiteToFly.position = targetPosition;
			Vector3 forward = KiteToFly.forward;
			KiteToFly.LookAt(base.transform);
			SetKiteTurning(forward, KiteToFly.forward);
			yield return null;
		}
	}

	private void HandleRandomDistance()
	{
		if (distanceTimer >= distanceTimerMax)
		{
			randomDistance = Random.Range(20f, 28f);
			distanceTimer = 0f;
			distanceTimerMax = Random.Range(2f, 5f);
		}
		else
		{
			distanceTimer += Time.deltaTime;
		}
		currentDistance = Mathf.Lerp(currentDistance, randomDistance, Time.deltaTime / 2f);
	}

	public Vector3 GetNewPosAfterRay(Vector3 checkPosition)
	{
		Vector3 vector = base.transform.position;
		if ((bool)myChar)
		{
			vector = myChar.transform.position + Vector3.up;
		}
		if ((bool)myAi)
		{
			vector = myAi.transform.position + Vector3.up;
		}
		Vector3 direction = checkPosition - vector;
		float magnitude = direction.magnitude;
		if (Physics.Raycast(vector, direction, out var hitInfo, magnitude, collisionLayers))
		{
			return hitInfo.point;
		}
		return checkPosition;
	}

	public void SetKiteTurning(Vector3 fwd, Vector3 targetDir)
	{
		currentTurn = Mathf.Lerp(currentTurn, LeftOrRight(fwd, targetDir, Vector3.up), Time.deltaTime * 4f);
		kiteAnimator.SetFloat("Turn", currentTurn);
	}

	public float LeftOrRight(Vector3 fwd, Vector3 targetDir, Vector3 up)
	{
		float num = Vector3.Dot(Vector3.Cross(fwd, targetDir), up);
		if (num > 0.005f)
		{
			if (num > 0.01f)
			{
				return 1f;
			}
			return 0.5f;
		}
		if (num < -0.005f)
		{
			if (num < -0.01f)
			{
				return -1f;
			}
			return -0.5f;
		}
		return 0f;
	}

	public Vector3 GetKitePos()
	{
		return localKitePos;
	}

	private void OnDestroy()
	{
		if (KiteToFly.gameObject != null)
		{
			Object.Destroy(KiteToFly.gameObject);
		}
	}
}
