using System;
using System.Collections.Generic;
using UnityEngine;

namespace StylizedWaterShader;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[HelpURL("http://staggart.xyz/unity/stylized-water-shader/documentation/")]
public class StylizedWater : MonoBehaviour
{
	public enum LightingMode
	{
		Unlit,
		Basic,
		Advanced
	}

	public string[] shaderNames;

	public int shaderIndex;

	public Shader shader;

	private Shader DesktopShader;

	private Shader MobileAdvancedShader;

	[Range(2000f, 4000f)]
	public int renderQueue;

	public bool enableVertexColors;

	public bool enableDepthTex = true;

	public bool isUnlit;

	public bool enableGradient;

	public Gradient colorGradient;

	[ColorUsage(true, true)]
	public Color waterShallowColor;

	[Range(0f, 100f)]
	public float depth;

	[ColorUsage(true, true)]
	public Color waterColor;

	[ColorUsage(true, true)]
	public Color fresnelColor;

	public float fresnel;

	[ColorUsage(true, true)]
	public Color rimColor;

	[Range(-0.5f, 0.5f)]
	public float waveTint;

	[Range(0f, 1f)]
	public float transparency;

	[Range(0.01f, 1f)]
	public float glossiness;

	[Range(0f, 1f)]
	public float metallicness;

	[Range(0f, 3f)]
	public float edgeFade;

	public string[] tilingMethodNames;

	public float worldSpaceTiling;

	[Range(0f, 0.2f)]
	public float refractionAmount;

	public bool enableNormalMap = true;

	[Range(0f, 1f)]
	public float normalStrength;

	public bool enableMacroNormals;

	[Range(250f, 3000f)]
	public float macroNormalsDistance = 1000f;

	[Range(0f, 50f)]
	public float normalTiling;

	public int intersectionSolver;

	public string[] intersectionSolverNames;

	[Range(0f, 30f)]
	public float rimSize;

	[Range(0.1f, 30f)]
	public float rimFalloff;

	public float rimTiling;

	[Range(0f, 1f)]
	public float rimDistortion;

	public bool enableVCIntersection;

	public int foamSolver;

	public string[] foamSolverNames;

	[Range(-1f, 1f)]
	public float foamOpacity;

	public float foamTiling;

	[Range(0f, 1f)]
	public float foamSize;

	[Range(0f, 3f)]
	public float foamDistortion;

	[Range(0f, 1f)]
	public float foamSpeed;

	[Range(0f, 1f)]
	public float waveFoam;

	[Range(0f, 1f)]
	public float reflectionStrength = 1f;

	[Range(0.01f, 10f)]
	public float reflectionFresnel = 10f;

	public bool showReflection;

	[Range(0f, 0.2f)]
	public float reflectionRefraction;

	[Range(0.01f, 10f)]
	public float waveSpeed;

	[Range(0f, 1f)]
	public float waveStrength;

	[Range(-1f, 1f)]
	public Vector4 waveDirectionXZ;

	public bool enableSecondaryWaves;

	public Texture2D customIntersection;

	public Texture2D customNormal;

	public Texture2D customHeightmap;

	public string[] intersectionStyleNames;

	public int intersectionStyle = 1;

	public string[] waveStyleNames;

	public int waveStyle;

	public string[] waveHeightmapNames;

	public int waveHeightmapStyle;

	public float waveSize;

	public bool useCustomIntersection;

	public bool useCustomNormals;

	public bool useCustomHeightmap;

	public Texture2D normals;

	public Texture2D shadermap;

	public Texture2D colorGradientTex;

	public bool useCompression;

	public static bool EnableReflections;

	private Camera reflectionCamera;

	private Camera refractCamera;

	public bool useReflection;

	public bool useRefractionCam;

	public bool enableReflectionBlur;

	[Range(1f, 15f)]
	public float reflectionBlurLength = 4f;

	[Range(1f, 4f)]
	public int reflectionBlurPasses = 4;

	private StylizedWaterBlur reflectionBlurRenderer;

	public string[] refractionSolverNames;

	public int refractionSolver;

	public string[] resolutionNames;

	public int reflectionRes = 1;

	public int refractRes = 2;

