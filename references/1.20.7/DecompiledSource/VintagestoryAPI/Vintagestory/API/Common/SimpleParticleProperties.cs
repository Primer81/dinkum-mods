using System;
using System.IO;
using System.Threading;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.Common;

/// <summary>
/// A configurable implementation of IParticlePropertiesProvider
/// </summary>
public class SimpleParticleProperties : IParticlePropertiesProvider
{
	public static ThreadLocal<Random> randTL = new ThreadLocal<Random>(() => new Random());

	public float MinQuantity;

	public float AddQuantity;

	public float WindAffectednes;

	public Vec3d MinPos;

	public Vec3d AddPos = new Vec3d();

	public Vec3f MinVelocity = new Vec3f();

	public Vec3f AddVelocity = new Vec3f();

	public float LifeLength;

	public float addLifeLength;

	public float MinSize = 1f;

	public float MaxSize = 1f;

	public int Color;

	public bool SelfPropelled;

	public Block ColorByBlock;

	/// <summary>
	/// The color map for climate color mapping. Leave null for no coloring by climate
	/// </summary>
	public string ClimateColorMap;

	/// <summary>
	/// The color map for season color mapping. Leave null for no coloring by season
	/// </summary>
	public string SeasonColorMap;

	protected Vec3d tmpPos = new Vec3d();

	private Vec3f tmpVelo = new Vec3f();

	public static Random rand => randTL.Value;

	public Vec3f ParentVelocity { get; set; }

	public float ParentVelocityWeight { get; set; }

	public float GravityEffect { get; set; }

	public int LightEmission { get; set; }

	public int VertexFlags { get; set; }

	public bool Async { get; set; }

	public float Bounciness { get; set; }

	public bool ShouldDieInAir { get; set; }

	public bool ShouldDieInLiquid { get; set; }

	public bool ShouldSwimOnLiquid { get; set; }

	public bool WithTerrainCollision { get; set; } = true;


	public EvolvingNatFloat OpacityEvolve { get; set; }

	public EvolvingNatFloat RedEvolve { get; set; }

	public EvolvingNatFloat GreenEvolve { get; set; }

	public EvolvingNatFloat BlueEvolve { get; set; }

	public EvolvingNatFloat SizeEvolve { get; set; }

	public bool RandomVelocityChange { get; set; }

	public bool DieInAir => ShouldDieInAir;

	public bool DieInLiquid => ShouldDieInLiquid;

	public bool SwimOnLiquid => ShouldSwimOnLiquid;

	public float Quantity => MinQuantity + (float)rand.NextDouble() * AddQuantity;

	public virtual Vec3d Pos
	{
		get
		{
			tmpPos.Set(MinPos.X + AddPos.X * rand.NextDouble(), MinPos.Y + AddPos.Y * rand.NextDouble(), MinPos.Z + AddPos.Z * rand.NextDouble());
			return tmpPos;
		}
	}

	public float Size => MinSize + (float)rand.NextDouble() * (MaxSize - MinSize);

	float IParticlePropertiesProvider.LifeLength => LifeLength + addLifeLength * (float)rand.NextDouble();

	public EnumParticleModel ParticleModel { get; set; }

	public EvolvingNatFloat[] VelocityEvolve => null;

	bool IParticlePropertiesProvider.SelfPropelled => SelfPropelled;

	public bool TerrainCollision => WithTerrainCollision;

	public IParticlePropertiesProvider[] SecondaryParticles { get; set; }

	public IParticlePropertiesProvider[] DeathParticles { get; set; }

	public float SecondarySpawnInterval => 0f;

	public bool DieOnRainHeightmap { get; set; }

	public bool WindAffected { get; set; }

	public SimpleParticleProperties()
	{
	}

	public void Init(ICoreAPI api)
	{
	}

	public SimpleParticleProperties(float minQuantity, float maxQuantity, int color, Vec3d minPos, Vec3d maxPos, Vec3f minVelocity, Vec3f maxVelocity, float lifeLength = 1f, float gravityEffect = 1f, float minSize = 1f, float maxSize = 1f, EnumParticleModel model = EnumParticleModel.Cube)
	{
		MinQuantity = minQuantity;
		AddQuantity = maxQuantity - minQuantity;
		Color = color;
		MinPos = minPos;
		AddPos = maxPos - minPos;
		MinVelocity = minVelocity;
		AddVelocity = maxVelocity - minVelocity;
		LifeLength = lifeLength;
		GravityEffect = gravityEffect;
		MinSize = minSize;
		MaxSize = maxSize;
		ParticleModel = model;
	}

	public Vec3f GetVelocity(Vec3d pos)
	{
		tmpVelo.Set(MinVelocity.X + AddVelocity.X * (float)rand.NextDouble(), MinVelocity.Y + AddVelocity.Y * (float)rand.NextDouble(), MinVelocity.Z + AddVelocity.Z * (float)rand.NextDouble());
		return tmpVelo;
	}

	public int GetRgbaColor(ICoreClientAPI capi)
	{
		if (ColorByBlock != null)
		{
			return ColorByBlock.GetRandomColor(capi, new ItemStack(ColorByBlock));
		}
		if (SeasonColorMap != null || ClimateColorMap != null)
		{
			return capi.World.ApplyColorMapOnRgba(ClimateColorMap, SeasonColorMap, Color, (int)MinPos.X, (int)MinPos.Y, (int)MinPos.Z);
		}
		return Color;
	}

