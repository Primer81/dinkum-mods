using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BulletinBoardTaskButton : MonoBehaviour
{
	public TextMeshProUGUI taskTitleText;

	public TextMeshProUGUI nameText;

	public GameObject completedButton;

	public GameObject expiredButton;

	public GameObject acceptedButton;

	public GameObject baseButton;

	public GameObject newButton;

	public Image border;

	private int myPostId;

	public void attachToPost(int postId)
	{
		myPostId = postId;
		if (postId < 0)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		taskTitleText.text = BulletinBoard.board.attachedPosts[myPostId].getTitleText(postId);
		nameText.text = BulletinBoard.board.attachedPosts[myPostId].getPostedByName();
		if (BulletinBoard.board.attachedPosts[myPostId].postedByNpcId > 0)
		{
			border.color = NPCManager.manage.NPCDetails[BulletinBoard.board.attachedPosts[myPostId].postedByNpcId].npcColor;
		}
		else
		{
			border.color = Color.grey;
		}
		completedButton.SetActive(value: false);
		expiredButton.SetActive(value: false);
		acceptedButton.SetActive(value: false);
		newButton.SetActive(value: false);
		baseButton.SetActive(value: false);
		if (BulletinBoard.board.attachedPosts[myPostId].checkIfExpired())
		{
			expiredButton.SetActive(value: true);
		}
		else if (!BulletinBoard.board.attachedPosts[myPostId].hasBeenRead)
		{
			newButton.SetActive(value: true);
		}
		else if (BulletinBoard.board.attachedPosts[myPostId].completed)
		{
			completedButton.SetActive(value: true);
		}
		else if (BulletinBoard.board.attachedPosts[myPostId].checkIfAccepted())
		{
			acceptedButton.SetActive(value: true);
		}
		else
		{
			baseButton.SetActive(value: true);
		}
	}

	public void pressButton()
	{
		BulletinBoard.board.setSelectedSlotAndShow(myPostId);
	}
}
