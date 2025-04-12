using UnityEngine;

public class BulletinBoardShowNewMessage : MonoBehaviour
{
	public static BulletinBoardShowNewMessage showMessage;

	public GameObject newNotification;

	private void Awake()
	{
		showMessage = this;
	}

	private void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isBulletinBoard = this;
	}

	private void OnEnable()
	{
		WorldManager.Instance.changeDayEvent.AddListener(showIfNewMessage);
		BulletinBoard.board.closeBoardEvent.AddListener(showIfNewMessage);
		if ((bool)NetworkMapSharer.Instance && NetworkMapSharer.Instance.isServer && BulletinBoard.board.attachedPosts.Count == 0)
		{
			PostOnBoard item = new PostOnBoard(0, -1, BulletinBoard.BoardPostType.Announcement);
			BulletinBoard.board.attachedPosts.Add(item);
		}
		showIfNewMessage();
	}

	public void showIfNewMessage()
	{
		newNotification.SetActive(BulletinBoard.board.checkIfAnyUnread());
	}

	private void OnDisable()
	{
		WorldManager.Instance.changeDayEvent.RemoveListener(showIfNewMessage);
		BulletinBoard.board.closeBoardEvent.RemoveListener(showIfNewMessage);
	}
}
