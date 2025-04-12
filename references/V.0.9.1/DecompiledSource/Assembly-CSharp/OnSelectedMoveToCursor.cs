using UnityEngine;
using UnityEngine.EventSystems;

public class OnSelectedMoveToCursor : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	private RectTransform thisButton;

	private void Start()
	{
		thisButton = GetComponent<RectTransform>();
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (Inventory.Instance.snapCursorOn)
		{
			Inventory.Instance.currentlySelected = thisButton;
			Inventory.Instance.cursor.position = Inventory.Instance.currentlySelected.transform.position + new Vector3(Inventory.Instance.currentlySelected.sizeDelta.x / 2f - 2f, 0f, 0f);
		}
	}
}
