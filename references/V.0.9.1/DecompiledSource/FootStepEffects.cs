using System.Collections;
using UnityEngine;

public class FootStepEffects : MonoBehaviour
{
	private Vector3 oldPosition;

	public LayerMask jumpLayers;

	public LayerMask tileOnlyMask;

	private CharMovement hasCharMovement;

	public Animator hairAnim;

	public Transform leftFoot;

	public Transform rightFoot;

	public ASound wingSound;

	private Vector3 lastPassengerPos = Vector3.zero;

	public float smallStepVolume = 0.5f;

	public float smallStepPitch = 6f;

	private bool hasFeet;

	private WaitForSeconds waterWakeWait = new WaitForSeconds(0.15f);

	private static string untaggedTag = "Untagged";

	private static string stoneTag = "StoneFoot";

	private static string carpetTag = "CarpetFoot";

	private static string fakeWaterTag = "WaterFoot";

	private static string steelTag = "MetalFoot";

	private static string plasticTag = "PlasticFoot";

	public void Start()
	{
		hasCharMovement = GetComponent<CharMovement>();
		tileOnlyMask = (int)tileOnlyMask | (1 << LayerMask.NameToLayer("Tiles"));
		if ((bool)hasCharMovement)
		{
			jumpLayers = hasCharMovement.jumpLayers;
		}
		if ((bool)leftFoot || (bool)rightFoot)
		{
			hasFeet = true;
		}
	}

	private void OnEnable()
	{
		StartCoroutine(checkForWaterWake());
	}

	private IEnumerator checkForWaterWake()
	{
		while (true)
		{
			if (CameraController.control.IsCloseToCamera(base.transform.position) && base.transform.position.y <= 0.6f && base.transform.position.y >= -5f)
			{
				int num = (int)(Mathf.Round(base.transform.position.x + 0.5f) / 2f);
				int num2 = (int)(Mathf.Round(base.transform.position.z + 0.5f) / 2f);
				if (num < 0 || num > WorldManager.Instance.GetMapSize() - 1 || num2 < 0 || num2 > WorldManager.Instance.GetMapSize() - 1)
				{
					ParticleManager.manage.waterWakePart(base.transform.position, 3);
					yield return waterWakeWait;
				}
				else if (WorldManager.Instance.waterMap[num, num2])
				{
					ParticleManager.manage.waterWakePart(base.transform.position, 3);
					yield return waterWakeWait;
				}
			}
			yield return null;
		}
	}

