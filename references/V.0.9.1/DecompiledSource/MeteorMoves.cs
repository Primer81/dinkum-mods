using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class MeteorMoves : NetworkBehaviour
{
	public ParticleSystemRenderer farPartRen;

	public ParticleSystemRenderer closePartRen;

	public ParticleSystemRenderer closeHitGroundPartRen;

	public ParticleSystemRenderer farHitGroundPartRen;

	public GameObject fakeRock;

	public bool resetPos;

	private bool grounded;

	public Light groundLight;

	private PickUpAndCarry myCarry;

	public ASound meteorHitsClose;

	public ASound meteorHitsFar;

	private Vector3 checkClosePosition;

	public AudioSource meteorSound;

	private Vector3 lastPos;

	public override void OnStartServer()
	{
		SetRandomPosition();
		StartCoroutine(HandleFallingMeteor());
	}

	private IEnumerator HandleFallingMeteor()
	{
		int currentDay = WorldManager.Instance.day;
		while (!grounded)
		{
			yield return null;
			if (base.transform.position.y <= (float)WorldManager.Instance.heightMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)])
			{
				grounded = true;
				RPCPlayLandedExplosion();
				base.transform.position = new Vector3(base.transform.position.x, WorldManager.Instance.heightMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)], base.transform.position.z);
				myCarry = NetworkMapSharer.Instance.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[14], base.transform.position);
				checkClosePosition = myCarry.transform.position;
			}
			else
			{
				base.transform.position += base.transform.forward * 15f * Time.deltaTime;
				if (WorldManager.Instance.day != currentDay)
				{
					break;
				}
			}
		}
		if (WorldManager.Instance.day == currentDay)
		{
			bool carryClose = true;
			while (carryClose)
			{
				yield return null;
				if ((bool)myCarry)
				{
					if (myCarry.transform == null || Vector3.Distance(myCarry.transform.position, checkClosePosition) >= 0.5f)
					{
						break;
					}
					if (WorldManager.Instance.day != currentDay)
					{
						NetworkServer.Destroy(myCarry.gameObject);
						carryClose = false;
					}
				}
				else
				{
					carryClose = false;
				}
			}
			RpcFadeOutLight();
		}
		StartCoroutine(DestroyAfterWait());
	}

	private IEnumerator DestroyAfterWait()
	{
		yield return new WaitForSeconds(5f);
		NetworkServer.Destroy(base.gameObject);
	}

	private void Update()
	{
		meteorSound.volume = 0.4f * SoundManager.Instance.GetGlobalSoundVolume();
		if (Vector3.Distance(CameraController.control.transform.position, base.transform.position) <= 100f)
		{
			farPartRen.enabled = true;
			closePartRen.enabled = true;
			closeHitGroundPartRen.enabled = true;
			farHitGroundPartRen.enabled = false;
		}
		else
		{
			farPartRen.enabled = true;
			closePartRen.enabled = true;
			closeHitGroundPartRen.enabled = false;
			farHitGroundPartRen.enabled = true;
		}
		if (Vector3.Distance(lastPos, base.transform.position) > 25f)
		{
			ResetTrail();
		}
		lastPos = base.transform.position;
	}

	[ClientRpc]
	private void RpcFadeOutLight()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(MeteorMoves), "RpcFadeOutLight", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator FadeOutLight()
	{
		float timer = 0f;
		while (timer < 1f)
		{
			groundLight.intensity = Mathf.Lerp(3f, 0f, timer);
			groundLight.range = Mathf.Lerp(15f, 0f, timer);
			timer += Time.deltaTime / 2f;
			yield return null;
		}
	}

	[ClientRpc]
	public void RPCPlayLandedExplosion()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(MeteorMoves), "RPCPlayLandedExplosion", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private void SetRandomPosition()
	{
		bool flag = false;
		int num = 4000;
		int num2 = Random.Range(200, 800);
		int num3 = Random.Range(200, 800);
		while (!flag)
		{
			if (tileIsEmptyOrHasGrass(num2, num3) && tileIsEmptyOrHasGrass(num2 + 1, num3) && tileIsEmptyOrHasGrass(num2, num3 + 1) && tileIsEmptyOrHasGrass(num2 + 1, num3 + 1))
			{
				flag = true;
			}
			else
			{
				num2 = Random.Range(200, 800);
				num3 = Random.Range(200, 800);
				num--;
			}
			if (num <= 0)
			{
				Debug.LogError("Tried location for meteor 4000 times");
				break;
			}
		}
		if (flag)
		{
			Vector3 worldPosition = new Vector3((float)num2 * 2f, WorldManager.Instance.heightMap[num2, num3], (float)num3 * 2f);
			base.transform.position = new Vector3(Random.Range(200, 1800), 200f, Random.Range(200, 1800));
			base.transform.LookAt(worldPosition);
		}
		else
		{
			Debug.LogError("No location found for meteor, so it was destroyed");
			NetworkServer.Destroy(base.gameObject);
		}
	}

	public bool tileIsEmptyOrHasGrass(int xPos, int yPos)
	{
		if (WorldManager.Instance.waterMap[xPos, yPos])
		{
			return false;
		}
		if (WorldManager.Instance.onTileMap[xPos, yPos] == -1)
		{
			return true;
		}
		if (WorldManager.Instance.onTileMap[xPos, yPos] >= 0 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].isGrass)
		{
			return true;
		}
		return false;
	}

	public void StopTrailEmission()
	{
		ParticleSystem.EmissionModule emission = farPartRen.GetComponent<ParticleSystem>().emission;
		ParticleSystem.EmissionModule emission2 = closePartRen.GetComponent<ParticleSystem>().emission;
		emission.enabled = false;
		emission2.enabled = false;
	}

	private void ResetTrail()
	{
		farPartRen.gameObject.SetActive(value: false);
		closePartRen.gameObject.SetActive(value: false);
		farPartRen.gameObject.SetActive(value: true);
		closePartRen.gameObject.SetActive(value: true);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcFadeOutLight()
	{
		StartCoroutine(FadeOutLight());
	}

	protected static void InvokeUserCode_RpcFadeOutLight(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFadeOutLight called on server.");
		}
		else
		{
			((MeteorMoves)obj).UserCode_RpcFadeOutLight();
		}
	}

	protected void UserCode_RPCPlayLandedExplosion()
	{
		closeHitGroundPartRen.gameObject.SetActive(value: true);
		farHitGroundPartRen.gameObject.SetActive(value: true);
		StopTrailEmission();
		fakeRock.SetActive(value: false);
		if (Vector3.Distance(CameraController.control.transform.position, base.transform.position) <= 100f)
		{
			SoundManager.Instance.play2DSound(meteorHitsClose);
			CameraController.control.shakeScreen(0.3f);
		}
		else
		{
			SoundManager.Instance.play2DSound(meteorHitsFar);
			CameraController.control.shakeScreen(0.15f);
		}
	}

	protected static void InvokeUserCode_RPCPlayLandedExplosion(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RPCPlayLandedExplosion called on server.");
		}
		else
		{
			((MeteorMoves)obj).UserCode_RPCPlayLandedExplosion();
		}
	}

	static MeteorMoves()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(MeteorMoves), "RpcFadeOutLight", InvokeUserCode_RpcFadeOutLight);
		RemoteCallHelper.RegisterRpcDelegate(typeof(MeteorMoves), "RPCPlayLandedExplosion", InvokeUserCode_RPCPlayLandedExplosion);
	}
}
