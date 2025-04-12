using UnityEngine;

public class Recipe : MonoBehaviour
{
	public enum CraftingCatagory
	{
		None,
		All,
		Tools,
		Decorations,
		Light,
		Gardening,
		House,
		Charm,
		Misc
	}

	public enum SubCatagory
	{
		None,
		Tools,
		Weapon,
		Path,
		Fence,
		Gate,
		Bridge,
		Decoration
	}

	public InventoryItem[] itemsInRecipe;

	public int[] stackOfItemsInRecipe;

	public int recipeGiveThisAmount = 1;

	public bool buildOnce;

	public bool isDeed;

	public CraftingManager.CraftingMenuType workPlaceConditions;

	public CraftingCatagory catagory;

	public SubCatagory subCatagory;

	public int tierLevel;

	public bool learnThroughQuest;

	public Recipe[] altRecipes;

	[Header("Level Unlock -------")]
	public bool learnThroughLevels;

	public CharLevelManager.SkillTypes skill;

	public int levelSkillLearnt;

	[Header("Licence Unlock -------")]
	public bool learnThroughLicence;

	public LicenceManager.LicenceTypes licenceType;

	public bool showInRecipeOverflow;

	public int licenceLevelLearnt;

	[Header("Crafter Unlock Level-------")]
	public int crafterLevelLearnt;

	public bool requiredBerkonium;

	[Header("Crafter Unlock Level-------")]
	public DailyTaskGenerator.genericTaskType completeTaskOnCraft;

	public bool meetsRequirement(int skillNo, int levelNo)
	{
		if (learnThroughLevels && skillNo == (int)skill && levelNo >= levelSkillLearnt)
		{
			return true;
		}
		return false;
	}

	public bool checkIfMeetsLicenceRequirement(LicenceManager.LicenceTypes checkLicenceType, int levelNo)
	{
		if (learnThroughLicence && checkLicenceType == licenceType && levelNo >= licenceLevelLearnt)
		{
			return true;
		}
		return false;
	}
}
