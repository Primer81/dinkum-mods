using System;
using Newtonsoft.Json;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Vintagestory.GameContent;

[JsonObject(MemberSerialization.OptIn)]
public class PositionVicinityCondition : IActionCondition, IStorableTypedComponent
{
	[JsonProperty]
	private float range;

	[JsonProperty]
	private float yrange = -1f;

	public Vec3d Target = new Vec3d();

	protected EntityActivitySystem vas;

	private Vec3d tmpPos = new Vec3d();

	[JsonProperty]
	public bool Invert { get; set; }

	[JsonProperty]
	public double targetX
	{
		get
		{
			return Target.X;
		}
		set
		{
			Target.X = value;
		}
	}

	[JsonProperty]
	public double targetY
	{
		get
		{
			return Target.Y;
		}
		set
		{
			Target.Y = value;
		}
	}

	[JsonProperty]
	public double targetZ
	{
		get
		{
			return Target.Z;
		}
		set
		{
			Target.Z = value;
		}
	}

	public string Type => "positionvicinity";

	public PositionVicinityCondition()
	{
	}

	public PositionVicinityCondition(EntityActivitySystem vas, Vec3d pos, float range, float yrange, bool invert = false)
	{
		this.vas = vas;
		Target = pos;
		this.range = range;
		this.yrange = yrange;
		Invert = invert;
	}

	public virtual bool ConditionSatisfied(Entity e)
	{
		tmpPos.Set(Target).Add(vas.ActivityOffset);
		if (yrange >= 0f)
		{
			if (e.ServerPos.HorDistanceTo(tmpPos) < (double)range)
			{
				return Math.Abs(e.ServerPos.Y - tmpPos.Y) < (double)yrange;
			}
			return false;
		}
		return e.ServerPos.DistanceTo(tmpPos) < (double)range;
	}

	public void LoadState(ITreeAttribute tree)
	{
	}

	public void StoreState(ITreeAttribute tree)
	{
	}

	public void AddGuiEditFields(ICoreClientAPI capi, GuiComposer singleComposer)
	{
		ElementBounds bc = ElementBounds.Fixed(0.0, 0.0, 65.0, 20.0);
		ElementBounds b = ElementBounds.Fixed(0.0, 0.0, 200.0, 20.0);
		singleComposer.AddStaticText("x/y/z Pos", CairoFont.WhiteDetailText(), bc).AddTextInput(bc = bc.BelowCopy(), null, CairoFont.WhiteDetailText(), "x").AddTextInput(bc = bc.CopyOffsetedSibling(70.0), null, CairoFont.WhiteDetailText(), "y")
			.AddTextInput(bc = bc.CopyOffsetedSibling(70.0), null, CairoFont.WhiteDetailText(), "z")
			.AddSmallButton("Tp to", () => onClickTpTo(capi), bc = bc.CopyOffsetedSibling(70.0), EnumButtonStyle.Small)
			.AddSmallButton("Insert Player Pos", () => onClickPlayerPos(capi, singleComposer), b = b.FlatCopy().FixedUnder(bc), EnumButtonStyle.Small)
			.AddStaticText("Range", CairoFont.WhiteDetailText(), b = b.BelowCopy(0.0, 10.0))
			.AddNumberInput(b = b.BelowCopy(0.0, -5.0), null, CairoFont.WhiteDetailText(), "range")
			.AddStaticText("Vertical Range (-1 to ignore)", CairoFont.WhiteDetailText(), b = b.BelowCopy(0.0, 10.0))
			.AddNumberInput(b = b.BelowCopy(0.0, -5.0), null, CairoFont.WhiteDetailText(), "vrange");
		singleComposer.GetNumberInput("range").SetValue(range);
		singleComposer.GetNumberInput("vrange").SetValue(yrange);
		GuiComposer composer = singleComposer;
		composer.GetTextInput("x").SetValue((Target?.X).ToString() ?? "");
		composer.GetTextInput("y").SetValue((Target?.Y).ToString() ?? "");
		composer.GetTextInput("z").SetValue((Target?.Z).ToString() ?? "");
	}

	private bool onClickTpTo(ICoreClientAPI capi)
	{
		double x = Target.X;
		double y = Target.Y;
		double z = Target.Z;
		if (vas != null)
		{
			x += (double)vas.ActivityOffset.X;
			y += (double)vas.ActivityOffset.Y;
			z += (double)vas.ActivityOffset.Z;
		}
		capi.SendChatMessage($"/tp ={x} ={y} ={z}");
		return false;
	}

	private bool onClickPlayerPos(ICoreClientAPI capi, GuiComposer singleComposer)
	{
		Vec3d plrPos = capi.World.Player.Entity.Pos.XYZ;
		singleComposer.GetTextInput("x").SetValue(Math.Round(plrPos.X, 1).ToString() ?? "");
		singleComposer.GetTextInput("y").SetValue(Math.Round(plrPos.Y, 1).ToString() ?? "");
		singleComposer.GetTextInput("z").SetValue(Math.Round(plrPos.Z, 1).ToString() ?? "");
		return true;
	}

	public void StoreGuiEditFields(ICoreClientAPI capi, GuiComposer s)
	{
		Target = new Vec3d(s.GetTextInput("x").GetText().ToDouble(), s.GetTextInput("y").GetText().ToDouble(), s.GetTextInput("z").GetText().ToDouble());
		range = s.GetNumberInput("range").GetValue();
		yrange = s.GetNumberInput("vrange").GetValue();
	}

	public IActionCondition Clone()
	{
		return new PositionVicinityCondition(vas, Target, range, yrange, Invert);
	}

	public override string ToString()
	{
		return (Invert ? "When not near pos " : "When near pos ") + Target;
	}

	public void OnLoaded(EntityActivitySystem vas)
	{
		this.vas = vas;
	}
}
