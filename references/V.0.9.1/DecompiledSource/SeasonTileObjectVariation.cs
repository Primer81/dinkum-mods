using UnityEngine;

public class SeasonTileObjectVariation : MonoBehaviour
{
	private enum ShowingSeason
	{
		Summer,
		Autumn,
		Winter,
		Spring,
		SnowDay,
		NotSet
	}

	public float variationChance = 50f;

	public GameObject[] showInSeasons;

	public GameObject showOnSnowDay;

	private ShowingSeason currentlyShowing = ShowingSeason.NotSet;

	private void OnEnable()
	{
		checkSeason();
		SeasonManager.manage.changeSeasonEvent.AddListener(CheckSeasonDelayed);
	}

	private void OnDisable()
	{
		SeasonManager.manage.changeSeasonEvent.RemoveListener(CheckSeasonDelayed);
		currentlyShowing = ShowingSeason.NotSet;
	}

	private void CheckSeasonDelayed()
	{
		Invoke("checkSeason", Random.Range(0f, 0.5f));
	}

	public void checkSeason()
	{
		if ((bool)showOnSnowDay && SeasonManager.manage.IsSnowDay())
		{
			if (currentlyShowing != ShowingSeason.SnowDay)
			{
				currentlyShowing = ShowingSeason.SnowDay;
				ShowSnowDay();
			}
		}
		else if (currentlyShowing != (ShowingSeason)(WorldManager.Instance.month - 1))
		{
			currentlyShowing = (ShowingSeason)(WorldManager.Instance.month - 1);
			Random.InitState(Mathf.RoundToInt(base.transform.position.x * base.transform.position.z + base.transform.position.x - base.transform.position.z * base.transform.position.y) * NetworkMapSharer.Instance.mineSeed);
			if (Random.Range(0f, 100f) < variationChance + SeasonManager.manage.GetSeasonalPercentageDif())
			{
				EnableAndDisableSeasonObjects(WorldManager.Instance.month - 1);
			}
			else
			{
				EnableAndDisableSeasonObjects(0);
			}
		}
	}

	public void EnableAndDisableSeasonObjects(int idToEnable)
	{
		if (!showInSeasons[idToEnable].activeInHierarchy)
		{
			for (int i = 0; i < showInSeasons.Length; i++)
			{
				showInSeasons[i].SetActive(value: false);
			}
			showInSeasons[idToEnable].SetActive(value: true);
			if ((bool)showOnSnowDay)
			{
				showOnSnowDay.SetActive(value: false);
			}
		}
	}

	private void ShowSnowDay()
	{
		if (!showOnSnowDay.activeInHierarchy)
		{
			for (int i = 0; i < showInSeasons.Length; i++)
			{
				showInSeasons[i].SetActive(value: false);
			}
			showOnSnowDay.SetActive(value: true);
		}
	}
}
