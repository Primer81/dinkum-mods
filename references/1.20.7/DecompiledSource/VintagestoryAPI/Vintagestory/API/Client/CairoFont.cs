using System;
using Cairo;
using Vintagestory.API.Config;

namespace Vintagestory.API.Client;

/// <summary>
/// Represent a font with sizing and styling for use in rendering text
/// </summary>
public class CairoFont : FontConfig, IDisposable
{
	private static ImageSurface surface;

	/// <summary>
	/// The static Context for all Cairo Fonts.
	/// </summary>
	public static Context FontMeasuringContext;

	/// <summary>
	/// Whether or not the font is rendered twice.
	/// </summary>
	public bool RenderTwice;

	public double LineHeightMultiplier = 1.0;

	private FontOptions CairoFontOptions;

	public FontSlant Slant;

	public EnumTextOrientation Orientation;

	static CairoFont()
	{
		surface = new ImageSurface(Format.Argb32, 1, 1);
		FontMeasuringContext = new Context(surface);
	}

	/// <summary>
	/// Creates an empty CairoFont instance.
	/// </summary>
	public CairoFont()
	{
	}

	/// <summary>
	/// Creates a pre-populated CairoFont instance.
	/// </summary>
	/// <param name="config">The configuration for the CairoFont</param>
	public CairoFont(FontConfig config)
	{
		UnscaledFontsize = config.UnscaledFontsize;
		Fontname = config.Fontname;
		FontWeight = config.FontWeight;
		Color = config.Color;
		StrokeColor = config.StrokeColor;
		StrokeWidth = config.StrokeWidth;
	}

	/// <summary>
	/// Creates a CairoFont object.
	/// </summary>
	/// <param name="unscaledFontSize">The size of the font before scaling is applied.</param>
	/// <param name="fontName">The name of the font.</param>
	public CairoFont(double unscaledFontSize, string fontName)
	{
		UnscaledFontsize = unscaledFontSize;
		Fontname = fontName;
	}

	public CairoFont WithLineHeightMultiplier(double lineHeightMul)
	{
		LineHeightMultiplier = lineHeightMul;
		return this;
	}

	public CairoFont WithStroke(double[] color, double width)
	{
		StrokeColor = color;
		StrokeWidth = width;
		return this;
	}

	/// <summary>
	/// Creates a CairoFont object
	/// </summary>
	/// <param name="unscaledFontSize">The size of the font before scaling is applied.</param>
	/// <param name="fontName">The name of the font.</param>
	/// <param name="color">The color of the font.</param>
	/// <param name="strokeColor">The color for the stroke of the font. (Default: Null)</param>
	public CairoFont(double unscaledFontSize, string fontName, double[] color, double[] strokeColor = null)
	{
		UnscaledFontsize = unscaledFontSize;
		Fontname = fontName;
		Color = color;
		StrokeColor = strokeColor;
		if (StrokeColor != null)
		{
			StrokeWidth = 1.0;
		}
	}

	/// <summary>
	/// Adjust font size so that it fits given bounds
	/// </summary>
	/// <param name="text">The text of the object.</param>
	/// <param name="bounds">The bounds of the element where the font is displayed.</param>
	/// <param name="onlyShrink"></param>
	public void AutoFontSize(string text, ElementBounds bounds, bool onlyShrink = true)
	{
		double origsize = UnscaledFontsize;
		UnscaledFontsize = 50.0;
		UnscaledFontsize *= (bounds.InnerWidth - 1.0) / GetTextExtents(text).Width;
		if (onlyShrink)
		{
			UnscaledFontsize = Math.Min(UnscaledFontsize, origsize);
		}
	}

