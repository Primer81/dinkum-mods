using System.Collections;
using UnityEngine;

public class ChangeOverDay : MonoBehaviour
{
	public GameObject[] objectsToShow;

	public int[] hourShownUpTo;

	private void OnEnable()
	{
		StartCoroutine(checkTime());
	}

	private IEnumerator checkTime()
	{
		new WaitForSeconds(1f);
		while (true)
		{
			yield return new WaitForSeconds(1f);
			bool flag = false;
			int num = RealWorldTimeLight.time.currentHour;
			if (num == 0)
			{
				num = 24;
			}
			for (int i = 0; i < objectsToShow.Length; i++)
			{
				if (!flag && num < hourShownUpTo[i])
				{
					objectsToShow[i].SetActive(value: true);
					flag = true;
				}
				else
				{
					objectsToShow[i].SetActive(value: false);
				}
			}
		}
	}
}
