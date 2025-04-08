using Cairo;
using Vintagestory.API.Config;

namespace Vintagestory.API.Client;

public class RichTextComponent : RichTextComponentBase
{
	protected TextDrawUtil textUtil;

	protected EnumLinebreakBehavior linebreak = EnumLinebreakBehavior.AfterWord;

	public string DisplayText;

	public CairoFont Font;

	public TextLine[] Lines;

	private double spaceWidthCached = -99.0;

	private double spaceWidth
	{
		get
		{
			if (spaceWidthCached >= 0.0)
			{
				return spaceWidthCached;
			}
			return spaceWidthCached = (Font.GetTextExtents("a b").Width - Font.GetTextExtents("ab").Width) / (double)RuntimeEnv.GUIScale;
		}
	}

	public RichTextComponent(ICoreClientAPI api, string displayText, CairoFont font)
		: base(api)
	{
		DisplayText = displayText;
		Font = font;
		linebreak = Lang.AvailableLanguages[Lang.CurrentLocale].LineBreakBehavior;
		if (api != null)
		{
			init();
		}
	}

	protected void init()
	{
		if (DisplayText.Length > 0)
		{
			bool done = false;
			while (!done && DisplayText.Length > 0)
			{
				done = true;
				if (DisplayText[DisplayText.Length - 1] == ' ')
				{
					PaddingRight += spaceWidth;
					done = false;
					DisplayText = DisplayText.Substring(0, DisplayText.Length - 1);
				}
				if (DisplayText.Length == 0)
				{
					break;
				}
				if (DisplayText[0] == ' ')
				{
					PaddingLeft += spaceWidth;
					DisplayText = DisplayText.Substring(1, DisplayText.Length - 1);
					done = false;
				}
			}
		}
		else
		{
			PaddingLeft = 0.0;
			PaddingRight = 0.0;
		}
		textUtil = new TextDrawUtil();
	}

	/// <summary>
	/// Composes the element.
	/// </summary>
	/// <param name="ctx">Context of the text component.</param>
	/// <param name="surface">The surface of the image.</param>
	public override void ComposeElements(Context ctx, ImageSurface surface)
	{
		textUtil.DrawMultilineText(ctx, Font, Lines, Font.Orientation);
	}

	/// <summary>
	/// Renders the text component.
	/// </summary>
	/// <param name="deltaTime"></param>
	/// <param name="renderX"></param>
	/// <param name="renderY"></param>
	/// <param name="renderZ"></param>
	public override void RenderInteractiveElements(float deltaTime, double renderX, double renderY, double renderZ)
	{
	}

	/// <summary>
	/// Initializes the size and stuff. Return true if you had to enter the next line
	/// </summary>
	/// <param name="flowPath"></param>
	/// <param name="currentLineHeight"></param>
	/// <param name="offsetX"></param>
	/// <param name="lineY"></param>
	/// <param name="nextOffsetX"></param>
	/// <returns>True when longer than 1 line</returns>
	public override EnumCalcBoundsResult CalcBounds(TextFlowPath[] flowPath, double currentLineHeight, double offsetX, double lineY, out double nextOffsetX)
	{
		offsetX += GuiElement.scaled(PaddingLeft);
		Lines = textUtil.Lineize(Font, DisplayText, linebreak, flowPath, offsetX, lineY);
		nextOffsetX = offsetX;
		BoundsPerLine = new LineRectangled[Lines.Length];
		for (int i = 0; i < Lines.Length; i++)
		{
			TextLine line = Lines[i];
			BoundsPerLine[i] = line.Bounds;
		}
		if (Lines.Length != 0)
		{
			LineRectangled lbnd = BoundsPerLine[Lines.Length - 1];
			lbnd.Width += GuiElement.scaled(PaddingRight);
			nextOffsetX = Lines[Lines.Length - 1].NextOffsetX + lbnd.Width;
		}
		if (Lines.Length <= 1)
		{
			return EnumCalcBoundsResult.Continue;
		}
		return EnumCalcBoundsResult.Multiline;
	}

	protected double GetFontOrientOffsetX()
	{
		if (Lines.Length == 0)
		{
			return 0.0;
		}
		TextLine textLine = Lines[Lines.Length - 1];
		double offsetX = 0.0;
		if (Font.Orientation == EnumTextOrientation.Center)
		{
			offsetX = (textLine.LeftSpace + textLine.RightSpace) / 2.0;
		}
		if (Font.Orientation == EnumTextOrientation.Right)
		{
			offsetX = textLine.LeftSpace + textLine.RightSpace;
		}
		return offsetX;
	}
}
