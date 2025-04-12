using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateTeleporter : MonoBehaviour
{
	public GameObject signObject;

	public GameObject pad;

	private List<CharMovement> charsInCol = new List<CharMovement>();

	private bool activationTimerComplete;

	private void OnTriggerEnter(Collider other)
	{
		CharMovement componentInParent = other.GetComponentInParent<CharMovement>();
		if ((bool)componentInParent && !charsInCol.Contains(componentInParent))
		{
			if (componentInParent.isLocalPlayer && activationTimerComplete)
			{
				if (moreThanOneTeleOn())
				{
					MenuButtonsTop.menu.switchToMap();
					RenderMap.Instance.canTele = true;
					StartCoroutine(centreCharWhileOnPad(componentInParent.transform));
				}
				else
				{
					Invoke("readSignDelay", 1f);
				}
			}
			charsInCol.Add(componentInParent);
		}
		checkPad();
		checkSign();
	}

	private void OnTriggerExit(Collider other)
	{
		CharMovement componentInParent = other.GetComponentInParent<CharMovement>();
		if ((bool)componentInParent)
		{
			if (componentInParent.isLocalPlayer)
			{
				RenderMap.Instance.canTele = false;
			}
			charsInCol.Remove(componentInParent);
		}
		checkPad();
		checkSign();
	}

	private void OnEnable()
	{
		activationTimerComplete = false;
		charsInCol.Clear();
		pad.SetActive(value: false);
		signObject.SetActive(value: false);
		Invoke("activateTimer", 1f);
	}

	public void activateTimer()
	{
		activationTimerComplete = true;
	}

	public void readSignDelay()
	{
		if (charsInCol.Contains(NetworkMapSharer.Instance.localChar))
		{
			signObject.GetComponent<ReadableSign>().readSign();
		}
	}

	public void checkPad()
	{
		if (charsInCol.Count > 0)
		{
			pad.SetActive(value: true);
		}
		else
		{
			pad.SetActive(value: false);
		}
	}

	public void checkSign()
	{
		if (!charsInCol.Contains(NetworkMapSharer.Instance.localChar))
		{
			signObject.SetActive(value: false);
		}
	}

	private IEnumerator centreCharWhileOnPad(Transform localChar)
	{
		while (RenderMap.Instance.mapOpen)
		{
			yield return null;
			localChar.position = Vector3.Lerp(localChar.position, base.transform.position, Time.deltaTime * 2f);
			localChar.rotation = Quaternion.Lerp(localChar.rotation, base.transform.rotation, Time.deltaTime * 2f);
		}
	}

	public bool moreThanOneTeleOn()
	{
		int num = 0;
		if (NetworkMapSharer.Instance.privateTowerPos != Vector2.zero)
		{
			num++;
		}
		if (NetworkMapSharer.Instance.northOn)
		{
			num++;
		}
		if (NetworkMapSharer.Instance.eastOn)
		{
			num++;
		}
		if (NetworkMapSharer.Instance.southOn)
		{
			num++;
		}
		if (NetworkMapSharer.Instance.westOn)
		{
			num++;
		}
		return num > 1;
	}
}
