using UnityEngine;
using UnityEngine.Events;

public class TileObjectConnect : MonoBehaviour
{
	public GameObject leftConnect;

	public GameObject rightConnect;

	public GameObject upConnect;

	public GameObject downConnect;

	private GameObject[] connections = new GameObject[4];

	public Transform heighTrans;

	public static UnityEvent heightPlaceChange = new UnityEvent();

	private int myTileType;

	private int myTileHeight;

	private int myTileX;

	private int myTileY;

	public bool matchHeight;

	public bool connectToNeighbours = true;

	public bool inverted;

	public bool specialWhenNoNeighbour;

	public GameObject showInsteadOfAllConnect;

	public GameObject colliderWhenNotAllOn;

	public TileObject myTileObject;

	public TileObject[] canConnectTo;

	public TileObjectConnect secondConnect;

	public bool secondConnectUseNeighbourHeight;

	public bool isFence;

	public bool waterConnect;

	public Ladder IsLadder;

	private void Awake()
	{
		connections[0] = upConnect;
		connections[1] = leftConnect;
		connections[2] = rightConnect;
		connections[3] = downConnect;
	}

	private void OnEnable()
	{
		if (matchHeight)
		{
			heightPlaceChange.AddListener(getHighestNeighbour);
			heightPlaceChange.Invoke();
		}
	}

	private void OnDisable()
	{
		if (matchHeight)
		{
			heightPlaceChange.RemoveListener(getHighestNeighbour);
		}
	}

	public void connectToTiles(int xTile, int yTile, int rotPreview = -1)
	{
		if ((bool)IsLadder)
		{
			int num = WorldManager.Instance.rotationMap[xTile, yTile];
			if (rotPreview != -1)
			{
				num = rotPreview;
			}
			switch (num)
			{
			case 0:
			case 1:
				if (WorldManager.Instance.isPositionOnMap(xTile, yTile + 1))
				{
					IsLadder.SetToLevel(WorldManager.Instance.heightMap[xTile, yTile + 1] - WorldManager.Instance.heightMap[xTile, yTile]);
				}
				break;
			case 2:
				if (WorldManager.Instance.isPositionOnMap(xTile + 1, yTile))
				{
					IsLadder.SetToLevel(WorldManager.Instance.heightMap[xTile + 1, yTile] - WorldManager.Instance.heightMap[xTile, yTile]);
				}
				break;
			case 3:
				if (WorldManager.Instance.isPositionOnMap(xTile, yTile - 1))
				{
					IsLadder.SetToLevel(WorldManager.Instance.heightMap[xTile, yTile - 1] - WorldManager.Instance.heightMap[xTile, yTile]);
				}
				break;
			case 4:
				if (WorldManager.Instance.isPositionOnMap(xTile - 1, yTile))
				{
					IsLadder.SetToLevel(WorldManager.Instance.heightMap[xTile - 1, yTile] - WorldManager.Instance.heightMap[xTile, yTile]);
				}
				break;
			}
			return;
		}
		if (connectToNeighbours && (bool)myTileObject && myTileObject.GetRotationFromMap())
		{
			if (rotPreview == -1)
			{
				swapForRotation(WorldManager.Instance.rotationMap[xTile, yTile]);
			}
			else
			{
				swapForRotation(rotPreview);
			}
		}
		myTileType = WorldManager.Instance.onTileMap[xTile, yTile];
		myTileHeight = WorldManager.Instance.heightMap[xTile, yTile];
		myTileX = xTile;
		myTileY = yTile;
		if (waterConnect)
		{
			doWaterConnect(xTile, yTile);
			return;
		}
		if (connectToNeighbours)
		{
			if ((bool)showInsteadOfAllConnect && neighbourSame(xTile - 1, yTile) && neighbourSame(xTile + 1, yTile) && neighbourSame(xTile, yTile + 1) && neighbourSame(xTile, yTile - 1))
			{
				showInsteadOfAllConnect.SetActive(value: true);
				colliderWhenNotAllOn.SetActive(value: false);
				pieceOn(leftConnect, isEnabled: false);
				pieceOn(rightConnect, isEnabled: false);
				pieceOn(upConnect, isEnabled: false);
				pieceOn(downConnect, isEnabled: false);
				if ((bool)secondConnect)
				{
					secondConnect.pieceOn(secondConnect.leftConnect, isEnabled: false);
					secondConnect.pieceOn(secondConnect.rightConnect, isEnabled: false);
					secondConnect.pieceOn(secondConnect.upConnect, isEnabled: false);
					secondConnect.pieceOn(secondConnect.downConnect, isEnabled: false);
				}
				return;
			}
			if ((bool)showInsteadOfAllConnect)
			{
				showInsteadOfAllConnect.SetActive(value: false);
				colliderWhenNotAllOn.SetActive(value: true);
			}
			if (!inverted)
			{
				pieceOn(leftConnect, neighbourSame(xTile - 1, yTile));
				pieceOn(rightConnect, neighbourSame(xTile + 1, yTile));
				pieceOn(upConnect, neighbourSame(xTile, yTile + 1));
				pieceOn(downConnect, neighbourSame(xTile, yTile - 1));
			}
			else
			{
				pieceOn(leftConnect, !neighbourSame(xTile - 1, yTile));
				pieceOn(rightConnect, !neighbourSame(xTile + 1, yTile));
				pieceOn(upConnect, !neighbourSame(xTile, yTile + 1));
				pieceOn(downConnect, !neighbourSame(xTile, yTile - 1));
			}
		}
		if (specialWhenNoNeighbour)
		{
			if (!neighbourSame(xTile - 1, yTile) && !neighbourSame(xTile + 1, yTile) && !neighbourSame(xTile, yTile + 1) && !neighbourSame(xTile, yTile - 1))
			{
				if (rotPreview == 1 || rotPreview == 3 || (rotPreview == -1 && WorldManager.Instance.rotationMap[xTile, yTile] == 1) || (rotPreview == -1 && WorldManager.Instance.rotationMap[xTile, yTile] == 3))
				{
					pieceOn(leftConnect, isEnabled: true);
					pieceOn(rightConnect, isEnabled: true);
					pieceOn(upConnect, isEnabled: false);
					pieceOn(downConnect, isEnabled: false);
				}
				else
				{
					pieceOn(upConnect, isEnabled: true);
					pieceOn(downConnect, isEnabled: true);
					pieceOn(leftConnect, isEnabled: false);
					pieceOn(rightConnect, isEnabled: false);
				}
			}
			else
			{
				if (rightConnect.activeInHierarchy || leftConnect.activeInHierarchy)
				{
					pieceOn(leftConnect, isEnabled: true);
					pieceOn(rightConnect, isEnabled: true);
				}
				if (upConnect.activeInHierarchy || downConnect.activeInHierarchy)
				{
					pieceOn(upConnect, isEnabled: true);
					pieceOn(downConnect, isEnabled: true);
				}
			}
		}
		if (matchHeight && base.gameObject.activeSelf)
		{
			getHighestNeighbour();
		}
		if ((bool)secondConnect)
		{
			secondConnect.connectToTiles(xTile, yTile);
		}
	}

