using UnityEngine;

public class TimeBasedEnableAndDisable : MonoBehaviour
{
	public int dayToShow = 1;

	public GameObject[] showOnCorrectDay;

	public GameObject[] showOnIncorrectDay;

	public void EnableAndDisable()
	{
		bool flag = WorldManager.Instance.day == dayToShow;
		for (int i = 0; i < showOnCorrectDay.Length; i++)
		{
			if (flag)
			{
				showOnCorrectDay[i].SetActive(value: true);
			}
			else
			{
				showOnCorrectDay[i].SetActive(value: false);
			}
		}
		for (int j = 0; j < showOnIncorrectDay.Length; j++)
		{
			if (flag)
			{
				showOnIncorrectDay[j].SetActive(value: false);
			}
			else
			{
				showOnIncorrectDay[j].SetActive(value: true);
			}
		}
	}

	private void OnEnable()
	{
		WorldManager.Instance.changeDayEvent.AddListener(EnableAndDisable);
		EnableAndDisable();
	}

	private void OnDisable()
	{
		WorldManager.Instance.changeDayEvent.RemoveListener(EnableAndDisable);
	}
}
