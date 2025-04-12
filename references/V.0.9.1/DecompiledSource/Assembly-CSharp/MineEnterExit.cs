using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineEnterExit : MonoBehaviour
{
	public bool isEntrance = true;

	public static MineEnterExit mineEntrance;

	public static MineEnterExit mineExit;

	public AudioSource elevatorSounds;

	public AudioClip elevatorStarts;

	public AudioClip elevatorStops;

	public Transform position;

	public Transform[] allMinePositions;

	public Animator doorAnim;

	public GameObject exitToLock;

	private static bool doorsClosed;

	public GameObject lightsAnim;

	public AudioSource doorSound;

	public AudioClip doorOpen;

	public AudioClip doorClose;

	public LayerMask playerLayer;

	public ConversationObject allPlayersMustBeInLiftConvo;

	public InventoryItem rubyShard;

	public InventoryItem emeraldShard;

	public void Start()
	{
		if (isEntrance)
		{
			mineEntrance = this;
		}
		else
		{
			mineExit = this;
		}
	}

	public void OnEnable()
	{
		if (isEntrance)
		{
			mineEntrance = this;
		}
		else
		{
			mineExit = this;
		}
		if (doorsClosed)
		{
			exitToLock.gameObject.SetActive(value: false);
		}
		else
		{
			Invoke("openDoorOnEnable", 0.01f);
		}
	}

	public void openDoorOnDeath()
	{
		doorsClosed = false;
		exitToLock.gameObject.SetActive(value: true);
	}

	public void openDoorOnEnable()
	{
		doorAnim.SetTrigger("Open");
	}

	public void closeDoors()
	{
		doorsClosed = true;
		exitToLock.SetActive(value: false);
		doorSound.PlayOneShot(doorClose);
		doorAnim.SetTrigger("Close");
	}

	public void startElevatorTimer()
	{
		StartCoroutine(elevatorTimer());
	}

	public IEnumerator elevatorTimer()
	{
		float timer = 0f;
		CameraController.control.shakeScreenMax(0.45f, 0.45f);
		elevatorSounds.PlayOneShot(elevatorStarts);
		lightsAnim.SetActive(value: true);
		for (; timer < 5.5f; timer += Time.deltaTime)
		{
			CameraController.control.shakeScreenMax(0.1f, 0.1f);
			yield return null;
		}
		elevatorSounds.Stop();
		CameraController.control.shakeScreenMax(0.45f, 0.45f);
		elevatorSounds.PlayOneShot(elevatorStops);
		lightsAnim.SetActive(value: false);
		doorsClosed = false;
		yield return new WaitForSeconds(1f);
		while (!WorldManager.Instance.chunkRefreshCompleted)
		{
			yield return null;
		}
		exitToLock.gameObject.SetActive(value: true);
		doorSound.PlayOneShot(doorOpen);
		doorAnim.SetTrigger("Open");
		yield return new WaitForSeconds(1f);
		NetworkMapSharer.Instance.canUseMineControls = true;
	}

	public bool checkIfAllPlayersAreInElevator()
	{
		Vector3 halfExtents = new Vector3(9.4f, 5.5f, 6.2f) / 2f;
		Collider[] array = Physics.OverlapBox(base.transform.position, halfExtents, Quaternion.identity, playerLayer);
		List<CharMovement> list = new List<CharMovement>();
		for (int i = 0; i < array.Length; i++)
		{
			CharMovement componentInParent = array[i].GetComponentInParent<CharMovement>();
			if ((bool)componentInParent && !list.Contains(componentInParent))
			{
				list.Add(componentInParent);
			}
		}
		if (list.Count == NetworkNavMesh.nav.getPlayerCount())
		{
			return true;
		}
		return false;
	}
}
