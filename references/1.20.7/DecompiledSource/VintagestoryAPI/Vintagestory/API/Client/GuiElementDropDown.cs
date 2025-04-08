using System;
using System.Linq;
using Cairo;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.Client;

/// <summary>
/// Creates a drop-down list of items.
/// </summary>
public class GuiElementDropDown : GuiElementTextBase
{
	public string SingularNameCode = "{0} item";

	public string PluralNameCode = "{0} items";

	public string PluralMoreNameCode = "+{0} more";

	public string SingularMoreNameCode = "+{0} more";

	public GuiElementListMenu listMenu;

	public GuiElementRichtext richTextElem;

	protected LoadedTexture highlightTexture;

	protected LoadedTexture currentValueTexture;

	protected LoadedTexture arrowDownButtonReleased;

	protected LoadedTexture arrowDownButtonPressed;

	protected ElementBounds highlightBounds;

	protected SelectionChangedDelegate onSelectionChanged;

	private bool multiSelect;

	private int valueWidth;

	private int valueHeight;

	/// <summary>
	/// The draw order of this GUI Element.
	/// </summary>
	public override double DrawOrder => 0.5;

	/// <summary>
	/// Can this element be put into focus?
	/// </summary>
	public override bool Focusable => true;

	/// <summary>
	/// The scale of this GUI element.
	/// </summary>
	public override double Scale
	{
		get
		{
			return base.Scale;
		}
		set
		{
			base.Scale = value;
			listMenu.Scale = value;
		}
	}

	public string SelectedValue
	{
		get
		{
			if (listMenu.SelectedIndex < 0)
			{
				return null;
			}
			return listMenu.Values[listMenu.SelectedIndex];
		}
	}

	public int[] SelectedIndices => listMenu.SelectedIndices;

	public string[] SelectedValues => listMenu.SelectedIndices.Select((int index) => listMenu.Values[index]).ToArray();

	public override bool Enabled
	{
		get
		{
			return base.Enabled;
		}
		set
		{
			if (enabled != value && currentValueTexture != null)
			{
				ComposeCurrentValue();
			}
			base.Enabled = value;
		}
	}

	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="capi">The client API</param>
	/// <param name="values">The values of the strings.</param>
	/// <param name="names">The names of the strings.</param>
	/// <param name="selectedIndex">The default selected index.</param>
	/// <param name="onSelectionChanged">The event that occurs when the selection is changed.</param>
	/// <param name="bounds">The bounds of the drop down.</param>
	/// <param name="font"></param>
	/// <param name="multiSelect"></param>
	public GuiElementDropDown(ICoreClientAPI capi, string[] values, string[] names, int selectedIndex, SelectionChangedDelegate onSelectionChanged, ElementBounds bounds, CairoFont font, bool multiSelect)
		: base(capi, "", font, bounds)
	{
		highlightTexture = new LoadedTexture(capi);
		currentValueTexture = new LoadedTexture(capi);
		arrowDownButtonReleased = new LoadedTexture(capi);
		arrowDownButtonPressed = new LoadedTexture(capi);
		listMenu = new GuiElementListMenu(capi, values, names, selectedIndex, didSelect, bounds, font, multiSelect)
		{
			HoveredIndex = selectedIndex
		};
		ElementBounds textBounds = ElementBounds.Fixed(0.0, 0.0, 900.0, 100.0).WithEmptyParent();
		richTextElem = new GuiElementRichtext(capi, new RichTextComponentBase[0], textBounds);
		this.onSelectionChanged = onSelectionChanged;
		this.multiSelect = multiSelect;
	}

	private void didSelect(string newvalue, bool on)
	{
		onSelectionChanged?.Invoke(newvalue, on);
		ComposeCurrentValue();
	}

