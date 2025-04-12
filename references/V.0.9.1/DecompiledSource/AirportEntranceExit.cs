using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirportEntranceExit : MonoBehaviour
{
	[SerializeField]
	private bool isEntrance = true;

	public static AirportEntranceExit entrance;

	public static AirportEntranceExit exit;

	private bool doorsClosed;

	public GameObject exitToLock;

	public ConversationObject allPlayersMustBeInConversation;

	public LayerMask playerLayer;

	public Animator flyingAnim;

	public AudioSource flyingSound;

	public Collider doorCollider;

	public Transform dooranimated;

	private Vector3 originalDoorSize;

	public ASound airshipLands;

	public ASound doorSound;

	public Transform offIslandSpawnPoint;

	private void Start()
	{
		originalDoorSize = dooranimated.localScale;
	}

	public void OnEnable()
	{
		if (isEntrance)
		{
			entrance = this;
		}
		else
		{
			exit = this;
		}
		if (doorsClosed)
		{
			exitToLock.gameObject.SetActive(value: false);
		}
		else
		{
			Invoke("OpenDoorOnEnable", 0.01f);
		}
	}

	public void OpenDoorOnEnable()
	{
	}

	public void CloseDoors()
	{
		StartCoroutine(CloseDoorRoutine());
	}

	public void StartFlyingAnimation()
	{
		StartCoroutine(FlyingEffectRoutine());
	}

	private IEnumerator FlyingEffectRoutine()
	{
		dooranimated.localScale = Vector3.one;
		doorCollider.enabled = true;
		flyingSound.volume = 0f;
		flyingSound.pitch = 0.8f;
		float desiredPitch = 1.25f;
		float desiredVolume = 0.15f;
		float timer4 = 0f;
		flyingSound.Play();
		flyingAnim.SetBool("Flying", value: true);
		flyingAnim.SetFloat("FlightSpeed", 0f);
		while (timer4 < 4f)
		{
			timer4 += Time.deltaTime;
			yield return null;
			CameraController.control.shakeScreenMax(0.15f, 0.15f);
			flyingSound.volume = Mathf.Lerp(0f, desiredVolume * SoundManager.Instance.getSoundVolumeForChange(), timer4 / 4f);
			flyingSound.pitch = Mathf.Lerp(0.8f, desiredPitch, timer4 / 4f);
			float value = Mathf.Lerp(0f, 1f, timer4 / 4f);
			flyingAnim.SetFloat("FlightSpeed", value);
		}
		timer4 = 0f;
		while (timer4 < 1f)
		{
			timer4 += Time.deltaTime;
			CameraController.control.shakeScreenMax(0.05f, 0.05f);
			yield return null;
		}
		timer4 = 0f;
		while (timer4 < 25f)
		{
			timer4 += Time.deltaTime;
			yield return null;
			CameraController.control.shakeScreenMax(0.05f, 0.05f);
			flyingSound.pitch = Mathf.Lerp(desiredPitch, 0.8f, timer4 / 25f);
			float value = Mathf.Lerp(1f, 0f, timer4 / 25f);
			flyingAnim.SetFloat("FlightSpeed", value);
		}
		yield return new WaitForSeconds(1f);
		flyingAnim.SetBool("Flying", value: false);
		SoundManager.Instance.play2DSound(airshipLands);
		CameraController.control.shakeScreenMax(0.15f, 0.15f);
		timer4 = 0f;
		while (timer4 < 5f)
		{
			timer4 += Time.deltaTime;
			yield return null;
			flyingSound.volume = Mathf.Lerp(desiredVolume * SoundManager.Instance.getSoundVolumeForChange(), 0f, timer4 / 5f);
		}
		flyingSound.Stop();
		StartCoroutine(OpenDoorRoutine());
		NetworkMapSharer.Instance.canUseMineControls = true;
	}

	public bool CheckIfAllPlayersAreInAirShip()
	{
		Vector3 halfExtents = new Vector3(6.15f, 7f, 6.45f) / 2f;
		Collider[] array = Physics.OverlapBox(offIslandSpawnPoint.position, halfExtents, Quaternion.identity, playerLayer);
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

	private IEnumerator CloseDoorRoutine()
	{
		doorCollider.enabled = true;
		float timer = 0f;
		while (timer < 0.5f)
		{
			dooranimated.localScale = Vector3.Lerp(originalDoorSize, Vector3.one, timer * 2f);
			timer += Time.deltaTime;
			yield return null;
		}
	}

	private IEnumerator OpenDoorRoutine()
	{
		float timer = 0f;
		while (timer < 0.5f)
		{
			dooranimated.localScale = Vector3.Lerp(Vector3.one, originalDoorSize, timer * 2f);
			timer += Time.deltaTime;
			yield return null;
		}
		doorCollider.enabled = false;
	}
}
