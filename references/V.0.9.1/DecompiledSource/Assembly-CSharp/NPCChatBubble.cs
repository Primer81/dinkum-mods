using System.Collections;
using UnityEngine;

public class NPCChatBubble : MonoBehaviour
{
	public GameObject speechBubblePrefab;

	public GameObject myCurrentBubble;

	private Coroutine myRoutine;

	public void tryAndTalk(string text, float waitTime = 4f, bool overrideOldBubble = false)
	{
		if (Vector3.Distance(base.transform.position, CameraController.control.transform.position) > 35f || (CameraController.control.transform.position.y >= -10f && base.transform.position.y < -11f) || ((bool)myCurrentBubble && !overrideOldBubble))
		{
			return;
		}
		if (overrideOldBubble)
		{
			if (myRoutine != null)
			{
				StopCoroutine(myRoutine);
			}
			if ((bool)myCurrentBubble)
			{
				myCurrentBubble.GetComponent<SpeechBubble>().fadeOutAndDestroy();
			}
			myCurrentBubble = null;
			myRoutine = null;
		}
		myRoutine = StartCoroutine(saySomething(text, waitTime));
	}

	private IEnumerator saySomething(string text, float waitTime)
	{
		myCurrentBubble = Object.Instantiate(speechBubblePrefab, NotificationManager.manage.speechBubbleWindow);
		myCurrentBubble.GetComponent<SpeechBubble>().setUpBubble(text, base.transform);
		yield return new WaitForSeconds(waitTime);
		myCurrentBubble.GetComponent<SpeechBubble>().fadeOutAndDestroy();
		myCurrentBubble = null;
		myRoutine = null;
	}

	private void OnDisable()
	{
		if ((bool)myCurrentBubble)
		{
			myCurrentBubble.GetComponent<SpeechBubble>().fadeOutAndDestroy();
			myCurrentBubble = null;
			myRoutine = null;
		}
	}
}
