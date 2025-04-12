using UnityEngine;

public class Instagrow : MonoBehaviour
{
	private CharMovement myChar;

	private void Start()
	{
		myChar = GetComponentInParent<CharMovement>();
	}

	public void doDamageNow()
	{
		if ((bool)myChar && myChar.isLocalPlayer)
		{
			Vector3 position = NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position;
			int num = Mathf.RoundToInt(position.x / 2f);
			int num2 = Mathf.RoundToInt(position.z / 2f);
			if (WorldManager.Instance.isPositionOnMap(num, num2) && !WorldManager.Instance.CheckTileClientLock(num, num2) && WorldManager.Instance.onTileMap[num, num2] >= 0 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[num, num2]].tileObjectGrowthStages && WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[num, num2]].tileObjectGrowthStages.needsTilledSoil && WorldManager.Instance.onTileStatusMap[num, num2] < WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[num, num2]].tileObjectGrowthStages.objectStages.Length - 1)
			{
				WorldManager.Instance.lockTileClient(num, num2);
				myChar.CmdUseInstaGrow(num, num2);
				Inventory.Instance.consumeItemInHand();
			}
		}
	}
}
