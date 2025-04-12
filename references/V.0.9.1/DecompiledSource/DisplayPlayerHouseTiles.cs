using UnityEngine;
using UnityEngine.Events;

public class DisplayPlayerHouseTiles : MonoBehaviour
{
	public bool isPlayerHouse;

	public Transform[] gridStartingPositions;

	public int rotationNo;

	private Transform startingPosition;

	public UnityEvent updateHouseContents;

	public TileObject[,] tileObjectsInHouse = new TileObject[25, 25];

	public TileObject[,] tileObjectsOnTop = new TileObject[25, 25];

	public int xSize = 7;

	public int ySize = 5;

	public int xLength;

	public int yLength;

	private bool firstSetUp = true;

	public int housePosX;

	public int housePosY;

	public MeshRenderer insideMesh;

	private TileObject myTileObject;

	private HouseDetails myHouseDetails;

	private HouseExterior exteriorDetails;

	public PlayerHouseExterior myHouseExterior;

	public int houseLevel;

	public string buildingName = "Guest House";

	public void Awake()
	{
		myTileObject = GetComponent<TileObject>();
		HouseManager.manage.housesOnDisplay.Add(this);
	}

	private void OnDestroy()
	{
		clearHouse();
		HouseManager.manage.housesOnDisplay.Remove(this);
		updateHouseContents.RemoveAllListeners();
	}

	private void OnDisable()
	{
		clearHouse();
	}

	public void Start()
	{
	}

