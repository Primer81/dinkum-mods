using System.Collections;
using UnityEngine;

public class AnimateAnimalAI : MonoBehaviour
{
	private AnimalAI myAi;

	private Animator myAnim;

	private Vector3 lastPos = Vector3.zero;

	private float travelSpeed;

	public bool animateDirection;

	public float animationSpeedMultiplier = 1f;

	private float facingDir;

	private Vector3 oldDirection = Vector3.zero;

	public bool flying;

	public bool canSwimAndWalk;

	private float flyingHeight;

	private AnimalMakeSounds hasSounds;

	public LayerMask groundLayers;

	private static int walkAnimationName;

	private static int swimAnimationName;

	private static int takeHitAnimationName;

	public float swimHeight = -1f;

	private bool useNormalPos;

	public Transform[] normalPositions;

	private bool fishInterested;

	private Quaternion lastRotation;

	public AnimalEyes myEyes;

	private bool inWater;

	private float currentFlyingHeight;

	public bool neverLands;

	public bool useFlightHeightFlux;

	public float heightMaxFluxMax = 6f;

	public float flyHeightFluxLow = 6f;

	private Transform _agentTransform;

	private Transform _myTransform;

	private int roundedX;

	private int roundedY;

	private float followY;

	private Vector3 followVel;

	private float[] lastSpeeds = new float[4];

	private int lastSpeedSlot;

	private float vel;

	private RaycastHit hit;

	private float scaleSwimDif = 1f;

	private void Start()
	{
		currentFlyingHeight = heightMaxFluxMax;
		walkAnimationName = Animator.StringToHash("WalkingSpeed");
		swimAnimationName = Animator.StringToHash("Swimming");
		takeHitAnimationName = Animator.StringToHash("TakeHit");
		myAi = GetComponent<AnimalAI>();
		myAnim = GetComponent<Animator>();
		scaleSwimDif = swimHeight;
		if ((bool)myAnim)
		{
			myAnim.SetFloat("Offset", Random.Range(0f, 1f));
			if (animationSpeedMultiplier != 1f)
			{
				myAnim.SetFloat("Speed", animationSpeedMultiplier);
			}
			hasSounds = GetComponent<AnimalMakeSounds>();
		}
		if (normalPositions.Length != 0)
		{
			useNormalPos = true;
		}
		followY = base.transform.position.y;
		_agentTransform = myAi.myAgent.transform;
		_myTransform = base.transform;
		roundedX = Mathf.RoundToInt(_agentTransform.position.x / 2f);
		roundedY = Mathf.RoundToInt(_agentTransform.position.z / 2f);
	}

