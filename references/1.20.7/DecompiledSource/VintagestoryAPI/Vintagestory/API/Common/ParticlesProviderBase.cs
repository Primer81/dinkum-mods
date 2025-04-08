using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.Common;

/// <summary>
/// A base class for all particle providers.
/// </summary>
public abstract class ParticlesProviderBase : IParticlePropertiesProvider
{
	public bool Async => false;

	public float Bounciness { get; set; }

	public bool RandomVelocityChange { get; set; }

	public bool DieOnRainHeightmap { get; set; }

	/// <summary>
	/// Determines whether or not the particle dies in liquid
	/// </summary>
	/// <returns></returns>
	public virtual bool DieInLiquid => false;

	public virtual bool SwimOnLiquid => false;

	/// <summary>
	/// Whether or not the particle dies in air.
	/// </summary>
	/// <returns></returns>
	public virtual bool DieInAir => false;

	/// <summary>
	/// Gets the quantity of particles.
	/// </summary>
	/// <returns></returns>
	public virtual float Quantity => 1f;

	/// <summary>
	/// Gets the position of particles.
	/// </summary>
	/// <returns></returns>
	public virtual Vec3d Pos => Vec3d.Zero;

	/// <summary>
	/// Gets the evolving opacity value of the particle.
	/// </summary>
	/// <returns>An evolving value based on opacity.</returns>
	public virtual EvolvingNatFloat OpacityEvolve => null;

	/// <summary>
	/// Gets the evolving red value of the particle.
	/// </summary>
	/// <returns></returns>
	public virtual EvolvingNatFloat RedEvolve => null;

	/// <summary>
	/// Gets the evolving green value of the particle.
	/// </summary>
	/// <returns></returns>
	public virtual EvolvingNatFloat GreenEvolve => null;

	/// <summary>
	/// Gets the evolving blue value of the particle.
	/// </summary>
	/// <returns></returns>
	public virtual EvolvingNatFloat BlueEvolve => null;

	/// <summary>
	/// Gets the model type of the particle.
	/// </summary>
	/// <returns></returns>
	public virtual EnumParticleModel ParticleModel => EnumParticleModel.Quad;

	/// <summary>
	/// gets the size of the particle.
	/// </summary>
	/// <returns></returns>
	public virtual float Size => 1f;

	/// <summary>
	/// gets the dynamic size of the particle.
	/// </summary>
	/// <returns></returns>
	public virtual EvolvingNatFloat SizeEvolve => null;

	/// <summary>
	/// Get the dynamic speeds of the particle.
	/// </summary>
	/// <returns></returns>
	public virtual EvolvingNatFloat[] VelocityEvolve => null;

	/// <summary>
	/// Gets the gravity effect on the particle.
	/// </summary>
	/// <returns></returns>
	public virtual float GravityEffect => 1f;

	/// <summary>
	/// gets the life length of the particle.
	/// </summary>
	/// <returns></returns>
	public virtual float LifeLength => 1f;

	/// <summary>
	/// gets the glow level of the particle.
	/// </summary>
	/// <returns></returns>
	public virtual int VertexFlags => 0;

	/// <summary>
	/// Whether or not the particle is self-propelled.
	/// </summary>
	/// <returns></returns>
	public virtual bool SelfPropelled => false;

	/// <summary>
	/// Whether or not the particle collides with the terrain or not.
	/// </summary>
	/// <returns></returns>
	public bool TerrainCollision => true;

	/// <summary>
	/// Gets the secondary particle spawn interval.
	/// </summary>
	/// <returns></returns>
	public virtual float SecondarySpawnInterval => 0f;

	/// <summary>
	/// Gets the secondary particle type for this particle.
	/// </summary>
	/// <returns></returns>
	public virtual IParticlePropertiesProvider[] SecondaryParticles => null;

	/// <summary>
	/// Gets the death particle for this type of particle.
	/// </summary>
	/// <returns></returns>
	public IParticlePropertiesProvider[] DeathParticles => null;

	public Vec3f ParentVelocity { get; set; }

	public bool WindAffected { get; set; }

	public float ParentVelocityWeight { get; set; }

	public int LightEmission { get; set; }

	/// <summary>
	/// Gets the velocity of the particles.
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public virtual Vec3f GetVelocity(Vec3d pos)
	{
		return Vec3f.Zero;
	}

	/// <summary>
	/// Gets the color of the particle.
	/// </summary>
	/// <param name="capi"></param>
	/// <returns></returns>
	public virtual int GetRgbaColor(ICoreClientAPI capi)
	{
		return -1;
	}

	/// <summary>
	/// How the particle is written to the save. (if it is)
	/// </summary>
	/// <param name="writer"></param>
	public virtual void ToBytes(BinaryWriter writer)
	{
	}

	/// <summary>
	/// How the particle is read from the save.
	/// </summary>
	/// <param name="reader"></param>
	/// <param name="resolver"></param>
	public virtual void FromBytes(BinaryReader reader, IWorldAccessor resolver)
	{
	}

	public virtual void BeginParticle()
	{
	}

	public virtual void PrepareForSecondarySpawn(ParticleBase particleInstance)
	{
	}

	public virtual void Init(ICoreAPI api)
	{
	}
}