	public void enterDeepWater()
	{
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.bigWaterSplash, base.transform.position);
		ParticleManager.manage.bigSplash(base.transform, 5);
	}

	private void bigSplashDelay()
	{
		ParticleManager.manage.bigSplash(base.transform);
	}

	public void treadWater()
	{
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.treadWater, base.transform.position);
		ParticleManager.manage.waterWakePart(base.transform.position + base.transform.forward + base.transform.right, 3);
		ParticleManager.manage.waterWakePart(base.transform.position + base.transform.forward + -base.transform.right, 3);
	}

	public void jumpNoise()
	{
	}

	public ASound getFootStepSoundForSurface()
	{
		if (Physics.Raycast(base.transform.position + Vector3.up / 4f, Vector3.down, out var hitInfo, 0.33f, jumpLayers))
		{
			if (hitInfo.transform.tag == untaggedTag)
			{
				return SoundManager.Instance.footStepWood;
			}
			if (hitInfo.transform.tag == carpetTag)
			{
				return SoundManager.Instance.carpetFootStep;
			}
			if (hitInfo.transform.tag == stoneTag)
			{
				return SoundManager.Instance.footStepStone;
			}
			if (hitInfo.transform.tag == fakeWaterTag)
			{
				return SoundManager.Instance.footStepWater;
			}
			if (hitInfo.transform.tag == steelTag)
			{
				return SoundManager.Instance.steelFootStep;
			}
			if (hitInfo.transform.tag == plasticTag)
			{
				return SoundManager.Instance.plasticFootStep;
			}
		}
		return SoundManager.Instance.footStepWood;
	}

	public void takeAStep(int foot = -1)
	{
		if (Vector3.Distance(base.transform.position, CameraController.control.transform.position) > 20f)
		{
			return;
		}
		if (Physics.CheckSphere(base.transform.position, 0.3f, jumpLayers) && Vector3.Distance(oldPosition, base.transform.position) > 0.5f)
		{
			bool flag = true;
			Vector3 position = base.transform.position;
			int num = Mathf.RoundToInt(position.x / 2f);
			int num2 = Mathf.RoundToInt(position.z / 2f);
			if (num < 0 || num > WorldManager.Instance.GetMapSize() - 1 || num2 < 0 || num2 > WorldManager.Instance.GetMapSize() - 1)
			{
				return;
			}
			if (WorldManager.Instance.waterMap[num, num2] && base.transform.position.y <= 0.6f && base.transform.position.y >= -1f)
			{
				ParticleManager.manage.waterSplash(base.transform.position);
			}
			else if (WorldManager.Instance.onTileMap[num, num2] == 527)
			{
				ParticleManager.manage.waterBedSplash(base.transform.position);
			}
			else if (WorldManager.Instance.onTileMap[num, num2] == 881)
			{
				ParticleManager.manage.lavaBedSplash(base.transform.position);
			}
			else if (base.transform.position.y < -6f)
			{
				SoundManager.Instance.playASoundAtPoint(getFootStepSoundForSurface(), base.transform.position);
			}
			else if ((bool)hasCharMovement && (bool)hasCharMovement.standingOnTrans)
			{
				if ((bool)hasCharMovement && Vector3.Distance(lastPassengerPos, hasCharMovement.transform.localPosition) > 0.1f)
				{
					SoundManager.Instance.playASoundAtPoint(getFootStepSoundForSurface(), base.transform.position);
					lastPassengerPos = hasCharMovement.transform.localPosition;
				}
			}
			else
			{
				RaycastHit hitInfo;
				if (!Physics.CheckSphere(base.transform.position, 0.3f, tileOnlyMask))
				{
					flag = false;
				}
				else if (Physics.Raycast(base.transform.position + Vector3.up / 4f, Vector3.down, out hitInfo, 0.33f, jumpLayers) && hitInfo.transform.gameObject.layer != LayerMask.NameToLayer("Tiles"))
				{
					flag = false;
				}
				if (hasFeet && flag)
				{
					if (foot == 0)
					{
						ParticleManager.manage.emitFeetDust(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].footStepParticle, leftFoot.position, base.transform.rotation);
					}
					if (foot == 1)
					{
						ParticleManager.manage.emitFeetDust(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].footStepParticle, rightFoot.position, base.transform.rotation);
					}
				}
				if (!flag)
				{
					SoundManager.Instance.playASoundAtPoint(getFootStepSoundForSurface(), base.transform.position);
				}
			}
			makeFootStepNoiseType(position, flag);
		}
		oldPosition = base.transform.position;
	}

	public void flapWing()
	{
		if ((bool)wingSound)
		{
			SoundManager.Instance.playASoundAtPoint(wingSound, base.transform.position);
		}
	}

	public void makeSmallFootStep()
	{
		if (Vector3.Distance(base.transform.position, CameraController.control.transform.position) > 20f || Vector3.Distance(oldPosition, base.transform.position) < 0.05f)
		{
			return;
		}
		oldPosition = base.transform.position;
		int num = (int)(Mathf.Round(base.transform.position.x + 0.5f) / 2f);
		int num2 = (int)(Mathf.Round(base.transform.position.z + 0.5f) / 2f);
		if (base.transform.position.y < 0.4f && base.transform.position.y >= -5f)
		{
			if (WorldManager.Instance.waterMap[num, num2])
			{
				SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.footStepWater, base.transform.position, smallStepVolume, smallStepPitch / 2f);
			}
		}
		else if (base.transform.position.y < -6f || ((bool)hasCharMovement && (bool)hasCharMovement.standingOnTrans))
		{
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.footStepWood, base.transform.position, smallStepVolume, smallStepPitch);
		}
		else if (WorldManager.Instance.onTileMap[num, num2] == 15)
		{
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.footStepWood, base.transform.position, smallStepVolume, smallStepPitch);
		}
		else if ((bool)WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].footStepSound)
		{
			SoundManager.Instance.playASoundAtPoint(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].footStepSound, base.transform.position, smallStepVolume, smallStepPitch);
		}
		else
		{
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.footStepDirt, base.transform.position, smallStepVolume, smallStepPitch);
		}
	}

	public void LadderSound()
	{
		int num = Mathf.RoundToInt(base.transform.position.x / 2f);
		int num2 = Mathf.RoundToInt(base.transform.position.z / 2f);
		if (WorldManager.Instance.isPositionOnMap(num, num2) && WorldManager.Instance.onTileMap[num, num2] >= 0 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[num, num2]].tileObjectConnect && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[num, num2]].tileObjectConnect.IsLadder)
		{
			SoundManager.Instance.playASoundAtPoint(WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[num, num2]].tileObjectConnect.IsLadder.climbSound, base.transform.position);
		}
	}

	public void makeFootStepNoiseType(Vector3 pos, bool isOnATile)
	{
		if (Vector3.Distance(base.transform.position, CameraController.control.transform.position) > 20f)
		{
			return;
		}
		int num = (int)(Mathf.Round(pos.x + 0.5f) / 2f);
		int num2 = (int)(Mathf.Round(pos.z + 0.5f) / 2f);
		if ((bool)hasCharMovement && (bool)hasCharMovement.standingOnTrans)
		{
			return;
		}
		if ((WorldManager.Instance.waterMap[num, num2] && base.transform.position.y < 0.4f && base.transform.position.y >= -5f) || WorldManager.Instance.onTileMap[num, num2] == 527)
		{
			if (base.transform.position.y <= -1f)
			{
				SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.underwaterFootSteps, base.transform.position);
			}
			else
			{
				SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.footStepWater, base.transform.position);
			}
		}
		else
		{
			if (base.transform.position.y < -6f || ((bool)hasCharMovement && (bool)hasCharMovement.standingOnTrans))
			{
				return;
			}
			if (WorldManager.Instance.onTileMap[num, num2] == 15)
			{
				if ((bool)hasCharMovement && (bool)hasCharMovement.myEquip && hasCharMovement.myEquip.shoeId == -1)
				{
					SoundManager.Instance.playASoundAtPointWithPitch(SoundManager.Instance.footStepWood, base.transform.position, SoundManager.Instance.footStepWood.getPitch() * 2f);
				}
				else
				{
					SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.footStepWood, base.transform.position);
				}
			}
			else if (isOnATile && (bool)WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].footStepSound)
			{
				if ((bool)hasCharMovement && (bool)hasCharMovement.myEquip && hasCharMovement.myEquip.shoeId == -1)
				{
					SoundManager.Instance.playASoundAtPointWithPitch(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].footStepSound, base.transform.position, WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].footStepSound.getPitch() * 2f);
				}
				else
				{
					SoundManager.Instance.playASoundAtPoint(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].footStepSound, base.transform.position);
				}
				if (WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].collectsSnow && SeasonManager.manage.IsSnowDay())
				{
					SoundManager.Instance.playASoundAtPoint(SeasonManager.manage.snowSteps, base.transform.position);
				}
			}
			else if (isOnATile)
			{
				SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.footStepDirt, base.transform.position);
			}
		}
	}
}
