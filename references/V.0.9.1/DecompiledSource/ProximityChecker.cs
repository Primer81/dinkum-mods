using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ProximityChecker : NetworkVisibility
{
	public List<NetworkConnection> playersObserving = new List<NetworkConnection>();

	private NetworkIdentity networkIdentity;

	private bool needsUpdating;

	public NetworkTransform netTransToHide;

	public Animator animatorToDisable;

	private bool hostCanSee = true;

	private bool isAlwaysVisible;

	private WaitForSeconds waitTime = new WaitForSeconds(1f);

	private void Awake()
	{
		networkIdentity = GetComponent<NetworkIdentity>();
	}

	public override void OnStartServer()
	{
		waitTime = new WaitForSeconds(Random.Range(0.75f, 1.05f));
		if (base.isActiveAndEnabled)
		{
			StartCoroutine(checkProximity());
		}
	}

	public override void OnStopServer()
	{
		StopAllCoroutines();
		playersObserving.Clear();
		NetworkServer.RebuildObservers(networkIdentity, initialize: false);
		needsUpdating = false;
	}

	public override void OnRebuildObservers(HashSet<NetworkConnection> observers, bool initial)
	{
		foreach (NetworkConnection item in playersObserving)
		{
			observers.Add(item);
		}
	}

	public override bool OnCheckObserver(NetworkConnection newObserver)
	{
		return false;
	}

	public void updateProximityToPlayers()
	{
		playersObserving.Clear();
		needsUpdating = true;
		hostCanSee = false;
		for (int i = 0; i < NetworkNavMesh.nav.charsConnected.Count; i++)
		{
			if (NetworkNavMesh.nav.charNetConn[i] != null && (isAlwaysVisible || Vector3.Distance(new Vector3(NetworkNavMesh.nav.charsConnected[i].position.x, 0f, NetworkNavMesh.nav.charsConnected[i].position.z), new Vector3(base.transform.position.x, 0f, base.transform.position.z)) < (float)NetworkNavMesh.nav.animalDistance))
			{
				if (i == 0)
				{
					hostCanSee = true;
				}
				playersObserving.Add(NetworkNavMesh.nav.charNetConn[i].connectionToClient);
			}
		}
	}

	public void AddPlayerObservingForOneFrame(NetworkConnection conn)
	{
		hostCanSee = true;
		if (!playersObserving.Contains(conn))
		{
			playersObserving.Add((NetworkConnectionToClient)conn);
			needsUpdating = true;
		}
		NetworkServer.RebuildObservers(networkIdentity, initialize: false);
	}

	private IEnumerator checkProximity()
	{
		while (true)
		{
			updateProximityToPlayers();
			if (needsUpdating)
			{
				NetworkServer.RebuildObservers(networkIdentity, initialize: false);
				needsUpdating = false;
			}
			yield return waitTime;
		}
	}

	public void SetAlwaysVisible(bool shouldAlwaysBeVisible)
	{
		isAlwaysVisible = shouldAlwaysBeVisible;
	}

	private void MirrorProcessed()
	{
	}
}
