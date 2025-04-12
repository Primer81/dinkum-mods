using UnityEngine;

public class MailBoxShowsMail : MonoBehaviour
{
	public static MailBoxShowsMail showsMail;

	public GameObject hasMailBox;

	public GameObject noMailBox;

	public bool hasMail;

	private void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isMailBox = this;
	}

	private void OnEnable()
	{
		MailManager.manage.newMailEvent.AddListener(refresh);
		showsMail = this;
		refresh();
	}

	private void OnDisable()
	{
		MailManager.manage.newMailEvent.RemoveListener(refresh);
	}

	public void refresh()
	{
		if (MailManager.manage.mailWindowOpen)
		{
			hasMailBox.gameObject.SetActive(value: false);
		}
		else if (MailManager.manage.checkIfAnyUndreadLetters())
		{
			hasMailBox.SetActive(value: true);
			noMailBox.SetActive(value: false);
		}
		else
		{
			hasMailBox.SetActive(value: false);
			noMailBox.SetActive(value: true);
		}
	}
}
