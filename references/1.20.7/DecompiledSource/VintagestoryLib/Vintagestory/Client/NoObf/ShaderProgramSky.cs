using Vintagestory.API.MathTools;

namespace Vintagestory.Client.NoObf;

public class ShaderProgramSky : ShaderProgram
{
	public float FogDensityIn
	{
		set
		{
			Uniform("fogDensityIn", value);
		}
	}

	public float FogMinIn
	{
		set
		{
			Uniform("fogMinIn", value);
		}
	}

	public float DayLight
	{
		set
		{
			Uniform("dayLight", value);
		}
	}

	public float HorizonFog
	{
		set
		{
			Uniform("horizonFog", value);
		}
	}

	public Vec3f PlayerPos
	{
		set
		{
			Uniform("playerPos", value);
		}
	}

	public Vec3f SunPosition
	{
		set
		{
			Uniform("sunPosition", value);
		}
	}

	public float FlatFogDensity
	{
		set
		{
			Uniform("flatFogDensity", value);
		}
	}

	public float FlatFogStart
	{
		set
		{
			Uniform("flatFogStart", value);
		}
	}

	public float ViewDistance
	{
		set
		{
			Uniform("viewDistance", value);
		}
	}

	public float ViewDistanceLod0
	{
		set
		{
			Uniform("viewDistanceLod0", value);
		}
	}

	public float ZNear
	{
		set
		{
			Uniform("zNear", value);
		}
	}

	public float ZFar
	{
		set
		{
			Uniform("zFar", value);
		}
	}

	public Vec3f LightPosition
	{
		set
		{
			Uniform("lightPosition", value);
		}
	}

	public float ShadowIntensity
	{
		set
		{
			Uniform("shadowIntensity", value);
		}
	}

	public int ShadowMapFar2D
	{
		set
		{
			BindTexture2D("shadowMapFar", value, 0);
		}
	}

	public float ShadowMapWidthInv
	{
		set
		{
			Uniform("shadowMapWidthInv", value);
		}
	}

	public float ShadowMapHeightInv
	{
		set
		{
			Uniform("shadowMapHeightInv", value);
		}
	}

	public int ShadowMapNear2D
	{
		set
		{
			BindTexture2D("shadowMapNear", value, 1);
		}
	}

	public float WindWaveCounter
	{
		set
		{
			Uniform("windWaveCounter", value);
		}
	}

	public float GlitchStrength
	{
		set
		{
			Uniform("glitchStrength", value);
		}
	}

	public float[] FogSpheres
	{
		set
		{
			Uniform("fogSpheres", value.Length, value);
		}
	}

	public int FogSphereQuantity
	{
		set
		{
			Uniform("fogSphereQuantity", value);
		}
	}

	public float PlayerToSealevelOffset
	{
		set
		{
			Uniform("playerToSealevelOffset", value);
		}
	}

	public int DitherSeed
	{
		set
		{
			Uniform("ditherSeed", value);
		}
	}

	public int HorizontalResolution
	{
		set
		{
			Uniform("horizontalResolution", value);
		}
	}

	public float FogWaveCounter
	{
		set
		{
			Uniform("fogWaveCounter", value);
		}
	}

	public int Glow2D
	{
		set
		{
			BindTexture2D("glow", value, 2);
		}
	}

	public int Sky2D
	{
		set
		{
			BindTexture2D("sky", value, 3);
		}
	}

	public float SunsetMod
	{
		set
		{
			Uniform("sunsetMod", value);
		}
	}

	public int LiquidDepth2D
	{
		set
		{
			BindTexture2D("liquidDepth", value, 4);
		}
	}

	public float CameraUnderwater
	{
		set
		{
			Uniform("cameraUnderwater", value);
		}
	}

	public Vec2f FrameSize
	{
		set
		{
			Uniform("frameSize", value);
		}
	}

	public Vec4f WaterMurkColor
	{
		set
		{
			Uniform("waterMurkColor", value);
		}
	}

	public Vec4f RgbaFogIn
	{
		set
		{
			Uniform("rgbaFogIn", value);
		}
	}

	public Vec3f RgbaAmbientIn
	{
		set
		{
			Uniform("rgbaAmbientIn", value);
		}
	}

	public float[] ProjectionMatrix
	{
		set
		{
			UniformMatrix("projectionMatrix", value);
		}
	}

	public float[] ModelViewMatrix
	{
		set
		{
			UniformMatrix("modelViewMatrix", value);
		}
	}

	public float ShadowRangeFar
	{
		set
		{
			Uniform("shadowRangeFar", value);
		}
	}

	public float[] ToShadowMapSpaceMatrixFar
	{
		set
		{
			UniformMatrix("toShadowMapSpaceMatrixFar", value);
		}
	}

	public float ShadowRangeNear
	{
		set
		{
			Uniform("shadowRangeNear", value);
		}
	}

	public float[] ToShadowMapSpaceMatrixNear
	{
		set
		{
			UniformMatrix("toShadowMapSpaceMatrixNear", value);
		}
	}

	public float GlitchStrengthFL
	{
		set
		{
			Uniform("glitchStrengthFL", value);
		}
	}

	public float NightVisionStrength
	{
		set
		{
			Uniform("nightVisionStrength", value);
		}
	}

	public int PointLightQuantity
	{
		set
		{
			Uniform("pointLightQuantity", value);
		}
	}

	public void PointLightsArray(int count, float[] values)
	{
		Uniforms3("pointLights", count, values);
	}

	public void PointLightColorsArray(int count, float[] values)
	{
		Uniforms3("pointLightColors", count, values);
	}
}
