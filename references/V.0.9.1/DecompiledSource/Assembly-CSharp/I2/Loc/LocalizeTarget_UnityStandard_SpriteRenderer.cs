using UnityEngine;

namespace I2.Loc;

public class LocalizeTarget_UnityStandard_SpriteRenderer : LocalizeTarget<SpriteRenderer>
{
	static LocalizeTarget_UnityStandard_SpriteRenderer()
	{
		AutoRegister();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void AutoRegister()
	{
		LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<SpriteRenderer, LocalizeTarget_UnityStandard_SpriteRenderer>
		{
			Name = "SpriteRenderer",
			Priority = 100
		});
	}

	public override eTermType GetPrimaryTermType(Localize cmp)
	{
		return eTermType.Sprite;
	}

	public override eTermType GetSecondaryTermType(Localize cmp)
	{
		return eTermType.Text;
	}

	public override bool CanUseSecondaryTerm()
	{
		return false;
	}

	public override bool AllowMainTermToBeRTL()
	{
		return false;
	}

	public override bool AllowSecondTermToBeRTL()
	{
		return false;
	}

	public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
	{
		Sprite sprite = mTarget.sprite;
		primaryTerm = ((sprite != null) ? sprite.name : string.Empty);
		secondaryTerm = null;
	}

	public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
	{
		Sprite sprite = mTarget.sprite;
		if (sprite == null || sprite.name != mainTranslation)
		{
			mTarget.sprite = cmp.FindTranslatedObject<Sprite>(mainTranslation);
		}
	}
}