	public bool UseLighting()
	{
		return true;
	}

	public void ToBytes(BinaryWriter writer)
	{
		writer.Write(MinQuantity);
		writer.Write(AddQuantity);
		MinPos.ToBytes(writer);
		AddPos.ToBytes(writer);
		MinVelocity.ToBytes(writer);
		AddVelocity.ToBytes(writer);
		writer.Write(LifeLength);
		writer.Write(GravityEffect);
		writer.Write(MinSize);
		writer.Write(MaxSize);
		writer.Write(Color);
		writer.Write(VertexFlags);
		writer.Write((int)ParticleModel);
		writer.Write(ShouldDieInAir);
		writer.Write(ShouldDieInLiquid);
		writer.Write(OpacityEvolve == null);
		if (OpacityEvolve != null)
		{
			OpacityEvolve.ToBytes(writer);
		}
		writer.Write(RedEvolve == null);
		if (RedEvolve != null)
		{
			RedEvolve.ToBytes(writer);
		}
		writer.Write(GreenEvolve == null);
		if (GreenEvolve != null)
		{
			GreenEvolve.ToBytes(writer);
		}
		writer.Write(BlueEvolve == null);
		if (BlueEvolve != null)
		{
			BlueEvolve.ToBytes(writer);
		}
		writer.Write(SizeEvolve == null);
		if (SizeEvolve != null)
		{
			SizeEvolve.ToBytes(writer);
		}
		writer.Write(SelfPropelled);
		writer.Write(ColorByBlock == null);
		if (ColorByBlock != null)
		{
			writer.Write(ColorByBlock.BlockId);
		}
		writer.Write(ClimateColorMap == null);
		if (ClimateColorMap != null)
		{
			writer.Write(ClimateColorMap);
		}
		writer.Write(SeasonColorMap == null);
		if (SeasonColorMap != null)
		{
			writer.Write(SeasonColorMap);
		}
		writer.Write(Bounciness);
		writer.Write(Async);
	}

	public void FromBytes(BinaryReader reader, IWorldAccessor resolver)
	{
		MinQuantity = reader.ReadSingle();
		AddQuantity = reader.ReadSingle();
		MinPos = Vec3d.CreateFromBytes(reader);
		AddPos = Vec3d.CreateFromBytes(reader);
		MinVelocity = Vec3f.CreateFromBytes(reader);
		AddVelocity = Vec3f.CreateFromBytes(reader);
		LifeLength = reader.ReadSingle();
		GravityEffect = reader.ReadSingle();
		MinSize = reader.ReadSingle();
		MaxSize = reader.ReadSingle();
		Color = reader.ReadInt32();
		VertexFlags = reader.ReadInt32();
		ParticleModel = (EnumParticleModel)reader.ReadInt32();
		ShouldDieInAir = reader.ReadBoolean();
		ShouldDieInLiquid = reader.ReadBoolean();
		if (!reader.ReadBoolean())
		{
			OpacityEvolve = EvolvingNatFloat.CreateFromBytes(reader);
		}
		if (!reader.ReadBoolean())
		{
			RedEvolve = EvolvingNatFloat.CreateFromBytes(reader);
		}
		if (!reader.ReadBoolean())
		{
			GreenEvolve = EvolvingNatFloat.CreateFromBytes(reader);
		}
		if (!reader.ReadBoolean())
		{
			BlueEvolve = EvolvingNatFloat.CreateFromBytes(reader);
		}
		if (!reader.ReadBoolean())
		{
			SizeEvolve = EvolvingNatFloat.CreateFromBytes(reader);
		}
		SelfPropelled = reader.ReadBoolean();
		if (!reader.ReadBoolean())
		{
			ColorByBlock = resolver.Blocks[reader.ReadInt32()];
		}
		if (!reader.ReadBoolean())
		{
			ClimateColorMap = reader.ReadString();
		}
		if (!reader.ReadBoolean())
		{
			SeasonColorMap = reader.ReadString();
		}
		Bounciness = reader.ReadSingle();
		Async = reader.ReadBoolean();
	}

	public void BeginParticle()
	{
		if (WindAffectednes > 0f)
		{
			ParentVelocityWeight = WindAffectednes;
			ParentVelocity = GlobalConstants.CurrentWindSpeedClient;
		}
	}

	public void PrepareForSecondarySpawn(ParticleBase particleInstance)
	{
		Vec3d particlePos = particleInstance.Position;
		MinPos.X = particlePos.X;
		MinPos.Y = particlePos.Y;
		MinPos.Z = particlePos.Z;
	}

	public SimpleParticleProperties Clone(IWorldAccessor worldForResovle)
	{
		SimpleParticleProperties cloned = new SimpleParticleProperties();
		using MemoryStream ms = new MemoryStream();
		using (BinaryWriter writer = new BinaryWriter(ms))
		{
			ToBytes(writer);
		}
		ms.Position = 0L;
		using BinaryReader reader = new BinaryReader(ms);
		cloned.FromBytes(reader, worldForResovle);
		return cloned;
	}
}