	/// <summary>
	/// Composes the element based on the context.
	/// </summary>
	/// <param name="ctx">The context of the element.</param>
	/// <param name="surface">The surface of the image. (Not used)</param>
	public override void ComposeElements(Context ctx, ImageSurface surface)
	{
		Bounds.CalcWorldBounds();
		ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.2);
		GuiElement.RoundRectangle(ctx, Bounds.drawX, Bounds.drawY, Bounds.InnerWidth, Bounds.InnerHeight, 3.0);
		ctx.Fill();
		EmbossRoundRectangleElement(ctx, Bounds, inverse: true, 1, 1);
		listMenu.ComposeDynamicElements();
		ComposeDynamicElements();
	}

	private void ComposeDynamicElements()
	{
		int btnWidth = (int)(GuiElement.scaled(20.0) * Scale);
		int btnHeight = (int)Bounds.InnerHeight;
		ImageSurface surface = new ImageSurface(Format.Argb32, btnWidth, btnHeight);
		Context ctx = genContext(surface);
		ctx.SetSourceRGB(GuiStyle.DialogDefaultBgColor[0], GuiStyle.DialogDefaultBgColor[1], GuiStyle.DialogDefaultBgColor[2]);
		GuiElement.RoundRectangle(ctx, 0.0, 0.0, btnWidth, btnHeight, GuiStyle.ElementBGRadius);
		ctx.FillPreserve();
		ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.1);
		ctx.Fill();
		EmbossRoundRectangleElement(ctx, 0.0, 0.0, btnWidth, btnHeight, inverse: false, 2, 1);
		ctx.SetSourceRGBA(GuiStyle.DialogHighlightColor);
		GuiElement.RoundRectangle(ctx, 0.0, 0.0, btnWidth, btnHeight, 1.0);
		ctx.Fill();
		double arrowHeight = Math.Min(Bounds.OuterHeight - GuiElement.scaled(6.0), GuiElement.scaled(16.0));
		double updownspace = (Bounds.OuterHeight - arrowHeight) / 2.0;
		double up = updownspace;
		double down = arrowHeight + updownspace;
		ctx.NewPath();
		ctx.LineTo((double)btnWidth - GuiElement.scaled(17.0) * Scale, up * Scale);
		ctx.LineTo((double)btnWidth - GuiElement.scaled(3.0) * Scale, up * Scale);
		ctx.LineTo((double)btnWidth - GuiElement.scaled(10.0) * Scale, down * Scale);
		ctx.ClosePath();
		ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.6);
		ctx.Fill();
		generateTexture(surface, ref arrowDownButtonReleased);
		ctx.Operator = Operator.Clear;
		ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.0);
		ctx.Paint();
		ctx.Operator = Operator.Over;
		ctx.SetSourceRGB(GuiStyle.DialogDefaultBgColor[0], GuiStyle.DialogDefaultBgColor[1], GuiStyle.DialogDefaultBgColor[2]);
		GuiElement.RoundRectangle(ctx, 0.0, 0.0, btnWidth, btnHeight, GuiStyle.ElementBGRadius);
		ctx.FillPreserve();
		ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.1);
		ctx.Fill();
		EmbossRoundRectangleElement(ctx, 0.0, 0.0, btnWidth, btnHeight, inverse: true, 2, 1);
		ctx.SetSourceRGBA(GuiStyle.DialogHighlightColor);
		GuiElement.RoundRectangle(ctx, 0.0, 0.0, btnWidth, btnHeight, 1.0);
		ctx.Fill();
		ctx.NewPath();
		ctx.LineTo((double)btnWidth - GuiElement.scaled(17.0) * Scale, up * Scale);
		ctx.LineTo((double)btnWidth - GuiElement.scaled(3.0) * Scale, up * Scale);
		ctx.LineTo((double)btnWidth - GuiElement.scaled(10.0) * Scale, down * Scale);
		ctx.ClosePath();
		ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.4);
		ctx.Fill();
		generateTexture(surface, ref arrowDownButtonPressed);
		surface.Dispose();
		ctx.Dispose();
		ImageSurface surfaceHighlight = new ImageSurface(Format.Argb32, (int)Bounds.OuterWidth - btnWidth, (int)Bounds.OuterHeight);
		Context context = genContext(surfaceHighlight);
		context.SetSourceRGBA(1.0, 1.0, 1.0, 0.3);
		context.Paint();
		generateTexture(surfaceHighlight, ref highlightTexture);
		context.Dispose();
		surfaceHighlight.Dispose();
		highlightBounds = Bounds.CopyOffsetedSibling().WithFixedPadding(0.0, 0.0).FixedGrow(2.0 * Bounds.absPaddingX, 2.0 * Bounds.absPaddingY);
		highlightBounds.fixedWidth -= (float)btnWidth / RuntimeEnv.GUIScale;
		highlightBounds.CalcWorldBounds();
		ComposeCurrentValue();
	}

	private void ComposeCurrentValue()
	{
		double width = Bounds.InnerWidth;
		valueWidth = (int)((Bounds.InnerWidth - GuiElement.scaled(20.0)) * Scale);
		valueHeight = (int)(GuiElement.scaled(30.0) * Scale);
		ImageSurface surface = new ImageSurface(Format.Argb32, valueWidth, valueHeight);
		Context ctx = genContext(surface);
		if (!enabled)
		{
			Font.Color[3] = 0.5;
		}
		Font.SetupContext(ctx);
		ctx.SetSourceRGBA(GuiStyle.DialogDefaultTextColor);
		string text = "";
		double height = Font.GetFontExtents().Height;
		if (listMenu.SelectedIndices.Length > 1)
		{
			for (int i = 0; i < listMenu.SelectedIndices.Length; i++)
			{
				int index = listMenu.SelectedIndices[i];
				string addText = "";
				if (text.Length > 0)
				{
					addText += ", ";
				}
				addText += listMenu.Names[index];
				int cntleft = listMenu.SelectedIndices.Length - i;
				int cnt = listMenu.SelectedIndices.Length;
				string moreText = ((text.Length > 0) ? (" " + ((cntleft == 1) ? Lang.Get(SingularMoreNameCode, cntleft) : Lang.Get(PluralMoreNameCode, cntleft))) : ((cnt == 1) ? Lang.Get(SingularNameCode, cnt) : Lang.Get(PluralNameCode, cnt)));
				if (Font.GetTextExtents(text + addText + Lang.Get(PluralMoreNameCode, cntleft)).Width < width)
				{
					text += addText;
					continue;
				}
				text += moreText;
				break;
			}
		}
		else if (listMenu.SelectedIndices.Length == 1)
		{
			text = listMenu.Names[listMenu.SelectedIndex];
		}
		richTextElem.SetNewTextWithoutRecompose(text, Font);
		richTextElem.BeforeCalcBounds();
		richTextElem.Bounds.fixedX = 5.0;
		richTextElem.Bounds.fixedY = ((double)valueHeight - height) / 2.0 / (double)RuntimeEnv.GUIScale;
		richTextElem.BeforeCalcBounds();
		richTextElem.Bounds.CalcWorldBounds();
		richTextElem.ComposeFor(richTextElem.Bounds, ctx, surface);
		generateTexture(surface, ref currentValueTexture);
		ctx.Dispose();
		surface.Dispose();
	}

	/// <summary>
	/// Renders the dropdown's interactive elements.
	/// </summary>
	/// <param name="deltaTime">The change in time.</param>
	public override void RenderInteractiveElements(float deltaTime)
	{
		if (base.HasFocus)
		{
			api.Render.Render2DTexture(highlightTexture.TextureId, highlightBounds);
		}
		api.Render.Render2DTexturePremultipliedAlpha(currentValueTexture.TextureId, (int)Bounds.renderX, (double)(int)Bounds.renderY + (Bounds.InnerHeight - (double)valueHeight) / 2.0, valueWidth, valueHeight);
		double renderX = Bounds.renderX + Bounds.InnerWidth - (double)arrowDownButtonReleased.Width;
		double renderY = Bounds.renderY;
		if (listMenu.IsOpened)
		{
			api.Render.Render2DTexturePremultipliedAlpha(arrowDownButtonPressed.TextureId, renderX, renderY, arrowDownButtonReleased.Width, arrowDownButtonReleased.Height);
		}
		else
		{
			api.Render.Render2DTexturePremultipliedAlpha(arrowDownButtonReleased.TextureId, renderX, renderY, arrowDownButtonReleased.Width, arrowDownButtonReleased.Height);
		}
		listMenu.RenderInteractiveElements(deltaTime);
	}

	public override void OnKeyDown(ICoreClientAPI api, KeyEvent args)
	{
		listMenu.OnKeyDown(api, args);
	}

	public override void OnMouseMove(ICoreClientAPI api, MouseEvent args)
	{
		listMenu.OnMouseMove(api, args);
	}

	public override void OnMouseWheel(ICoreClientAPI api, MouseWheelEventArgs args)
	{
		if (enabled && base.HasFocus)
		{
			if (!listMenu.IsOpened && IsPositionInside(api.Input.MouseX, api.Input.MouseY))
			{
				SetSelectedIndex(GameMath.Mod(listMenu.SelectedIndex + ((args.delta <= 0) ? 1 : (-1)), listMenu.Values.Length));
				args.SetHandled();
				onSelectionChanged?.Invoke(SelectedValue, selected: true);
			}
			else
			{
				listMenu.OnMouseWheel(api, args);
			}
		}
	}

	public override void OnMouseUp(ICoreClientAPI api, MouseEvent args)
	{
		listMenu.OnMouseUp(api, args);
		args.Handled |= IsPositionInside(args.X, args.Y);
	}

	public override bool IsPositionInside(int posX, int posY)
	{
		if (!base.IsPositionInside(posX, posY))
		{
			if (listMenu.IsOpened)
			{
				return listMenu.IsPositionInside(posX, posY);
			}
			return false;
		}
		return true;
	}

	public override void OnMouseDown(ICoreClientAPI api, MouseEvent args)
	{
		if (enabled)
		{
			listMenu.OnMouseDown(api, args);
			if (!listMenu.IsOpened && IsPositionInside(args.X, args.Y) && !args.Handled)
			{
				listMenu.Open();
				api.Gui.PlaySound("menubutton");
				args.Handled = true;
			}
		}
	}

	public override void OnFocusLost()
	{
		base.OnFocusLost();
		listMenu.OnFocusLost();
	}

	/// <summary>
	/// Sets the current index to a newly selected index.
	/// </summary>
	/// <param name="selectedIndex">the index that is to be selected.</param>
	public void SetSelectedIndex(int selectedIndex)
	{
		listMenu.SetSelectedIndex(selectedIndex);
		ComposeCurrentValue();
	}

	/// <summary>
	/// Sets the current index to the value of the selected string.
	/// </summary>
	/// <param name="value">the string contained in the drop down.</param>
	public void SetSelectedValue(params string[] value)
	{
		listMenu.SetSelectedValue(value);
		ComposeCurrentValue();
	}

	/// <summary>
	/// Sets the values of the list with their corresponding names.
	/// </summary>
	/// <param name="values">The values of the list.</param>
	/// <param name="names">The names of the list.</param>
	public void SetList(string[] values, string[] names)
	{
		listMenu.SetList(values, names);
	}

	public override void Dispose()
	{
		base.Dispose();
		highlightTexture.Dispose();
		currentValueTexture.Dispose();
		listMenu?.Dispose();
		arrowDownButtonReleased.Dispose();
		arrowDownButtonPressed.Dispose();
	}
}