	public int reflectionTextureSize = 512;

	public int refractTextureSize = 1024;

	[Range(0f, 10f)]
	public float clipPlaneOffset = 1f;

	public LayerMask reflectLayers = -1;

	public LayerMask refractLayers = -1;

	private Dictionary<Camera, Camera> m_ReflectionCameras = new Dictionary<Camera, Camera>();

	private Dictionary<Camera, StylizedWaterBlur> m_BlurRenderers = new Dictionary<Camera, StylizedWaterBlur>();

	private Dictionary<Camera, Camera> m_RefractCameras = new Dictionary<Camera, Camera>();

	private RenderTexture m_ReflectionTexture;

	private RenderTexture m_RefractTexture;

	private int m_OldReflectionTextureSize;

	private int m_OldRefractTextureSize;

	private bool s_InsideRendering;

	public LightingMode lightingMethod = LightingMode.Advanced;

	public bool enableShadows;

	private StylizedWaterShadowCaster shadowRenderer;

	public Light shadowCaster;

	[NonSerialized]
	private MeshRenderer meshRenderer;

	public Material material;

	public bool isMobileAdvanced;

	public bool isMobilePlatform;

	public string shaderName;

	public bool isWaterLayer;

	public bool hasShaderParams;

	public bool hasMaterial;

	public bool usingSinglePassRendering;

	public bool hideMeshRenderer = true;

	public void OnEnable()
	{
		if (!meshRenderer)
		{
			meshRenderer = GetComponent<MeshRenderer>();
		}
		GetProperties();
		SetProperties();
	}

	public void Init()
	{
	}

	public void GetProperties()
	{
	}

	private void GetShaderProperties()
	{
		if (!material)
		{
			hasMaterial = false;
			return;
		}
		hasMaterial = true;
		renderQueue = material.renderQueue;
		GetShaderType();
		if (material.GetFloat("_LIGHTING") == 0f && material.GetFloat("_Unlit") == 1f)
		{
			lightingMethod = LightingMode.Unlit;
		}
		else if (material.GetFloat("_LIGHTING") == 0f && material.GetFloat("_Unlit") == 0f)
		{
			lightingMethod = LightingMode.Basic;
		}
		else if (material.GetFloat("_LIGHTING") == 1f && material.GetFloat("_Unlit") == 0f)
		{
			lightingMethod = LightingMode.Advanced;
		}
		enableVertexColors = material.GetFloat("_ENABLE_VC") == 1f;
		shadermap = material.GetTexture("_Shadermap") as Texture2D;
		normals = material.GetTexture("_Normals") as Texture2D;
		waterColor = material.GetColor("_WaterColor");
		waterShallowColor = material.GetColor("_WaterShallowColor");
		depth = material.GetFloat("_Depth");
		waveTint = material.GetFloat("_Wavetint");
		rimColor = material.GetColor("_RimColor");
		worldSpaceTiling = material.GetFloat("_Worldspacetiling");
		transparency = material.GetFloat("_Transparency");
		edgeFade = material.GetFloat("_EdgeFade");
		glossiness = material.GetFloat("_Glossiness");
		metallicness = material.GetFloat("_Metallicness");
		normalStrength = material.GetFloat("_NormalStrength");
		normalTiling = material.GetFloat("_NormalTiling");
		foamOpacity = material.GetFloat("_FoamOpacity");
		foamSolver = (int)material.GetFloat("_UseIntersectionFoam");
		foamTiling = material.GetFloat("_FoamTiling");
		foamSize = material.GetFloat("_FoamSize");
		foamSpeed = material.GetFloat("_FoamSpeed");
		waveFoam = material.GetFloat("_WaveFoam");
		intersectionSolver = (int)material.GetFloat("_USE_VC_INTERSECTION");
		rimSize = material.GetFloat("_RimSize");
		rimFalloff = material.GetFloat("_Rimfalloff");
		rimTiling = material.GetFloat("_Rimtiling");
		waveSpeed = material.GetFloat("_Wavesspeed");
		waveDirectionXZ = material.GetVector("_WaveDirection");
		waveStrength = material.GetFloat("_WaveHeight");
		waveSize = material.GetFloat("_WaveSize");
		if (isMobileAdvanced)
		{
			enableDepthTex = material.GetFloat("_EnableDepthTexture") == 1f;
		}
		if (!isMobileAdvanced)
		{
			enableShadows = material.GetFloat("_ENABLE_SHADOWS") == 1f;
			enableNormalMap = true;
			enableGradient = material.GetFloat("_ENABLE_GRADIENT") == 1f;
			fresnelColor = material.GetColor("_FresnelColor");
			fresnel = material.GetFloat("_Fresnelexponent");
			refractionAmount = material.GetFloat("_RefractionAmount");
			foamDistortion = material.GetFloat("_FoamDistortion");
			if (useReflection)
			{
				reflectionStrength = material.GetFloat("_ReflectionStrength");
			}
			reflectionFresnel = material.GetFloat("_ReflectionFresnel");
			reflectionRefraction = material.GetFloat("_ReflectionRefraction");
			rimDistortion = material.GetFloat("_RimDistortion");
			enableMacroNormals = material.IsKeywordEnabled("_MACRO_WAVES_ON");
			macroNormalsDistance = material.GetFloat("_MacroBlendDistance");
			enableSecondaryWaves = material.IsKeywordEnabled("_SECONDARY_WAVES_ON");
		}
		if (isMobileAdvanced)
		{
			enableNormalMap = material.IsKeywordEnabled("_NORMAL_MAP_ON");
			intersectionSolver = (int)material.GetFloat("_USE_VC_INTERSECTION");
			foamSolver = (material.IsKeywordEnabled("_USEINTERSECTIONFOAM_ON") ? 1 : 0);
		}
		hasShaderParams = true;
	}

