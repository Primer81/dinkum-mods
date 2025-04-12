using System;
using System.Collections;
using UnityEngine;

public class cameraWonderOnMenu : MonoBehaviour
{
	public static cameraWonderOnMenu wonder;

	public Animator fadeAnimator;

	public GameObject quickSlotToHide;

	public AudioListener listener;

	public GameObject menuToHide;

	private float showingHeight = 10f;

	private float camPosTimer;

	private float camPosMax = 25f;

	private void Awake()
	{
		wonder = this;
	}

	private void Start()
	{
		UnityEngine.Random.InitState(Environment.TickCount);
		quickSlotToHide.SetActive(value: false);
		float x = UnityEngine.Random.Range(500f, 1500f);
		float z = UnityEngine.Random.Range(500f, 1500f);
		base.transform.position = new Vector3(x, 10f, z);
	}

	public void finishLoading()
	{
		menuToHide.SetActive(value: true);
		StartCoroutine("cameraWonder");
	}

	private void OnEnable()
	{
		menuToHide.SetActive(value: false);
	}

	public void stopWonder()
	{
		StopCoroutine("cameraWonder");
	}

	public void OnDisable()
	{
		StopAllCoroutines();
		StopCoroutine("cameraWonder");
		StartCoroutine("cameraStop");
	}

	private IEnumerator cameraWonder()
	{
		fadeAnimator.SetBool("IsBlack", value: false);
		UnityEngine.Random.Range(500f, 1500f);
		UnityEngine.Random.Range(500f, 1500f);
		float dirToFloatX = UnityEngine.Random.Range(-1, 1);
		float dirToFloatY = UnityEngine.Random.Range(-1, 1);
		while (dirToFloatX == 0f && dirToFloatY == 0f)
		{
			dirToFloatX = UnityEngine.Random.Range(-1, 1);
			dirToFloatY = UnityEngine.Random.Range(-1, 1);
		}
		while (true)
		{
			showingHeight = Mathf.Lerp(showingHeight, (float)WorldManager.Instance.heightMap[(int)base.transform.position.x / 2, (int)base.transform.position.z / 2] + 5f, Time.deltaTime / 5f);
			Vector3 position = base.transform.position + new Vector3(dirToFloatX, 0f, dirToFloatY) * Time.deltaTime;
			position.y = showingHeight;
			base.transform.position = position;
			if (camPosTimer < camPosMax)
			{
				camPosTimer += Time.deltaTime;
				if (camPosTimer > camPosMax - 2f)
				{
					fadeAnimator.SetBool("IsBlack", value: true);
				}
			}
			else
			{
				camPosTimer = 0f;
				dirToFloatX = UnityEngine.Random.Range(-1, 1);
				dirToFloatY = UnityEngine.Random.Range(-1, 1);
				while (dirToFloatX == 0f && dirToFloatY == 0f)
				{
					dirToFloatX = UnityEngine.Random.Range(-1, 1);
					dirToFloatY = UnityEngine.Random.Range(-1, 1);
				}
				float x = UnityEngine.Random.Range(500f, 1500f);
				float z = UnityEngine.Random.Range(500f, 1500f);
				showingHeight = (float)WorldManager.Instance.heightMap[(int)base.transform.position.x / 2, (int)base.transform.position.z / 2] + 5f;
				base.transform.position = new Vector3(x, showingHeight, z);
				fadeAnimator.SetBool("IsBlack", value: false);
			}
			yield return null;
		}
	}

	private IEnumerator cameraStop()
	{
		fadeAnimator.SetBool("IsBlack", value: false);
		quickSlotToHide.SetActive(value: true);
		Inventory.Instance.menuOpen = false;
		yield return new WaitForSeconds(0.2f);
		fadeAnimator.gameObject.SetActive(value: false);
		Inventory.Instance.checkIfWindowIsNeeded();
		StopAllCoroutines();
	}
}
