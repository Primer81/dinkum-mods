using System;

[Serializable]
internal class RecipesUnlockedSave
{
	public int crafterLevel;

	public int crafterWorkingOnItemId = -1;

	public int currentPoints;

	public bool crafterCurrentlyWorking;

	public bool crafterHasBerkonium;

	public RecipeToUnlock[] recipesUnlocked;

	public void loadRecipes()
	{
		for (int i = 0; i < CharLevelManager.manage.recipes.Count; i++)
		{
			if (i < recipesUnlocked.Length)
			{
				CharLevelManager.manage.recipes[i] = recipesUnlocked[i];
			}
		}
	}

	public void SaveRecipes()
	{
		crafterLevel = CraftsmanManager.manage.currentLevel;
		currentPoints = CraftsmanManager.manage.currentPoints;
		crafterHasBerkonium = CraftsmanManager.manage.craftsmanHasBerkonium;
		crafterWorkingOnItemId = CraftsmanManager.manage.itemCurrentlyCrafting;
		crafterCurrentlyWorking = NetworkMapSharer.Instance.craftsmanWorking;
		recipesUnlocked = CharLevelManager.manage.recipes.ToArray();
	}
}