	private void GetShaderType()
	{
	}

	private void SetShaderType()
	{
	}

	public void SetProperties()
	{
		if (enableShadows && (bool)shadowCaster)
		{
			EnableShadowRendering();
		}
		else
		{
			DisableShadowRendering();
		}
		SetShaderProperties();
	}

	private void SetShaderProperties()
	{
		if (!material)
		{
			return;
		}
		SetShaderType();
		material.renderQueue = renderQueue;
		if ((bool)shadermap)
		{
			material.SetTexture("_Shadermap", shadermap);
		}
		if ((bool)normals)
		{
			material.SetTexture("_Normals", normals);
		}
		switch (lightingMethod)
		{
		case LightingMode.Unlit:
			material.DisableKeyword("_LIGHTING_ON");
			material.SetFloat("_LIGHTING", 0f);
			material.SetFloat("_Unlit", 1f);
			break;
		case LightingMode.Basic:
			material.DisableKeyword("_LIGHTING_ON");
			material.SetFloat("_LIGHTING", 0f);
			material.SetFloat("_Unlit", 0f);
			break;
		case LightingMode.Advanced:
			material.EnableKeyword("_LIGHTING_ON");
			material.SetFloat("_LIGHTING", 1f);
			material.SetFloat("_Unlit", 0f);
			break;
		}
		material.SetColor("_WaterColor", waterColor);
		material.SetColor("_WaterShallowColor", waterShallowColor);
		material.SetFloat("_Depth", depth);
		material.SetColor("_RimColor", rimColor);
		material.SetFloat("_Wavetint", waveTint);
		material.SetFloat("_Transparency", transparency);
		material.SetFloat("_Glossiness", glossiness);
		material.SetFloat("_Metallicness", metallicness);
		material.SetFloat("_Worldspacetiling", worldSpaceTiling);
		material.SetFloat("_EdgeFade", edgeFade);
		material.SetFloat("_NormalStrength", normalStrength);
		material.SetFloat("_NormalTiling", normalTiling);
		material.SetFloat("_USE_VC_INTERSECTION", intersectionSolver);
		material.SetFloat("_RimSize", rimSize);
		material.SetFloat("_Rimfalloff", rimFalloff);
		material.SetFloat("_Rimtiling", rimTiling);
		material.SetFloat("_UseIntersectionFoam", foamSolver);
		material.SetFloat("_FoamOpacity", foamOpacity);
		material.SetFloat("_FoamSize", foamSize);
		material.SetFloat("_FoamTiling", foamTiling);
		material.SetFloat("_FoamSpeed", foamSpeed);
		material.SetFloat("_WaveFoam", waveFoam);
		material.SetFloat("_Wavesspeed", waveSpeed);
		material.SetVector("_WaveDirection", waveDirectionXZ);
		material.SetFloat("_WaveHeight", waveStrength);
		material.SetFloat("_WaveSize", waveSize);
		material.SetFloat("_ENABLE_VC", enableVertexColors ? 1 : 0);
		if (isMobileAdvanced)
		{
			material.SetFloat("_EnableDepthTexture", enableDepthTex ? 1 : 0);
		}
		if (!isMobileAdvanced)
		{
			material.SetFloat("_ENABLE_SHADOWS", enableShadows ? 1 : 0);
			material.SetFloat("_ENABLE_GRADIENT", enableGradient ? 1 : 0);
			material.SetTexture("_GradientTex", colorGradientTex);
			material.SetColor("_FresnelColor", fresnelColor);
			material.SetFloat("_Fresnelexponent", fresnel);
			useRefractionCam = refractionSolver == 1;
			if (!useRefractionCam)
			{
				DisableRefractionCam();
			}
			material.SetFloat("_RT_REFRACTION", refractionSolver);
			material.SetFloat("_RefractionAmount", refractionAmount);
			material.SetFloat("_RimDistortion", rimDistortion);
			if (usingSinglePassRendering)
			{
				useReflection = false;
				material.SetFloat("_ReflectionStrength", 0f);
			}
			else
			{
				material.SetFloat("_ReflectionStrength", reflectionStrength);
				material.SetFloat("_ReflectionFresnel", reflectionFresnel);
				material.SetFloat("_ReflectionRefraction", reflectionRefraction);
			}
			if ((bool)reflectionBlurRenderer)
			{
				reflectionBlurRenderer.length = reflectionBlurLength;
				reflectionBlurRenderer.passes = reflectionBlurPasses;
			}
			material.SetFloat("_UseIntersectionFoam", foamSolver);
			material.SetFloat("_FoamDistortion", foamDistortion);
			if (enableMacroNormals)
			{
				material.EnableKeyword("_MACRO_WAVES_ON");
			}
			else
			{
				material.DisableKeyword("_MACRO_WAVES_ON");
			}
			material.SetFloat("_MacroBlendDistance", macroNormalsDistance);
			if (enableSecondaryWaves)
			{
				material.EnableKeyword("_SECONDARY_WAVES_ON");
			}
			else
			{
				material.DisableKeyword("_SECONDARY_WAVES_ON");
			}
		}
		if (isMobileAdvanced)
		{
			if (enableNormalMap)
			{
				material.EnableKeyword("_NORMAL_MAP_ON");
			}
			else
			{
				material.DisableKeyword("_NORMAL_MAP_ON");
			}
			if (foamSolver == 1)
			{
				material.EnableKeyword("_USEINTERSECTIONFOAM_ON");
			}
			else
			{
				material.DisableKeyword("_USEINTERSECTIONFOAM_ON");
			}
		}
	}

