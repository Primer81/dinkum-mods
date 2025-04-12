using System;

[Serializable]
public class RecipeToUnlock
{
	public bool unlocked;

	public int recipeId;

	public RecipeToUnlock()
	{
	}

	public RecipeToUnlock(int newRecipeId)
	{
		recipeId = newRecipeId;
	}

	public bool unlockedThroughOtherWay()
	{
		if (Inventory.Instance.allItems[recipeId].craftable.learnThroughQuest || Inventory.Instance.allItems[recipeId].craftable.learnThroughLevels || Inventory.Instance.allItems[recipeId].craftable.learnThroughLicence)
		{
			return true;
		}
		return false;
	}

	public void unlockRecipe()
	{
		unlocked = true;
	}

	public bool isUnlocked()
	{
		return unlocked;
	}

	public Recipe.CraftingCatagory getCatagory()
	{
		return Inventory.Instance.allItems[recipeId].craftable.catagory;
	}

	public int unlockTier()
	{
		return Inventory.Instance.allItems[recipeId].craftable.tierLevel;
	}
}