	private bool checkIfNeedsUpgrade()
	{
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				if (j < xLength && i < yLength && myHouseDetails.houseMapOnTile[j, i] == -2)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void upgradeHouseSize()
	{
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				if (j >= xLength || i >= yLength)
				{
					myHouseDetails.houseMapOnTile[j, i] = -2;
				}
				else if (myHouseDetails.houseMapOnTile[j, i] == -2)
				{
					myHouseDetails.houseMapOnTile[j, i] = -1;
				}
			}
		}
	}

	public void refreshWalls()
	{
		if (myHouseDetails == null)
		{
			return;
		}
		Material[] materials = new Material[2]
		{
			Inventory.Instance.allItems[myHouseDetails.floor].equipable.material,
			Inventory.Instance.allItems[myHouseDetails.wall].equipable.material
		};
		if ((bool)insideMesh)
		{
			insideMesh.materials = materials;
		}
		for (int i = 0; i < yLength; i++)
		{
			for (int j = 0; j < xLength; j++)
			{
				if ((bool)tileObjectsInHouse[j, i] && (bool)tileObjectsInHouse[j, i].tileObjectConnect)
				{
					MatchInteriorWalls component = tileObjectsInHouse[j, i].GetComponent<MatchInteriorWalls>();
					if ((bool)component)
					{
						component.ChangeMaterialInside(Inventory.Instance.allItems[myHouseDetails.wall].equipable.material);
					}
				}
			}
		}
	}

	public void updateHouseExterior(bool isCustomisation = false)
	{
		if (exteriorDetails != null && (bool)myHouseExterior)
		{
			myHouseExterior.setExterior(exteriorDetails);
		}
		if (isCustomisation)
		{
			SoundManager.Instance.playASoundAtPoint(HouseEditor.edit.changeHouseSound, myHouseExterior.transform.position);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.houseChangePart, myHouseExterior.transform.position, 300);
		}
	}

	public void setInteriorPosAndRotation(int xPos, int yPos)
	{
		if (!NetworkMapSharer.Instance || (NetworkMapSharer.Instance.isServer && !NetworkMapSharer.Instance.serverActive()))
		{
			return;
		}
		myHouseDetails = HouseManager.manage.getHouseInfo(xPos, yPos);
		exteriorDetails = HouseManager.manage.getHouseExterior(xPos, yPos);
		if (exteriorDetails != null)
		{
			exteriorDetails.playerHouse = true;
			exteriorDetails.houseLevel = houseLevel;
			if ((bool)myHouseExterior)
			{
				myHouseExterior.setExterior(exteriorDetails);
			}
		}
		rotationNo = WorldManager.Instance.rotationMap[xPos, yPos];
		startingPosition = gridStartingPositions[Mathf.Clamp(rotationNo - 1, 0, gridStartingPositions.Length)];
		xLength = xSize;
		yLength = ySize;
		housePosX = xPos;
		housePosY = yPos;
		if (rotationNo == 2 || rotationNo == 4)
		{
			xLength = ySize;
			yLength = xSize;
		}
	}

	public void firstRefresh()
	{
		refreshHouseTiles(firstTime: true);
	}

	public int getCurrentHouseId()
	{
		return WorldManager.Instance.onTileMap[housePosX, housePosY];
	}

	public Transform getStartingPosTransform()
	{
		return startingPosition;
	}

	public void clearForUpgrade()
	{
		clearHouse();
		myHouseDetails = null;
	}

	public void clearHouse()
	{
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				if (tileObjectsInHouse[j, i] != null)
				{
					WorldManager.Instance.returnTileObject(tileObjectsInHouse[j, i]);
					tileObjectsInHouse[j, i].gameObject.SetActive(value: false);
					tileObjectsInHouse[j, i] = null;
				}
				if (tileObjectsOnTop[j, i] != null)
				{
					WorldManager.Instance.returnTileObject(tileObjectsOnTop[j, i]);
					tileObjectsOnTop[j, i].gameObject.SetActive(value: false);
					tileObjectsOnTop[j, i] = null;
				}
			}
		}
	}

	public void refreshHouseTiles(bool firstTime = false)
	{
		if (myHouseDetails == null)
		{
			return;
		}
		xLength = xSize;
		yLength = ySize;
		if (rotationNo == 2 || rotationNo == 4)
		{
			xLength = ySize;
			yLength = xSize;
		}
		refreshWalls();
		for (int i = 0; i < yLength; i++)
		{
			for (int j = 0; j < xLength; j++)
			{
				if (myHouseDetails.houseMapOnTile[j, i] > -1)
				{
					Vector3 moveTo = startingPosition.position + new Vector3(j * 2, 0f, i * 2);
					if (tileObjectsInHouse[j, i] == null)
					{
						tileObjectsInHouse[j, i] = WorldManager.Instance.getTileObjectForHouse(myHouseDetails.houseMapOnTile[j, i], moveTo, j, i, myHouseDetails);
						if (!firstTime)
						{
							tileObjectsInHouse[j, i].placeDown();
						}
					}
					else if (tileObjectsInHouse[j, i].tileObjectId != myHouseDetails.houseMapOnTile[j, i])
					{
						WorldManager.Instance.returnTileObject(tileObjectsInHouse[j, i]);
						tileObjectsInHouse[j, i] = WorldManager.Instance.getTileObjectForHouse(myHouseDetails.houseMapOnTile[j, i], moveTo, j, i, myHouseDetails);
						if (!firstTime)
						{
							tileObjectsInHouse[j, i].placeDown();
						}
					}
					if (!tileObjectsInHouse[j, i])
					{
						continue;
					}
					tileObjectsInHouse[j, i].setXAndYForHouse(j, i);
					tileObjectsInHouse[j, i].checkOnTopInside(j, i, myHouseDetails);
					if ((bool)tileObjectsInHouse[j, i].tileObjectWritableSign)
					{
						tileObjectsInHouse[j, i].tileObjectWritableSign.updateSignText(j, i, myHouseDetails.xPos, myHouseDetails.yPos);
					}
					if ((bool)tileObjectsInHouse[j, i].tileObjectConnect)
					{
						tileObjectsInHouse[j, i].tileObjectConnect.connectToTilesInside(j, i, myHouseDetails, xSize, ySize);
						MatchInteriorWalls component = tileObjectsInHouse[j, i].GetComponent<MatchInteriorWalls>();
						if ((bool)component)
						{
							component.ChangeMaterialInside(Inventory.Instance.allItems[myHouseDetails.wall].equipable.material);
						}
					}
					if ((bool)tileObjectsInHouse[j, i].showObjectOnStatusChange)
					{
						tileObjectsInHouse[j, i].showObjectOnStatusChange.showGameObject(j, i, myHouseDetails);
					}
					if ((bool)tileObjectsInHouse[j, i].tileObjectFurniture)
					{
						tileObjectsInHouse[j, i].tileObjectFurniture.updateOnTileStatus(j, i, myHouseDetails);
					}
					if ((bool)tileObjectsInHouse[j, i].tileObjectItemChanger)
					{
						tileObjectsInHouse[j, i].tileObjectItemChanger.mapUpdatePos(j, i, myHouseDetails);
					}
				}
				else if (tileObjectsInHouse[j, i] != null)
				{
					tileObjectsInHouse[j, i].onDeath();
					WorldManager.Instance.returnTileObject(tileObjectsInHouse[j, i]);
					tileObjectsInHouse[j, i] = null;
				}
			}
		}
		for (int k = 0; k < yLength; k++)
		{
			for (int l = 0; l < xLength; l++)
			{
				if (tileObjectsOnTop[l, k] != null)
				{
					WorldManager.Instance.returnTileObject(tileObjectsOnTop[l, k]);
					tileObjectsOnTop[l, k] = null;
				}
			}
		}
	}

	public void MoveCarriablesOnHouseMove()
	{
		Vector3 centerPos = (gridStartingPositions[0].position + gridStartingPositions[1].position + gridStartingPositions[2].position + gridStartingPositions[3].position) / 4f;
		WorldManager.Instance.MoveAllCarriablesInsideHouseToSurfaceOnMove(centerPos, xSize);
	}

	public void PlayWallOrFloorSound()
	{
		Vector3 position = (gridStartingPositions[0].position + gridStartingPositions[1].position + gridStartingPositions[2].position + gridStartingPositions[3].position) / 4f;
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.changeWallOrFloorSound, position);
	}
}