	public void connectToTilesInside(int xTile, int yTile, HouseDetails house, int xSize, int ySize)
	{
		pieceOn(leftConnect, neighbourSameInside(xTile - 1, yTile, house, xSize, ySize));
		pieceOn(rightConnect, neighbourSameInside(xTile + 1, yTile, house, xSize, ySize));
		pieceOn(upConnect, neighbourSameInside(xTile, yTile + 1, house, xSize, ySize));
		pieceOn(downConnect, neighbourSameInside(xTile, yTile - 1, house, xSize, ySize));
	}

	public void getHighestNeighbour()
	{
		if (!matchHeight || !myTileObject || WorldManager.Instance.onTileMap[myTileX, myTileY] != myTileObject.tileObjectId || myTileX == 0 || myTileY == 0 || !matchHeight)
		{
			return;
		}
		int num = myTileX;
		int num2 = myTileY;
		int num3 = myTileHeight;
		int num4 = WorldManager.Instance.onTileStatusMap[num, num2];
		for (int i = 1; checkNeighbourIsConnectable(WorldManager.Instance.onTileMap[num - i, num2]); i++)
		{
			if (WorldManager.Instance.heightMap[num - i, num2] > num3)
			{
				num3 = WorldManager.Instance.heightMap[num - i, num2];
			}
			if (WorldManager.Instance.onTileStatusMap[num - i, num2] > num4)
			{
				num4 = WorldManager.Instance.onTileStatusMap[num - i, num2];
			}
		}
		for (int j = 1; checkNeighbourIsConnectable(WorldManager.Instance.onTileMap[num + j, num2]); j++)
		{
			if (WorldManager.Instance.heightMap[num + j, num2] > num3)
			{
				num3 = WorldManager.Instance.heightMap[num + j, num2];
			}
			if (WorldManager.Instance.onTileStatusMap[num + j, num2] > num4)
			{
				num4 = WorldManager.Instance.onTileStatusMap[num + j, num2];
			}
		}
		for (int k = 1; checkNeighbourIsConnectable(WorldManager.Instance.onTileMap[num, num2 - k]); k++)
		{
			if (WorldManager.Instance.heightMap[num, num2 - k] > num3)
			{
				num3 = WorldManager.Instance.heightMap[num, num2 - k];
			}
			if (WorldManager.Instance.onTileStatusMap[num, num2 - k] > num4)
			{
				num4 = WorldManager.Instance.onTileStatusMap[num, num2 - k];
			}
		}
		for (int l = 1; checkNeighbourIsConnectable(WorldManager.Instance.onTileMap[num, num2 + l]); l++)
		{
			if (WorldManager.Instance.heightMap[num, num2 + l] > num3)
			{
				num3 = WorldManager.Instance.heightMap[num, num2 + l];
			}
			if (WorldManager.Instance.onTileStatusMap[num, num2 + l] > num4)
			{
				num4 = WorldManager.Instance.onTileStatusMap[num, num2 + l];
			}
		}
		if (num3 > num4)
		{
			heighTrans.position = new Vector3(base.transform.position.x, num3, base.transform.position.z);
			WorldManager.Instance.onTileStatusMap[num, num2] = num3;
		}
		else
		{
			heighTrans.position = new Vector3(base.transform.position.x, num4, base.transform.position.z);
			WorldManager.Instance.onTileStatusMap[num, num2] = num4;
		}
	}

