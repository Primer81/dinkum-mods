using System.Collections;
using Mirror;
using UnityEngine;

public class RandomDropFromAnimator : NetworkBehaviour
{
	public InventoryItem drop;

	public InventoryItem dropsWithHigherStatus;

	public int secondsPerDrop = 10;

	public float percentageOfDrop = 1f;

	public int timesPerDay = 1;

	public WaitForSeconds secondsToWait;

	public FarmAnimal farmAnimal;

	public bool droppedToday;

	private Coroutine dropRoutine;

	public bool dropsPoo;

	public override void OnStartServer()
	{
		WorldManager.Instance.changeDayEvent.AddListener(startDaytimeCounter);
		secondsToWait = new WaitForSeconds(secondsPerDrop);
		if (dropRoutine != null)
		{
			StopCoroutine(dropRoutine);
		}
		dropRoutine = StartCoroutine(randomDrops());
	}

	public override void OnStopClient()
	{
		WorldManager.Instance.changeDayEvent.RemoveListener(startDaytimeCounter);
	}

	public void startDaytimeCounter()
	{
		Invoke("startDayDelay", Random.Range(0.15f, 0.2f));
	}

	private void startDayDelay()
	{
		if (dropRoutine != null)
		{
			StopCoroutine(dropRoutine);
		}
		dropRoutine = StartCoroutine(randomDrops());
		droppedToday = false;
	}

	public void dropTheDrop()
	{
		if ((!dropsPoo || !NetworkMapSharer.Instance.wishManager.IsWishActive(WishManager.WishType.SpotlessWish)) && (farmAnimal.getDetails().hasHouse() || !farmAnimal.canBeHarvested))
		{
			if ((bool)dropsWithHigherStatus && farmAnimal.getRelationship() >= Random.Range(80, 90))
			{
				NetworkMapSharer.Instance.spawnAServerDropToSave(Inventory.Instance.getInvItemId(dropsWithHigherStatus), 1, base.transform.position);
			}
			else
			{
				NetworkMapSharer.Instance.spawnAServerDropToSave(Inventory.Instance.getInvItemId(drop), 1, base.transform.position);
			}
			farmAnimal.getDetails().hasDoneDrop = true;
		}
	}

	private IEnumerator randomDrops()
	{
		if (dropsPoo && NetworkMapSharer.Instance.wishManager.IsWishActive(WishManager.WishType.SpotlessWish))
		{
			yield break;
		}
		yield return null;
		int timesHappened = 0;
		while (timesHappened < timesPerDay)
		{
			yield return secondsToWait;
			if (((bool)farmAnimal && (farmAnimal.getDetails() == null || !farmAnimal.getDetails().ateYesterDay || !farmAnimal.getDetails().ateYesterDay)) || !(Random.Range(0f, 100f) < percentageOfDrop))
			{
				continue;
			}
			if (!farmAnimal)
			{
				NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(drop), 1, base.transform.position);
				timesHappened++;
				continue;
			}
			if (!farmAnimal.getDetails().hasDoneDrop)
			{
				dropTheDrop();
			}
			break;
		}
		droppedToday = true;
	}

	private void MirrorProcessed()
	{
	}
}
