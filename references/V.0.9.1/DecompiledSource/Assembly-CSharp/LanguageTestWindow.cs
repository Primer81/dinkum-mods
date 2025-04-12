using System.Collections;
using I2.Loc;
using UnityEngine;

public class LanguageTestWindow : MonoBehaviour
{
	public GameObject testWindow;

	public bool windowOpen;

	private void OnEnable()
	{
		StartCoroutine(UpdateWhileOpen());
	}

	private void OnDisable()
	{
		if (windowOpen)
		{
			testWindow.SetActive(value: false);
		}
	}

	private IEnumerator UpdateWhileOpen()
	{
		yield return new WaitForSeconds(0.5f);
		while (true)
		{
			if (Input.GetKeyDown(KeyCode.F1))
			{
				testWindow.SetActive(value: true);
				windowOpen = true;
			}
			yield return null;
		}
	}

	public void UpdateFromLiveDoc()
	{
		LocalizationManager.Sources[0].Import_Google(ForceUpdate: true, justCheck: false);
	}
}
