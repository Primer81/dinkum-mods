using System;
using Cairo;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.Client;

public class GuiElementSlider : GuiElementControl
{
	private int minValue;

	private int maxValue = 100;

	private int step = 1;

	private string unit = "";

	private int currentValue;

	private int alarmValue;

	private bool mouseDownOnSlider;

	private bool mouseOnSlider;

	private bool triggerOnMouseUp;

	private bool didChangeValue;

	private LoadedTexture handleTexture;

	private LoadedTexture hoverTextTexture;

	private LoadedTexture waterTexture;

	private LoadedTexture alarmValueTexture;

	private GuiElementStaticText textElem;

	private Rectangled alarmTextureRect;

	private ActionConsumable<int> onNewSliderValue;

	public SliderTooltipDelegate OnSliderTooltip;

	internal const int unscaledHeight = 20;

	internal const int unscaledPadding = 4;

	private int unscaledHandleWidth = 15;

	private int unscaledHandleHeight = 35;

	private int unscaledHoverTextHeight = 50;

	private double handleWidth;

	private double handleHeight;

	private double hoverTextWidth;

	private double hoverTextHeight;

	private double padding;

	public override bool Focusable => true;

	/// <summary>
	/// Builds a slider.  A horizontal customizeable slider.
	/// </summary>
	/// <param name="capi">The Client API</param>
	/// <param name="onNewSliderValue">The event that's fired when the slider changed.</param>
	/// <param name="bounds">the bounds of the object.</param>
	public GuiElementSlider(ICoreClientAPI capi, ActionConsumable<int> onNewSliderValue, ElementBounds bounds)
		: base(capi, bounds)
	{
		handleTexture = new LoadedTexture(capi);
		hoverTextTexture = new LoadedTexture(capi);
		waterTexture = new LoadedTexture(capi);
		alarmValueTexture = new LoadedTexture(capi);
		this.onNewSliderValue = onNewSliderValue;
	}

