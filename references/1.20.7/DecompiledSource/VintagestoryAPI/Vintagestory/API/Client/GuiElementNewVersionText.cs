using System;
using Cairo;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Vintagestory.API.Client;

internal class GuiElementNewVersionText : GuiElementTextBase
{
	private LoadedTexture texture;

	private LoadedTexture hoverTexture;

	public bool visible;

	public double offsetY;

	private int shadowHeight = 10;

	public Action<string> OnClicked;

	private string versionnumber;

	private double[] backColor = new double[4]
	{
		197.0 / 255.0,
		137.0 / 255.0,
		24.0 / 85.0,
		1.0
	};

	/// <summary>
	/// Creates a NewVersion text component.
	/// </summary>
	/// <param name="capi">The Client API</param>
	/// <param name="font">The font of the text.</param>
	/// <param name="bounds">The bounds of the component.</param>
	public GuiElementNewVersionText(ICoreClientAPI capi, CairoFont font, ElementBounds bounds)
		: base(capi, "", font, bounds)
	{
		texture = new LoadedTexture(capi);
		hoverTexture = new LoadedTexture(capi);
	}

	public override void ComposeTextElements(Context ctx, ImageSurface surface)
	{
	}

	/// <summary>
	/// Recomposes a multi-line message.
	/// </summary>
	/// <param name="versionnumber">The version number of the new version.</param>
	public void RecomposeMultiLine(string versionnumber)
	{
		RightPadding = (float)GuiElement.scaled(25.0);
		text = Lang.Get((RuntimeEnv.OS == OS.Windows) ? "versionavailable-autoupdate" : "versionavailable-manualupdate", versionnumber);
		Bounds.fixedHeight = GetMultilineTextHeight() / (double)RuntimeEnv.GUIScale;
		Bounds.CalcWorldBounds();
		offsetY = -2.0 * Bounds.fixedHeight;
		ImageSurface surface = new ImageSurface(Format.Argb32, (int)Bounds.OuterWidth, (int)Bounds.OuterHeight + shadowHeight);
		Context ctx = genContext(surface);
		double iconX = GuiElement.scaled(15.0);
		double iconSize = GuiElement.scaled(14.0);
		double iconY = (Bounds.InnerHeight - iconSize) / 2.0;
		double[] bgc = GuiStyle.DarkBrownColor;
		bgc[0] /= 2.0;
		bgc[1] /= 2.0;
		bgc[2] /= 2.0;
		LinearGradient gradient = new LinearGradient(0.0, Bounds.OuterHeightInt, 0.0, Bounds.OuterHeightInt + 10);
		gradient.AddColorStop(0.0, new Color(bgc[0], bgc[1], bgc[2], 1.0));
		gradient.AddColorStop(1.0, new Color(bgc[0], bgc[1], bgc[2], 0.0));
		ctx.SetSource(gradient);
		ctx.Rectangle(0.0, Bounds.OuterHeightInt, Bounds.OuterWidthInt, Bounds.OuterHeightInt + 10);
		ctx.Fill();
		gradient.Dispose();
		gradient = new LinearGradient(0.0, 0.0, Bounds.OuterWidth, 0.0);
		gradient.AddColorStop(0.0, new Color(backColor[0], backColor[1], backColor[2], 1.0));
		gradient.AddColorStop(0.99, new Color(backColor[0], backColor[1], backColor[2], 1.0));
		gradient.AddColorStop(1.0, new Color(backColor[0], backColor[1], backColor[2], 0.0));
		ctx.SetSource(gradient);
		ctx.Rectangle(0.0, 0.0, Bounds.OuterWidthInt, Bounds.OuterHeightInt);
		ctx.Fill();
		gradient.Dispose();
		ctx.Arc(Bounds.drawX + iconX, Bounds.OuterHeight / 2.0, iconSize / 2.0 + GuiElement.scaled(4.0), 0.0, Math.PI * 2.0);
		ctx.SetSourceRGBA(GuiStyle.DarkBrownColor);
		ctx.Fill();
		byte[] pngdata = api.Assets.Get("textures/gui/newversion.png").Data;
		BitmapExternal bitmap = api.Render.BitmapCreateFromPng(pngdata);
		surface.Image(bitmap, (int)(Bounds.drawX + iconX - iconSize / 2.0), (int)(Bounds.drawY + iconY), (int)iconSize, (int)iconSize);
		bitmap.Dispose();
		DrawMultilineTextAt(ctx, Bounds.drawX + iconX + 20.0, Bounds.drawY);
		generateTexture(surface, ref texture);
		ctx.Dispose();
		surface.Dispose();
		surface = new ImageSurface(Format.Argb32, (int)Bounds.OuterWidth, (int)Bounds.OuterHeight);
		ctx = genContext(surface);
		ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.1);
		ctx.Paint();
		generateTexture(surface, ref hoverTexture);
		ctx.Dispose();
		surface.Dispose();
	}

	internal void Activate(string versionnumber)
	{
		this.versionnumber = versionnumber;
		visible = true;
		RecomposeMultiLine(versionnumber);
		MouseOverCursor = "linkselect";
	}

	public override void RenderInteractiveElements(float deltaTime)
	{
		if (visible)
		{
			api.Render.Render2DTexturePremultipliedAlpha(texture.TextureId, (int)Bounds.renderX, (double)(int)Bounds.renderY + offsetY, (int)Bounds.OuterWidth, (int)Bounds.OuterHeight + shadowHeight);
			if (Bounds.PointInside(api.Input.MouseX, api.Input.MouseY))
			{
				api.Render.Render2DTexturePremultipliedAlpha(hoverTexture.TextureId, (int)Bounds.renderX, (double)(int)Bounds.renderY + offsetY, (int)Bounds.OuterWidth, (int)Bounds.OuterHeight);
			}
			offsetY = Math.Min(0.0, offsetY + (double)(100f * deltaTime));
		}
	}

	public override void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args)
	{
		base.OnMouseDownOnElement(api, args);
		if (visible && Bounds.PointInside(args.X, args.Y))
		{
			OnClicked(versionnumber);
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		texture?.Dispose();
		hoverTexture?.Dispose();
	}
}
