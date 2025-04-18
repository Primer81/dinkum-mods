using UnityEngine;

public class SpawnAnimalButton : MonoBehaviour
{
	private int animalNo;

	private void Start()
	{
		animalNo = GetComponent<InvButton>().craftRecipeNumber;
	}

	public void spawnAnimal()
	{
		Vector3 position = NetworkMapSharer.Instance.localChar.transform.position;
		_ = (bool)AnimalManager.manage.allAnimals[animalNo].hasVariation;
		NetworkNavMesh.nav.SpawnAnAnimalOnTile(animalNo * 10, (int)(position.x / 2f), (int)(position.z / 2f));
	}
}
