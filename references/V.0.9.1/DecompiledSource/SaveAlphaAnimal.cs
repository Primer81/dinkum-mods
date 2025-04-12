using UnityEngine;

public class SaveAlphaAnimal : MonoBehaviour
{
	public Vector3 mySpawnPoint;

	public int daysRemaining;

	public bool canSpawnInWater;

	private int baseMaxHealth;

	private void Start()
	{
		baseMaxHealth = AnimalManager.manage.allAnimals[GetComponent<AnimalAI>().animalId].GetComponent<Damageable>().maxHealth;
	}

	private void OnEnable()
	{
		AdjustMaxHealthForMultiplayer();
		WorldManager.Instance.changeDayEvent.AddListener(takeADayAway);
	}

	private void OnDisable()
	{
		WorldManager.Instance.changeDayEvent.RemoveListener(takeADayAway);
	}

	public void takeADayAway()
	{
		daysRemaining--;
	}

	public void AdjustMaxHealthForMultiplayer()
	{
		baseMaxHealth = AnimalManager.manage.allAnimals[GetComponent<AnimalAI>().animalId].GetComponent<Damageable>().maxHealth;
		Damageable component = GetComponent<Damageable>();
		if ((bool)component)
		{
			if (NetworkNavMesh.nav.getPlayerCount() <= 1)
			{
				component.maxHealth = baseMaxHealth;
			}
			else
			{
				component.maxHealth = baseMaxHealth + Mathf.RoundToInt((float)(baseMaxHealth * (NetworkNavMesh.nav.getPlayerCount() - 1)) / 1.5f);
			}
		}
	}
}
