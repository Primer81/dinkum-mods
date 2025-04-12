using System.Collections;
using UnityEngine;

public class UseBook : MonoBehaviour
{
	public bool isBugBook;

	public bool isFishBook;

	public bool isPlantBook;

	public bool isJournal;

	public bool isMachineManual;

	private bool plantBookOpen;

	public NameTag myNameTag;

	public ASound openBookSound;

	public ASound closeBookSound;

	private bool bookOpen;

	private CharMovement myChar;

	private void Start()
	{
		if (isJournal)
		{
			GetComponent<Animator>().SetBool("IsJournal", value: true);
		}
		if (isPlantBook)
		{
			StartCoroutine(plantBookRoutine());
		}
		myChar = base.transform.GetComponentInParent<CharMovement>();
	}

	public void openBook()
	{
		if (!bookOpen)
		{
			bookOpen = true;
			SoundManager.Instance.playASoundAtPoint(openBookSound, base.transform.position);
		}
		if (!isJournal && (bool)myChar && myChar.isLocalPlayer)
		{
			if (isMachineManual)
			{
				BookWindow.book.openBook();
			}
			if (isPlantBook)
			{
				plantBookOpen = true;
			}
			if (isBugBook && !AnimalManager.manage.bugBookOpen)
			{
				AnimalManager.manage.bugBookOpen = true;
				AnimalManager.manage.lookAtBugBook.Invoke();
			}
			if (isFishBook && !AnimalManager.manage.fishBookOpen)
			{
				AnimalManager.manage.fishBookOpen = true;
				AnimalManager.manage.lookAtFishBook.Invoke();
			}
		}
	}

	public void closeBook()
	{
		if (bookOpen)
		{
			bookOpen = false;
			SoundManager.Instance.playASoundAtPoint(closeBookSound, base.transform.position);
		}
		if (!isJournal && (bool)myChar && myChar.isLocalPlayer)
		{
			if (isMachineManual)
			{
				BookWindow.book.closeBook();
			}
			if (isPlantBook)
			{
				plantBookOpen = false;
				BookWindow.book.closeBook();
			}
			if (isBugBook && AnimalManager.manage.bugBookOpen)
			{
				AnimalManager.manage.bugBookOpen = false;
				AnimalManager.manage.lookAtBugBook.Invoke();
			}
			if (isFishBook && AnimalManager.manage.fishBookOpen)
			{
				AnimalManager.manage.fishBookOpen = false;
				AnimalManager.manage.lookAtFishBook.Invoke();
			}
		}
	}

	private void OnDestroy()
	{
		closeBook();
	}

	private IEnumerator plantBookRoutine()
	{
		int lastXpos = -1;
		int lastYpos = -1;
		while (true)
		{
			if (plantBookOpen)
			{
				int num = Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position.x / 2f);
				int num2 = Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position.z / 2f);
				if (lastXpos != num || lastYpos != num2)
				{
					if (WorldManager.Instance.onTileMap[num, num2] >= 0 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[num, num2]].tileObjectGrowthStages && WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[num, num2]].tileObjectGrowthStages.isPlant && !WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[num, num2]].tileObjectGrowthStages.normalPickUp)
					{
						TileObjectGrowthStages tileObjectGrowthStages = WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[num, num2]].tileObjectGrowthStages;
						if ((bool)tileObjectGrowthStages.steamsOutInto && (bool)tileObjectGrowthStages.steamsOutInto.tileObjectGrowthStages && (bool)tileObjectGrowthStages.steamsOutInto.tileObjectGrowthStages.harvestDrop)
						{
							BookWindow.book.objectTitle.text = string.Format(ConversationGenerator.generate.GetToolTip("Tip_ItemNamePlant"), tileObjectGrowthStages.steamsOutInto.tileObjectGrowthStages.harvestDrop.getInvItemName());
							if (tileObjectGrowthStages.objectStages.Length - WorldManager.Instance.onTileStatusMap[num, num2] - 1 == 0)
							{
								BookWindow.book.openPlantBook(ConversationGenerator.generate.GetToolTip("Tip_ReadyForOffshoots"));
							}
							else
							{
								BookWindow.book.openPlantBook(string.Format(ConversationGenerator.generate.GetToolTip("Tip_MatureInNumberDays"), tileObjectGrowthStages.objectStages.Length - WorldManager.Instance.onTileStatusMap[num, num2] - 1));
							}
						}
						else if ((bool)tileObjectGrowthStages.harvestDrop)
						{
							BookWindow.book.objectTitle.text = string.Format(ConversationGenerator.generate.GetToolTip("Tip_ItemNamePlant"), tileObjectGrowthStages.harvestDrop.getInvItemName());
							if (tileObjectGrowthStages.objectStages.Length - WorldManager.Instance.onTileStatusMap[num, num2] - 1 == 0)
							{
								BookWindow.book.openPlantBook(ConversationGenerator.generate.GetToolTip("Tip_ReadyforHarvest"));
							}
							else
							{
								BookWindow.book.openPlantBook(string.Format(ConversationGenerator.generate.GetToolTip("Tip_MatureInNumberDays"), tileObjectGrowthStages.objectStages.Length - WorldManager.Instance.onTileStatusMap[num, num2] - 1));
							}
						}
						else
						{
							BookWindow.book.objectTitle.text = string.Format(ConversationGenerator.generate.GetToolTip("Tip_ItemNamePlant"), "");
							if (tileObjectGrowthStages.objectStages.Length - WorldManager.Instance.onTileStatusMap[num, num2] - 1 == 0)
							{
								BookWindow.book.openPlantBook("");
							}
							else
							{
								BookWindow.book.openPlantBook(string.Format(ConversationGenerator.generate.GetToolTip("Tip_MatureInNumberDays"), tileObjectGrowthStages.objectStages.Length - WorldManager.Instance.onTileStatusMap[num, num2] - 1));
							}
						}
					}
					else
					{
						BookWindow.book.closeBook();
					}
				}
				yield return null;
			}
			else
			{
				BookWindow.book.closeBook();
				while (!plantBookOpen)
				{
					yield return null;
				}
			}
		}
	}
}
