using System.Collections;
using UnityEngine;

public class TileObject : MonoBehaviour
{
	public int tileObjectId;

	public int xPos;

	public int yPos;

	[Header("Death stuff --------------")]
	public float currentHealth = 100f;

	public Transform[] dropObjectFromPositions;

	[Header("Death Particles --------------")]
	public Transform[] particlePositions;

	[Header("Damage Stuff --------------")]
	public Transform[] damageParticlePositions;

	private bool damageAnimPlaying;

	private bool bounceAnimPlaying;

	[Header("Furniture settings --------------")]
	public Transform[] placedPositions;

	[Header("Connecting scripts--------------")]
	public TileObjectConnect tileObjectConnect;

	public TileObjectBridge tileObjectBridge;

	public ItemDepositAndChanger tileObjectItemChanger;

	public TileObjectGrowthStages tileObjectGrowthStages;

	public FurnitureStatus tileObjectFurniture;

	public DisplayPlayerHouseTiles displayPlayerHouseTiles;

	public ShowObjectOnStatusChange showObjectOnStatusChange;

	public TileObjectAnimalHouse tileObjectAnimalHouse;

	public AnimalDropOffSpot tileObjectAnimalDropOffSpot;

	public SprinklerTile sprinklerTile;

	public OnOffTile tileOnOff;

	public ChestPlaceable tileObjectChest;

	public WritableSign tileObjectWritableSign;

	[Header("Connecting scripts Transforms --------------")]
	public Transform loadInsidePos;

	[Header("Local Stuff --------------")]
	public Transform _transform;

	public GameObject _gameObject;

	public Transform AnimDamage;

	public Transform AnimDamage2;

	public bool active = true;

	public bool hasExtensions;

	private ShowObjectOnTop onTop;

	public void setXAndY(int newXPos, int newYPos)
	{
		xPos = newXPos;
		yPos = newYPos;
		checkForAllObjectsOnXYChange(inside: false);
		if (placedPositions.Length != 0)
		{
			if (onTop == null)
			{
				onTop = base.gameObject.AddComponent<ShowObjectOnTop>();
				onTop.setUp(placedPositions);
			}
			if (ItemOnTopManager.manage.hasItemsOnTop(xPos, yPos))
			{
				onTop.updateItemsOnTopOfMe(ItemOnTopManager.manage.getAllItemsOnTop(xPos, yPos, null));
			}
			else
			{
				onTop.clearObjectsOnTopOfMe();
			}
		}
	}

	public void setXAndYForHouse(int newXPos, int newYPos)
	{
		xPos = newXPos;
		yPos = newYPos;
		checkForAllObjectsOnXYChange(inside: true);
	}

	private void checkForAllObjectsOnXYChange(bool inside)
	{
		if ((bool)WorldManager.Instance.allObjectSettings[tileObjectId].tileObjectLoadInside)
		{
			WorldManager.Instance.allObjectSettings[tileObjectId].tileObjectLoadInside.checkForInterior(xPos, yPos);
		}
		if (!inside && (bool)tileObjectConnect)
		{
			tileObjectConnect.connectToTiles(xPos, yPos);
		}
		if (!inside && (bool)tileObjectWritableSign)
		{
			tileObjectWritableSign.updateSignText(xPos, yPos, -1, -1);
		}
		if (hasExtensions)
		{
			if ((bool)tileObjectBridge)
			{
				tileObjectBridge.setUpBridge(WorldManager.Instance.onTileStatusMap[xPos, yPos]);
			}
			if ((bool)displayPlayerHouseTiles && WorldManager.Instance.rotationMap[xPos, yPos] != 0)
			{
				displayPlayerHouseTiles.setInteriorPosAndRotation(xPos, yPos);
			}
			if ((bool)tileObjectGrowthStages)
			{
				tileObjectGrowthStages.setStage(xPos, yPos);
			}
			else if ((bool)tileObjectFurniture)
			{
				tileObjectFurniture.updateOnTileStatus(xPos, yPos);
			}
			else if ((bool)tileObjectItemChanger)
			{
				tileObjectItemChanger.mapUpdatePos(xPos, yPos);
			}
			else if ((bool)tileOnOff)
			{
				tileOnOff.setOnOff(xPos, yPos);
			}
			else if ((bool)showObjectOnStatusChange)
			{
				showObjectOnStatusChange.showGameObject(xPos, yPos);
			}
		}
	}

