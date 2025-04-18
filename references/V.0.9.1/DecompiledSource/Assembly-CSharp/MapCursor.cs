using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapCursor : MonoBehaviour
{
	public GameObject topLeft;

	public GameObject topRight;

	public GameObject bottomLeft;

	public GameObject bottomRight;

	private Vector2 topLeftPos = new Vector2(-1f, 1f);

	private Vector2 topRightPos = new Vector2(1f, 1f);

	private Vector2 bottomLeftPos = new Vector2(-1f, -1f);

	private Vector2 bottomRightPos = new Vector2(1f, -1f);

	private Coroutine pressDelay;

	public Image placePingTrans;

	public Color fadedPlaceColor;

	public GameObject nameTagObject;

	public TextMeshProUGUI nameTagText;

	private bool pressing;

	private bool hovering;

	private void OnEnable()
	{
		StartCoroutine(cursorWorks());
		placePingTrans.transform.localScale = new Vector3(1f, 1f, 1f);
		placePingTrans.color = fadedPlaceColor;
	}

	public void SetHovering(mapIcon mIcon)
	{
		if ((object)mIcon != null && !mIcon.IconName.Equals(string.Empty))
		{
			ShowNameTag(mIcon);
		}
		else
		{
			nameTagObject.SetActive(value: false);
		}
	}

	public void setPressing(bool isPressing)
	{
		pressing = isPressing;
	}

	public void PressDownOnButton()
	{
		if (pressDelay != null)
		{
			StopCoroutine(pressDelay);
		}
		pressDelay = StartCoroutine(pressingDelay());
	}

	public void PlaceButtonPing()
	{
		StartCoroutine(placePing());
	}

	private IEnumerator cursorWorks()
	{
		while (true)
		{
			yield return null;
			float changeTimer = 0f;
			if (pressing)
			{
				while (pressing)
				{
					yield return null;
					topLeft.transform.localPosition = Vector2.Lerp(topLeft.transform.localPosition, topLeftPos * 5f, changeTimer);
					topRight.transform.localPosition = Vector2.Lerp(topRight.transform.localPosition, topRightPos * 5f, changeTimer);
					bottomLeft.transform.localPosition = Vector2.Lerp(bottomLeft.transform.localPosition, bottomLeftPos * 5f, changeTimer);
					bottomRight.transform.localPosition = Vector2.Lerp(bottomRight.transform.localPosition, bottomRightPos * 5f, changeTimer);
					changeTimer = Mathf.Clamp01(changeTimer + Time.deltaTime * 4f);
				}
			}
			else if (hovering)
			{
				while (hovering && !pressing)
				{
					yield return null;
					topLeft.transform.localPosition = Vector2.Lerp(topLeft.transform.localPosition, topLeftPos * 15f, changeTimer);
					topRight.transform.localPosition = Vector2.Lerp(topRight.transform.localPosition, topRightPos * 15f, changeTimer);
					bottomLeft.transform.localPosition = Vector2.Lerp(bottomLeft.transform.localPosition, bottomLeftPos * 15f, changeTimer);
					bottomRight.transform.localPosition = Vector2.Lerp(bottomRight.transform.localPosition, bottomRightPos * 15f, changeTimer);
					changeTimer = Mathf.Clamp01(changeTimer + Time.deltaTime * 4f);
				}
			}
			else
			{
				while (!hovering && !pressing)
				{
					yield return null;
					topLeft.transform.localPosition = Vector2.Lerp(topLeft.transform.localPosition, topLeftPos * 10f, changeTimer);
					topRight.transform.localPosition = Vector2.Lerp(topRight.transform.localPosition, topRightPos * 10f, changeTimer);
					bottomLeft.transform.localPosition = Vector2.Lerp(bottomLeft.transform.localPosition, bottomLeftPos * 10f, changeTimer);
					bottomRight.transform.localPosition = Vector2.Lerp(bottomRight.transform.localPosition, bottomRightPos * 10f, changeTimer);
					changeTimer = Mathf.Clamp01(changeTimer + Time.deltaTime * 4f);
				}
			}
		}
	}

	private void ShowNameTag(mapIcon icon)
	{
		if (icon.IconName.Contains("<buildingName>"))
		{
			nameTagText.SetText(ConversationGenerator.generate.GetBuildingName(icon.IconName.Replace("<buildingName>", "")));
		}
		else
		{
			nameTagText.SetText(icon.IconName);
		}
		nameTagObject.SetActive(value: true);
	}

	private IEnumerator pressingDelay()
	{
		float timer = 0f;
		pressing = true;
		for (; timer < 0.25f; timer += Time.deltaTime)
		{
			yield return null;
		}
		pressing = false;
	}

	private IEnumerator placePing()
	{
		float timer = 0f;
		while (timer < 0.5f)
		{
			yield return null;
			timer += Time.deltaTime;
			placePingTrans.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(1f, 1f, 1f), timer * 2f);
			placePingTrans.color = Color.Lerp(Color.white, fadedPlaceColor, timer * 2f);
		}
	}
}
