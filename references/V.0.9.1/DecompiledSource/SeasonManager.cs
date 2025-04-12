using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SeasonManager : MonoBehaviour
{
	public static SeasonManager manage;

	[Header("Day Length Difference------")]
	public int summerLengthDif = 3;

	public int autumnLengthDif = 1;

	public int winterLengthDif;

	public int springLengthDif = 2;

	public SeasonalMaterials[] allSeasonMats;

	public ASound snowSteps;

	public UnityEvent changeSeasonEvent = new UnityEvent();

	[Header("Sun Colours------")]
	public Color summerSunColour;

	public Color autumnSunColour;

	public Color springSunColour;

	public Color winterSunColour;

	[Header("SeasonParticles------")]
	public GameObject springParticles;

	[Header("Grow Back Tables------")]
	public SeasonalGrowBack bushlandGrowBack;

	public SeasonalGrowBack desertRainGrowBack;

	public SeasonalGrowBack tropicalGrowBack;

	public SeasonalGrowBack coldGrowBack;

	[Header("Autumn Mushrooms------")]
	public TileObject fieldMushroom;

	public TileObject slipperyJack;

	public TileObject orangeMorelMushroom;

	public TileObject milkCapMushroom;

	public TileObject redLeadRoundHead;

	public TileObject deadMushroom;

	private void Awake()
	{
		manage = this;
	}

	public void checkSeasonAndChangeMaterials()
	{
		SetSeasonMaterialForAllMats();
		changeSeasonEvent.Invoke();
		setSunLightColour();
		GenerateMap.generate.bushLandGrowBack = bushlandGrowBack.getThisSeasonsTable();
		GenerateMap.generate.desertRainGrowBack = desertRainGrowBack.getThisSeasonsTable();
		GenerateMap.generate.tropicalGrowBack = tropicalGrowBack.getThisSeasonsTable();
		GenerateMap.generate.coldLandGrowBack = coldGrowBack.getThisSeasonsTable();
		RealWorldTimeLight.time.setNewSunsetDif(getDayLengthDif());
		setSeasonParticles();
		if (WorldManager.Instance.month == 2)
		{
			StartCoroutine(ClearMushroomsAtMidday());
		}
	}

	private void SetSeasonMaterialForAllMats()
	{
		for (int i = 0; i < allSeasonMats.Length; i++)
		{
			allSeasonMats[i].setSeasonMat();
		}
		for (int j = 0; j < WorldManager.Instance.tileTypes.Length; j++)
		{
			if (WorldManager.Instance.tileTypes[j].collectsSnow)
			{
				if (IsSnowDay())
				{
					WorldManager.Instance.tileTypes[j].footStepParticle = 5;
				}
				else
				{
					WorldManager.Instance.tileTypes[j].footStepParticle = 2;
				}
			}
		}
	}

	private void SetDefaultSeasonMaterialForAllMats()
	{
		for (int i = 0; i < allSeasonMats.Length; i++)
		{
			allSeasonMats[i].resetToDefaultOnDestroy();
		}
		for (int j = 0; j < WorldManager.Instance.tileTypes.Length; j++)
		{
			if (WorldManager.Instance.tileTypes[j].collectsSnow)
			{
				WorldManager.Instance.tileTypes[j].footStepParticle = 2;
			}
		}
	}

	public int getDayLengthDif()
	{
		if (WorldManager.Instance.month == 1)
		{
			return summerLengthDif;
		}
		if (WorldManager.Instance.month == 2)
		{
			return autumnLengthDif;
		}
		if (WorldManager.Instance.month == 3)
		{
			return winterLengthDif;
		}
		if (WorldManager.Instance.month == 4)
		{
			return springLengthDif;
		}
		return 0;
	}

	public void setSunLightColour()
	{
		if (WorldManager.Instance.month == 1)
		{
			RealWorldTimeLight.time.defaultSunColor = summerSunColour;
		}
		else if (WorldManager.Instance.month == 2)
		{
			RealWorldTimeLight.time.defaultSunColor = autumnSunColour;
		}
		else if (WorldManager.Instance.month == 3)
		{
			RealWorldTimeLight.time.defaultSunColor = winterSunColour;
		}
		else if (WorldManager.Instance.month == 4)
		{
			RealWorldTimeLight.time.defaultSunColor = springSunColour;
		}
	}

	public void setSeasonParticles()
	{
		if (WorldManager.Instance.month == 4)
		{
			if ((bool)WeatherManager.Instance && !WeatherManager.Instance.rainMgr.IsActive && WeatherManager.Instance.windMgr.IsWindBlowing)
			{
				springParticles.SetActive(value: true);
			}
			else
			{
				springParticles.SetActive(value: false);
			}
		}
		else
		{
			springParticles.SetActive(value: false);
		}
	}

	public float GetSeasonalPercentageDif()
	{
		if ((float)WorldManager.Instance.month == 2f)
		{
			return 18f;
		}
		if ((float)WorldManager.Instance.month == 3f)
		{
			return 100f;
		}
		return 0f;
	}

	public int GetRandomMushroomId(int tileType)
	{
		if (tileType < 0)
		{
			return -1;
		}
		switch (tileType)
		{
		case 3:
		case 5:
			return -1;
		case 14:
			return redLeadRoundHead.tileObjectId;
		case 1:
		case 23:
			return fieldMushroom.tileObjectId;
		case 4:
		case 25:
			return slipperyJack.tileObjectId;
		case 15:
		case 24:
			return orangeMorelMushroom.tileObjectId;
		default:
			return milkCapMushroom.tileObjectId;
		}
	}

	private bool IsMushroom(int checkId)
	{
		if (checkId < orangeMorelMushroom.tileObjectId || checkId > redLeadRoundHead.tileObjectId)
		{
			return false;
		}
		if (checkId == orangeMorelMushroom.tileObjectId || checkId == fieldMushroom.tileObjectId || checkId == slipperyJack.tileObjectId || checkId == milkCapMushroom.tileObjectId || checkId == redLeadRoundHead.tileObjectId)
		{
			return true;
		}
		return false;
	}

	public IEnumerator ClearMushroomsAtMidday()
	{
		WaitForSeconds wait = new WaitForSeconds(0.5f);
		while (!RealWorldTimeLight.time)
		{
			yield return null;
		}
		int currentDay = WorldManager.Instance.day;
		while (RealWorldTimeLight.time.currentHour != 12)
		{
			yield return wait;
			if (currentDay != WorldManager.Instance.day)
			{
				yield break;
			}
		}
		while (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
		{
			yield return wait;
		}
		int mapSize = WorldManager.Instance.GetMapSize();
		for (int j = 0; j < mapSize / 10; j++)
		{
			for (int k = 0; k < mapSize / 10; k++)
			{
				if (!WorldManager.Instance.chunkChangedMap[k, j])
				{
					continue;
				}
				for (int l = j * 10; l < j * 10 + 10; l++)
				{
					for (int m = k * 10; m < k * 10 + 10; m++)
					{
						if (IsMushroom(WorldManager.Instance.onTileMap[m, l]))
						{
							WorldManager.Instance.onTileMap[m, l] = deadMushroom.tileObjectId;
						}
					}
				}
			}
		}
		for (int i = 0; i < WorldManager.Instance.chunksInUse.Count; i++)
		{
			if (WorldManager.Instance.chunksInUse[i].gameObject.activeInHierarchy)
			{
				WorldManager.Instance.chunksInUse[i].refreshChunksOnTileObjects();
				yield return null;
			}
		}
	}

	public bool IsSnowDay()
	{
		if (WorldManager.Instance.month == 3 && (bool)WeatherManager.Instance)
		{
			return WeatherManager.Instance.IsSnowDay;
		}
		return false;
	}

	private void OnDestroy()
	{
		SetDefaultSeasonMaterialForAllMats();
	}
}
