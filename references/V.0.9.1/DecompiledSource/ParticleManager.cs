using System;
using System.Collections;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
	public ParticleSystem[] allParts;

	public static ParticleManager manage;

	public ParticleSystem sweatParticles;

	public ParticleSystem waterParticle;

	public ParticleSystem waterMist;

	public ParticleSystem bigSplashPart;

	public ParticleSystem giantSplashPart;

	public ParticleSystem waterWake;

	public ParticleSystem attackBubble;

	public ParticleSystem attackParticle;

	public ParticleSystem fishSplash;

	public ParticleSystem supriseParticle;

	public ParticleSystem redAttackBubble;

	public ParticleSystem redAttackParticle;

	public ParticleSystem[] feetDust;

	public ParticleSystem laughingPart;

	public ParticleSystem angryPart;

	public ParticleSystem cryingPart;

	public ParticleSystem cryingPart2;

	public ParticleSystem thinkingPart;

	public ParticleSystem shockedPart;

	public ParticleSystem sighPart;

	public ParticleSystem proudPart;

	public ParticleSystem pumpedParticle;

	public ParticleSystem gleePart;

	public ParticleSystem playerSpeakingPart;

	public ParticleSystem questionPart;

	public ParticleSystem shyPart;

	public ParticleSystem worriedPart;

	public ParticleSystem sleepingPart;

	public ParticleSystem groundAttackBubble;

	public ParticleSystem breathFog;

	public ParticleSystem brokenItemPart;

	public ParticleSystem fireStatusParticle;

	public ParticleSystem fireStatusGlowParticles;

	public ParticleSystem poisonStatusParticle;

	public ParticleSystem callForHelpPart;

	public ParticleSystem teleportParticles;

	public ParticleSystem motorBikeTracks;

	public ParticleSystem paintVehicle;

	public ParticleSystem explosion;

	public ParticleSystem toadSplosion;

	public ParticleSystem metalDetectorClose;

	public ParticleSystem metalDetectorFound;

	public ParticleSystem deathSmoke;

	public ParticleSystem deathSpark;

	public ParticleSystem healParticle;

	public ParticleSystem enemyHealParticle;

	private ParticleSystem.MainModule motorbikeTrackMain;

	public GameObject stunnedParticle;

	[Header("Pick Up Particles")]
	public ParticleSystem pickUpParticle;

	public ParticleSystem pickUpBubble;

	public ParticleSystem houseChangePart;

	public ParticleSystem bubbleBlowerPart;

	public ParticleSystem wishingWellPart;

	public ParticleSystem extinquishLavalPart;

	public ParticleSystem LavaSplashPart;

	private WaitForSeconds splashWait = new WaitForSeconds(0.01f);

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
		motorbikeTrackMain = motorBikeTracks.main;
	}

	public void emitParticleAtPosition(ParticleSystem partToMove, Vector3 position, int emitAmount = 25)
	{
		if (Vector3.Distance(CameraController.control.transform.position, position) <= (float)(NewChunkLoader.loader.getChunkDistance() * 15))
		{
			partToMove.transform.position = position;
			partToMove.Emit(emitAmount);
		}
	}

	public void waterSplash(Vector3 splashPos, int emitAmount = 3)
	{
		if (Vector3.Distance(CameraController.control.transform.position, splashPos) <= (float)(NewChunkLoader.loader.getChunkDistance() * 15))
		{
			splashPos = new Vector3(splashPos.x, 0.61f, splashPos.z);
			waterParticle.transform.position = splashPos;
			waterParticle.Emit(emitAmount);
			waterMist.transform.position = splashPos;
			waterMist.Emit(emitAmount);
		}
	}

	public void waterBedSplash(Vector3 splashPos, int emitAmount = 3)
	{
		if (Vector3.Distance(CameraController.control.transform.position, splashPos) <= (float)(NewChunkLoader.loader.getChunkDistance() * 15))
		{
			float y = (float)WorldManager.Instance.heightMap[Mathf.RoundToInt(splashPos.x / 2f), Mathf.RoundToInt(splashPos.z / 2f)] + 0.61f;
			splashPos = new Vector3(splashPos.x, y, splashPos.z);
			waterParticle.transform.position = splashPos;
			waterParticle.Emit(emitAmount);
			waterMist.transform.position = splashPos;
			waterMist.Emit(emitAmount);
		}
	}

	public void lavaBedSplash(Vector3 splashPos, int emitAmount = 3)
	{
		if (Vector3.Distance(CameraController.control.transform.position, splashPos) <= (float)(NewChunkLoader.loader.getChunkDistance() * 15))
		{
			float y = (float)WorldManager.Instance.heightMap[Mathf.RoundToInt(splashPos.x / 2f), Mathf.RoundToInt(splashPos.z / 2f)] + 0.61f;
			splashPos = new Vector3(splashPos.x, y, splashPos.z);
			LavaSplashPart.transform.position = splashPos;
			LavaSplashPart.Emit(emitAmount);
		}
	}

	public void waterWakePart(Vector3 splashPos, int emitAmount = 25)
	{
		if (Vector3.Distance(CameraController.control.transform.position, splashPos) <= (float)(NewChunkLoader.loader.getChunkDistance() * 15))
		{
			splashPos = new Vector3(splashPos.x, 0.62f, splashPos.z);
			waterWake.transform.position = splashPos;
			waterWake.Emit(emitAmount);
		}
	}

	public void bigSplash(Transform splashPos, int emitAmount = 1, float height = 0.61f)
	{
		StartCoroutine(splashDropDelay(splashPos.position, emitAmount, height));
	}

	private IEnumerator splashDropDelay(Vector3 splashPos, int amount, float height)
	{
		while (amount > 0)
		{
			splashPos.y = height;
			bigSplashPart.transform.position = splashPos;
			bigSplashPart.Emit(2);
			waterMist.transform.position = splashPos;
			waterMist.Emit(5);
			waterWakePart(splashPos, 4);
			yield return splashWait;
			amount--;
		}
	}

	public void emitAttackParticle(Vector3 attackPos, int emitAmount = 35)
	{
		if (Vector3.Distance(CameraController.control.transform.position, attackPos) < (float)(NewChunkLoader.loader.getChunkDistance() * 15))
		{
			attackBubble.transform.position = attackPos;
			attackBubble.Emit(1);
			attackParticle.transform.position = attackPos;
			attackParticle.Emit(emitAmount);
		}
	}

	public void emitPickupParticle(Vector3 attackPos)
	{
		if (Vector3.Distance(CameraController.control.transform.position, attackPos) < (float)(NewChunkLoader.loader.getChunkDistance() * 15))
		{
			pickUpBubble.transform.position = attackPos;
			pickUpBubble.Emit(1);
		}
	}

	public void emitBrokenItemPart(Vector3 attackPos, int emitAmount = 10)
	{
		if (Vector3.Distance(CameraController.control.transform.position, attackPos) < (float)(NewChunkLoader.loader.getChunkDistance() * 15))
		{
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.toolBreaks, attackPos);
			brokenItemPart.transform.position = attackPos;
			brokenItemPart.Emit(emitAmount);
			attackParticle.transform.position = attackPos;
			attackParticle.Emit(25);
		}
	}

	public void emitRedAttackParticle(Vector3 attackPos, int emitAmount = 50)
	{
		if (Vector3.Distance(CameraController.control.transform.position, attackPos) < (float)(NewChunkLoader.loader.getChunkDistance() * 15))
		{
			redAttackBubble.transform.position = attackPos;
			redAttackBubble.Emit(1);
			redAttackParticle.transform.position = attackPos;
			redAttackParticle.Emit(emitAmount);
		}
	}

	public void emitDeathParticle(Vector3 position)
	{
		if (Vector3.Distance(CameraController.control.transform.position, position) < (float)(NewChunkLoader.loader.getChunkDistance() * 15))
		{
			deathSpark.transform.position = position;
			deathSpark.Emit(25);
			deathSmoke.transform.position = position;
			deathSmoke.Emit(15);
		}
	}

	public void emitGroundAttackParticles(Vector3 attackPos)
	{
		if (Vector3.Distance(CameraController.control.transform.position, attackPos) < (float)(NewChunkLoader.loader.getChunkDistance() * 15))
		{
			groundAttackBubble.transform.position = new Vector3(attackPos.x, WorldManager.Instance.heightMap[(int)attackPos.x / 2, (int)attackPos.z / 2], attackPos.z);
			groundAttackBubble.Emit(5);
			attackParticle.transform.position = new Vector3(attackPos.x, WorldManager.Instance.heightMap[(int)attackPos.x / 2, (int)attackPos.z / 2], attackPos.z);
			attackParticle.Emit(15);
		}
	}

	public void emitFeetDust(int desiredFootPart, Vector3 footPos, Quaternion rotation)
	{
		if (desiredFootPart != -1)
		{
			feetDust[desiredFootPart].transform.rotation = rotation;
			feetDust[desiredFootPart].transform.position = footPos;
			feetDust[desiredFootPart].Emit(8);
		}
	}

	public void breathParticleAtPos(Transform pos)
	{
		breathFog.transform.position = pos.position;
		breathFog.transform.rotation = pos.rotation;
		breathFog.Emit(8);
	}

	public void startTeleportParticles(Transform trans, int[] teleportToPos)
	{
		StartCoroutine(teleportParticlesDelay(trans, teleportToPos));
	}

	public void startTeleportParticlesVector(Transform trans, Vector3 teleportToPos)
	{
		StartCoroutine(teleportParticlesDelayVector(trans, teleportToPos));
	}

	public void makeMotorbikeTrack(Transform wheelPos)
	{
		motorBikeTracks.transform.position = wheelPos.position;
		motorbikeTrackMain.startRotation = wheelPos.rotation.eulerAngles.y * ((float)Math.PI / 180f);
		motorBikeTracks.Emit(1);
	}

	public void spawnStunnedParticle(Damageable belongsTo)
	{
		Transform headPos = belongsTo.transform;
		Vector3 followPos = Vector3.up * 2.5f;
		if ((bool)belongsTo.headPos)
		{
			headPos = belongsTo.headPos;
			followPos = Vector3.up;
		}
		GameObject toDestory = UnityEngine.Object.Instantiate(stunnedParticle);
		StartCoroutine(destroyStunnedPartAfterStunned(toDestory, headPos, followPos, belongsTo));
	}

	private IEnumerator destroyStunnedPartAfterStunned(GameObject toDestory, Transform follow, Vector3 followPos, Damageable belongsTo)
	{
		while ((bool)belongsTo && belongsTo.isStunned())
		{
			toDestory.transform.position = follow.position + followPos;
			yield return null;
		}
		UnityEngine.Object.Destroy(toDestory);
	}

	private IEnumerator teleportParticlesDelay(Transform telepos, int[] teleportToPos)
	{
		float timer = 0f;
		Vector3 secondPos = new Vector3((float)teleportToPos[0] * 2f + 1f, (float)WorldManager.Instance.heightMap[teleportToPos[0], teleportToPos[1]] + 0.61f, (float)teleportToPos[1] * 2f + 1.5f);
		while (timer < 2f)
		{
			timer += Time.deltaTime;
			if (Vector3.Distance(telepos.position, secondPos) > 10f)
			{
				emitParticleAtPosition(teleportParticles, telepos.position, 5);
			}
			emitParticleAtPosition(teleportParticles, secondPos, 5);
			yield return null;
		}
	}

	private IEnumerator teleportParticlesDelayVector(Transform telepos, Vector3 teleportToPos)
	{
		float timer = 0f;
		while (timer < 2f)
		{
			timer += Time.deltaTime;
			if (Vector3.Distance(telepos.position, teleportToPos) > 3f)
			{
				emitParticleAtPosition(teleportParticles, telepos.position, 5);
			}
			emitParticleAtPosition(teleportParticles, teleportToPos, 5);
			yield return null;
		}
	}
}