	public void checkOnTopInside(int insideX, int insideY, HouseDetails details)
	{
		if (placedPositions.Length != 0)
		{
			if (onTop == null)
			{
				onTop = base.gameObject.AddComponent<ShowObjectOnTop>();
				onTop.setUp(placedPositions);
			}
			onTop.updateItemsOnTopOfMe(ItemOnTopManager.manage.getAllItemsOnTop(insideX, insideY, details));
		}
	}

	public bool isAtPos(int checkX, int checkY)
	{
		if (xPos == checkX && yPos == checkY)
		{
			return true;
		}
		return false;
	}

	public bool canBePlaceOn()
	{
		return WorldManager.Instance.allObjectSettings[tileObjectId].canBePlacedOn();
	}

	public int getTileObjectChangeToOnDeath()
	{
		return WorldManager.Instance.allObjectSettings[tileObjectId].changeToTileObjectOnDeath;
	}

	public bool GetRotationFromMap()
	{
		return WorldManager.Instance.allObjectSettings[tileObjectId].getRotationFromMap;
	}

	public bool hasRandomScale()
	{
		return WorldManager.Instance.allObjectSettings[tileObjectId].hasRandomScale;
	}

	public bool canBePickedUp()
	{
		return WorldManager.Instance.allObjectSettings[tileObjectId].canBePickedUp;
	}

	public bool IsMultiTileObject()
	{
		return WorldManager.Instance.allObjectSettings[tileObjectId].isMultiTileObject;
	}

	public int GetXSize()
	{
		return WorldManager.Instance.allObjectSettings[tileObjectId].xSize;
	}

	public int GetYSize()
	{
		return WorldManager.Instance.allObjectSettings[tileObjectId].ySize;
	}

	public bool canBePlacedOntoFurniture()
	{
		return WorldManager.Instance.allObjectSettings[tileObjectId].canBePlacedOnTopOfFurniture;
	}

	public void checkForAllExtensions()
	{
		if ((bool)showObjectOnStatusChange || (bool)tileObjectItemChanger || (bool)tileObjectGrowthStages || (bool)tileObjectFurniture || (bool)displayPlayerHouseTiles || (bool)tileOnOff || (bool)tileObjectBridge)
		{
			hasExtensions = true;
		}
		else
		{
			hasExtensions = false;
		}
	}

	public bool checkIfHasAnyItemsOnTop(Vector3 housePos, HouseDetails insideHouse, int xPos, int yPos)
	{
		int num = 0;
		Transform[] array = placedPositions;
		foreach (Transform obj in array)
		{
			_ = (int)(obj.position.x - housePos.x) / 2;
			_ = (int)(obj.position.z - housePos.z) / 2;
		}
		if (num != 0)
		{
			return true;
		}
		return false;
	}

	public Transform findClosestPlacedPosition(Vector3 cursorPos)
	{
		float num = 10f;
		Transform result = null;
		if (placedPositions.Length == 1)
		{
			return placedPositions[0];
		}
		Transform[] array = placedPositions;
		foreach (Transform transform in array)
		{
			float num2 = Vector3.Distance(new Vector3(cursorPos.x, transform.position.y, cursorPos.z), transform.position);
			if (num2 < num)
			{
				num = num2;
				result = transform;
			}
		}
		return result;
	}

