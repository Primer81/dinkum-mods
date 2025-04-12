using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class TeleSignal : NetworkBehaviour
{
	public static TeleSignal currentSignal;

	private void Awake()
	{
		currentSignal = this;
	}

	private void OnDestroy()
	{
		currentSignal = null;
	}

	public override void OnStartServer()
	{
		RpcMakeClientRequestChunkOnSignalMove(Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f));
	}

	public override void OnStartClient()
	{
		NetworkMapSharer.Instance.RequestChunkAtXPosAndYPos(Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f));
	}

	public void UpdatePosition(Vector3 newPosition)
	{
		base.transform.position = newPosition;
		RpcMakeClientRequestChunkOnSignalMove(Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f));
	}

	[ClientRpc]
	private void RpcMakeClientRequestChunkOnSignalMove(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(TeleSignal), "RpcMakeClientRequestChunkOnSignalMove", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcMakeClientRequestChunkOnSignalMove(int xPos, int yPos)
	{
		NetworkMapSharer.Instance.RequestChunkAtXPosAndYPos(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcMakeClientRequestChunkOnSignalMove(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMakeClientRequestChunkOnSignalMove called on server.");
		}
		else
		{
			((TeleSignal)obj).UserCode_RpcMakeClientRequestChunkOnSignalMove(reader.ReadInt(), reader.ReadInt());
		}
	}

	static TeleSignal()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(TeleSignal), "RpcMakeClientRequestChunkOnSignalMove", InvokeUserCode_RpcMakeClientRequestChunkOnSignalMove);
	}
}
