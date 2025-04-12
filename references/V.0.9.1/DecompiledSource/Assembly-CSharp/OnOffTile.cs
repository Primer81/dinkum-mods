using System.Collections;
using UnityEngine;

public class OnOffTile : MonoBehaviour
{
	public Animator onOffAnim;

	public bool isGate;

	public bool isOpen;

	private bool firstRefresh = true;

	public ASound openSound;

	public ASound closeSound;

	public InventoryItem requiredToOpen;

	public DailyTaskGenerator.genericTaskType taskWhenUnlocked;

	private Coroutine animatorRoutine;

	public static WaitForSeconds gateWait = new WaitForSeconds(5f);

	public void setOnOff(int xPos, int yPos, bool changedByPlayer = false)
	{
		if (firstRefresh)
		{
			if (WorldManager.Instance.onTileStatusMap[xPos, yPos] == 0)
			{
				turnOffGateAnimAfterTime();
				onOffAnim.SetTrigger("StartClosed");
				isOpen = false;
			}
			else
			{
				turnOffGateAnimAfterTime();
				onOffAnim.SetTrigger("StartOpen");
				isOpen = true;
			}
			firstRefresh = false;
		}
		else if (WorldManager.Instance.onTileStatusMap[xPos, yPos] == 0)
		{
			if (isOpen)
			{
				turnOffGateAnimAfterTime();
				isOpen = false;
				onOffAnim.SetTrigger("Close");
				if (changedByPlayer && (bool)openSound)
				{
					SoundManager.Instance.playASoundAtPoint(openSound, base.transform.position);
				}
			}
		}
		else if (!isOpen)
		{
			turnOffGateAnimAfterTime();
			isOpen = true;
			onOffAnim.SetTrigger("Open");
			if (changedByPlayer && (bool)closeSound)
			{
				SoundManager.Instance.playASoundAtPoint(closeSound, base.transform.position);
			}
		}
		firstRefresh = false;
	}

	public void fakeOpen()
	{
		turnOffGateAnimAfterTime();
		onOffAnim.SetTrigger("StartOpen");
	}

	public void fakeClose()
	{
		turnOffGateAnimAfterTime();
		onOffAnim.SetTrigger("StartClosed");
	}

	public bool getIfOpen(int xPos, int yPos)
	{
		if (WorldManager.Instance.onTileStatusMap[xPos, yPos] == 0)
		{
			return false;
		}
		return true;
	}

	private void OnEnable()
	{
		firstRefresh = true;
	}

	public void turnOffGateAnimAfterTime()
	{
		if (animatorRoutine != null)
		{
			StopCoroutine(animatorRoutine);
		}
		animatorRoutine = StartCoroutine(turnOffGateAnimatorDelay());
	}

	private IEnumerator turnOffGateAnimatorDelay()
	{
		onOffAnim.updateMode = AnimatorUpdateMode.Normal;
		onOffAnim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		onOffAnim.enabled = true;
		yield return gateWait;
		onOffAnim.enabled = false;
		animatorRoutine = null;
	}
}