	public int returnClosestPlacedPositionId(Vector3 cursorPos)
	{
		if (placedPositions.Length == 1)
		{
			return 0;
		}
		float num = 10f;
		int result = 0;
		for (int i = 0; i < placedPositions.Length; i++)
		{
			float num2 = Vector3.Distance(new Vector3(cursorPos.x, 0f, cursorPos.z), new Vector3(placedPositions[i].position.x, 0f, placedPositions[i].position.z));
			if (num2 < num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public int returnClosestPositionWithItemOnTop(Vector3 cursorPos, int xPos, int yPos, HouseDetails insideHouseDetails)
	{
		if (placedPositions.Length == 1)
		{
			return 0;
		}
		float num = 10f;
		int result = 0;
		for (int i = 0; i < placedPositions.Length; i++)
		{
			float num2 = Vector3.Distance(new Vector3(cursorPos.x, 0f, cursorPos.z), new Vector3(placedPositions[i].position.x, 0f, placedPositions[i].position.z));
			if (ItemOnTopManager.manage.getItemOnTopInPosition(i, xPos, yPos, insideHouseDetails) != null && num2 < num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public void addXp()
	{
		if (!WorldManager.Instance.allObjectSettings[tileObjectId].canBePickedUp && (!WorldManager.Instance.allObjectSettings[tileObjectId].dropsItemOnDeath || !WorldManager.Instance.allObjectSettings[tileObjectId].dropsItemOnDeath.placeable || WorldManager.Instance.allObjectSettings[tileObjectId].dropsItemOnDeath.placeable.tileObjectId != tileObjectId))
		{
			if (WorldManager.Instance.allObjectSettings[tileObjectId].isStone || WorldManager.Instance.allObjectSettings[tileObjectId].isHardStone)
			{
				CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Mining, 1 + Mathf.RoundToInt(WorldManager.Instance.allObjectSettings[tileObjectId].fullHealth / 10f));
			}
			else if (WorldManager.Instance.allObjectSettings[tileObjectId].isWood || WorldManager.Instance.allObjectSettings[tileObjectId].isHardWood)
			{
				CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Foraging, 1 + Mathf.RoundToInt(WorldManager.Instance.allObjectSettings[tileObjectId].fullHealth / 10f));
			}
		}
	}

	public int getXpTallyType()
	{
		if (((bool)tileObjectGrowthStages && tileObjectGrowthStages.needsTilledSoil) || ((bool)tileObjectGrowthStages && tileObjectGrowthStages.isAPlantSproutFromAFarmPlant(tileObjectId)))
		{
			return 0;
		}
		if ((bool)tileObjectGrowthStages && tileObjectGrowthStages.mustBeInWater)
		{
			return 3;
		}
		if ((bool)tileObjectGrowthStages && (bool)tileObjectGrowthStages.harvestDrop && (!tileObjectGrowthStages.harvestDrop.placeable || tileObjectGrowthStages.harvestDrop.placeable.tileObjectId != tileObjectId))
		{
			return 1;
		}
		if (WorldManager.Instance.allObjectSettings[tileObjectId].canBePickedUp || ((bool)WorldManager.Instance.allObjectSettings[tileObjectId].dropsItemOnDeath && (bool)WorldManager.Instance.allObjectSettings[tileObjectId].dropsItemOnDeath.placeable && WorldManager.Instance.allObjectSettings[tileObjectId].dropsItemOnDeath.placeable.tileObjectId == tileObjectId))
		{
			return -1;
		}
		if (WorldManager.Instance.allObjectSettings[tileObjectId].isStone || WorldManager.Instance.allObjectSettings[tileObjectId].isHardStone)
		{
			return 2;
		}
		if (WorldManager.Instance.allObjectSettings[tileObjectId].isWood || WorldManager.Instance.allObjectSettings[tileObjectId].isHardWood)
		{
			return 1;
		}
		return -1;
	}

	public void damage(bool damageWithSound = true, bool damageParticleOn = true)
	{
		if ((bool)WorldManager.Instance.allObjectSettings[tileObjectId].damageSound && damageWithSound)
		{
			SoundManager.Instance.playASoundAtPoint(WorldManager.Instance.allObjectSettings[tileObjectId].damageSound, _transform.position);
		}
		if ((bool)AnimDamage && CameraController.control.IsCloseToCamera50(base.transform.position))
		{
			if (!damageAnimPlaying && base.gameObject.activeSelf)
			{
				StartCoroutine(damageNoAnim());
			}
			if (!bounceAnimPlaying)
			{
				StartCoroutine(damageBounce());
			}
		}
		if (!damageParticleOn || WorldManager.Instance.allObjectSettings[tileObjectId].damageParticle == -1)
		{
			return;
		}
		Transform[] array = damageParticlePositions;
		foreach (Transform transform in array)
		{
			if (transform != null)
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[WorldManager.Instance.allObjectSettings[tileObjectId].damageParticle], transform.position, WorldManager.Instance.allObjectSettings[tileObjectId].damageParticlesPerPosition);
			}
		}
	}

	public void onDeath()
	{
		if (WorldManager.Instance.allObjectSettings[tileObjectId].deathParticle != -1)
		{
			if (particlePositions.Length == 0)
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[WorldManager.Instance.allObjectSettings[tileObjectId].deathParticle], _transform.position, WorldManager.Instance.allObjectSettings[tileObjectId].particlesPerPositon);
			}
			else if (!tileObjectGrowthStages || tileObjectGrowthStages.getShowingStage() != -1)
			{
				Transform[] array = particlePositions;
				foreach (Transform transform in array)
				{
					if (transform != null)
					{
						ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[WorldManager.Instance.allObjectSettings[tileObjectId].deathParticle], transform.position, WorldManager.Instance.allObjectSettings[tileObjectId].particlesPerPositon);
					}
				}
			}
		}
		if ((bool)tileObjectGrowthStages && tileObjectGrowthStages.diesOnHarvest && (bool)tileObjectGrowthStages.harvestSound)
		{
			SoundManager.Instance.playASoundAtPoint(tileObjectGrowthStages.harvestSound, base.transform.position);
		}
		else if ((bool)WorldManager.Instance.allObjectSettings[tileObjectId].deathSound)
		{
			SoundManager.Instance.playASoundAtPoint(WorldManager.Instance.allObjectSettings[tileObjectId].deathSound, _transform.position);
		}
	}

	public void onDeathServer(int xPos, int yPos)
	{
		WorldManager.Instance.allObjectSettings[tileObjectId].onDeathServer(xPos, yPos, null, tileObjectGrowthStages, _transform, dropObjectFromPositions);
	}

	public void onDeathInsideServer(int xPos, int yPos, int houseX, int houseY)
	{
		WorldManager.Instance.allObjectSettings[tileObjectId].onDeathServer(xPos, yPos, HouseManager.manage.getHouseInfo(houseX, houseY), tileObjectGrowthStages, _transform, dropObjectFromPositions);
	}

	public bool checkIfMultiTileObjectCanBePlaced(int startingXPos, int startingYPos, int rotation)
	{
		if (WorldManager.Instance.allObjectSettings[tileObjectId].canPlaceItemsUnderIt)
		{
			return WorldManager.Instance.allObjectSettings[tileObjectId].checkIfCanPlaceUnderMultiTileObjectCanBePlaced(startingXPos, startingYPos, rotation);
		}
		return WorldManager.Instance.allObjectSettings[tileObjectId].checkIfMultiTileObjectCanBePlaced(startingXPos, startingYPos, rotation);
	}

	public bool checkIfMultiTileObjectIsUnderWater(InventoryItem invItemChecking, int startingXPos, int startingYPos, int rotation)
	{
		if ((bool)invItemChecking && invItemChecking.placeOnWaterOnly)
		{
			return WorldManager.Instance.allObjectSettings[tileObjectId].checkIfMultiTileObjectIsUnderWater(startingXPos, startingYPos, rotation);
		}
		return true;
	}

	public bool checkIfMultiTileObjectCanBePlacedMapGenerate(int startingXPos, int startingYPos, int rotation)
	{
		return WorldManager.Instance.allObjectSettings[tileObjectId].checkIfMultiTileObjectCanBePlacedMapGenerationOnly(startingXPos, startingYPos, rotation);
	}

	public bool checkIfDeedCanBePlaced(int startingXPos, int startingYPos, int rotation)
	{
		return WorldManager.Instance.allObjectSettings[tileObjectId].checkIfDeedCanBePlaced(startingXPos, startingYPos, rotation);
	}

	public string getWhyCantPlaceDeedText(int startingXPos, int startingYPos, int rotation)
	{
		return WorldManager.Instance.allObjectSettings[tileObjectId].getWhyCantPlaceDeedText(startingXPos, startingYPos, rotation);
	}

	public bool checkIfBridgeCanBePlaced(int startingXPos, int startingYPos, int rotation)
	{
		return WorldManager.Instance.allObjectSettings[tileObjectId].checkIfBridgeCanBePlaced(startingXPos, startingYPos, rotation);
	}

	public bool checkIfMultiTileObjectCanBePlacedOnStoredMap(int startingXPos, int startingYPos, int[,] storedOnTileArray, int[,] storedOnHeightArray, int rotation, bool placeOverSingleTiledObjects = false, bool ignoreHeight = false)
	{
		return WorldManager.Instance.allObjectSettings[tileObjectId].checkIfMultiTileObjectCanBePlacedOnStoredMap(startingXPos, startingYPos, storedOnTileArray, storedOnHeightArray, rotation, placeOverSingleTiledObjects, ignoreHeight);
	}

	public bool CheckIfMultiTileObjectCanBePlacedInside(HouseDetails house, int startingXPos, int startingYPos, int rotation)
	{
		return WorldManager.Instance.allObjectSettings[tileObjectId].CheckIfMultiTileObjectCanBePlacedInside(startingXPos, startingYPos, rotation, house);
	}

	public int[] placeBridgeTiledObject(int startingXPos, int startingYPos, int rotation = 4, int bridgeLength = 2)
	{
		return WorldManager.Instance.allObjectSettings[tileObjectId].placeBridgeTiledObject(startingXPos, startingYPos, rotation, bridgeLength);
	}

	public void placeMultiTiledObject(int startingXPos, int startingYPos, int rotation = 4)
	{
		WorldManager.Instance.allObjectSettings[tileObjectId].placeMultiTiledObject(startingXPos, startingYPos, rotation);
	}

	public void placeMultiTiledObjectPlaceUnder(int startingXPos, int startingYPos, int rotation = 4)
	{
		WorldManager.Instance.allObjectSettings[tileObjectId].placeMultiTiledObjectPlaceUnder(startingXPos, startingYPos, rotation);
	}

	public void placeMultiTiledObjectOnStoredMap(int startingXPos, int startingYPos, MapStorer.LoadMapType mapType, int rotation = 4)
	{
		WorldManager.Instance.allObjectSettings[tileObjectId].placeMultiTiledObjectOnStoredMap(startingXPos, startingYPos, mapType, rotation);
	}

	public void PlaceMultiTiledObjectInside(int startingXPos, int startingYPos, int rotation, HouseDetails houseDetails)
	{
		WorldManager.Instance.allObjectSettings[tileObjectId].PlaceMultiTiledObjectInside(startingXPos, startingYPos, rotation, houseDetails);
	}

	public void removeMultiTiledObject(int startingXPos, int startingYPos, int rotation)
	{
		if (WorldManager.Instance.allObjectSettings[tileObjectId].canPlaceItemsUnderIt && WorldManager.Instance.onTileMap[Mathf.Clamp(startingXPos + 1, 0, WorldManager.Instance.GetMapSize() - 1), startingYPos] != -4 && WorldManager.Instance.onTileMap[startingXPos, Mathf.Clamp(startingYPos + 1, 0, WorldManager.Instance.GetMapSize() - 1)] != -3)
		{
			WorldManager.Instance.allObjectSettings[tileObjectId].removeMultiTiledObjectPlaceUnder(startingXPos, startingYPos, rotation);
		}
		else
		{
			WorldManager.Instance.allObjectSettings[tileObjectId].removeMultiTiledObject(startingXPos, startingYPos, rotation);
		}
	}

	public void removeMultiTiledObjectInside(int startingXPos, int startingYPos, int rotation, HouseDetails houseDetails)
	{
		WorldManager.Instance.allObjectSettings[tileObjectId].removeMultiTiledObjectInside(startingXPos, startingYPos, rotation, houseDetails);
	}

	private void OnDisable()
	{
		if ((bool)AnimDamage && (damageAnimPlaying || bounceAnimPlaying))
		{
			AnimDamage.localScale = Vector3.one;
			AnimDamage.localRotation = Quaternion.Euler(0f, 0f, 0f);
			if ((bool)AnimDamage2)
			{
				AnimDamage2.localRotation = Quaternion.Euler(0f, 0f, 0f);
			}
			damageAnimPlaying = false;
			bounceAnimPlaying = false;
		}
	}

	public void getRotation(int xPos, int yPos)
	{
		if (IsMultiTileObject() || GetRotationFromMap())
		{
			if ((bool)tileObjectBridge)
			{
				setMapRotationBridge(xPos, yPos);
			}
			else
			{
				setMapRotation(xPos, yPos);
			}
		}
		else if (WorldManager.Instance.allObjectSettings[tileObjectId].hasRandomRotation)
		{
			getRandomRot(xPos, yPos);
		}
	}

	public void getRotationInside(int xPos, int yPos, HouseDetails inHouse = null)
	{
		if (IsMultiTileObject() || GetRotationFromMap())
		{
			setMapRotationInside(xPos, yPos, inHouse);
		}
		else if (WorldManager.Instance.allObjectSettings[tileObjectId].hasRandomRotation)
		{
			getRandomRot(xPos, yPos);
		}
	}

	public Vector3 SetRotationNumberForPreviewObject(int mapRot, int length = 5)
	{
		if (IsMultiTileObject())
		{
			if ((bool)tileObjectBridge)
			{
				switch (mapRot)
				{
				case 1:
					length--;
					_transform.rotation = Quaternion.Euler(0f, 180f, 0f);
					return new Vector3(GetXSize() * 2 - 2, 0f, -length * 2);
				case 2:
					length--;
					_transform.rotation = Quaternion.Euler(0f, 270f, 0f);
					return new Vector3(0f, 0f, (float)(GetXSize() * 2) - 2f * tileObjectBridge.xDif) - new Vector3(length * 2, 0f, 2f);
				case 3:
					_transform.rotation = Quaternion.Euler(0f, 180f, 0f);
					return new Vector3(GetXSize() * 2 - 2, 0f, GetYSize() * 2 - 2);
				case 4:
					_transform.rotation = Quaternion.Euler(0f, 270f, 0f);
					return new Vector3(GetYSize() * 2 - 2, 0f, 0f);
				}
			}
			else
			{
				switch (mapRot)
				{
				case 1:
					_transform.rotation = Quaternion.Euler(0f, 0f, 0f);
					return Vector3.zero;
				case 2:
					_transform.rotation = Quaternion.Euler(0f, 90f, 0f);
					return new Vector3(0f, 0f, GetXSize() * 2 - 2);
				case 3:
					_transform.rotation = Quaternion.Euler(0f, 180f, 0f);
					return new Vector3(GetXSize() * 2 - 2, 0f, GetYSize() * 2 - 2);
				case 4:
					_transform.rotation = Quaternion.Euler(0f, 270f, 0f);
					return new Vector3(GetYSize() * 2 - 2, 0f, 0f);
				}
			}
		}
		else
		{
			switch (mapRot)
			{
			case 1:
				_transform.rotation = Quaternion.Euler(0f, 0f, 0f);
				break;
			case 2:
				_transform.rotation = Quaternion.Euler(0f, 90f, 0f);
				break;
			case 3:
				_transform.rotation = Quaternion.Euler(0f, 180f, 0f);
				break;
			case 4:
				_transform.rotation = Quaternion.Euler(0f, 270f, 0f);
				break;
			}
		}
		return Vector3.zero;
	}

	public void setRotatiomNumber(int mapRot)
	{
		if (IsMultiTileObject())
		{
			switch (mapRot)
			{
			case 1:
				_transform.rotation = Quaternion.Euler(0f, 0f, 0f);
				break;
			case 2:
				_transform.rotation = Quaternion.Euler(0f, 90f, 0f);
				_transform.position += new Vector3(0f, 0f, GetXSize() * 2 - 2);
				break;
			case 3:
				_transform.rotation = Quaternion.Euler(0f, 180f, 0f);
				_transform.position += new Vector3(GetXSize() * 2 - 2, 0f, GetYSize() * 2 - 2);
				break;
			case 4:
				_transform.rotation = Quaternion.Euler(0f, 270f, 0f);
				_transform.position += new Vector3(GetYSize() * 2 - 2, 0f, 0f);
				break;
			}
		}
		else
		{
			switch (mapRot)
			{
			case 1:
				_transform.rotation = Quaternion.Euler(0f, 0f, 0f);
				break;
			case 2:
				_transform.rotation = Quaternion.Euler(0f, 90f, 0f);
				break;
			case 3:
				_transform.rotation = Quaternion.Euler(0f, 180f, 0f);
				break;
			case 4:
				_transform.rotation = Quaternion.Euler(0f, 270f, 0f);
				break;
			}
		}
	}

	private void setMapRotationInside(int xPos, int yPos, HouseDetails house)
	{
		setRotatiomNumber(house.houseMapRotation[xPos, yPos]);
	}

	private void setMapRotationBridge(int xPos, int yPos)
	{
		_transform.position = new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2);
		if (WorldManager.Instance.rotationMap[xPos, yPos] == 1)
		{
			_transform.rotation = Quaternion.Euler(0f, 0f, 0f);
			_transform.position += new Vector3(0f, 0f, WorldManager.Instance.onTileStatusMap[xPos, yPos] * 2 - 2);
		}
		else if (WorldManager.Instance.rotationMap[xPos, yPos] == 2)
		{
			_transform.rotation = Quaternion.Euler(0f, 90f, 0f);
			_transform.position += new Vector3(WorldManager.Instance.onTileStatusMap[xPos, yPos] * 2 - 2, 0f, 2f);
		}
		else if (WorldManager.Instance.rotationMap[xPos, yPos] == 3)
		{
			_transform.rotation = Quaternion.Euler(0f, 180f, 0f);
			_transform.position += new Vector3(GetXSize() * 2 - 2, 0f, GetYSize() * 2 - 2);
		}
		else if (WorldManager.Instance.rotationMap[xPos, yPos] == 4)
		{
			_transform.rotation = Quaternion.Euler(0f, 270f, 0f);
			_transform.position += new Vector3(GetYSize() * 2 - 2, 0f, 0f);
		}
	}

	private void setMapRotation(int xPos, int yPos)
	{
		setRotatiomNumber(WorldManager.Instance.rotationMap[xPos, yPos]);
	}

	public void getRandomRot(int xPos, int yPos)
	{
		Random.InitState(xPos * yPos + xPos - yPos);
		_transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
		if (hasRandomScale())
		{
			float num = Random.Range(0.75f, 1.1f);
			_transform.localScale = new Vector3(num, num, num);
		}
	}

	public void placeDown()
	{
		StartCoroutine(AnimateAppear());
	}

	private IEnumerator AnimateAppear()
	{
		bounceAnimPlaying = true;
		if ((bool)AnimDamage)
		{
			float journey = 0f;
			float duration = 0.25f;
			while (journey <= duration)
			{
				journey += Time.deltaTime;
				float time = Mathf.Clamp01(journey / duration);
				float t = UIAnimationManager.manage.placeableYCurve.Evaluate(time);
				float t2 = UIAnimationManager.manage.placeableXCurve.Evaluate(time);
				float num = Mathf.LerpUnclamped(0.25f, 1f, t2);
				float y = Mathf.LerpUnclamped(0.1f, 1f, t);
				AnimDamage.localScale = new Vector3(num, y, num);
				yield return null;
			}
			AnimDamage.localScale = Vector3.one;
		}
		bounceAnimPlaying = false;
	}

	private IEnumerator damageBounce()
	{
		float scaleTimer = 0f;
		bounceAnimPlaying = true;
		float bounceAmount = 0.02f;
		if (WorldManager.Instance.allObjectSettings[tileObjectId].isGrass)
		{
			bounceAmount = 0.03f;
		}
		else if (WorldManager.Instance.allObjectSettings[tileObjectId].isWood || WorldManager.Instance.allObjectSettings[tileObjectId].isStone || WorldManager.Instance.allObjectSettings[tileObjectId].isHardWood || WorldManager.Instance.allObjectSettings[tileObjectId].isHardWood)
		{
			bounceAmount = 0.01f;
		}
		for (; scaleTimer < bounceAmount; scaleTimer += Time.deltaTime / 4f)
		{
			yield return null;
			AnimDamage.localScale = new Vector3(1f - scaleTimer * 1.1f, 1f - scaleTimer, 1f - scaleTimer * 1.1f);
		}
		while (scaleTimer > 0f)
		{
			yield return null;
			AnimDamage.localScale = new Vector3(1f - scaleTimer * 1.1f, 1f - scaleTimer, 1f - scaleTimer * 1.1f);
			scaleTimer -= Time.deltaTime / 5f;
		}
		for (; scaleTimer < bounceAmount; scaleTimer += Time.deltaTime / 8f)
		{
			yield return null;
			AnimDamage.localScale = new Vector3(1f + scaleTimer * 1.1f, 1f + scaleTimer, 1f + scaleTimer * 1.1f);
		}
		while (scaleTimer > 0f)
		{
			yield return null;
			AnimDamage.localScale = new Vector3(1f + scaleTimer * 1.1f, 1f + scaleTimer, 1f + scaleTimer * 1.1f);
			scaleTimer -= Time.deltaTime / 10f;
		}
		AnimDamage.localScale = Vector3.one;
		bounceAnimPlaying = false;
	}

	private IEnumerator damageNoAnim()
	{
		int num = Random.Range(-2, 2);
		int num2 = Random.Range(-2, 2);
		while (num == 0)
		{
			num = Random.Range(-2, 2);
		}
		while (num2 == 0)
		{
			num2 = Random.Range(-2, 2);
		}
		float shakeX = 1f * (float)num;
		float shakeY = 1f * (float)num2;
		damageAnimPlaying = true;
		float currentVelocityX = 0f;
		float currentVelocityY = 0f;
		while (damageAnimPlaying)
		{
			AnimDamage.localRotation = Quaternion.Lerp(AnimDamage.localRotation, Quaternion.Euler(shakeX, 0f, shakeY), Time.deltaTime * 50f);
			if ((bool)AnimDamage2)
			{
				AnimDamage2.localRotation = Quaternion.Lerp(AnimDamage2.localRotation, Quaternion.Euler((0f - shakeX) * 2f, 0f, (0f - shakeY) * 2f), Time.deltaTime * 25f);
			}
			if (shakeX == 0f && currentVelocityX == 0f && shakeY == 0f && currentVelocityY == 0f)
			{
				if (AnimDamage.localRotation.x < 0.15f && AnimDamage.localRotation.x > -0.15f && AnimDamage.localRotation.z < 0.15f && AnimDamage.localRotation.z > -0.15f)
				{
					AnimDamage.localRotation = Quaternion.Euler(0f, 0f, 0f);
					if ((bool)AnimDamage2)
					{
						AnimDamage2.localRotation = Quaternion.Euler(0f, 0f, 0f);
					}
					damageAnimPlaying = false;
				}
			}
			else
			{
				calcSpring(shakeX, currentVelocityX, out shakeX, out currentVelocityX);
				calcSpring(shakeY, currentVelocityY, out shakeY, out currentVelocityY);
			}
			yield return null;
		}
	}

	public bool isAPlantThatSpoutsOut(int tileObjectId)
	{
		_ = WorldManager.Instance.allObjects[tileObjectId];
		return true;
	}

	public void calcSpring(float shake, float currentVelocity, out float outShake, out float outVel)
	{
		currentVelocity = currentVelocity * Mathf.Max(0f, 1f - 0.05f * Time.fixedDeltaTime * 50f) + (0f - shake) * 1f * Time.fixedDeltaTime * 50f;
		shake += currentVelocity * Time.fixedDeltaTime;
		if (Mathf.Abs(shake - 0f) < 0.01f && Mathf.Abs(currentVelocity) < 0.01f)
		{
			shake = 0f;
			currentVelocity = 0f;
		}
		outShake = shake;
		outVel = currentVelocity;
	}

	public int getEffectedBuffLevel()
	{
		if (WorldManager.Instance.allObjectSettings[tileObjectId].isHardWood || WorldManager.Instance.allObjectSettings[tileObjectId].isWood)
		{
			return StatusManager.manage.getBuffLevel(StatusManager.BuffType.loggingBuff);
		}
		if (WorldManager.Instance.allObjectSettings[tileObjectId].isStone || WorldManager.Instance.allObjectSettings[tileObjectId].isHardStone)
		{
			return StatusManager.manage.getBuffLevel(StatusManager.BuffType.miningBuff);
		}
		return 0;
	}
}