	/// <summary>
	/// Adjust the bounds so that it fits given text in one line
	/// </summary>
	/// <param name="text">The text to adjust</param>
	/// <param name="bounds">The bounds to adjust the text to.</param>
	/// <param name="onlyGrow">If true, the box will not be made smaller</param>
	public void AutoBoxSize(string text, ElementBounds bounds, bool onlyGrow = false)
	{
		double textWidth = 0.0;
		double textHeight = 0.0;
		FontExtents fontExtents = GetFontExtents();
		if (text.Contains('\n'))
		{
			string[] lines = text.Split('\n');
			for (int i = 0; i < lines.Length && (lines[i].Length != 0 || i != lines.Length - 1); i++)
			{
				textWidth = Math.Max(textWidth, GetTextExtents(lines[i]).Width);
				textHeight += fontExtents.Height;
			}
		}
		else
		{
			textWidth = GetTextExtents(text).Width;
			textHeight = fontExtents.Height;
		}
		if (text.Length == 0)
		{
			textWidth = 0.0;
			textHeight = 0.0;
		}
		if (onlyGrow)
		{
			bounds.fixedWidth = Math.Max(bounds.fixedWidth, textWidth / (double)RuntimeEnv.GUIScale + 1.0);
			bounds.fixedHeight = Math.Max(bounds.fixedHeight, textHeight / (double)RuntimeEnv.GUIScale);
		}
		else
		{
			bounds.fixedWidth = Math.Max(1.0, textWidth / (double)RuntimeEnv.GUIScale + 1.0);
			bounds.fixedHeight = Math.Max(1.0, textHeight / (double)RuntimeEnv.GUIScale);
		}
	}

	/// <summary>
	/// Sets the color of the CairoFont.
	/// </summary>
	/// <param name="color">The color to set.</param>
	public CairoFont WithColor(double[] color)
	{
		Color = (double[])color.Clone();
		return this;
	}

	/// <summary>
	/// Adds a weight to the font.
	/// </summary>
	/// <param name="weight">The weight of the font.</param>
	public CairoFont WithWeight(FontWeight weight)
	{
		FontWeight = weight;
		return this;
	}

	/// <summary>
	/// Sets the font to render twice.
	/// </summary>
	public CairoFont WithRenderTwice()
	{
		RenderTwice = true;
		return this;
	}

	public CairoFont WithSlant(FontSlant slant)
	{
		Slant = slant;
		return this;
	}

	public CairoFont WithFont(string fontname)
	{
		Fontname = fontname;
		return this;
	}

	/// <summary>
	/// Sets up the context. Must be executed in the main thread, as it is not thread safe.
	/// </summary>
	/// <param name="ctx">The context to set up the CairoFont with.</param>
	public void SetupContext(Context ctx)
	{
		ctx.SetFontSize(GuiElement.scaled(UnscaledFontsize));
		ctx.SelectFontFace(Fontname, Slant, FontWeight);
		CairoFontOptions = new FontOptions();
		CairoFontOptions.Antialias = Antialias.Subpixel;
		ctx.FontOptions = CairoFontOptions;
		if (Color != null)
		{
			if (Color.Length == 3)
			{
				ctx.SetSourceRGB(Color[0], Color[1], Color[2]);
			}
			if (Color.Length == 4)
			{
				ctx.SetSourceRGBA(Color[0], Color[1], Color[2], Color[3]);
			}
		}
	}

	/// <summary>
	/// Gets the font's extents.
	/// </summary>
	/// <returns>The FontExtents for this particular font.</returns>
	public FontExtents GetFontExtents()
	{
		SetupContext(FontMeasuringContext);
		return FontMeasuringContext.FontExtents;
	}

	/// <summary>
	/// Gets the extents of the text.
	/// </summary>
	/// <param name="text">The text to extend.</param>
	/// <returns>The Text extends for this font with this text.</returns>
	public TextExtents GetTextExtents(string text)
	{
		SetupContext(FontMeasuringContext);
		return FontMeasuringContext.TextExtents(text);
	}

	/// <summary>
	/// Clone function.  Creates a duplicate of this Cairofont.
	/// </summary>
	/// <returns>The duplicate font.</returns>
	public CairoFont Clone()
	{
		CairoFont font = (CairoFont)MemberwiseClone();
		font.Color = new double[Color.Length];
		Array.Copy(Color, font.Color, Color.Length);
		return font;
	}

	/// <summary>
	/// Sets the base size of the CairoFont.
	/// </summary>
	/// <param name="fontSize">The new font size</param>
	public CairoFont WithFontSize(float fontSize)
	{
		UnscaledFontsize = fontSize;
		return this;
	}

	public static CairoFont SmallButtonText(EnumButtonStyle style = EnumButtonStyle.Normal)
	{
		return new CairoFont
		{
			Color = (double[])GuiStyle.ButtonTextColor.Clone(),
			FontWeight = ((style != EnumButtonStyle.Small) ? FontWeight.Bold : FontWeight.Normal),
			Orientation = EnumTextOrientation.Center,
			Fontname = GuiStyle.StandardFontName,
			UnscaledFontsize = GuiStyle.SmallFontSize
		};
	}

