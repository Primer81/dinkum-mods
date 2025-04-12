using UnityChan;
using UnityEngine;

public class CharNetworkAnimator : MonoBehaviour
{
	private float travelSpeed;

	private Vector3 lastPos;

	private Vector3 lastPosVehicle = Vector3.zero;

	public CharMovement myMovement;

	public Animator myAnim;

	public static int groundedAnimName;

	public static int walkingAnimName;

	public static int swimmingAnimName;

	public static int underwaterAnimName;

	public static int usingAnimName;

	public static int blockingAnimName;

	public static int climbingAnimName;

	public bool grounded;

	public SpringManager hairSpring;

	public Cloth dressCloth;

	public Cloth skirtCloth;

	public Cloth longDressCloth;

	private float desiredTravelSpeed;

	public GameObject navMeshObsticle;

	private bool standingOnVehicle;

	private bool dressOn;

	public bool swimming;

	private bool dressPhysicsOn = true;

	private float hairVel;

	private void Start()
	{
		groundedAnimName = Animator.StringToHash("Grounded");
		walkingAnimName = Animator.StringToHash("WalkSpeed");
		swimmingAnimName = Animator.StringToHash("Swimming");
		underwaterAnimName = Animator.StringToHash("UnderWater");
		usingAnimName = Animator.StringToHash("Using");
		blockingAnimName = Animator.StringToHash("Blocking");
		climbingAnimName = Animator.StringToHash("Climbing");
		lastPos = base.transform.position;
	}

	private void FixedUpdate()
	{
		grounded = Physics.CheckSphere(base.transform.position, 0.3f, myMovement.jumpLayers);
		if (grounded)
		{
			navMeshObsticle.SetActive(value: true);
		}
		else
		{
			navMeshObsticle.SetActive(value: false);
		}
		if (!myMovement.isLocalPlayer)
		{
			if (Vector3.Distance(base.transform.position, CameraController.control.transform.position) < 70f)
			{
				EnableDresses();
			}
			else
			{
				DisableDresses();
				grounded = true;
			}
			swimming = Physics.CheckSphere(base.transform.position, 0.1f, myMovement.swimLayers);
			if (!standingOnVehicle && (bool)base.transform.parent)
			{
				standingOnVehicle = true;
				lastPosVehicle = base.transform.localPosition;
			}
			else if (standingOnVehicle && !base.transform.parent)
			{
				standingOnVehicle = false;
			}
			if (standingOnVehicle)
			{
				desiredTravelSpeed = Vector3.Distance(base.transform.localPosition, lastPosVehicle) / Time.fixedDeltaTime / 9f;
				if (grounded)
				{
					travelSpeed = Mathf.Lerp(travelSpeed, desiredTravelSpeed, Time.fixedDeltaTime * 6f);
				}
				else
				{
					travelSpeed = Mathf.Lerp(travelSpeed, 0f, Time.deltaTime * 2f);
				}
				lastPosVehicle = base.transform.localPosition;
			}
			else if (grounded || swimming || myMovement.underWater || myMovement.climbing)
			{
				desiredTravelSpeed = Vector3.Distance(base.transform.position, lastPos) / Time.fixedDeltaTime / 9f;
				travelSpeed = Mathf.Lerp(travelSpeed, desiredTravelSpeed, Time.fixedDeltaTime * 6f);
			}
			else
			{
				desiredTravelSpeed = 0.75f;
				travelSpeed = Mathf.Lerp(travelSpeed, 0f, Time.deltaTime * 2f);
			}
			lastPos = base.transform.position;
			myAnim.SetBool(groundedAnimName, grounded);
			myAnim.SetBool(climbingAnimName, myMovement.climbing);
			myAnim.SetFloat(walkingAnimName, travelSpeed * 2f);
			if (myMovement.underWater && myMovement.myEquip.headId == myMovement.divingHelmet.getItemId())
			{
				myAnim.SetBool(swimmingAnimName, value: false);
				myAnim.SetBool(underwaterAnimName, value: false);
			}
			else
			{
				myAnim.SetBool(swimmingAnimName, swimming);
				myAnim.SetBool(underwaterAnimName, myMovement.underWater);
			}
			if ((bool)hairSpring)
			{
				if (standingOnVehicle)
				{
					hairSpring.dynamicRatio = Mathf.SmoothDamp(hairSpring.dynamicRatio, 0.15f, ref hairVel, 0.05f);
				}
				else if (desiredTravelSpeed > 0.5f)
				{
					hairSpring.dynamicRatio = Mathf.SmoothDamp(hairSpring.dynamicRatio, Mathf.Clamp(1f - desiredTravelSpeed, 0.15f, 1f), ref hairVel, 0.05f);
				}
				else
				{
					hairSpring.dynamicRatio = Mathf.SmoothDamp(hairSpring.dynamicRatio, Mathf.Clamp(1f - desiredTravelSpeed, 0.15f, 1f), ref hairVel, 0.15f);
				}
			}
			return;
		}
		if ((bool)hairSpring || (myMovement.myPickUp.drivingVehicle && myMovement.myPickUp.currentlyDriving.animateCharAsWell))
		{
			if (myMovement.standingOn != 0)
			{
				DisableDresses();
			}
			else
			{
				EnableDresses();
			}
			if (myMovement.grounded || myMovement.swimming || myMovement.underWater)
			{
				desiredTravelSpeed = Vector3.Distance(base.transform.position, lastPos) / Time.fixedDeltaTime / 9f;
				travelSpeed = Mathf.Lerp(travelSpeed, desiredTravelSpeed, Time.fixedDeltaTime * 6f);
			}
			else
			{
				desiredTravelSpeed = 0.75f;
				travelSpeed = Mathf.Lerp(travelSpeed, 0f, Time.deltaTime * 2f);
			}
			if (myMovement.myPickUp.drivingVehicle && myMovement.myPickUp.currentlyDriving.animateCharAsWell)
			{
				myAnim.SetFloat(walkingAnimName, travelSpeed * 2f);
			}
			if ((bool)hairSpring)
			{
				if (myMovement.standingOn != 0)
				{
					hairSpring.dynamicRatio = Mathf.SmoothDamp(hairSpring.dynamicRatio, 0.15f, ref hairVel, 0.05f);
				}
				else if (desiredTravelSpeed > 0.5f)
				{
					hairSpring.dynamicRatio = Mathf.SmoothDamp(hairSpring.dynamicRatio, Mathf.Clamp(1f - desiredTravelSpeed, 0.15f, 1f), ref hairVel, 0.05f);
				}
				else
				{
					hairSpring.dynamicRatio = Mathf.SmoothDamp(hairSpring.dynamicRatio, Mathf.Clamp(1f - desiredTravelSpeed, 0.15f, 1f), ref hairVel, 0.15f);
				}
			}
		}
		lastPos = base.transform.position;
	}

	public void SetDressOnOrOff(bool isDressOn)
	{
		dressOn = isDressOn;
	}

	public void DisableDresses()
	{
		if (dressPhysicsOn)
		{
			dressCloth.enabled = false;
			skirtCloth.enabled = false;
			longDressCloth.enabled = false;
			dressPhysicsOn = false;
		}
	}

	public void EnableDresses()
	{
		if (!dressPhysicsOn)
		{
			dressCloth.enabled = true;
			skirtCloth.enabled = true;
			longDressCloth.enabled = true;
			dressPhysicsOn = true;
		}
	}
}