	public void DisableReflectionCam()
	{
		if ((bool)m_ReflectionTexture)
		{
			UnityEngine.Object.DestroyImmediate(m_ReflectionTexture);
			m_ReflectionTexture = null;
		}
		foreach (KeyValuePair<Camera, Camera> reflectionCamera in m_ReflectionCameras)
		{
			UnityEngine.Object.DestroyImmediate(reflectionCamera.Value.gameObject);
		}
		m_ReflectionCameras.Clear();
		if (!useReflection && (bool)material)
		{
			material.SetFloat("_ReflectionStrength", 0f);
		}
	}

	public void DisableRefractionCam()
	{
		if ((bool)m_RefractTexture)
		{
			UnityEngine.Object.DestroyImmediate(m_RefractTexture);
			m_RefractTexture = null;
		}
		foreach (KeyValuePair<Camera, Camera> refractCamera in m_RefractCameras)
		{
			UnityEngine.Object.DestroyImmediate(refractCamera.Value.gameObject);
		}
		m_RefractCameras.Clear();
	}

	private void EnableShadowRendering()
	{
		if (shadowCaster.GetComponent<StylizedWaterShadowCaster>() == null)
		{
			shadowRenderer = shadowCaster.gameObject.AddComponent<StylizedWaterShadowCaster>();
		}
	}

