using TMPro;
using UnityEngine;

public class Calendar : MonoBehaviour
{
	public static Calendar calendar;

	public GameObject calendarWindow;

	public CalendarButton[] calendarButtons;

	public TextMeshProUGUI seasonName;

	public TextMeshProUGUI[] weekNameTexts;

	public bool calendarOpen;

	private void Awake()
	{
		calendar = this;
	}

	public void openCalendar()
	{
		seasonName.text = RealWorldTimeLight.time.getSeasonName(WorldManager.Instance.month - 1);
		PopulateWeekNames();
		calendarOpen = true;
		calendarWindow.SetActive(value: true);
		fillCalendar();
	}

	public void updateCalendar()
	{
		seasonName.text = RealWorldTimeLight.time.getSeasonName(WorldManager.Instance.month - 1);
		PopulateWeekNames();
		fillCalendar();
	}

	public void closeCalendar()
	{
		calendarOpen = false;
		calendarWindow.SetActive(value: false);
	}

	public void fillCalendar()
	{
		for (int i = 0; i < calendarButtons.Length; i++)
		{
			calendarButtons[i].showDateDetails();
		}
	}

	private void PopulateWeekNames()
	{
		for (int i = 0; i < weekNameTexts.Length; i++)
		{
			string dayName = RealWorldTimeLight.time.getDayName(i);
			string text = ((dayName.Length > 3) ? dayName.Substring(0, 3) : dayName);
			weekNameTexts[i].text = text;
		}
	}
}
