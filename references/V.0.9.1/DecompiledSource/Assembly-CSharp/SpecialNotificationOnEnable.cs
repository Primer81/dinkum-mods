using UnityEngine;

public class SpecialNotificationOnEnable : MonoBehaviour
{
	public string message;

	private void OnEnable()
	{
		if (message.Contains("Tip_"))
		{
			NotificationManager.manage.createChatNotification(ConversationGenerator.generate.GetToolTip(message), specialTip: true);
		}
		else
		{
			NotificationManager.manage.createChatNotification(message, specialTip: true);
		}
	}
}