	private void DisableShadowRendering()
	{
		if ((bool)shadowRenderer)
		{
			UnityEngine.Object.DestroyImmediate(shadowRenderer);
			return;
		}
		try
		{
			if ((bool)shadowCaster)
			{
				shadowRenderer = shadowCaster.GetComponent<StylizedWaterShadowCaster>();
			}
			if ((bool)shadowRenderer)
			{
				UnityEngine.Object.DestroyImmediate(shadowRenderer);
			}
		}
		catch (Exception)
		{
			throw;
		}
	}

	public void SetVegetationStudioWaterLevel()
	{
	}

	public void OnWillRenderObject()
	{
		if (!base.enabled || !material)
		{
			return;
		}
		Camera current = Camera.current;
		if (!current || s_InsideRendering)
		{
			return;
		}
		s_InsideRendering = true;
		CreateWaterObjects(current, out var camera, out var camera2);
		Vector3 position = base.transform.position;
		Vector3 up = base.transform.up;
		int pixelLightCount = QualitySettings.pixelLightCount;
		QualitySettings.pixelLightCount = 0;
		if (useReflection)
		{
			StylizedWaterUtilities.CameraUtils.CopyCameraSettings(current, camera);
			Vector4 vector = new Vector4(w: 0f - Vector3.Dot(up, position) - (clipPlaneOffset + 0.001f), x: up.x, y: up.y, z: up.z);
			Matrix4x4 reflectionMat = Matrix4x4.zero;
			StylizedWaterUtilities.CameraUtils.CalculateReflectionMatrix(ref reflectionMat, vector);
			Vector3 position2 = current.transform.position;
			Vector3 position3 = reflectionMat.MultiplyPoint(position2);
			camera.worldToCameraMatrix = current.worldToCameraMatrix * reflectionMat;
			Vector4 clipPlane = StylizedWaterUtilities.CameraUtils.CameraSpacePlane(camera, position, up, 1f, clipPlaneOffset);
			camera.projectionMatrix = current.CalculateObliqueMatrix(clipPlane);
			camera.cullingMask = -17 & reflectLayers.value;
			camera.targetTexture = m_ReflectionTexture;
			bool invertCulling = GL.invertCulling;
			GL.invertCulling = !invertCulling;
			camera.transform.position = position3;
			Vector3 eulerAngles = current.transform.eulerAngles;
			camera.transform.eulerAngles = new Vector3(0f - eulerAngles.x, eulerAngles.y, eulerAngles.z);
			if (Mathf.Abs(Vector3.Dot(vector, camera.transform.forward)) > 0f)
			{
				if (enableReflectionBlur && (bool)reflectionBlurRenderer)
				{
					reflectionBlurRenderer.Render();
				}
				camera.Render();
			}
			camera.transform.position = position2;
			GL.invertCulling = invertCulling;
			if (!enableReflectionBlur)
			{
				Shader.SetGlobalTexture("_ReflectionTex", m_ReflectionTexture);
			}
		}
		else
		{
			DisableReflectionCam();
		}
		if (useRefractionCam)
		{
			StylizedWaterUtilities.CameraUtils.CopyCameraSettings(current, camera2);
			camera2.worldToCameraMatrix = current.worldToCameraMatrix;
			Vector4 clipPlane2 = StylizedWaterUtilities.CameraUtils.CameraSpacePlane(camera2, position, up, -1f, clipPlaneOffset);
			camera2.projectionMatrix = current.CalculateObliqueMatrix(clipPlane2);
			camera2.cullingMask = -17 & refractLayers.value;
			camera2.targetTexture = m_RefractTexture;
			camera2.transform.position = current.transform.position;
			camera2.transform.rotation = current.transform.rotation;
			camera2.Render();
			material.SetTexture("_RefractionTex", m_RefractTexture);
		}
		QualitySettings.pixelLightCount = pixelLightCount;
		s_InsideRendering = false;
	}

