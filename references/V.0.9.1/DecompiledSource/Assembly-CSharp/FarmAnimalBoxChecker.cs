using UnityEngine;

public class FarmAnimalBoxChecker : MonoBehaviour
{
	public LayerMask carryableLayer;

	public Transform moveToIfFound;

	public Vector3 size;

	private void OnEnable()
	{
		WorldManager.Instance.changeDayEvent.AddListener(endOfDayAnimalCheckAndMove);
	}

	private void OnDisable()
	{
		WorldManager.Instance.changeDayEvent.RemoveListener(endOfDayAnimalCheckAndMove);
	}

	public void endOfDayAnimalCheckAndMove()
	{
		if ((bool)moveToIfFound)
		{
			checkMyBox(moveBox: true);
		}
	}

	public bool checkIfAnimalIsInBuilding()
	{
		return checkMyBox();
	}

	public bool checkMyBox(bool moveBox = false)
	{
		Collider[] array = Physics.OverlapBox(base.transform.position, size, Quaternion.identity, carryableLayer);
		bool flag = false;
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].GetComponentInParent<AnimalCarryBox>())
			{
				continue;
			}
			flag = true;
			if (moveBox)
			{
				if ((bool)array[i].GetComponentInParent<PickUpAndCarry>())
				{
					array[i].transform.root.position = moveToIfFound.transform.position;
					array[i].GetComponentInParent<PickUpAndCarry>().MoveToNewDropPos(moveToIfFound.transform.position.y);
				}
			}
			else if (!moveBox && flag)
			{
				break;
			}
		}
		return flag;
	}
}
