using System.Collections;
using UnityEngine;

public class WorldObject : MonoBehaviour
{
	public bool destroyAndDropItemAfterTime;

	public bool destroyAndDropBeforeTime;

	public float destroyTime;

	public Transform[] dropPositions;

	public float health = 100f;

	public InventoryItem dropsItemOnDestroyed;

	public InventoryItemLootTable spawnBugOnDrop;

	public AnimalAI spawnAnimalOnDrop;

	public Transform[] bugPositions;

	private int itemDrop = -1;

	[Header("Spawn Carryable")]
	public PickUpAndCarry carryableId;

	public float chance;

	private float randomChance;

	public GameObject dummyItem;

	public bool deathPartsOn = true;

	public void Start()
	{
		if ((bool)dropsItemOnDestroyed)
		{
			itemDrop = Inventory.Instance.getInvItemId(dropsItemOnDestroyed);
		}
		if (destroyAndDropItemAfterTime)
		{
			StartCoroutine("runClock");
		}
		if (destroyAndDropBeforeTime)
		{
			doDrop();
		}
		if ((bool)carryableId)
		{
			Random.InitState((int)(base.transform.position.x * base.transform.position.y) * NetworkMapSharer.Instance.mineSeed + (int)base.transform.position.z + RealWorldTimeLight.time.currentHour);
			randomChance = Random.Range(0f, 100f);
			dummyItem.SetActive(chance > randomChance);
		}
	}

	private IEnumerator runClock()
	{
		yield return new WaitForSeconds(destroyTime);
		if (!destroyAndDropBeforeTime)
		{
			doDrop();
		}
		if (deathPartsOn)
		{
			Transform[] array = dropPositions;
			foreach (Transform transform in array)
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], transform.position);
			}
		}
		Object.Destroy(base.gameObject);
	}

	public void doDrop()
	{
		if (NetworkMapSharer.Instance.isServer)
		{
			Transform[] array = dropPositions;
			foreach (Transform transform in array)
			{
				NetworkMapSharer.Instance.spawnAServerDrop(itemDrop, 1, transform.position, null, tryNotToStack: true, 1);
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], transform.position);
			}
			if ((bool)spawnAnimalOnDrop)
			{
				NetworkNavMesh.nav.SpawnAnAnimalOnTile(spawnAnimalOnDrop.animalId * 10, Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f));
			}
			array = bugPositions;
			foreach (Transform transform2 in array)
			{
				NetworkNavMesh.nav.spawnSpecificBug(spawnBugOnDrop.getRandomDropFromTable().getItemId(), transform2.position);
			}
			if (dropPositions.Length == 0)
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position);
			}
			if (chance > randomChance && NetworkMapSharer.Instance.isServer)
			{
				NetworkMapSharer.Instance.spawnACarryable(carryableId.gameObject, dummyItem.transform.position);
			}
		}
	}
}
