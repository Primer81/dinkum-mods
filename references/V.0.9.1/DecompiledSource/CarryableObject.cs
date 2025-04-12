using System;
using UnityEngine;

[Serializable]
public class CarryableObject
{
	public int carryablePrefabId;

	public bool trappedAnimal;

	public bool farmAnimalBox;

	public int animalId;

	public int animalVariation;

	public string animalName;

	public float[] position = new float[3];

	public CarryableObject()
	{
	}

	public CarryableObject(PickUpAndCarry myCarry)
	{
		AnimalCarryBox component = myCarry.GetComponent<AnimalCarryBox>();
		if ((bool)component)
		{
			farmAnimalBox = true;
			trappedAnimal = false;
			animalId = component.animalId;
			animalVariation = component.variation;
			animalName = component.animalName;
		}
		TrappedAnimal component2 = myCarry.GetComponent<TrappedAnimal>();
		if ((bool)component2)
		{
			farmAnimalBox = false;
			trappedAnimal = true;
			animalId = component2.trappedAnimalId;
			animalVariation = component2.trappedAnimalVariation;
		}
		carryablePrefabId = myCarry.prefabId;
		position[0] = myCarry.transform.position.x;
		position[1] = myCarry.transform.position.y;
		position[2] = myCarry.transform.position.z;
	}

	public void SpawnTheObject()
	{
		if (position[1] <= -2000f)
		{
			Debug.Log("Found a carriable out of world space. Moving back");
			Debug.Log(SaveLoad.saveOrLoad.carryablePrefabs[carryablePrefabId].name);
			Debug.Log(new Vector3(position[0], position[1], position[2]));
			if (WorldManager.Instance.isPositionOnMap(Mathf.RoundToInt(position[0] / 2f), Mathf.RoundToInt(position[2] / 2f)))
			{
				position[1] = WorldManager.Instance.heightMap[Mathf.RoundToInt(position[0] / 2f), Mathf.RoundToInt(position[2] / 2f)];
			}
			else
			{
				position[1] = 2f;
			}
		}
		PickUpAndCarry pickUpAndCarry = NetworkMapSharer.Instance.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[carryablePrefabId], new Vector3(position[0], position[1], position[2]), moveToGroundLevel: false);
		if (farmAnimalBox)
		{
			AnimalCarryBox component = pickUpAndCarry.GetComponent<AnimalCarryBox>();
			component.NetworkanimalId = animalId;
			component.Networkvariation = animalVariation;
			component.NetworkanimalName = animalName;
		}
		if (trappedAnimal)
		{
			TrappedAnimal component2 = pickUpAndCarry.GetComponent<TrappedAnimal>();
			component2.NetworktrappedAnimalId = animalId;
			component2.NetworktrappedAnimalVariation = animalVariation;
		}
		if (!pickUpAndCarry.investigationItem)
		{
			pickUpAndCarry.OnLoadCheckForVehicles();
		}
	}
}
