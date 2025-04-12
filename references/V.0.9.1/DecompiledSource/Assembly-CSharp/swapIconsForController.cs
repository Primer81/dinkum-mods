using UnityEngine;
using UnityEngine.UI;

public class swapIconsForController : MonoBehaviour
{
	public Sprite controllerSprite;

	public Sprite keyboardSprite;

	public Image toBeReplaced;

	public GameObject controllerObject;

	public GameObject keyboardObject;

	private void Start()
	{
		setCorrectSprite();
		Inventory.Instance.changeControlsEvent.AddListener(setCorrectSprite);
	}

	private void OnEnable()
	{
		setCorrectSprite();
	}

	public void setCorrectSprite()
	{
		if (Inventory.Instance.usingMouse)
		{
			toBeReplaced.sprite = keyboardSprite;
			if (!controllerObject)
			{
			}
		}
		else
		{
			toBeReplaced.sprite = controllerSprite;
		}
		if ((bool)controllerObject)
		{
			controllerObject.SetActive(!Inventory.Instance.usingMouse);
			keyboardObject.SetActive(Inventory.Instance.usingMouse);
		}
	}

	private void OnDestroy()
	{
		Inventory.Instance.changeControlsEvent.RemoveListener(setCorrectSprite);
	}
}
