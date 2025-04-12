using UnityEngine;

public class HideMenuButtonsWhenNoFile : MonoBehaviour
{
	public GameObject loadButton;

	public GameObject loadButtonDummy;

	public GameObject multiplayerErrorMessage;

	private void OnEnable()
	{
		if (SaveLoad.saveOrLoad.isASaveSlot())
		{
			loadButton.SetActive(value: true);
			multiplayerErrorMessage.SetActive(value: false);
			loadButtonDummy.SetActive(value: false);
		}
		else
		{
			loadButton.SetActive(value: false);
			multiplayerErrorMessage.SetActive(value: true);
			loadButtonDummy.SetActive(value: true);
		}
	}
}