	private void OnEnable()
	{
		if ((bool)myAnim)
		{
			myAnim.SetFloat("Offset", Random.Range(0f, 1f));
		}
		lastPos = base.transform.position;
		lastPos.y = 0f;
		flyingHeight = 0f;
		if (useFlightHeightFlux)
		{
			StartCoroutine(flyingHeightVariation());
		}
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	private void LateUpdate()
	{
		if (!myAi.isServer && !myAi.isInJump() && !myAi.waterOnly)
		{
			if (canSwimAndWalk && _myTransform.position.y < 1f && WorldManager.Instance.waterMap[roundedX, roundedY] && WorldManager.Instance.heightMap[roundedX, roundedY] < 0)
			{
				followY = _myTransform.position.y;
			}
			else if (NetworkNavMesh.nav.IsTileWalkable(roundedX, roundedY))
			{
				followY = _myTransform.position.y;
			}
			else if (Physics.Raycast(_myTransform.position + Vector3.up / 2f, Vector3.down, out hit, 3f, AnimalManager.manage.animalGroundedLayer))
			{
				followY = Mathf.Lerp(followY, hit.point.y, Time.deltaTime * 8f);
				_myTransform.position = new Vector3(_myTransform.position.x, followY, _myTransform.position.z);
				lastPos = _myTransform.position;
				lastPos.y = 0f;
			}
			else
			{
				followY = _myTransform.position.y;
			}
		}
	}

	private void FixedUpdate()
	{
		roundedX = Mathf.RoundToInt(_agentTransform.position.x / 2f);
		roundedY = Mathf.RoundToInt(_agentTransform.position.z / 2f);
		animateBasedOnSpeed();
		if (!myAi.isServer)
		{
			return;
		}
		_ = (bool)myAi.myAgent;
		Vector3 vector = _agentTransform.position;
		if (canSwimAndWalk)
		{
			if (_agentTransform.position.y < 1f && WorldManager.Instance.waterMap[roundedX, roundedY] && WorldManager.Instance.heightMap[roundedX, roundedY] < 0)
			{
				vector.y = swimHeight;
				inWater = true;
			}
			else
			{
				inWater = false;
			}
		}
		if (myAi.waterOnly)
		{
			vector = (fishInterested ? new Vector3(_agentTransform.position.x, 0f - scaleSwimDif, _agentTransform.position.z) : ((!myAi.currentlyAttacking()) ? new Vector3(_agentTransform.position.x, Mathf.Clamp(WorldManager.Instance.heightMap[(int)_agentTransform.position.x / 2, (int)_agentTransform.position.z / 2], -20f, 0f - scaleSwimDif), _agentTransform.position.z) : new Vector3(_agentTransform.position.x, Mathf.Clamp(myAi.currentlyAttacking().position.y, -20f, 0f - scaleSwimDif), _agentTransform.position.z)));
		}
		if (flying)
		{
			if (myAi.isDead())
			{
				flyingHeight = Mathf.Lerp(flyingHeight, 0f, Time.deltaTime * 15f);
			}
			else if (myAi.isStunned())
			{
				flyingHeight = Mathf.Lerp(flyingHeight, 0f, Time.deltaTime * 15f);
			}
			else if ((bool)myAi.currentlyAttacking() && Vector3.Distance(new Vector3(myAi.currentlyAttacking().position.x, 0f, myAi.currentlyAttacking().position.z), new Vector3(_myTransform.position.x, 0f, _myTransform.position.z)) <= 4f)
			{
				flyingHeight = Mathf.Lerp(flyingHeight, 2f, Time.deltaTime * 5f);
			}
			else if (neverLands || (myAi.myAgent.speed > myAi.getBaseSpeed() && !myAi.checkIfHasArrivedAtDestination()))
			{
				flyingHeight = Mathf.Lerp(flyingHeight, currentFlyingHeight, Time.deltaTime * 5f);
			}
			else
			{
				flyingHeight = Mathf.Lerp(flyingHeight, 0f, Time.deltaTime * 10f);
			}
		}
		if (!inWater && !myAi.waterOnly && NetworkNavMesh.nav.IsTileWalkable(roundedX, roundedY))
		{
			vector.y = WorldManager.Instance.heightMap[roundedX, roundedY];
		}
		else if (!inWater && !myAi.waterOnly && CameraController.control.IsCloseToCamera(_myTransform.position) && Physics.Raycast(vector + Vector3.up / 2f, Vector3.down, out hit, 4f, AnimalManager.manage.animalGroundedLayer))
		{
			vector.y = hit.point.y;
		}
		if (!myAi.isInJump())
		{
			_myTransform.position = Vector3.SmoothDamp(_myTransform.position, vector + Vector3.up * flyingHeight, ref followVel, 0.25f);
		}
		_myTransform.rotation = Quaternion.Lerp(_myTransform.rotation, _agentTransform.rotation, Time.deltaTime * myAi.myAgent.angularSpeed);
	}

	public IEnumerator flyingHeightVariation()
	{
		bool goingDown = false;
		float variationChangeTimer = 0f;
		while (true)
		{
			yield return null;
			while (variationChangeTimer < 2f)
			{
				if (goingDown)
				{
					currentFlyingHeight = Mathf.Lerp(heightMaxFluxMax, flyHeightFluxLow, variationChangeTimer / 2f);
				}
				else
				{
					currentFlyingHeight = Mathf.Lerp(flyHeightFluxLow, heightMaxFluxMax, variationChangeTimer / 2f);
				}
				variationChangeTimer += Time.deltaTime;
				yield return null;
			}
			goingDown = !goingDown;
			variationChangeTimer = 0f;
		}
	}

	private float getAverageSpeed(float latestSpeed)
	{
		lastSpeeds[lastSpeedSlot] = latestSpeed;
		lastSpeedSlot++;
		if (lastSpeedSlot > 3)
		{
			lastSpeedSlot = 0;
		}
		float num = 0f;
		for (int i = 0; i < 4; i++)
		{
			num += lastSpeeds[i];
		}
		return num / 4f;
	}

	private void animateBasedOnSpeed()
	{
		if (!myAnim)
		{
			return;
		}
		float num = Vector3.Distance(new Vector3(_myTransform.position.x, 0f, _myTransform.position.z), lastPos) / Time.fixedDeltaTime / myAi.getBaseSpeed();
		if (num > 0.15f)
		{
			travelSpeed = Mathf.SmoothDamp(travelSpeed, num, ref vel, 0.1f);
		}
		else
		{
			if (Quaternion.Angle(_myTransform.rotation, lastRotation) > 0.25f)
			{
				num = 1f;
			}
			if (num <= 0.1f)
			{
				num = 0f;
			}
			travelSpeed = Mathf.SmoothDamp(travelSpeed, num, ref vel, 0.1f);
		}
		if (neverLands)
		{
			travelSpeed = Mathf.Clamp(travelSpeed, 1.5f, 2f);
		}
		lastRotation = _myTransform.rotation;
		lastPos = _myTransform.position;
		lastPos.y = 0f;
		myAnim.SetFloat(walkAnimationName, travelSpeed);
		if (canSwimAndWalk)
		{
			if (_agentTransform.position.y < 1f && WorldManager.Instance.waterMap[roundedX, roundedY] && WorldManager.Instance.heightMap[roundedX, roundedY] < 0)
			{
				myAnim.SetBool(swimAnimationName, value: true);
			}
			else
			{
				myAnim.SetBool(swimAnimationName, value: false);
			}
		}
		if (animateDirection)
		{
			facingDir = Mathf.Lerp(facingDir, leftOrRight(oldDirection, _myTransform.forward, Vector3.up), Time.deltaTime * 1.5f);
			oldDirection = _myTransform.forward;
			myAnim.SetFloat("Direction", facingDir);
		}
	}

	public float leftOrRight(Vector3 fwd, Vector3 targetDir, Vector3 up)
	{
		float num = Vector3.Dot(Vector3.Cross(fwd, targetDir), up);
		if (num > 0.005f)
		{
			if (num > 0.01f)
			{
				return 1f;
			}
			return 0.5f;
		}
		if (num < -0.005f)
		{
			if (num < -0.01f)
			{
				return -1f;
			}
			return -0.5f;
		}
		return 0f;
	}

	public void takeAHitLocal()
	{
		if ((bool)hasSounds)
		{
			hasSounds.playDamageSound();
		}
		if ((bool)myAnim)
		{
			myAnim.SetTrigger(takeHitAnimationName);
		}
	}

	public void playDeathAnimation()
	{
		if ((bool)myEyes)
		{
			myEyes.deadEyes();
		}
		if ((bool)GetComponent<AnimalMakeSounds>())
		{
			GetComponent<AnimalMakeSounds>().playDamageSound();
		}
		if ((bool)GetComponent<Ragdoll>())
		{
			activateRagdoll();
		}
		myAnim.SetTrigger("Die");
	}

	public void activateRagdoll()
	{
		GetComponent<Ragdoll>().enableRagDoll();
	}

	public void setScaleSwimDif()
	{
		scaleSwimDif = Mathf.Clamp(base.transform.localScale.y, 1f, 3f);
	}

	public void makeFishInterested(bool newInterested)
	{
		fishInterested = newInterested;
	}

	public void setAnimator(Animator anim)
	{
		myAnim = anim;
		if (anim != null)
		{
			myAnim.SetFloat("Offset", Random.Range(0f, 1f));
			if ((bool)GetComponent<AnimalAI_Attack>())
			{
				GetComponent<AnimalAI_Attack>().setAnimator(myAnim);
			}
		}
	}
}