	public override void ComposeElements(Context ctxStatic, ImageSurface surfaceStatic)
	{
		handleWidth = GuiElement.scaled(unscaledHandleWidth) * Scale;
		handleHeight = GuiElement.scaled(unscaledHandleHeight) * Scale;
		hoverTextWidth = GuiElement.scaled(unscaledHoverTextHeight);
		hoverTextHeight = GuiElement.scaled(unscaledHoverTextHeight);
		padding = GuiElement.scaled(4.0) * Scale;
		Bounds.CalcWorldBounds();
		ctxStatic.SetSourceRGBA(0.0, 0.0, 0.0, 0.2);
		GuiElement.RoundRectangle(ctxStatic, Bounds.drawX, Bounds.drawY, Bounds.InnerWidth, Bounds.InnerHeight, 1.0);
		ctxStatic.Fill();
		EmbossRoundRectangleElement(ctxStatic, Bounds, inverse: true, 1, 1);
		_ = Bounds.InnerWidth;
		_ = padding;
		_ = Bounds.InnerHeight;
		_ = padding;
		ImageSurface surface = new ImageSurface(Format.Argb32, (int)handleWidth + 4, (int)handleHeight + 4);
		Context ctx = genContext(surface);
		ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.0);
		ctx.Paint();
		GuiElement.RoundRectangle(ctx, 2.0, 2.0, handleWidth, handleHeight, 1.0);
		GuiElement.fillWithPattern(api, ctx, GuiElement.woodTextureName, nearestScalingFiler: false, preserve: true, 255, 0.5f);
		ctx.SetSourceRGB(43.0 / 255.0, 11.0 / 85.0, 8.0 / 85.0);
		ctx.LineWidth = 2.0;
		ctx.Stroke();
		generateTexture(surface, ref handleTexture);
		ctx.Dispose();
		surface.Dispose();
		ComposeWaterTexture();
		ComposeHoverTextElement();
	}

	internal void ComposeHoverTextElement()
	{
		ElementBounds bounds = new ElementBounds().WithFixedPadding(7.0).WithParent(ElementBounds.Empty);
		string text = currentValue + unit;
		if (OnSliderTooltip != null)
		{
			text = OnSliderTooltip(currentValue);
		}
		textElem = new GuiElementStaticText(api, text, EnumTextOrientation.Center, bounds, CairoFont.WhiteMediumText().WithFontSize((float)GuiStyle.SubNormalFontSize));
		textElem.Font.UnscaledFontsize = GuiStyle.SmallishFontSize;
		textElem.AutoBoxSize();
		textElem.Bounds.CalcWorldBounds();
		ImageSurface surface = new ImageSurface(Format.Argb32, (int)bounds.OuterWidth, (int)bounds.OuterHeight);
		Context ctx = genContext(surface);
		ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.0);
		ctx.Paint();
		ctx.SetSourceRGBA(GuiStyle.DialogStrongBgColor);
		GuiElement.RoundRectangle(ctx, 0.0, 0.0, bounds.OuterWidth, bounds.OuterHeight, GuiStyle.ElementBGRadius);
		ctx.FillPreserve();
		double[] color = GuiStyle.DialogStrongBgColor;
		ctx.SetSourceRGBA(color[0] / 2.0, color[1] / 2.0, color[2] / 2.0, color[3]);
		ctx.Stroke();
		textElem.ComposeElements(ctx, surface);
		generateTexture(surface, ref hoverTextTexture);
		ctx.Dispose();
		surface.Dispose();
	}

	internal void ComposeWaterTexture()
	{
		double handlePosition = (Bounds.InnerWidth - 2.0 * padding - handleWidth / 2.0) * (1.0 * (double)currentValue - (double)minValue) / (double)(maxValue - minValue);
		double insetHeight = Bounds.InnerHeight - 2.0 * padding;
		ImageSurface surface = new ImageSurface(Format.Argb32, (int)(handlePosition + 5.0), (int)insetHeight);
		Context context = genContext(surface);
		SurfacePattern pattern = GuiElement.getPattern(api, GuiElement.waterTextureName, doCache: true, 255, 0.5f);
		GuiElement.RoundRectangle(context, 0.0, 0.0, surface.Width, surface.Height, 1.0);
		context.SetSource(pattern);
		context.Fill();
		generateTexture(surface, ref waterTexture);
		context.Dispose();
		surface.Dispose();
	}

	public override void RenderInteractiveElements(float deltaTime)
	{
		if ((float)(alarmValue - minValue) / (float)(maxValue - minValue) > 0f && alarmValueTexture.TextureId > 0)
		{
			_ = (float)alarmValue / (float)maxValue;
			api.Render.RenderTexture(alarmValueTexture.TextureId, Bounds.renderX + alarmTextureRect.X, Bounds.renderY + alarmTextureRect.Y, alarmTextureRect.Width, alarmTextureRect.Height);
		}
		double handlePosition = (Bounds.InnerWidth - 2.0 * padding - handleWidth / 2.0) * (1.0 * (double)currentValue - (double)minValue) / (double)(maxValue - minValue);
		double insetHeight = Bounds.InnerHeight - 2.0 * padding;
		double dy = (handleHeight - Bounds.OuterHeight + padding) / 2.0;
		api.Render.Render2DTexturePremultipliedAlpha(waterTexture.TextureId, Bounds.renderX + padding, Bounds.renderY + padding, (int)(handlePosition + 5.0), (int)insetHeight);
		if (Enabled)
		{
			api.Render.Render2DTexturePremultipliedAlpha(handleTexture.TextureId, Bounds.renderX + handlePosition, Bounds.renderY - dy, (int)handleWidth + 4, (int)handleHeight + 4);
		}
		if (mouseDownOnSlider || mouseOnSlider)
		{
			ElementBounds elemBounds = textElem.Bounds;
			api.Render.Render2DTexturePremultipliedAlpha(hoverTextTexture.TextureId, (int)(Bounds.renderX + padding + handlePosition - elemBounds.OuterWidth / 2.0 + handleWidth / 2.0), (int)(Bounds.renderY - GuiElement.scaled(20.0) - elemBounds.OuterHeight), elemBounds.OuterWidthInt, elemBounds.OuterHeightInt, 300f);
		}
	}

	private void MakeAlarmValueTexture()
	{
		float alarmValueRel = (float)(alarmValue - minValue) / (float)(maxValue - minValue);
		alarmTextureRect = new Rectangled
		{
			X = padding + (Bounds.InnerWidth - 2.0 * padding) * (double)alarmValueRel,
			Y = padding,
			Width = (Bounds.InnerWidth - 2.0 * padding) * (double)(1f - alarmValueRel),
			Height = Bounds.InnerHeight - 2.0 * padding
		};
		ImageSurface surface = new ImageSurface(Format.Argb32, (int)alarmTextureRect.Width, (int)alarmTextureRect.Height);
		Context context = genContext(surface);
		context.SetSourceRGBA(1.0, 0.0, 1.0, 0.4);
		GuiElement.RoundRectangle(context, 0.0, 0.0, alarmTextureRect.Width, alarmTextureRect.Height, GuiStyle.ElementBGRadius);
		context.Fill();
		generateTexture(surface, ref alarmValueTexture.TextureId);
		context.Dispose();
		surface.Dispose();
	}

	public override void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args)
	{
		if (enabled && Bounds.PointInside(api.Input.MouseX, api.Input.MouseY))
		{
			args.Handled = updateValue(api.Input.MouseX);
			mouseDownOnSlider = true;
		}
	}

	public override void OnMouseUp(ICoreClientAPI api, MouseEvent args)
	{
		mouseDownOnSlider = false;
		if (enabled)
		{
			if (onNewSliderValue != null && didChangeValue && triggerOnMouseUp)
			{
				onNewSliderValue(currentValue);
			}
			didChangeValue = false;
		}
	}

	public override void OnMouseMove(ICoreClientAPI api, MouseEvent args)
	{
		mouseOnSlider = Bounds.PointInside(api.Input.MouseX, api.Input.MouseY);
		if (enabled && mouseDownOnSlider)
		{
			args.Handled = updateValue(api.Input.MouseX);
		}
	}

	public override void OnMouseWheel(ICoreClientAPI api, MouseWheelEventArgs args)
	{
		if (enabled && Bounds.PointInside(api.Input.MouseX, api.Input.MouseY))
		{
			args.SetHandled();
			int dir = Math.Sign(args.deltaPrecise);
			currentValue = Math.Max(minValue, Math.Min(maxValue, currentValue + dir * step));
			ComposeHoverTextElement();
			ComposeWaterTexture();
			onNewSliderValue?.Invoke(currentValue);
		}
	}

	/// <summary>
	/// Trigger event only once user release the mouse
	/// </summary>
	/// <param name="trigger"></param>
	internal void TriggerOnlyOnMouseUp(bool trigger = true)
	{
		triggerOnMouseUp = trigger;
	}

	private bool updateValue(int mouseX)
	{
		double sliderWidth = Bounds.InnerWidth - 2.0 * padding - handleWidth / 2.0;
		double mouseDeltaX = GameMath.Clamp((double)mouseX - Bounds.renderX - padding, 0.0, sliderWidth);
		double value = (double)minValue + (double)(maxValue - minValue) * mouseDeltaX / sliderWidth;
		int newValue = Math.Max(minValue, Math.Min(maxValue, step * (int)Math.Round(1.0 * value / (double)step)));
		bool didChangeNow = newValue != currentValue;
		if (didChangeNow)
		{
			didChangeValue = true;
		}
		currentValue = newValue;
		ComposeHoverTextElement();
		if (onNewSliderValue != null)
		{
			ComposeWaterTexture();
			if (!triggerOnMouseUp && didChangeNow)
			{
				return onNewSliderValue(currentValue);
			}
		}
		return true;
	}

	/// <summary>
	/// Sets a value to warn the player that going over this is not a good idea.
	/// </summary>
	/// <param name="value">The maximum limit before things break down.</param>
	public void SetAlarmValue(int value)
	{
		alarmValue = value;
		MakeAlarmValueTexture();
	}

	/// <summary>
	/// Sets the values of the slider.
	/// </summary>
	/// <param name="currentValue">The value the slider is now.</param>
	/// <param name="minValue">The lowest value.</param>
	/// <param name="maxValue">The highest value.</param>
	/// <param name="step">Each step between values.</param>
	/// <param name="unit">The units of the value. %, chunks, ect.</param>
	public void SetValues(int currentValue, int minValue, int maxValue, int step, string unit = "")
	{
		this.currentValue = currentValue;
		this.minValue = minValue;
		this.maxValue = maxValue;
		this.step = step;
		this.unit = unit;
		ComposeHoverTextElement();
		ComposeWaterTexture();
	}

	public void SetValue(int currentValue)
	{
		this.currentValue = currentValue;
	}

	/// <summary>
	/// Gets the current value of the slider.
	/// </summary>
	public int GetValue()
	{
		return currentValue;
	}

	public override void Dispose()
	{
		base.Dispose();
		handleTexture.Dispose();
		hoverTextTexture.Dispose();
		waterTexture.Dispose();
		alarmValueTexture.Dispose();
	}
}
