using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoredFoodType : MonoBehaviour
{
	private int currentFoodTypeId = -1;

	private int secondsRemaining;

	public TextMeshProUGUI timerText;

	public Image foodIcon;

	private Consumeable currentFood;

	public void AddFood(InventoryItem foodItem)
	{
		foodIcon.sprite = foodItem.getSprite();
		currentFood = foodItem.consumeable;
		AddStatus();
		base.gameObject.SetActive(value: true);
		StatusManager.manage.StartCountDownFoodTimer(this);
		GetComponent<HoverToolTipOnButton>().hoveringText = foodItem.getInvItemName();
	}

	private void AddStatus()
	{
		StatusManager.manage.AddToFullness();
		if (currentFood.givesTempPoints)
		{
			StatusManager.manage.addTempPoints(currentFood.tempHealthGain, currentFood.tempStaminaGain);
		}
		StatusManager.manage.CalculateActiveFoodToRegen();
		secondsRemaining = currentFood.durationSeconds;
		SetTimeText();
		currentFood.giveBuffs(secondsRemaining);
	}

	private void RemoveStatus()
	{
		StatusManager.manage.RemoveFullness();
	}

	public bool CurrentlyEmpty()
	{
		return currentFood == null;
	}

	public void Tick()
	{
		secondsRemaining--;
		SetTimeText();
		if (secondsRemaining <= 0)
		{
			ClearFood();
		}
	}

	public void ClearFood()
	{
		if (!CurrentlyEmpty())
		{
			RemoveStatus();
			secondsRemaining = 0;
			base.gameObject.SetActive(value: false);
			currentFood = null;
			StatusManager.manage.AdjustExtraStaminaAndHealthToCurrentFood();
			StatusManager.manage.CalculateActiveFoodToRegen();
		}
	}

	private void SetTimeText()
	{
		if (Mathf.RoundToInt((float)secondsRemaining / 60f) > 1)
		{
			timerText.text = Mathf.RoundToInt((float)secondsRemaining / 60f) + ConversationGenerator.generate.GetDescriptionDetails("FoodTimerMins");
		}
		else if ((float)secondsRemaining >= 60f)
		{
			timerText.text = 1 + ConversationGenerator.generate.GetDescriptionDetails("FoodTimerMins");
		}
		else
		{
			timerText.text = secondsRemaining + ConversationGenerator.generate.GetDescriptionDetails("FoodTimerSeconds");
		}
	}

	public int GetTotalExtraHealthGivenFromThisFood()
	{
		if (currentFood == null)
		{
			return 0;
		}
		return currentFood.tempHealthGain;
	}

	public float GetTotalExtraStaminaGivenFromThisFood()
	{
		if (currentFood == null)
		{
			return 0f;
		}
		return currentFood.tempStaminaGain;
	}

	public int GetCurrentHealthTick()
	{
		if (currentFood == null)
		{
			return 0;
		}
		return currentFood.healthGain;
	}

	public float GetCurrentStaminaTick()
	{
		if (currentFood == null)
		{
			return 0f;
		}
		return currentFood.staminaGain;
	}
}
