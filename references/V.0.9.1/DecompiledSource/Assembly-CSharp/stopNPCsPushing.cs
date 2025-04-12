using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class stopNPCsPushing : MonoBehaviour
{
	public NavMeshAgent myAgent;

	public NPCAI myAi;

	public GameObject myObsticles;

	public LayerMask npcLayer;

	public bool isStopped;

	public bool stoppedSelf;

	private stopNPCsPushing stoppingFor;

	private static WaitForSeconds checkWait = new WaitForSeconds(1f);

	private int checkCount;

	private void OnEnable()
	{
		if (NetworkMapSharer.Instance.isServer)
		{
			stoppingFor = null;
			isStopped = false;
			stoppedSelf = false;
			StartCoroutine(StopClumping());
		}
	}

	private IEnumerator newStopClumping()
	{
		while (true)
		{
			yield return null;
		}
	}

	private IEnumerator StopClumping()
	{
		while (true)
		{
			yield return checkWait;
			if (stoppedSelf)
			{
				continue;
			}
			RaycastHit hitInfo;
			if (isStopped)
			{
				checkCount--;
				if (checkCount <= 0 || myAi.doesTask.isScared || !stoppingFor || stoppingFor.isStopped)
				{
					startNPC();
				}
				else if (Physics.Raycast(base.transform.position + Vector3.up, base.transform.forward, out hitInfo, 2.5f, npcLayer))
				{
					if (hitInfo.transform.root != stoppingFor.transform)
					{
						startNPC();
					}
				}
				else
				{
					startNPC();
				}
			}
			else
			{
				if (!Physics.Raycast(base.transform.position + Vector3.up, base.transform.forward, out hitInfo, 2.5f, npcLayer))
				{
					continue;
				}
				stopNPCsPushing component = hitInfo.transform.GetComponent<stopNPCsPushing>();
				if (!component || component.isStopped)
				{
					continue;
				}
				if ((bool)myAi.doesTask && myAi.doesTask.hasTask && (bool)component.myAi.doesTask && !component.myAi.doesTask.hasTask)
				{
					if (component.isStopped)
					{
						if (!myAi.canStillReachTaskLocation(myAi.myAgent.destination))
						{
							component.askToMoveOutOfTheWay();
						}
					}
					else
					{
						component.stopNPC(this);
						recalculatePath();
					}
				}
				else if (!myAi.doesTask.hasTask && component.myAi.doesTask.hasTask)
				{
					stopNPC(component);
					component.recalculatePath();
				}
				else if (myAgent.hasPath && !component.myAgent.hasPath)
				{
					component.stopNPC(this);
					recalculatePath();
				}
				else if (!myAgent.hasPath && component.myAgent.hasPath)
				{
					stopNPC(component);
					component.recalculatePath();
				}
				else if (Random.Range(0, 1) == 1)
				{
					component.stopNPC(this);
					recalculatePath();
				}
				else
				{
					stopNPC(component);
					component.recalculatePath();
				}
			}
		}
	}

	public void stopNPC(stopNPCsPushing stoppedfor)
	{
		checkCount = Random.Range(7, 10);
		stoppingFor = stoppedfor;
		isStopped = true;
		if (myAgent.isOnNavMesh)
		{
			myAgent.isStopped = true;
		}
		myObsticles.SetActive(value: true);
	}

	public void startNPC()
	{
		stoppingFor = null;
		isStopped = false;
		if (myAgent.isOnNavMesh && myAgent.isStopped)
		{
			myAgent.isStopped = false;
		}
		myObsticles.SetActive(value: false);
	}

	public bool checkIfIHavePriority(NavMeshAgent toCompare)
	{
		if (myAgent.avoidancePriority < toCompare.avoidancePriority)
		{
			return true;
		}
		return false;
	}

	public void askToMoveOutOfTheWay()
	{
		startNPC();
		myAi.myAgent.SetDestination(base.transform.position + new Vector3(Random.Range(-4f, 4f), 0f, Random.Range(-4f, 4f)));
	}

	public void recalculatePath()
	{
		if (myAgent.isActiveAndEnabled && myAgent.isStopped && myAgent.isOnNavMesh)
		{
			Vector3 destination = myAgent.destination;
			myAgent.ResetPath();
			myAgent.SetDestination(destination);
		}
	}

	public void stopSelf()
	{
		stoppedSelf = true;
		stoppingFor = null;
		isStopped = true;
		if (myAgent.isOnNavMesh)
		{
			myAgent.isStopped = true;
		}
		myObsticles.SetActive(value: true);
	}

	public void startSelf()
	{
		stoppedSelf = false;
		stoppingFor = null;
		isStopped = false;
		if (myAgent.isStopped && myAgent.isOnNavMesh)
		{
			myAgent.isStopped = false;
		}
		myObsticles.SetActive(value: false);
	}
}
