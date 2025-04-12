using System.Collections;
using System.IO;
using UnityEngine;

public class ItemPictureTaker : MonoBehaviour
{
	public bool takePhoto;

	private RenderTexture photoTexture;

	[Header("Object ------")]
	public bool spawnObject;

	public bool useSeedBagPlacement;

	public InventoryItem itemToSpawn;

	public InventoryItem[] listOfItemsToSpawn;

	private GameObject currentlySpawned;

	[Header("Default ------")]
	public Camera photoCamera;

	public Transform spawnPos;

	[Header("Positions ------")]
	public Transform fishPos;

	public Transform axesAndPickaxes;

	public Transform placeablePos;

	public Transform pathPos;

	public Transform eatablePos;

	public Transform bugPos;

	public Transform underWaterCreature;

	public Transform furniturePos;

	public Transform gliderPosition;

	public Transform seedPosition;

	[Header("Clothing Positions ------")]
	public MeshRenderer shirtPos;

	public MeshRenderer dressPos;

	public MeshRenderer pantsPos;

	public MeshRenderer shoePos;

	public Transform headPos;

	public GameObject maniquinHead;

	[Header("Furniture Picture")]
	public bool takeFurniturePhoto;

	public bool takeClothingPhoto;

	public bool forceUseAltDrop;

	public void Update()
	{
		if (takePhoto)
		{
			if (listOfItemsToSpawn.Length != 0)
			{
				StartCoroutine(takeMultiPictures());
			}
			else
			{
				takePhotoAndSave(itemToSpawn.itemName);
			}
			takePhoto = false;
		}
		if (spawnObject)
		{
			spawnObject = false;
			if ((bool)currentlySpawned)
			{
				Object.Destroy(currentlySpawned);
			}
			if ((bool)itemToSpawn)
			{
				spawnNewObjectInPos(itemToSpawn);
			}
		}
		if (takeFurniturePhoto)
		{
			StartCoroutine(takeFurniturePictures());
			takeFurniturePhoto = false;
		}
		if (takeClothingPhoto)
		{
			StartCoroutine(takeClothingPictures());
			takeClothingPhoto = false;
		}
	}

	public void takePhotoAndSave(string photoName)
	{
		int num = 512;
		photoTexture = new RenderTexture(num, num, 32);
		photoTexture.filterMode = FilterMode.Trilinear;
		photoTexture.antiAliasing = 3;
		photoCamera.targetTexture = photoTexture;
		Texture2D texture2D = new Texture2D(num, num, TextureFormat.ARGB32, mipChain: false);
		photoCamera.Render();
		RenderTexture.active = photoTexture;
		texture2D.ReadPixels(new Rect(0f, 0f, num, num), 0, 0);
		byte[] bytes = texture2D.EncodeToPNG();
		File.WriteAllBytes(ScreenShotName(num, num, photoName), bytes);
	}

	public static string ScreenShotName(int width, int height, string name)
	{
		MonoBehaviour.print("Photo saved to " + SaveLoad.saveOrLoad.saveSlot());
		SaveLoad.saveOrLoad.createPhotoDir();
		return $"{SaveLoad.saveOrLoad.saveSlot()}/Photos/{name}.png";
	}

