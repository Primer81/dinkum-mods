using UnityEngine;

public class UIAnimationManager : MonoBehaviour
{
	public static UIAnimationManager manage;

	[Header("Main Window --------------")]
	public AnimationCurve windowsOpenCurve;

	[Header("TilePlacable --------------")]
	public AnimationCurve placeableYCurve;

	public AnimationCurve placeableXCurve;

	[Header("FlyingCurve --------------")]
	public AnimationCurve flyingHeightCurve;

	public AnimationCurve flyingHorizonalCurve;

	[Header("Invslots --------------")]
	public AnimationCurve invSlotUpdateCurve;

	public AnimationCurve invSlotSelectedCurve;

	public AnimationCurve invSlotDeselectCurve;

	[Header("Button Animators --------------")]
	public AnimationCurve buttonPressCurve;

	public AnimationCurve buttonHoverCurve;

	public AnimationCurve buttonRollOutCurve;

	[Header("Character Animators --------------")]
	public AnimationCurve hairChangeBounce;

	[Header("TextColors --------------")]
	public Color itemNameColor;

	public Color characterNameColor;

	public Color moneyAmountColor;

	public Color pointsAmountColor;

	public Color fadedColor;

	[Header("UITextColors --------------")]
	public Color plantableText;

	public Color consumableText;

	[Header("UI Yes No Colors-----")]
	public Color yesColor;

	public Color noColor;

	[Header("Invslot Types-----")]
	public Sprite baseSlot;

	public Sprite toolSlot;

	public Sprite eatableSlot;

	public Sprite placeableSlot;

	public Sprite bugOrFishSlot;

	public Sprite clothSlot;

	public Sprite vehicleSlot;

	public Sprite relicSlot;

	public Color colourBaseSlot;

	public Color colourToolSlot;

	public Color colourEatableSlot;

	public Color colourPlaceableSlot;

	public Color colourBugOrFishSlot;

	public Color colourClothSlot;

	public Color colourVehicleSlot;

	public Color colourRelicSlot;

	[Header("Dropped Item Curve --------------")]
	public AnimationCurve droppedItemCurve;

	private void Awake()
	{
		manage = this;
	}

	public string GetItemColorTag(string inString)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(itemNameColor) + ">" + inString + "</color>";
	}

	public string GetCharacterNameTag(string inString)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(characterNameColor) + ">" + inString + "</color>";
	}

	public string MoneyAmountColorTag(string inString)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(moneyAmountColor) + ">" + inString + "</color>";
	}

	public string PointsAmountColorTag(string inString)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(pointsAmountColor) + ">" + inString + "</color>";
	}

	public string WrapStringInYesColor(string text)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(yesColor) + ">" + text + "</color>";
	}

	public string WrapStringInNotEnoughColor(string text)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(fadedColor) + ">" + text + "</color>";
	}

	public Sprite getSlotSprite(int itemId)
	{
		if ((bool)Inventory.Instance.allItems[itemId].spawnPlaceable)
		{
			if ((bool)Inventory.Instance.allItems[itemId].spawnPlaceable.GetComponent<Vehicle>())
			{
				return vehicleSlot;
			}
			return placeableSlot;
		}
		if ((bool)Inventory.Instance.allItems[itemId].bug || (bool)Inventory.Instance.allItems[itemId].fish || (bool)Inventory.Instance.allItems[itemId].underwaterCreature)
		{
			return bugOrFishSlot;
		}
		if (Inventory.Instance.allItems[itemId].isATool)
		{
			return toolSlot;
		}
		if ((bool)Inventory.Instance.allItems[itemId].relic)
		{
			return relicSlot;
		}
		if ((bool)Inventory.Instance.allItems[itemId].equipable && Inventory.Instance.allItems[itemId].equipable.cloths)
		{
			return clothSlot;
		}
		if ((bool)Inventory.Instance.allItems[itemId].consumeable)
		{
			return eatableSlot;
		}
		if (((bool)Inventory.Instance.allItems[itemId].placeable && !Inventory.Instance.allItems[itemId].burriedPlaceable) || (Inventory.Instance.allItems[itemId].placeableTileType > -1 && WorldManager.Instance.tileTypes[Inventory.Instance.allItems[itemId].placeableTileType].isPath))
		{
			return placeableSlot;
		}
		if ((bool)Inventory.Instance.allItems[itemId].itemChange && (bool)Inventory.Instance.allItems[itemId].itemChange.changesAndTheirChanger[0].changesWhenComplete && (bool)Inventory.Instance.allItems[itemId].itemChange.changesAndTheirChanger[0].changesWhenComplete.consumeable)
		{
			return eatableSlot;
		}
		return baseSlot;
	}

	public Color getSlotColour(int itemId)
	{
		if (itemId > -1)
		{
			if ((bool)Inventory.Instance.allItems[itemId].spawnPlaceable)
			{
				if ((bool)Inventory.Instance.allItems[itemId].spawnPlaceable.GetComponent<Vehicle>())
				{
					return colourVehicleSlot;
				}
				return colourPlaceableSlot;
			}
			if ((bool)Inventory.Instance.allItems[itemId].bug || (bool)Inventory.Instance.allItems[itemId].fish || (bool)Inventory.Instance.allItems[itemId].underwaterCreature)
			{
				return colourBugOrFishSlot;
			}
			if (Inventory.Instance.allItems[itemId].isATool)
			{
				return colourToolSlot;
			}
			if ((bool)Inventory.Instance.allItems[itemId].relic)
			{
				return colourRelicSlot;
			}
			if ((bool)Inventory.Instance.allItems[itemId].equipable && Inventory.Instance.allItems[itemId].equipable.cloths)
			{
				return colourClothSlot;
			}
			if ((bool)Inventory.Instance.allItems[itemId].consumeable)
			{
				return colourEatableSlot;
			}
			if (((bool)Inventory.Instance.allItems[itemId].placeable && !Inventory.Instance.allItems[itemId].burriedPlaceable) || (Inventory.Instance.allItems[itemId].placeableTileType > -1 && WorldManager.Instance.tileTypes[Inventory.Instance.allItems[itemId].placeableTileType].isPath))
			{
				return colourPlaceableSlot;
			}
			if ((bool)Inventory.Instance.allItems[itemId].itemChange && (bool)Inventory.Instance.allItems[itemId].itemChange.changesAndTheirChanger[0].changesWhenComplete && (bool)Inventory.Instance.allItems[itemId].itemChange.changesAndTheirChanger[0].changesWhenComplete.consumeable)
			{
				return colourEatableSlot;
			}
		}
		return colourBaseSlot;
	}
}
