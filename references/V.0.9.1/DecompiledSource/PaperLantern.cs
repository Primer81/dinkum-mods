using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class PaperLantern : NetworkBehaviour
{
	private Transform _Transform;

	private int currentX;

	private int currentY;

	private float speed = 0.5f;

	private float heightDif = 1f;

	private float currentLifeTime;

	private float totalLifeTime = 10f;

	public Animator myAnim;

	[SyncVar(hook = "OnChangeColour")]
	private int showingColour;

	public Mesh[] colourMeshes;

	public MeshFilter meshFilter;

	private NetworkTransform netTransform;

	public LayerMask collisionsLayer;

	private bool dead;

	private Transform closestColliderTransform;

	private float closeColliderCheckTimer;

	private static WaitForSeconds lanternWait = new WaitForSeconds(1f);

	public int NetworkshowingColour
	{
		get
		{
			return showingColour;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref showingColour))
			{
				int oldColour = showingColour;
				SetSyncVar(value, ref showingColour, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnChangeColour(oldColour, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	private void Start()
	{
		_Transform = base.transform;
		RandomiseSettings();
	}

	public override void OnStartClient()
	{
		OnChangeColour(showingColour, showingColour);
		StartCoroutine(AnimatePaperLantern());
	}

	public override void OnStartServer()
	{
		netTransform = GetComponent<NetworkTransform>();
		StartCoroutine(LanternBehaves());
	}

	private IEnumerator LanternBehaves()
	{
		bool falling = true;
		bool rising = false;
		dead = false;
		while (!dead)
		{
			yield return null;
			Vector3 vector = _Transform.position + WeatherManager.Instance.windMgr.WindDirection * (speed + WeatherManager.Instance.windMgr.CurrentWindSpeed) * Time.deltaTime;
			vector += ClosestColliderAvoidence() * speed * 20f * Time.deltaTime;
			if (falling)
			{
				if (_Transform.position.y > GetCurrentHeight(vector))
				{
					vector.y -= speed * 2f * Time.deltaTime;
				}
				else
				{
					falling = false;
				}
			}
			else if (rising)
			{
				if (_Transform.position.y < 45f)
				{
					vector.y += speed * 2f * Time.deltaTime;
				}
				else
				{
					rising = false;
					dead = true;
				}
			}
			else
			{
				currentLifeTime += Time.deltaTime;
				if (IsLifeTimeUp())
				{
					rising = true;
				}
			}
			_Transform.position = vector;
			if (!NetworkNavMesh.nav.CheckIfVectorIsDistanceAwayFromPlayer(_Transform.position, 120f))
			{
				dead = true;
			}
		}
		ResetPosition();
	}

	public Vector3 ClosestColliderAvoidence()
	{
		if (closeColliderCheckTimer <= 0f)
		{
			closeColliderCheckTimer = Random.Range(0.25f, 0.35f);
			GetClosestColliderToAvoid();
		}
		else
		{
			closeColliderCheckTimer -= Time.deltaTime;
		}
		if ((bool)closestColliderTransform)
		{
			Vector3 normalized = (base.transform.position - closestColliderTransform.position).normalized;
			normalized.y = 0f;
			MonoBehaviour.print(closestColliderTransform);
			return normalized;
		}
		return Vector3.zero;
	}

	public void GetClosestColliderToAvoid()
	{
		if (closestColliderTransform != null && Vector3.Distance(closestColliderTransform.position, base.transform.position) >= 8f)
		{
			closestColliderTransform = null;
		}
		if (!(closestColliderTransform == null))
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, 6f, collisionsLayer);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].transform != base.transform)
			{
				closestColliderTransform = array[i].transform;
				break;
			}
		}
	}

	public float GetCurrentHeight(Vector3 nextPos)
	{
		if (WorldManager.Instance.isPositionOnMap(nextPos))
		{
			SetCurrentXAndY(nextPos);
			return (float)Mathf.Clamp(WorldManager.Instance.heightMap[currentX, currentY], 0, 18) + 12f + heightDif;
		}
		return 0f;
	}

	public bool IsLifeTimeUp()
	{
		return currentLifeTime >= totalLifeTime;
	}

	public void SetCurrentXAndY(Vector3 nextPos)
	{
		currentX = Mathf.RoundToInt(nextPos.x / 2f);
		currentY = Mathf.RoundToInt(nextPos.z / 2f);
	}

	public void RandomiseSettings()
	{
		speed = Random.Range(0.25f, 0.5f);
		heightDif = Random.Range(1f, 2.5f);
		totalLifeTime = Random.Range(35, 40);
		currentLifeTime = 0f;
		NetworkshowingColour = Random.Range(0, colourMeshes.Length);
	}

	private void OnEnable()
	{
		myAnim.SetFloat("OffSet", Random.Range(0f, 1f));
	}

	public void ResetPosition()
	{
		if (TownEventManager.manage.laternsActive)
		{
			netTransform.RpcTeleport(TownEventManager.manage.GetRandomLanternLocation());
			RandomiseSettings();
			StartCoroutine(LanternBehaves());
		}
		else
		{
			NetworkServer.Destroy(base.gameObject);
		}
	}

	public void OnChangeColour(int oldColour, int newColour)
	{
		NetworkshowingColour = newColour;
		meshFilter.sharedMesh = colourMeshes[newColour];
	}

	public void HitDead()
	{
		GetComponent<Damageable>().Networkhealth = 1;
		dead = true;
	}

	private IEnumerator AnimatePaperLantern()
	{
		myAnim.SetFloat("OffSet", Random.Range(0f, 1f));
		while (true)
		{
			if (base.transform.position.y >= 40f)
			{
				myAnim.SetBool("IsSmall", value: true);
			}
			else
			{
				myAnim.SetBool("IsSmall", value: false);
			}
			yield return lanternWait;
		}
	}

	private void MirrorProcessed()
	{
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(showingColour);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(showingColour);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = showingColour;
			NetworkshowingColour = reader.ReadInt();
			if (!SyncVarEqual(num, ref showingColour))
			{
				OnChangeColour(num, showingColour);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			int num3 = showingColour;
			NetworkshowingColour = reader.ReadInt();
			if (!SyncVarEqual(num3, ref showingColour))
			{
				OnChangeColour(num3, showingColour);
			}
		}
	}
}
