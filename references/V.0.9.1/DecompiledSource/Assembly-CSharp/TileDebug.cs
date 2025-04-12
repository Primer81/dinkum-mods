using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TileDebug : MonoBehaviour
{
	public static TileDebug debug;

	public Text title;

	public Text tilePos;

	public Text tileType;

	public Text onTileId;

	public Text tileHeight;

	public Text tileStatus;

	public Text bytes;

	public Text chunkChangeText;

	private float deltaTime;

	public GameObject spawnAnimalButton;

	public Transform animalDebugWindow;

	public Transform spawnAnimalButtonsTransform;

	private void Awake()
	{
		debug = this;
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	private IEnumerator countBPS()
	{
		yield return new WaitForSeconds(1f);
	}

	private void Update()
	{
		if (!NetworkMapSharer.Instance.localChar)
		{
			return;
		}
		if (NetworkMapSharer.Instance.localChar.myInteract.IsInsidePlayerHouse)
		{
			int num = (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.x;
			int num2 = (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.y;
			int num3 = NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.houseMapOnTile[num, num2];
			title.text = "House tile =  | [" + num + "," + num2 + "] =";
			tileType.text = "<color=yellow>On House Tile:</color>" + num3;
			tileHeight.text = "House tile Rotation: " + NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.houseMapRotation[num, num2];
			tilePos.text = "House tile Status " + NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.houseMapOnTileStatus[num, num2];
			onTileId.text = "";
			tileStatus.text = "";
			return;
		}
		title.text = "Debug | [" + Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position.x / 2f) + "," + Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position.z / 2f) + "] =";
		title.text += WorldManager.Instance.onTileMap[Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position.x / 2f), Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position.z / 2f)];
		int num4 = (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.x;
		int num5 = (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.y;
		tilePos.text = num4 + "," + num5 + "  |  Biome: " + GenerateMap.generate.checkBiomType(num4, num5);
		tileType.text = "Tile Type [" + WorldManager.Instance.tileTypeMap[num4, num5] + "]<color=yellow> " + WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num4, num5]].name + "</color>";
		Text text = tileType;
		text.text = text.text + "    | Under Tile: [" + WorldManager.Instance.tileTypeStatusMap[num4, num5] + "] ";
		if (WorldManager.Instance.tileTypeStatusMap[num4, num5] >= 0)
		{
			Text text2 = tileType;
			text2.text = text2.text + "<color=yellow>" + WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeStatusMap[num4, num5]].name + "</color>";
		}
		else
		{
			tileType.text += "<color=red>Nothing Under</color>";
		}
		onTileId.text = "Ontile [" + WorldManager.Instance.onTileMap[num4, num5] + "] ";
		if (WorldManager.Instance.onTileMap[num4, num5] >= 0)
		{
			if (WorldManager.Instance.onTileMap[num4, num5] == 500)
			{
				onTileId.text += "<color=red> A request tile from server</color>";
			}
			else
			{
				Text text3 = onTileId;
				text3.text = text3.text + "<color=yellow>" + WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[num4, num5]].name + "</color>";
			}
		}
		else
		{
			onTileId.text += "<color=red>Empty Tile</color>";
		}
		tileHeight.text = "Tile Height [" + WorldManager.Instance.heightMap[num4, num5] + "]--Rotation: " + WorldManager.Instance.rotationMap[num4, num5];
		if (WorldManager.Instance.onTileMap[num4, num5] >= 0 && WorldManager.Instance.onTileMap[num4, num5] != 500)
		{
			if ((bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[num4, num5]].tileObjectItemChanger && WorldManager.Instance.onTileStatusMap[num4, num5] >= 0)
			{
				tileStatus.text = "Changing item ID: " + WorldManager.Instance.onTileStatusMap[num4, num5] + Inventory.Instance.allItems[WorldManager.Instance.onTileStatusMap[num4, num5]].itemName;
			}
			else if ((bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[num4, num5]].tileObjectGrowthStages)
			{
				tileStatus.text = "Growing: " + WorldManager.Instance.onTileStatusMap[num4, num5] + " of " + (WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[num4, num5]].tileObjectGrowthStages.objectStages.Length - 1);
			}
			else
			{
				tileStatus.text = "Tile Status [" + WorldManager.Instance.onTileStatusMap[num4, num5] + "]";
			}
		}
		else
		{
			tileStatus.text = "Tile Status [" + WorldManager.Instance.onTileStatusMap[num4, num5] + "]";
		}
		BuriedItem buriedItem = BuriedManager.manage.checkIfBuriedItem(num4, num5);
		if (buriedItem != null)
		{
			Text text4 = tileStatus;
			text4.text = text4.text + "| Burried Map = <color=red>" + Inventory.Instance.allItems[buriedItem.itemId].itemName + "</color>";
		}
		else if (WorldManager.Instance.onTileMap[num4, num5] == 30)
		{
			tileStatus.text += "| Burried Map = <color=red> Buried item to be generated! </color>";
		}
		else
		{
			tileStatus.text += "| Burried Map = NONE";
		}
		Text text5 = tileStatus;
		text5.text = text5.text + "| <color=blue> Water = " + WorldManager.Instance.waterMap[num4, num5] + "</color>";
		if (WorldManager.Instance.fencedOffMap[num4, num5] == 0)
		{
			bytes.text = "Fenced off? : <b><color=red>No.</color></b>";
		}
		else
		{
			bytes.text = "Fenced off? : <b><color=green>YUP! - GroupId = " + WorldManager.Instance.fencedOffMap[num4, num5] + " </color></b>";
		}
		int num6 = Mathf.RoundToInt(num4 / 10) * 10;
		int num7 = Mathf.RoundToInt(num5 / 10) * 10;
		if (WorldManager.Instance.chunkChangedMap[num6 / 10, num7 / 10])
		{
			chunkChangeText.text = "Changed";
			if (WorldManager.Instance.changedMapOnTile[num6 / 10, num7 / 10])
			{
				chunkChangeText.text += " [<color=green>On Tile</color>]";
			}
			if (WorldManager.Instance.changedMapTileType[num6 / 10, num7 / 10])
			{
				chunkChangeText.text += " [<color=green>Type</color>]";
			}
			if (WorldManager.Instance.changedMapHeight[num6 / 10, num7 / 10])
			{
				chunkChangeText.text += " [<color=green>Height</color>]";
			}
			if (WorldManager.Instance.changedMapWater[num6 / 10, num7 / 10])
			{
				chunkChangeText.text += " [<color=green>Water</color>]";
			}
		}
		else
		{
			chunkChangeText.text = "No Change";
		}
	}
}