	/// <summary>
	/// Creates a Button Text preset.
	/// </summary>
	/// <returns>The button text preset.</returns>
	public static CairoFont ButtonText()
	{
		return new CairoFont
		{
			Color = (double[])GuiStyle.ButtonTextColor.Clone(),
			FontWeight = FontWeight.Bold,
			Orientation = EnumTextOrientation.Center,
			Fontname = GuiStyle.DecorativeFontName,
			UnscaledFontsize = 24.0
		};
	}

	/// <summary>
	/// Creates a text preset for when the button is pressed.
	/// </summary>
	/// <returns>The text preset for a pressed button.</returns>
	public static CairoFont ButtonPressedText()
	{
		return new CairoFont
		{
			Color = (double[])GuiStyle.ActiveButtonTextColor.Clone(),
			FontWeight = FontWeight.Bold,
			Fontname = GuiStyle.DecorativeFontName,
			Orientation = EnumTextOrientation.Center,
			UnscaledFontsize = 24.0
		};
	}

	public CairoFont WithOrientation(EnumTextOrientation orientation)
	{
		Orientation = orientation;
		return this;
	}

	/// <summary>
	/// Creates a text preset for text input fields.
	/// </summary>
	/// <returns>The text field input preset.</returns>
	public static CairoFont TextInput()
	{
		CairoFont cairoFont = new CairoFont();
		cairoFont.Color = new double[4] { 1.0, 1.0, 1.0, 0.9 };
		cairoFont.Fontname = GuiStyle.StandardFontName;
		cairoFont.UnscaledFontsize = 18.0;
		return cairoFont;
	}

	/// <summary>
	/// Creates a text oreset for smaller text input fields.
	/// </summary>
	/// <returns>The smaller text input preset.</returns>
	public static CairoFont SmallTextInput()
	{
		CairoFont cairoFont = new CairoFont();
		cairoFont.Color = new double[4] { 0.0, 0.0, 0.0, 0.9 };
		cairoFont.Fontname = GuiStyle.StandardFontName;
		cairoFont.UnscaledFontsize = GuiStyle.SmallFontSize;
		return cairoFont;
	}

	/// <summary>
	/// Creates a white text for medium dialog.
	/// </summary>
	/// <returns>The white text for medium dialog.</returns>
	public static CairoFont WhiteMediumText()
	{
		return new CairoFont
		{
			Color = (double[])GuiStyle.DialogDefaultTextColor.Clone(),
			Fontname = GuiStyle.StandardFontName,
			UnscaledFontsize = GuiStyle.NormalFontSize
		};
	}

	/// <summary>
	/// Creates a white text for smallish dialogs.
	/// </summary>
	/// <returns>The white text for small dialogs.</returns>
	public static CairoFont WhiteSmallishText()
	{
		return new CairoFont
		{
			Color = (double[])GuiStyle.DialogDefaultTextColor.Clone(),
			Fontname = GuiStyle.StandardFontName,
			UnscaledFontsize = GuiStyle.SmallishFontSize
		};
	}

	/// <summary>
	/// Creates a white text for smallish dialogs, using the specified base font
	/// </summary>
	/// <param name="baseFont"></param>
	/// <returns></returns>
	public static CairoFont WhiteSmallishText(string baseFont)
	{
		return new CairoFont
		{
			Color = (double[])GuiStyle.DialogDefaultTextColor.Clone(),
			Fontname = baseFont,
			UnscaledFontsize = GuiStyle.SmallishFontSize
		};
	}

	/// <summary>
	/// Creates a white text for small dialogs.
	/// </summary>
	/// <returns>The white text for small dialogs</returns>
	public static CairoFont WhiteSmallText()
	{
		return new CairoFont
		{
			Color = (double[])GuiStyle.DialogDefaultTextColor.Clone(),
			Fontname = GuiStyle.StandardFontName,
			UnscaledFontsize = GuiStyle.SmallFontSize
		};
	}

	/// <summary>
	/// Creates a white text for details.
	/// </summary>
	/// <returns>A white text for details.</returns>
	public static CairoFont WhiteDetailText()
	{
		return new CairoFont
		{
			Color = (double[])GuiStyle.DialogDefaultTextColor.Clone(),
			Fontname = GuiStyle.StandardFontName,
			UnscaledFontsize = GuiStyle.DetailFontSize
		};
	}

	public void Dispose()
	{
		CairoFontOptions.Dispose();
	}
}
