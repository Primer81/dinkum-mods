using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class NetworkBall : NetworkBehaviour
{
	public Rigidbody myRig;

	private WaitForFixedUpdate fixedWait;

	private NetworkIdentity networkIdentity;

	public float kickMultiplier = 2f;

	public ASound ballKick;

	public ASound ballBounce;

	public float bounceSoundThreshold = 2f;

	public float waterHeight = -0.6f;

	private bool landedInWater;

	private bool canMakeBounceSound = true;

	private void Start()
	{
		networkIdentity = GetComponent<NetworkIdentity>();
	}

	public override void OnStartServer()
	{
		WorldManager.Instance.changeDayEvent.AddListener(PopBall);
		NetworkMapSharer.Instance.onChangeMaps.AddListener(PopBall);
	}

	public override void OnStartClient()
	{
		if (base.hasAuthority)
		{
			StartAuthSetUp();
		}
	}

	public override void OnStartAuthority()
	{
		StartAuthSetUp();
	}

	public override void OnStopAuthority()
	{
		myRig.isKinematic = true;
		myRig.useGravity = true;
	}

	private void StartAuthSetUp()
	{
		myRig.isKinematic = false;
		myRig.useGravity = true;
		StartCoroutine(CheckIfPlayerWithAuthorityIsClose());
	}

	private void FixedUpdate()
	{
		if (WorldManager.Instance.isPositionInWater(base.transform.position) && base.transform.position.y < waterHeight)
		{
			if (base.hasAuthority)
			{
				float num = Mathf.Abs(base.transform.position.y - waterHeight);
				myRig.velocity = new Vector3(myRig.velocity.x / 1.025f, num / 5f, myRig.velocity.z / 1.025f);
				myRig.useGravity = false;
			}
			if (!landedInWater)
			{
				ParticleManager.manage.bigSplash(base.transform);
				SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.waterSplash, base.transform.position);
				landedInWater = true;
			}
		}
		else
		{
			if (landedInWater)
			{
				ParticleManager.manage.bigSplash(base.transform);
				SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.waterSplash, base.transform.position);
				landedInWater = false;
			}
			if (base.hasAuthority)
			{
				myRig.useGravity = true;
			}
		}
	}

	private IEnumerator CheckIfPlayerWithAuthorityIsClose()
	{
		while (true)
		{
			Vector3 position = NetworkMapSharer.Instance.localChar.transform.position;
			position.y = 0f;
			Vector3 position2 = base.transform.position;
			position2.y = 0f;
			if (Vector3.Distance(position, position2) > 80f)
			{
				break;
			}
			yield return fixedWait;
		}
		myRig.isKinematic = true;
		CmdFindPlayerCloseAndGiveAuthority();
	}

	public NetworkIdentity FindPlayerCloseEnoughForAuthority()
	{
		Vector3 position = base.transform.position;
		position.y = 0f;
		for (int i = 0; i < NetworkNavMesh.nav.charsConnected.Count; i++)
		{
			Vector3 position2 = NetworkNavMesh.nav.charsConnected[i].position;
			position2.y = 0f;
			if (Vector3.Distance(position2, position) < 80f)
			{
				return NetworkNavMesh.nav.charNetConn[i];
			}
		}
		return null;
	}

	[Command]
	public void CmdFindPlayerCloseAndGiveAuthority()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(NetworkBall), "CmdFindPlayerCloseAndGiveAuthority", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void OnTriggerEnter(Collider other)
	{
		CharMovement componentInParent = other.GetComponentInParent<CharMovement>();
		other.GetComponentInParent<Vehicle>();
		if ((bool)componentInParent && componentInParent.isLocalPlayer)
		{
			MonoBehaviour.print("Sending kick");
			componentInParent.KickABall(base.netId);
		}
	}

	[ClientRpc]
	public void RpcKickInDirection(Vector3 dir, float strength)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(dir);
		writer.WriteFloat(strength);
		SendRPCInternal(typeof(NetworkBall), "RpcKickInDirection", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator CanMakeBounceSoundTimer()
	{
		canMakeBounceSound = false;
		yield return new WaitForSeconds(0.1f);
		canMakeBounceSound = true;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (canMakeBounceSound && collision.relativeVelocity.magnitude > bounceSoundThreshold)
		{
			StartCoroutine(CanMakeBounceSoundTimer());
			SoundManager.Instance.playASoundAtPoint(ballBounce, base.transform.position);
		}
	}

	private void PopBall()
	{
		MonoBehaviour.print("Popping ball here");
		GetComponent<Damageable>().attackAndDoDamage(100, null);
	}

	private void OnDestroy()
	{
		WorldManager.Instance.changeDayEvent.RemoveListener(PopBall);
		NetworkMapSharer.Instance.onChangeMaps.RemoveListener(PopBall);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdFindPlayerCloseAndGiveAuthority()
	{
		if (this.networkIdentity.connectionToClient != null)
		{
			this.networkIdentity.RemoveClientAuthority();
		}
		NetworkIdentity networkIdentity = FindPlayerCloseEnoughForAuthority();
		if (networkIdentity != null)
		{
			this.networkIdentity.AssignClientAuthority(networkIdentity.connectionToClient);
		}
		else
		{
			PopBall();
		}
	}

	protected static void InvokeUserCode_CmdFindPlayerCloseAndGiveAuthority(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdFindPlayerCloseAndGiveAuthority called on client.");
		}
		else
		{
			((NetworkBall)obj).UserCode_CmdFindPlayerCloseAndGiveAuthority();
		}
	}

	protected void UserCode_RpcKickInDirection(Vector3 dir, float strength)
	{
		if (base.hasAuthority)
		{
			myRig.AddForce((dir + Vector3.up).normalized * (strength * kickMultiplier), ForceMode.Impulse);
		}
		SoundManager.Instance.playASoundAtPoint(ballKick, base.transform.position);
	}

	protected static void InvokeUserCode_RpcKickInDirection(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcKickInDirection called on server.");
		}
		else
		{
			((NetworkBall)obj).UserCode_RpcKickInDirection(reader.ReadVector3(), reader.ReadFloat());
		}
	}

	static NetworkBall()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkBall), "CmdFindPlayerCloseAndGiveAuthority", InvokeUserCode_CmdFindPlayerCloseAndGiveAuthority, requiresAuthority: true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkBall), "RpcKickInDirection", InvokeUserCode_RpcKickInDirection);
	}
}
