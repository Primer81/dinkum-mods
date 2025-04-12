using System.Collections;
using Mirror;
using UnityEngine;

public class AnimateSittingBug : NetworkBehaviour
{
	private static int walkAnimationName;

	private Animator myAnim;

	public void setAnimator(Animator anim)
	{
		walkAnimationName = Animator.StringToHash("WalkingSpeed");
		myAnim = anim;
		myAnim.SetFloat("Offset", Random.Range(0f, 1f));
		myAnim.SetFloat(walkAnimationName, 0f);
	}

	public override void OnStartServer()
	{
		StartCoroutine(DistanceCheck());
	}

	private IEnumerator DistanceCheck()
	{
		int xPos = Mathf.RoundToInt(base.transform.position.x / 2f);
		int yPos = Mathf.RoundToInt(base.transform.position.z / 2f);
		int onTileId = WorldManager.Instance.onTileMap[xPos, yPos];
		WaitForSeconds checkTimer = new WaitForSeconds(0.1f);
		while (true)
		{
			yield return checkTimer;
			if (onTileId != WorldManager.Instance.onTileMap[xPos, yPos])
			{
				NetworkMapSharer.Instance.RpcReleaseBugFromSitting(GetComponent<BugTypes>().getBugTypeId(), base.transform.position);
				NetworkServer.Destroy(base.gameObject);
			}
			if (!NetworkNavMesh.nav.doesPositionHaveNavChunk((int)(base.transform.position.x / 2f), (int)(base.transform.position.z / 2f)) && !checkIfThereIsStillNavmeshThere())
			{
				AnimalManager.manage.SaveSittingBugToMap(base.transform.position);
				NetworkServer.Destroy(base.gameObject);
			}
		}
	}

	public override void OnStopServer()
	{
		NetworkNavMesh.nav.RemoveBugInPos(GetComponent<BugTypes>());
	}

	public bool checkIfThereIsStillNavmeshThere()
	{
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
