using UnityEngine;

public class LevelTerrain : MonoBehaviour
{
	public CharInteract myCharInteract;

	public CharMovement myCharMovement;

	public Animator myAnim;

	public bool movesUp;

	private void Start()
	{
		myCharInteract = GetComponentInParent<CharInteract>();
		myCharMovement = GetComponentInParent<CharMovement>();
	}

	private void doLevelTerrain()
	{
		if (!myCharInteract || !myCharInteract.isLocalPlayer)
		{
			return;
		}
		int num;
		int num2;
		if (myCharInteract.isLocalPlayer)
		{
			num = (int)myCharInteract.selectedTile.x;
			num2 = (int)myCharInteract.selectedTile.y;
		}
		else
		{
			num = (int)myCharInteract.currentlyAttackingPos.x;
			num2 = (int)myCharInteract.currentlyAttackingPos.y;
		}
		if (myCharInteract.CheckIfCanDamage(new Vector2(num, num2)) && !WorldManager.Instance.CheckTileClientLock(num, num2))
		{
			if (movesUp && WorldManager.Instance.heightMap[num, num2] <= Mathf.RoundToInt(base.transform.root.position.y))
			{
				if ((bool)myCharMovement && myCharMovement.grounded)
				{
					myCharInteract.doDamage();
					WorldManager.Instance.lockTileClient(num, num2);
				}
			}
			else if (!movesUp && WorldManager.Instance.heightMap[num, num2] == Mathf.RoundToInt(base.transform.root.position.y + 1f))
			{
				if ((bool)myCharMovement && myCharMovement.grounded)
				{
					myCharInteract.doDamage();
					WorldManager.Instance.lockTileClient(num, num2);
				}
			}
			else if (WorldManager.Instance.heightMap[num, num2] > Mathf.RoundToInt(base.transform.root.position.y + 1f))
			{
				myAnim.SetTrigger("Clang");
			}
		}
		else if ((bool)myAnim)
		{
			myAnim.SetTrigger("Clang");
		}
	}
}
