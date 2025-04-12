using UnityEngine;

public class SpecialWeaponEffect : MonoBehaviour
{
	public Animator playAnim;

	public void playSpecialEffect()
	{
		playAnim.SetTrigger("Play");
	}
}