	public void spawnNewObjectInPos(InventoryItem itemToSpawnThisTime)
	{
		int invItemId = Inventory.Instance.getInvItemId(itemToSpawnThisTime);
		shirtPos.gameObject.SetActive(value: false);
		dressPos.gameObject.SetActive(value: false);
		shoePos.gameObject.SetActive(value: false);
		pantsPos.gameObject.SetActive(value: false);
		maniquinHead.SetActive(value: false);
		if (forceUseAltDrop && (bool)itemToSpawnThisTime.altDropPrefab)
		{
			currentlySpawned = (currentlySpawned = Object.Instantiate(itemToSpawnThisTime.altDropPrefab, axesAndPickaxes));
		}
		else if (useSeedBagPlacement)
		{
			currentlySpawned = (currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, seedPosition));
		}
		else if ((bool)itemToSpawnThisTime.GetComponent<MusicCassette>())
		{
			currentlySpawned = (currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, fishPos));
		}
		else if ((bool)itemToSpawnThisTime.equipable && itemToSpawnThisTime.equipable.cloths)
		{
			if (itemToSpawnThisTime.equipable.dress)
			{
				dressPos.gameObject.SetActive(value: true);
				if (itemToSpawnThisTime.equipable.longDress)
				{
					dressPos.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defaultLongDress;
				}
				else
				{
					dressPos.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defaultDress;
				}
				dressPos.GetComponent<MeshRenderer>().sharedMaterial = itemToSpawnThisTime.equipable.material;
			}
			else if (itemToSpawnThisTime.equipable.shirt)
			{
				shirtPos.gameObject.SetActive(value: true);
				if ((bool)itemToSpawnThisTime.equipable.shirtMesh)
				{
					shirtPos.GetComponent<MeshFilter>().mesh = itemToSpawnThisTime.equipable.shirtMesh;
				}
				else
				{
					shirtPos.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defaultShirtMesh;
				}
				shirtPos.GetComponent<MeshRenderer>().sharedMaterial = itemToSpawnThisTime.equipable.material;
			}
			else if (itemToSpawnThisTime.equipable.shoes)
			{
				shoePos.gameObject.SetActive(value: true);
				if ((bool)itemToSpawnThisTime.equipable.useAltMesh)
				{
					shoePos.GetComponent<MeshFilter>().mesh = itemToSpawnThisTime.equipable.useAltMesh;
				}
				else
				{
					shoePos.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defualtShoeMesh;
				}
				shoePos.GetComponent<MeshRenderer>().sharedMaterial = itemToSpawnThisTime.equipable.material;
			}
			else if (itemToSpawnThisTime.equipable.pants)
			{
				pantsPos.gameObject.SetActive(value: true);
				if ((bool)itemToSpawnThisTime.equipable.useAltMesh)
				{
					pantsPos.GetComponent<MeshFilter>().mesh = itemToSpawnThisTime.equipable.useAltMesh;
				}
				else
				{
					pantsPos.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defaultPants;
				}
				pantsPos.GetComponent<MeshRenderer>().sharedMaterial = itemToSpawnThisTime.equipable.material;
			}
			else if (itemToSpawnThisTime.equipable.face || itemToSpawnThisTime.equipable.hat)
			{
				maniquinHead.SetActive(value: true);
				currentlySpawned = Object.Instantiate(itemToSpawnThisTime.equipable.hatPrefab, headPos);
				currentlySpawned.transform.localPosition = Vector3.zero;
				currentlySpawned.transform.localRotation = Quaternion.identity;
			}
		}
		else if (itemToSpawnThisTime.isFurniture)
		{
			currentlySpawned = Object.Instantiate(itemToSpawnThisTime.placeable.gameObject, furniturePos);
			if (WorldManager.Instance.allObjectSettings[itemToSpawnThisTime.placeable.tileObjectId].isMultiTileObject && WorldManager.Instance.allObjectSettings[itemToSpawnThisTime.placeable.tileObjectId].xSize > 2)
			{
				currentlySpawned.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
			}
			else
			{
				currentlySpawned.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
			}
		}
		else if ((bool)itemToSpawnThisTime.fish)
		{
			currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, fishPos);
		}
		else if ((bool)itemToSpawnThisTime.bug)
		{
			currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, bugPos);
		}
		else if ((bool)itemToSpawnThisTime.underwaterCreature)
		{
			currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, underWaterCreature);
		}
		else if ((bool)itemToSpawnThisTime.consumeable)
		{
			currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, eatablePos);
		}
		else if (itemToSpawnThisTime.damageStone || itemToSpawnThisTime.damageWood || itemToSpawnThisTime.isATool)
		{
			if ((bool)itemToSpawnThisTime.itemPrefab && (bool)itemToSpawnThisTime.itemPrefab.GetComponent<HangGliderHandObject>())
			{
				currentlySpawned = Object.Instantiate(itemToSpawnThisTime.altDropPrefab, gliderPosition);
			}
			else
			{
				currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, axesAndPickaxes);
			}
		}
		else if ((bool)itemToSpawnThisTime.placeable)
		{
			currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, placeablePos);
		}
		else if (itemToSpawnThisTime.placeableTileType != -1 && !itemToSpawnThisTime.isATool)
		{
			currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, pathPos);
		}
		else
		{
			currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, spawnPos);
		}
		if (!currentlySpawned)
		{
			return;
		}
		Animator[] componentsInChildren = currentlySpawned.GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		currentlySpawned.transform.localPosition = Vector3.zero;
		if (itemToSpawnThisTime.isFurniture && WorldManager.Instance.allObjectSettings[itemToSpawnThisTime.placeable.tileObjectId].isMultiTileObject)
		{
			currentlySpawned.transform.localPosition -= new Vector3((float)WorldManager.Instance.allObjectSettings[itemToSpawnThisTime.placeable.tileObjectId].xSize * 0.25f, 0f);
		}
		if (currentlySpawned.transform.parent == placeablePos)
		{
			currentlySpawned.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
		}
		else
		{
			currentlySpawned.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
		SetItemTexture component = currentlySpawned.GetComponent<SetItemTexture>();
		if ((bool)component)
		{
			component.setTexture(Inventory.Instance.allItems[invItemId]);
			if ((bool)component.changeSize)
			{
				component.changeSizeOfTrans(Inventory.Instance.allItems[invItemId].transform.localScale);
			}
		}
	}

	private IEnumerator takeMultiPictures()
	{
		for (int i = 0; i < listOfItemsToSpawn.Length; i++)
		{
			if ((bool)currentlySpawned)
			{
				Object.Destroy(currentlySpawned);
			}
			yield return null;
			spawnNewObjectInPos(listOfItemsToSpawn[i]);
			yield return null;
			takePhotoAndSave(listOfItemsToSpawn[i].itemName);
			yield return null;
		}
	}

	private IEnumerator takeFurniturePictures()
	{
		int furnitureSpriteId = 0;
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (Inventory.Instance.allItems[i].isFurniture)
			{
				if ((bool)currentlySpawned)
				{
					Object.Destroy(currentlySpawned);
				}
				yield return null;
				spawnNewObjectInPos(Inventory.Instance.allItems[i]);
				yield return null;
				takePhotoAndSave("Furniture_" + furnitureSpriteId + "_" + Inventory.Instance.allItems[i].itemName);
				furnitureSpriteId++;
				yield return null;
			}
		}
	}

	private IEnumerator takeClothingPictures()
	{
		int clothingSpriteId = 0;
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.cloths)
			{
				if ((bool)currentlySpawned)
				{
					Object.Destroy(currentlySpawned);
				}
				yield return null;
				spawnNewObjectInPos(Inventory.Instance.allItems[i]);
				yield return null;
				takePhotoAndSave("Clothing_" + clothingSpriteId + "_" + Inventory.Instance.allItems[i].itemName);
				clothingSpriteId++;
				yield return null;
			}
		}
	}
}
