using UnityEngine;

public class SouthCityResident : MonoBehaviour
{
	private Animator myAnim;

	public bool sitting;

	public bool walking;

	public bool warmingHands;

	public bool isFletch;

	public bool isPlayer;

	public Transform headSpot;

	public EyesScript charEyes;

	public SkinnedMeshRenderer charSkinColour;

	private Vector3 startingPos = Vector3.zero;

	private GameObject hairObject;

	private void Start()
	{
		startingPos = base.transform.position;
	}

	private void OnEnable()
	{
		if (startingPos != Vector3.zero)
		{
			base.transform.position = startingPos;
		}
		if (!isFletch && !isPlayer)
		{
			Invoke("randomiseCharacterAppearance", Random.Range(0f, 0.25f));
		}
		else if (isFletch)
		{
			charEyes.changeSkinColor(NPCManager.manage.NPCDetails[6].NpcSkin.color);
		}
		else if (isPlayer)
		{
			setCharacterAppearance();
		}
	}

	public void randomiseCharacterAppearance()
	{
		Material[] materials = charSkinColour.materials;
		materials[0] = CharacterCreatorScript.create.skinTones[Random.Range(0, 5)];
		charSkinColour.materials = materials;
		charEyes.changeEyeColor(CharacterCreatorScript.create.eyeColours[Random.Range(0, 5)]);
		charEyes.noseMesh.GetComponent<MeshFilter>().sharedMesh = CharacterCreatorScript.create.noseMeshes[Random.Range(0, CharacterCreatorScript.create.noseMeshes.Length)];
		charEyes.changeSkinColor(materials[0].color);
		if ((bool)hairObject)
		{
			Object.Destroy(hairObject);
		}
		Transform transform = Object.Instantiate(CharacterCreatorScript.create.allHairStyles[Random.Range(0, CharacterCreatorScript.create.allHairStyles.Length)], headSpot).transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.Find("Hair").gameObject.SetActive(value: false);
		transform.Find("Hair_Hat").gameObject.SetActive(value: true);
		transform.Find("Hair_Hat").GetComponent<MeshRenderer>().material = CharacterCreatorScript.create.allHairColours[Random.Range(0, 6)];
		hairObject = transform.gameObject;
		myAnim = GetComponent<Animator>();
		if (sitting)
		{
			myAnim.SetBool("SittingOrLaying", value: true);
			myAnim.SetTrigger("Sitting");
		}
		if (walking)
		{
			myAnim.SetFloat("WalkSpeed", 0.5f);
			myAnim.SetBool("Tired", value: true);
		}
	}

	public void setCharacterAppearance()
	{
		Material[] materials = charSkinColour.materials;
		materials[0] = CharacterCreatorScript.create.skinTones[Inventory.Instance.skinTone];
		charEyes.changeEyeMat(CharacterCreatorScript.create.allEyeTypes[Inventory.Instance.playerEyes], materials[0].color);
		charSkinColour.materials = materials;
		charEyes.changeEyeColor(CharacterCreatorScript.create.eyeColours[Inventory.Instance.playerEyeColor]);
		charEyes.noseMesh.GetComponent<MeshFilter>().sharedMesh = CharacterCreatorScript.create.noseMeshes[Inventory.Instance.nose];
		charEyes.changeSkinColor(materials[0].color);
		if ((bool)hairObject)
		{
			Object.Destroy(hairObject);
		}
		Transform transform = Object.Instantiate(CharacterCreatorScript.create.allHairStyles[Inventory.Instance.playerHair], headSpot).transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.Find("Hair").gameObject.SetActive(value: false);
		transform.Find("Hair_Hat").gameObject.SetActive(value: true);
		transform.Find("Hair_Hat").GetComponent<MeshRenderer>().material = CharacterCreatorScript.create.allHairColours[Inventory.Instance.playerHairColour];
		hairObject = transform.gameObject;
		myAnim = GetComponent<Animator>();
		if (walking)
		{
			myAnim.SetFloat("WalkSpeed", 0.75f);
		}
	}

	private void Update()
	{
		if (walking)
		{
			base.transform.position = base.transform.position + base.transform.forward / 4f * Time.fixedDeltaTime;
		}
	}

	public void takeAStep()
	{
	}
}
