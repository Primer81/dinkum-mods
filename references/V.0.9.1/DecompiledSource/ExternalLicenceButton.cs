using UnityEngine;

public class ExternalLicenceButton : MonoBehaviour
{
	public void PressLicenceButton()
	{
		if (!string.IsNullOrEmpty(ConversationGenerator.generate.GetMenuText("LicenceButton_Link")))
		{
			Application.OpenURL(ConversationGenerator.generate.GetMenuText("LicenceButton_Link"));
		}
	}
}