	private bool neighbourSame(int neighbourX, int neighbourY)
	{
		if (neighbourX < 0 || neighbourX > WorldManager.Instance.GetMapSize() - 1 || neighbourY < 0 || neighbourY > WorldManager.Instance.GetMapSize() - 1)
		{
			return false;
		}
		if (WorldManager.Instance.onTileMap[neighbourX, neighbourY] < -1)
		{
			Vector2 vector = moveSelectionToMainTileForMultiTiledObject(neighbourX, neighbourY);
			neighbourX = (int)vector.x;
			neighbourY = (int)vector.y;
		}
		if (checkNeighbourIsConnectable(WorldManager.Instance.onTileMap[neighbourX, neighbourY]))
		{
			if (matchHeight)
			{
				return true;
			}
			if (secondConnectUseNeighbourHeight)
			{
				return true;
			}
			if (!matchHeight && myTileHeight == WorldManager.Instance.heightMap[neighbourX, neighbourY])
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private bool neighbourSameInside(int neighbourX, int neighbourY, HouseDetails house, int xSize, int ySize)
	{
		if (neighbourX < 0 || neighbourX >= xSize || neighbourY < 0 || neighbourY >= ySize)
		{
			return true;
		}
		if (house.houseMapOnTile[neighbourX, neighbourY] < -1)
		{
			Vector2 vector = moveSelectionToMainTileForMultiTiledObjectInside(neighbourX, neighbourY, house, xSize, ySize);
			neighbourX = (int)vector.x;
			neighbourY = (int)vector.y;
		}
		if (house.houseMapOnTile[neighbourX, neighbourY] == -1)
		{
			return false;
		}
		if (checkNeighbourIsConnectable(house.houseMapOnTile[neighbourX, neighbourY]))
		{
			return true;
		}
		return false;
	}

	private bool neighbourIsHigher(int neighbourX, int neighbourY)
	{
		if (myTileHeight < WorldManager.Instance.heightMap[neighbourX, neighbourY])
		{
			return true;
		}
		return false;
	}

	public void pieceOn(GameObject dirPiece, bool isEnabled)
	{
		dirPiece.SetActive(isEnabled);
	}

	private bool checkNeighbourIsConnectable(int neighbourToCheck)
	{
		if (neighbourToCheck == -1)
		{
			return false;
		}
		if (myTileType == neighbourToCheck)
		{
			return true;
		}
		if (isFence && neighbourToCheck > -1 && (bool)WorldManager.Instance.allObjects[neighbourToCheck].tileObjectConnect && WorldManager.Instance.allObjects[neighbourToCheck].tileObjectConnect.isFence)
		{
			return true;
		}
		if (canConnectTo.Length != 0)
		{
			for (int i = 0; i < canConnectTo.Length; i++)
			{
				if (canConnectTo[i].tileObjectId == neighbourToCheck)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void swapForRotation(int rotation)
	{
		switch (rotation)
		{
		case 1:
			leftConnect = connections[1];
			rightConnect = connections[2];
			upConnect = connections[0];
			downConnect = connections[3];
			break;
		case 2:
			leftConnect = connections[3];
			rightConnect = connections[0];
			upConnect = connections[1];
			downConnect = connections[2];
			break;
		case 3:
			leftConnect = connections[2];
			rightConnect = connections[1];
			upConnect = connections[3];
			downConnect = connections[0];
			break;
		default:
			leftConnect = connections[0];
			rightConnect = connections[3];
			upConnect = connections[2];
			downConnect = connections[1];
			break;
		}
	}

	public Vector2 moveSelectionToMainTileForMultiTiledObject(int xPos, int yPos)
	{
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		int num2 = 0;
		while (!flag || !flag2)
		{
			if (!flag2)
			{
				if (checkIfOnMap(xPos + num))
				{
					if (WorldManager.Instance.onTileMap[xPos + num, yPos] == -3)
					{
						flag2 = true;
					}
					else if (WorldManager.Instance.onTileMap[xPos + num, yPos] == -4 && checkIfOnMap(xPos + (num - 1)) && WorldManager.Instance.onTileMap[xPos + (num - 1), yPos] != -4)
					{
						num--;
						flag2 = true;
					}
					else
					{
						num--;
					}
				}
				else
				{
					num = 0;
					flag2 = true;
				}
			}
			if (!flag2)
			{
				continue;
			}
			if (checkIfOnMap(yPos + num2))
			{
				if (WorldManager.Instance.onTileMap[xPos + num, yPos + num2] != -3)
				{
					flag = true;
				}
				else if (WorldManager.Instance.onTileMap[xPos + num, yPos + num2] == -3 && checkIfOnMap(yPos + (num2 - 1)) && WorldManager.Instance.onTileMap[xPos + num, yPos + (num2 - 1)] != -3)
				{
					num2--;
					flag = true;
				}
				else
				{
					num2--;
				}
			}
			else
			{
				num2 = 0;
				flag = true;
			}
		}
		xPos += num;
		yPos += num2;
		return new Vector2(xPos, yPos);
	}

	public Vector2 moveSelectionToMainTileForMultiTiledObjectInside(int xPos, int yPos, HouseDetails house, int sizeX, int sizeY)
	{
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		int num2 = 0;
		while (!flag || !flag2)
		{
			if (!flag2)
			{
				if (CheckIfOnMapInside(xPos + num, sizeX))
				{
					if (house.houseMapOnTile[xPos + num, yPos] == -3)
					{
						flag2 = true;
					}
					else if (house.houseMapOnTile[xPos + num, yPos] == -4 && CheckIfOnMapInside(xPos + (num - 1), sizeX) && house.houseMapOnTile[xPos + (num - 1), yPos] != -4)
					{
						num--;
						flag2 = true;
					}
					else
					{
						num--;
					}
				}
				else
				{
					num = 0;
					flag2 = true;
				}
			}
			if (!flag2)
			{
				continue;
			}
			if (CheckIfOnMapInside(yPos + num2, sizeX))
			{
				if (house.houseMapOnTile[xPos + num, yPos + num2] != -3)
				{
					flag = true;
				}
				else if (house.houseMapOnTile[xPos + num, yPos + num2] == -3 && CheckIfOnMapInside(yPos + (num2 - 1), sizeX) && house.houseMapOnTile[xPos + num, yPos + (num2 - 1)] != -3)
				{
					num2--;
					flag = true;
				}
				else
				{
					num2--;
				}
			}
			else
			{
				num2 = 0;
				flag = true;
			}
		}
		xPos += num;
		yPos += num2;
		return new Vector2(xPos, yPos);
	}

	private bool checkIfOnMap(int intToCheck)
	{
		if (intToCheck >= 0 && intToCheck < WorldManager.Instance.GetMapSize())
		{
			return true;
		}
		return false;
	}

	private bool CheckIfOnMapInside(int intToCheck, int size)
	{
		if (intToCheck >= 0 && intToCheck < size)
		{
			return true;
		}
		return false;
	}

	public bool sideOnForWater(int xTile, int yTile)
	{
		if (sideWaterFallOn(xTile, yTile))
		{
			return false;
		}
		if (neighbourIsHigher(xTile, yTile))
		{
			return false;
		}
		if (neighbourSame(xTile, yTile))
		{
			return false;
		}
		return true;
	}

	public bool sideWaterFallOn(int xTile, int yTile)
	{
		if (myTileHeight > WorldManager.Instance.heightMap[xTile, yTile] && (WorldManager.Instance.onTileMap[xTile, yTile] == myTileType || WorldManager.Instance.waterMap[xTile, yTile]))
		{
			return true;
		}
		return false;
	}

	public void doWaterConnect(int xTile, int yTile)
	{
		pieceOn(leftConnect, sideOnForWater(xTile - 1, yTile));
		pieceOn(rightConnect, sideOnForWater(xTile + 1, yTile));
		pieceOn(upConnect, sideOnForWater(xTile, yTile + 1));
		pieceOn(downConnect, sideOnForWater(xTile, yTile - 1));
		if ((bool)secondConnect)
		{
			pieceOn(secondConnect.leftConnect, sideWaterFallOn(xTile - 1, yTile));
			pieceOn(secondConnect.rightConnect, sideWaterFallOn(xTile + 1, yTile));
			pieceOn(secondConnect.upConnect, sideWaterFallOn(xTile, yTile + 1));
			pieceOn(secondConnect.downConnect, sideWaterFallOn(xTile, yTile - 1));
		}
	}
}
