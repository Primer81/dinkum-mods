using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemDescription : MonoBehaviour
{
	[Header("Weapon Details")]
	public GameObject weaponDamage;

	public TextMeshProUGUI weaponDamageText;

	public TextMeshProUGUI weaponDPSText;

	[Header("Farming Details")]
	public GameObject farmingDetails;

	public TextMeshProUGUI farmingDetailText;

	public TextMeshProUGUI farmingLengthText;

	[Header("ConsumableDetails")]
	public TextMeshProUGUI hungerTimerText;

	public GameObject consumeableWindow;

	public GameObject health;

	public TextMeshProUGUI healthText;

	public GameObject stamina;

	public TextMeshProUGUI staminaText;

	public GameObject healthPlus;

	public TextMeshProUGUI healthPlusText;

	public GameObject staminaPlus;

	public TextMeshProUGUI staminaPlusText;

	[Header("Buff Details")]
	public GameObject buffwindow;

	public GameObject[] buffObjects;

	public Image[] buffLevel;

	public TextMeshProUGUI[] buffSeconds;

	public Sprite buffLevel2;

	public Sprite buffLevel3;

	[Header("WindMill And Sprinklers")]
	public GameObject windmillCompatible;

	public GameObject solarCompatible;

	public GameObject reachTiles;

	public TextMeshProUGUI reachTileText;

	[Header("Bridge Details")]
	public GameObject bridgeWindow;

	public TextMeshProUGUI bridgeWidthText;

	public void fillItemDescription(InventoryItem item)
	{
		if (item == null)
		{
			return;
		}
		if ((bool)item.itemPrefab)
		{
			MeleeAttacks component = item.itemPrefab.GetComponent<MeleeAttacks>();
			if ((bool)component && component.isWeapon)
			{
				weaponDamage.SetActive(value: true);
				float num = component.framesBeforeAttackLocked + component.framesAfterAttackLocked + component.attackFramesLength;
				float num2 = 60f;
				float num3 = num / num2;
				float num4 = 1f / num3;
				float num5 = item.weaponDamage * num4;
				weaponDamageText.text = item.weaponDamage.ToString();
				weaponDPSText.text = num5.ToString("0") + "<size=8> DPS";
			}
			else
			{
				weaponDamage.SetActive(value: false);
			}
		}
		else
		{
			weaponDamage.SetActive(value: false);
		}
		if ((bool)item.placeable && (bool)item.placeable.tileObjectGrowthStages && item.placeable.tileObjectGrowthStages.isPlant)
		{
			if (item.placeable.tileObjectGrowthStages.needsTilledSoil)
			{
				farmingDetails.SetActive(value: true);
				string text = "";
				if (item.placeable.tileObjectGrowthStages.growsInSummer && item.placeable.tileObjectGrowthStages.growsInWinter && item.placeable.tileObjectGrowthStages.growsInSpring && item.placeable.tileObjectGrowthStages.growsInAutum)
				{
					text = ConversationGenerator.generate.GetLocStringByTag("Time/all year");
				}
				else
				{
					if (item.placeable.tileObjectGrowthStages.growsInSummer)
					{
						text += RealWorldTimeLight.time.getSeasonName(0);
					}
					if (item.placeable.tileObjectGrowthStages.growsInAutum)
					{
						if (text != "")
						{
							text += " & ";
						}
						text += RealWorldTimeLight.time.getSeasonName(1);
					}
					if (item.placeable.tileObjectGrowthStages.growsInWinter)
					{
						if (text != "")
						{
							text += " & ";
						}
						text += RealWorldTimeLight.time.getSeasonName(2);
					}
					if (item.placeable.tileObjectGrowthStages.growsInSpring)
					{
						if (text != "")
						{
							text += " & ";
						}
						text += RealWorldTimeLight.time.getSeasonName(3);
					}
				}
				farmingDetailText.text = text;
				farmingLengthText.text = item.placeable.tileObjectGrowthStages.objectStages.Length + " " + ConversationGenerator.generate.GetDescriptionDetails("Title_days");
			}
			else if (item.burriedPlaceable)
			{
				farmingDetails.SetActive(value: true);
				farmingDetailText.text = ConversationGenerator.generate.GetDescriptionDetails("Title_Bury");
				farmingLengthText.text = item.placeable.tileObjectGrowthStages.objectStages.Length + " " + ConversationGenerator.generate.GetDescriptionDetails("Title_days");
			}
			else
			{
				farmingDetails.SetActive(value: false);
			}
		}
		else
		{
			farmingDetails.SetActive(value: false);
		}
		if ((bool)item.consumeable)
		{
			consumeableWindow.SetActive(value: true);
			hungerTimerText.text = GetHungerTimeText(item.consumeable);
			if (item.consumeable.healthGain != 0)
			{
				health.SetActive(value: true);
				healthText.text = item.consumeable.healthGain + ConversationGenerator.generate.GetDescriptionDetails("FoodTimerPerTick");
			}
			else
			{
				health.SetActive(value: false);
			}
			if (item.consumeable.staminaGain != 0f)
			{
				stamina.SetActive(value: true);
				staminaText.text = item.consumeable.staminaGain + ConversationGenerator.generate.GetDescriptionDetails("FoodTimerSeconds");
			}
			else
			{
				stamina.SetActive(value: false);
			}
			if (item.consumeable.givesTempPoints)
			{
				if (item.consumeable.tempHealthGain != 0)
				{
					healthPlus.SetActive(value: true);
					healthPlusText.text = item.consumeable.tempHealthGain.ToString();
				}
				else
				{
					healthPlus.SetActive(value: false);
				}
				if (item.consumeable.tempStaminaGain != 0f)
				{
					staminaPlus.SetActive(value: true);
					staminaPlusText.text = item.consumeable.tempStaminaGain.ToString();
				}
				else
				{
					staminaPlus.SetActive(value: false);
				}
			}
			else
			{
				healthPlus.SetActive(value: false);
				staminaPlus.SetActive(value: false);
			}
			if (item.consumeable.myBuffs.Length != 0)
			{
				buffwindow.SetActive(value: true);
				for (int i = 0; i < buffObjects.Length; i++)
				{
					buffObjects[i].SetActive(value: false);
				}
				for (int j = 0; j < item.consumeable.myBuffs.Length; j++)
				{
					buffObjects[(int)item.consumeable.myBuffs[j].myType].SetActive(value: true);
					buffLevel[(int)item.consumeable.myBuffs[j].myType].enabled = item.consumeable.myBuffs[j].myLevel > 1;
					if (item.consumeable.myBuffs[j].myLevel == 2)
					{
						buffLevel[(int)item.consumeable.myBuffs[j].myType].sprite = buffLevel2;
					}
					else
					{
						buffLevel[(int)item.consumeable.myBuffs[j].myType].sprite = buffLevel3;
					}
					if (item.consumeable.myBuffs[j].seconds > 60)
					{
						buffSeconds[(int)item.consumeable.myBuffs[j].myType].text = Mathf.RoundToInt(item.consumeable.myBuffs[j].seconds / 60) + ConversationGenerator.generate.GetDescriptionDetails("FoodTimerMins");
					}
					else
					{
						buffSeconds[(int)item.consumeable.myBuffs[j].myType].text = item.consumeable.myBuffs[j].seconds + ConversationGenerator.generate.GetDescriptionDetails("FoodTimerSeconds");
					}
				}
			}
			else
			{
				buffwindow.SetActive(value: false);
			}
		}
		else
		{
			consumeableWindow.SetActive(value: false);
			buffwindow.SetActive(value: false);
		}
		windmillCompatible.SetActive((bool)item.placeable && (bool)item.placeable.tileObjectItemChanger && item.placeable.tileObjectItemChanger.useWindMill);
		solarCompatible.SetActive((bool)item.placeable && (bool)item.placeable.tileObjectItemChanger && item.placeable.tileObjectItemChanger.useSolar);
		if (((bool)item.placeable && (bool)item.placeable.sprinklerTile) || ((bool)item.placeable && item.placeable.tileObjectId == 16) || ((bool)item.placeable && item.placeable.tileObjectId == 703) || ((bool)item.placeable && item.placeable.tileObjectId == 36) || ((bool)item.placeable && item.placeable.tileObjectId == 773) || ((bool)item.placeable && item.placeable.tileObjectId == 852))
		{
			reachTiles.SetActive(value: true);
			if (item.placeable.tileObjectId == 16)
			{
				reachTileText.text = string.Format(ConversationGenerator.generate.GetDescriptionDetails("SpeedUpProductionTileRange"), "14");
			}
			else if (item.placeable.tileObjectId == 703)
			{
				reachTileText.text = string.Format(ConversationGenerator.generate.GetDescriptionDetails("SpeedUpProductionTileRange"), "8");
			}
			else if (item.placeable.tileObjectId == 36 || item.placeable.tileObjectId == 773)
			{
				reachTileText.text = ConversationGenerator.generate.GetDescriptionDetails("PullFromStorage");
			}
			else if (item.placeable.tileObjectId == 852)
			{
				reachTileText.text = ConversationGenerator.generate.GetDescriptionDetails("SortToStorage");
			}
			else if (!item.placeable.sprinklerTile.isTank && !item.placeable.sprinklerTile.isSilo)
			{
				reachTileText.text = string.Format(ConversationGenerator.generate.GetDescriptionDetails("WatersNumberOfTilesOut"), item.placeable.sprinklerTile.verticlSize) + "\n<color=red>" + ConversationGenerator.generate.GetDescriptionDetails("RequiresWaterTank") + "</color>";
			}
			else if (item.placeable.sprinklerTile.isTank)
			{
				reachTileText.text = string.Format(ConversationGenerator.generate.GetDescriptionDetails("WaterTankRange"), item.placeable.sprinklerTile.verticlSize);
			}
			else if (item.placeable.sprinklerTile.isSilo)
			{
				reachTileText.text = string.Format(ConversationGenerator.generate.GetDescriptionDetails("FillsAnimalFeedersRange"), item.placeable.sprinklerTile.verticlSize) + "\n<color=red>" + ConversationGenerator.generate.GetDescriptionDetails("RequiresAnimalFood") + "</color>";
			}
		}
		else
		{
			reachTiles.SetActive(value: false);
		}
		bridgeWindow.SetActive((bool)item.placeable && (bool)item.placeable.tileObjectBridge);
		if ((bool)item.placeable && (bool)item.placeable.tileObjectBridge)
		{
			bridgeWidthText.text = string.Format(ConversationGenerator.generate.GetDescriptionDetails("Number_Tiles_Wide"), item.placeable.GetXSize());
		}
	}

	private string GetHungerTimeText(Consumeable item)
	{
		if ((bool)item)
		{
			if (Mathf.RoundToInt((float)item.durationSeconds / 60f) > 1)
			{
				return Mathf.RoundToInt((float)item.durationSeconds / 60f) + ConversationGenerator.generate.GetDescriptionDetails("FoodTimerMins");
			}
			if ((float)item.durationSeconds >= 60f)
			{
				return 1 + ConversationGenerator.generate.GetDescriptionDetails("FoodTimerMins");
			}
			return item.durationSeconds + ConversationGenerator.generate.GetDescriptionDetails("FoodTimerSeconds");
		}
		return "";
	}
}