	public void CreateReflectionTexture()
	{
		switch (reflectionRes)
		{
		case 0:
			reflectionTextureSize = 256;
			break;
		case 1:
			reflectionTextureSize = 512;
			break;
		case 2:
			reflectionTextureSize = 1024;
			break;
		}
		if (!m_ReflectionTexture || m_OldReflectionTextureSize != reflectionTextureSize)
		{
			m_ReflectionTexture = new RenderTexture(reflectionTextureSize, reflectionTextureSize, 16)
			{
				name = "__WaterReflection",
				isPowerOfTwo = true,
				hideFlags = HideFlags.None
			};
			m_OldReflectionTextureSize = reflectionTextureSize;
		}
	}

	public void CreateRefractionTexture()
	{
		if ((bool)m_RefractTexture)
		{
			UnityEngine.Object.DestroyImmediate(m_RefractTexture);
			m_RefractTexture = null;
		}
		if (!m_RefractTexture || m_OldRefractTextureSize != refractTextureSize)
		{
			switch (refractRes)
			{
			case 0:
				refractTextureSize = 256;
				break;
			case 1:
				refractTextureSize = 512;
				break;
			case 2:
				refractTextureSize = 1024;
				break;
			}
			m_RefractTexture = new RenderTexture(refractTextureSize, refractTextureSize, 16);
			m_RefractTexture.name = "__WaterRefraction";
			m_RefractTexture.isPowerOfTwo = true;
			m_RefractTexture.hideFlags = HideFlags.DontSave;
			m_OldRefractTextureSize = refractTextureSize;
		}
	}

	private void CreateWaterObjects(Camera currentCamera, out Camera reflectionCamera, out Camera refractCamera)
	{
		reflectionCamera = null;
		refractCamera = null;
		reflectionBlurRenderer = null;
		if (useReflection)
		{
			CreateReflectionTexture();
			m_ReflectionCameras.TryGetValue(currentCamera, out reflectionCamera);
			if (!reflectionCamera)
			{
				GameObject gameObject = new GameObject("", typeof(Camera));
				gameObject.name = "Reflection Camera " + gameObject.GetInstanceID() + " for " + currentCamera.name;
				gameObject.hideFlags = HideFlags.HideAndDontSave;
				reflectionCamera = gameObject.GetComponent<Camera>();
				reflectionCamera.enabled = false;
				reflectionCamera.useOcclusionCulling = false;
				reflectionCamera.transform.position = base.transform.position;
				reflectionCamera.transform.rotation = base.transform.rotation;
				reflectionCamera.gameObject.AddComponent<FlareLayer>();
				m_ReflectionCameras[currentCamera] = reflectionCamera;
			}
			if ((bool)reflectionCamera && enableReflectionBlur)
			{
				m_BlurRenderers.TryGetValue(currentCamera, out reflectionBlurRenderer);
				if (!reflectionBlurRenderer)
				{
					reflectionBlurRenderer = reflectionCamera.gameObject.AddComponent<StylizedWaterBlur>();
				}
				m_BlurRenderers[currentCamera] = reflectionBlurRenderer;
			}
		}
		if (useRefractionCam)
		{
			CreateRefractionTexture();
			m_RefractCameras.TryGetValue(currentCamera, out refractCamera);
			if (!refractCamera)
			{
				GameObject gameObject2 = new GameObject("", typeof(Camera));
				gameObject2.name = "Refraction Camera " + gameObject2.GetInstanceID() + " for " + currentCamera.name;
				gameObject2.hideFlags = HideFlags.HideAndDontSave;
				refractCamera = gameObject2.GetComponent<Camera>();
				refractCamera.enabled = false;
				refractCamera.useOcclusionCulling = false;
				refractCamera.transform.position = base.transform.position;
				refractCamera.transform.rotation = base.transform.rotation;
				refractCamera.gameObject.AddComponent<FlareLayer>();
				m_RefractCameras[currentCamera] = refractCamera;
			}
		}
	}

	private void OnDisable()
	{
		DisableReflectionCam();
		DisableRefractionCam();
		if ((bool)shadowRenderer)
		{
			UnityEngine.Object.DestroyImmediate(shadowRenderer);
		}
	}

	private void OnDestroy()
	{
		DisableShadowRendering();
	}
}
