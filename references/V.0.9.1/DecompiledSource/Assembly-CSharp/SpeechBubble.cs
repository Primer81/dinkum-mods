using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeechBubble : MonoBehaviour
{
	public TextMeshProUGUI bubbleText;

	public Transform characterSaying;

	public Image bubbleTail;

	public Image bubble;

	private Renderer charRen;

	public void setUpBubble(string setBubbleText, Transform followChar)
	{
		bubbleText.text = splitBubbleTextAndPlaceInNewLine(setBubbleText);
		characterSaying = followChar;
		StartCoroutine(moveChatBubble());
	}

	public string splitBubbleTextAndPlaceInNewLine(string checkString)
	{
		StringBuilder stringBuilder = new StringBuilder(checkString);
		int num = 0;
		for (int i = 0; i < checkString.Length; i++)
		{
			num++;
			if (num > 20 && stringBuilder[i] == ' ')
			{
				stringBuilder[i] = '\n';
				num = 0;
			}
		}
		return stringBuilder.ToString();
	}

	private IEnumerator moveChatBubble()
	{
		moveInstantly();
		charRen = characterSaying.GetComponentInChildren<Renderer>();
		while (true)
		{
			yield return null;
			if (!isGameObjectVisible())
			{
				base.transform.position = new Vector3(-500f, -500f, 0f);
				while (!isGameObjectVisible())
				{
					yield return null;
				}
				moveInstantly();
			}
			moveAndScale();
		}
	}

	public void moveAndScale()
	{
		base.transform.position = Vector3.Lerp(base.transform.position, CameraController.control.mainCamera.WorldToScreenPoint(characterSaying.position + Vector3.up * 2f + CameraController.control.mainCamera.transform.right), Time.deltaTime * 25f);
		float value = Vector3.Distance(CameraController.control.transform.position, characterSaying.transform.position);
		value = Mathf.Clamp(value, 8f, 32f);
		float num = 1f / (value / 8f);
		base.transform.localScale = new Vector3(num, num, num);
	}

	public void moveInstantly()
	{
		base.transform.position = CameraController.control.mainCamera.WorldToScreenPoint(characterSaying.position + Vector3.up * 2f + CameraController.control.mainCamera.transform.right);
		float value = Vector3.Distance(CameraController.control.transform.position, characterSaying.transform.position);
		value = Mathf.Clamp(value, 8f, 32f);
		float num = 1f / (value / 8f);
		base.transform.localScale = new Vector3(num, num, num);
	}

	public bool isGameObjectVisible()
	{
		if (!characterSaying)
		{
			return false;
		}
		if (Vector3.Distance(characterSaying.position, CameraController.control.transform.position) > 35f || (CameraController.control.transform.position.y >= -10f && characterSaying.position.y < -11f))
		{
			return false;
		}
		if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(CameraController.control.mainCamera), charRen.bounds))
		{
			return true;
		}
		return false;
	}

	public void fadeOutAndDestroy()
	{
		StartCoroutine(fadeAndThenDestroy());
	}

	private IEnumerator fadeAndThenDestroy()
	{
		float fadeTimer = 0f;
		Color whiteFade = Color.white;
		whiteFade.a = 0f;
		Color textColour = bubbleText.color;
		Color fadedTextColor = bubbleText.color;
		fadedTextColor.a = 0f;
		while (fadeTimer < 1f)
		{
			bubbleText.color = Color.Lerp(textColour, fadedTextColor, fadeTimer);
			bubble.color = Color.Lerp(Color.white, whiteFade, fadeTimer);
			bubbleTail.color = Color.Lerp(Color.white, whiteFade, fadeTimer * 2f);
			fadeTimer += Time.deltaTime * 10f;
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}
}
