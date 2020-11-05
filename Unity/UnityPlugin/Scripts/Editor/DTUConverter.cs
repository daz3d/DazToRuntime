using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace Daz3D
{
	public static class DTU_Constants
	{
		public const string shaderNameMetal = "Daz3D/IrayUberMetal";
		public const string shaderNameSpecular = "Daz3D/IrayUberSpec";
		public const string shaderNameIraySkin = "Daz3D/IrayUberSkin";
		public const string shaderNameHair = "Daz3D/Hair";
		public const string shaderNameWet = "Daz3D/Wet";
		public const string shaderNameInvisible = "Daz3D/Invisible";        //special shader that doesn't render anything

	}

	/// <summary>
	/// A temporary data structure that reads and interprest the .dtu file the Daz to Unity bridge produces
	/// </summary>
	public struct DTU
	{
		/// <summary>
		/// Where is the source DTU, this is important as this is in the root folder for the asset, textures and materials will exist at this location
		/// </summary>
		public string DTUPath;

		private string _DTUDir;
		public string DTUDir
		{
			get
			{
				if(string.IsNullOrEmpty(_DTUDir))
				{
					_DTUDir = new System.IO.FileInfo(DTUPath).Directory.FullName;
					//if we have regular backslashes on windows, convert to forward slashes
					_DTUDir = _DTUDir.Replace(@"\",@"/");
					//remove everything up to Assets from the path so it's relative to the project folder
					_DTUDir = _DTUDir.Replace(Application.dataPath,"Assets");
				}
				return _DTUDir;
			}
		}

		public string AssetID;
		public string AssetName;
		public string AssetType;
		public string FBXFile;
		public string ImportFolder;
		public List<DTUMaterial> Materials;

		//DiffusionProfile is sealed in older versions of HDRP, will need to use reflection if we want access to it
		//public UnityEngine.Rendering.HighDefinition.DiffusionProfile diffusionProfile = null;


		/// <summary>
		/// These are analagous to the shaders in Daz3D, if your shader is not in this list
		///  the material will fail or be skipped
		/// </summary>
		public enum DTUMaterialType
		{
			Unknown,
			IrayUber,
			PBRSP,
			DazStudioDefault,
			OmUberSurface,
			OOTHairblendingHair,
		}

		/// <summary>
		/// Used with the Daz Studio Default shader
		/// </summary>
		public enum DTULightingModel
		{
			Unknown = -1,
			Plastic = 0,
			Metallic = 1,
			Skin = 2,
			GlossyPlastic = 3,
			Matte = 4,
			GlossyMetallic = 5,

		}

		/// <summary>
		/// Used when the shader type is Iray Uber, this defines the flow and changes which properties are read
		/// </summary>
		public enum DTUBaseMixing
		{
			Unknown = -1,
			PBRMetalRoughness = 0,
			PBRSpecularGlossiness = 1,
			Weighted = 2,
		}

		/// <summary>
		/// Guess if our material looks like a hair
		/// </summary>
		/// <param name="dtuMaterial"></param>
		/// <returns></returns>
		public bool IsDTUMaterialHair(DTUMaterial dtuMaterial)
		{
			var matNameLower = dtuMaterial.MaterialName.ToLower();
			var assetNameLower = dtuMaterial.AssetName.ToLower();
			var valueLower = dtuMaterial.Value.ToLower();


			//hair requires some special changes b/c it's not rendered the same as in Daz Studio
			// this is one of the outlier material types that doesn't use the same property types
			//TODO: this lookup feels a bit silly, we should make a function where we pass in this mat and it does something smarter
			if(
				valueLower.Contains("hair") || assetNameLower.EndsWith("hair") || matNameLower.Contains("hair") 
				|| valueLower.Contains("moustache") || assetNameLower.EndsWith("moustache") || matNameLower.Contains("moustache") 
				|| valueLower.Contains("beard") || assetNameLower.EndsWith("beard") || matNameLower.Contains("beard") 
			)
			{
				return true;
			}

			return false;
		}
		
		/// <summary>
		/// Guess if our material is wet, such as a cornea, or eye moisture
		/// </summary>
		/// <param name="dtuMaterial"></param>
		/// <returns></returns>
		public bool IsDTUMaterialWet(DTUMaterial dtuMaterial)
		{
			var matNameLower = dtuMaterial.MaterialName.ToLower();
			var assetNameLower = dtuMaterial.AssetName.ToLower();
			var valueLower = dtuMaterial.Value.ToLower();

			if(matNameLower.Contains("cornea") || matNameLower.Contains("eyemoisture") || matNameLower.Contains("eyereflection"))
			{
				return true;
			}


			return false;
		}


		public bool IsDTUMaterialSclera(DTUMaterial dtuMaterial)
		{
			var matNameLower = dtuMaterial.MaterialName.ToLower();
			var assetNameLower = dtuMaterial.AssetName.ToLower();
			var valueLower = dtuMaterial.Value.ToLower();


			if(matNameLower.Contains("sclera"))
			{
				return true;
			}


			return false;
		}

		/// <summary>
		/// Guess if our material is a skin
		/// </summary>
		/// <param name="dTUMaterial"></param>
		/// <returns></returns>
		public bool IsDTUMaterialSkin(DTUMaterial dtuMaterial)
		{

			var dualLobeSpecularWeight = dtuMaterial.Get("Dual Lobe Specular Weight");
			var dualLobeSpecularReflectivity = dtuMaterial.Get("Dual Lobe Specular Reflectivity");

			if(dtuMaterial.MaterialType == "omUberSurface" || dtuMaterial.MaterialType == "omHumanSurface")
			{
				if(IsDTUMaterialWet(dtuMaterial))
				{
					return false;
				}
				if(IsDTUMaterialHair(dtuMaterial))
				{
					return false;
				}
				if(IsDTUMaterialSclera(dtuMaterial))
				{
					return false;
				}

				var matNameLower = dtuMaterial.MaterialName.ToLower();

				if(matNameLower.Contains("iris") || matNameLower.Contains("eyelash") || matNameLower.Contains("teeth") || matNameLower.Contains("nail") || matNameLower.Contains("tongue") || matNameLower.Contains("mouth") || matNameLower.Contains("pupil"))
				{
					return false;
				}

				return dtuMaterial.Value == "Actor/Character" || dtuMaterial.Value == "Actor";
			}

			//TODO: this is a bit naive as people will use the dual lobe properties for non skin, but for now it's ok
			if(
				dualLobeSpecularWeight.TextureExists() || dualLobeSpecularReflectivity.TextureExists()
			)
			{
				return true;
			}


			return false;
		}




		/// <summary>
		/// Toggle various states and parameters if we're transparent, opaque, and/or double sided
		/// </summary>
		/// <param name="mat"></param>
		/// <param name="matNameLower"></param>
		/// <param name="isTransparent"></param>
		/// <param name="isDoubleSided"></param>
		/// <param name="hasDualLobeSpecularWeight"></param>
		/// <param name="hasDualLobeSpecularReflectivity"></param>
		/// <param name="sortingPriority"></param>
		/// <param name="hasGlossyLayeredWeight"></param>
		/// <param name="hasGlossyColor"></param>
		public void ToggleCommonMaterialProperties(ref Material mat, string matNameLower, bool isTransparent = false, bool isDoubleSided = false, bool hasDualLobeSpecularWeight = false, bool hasDualLobeSpecularReflectivity = false, int sortingPriority = 0, bool hasGlossyLayeredWeight=false, bool hasGlossyColor=false)
		{
			if(isTransparent)
			{
				mat.SetFloat("_ZWrite",0f);
				mat.SetFloat("_TransparentZWrite",0f);
				mat.SetFloat("_ZTestGBuffer",3f);
				mat.SetFloat("_SurfaceType",1f);
				mat.SetFloat("_AlphaCutoffEnable",1f);
				mat.SetFloat("_AlphaDstBlend",10f);
				mat.SetFloat("_DstBlend",10f);


				mat.EnableKeyword("_ALPHATEST_ON");
				mat.EnableKeyword("_BLENDMODE_ALPHA");
				mat.EnableKeyword("_ENABLE_FOG_ON_TRANSPARENT");
				mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
				
				mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent + sortingPriority;
			}
			else
			{
				mat.SetFloat("_ZWrite",1f);
				mat.SetFloat("_TransparentZWrite",1f);
				mat.SetFloat("_ZTestGBuffer",5f);
				mat.SetFloat("_SurfaceType",0f);
				mat.SetFloat("_AlphaCutoffEnable",0f);
				mat.SetFloat("_AlphaDstBlend",0f);
				mat.SetFloat("_DstBlend",0f);

				mat.DisableKeyword("_ALPHATEST_ON");
				mat.DisableKeyword("_BLENDMODE_ALPHA");
				mat.DisableKeyword("_ENABLE_FOG_ON_TRANSPARENT");
				mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
				
				mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry + sortingPriority;
			}


			mat.SetShaderPassEnabled("MOTIONVECTORS",false);
			mat.SetShaderPassEnabled("TransparentBackface",false);
			mat.SetOverrideTag("MotionVector","User");


			if(isDoubleSided)
			{
				mat.SetFloat("_CullMode",0f);
				mat.SetFloat("_CullModeForward",0f);
				mat.SetFloat("_DoubleSidedEnable",1f);
				mat.SetFloat("_DoubleSidedNormalMode",1f); //Mirror

				//if we're transparent and double sided, unless we're something like eyelashes we want to default to writing to the depth buffer
				if(isTransparent && !matNameLower.Contains("eyelash"))
				{
					UnityEngine.Debug.LogWarning("Material is both double sided and transparent, you will have rendering artifacts against the two sides where they are alphad, for mat: " + mat.name);
					mat.SetFloat("_ZWrite",1f);
					mat.SetFloat("_TransparentZWrite",1f);
				}
				
				mat.EnableKeyword("_DOUBLESIDED_ON");
				
				mat.doubleSidedGI = true;
			}
			else
			{
				mat.SetFloat("_CullMode",2f);
				mat.SetFloat("_CullModeForward",2f);
				mat.SetFloat("_DoubleSidedEnable",0f);
				mat.SetFloat("_DoubleSidedNormalMode",2f); //None
				
				mat.DisableKeyword("_DOUBLESIDED_ON");
				
				mat.doubleSidedGI = false;
			}

			if(hasDualLobeSpecularWeight)
			{
				mat.EnableKeyword("IRAYUBER_DUALLOBESPECULARACTIVE");
			}
			else
			{
				mat.DisableKeyword("IRAYUBER_DUALLOBESPECULARACTIVE");
			}
			if(hasDualLobeSpecularReflectivity)
			{
				mat.EnableKeyword("IRAYUBER_DUALLOBESPECULARREFLECTIVITYACTIVE");
			}
			else
			{
				mat.DisableKeyword("IRAYUBER_DUALLOBESPECULARREFLECTIVITYACTIVE");
			}
			if(hasGlossyLayeredWeight)
			{
				mat.EnableKeyword("IRAYUBER_GLOSSYLAYEREDWEIGHTACTIVE");
			}
			else
			{
				mat.DisableKeyword("IRAYUBER_GLOSSYLAYEREDWEIGHTACTIVE");
			}
			if(hasGlossyColor)
			{
				mat.EnableKeyword("IRAYUBER_GLOSSYCOLORACTIVE");
			}
			else
			{
				mat.DisableKeyword("IRAYUBER_GLOSSYCOLORACTIVE");
			}
		}



		/// <summary>
		/// Converts a DTU material block into a suitable unity represnetation
		/// This function only converts materials from the Iray Uber shader in Daz
		///  for other shaders see the base conversion ConvertToUnity
		/// </summary>
		/// <param name="dtuMaterial"></param>
		/// <param name="materialDir"></param>
		/// <returns></returns>
		public Material ConvertToUnityIrayUber(DTUMaterial dtuMaterial, string materialDir)
		{
			// We have a few branches we're going to go down and filter out to the following shaders
			//  IrayUberMetal - Used if we can safely assume this is a material of base mix metal/rough and no translucency/skin detected
			//  IrayUberSpec - Used for base mixing of both specular/gloss and weighted
			//  IrayUberTranslucent - Used when the material is translucent, note metal flow no longer works now
			//  IrayUberSkin - Used when we guess that the mat is for skin
			//  IrayUberHair - Used when we guess that the mat is for hair
	
			/**
			The Lit master node in unity supports a few material types:
			 Standard (metal/rough), Specular Color (Spec/gloss), Tranlsucent (no sss), SSS (same as translucent but with a SSS mask)
			There is also a stack master which attempts to combine several of these features into one node
			There is a problem with the stack master which is you can't mix and match all the features
			There is also the problem of performance when you start enabling all these features and you have them plugged in

			Instead of using the stack master we're using more targetted shaders into the following buckets

			Metal, Spec, Translucent, Skin, Hair

			Translucent and Skin are spec only and don't support metalness
			Hair is a more custom approach and highly deviates from the Iray Uber base shader

			In order to decide which shader to use, we scan and look for a few property values below
			 then pick one path and stick to it, this means we will ignore some features that iray
			 supports in daz
			
			Below is a table I generated that explains some mappings between daz->unity and which base mixing value
			 they belong to along with what types of inputs are supported, you can use that as a guide if you want
			 to make your own shader or customize this material generator
			*/



			/**

			See: http://docs.daz3d.com/doku.php/public/software/dazstudio/4/referenceguide/interface/panes/surfaces/shaders/iray_uber_shader/shader_general_concepts/start
			See: https://www.deviantart.com/sickleyield/journal/Iray-Surfaces-And-What-They-Mean-519346747
			See: https://attachments.f95zone.to/2019/11/481480_Skin_Shading_Essentials.pdf

			For information on dual lobe check out the notes on "Next Generation Character Rendering" - http://www.iryoku.com/stare-into-the-future

			| Property                           | M | S | W | Type | Purpose |
			| ---------------------------------- | - | - | - | ---- | ------- |
			| Base Color / Diffuse Color         | X | X | X | CT   | Diffuse color both texture and color prop (note in daz shows as "Base Color" in our list it shows as "Diffuse Color"), uses "Oren-Nayar" model |
			| Metallicity / Metallic Weight      | X |   |   | DT   | Sets metalness 0=> non metal, 1=> metal |
			| Diffuse Weight                     |   |   | X | DT   | If 0, do not render diffuse tex/color, if 1 do, does not effect iray, we ignore this |
			| Diffuse Roughness                  | X | X | X | DT   | Seemingly affects roughness, but has almost no effect |
			| Diffuse Overlay Weight             | X | X | X | D    | If > 0 mixes the "Diffuse Overlay Color" into the diffuse on top (amongst other props, see below) |
			| ++ Diffuse Overlay Weight Squared  | X | X | X | B    | if on, take diffuse overlay weight and square the value (0.5 => 0.25) |
			| ++ Diffuse Overlay Color           | X | X | X | CT   |  applies a color (mixes into the diffuse channel on top of the existing) | 
			| ++ Diffuse Overlay Color Effect    | X | X | X | E    | Scatter Only/ Scatter Transmit / Scatter Transmit Intensity | 
			| ++ Diffuse Overlay Roughness       | X | X | X | D    | (same effect as Diffuse Roughness |
			| Translucency Weight                | X | X | X | D    | if > 0 makes the material translucent (see through) and enables the following |
			| ++ Base Color Effect               | X | X | X | E    | Scatter Only/ Scatter Transmit / Scatter Transmit Intensity | 
			| ++ Translucency Color              | X | X | X | CT   | Sets the color that the mat is translucent (like how the light passes through it) greatly effects the final color, washes out diffuse quite a bit |
			| ++ Invert Transmission Normal      | X | X | X | B    | ?Flips the normal? |
			| Dual Lobe Specular Weight          | X | X | X | D    | See GDC notes on Next Generateion Character Rendering for info, the gist is these are top coat specular highlights (primiarily used for skin), 0 => off, 1=>on at 100% |
			| ++ Dual Lobe Specular Reflectivity | X | X | X | D    | How much of this coat should the environment show up (how mirrored is it) |
			| ++ Specular Lobe 1 Roughness       | X |   | X | D    | 0=>smooth, 1=>rough |
			| ++ Specular Lobe 2 Roughness       | X |   | X | D    | 0=>smooth, 1=>rough |
			| ++ Specular Lobe 1 Glossiness      |   | X |   | D    | 0=>rough, 1=>smooth |
			| ++ Specular Lobe 2 Glossiness      |   | X |   | D    | 0=>rough, 1=>smooth |
			| ++ Dual Lobe Specular Ratio        | X | X | X | D    | A lerp between lobe 1 and 2, where 0 is just lobe 2, 1 is just lobe 1 (notice the flip there!) and 0.5 is 50% of each|
			| Glossy Layered Weight              | X | X |   | D    | 0 => Rough, 1 => Smooth, controls metal/spec paths for all gloss values below |
			| Glossy Weight                      |   |   | X | D    | 0 => Rough, 1 => Smooth If > 0 enables: Glossy Color, Glossy Color Effect, Glossy Roughness, Glossy Anisotropy, Backscattering Weight | 
			| ++ Glossy Color                    | X | X | X | CT   | Effects specular highlights, (on by default for Metal and Spec) |
			| ++ Glossy Color Effect             | X | X | X | E    | Scatter Only/ Scatter Transmit / Scatter Transmit Intensity (on by default for Metal and Spec) | 
			| ++ Glossy Roughness                | X | X | X | DT   | 0 => smooth, 1 => Rough (on by default for Metal and Spec) | 
			| ++ Glossy Reflectivity             | X |   |   | DT   | How much of the environment should be reflected in the gloss layer? 1=> high, 0=>off
			| ++++ Glossy Anisotropy             | X | X | X | DT   | |
			| ++++ Glossy Anisotropy Rotations   | X | X |   | DT   | |
			| ++ Backscattering Weight           | X | X | X | DT   | (on by default for Metal and Spec) | 
			| Share Glossy Inputs                | X | X | X | B    | Setting on or off had no effect? |
			| Glossy Specular                    |   | X |   | CT   | Affects the gloss epcular highlights |
			| Glossiness                         |   | X |   | DT   | Affects smoothness (0=>rough 1=>smooth), ignored if Glossy Layered Weight = 0
			| Refraction Index                   | X | X | X | D    | Used for clear surfaces, see Index of Refraction, defaults to 1.5 |
			| Refraction Weight                  | X | X |   | DT   | Used for clear surfaces, if > 0 this object is clear in some way (enable transparent shader) |
			| ++ Refraction Color                | X | X | X | C    | Kind of like a specular highlight color for the color being refracted, not quite though |
			| ++ Refraction Roughness            | X |   |   | DT   | How rough the refracted surface is |
			| ++ Refraction Glossiness           |   | X |   | DT   | How rough the refracted surface is |
			| ++ Abbe                            | X | X | X | D    | Used to detmine how much the light splits like with a prism, high values have low dispersion, low values have high dispersion, see: https://en.wikipedia.org/wiki/Abbe_number |
			| Base Thin Film                     | X | X | X | DT   | |
			| ++ Base Thin Film IOR              | X | X | X | DT   | |
			| Base Bump                          | X | X | X | DT   | A height map, you want to read both the texture and the value | 
			| Normal Map                         | X | X | X | DT   | A normal map, the value is the "strength" of the normal | 
			| Metallic Flakes Weight             | X | X | X | DT   | Enables a lot of flake options, we're ignoring this for now |
			| Top Coat Weight                    | X | X | X | DT   | Enables additional options, adds a 3rd layer to iray, ignored by us |
			| Thin Walled                        | X | X | X | B    | On for thin things like bubbles, hollow, etc, off for thick things like fluids and solids |
			| Emission Color                     | X | X | X | CT   | Classic emission, adds to the final composite color as a glow |
			| Cutout Opacity                     | X | X | X | DT   | "Opacity without Refraction" should not be used for transparent/translucent things, just things that are not there like classic cutouts, have found it's abused though and can treat like classic alpha handling |
			| Displacement Strength              | X | X | X | T    | Applies a classic displacement map (not supporetd by us yet) |
			| Horizontal Tile                    | X | X | X | D    | Defaults 1, uv tiling |
			| Horizontal Offset                  | X | X | X | D    | Defaults 0, uv offset |
			| Vertical Tile                      | X | X | X | D    | Defaults 1, uv tiling |
			| Vertical Offset                    | X | X | X | D    | Defaults 0, uv offset |
			| UV Set                             | X | X | X | D    | Used to specify an alternate uv set, such as with gen2 or similar |
			| Smooth                             | X | X | X | B    | On for most things, off for hard edges (think split normals such as glass/gems/etc) |
			| Angle                              | X | X | X | D    | Used for smoothing |
			| Round Corners Radius               | X | X | X | D    | Used for smoothing |
			| Round Corners Across Materials     | X | X | X | B    | Used for smoothing |
			| Round Corners Roundness            | X | X | X | D    | Used for smoothing |
			| Line Preview Color                 | X | X | X | C    | ignored by us |


			Types: B => Boolean,C => Color, D => Double, E => Enum, T => Texture
			  
			*/

			var matNameLower = dtuMaterial.MaterialName.ToLower();
			var assetNameLower = dtuMaterial.AssetName.ToLower();
			var valueLower = dtuMaterial.Value.ToLower();


			//There are 3 types of iray uber surfaces, which are represented by "Base Mixing"

			DTUBaseMixing baseMixing = DTUBaseMixing.Unknown;
			var baseMixingProp = dtuMaterial.Get("Base Mixing");
			var baseMixingVal = (baseMixingProp.Exists ? (int) baseMixingProp.Value.AsDouble : (int) DTUBaseMixing.PBRMetalRoughness);
			baseMixing = (DTUBaseMixing)baseMixingVal;

			string shaderName = "";

			//let's setup the flags we care about, we'll figoure out our path first

			//Is our material based on metal roughness workflow (mutually exclusive with isSpecular)
			bool isMetal = false;
			//Is our material based on specular gloss workflow (mutually exclusive with isMetal)
			bool isSpecular = false;
			//Is our material using a completely different model as it's hair?
			bool isHair = false;
			//Is our material using our skin shader instead (SSS/Translucent/Spec flow)
			bool isSkin = false;
			//Does our material have translucency (only valid for spec/gloss workflow for now)
			bool isTranslucent = false;
			//Used for things like corena, eyemoisture, etc
			bool isWet = false;

			//Can we see through our material (does not affect shader choice)
			bool isTransparent = false;
			//Should we render backfaces (does not affect shader choice)
			bool isDoubleSided = true;
			//Should we just not render this at all (note this early exits from this function as we don't need to setup any props)
			bool isInvisible = false;

			bool isSclera = false;

			


			//Let's load all the properties we might use, not all paths read all these values

			var diffuseColor = dtuMaterial.Get("Diffuse Color");
			var metallicWeight = dtuMaterial.Get("Metallic Weight");
			
			var diffuseWeight = dtuMaterial.Get("Diffuse Weight");
			var diffuseRougness = dtuMaterial.Get("Diffuse Roughness");

			var diffuseOverlayWeight = dtuMaterial.Get("Diffuse Overlay Weight");
			var diffuseOverlayWeightSquared = dtuMaterial.Get("Diffuse Overlay Weight Squared");
			var diffuseOverlayColor = dtuMaterial.Get("Diffuse Overlay Color");
			var diffuseOverlayEffect = dtuMaterial.Get("Diffuse Overlay Effect");
			var diffuseOverlayRoughness = dtuMaterial.Get("Diffuse Overlay Roughness");

			var translucencyWeight = dtuMaterial.Get("Translucency Weight");
			var translucencyColor = dtuMaterial.Get("Translucency Color");
			var invertTransmissionNormal = dtuMaterial.Get("Invert Transmission Normal");

			var dualLobeSpecularWeight = dtuMaterial.Get("Dual Lobe Specular Weight");
			var dualLobeSpecularReflectivity = dtuMaterial.Get("Dual Lobe Specular Reflectivity");
			var specularLobe1Roughness = dtuMaterial.Get("Specular Lobe 1 Roughness");
			var specularLobe2Roughness = dtuMaterial.Get("Specular Lobe 2 Roughness");
			var specularLobe1Glossiness = dtuMaterial.Get("Specular Lobe 1 Glossiness");
			var specularLobe2Glossiness = dtuMaterial.Get("Specular Lobe 2 Glossiness");
			var dualLobeSpecularRatio = dtuMaterial.Get("Dual Lobe Specular Ratio");

			var glossyLayeredWeight = dtuMaterial.Get("Glossy Layered Weight");
			var glossyWeight = dtuMaterial.Get("Glossy Weight");
			var glossyColor = dtuMaterial.Get("Glossy Color");
			var glossyRoughness = dtuMaterial.Get("Glossy Roughness");
			var glossySpecular = dtuMaterial.Get("Glossy Specular");
			var glossiness = dtuMaterial.Get("Glossiness");
			var refractionIndex = dtuMaterial.Get("Refraction Index");
			var refractionWeight = dtuMaterial.Get("Refraction Weight");
			var refractionRoughness = dtuMaterial.Get("Refraction Roughness");
			var refractionGlossiness = dtuMaterial.Get("Refraction Glossiness");

			//var baseThinFilm = dtuMaterial.Get("Base Thin Film");
			var bumpStrength = dtuMaterial.Get("Bump Strength");
			var normalMap = dtuMaterial.Get("Normal Map");
			var thinWalled = dtuMaterial.Get("Thin Walled");
			var emissionColor = dtuMaterial.Get("Emission Color");
			var cutoutOpacity = dtuMaterial.Get("Cutout Opacity");

			//we don't support these yet, but they're easy to add, need to apply a mat.SetTextureOffset/Scale to each texture we set
			var horizontalTile = dtuMaterial.Get("Horizontal Tiles");
			var horizontalOffset = dtuMaterial.Get("Horizontal Offset");
			var verticalTile = dtuMaterial.Get("Vertical Tiles");
			var verticalOffset = dtuMaterial.Get("Vertical Offset");
			//we only support uv0 atm, but we should support alternates, requires us keywording/updating the shaders or procedurally updating the meshes to copy this value to uv0
			var uvSet = dtuMaterial.Get("UV Set");



			//let's figure out what type of shader we're going to pick

			//Do we have any path specific config options to set before we create our material?
			switch(baseMixing)
			{
				case DTUBaseMixing.PBRMetalRoughness:
					isMetal = true;
					break;
				case DTUBaseMixing.PBRSpecularGlossiness:
					isSpecular = true;
					break;
				case DTUBaseMixing.Weighted:
					isSpecular = true;
					break;
			}
			
			//This will be set for things like corenas, eye moisture, glass, etc
			//isTransparent = translucencyWeight.Value.AsDouble > 0 || refractionWeight.Value.AsDouble > 0 || cutoutOpacity.TextureExists();

			//for now we're only assuming transparent if a cutout texture is present
			isTransparent = cutoutOpacity.TextureExists();
			isTranslucent = translucencyWeight.Float > 0f;

			isHair = IsDTUMaterialHair(dtuMaterial);
			isSkin = IsDTUMaterialSkin(dtuMaterial);
			isWet = IsDTUMaterialWet(dtuMaterial);
			isSclera = IsDTUMaterialSclera(dtuMaterial);

			//Swap shaders if we need to
			
			if(isHair)
			{
				shaderName = DTU_Constants.shaderNameHair;
			}
			else if(isSkin)
			{
				//if we're skin, force a specular workflow as well
				isSpecular = true;
				shaderName = DTU_Constants.shaderNameIraySkin;
				isDoubleSided = false;
				isTransparent = false;
				isTranslucent = true;
			}
			else if(isWet)
			{
				shaderName = DTU_Constants.shaderNameWet;
			}
			else
			{
				//If we're not hair or skin, let's see which other shader we should fall into

				if(isTranslucent)
				{
					if(isMetal)
					{
						UnityEngine.Debug.LogWarning("Using translucency with metal is not supported, swapping to specular instead for mat: " + dtuMaterial.MaterialName);
					}

					//if we're translucent, force into a specular workflow
					isSpecular = true;
					shaderName = DTU_Constants.shaderNameSpecular;
				} else if(isSpecular)
				{
					shaderName = DTU_Constants.shaderNameSpecular;
				}
				else if(isMetal)
				{
					shaderName = DTU_Constants.shaderNameMetal;
				}
				else {
					UnityEngine.Debug.LogError("Invalid material, we don't know what shader to pick");
					return null;
				}
			}

			//Now that we know which shader to use, go ahead and make the mat

			var shader = Shader.Find(shaderName);
			if(shader == null)
			{
				UnityEngine.Debug.LogError("Failed to locate shader: " + shaderName + " for mat: " + dtuMaterial.MaterialName);
				return null;
			}
			var mat = new Material(shader);

			if(horizontalTile.Exists && mat.HasProperty("_Tiling"))
			{
				var tiling = new Vector2(horizontalTile.Float,verticalTile.Float);
				mat.SetVector("_Tiling",tiling);
			}
			if(horizontalOffset.Exists && mat.HasProperty("_Offset"))
			{
				var offset = new Vector2(horizontalOffset.Float,verticalOffset.Float);
				mat.SetVector("_Offset",offset);
			}


			if(isSclera)
			{
				//scleras are a little dark on our side, so we'll brighten them up
				mat.SetFloat("_DiffuseMultiplier",2.5f);
			}

			var record = new Daz3DDTUImporter.ImportEventRecord();

			//Our prep is done, now we know which shader we're loading into

			if (isHair)
			{
				//Hairs are pretty simple b/c we only care about a few properties, so we're forking here to deal with it
				isDoubleSided = true;
				isTransparent = true;

				mat.SetColor("_Diffuse",diffuseColor.Color);
				mat.SetTexture("_DiffuseMap",ImportTextureFromPath(diffuseColor.Texture,materialDir, record));
				mat.SetTexture("_NormalMap",ImportTextureFromPath(normalMap.Texture,materialDir, record, true));
				mat.SetFloat("_NormalStrength",normalMap.Float);
				mat.SetFloat("_Height",bumpStrength.Float);
				mat.SetTexture("_HeightMap",ImportTextureFromPath(bumpStrength.Texture,materialDir, record, false, true));
				mat.SetFloat("_HeightOffset",0.25f);
				mat.SetTexture("_CutoutOpacityMap",ImportTextureFromPath(cutoutOpacity.Texture,materialDir, record, false, true));
				mat.SetTexture("_GlossyRoughnessMap",ImportTextureFromPath(glossyRoughness.Texture,materialDir, record, false, true));
				mat.SetFloat("_GlossyRoughness",glossyRoughness.Float);
				Color specularColor = Color.black;
				if(baseMixing == DTUBaseMixing.PBRSpecularGlossiness)
				{
					specularColor = glossySpecular.Color;
				} else {
					specularColor = glossyColor.Color;
				}
				mat.SetColor("_SpecularColor",specularColor);

				//A few magic values that work for most hairs
				mat.SetFloat("_AlphaStrength",1.5f);
				mat.SetFloat("_AlphaOffset",0.35f);
				mat.SetFloat("_AlphaClip",0.75f);
				mat.SetFloat("_AlphaPower",0.4f);
			}
			else if(isWet)
			{
				isDoubleSided = false;
				isTransparent = true;

				mat.SetFloat("_Alpha", 0f);
				mat.SetFloat("_Coat",0.25f);
				mat.SetFloat("_IndexOfRefraction",refractionIndex.Float);
				//TODO: we can pull data from the existing object, but for now these values work well
				mat.SetFloat("_Smoothness",0.97f);
				mat.SetFloat("_Normal",normalMap.Float);
				mat.SetTexture("_NormalMap",ImportTextureFromPath(normalMap.Texture,materialDir, record, true));
				mat.SetFloat("_Height",bumpStrength.Float);
				mat.SetTexture("_HeightMap",ImportTextureFromPath(bumpStrength.Texture,materialDir, record, false,true));
				mat.SetFloat("_HeightOffset",0.25f);
			}
			else 
			{
				//this means we're either skin, metal, spec, etc... 

				isTransparent = refractionWeight.Float > 0f || refractionWeight.TextureExists() || cutoutOpacity.TextureExists();

				//These properties are going to be parsed/interpretted in roughly the order that they appear in the table of iray props in the large comment block

				//Diffuse affects color, and it's handled the same in every path and shader we have
				mat.SetColor("_Diffuse",diffuseColor.Color);
				if(diffuseColor.TextureExists())
				{
					var tex = ImportTextureFromPath(diffuseColor.Texture,materialDir, record);
					mat.SetTexture("_DiffuseMap",tex);
				}

				//Metallic Weight affects the metalness and is only used with PBR Metal
				if(isMetal)
				{
					//If we're usign a metal workflow...
					mat.SetFloat("_Metallic", metallicWeight.Float);
					if(metallicWeight.TextureExists())
					{
						var tex = ImportTextureFromPath(metallicWeight.Texture,materialDir, record, false,true);
						mat.SetTexture("_MetallicMap",tex);
					}
				}
				else
				{
					//Spec/Gloss and Weighted don't use metalness in their shader, so we will clear them out if they do exist, though it doesn't matter
					if(mat.HasProperty("_Metallic"))
					{
						mat.SetFloat("_Metallic",0.0f);
					}
					if(mat.HasProperty("_MetallicaMap"))
					{
						mat.SetTexture("_MetallicaMap",null);
					}
				}

				if(baseMixing == DTUBaseMixing.Weighted)
				{
					if(diffuseOverlayWeight.Float > 0)
					{
						//mix the diffuse overlay color into the diffuse color, we're not going to blend in the other texture, we're already getting pretty high on the samplers
						var weight = diffuseOverlayWeight.Float;
						if(diffuseOverlayWeightSquared.Boolean)
						{
							weight *= weight;
						}
						var diffuseColorValue = Color.Lerp(diffuseColor.Color,diffuseOverlayColor.Color,weight);
						mat.SetColor("_Diffuse",diffuseColorValue);

						//We're ignoring the color texture, if set, and Diffuse Overlay Color Effect and Diffuse Overlay Roughness
					}
				}

				//For proper translucency support we need to wait until the hdrp is at 10.0 for general release
				// this unseals the DiffusionProfile class which means we won't need to use reflection to modify it
				// instead we're going to simulate translucency colors in the shader w/o using the profiles
				if(isTranslucent)
				{
					mat.SetColor("_TranslucencyColor",translucencyColor.Color);
					if(translucencyColor.TextureExists())
					{
						var tex = ImportTextureFromPath(translucencyColor.Texture,materialDir, record);
						mat.SetTexture("_TranslucencyColorMap",tex);
					}
				}
				mat.SetFloat("_TranslucencyWeight",translucencyWeight.Float);


				if(dualLobeSpecularWeight.Float > 0f)
				{
					// This means we're using dual lobe support, which is essentially an extra spec highlight commonly seen on skin
					// In Daz we have 2 lobe values that get blended using Dual Lobe Specular Ratio, where a value of 1 means read from
					//  the first lobe rough/gloss and a value of 0 means read from the second lobe rough/gloss
					// If we use the builtin dual lobe shader mat prop in the stacklit mat
					//  there are two smoothness values (smoothnessA and smoothnessB where B  is unlocked if using dual lobe)
					//  LobeMix will change the weight from A->B wtih 1.0 meaning only read from B and 0.0 meaning only read from A
					//  Our base smoothness is also contained in A, so what we need to do is merge the 2 spec lobes together
					//  then put them in smoothnessB and blend using the weight
					//  or just merge them all together and ignore unity's dual lobe prop

					if(!isSkin)
					{
						UnityEngine.Debug.LogWarning("Dual Lobe support is only available on the skin shader currently for mat: " + dtuMaterial.MaterialName);
					}

					if(mat.HasProperty("_DualLobeSpecularWeight")){
						mat.SetFloat("_DualLobeSpecularWeight",dualLobeSpecularWeight.Float);
						//mat.SetTexture("_DualLobeSpecularWeightMap",ImportTextureFromPath(dualLobeSpecularWeight.Texture,materialDir));
						mat.SetFloat("_DualLobeSpecularReflectivity",dualLobeSpecularReflectivity.Float);
						mat.SetTexture("_DualLobeSpecularReflectivityMap",ImportTextureFromPath(dualLobeSpecularReflectivity.Texture,materialDir,record,false,true));

						float specularLobe1RoughnessValue = 0f;
						float specularLobe2RoughnessValue = 0f;
						Texture specularLobe1RoughnessTexture = null;
						Texture specularLobe2RoughnessTexture = null;

						if(baseMixing == DTUBaseMixing.PBRSpecularGlossiness)
						{
							specularLobe1RoughnessValue = 1.0f - specularLobe1Glossiness.Float;
							specularLobe2RoughnessValue = 1.0f - specularLobe2Glossiness.Float;
							
							//In our gloss shader, ensure we do a one minus on this
							specularLobe1RoughnessTexture = ImportTextureFromPath(specularLobe1Glossiness.Texture,materialDir,record,false,true);
							specularLobe1RoughnessTexture = ImportTextureFromPath(specularLobe2Glossiness.Texture,materialDir,record,false,true);
						}
						else
						{
							specularLobe1RoughnessValue = specularLobe1Roughness.Float;
							specularLobe2RoughnessValue = specularLobe2Roughness.Float;

							specularLobe1RoughnessTexture = ImportTextureFromPath(specularLobe1Roughness.Texture,materialDir,record,false,true);
							specularLobe1RoughnessTexture = ImportTextureFromPath(specularLobe2Roughness.Texture,materialDir,record,false,true);
						}

						mat.SetFloat("_SpecularLobe1Roughness",specularLobe1RoughnessValue);
						mat.SetFloat("_SpecularLobe2Roughness",specularLobe2RoughnessValue);
						mat.SetFloat("_DualLobeSpecularRatio",dualLobeSpecularRatio.Float);
						mat.SetTexture("_SpecularLobe1RoughnessMap",specularLobe1RoughnessTexture);
						mat.SetTexture("_SpecularLobe2RoughnessMap",specularLobe2RoughnessTexture);
					} else {
						UnityEngine.Debug.LogWarning("Shader: " +shaderName + " doesn't support dual lobe support yet for mat: " + dtuMaterial.MaterialName);
					}
				}


				//roughness is a bit complicated and deviates heavily on the basemix and props set
				float glossyRoughnessValue = 0.0f;
				Texture glossyRoughessMap = null;

				float glossinessValue = 0.0f;
				Texture glossinessMap = null;

				if(
					(baseMixing == DTUBaseMixing.PBRMetalRoughness || baseMixing == DTUBaseMixing.PBRSpecularGlossiness)
					|| (baseMixing == DTUBaseMixing.Weighted &&  glossyWeight.Float > 0.0f)
				)
				{
					//if we have a glossy weight set, use values from these fields
					glossyRoughnessValue = glossyRoughness.Float;
					if(glossyRoughness.TextureExists())
					{
						glossyRoughessMap = ImportTextureFromPath(glossyRoughness.Texture,materialDir,record,false,true);
					}


					if(baseMixing == DTUBaseMixing.Weighted)
					{
						glossyRoughnessValue *= glossyRoughness.Float;
					}
				}

				switch(baseMixing)
				{
					case DTUBaseMixing.PBRMetalRoughness:
						//no gloss maps on metal...

						glossyRoughnessValue = Mathf.Lerp(1.0f,glossyRoughnessValue,glossyLayeredWeight.Float);
						break;
					case DTUBaseMixing.PBRSpecularGlossiness:
						glossyRoughnessValue = 1f - (glossiness.Float * glossyLayeredWeight.Float);
						//this is an inverted map where 1 is smooth and 0 is rough
						glossinessMap = ImportTextureFromPath(glossiness.Texture,materialDir,record,false,true);
						glossinessValue = glossiness.Float * glossyLayeredWeight.Float;
						break;
					case DTUBaseMixing.Weighted:
						//if glossy weight > 0 in iray it applies a glossy layer on top, you now need to pay attention to the glossyColor
						// we're not going to render the same way

						glossyRoughnessValue = 1f - glossyWeight.Float;
						if(glossyWeight.TextureExists())
						{
							//this is an inverted map where 1 is smooth and 0 is rough
							glossinessMap = ImportTextureFromPath(glossyWeight.Texture,materialDir,record,false,true);
							glossinessValue = glossyWeight.Float;
						}
						break;
				}

				var alpha = cutoutOpacity.Float;
				if(refractionWeight.Float > 0f){
					switch(baseMixing){
						case DTUBaseMixing.PBRMetalRoughness:
							alpha *= 1f - refractionWeight.Float;
							glossyRoughnessValue *= refractionRoughness.Float;
							break;
						case DTUBaseMixing.PBRSpecularGlossiness:
							alpha *= 1f - refractionWeight.Float;
							glossyRoughnessValue *= 1.0f - refractionGlossiness.Float;
							break;
					}
				}

				//This is only useful for clipping or transparent assets
				mat.SetFloat("_Alpha",alpha);
				mat.SetTexture("_AlphaMap",ImportTextureFromPath(cutoutOpacity.Texture,materialDir,record,false,true));

				mat.SetFloat("_Roughness",glossyRoughnessValue);
				mat.SetTexture("_RoughnessMap",glossyRoughessMap);

				if(isSpecular)
				{
					mat.SetColor("_SpecularColor",glossySpecular.Color);
					mat.SetTexture("_SpecularColorMap",ImportTextureFromPath(glossySpecular.Texture,materialDir, record));
					mat.SetFloat("_Glossiness",glossinessValue);
					mat.SetTexture("_GlossinessMap",glossinessMap);
				}

				//this only applies for some material types such as see thru mats
				if(refractionWeight.Float>0f)
				{
					mat.SetFloat("_IndexOfRefraction",refractionIndex.Float);
					mat.SetFloat("_IndexOfRefractionWeight",refractionWeight.Float);
				}


				//bump maps are like old school black/white bump maps, so we'll either plug them in directly or blend them into the normal map
				mat.SetFloat("_Height",bumpStrength.Float);
				mat.SetTexture("_HeightMap",ImportTextureFromPath(bumpStrength.Texture,materialDir,record,false,true));

				mat.SetFloat("_Normal",normalMap.Float);
				mat.SetTexture("_NormalMap",ImportTextureFromPath(normalMap.Texture,materialDir,record,true));

				//right now we're ignoring top coats

				mat.SetColor("_Emission",emissionColor.Color);
				mat.SetTexture("_EmissionMap",ImportTextureFromPath(emissionColor.Texture,materialDir, record));


				//TODO: support displacement maps and tessellation
				//TODO: support alternate uv sets (this can be done easier in code then in the shader though)
			}


			bool hasDualLobeSpecularWeight = dualLobeSpecularWeight.Float>0;
			bool hasDualLobeSpecularReflectivity = dualLobeSpecularReflectivity.Float>0;
			bool hasGlossyLayeredWeight = false;
			bool hasGlossyColor = false;
			int sortingPriority = 0;

			ToggleCommonMaterialProperties(ref mat,matNameLower,isTransparent,isDoubleSided, hasDualLobeSpecularWeight, hasDualLobeSpecularReflectivity,sortingPriority,hasGlossyLayeredWeight,hasGlossyColor);






			if(isSpecular)
			{
				mat.EnableKeyword("IRAYUBER_GLOSSYCOLORACTIVE");
			} else 
			{
				mat.DisableKeyword("IRAYUBER_GLOSSYCOLORACTIVE");
			}

			if (record.Tokens.Count > 0)
				Daz3DDTUImporter.EventQueue.Enqueue(record);

			return mat;
		}

		/// <summary>
		/// Converts a material using the Daz Studio Default shader type
		/// </summary>
		/// <param name="dTUMaterial"></param>
		/// <returns></returns>
		public Material ConvertToUnityDazStudioDefault(DTUMaterial dtuMaterial, string materialDir)
		{

			var lightingModel = dtuMaterial.Get("Lighting Model");
			var diffuseColor = dtuMaterial.Get("Diffuse Color");
			var diffuseStrength = dtuMaterial.Get("Diffuse Strength");
			var glossiness = dtuMaterial.Get("Glossiness");
			var specularColor = dtuMaterial.Get("Specular Color");
			var specularStrength = dtuMaterial.Get("Specular Strength");
			var multiplySpecularThroughOpacity = dtuMaterial.Get("Multiply Specular Through Opacity");
			var ambientColor = dtuMaterial.Get("Ambient Color");
			var ambientStrength = dtuMaterial.Get("Ambient Strength");
			var opacityStrength = dtuMaterial.Get("Opacity Strength");
			var bumpStrength = dtuMaterial.Get("Bump Strength");
			var negativeBump = dtuMaterial.Get("Negative Bump");
			var postiiveBump = dtuMaterial.Get("Positive Bump");
			var displacementStrength = dtuMaterial.Get("Displacement Strength");
			var minimumDisplacement = dtuMaterial.Get("Minimum Displacement");
			var maximumDisplacement = dtuMaterial.Get("Maximum Displacement");
			var normalMap = dtuMaterial.Get("Normal Map");
			var reflectionColor = dtuMaterial.Get("Reflection Color");
			var reflectionStrength = dtuMaterial.Get("Reflection Strength");
			var refractionColor = dtuMaterial.Get("Refraction Color");
			var refractionStrength = dtuMaterial.Get("Refraction Strength");
			var indexOfRefraction = dtuMaterial.Get("Index of Refraction");
			var sheenColor = dtuMaterial.Get("Sheen Color");
			var scatterColor = dtuMaterial.Get("Scatter Color");
			var thickness = dtuMaterial.Get("Thickness");


			var horizontalTile = dtuMaterial.Get("Horizontal Tiles");
			var horizontalOffset = dtuMaterial.Get("Horizontal Offset");
			var verticalTile = dtuMaterial.Get("Vertical Tiles");
			var verticalOffset = dtuMaterial.Get("Vertical Offset");
			var uvSet = dtuMaterial.Get("UV Set");



			//Fallback properties (not in the Daz Studio Default)
			var cutoutOpacity = dtuMaterial.Get("Cutout Opacity");


			/**
			Plastic, Metallic, GlossyPlastic, GLossyMetallic uses all but
				Sheen Color
				Scatter Color
				Thickness
			
			Skin uses all

			Matte uses all but
				Glossiness
				Specular Color
				Specular Strength
				Multiply Specular Through Opacity
				Reflection Color
				Reflection Strength
				Refraction Color
				Refraction Strength
				Index of Refraction
				Sheen Color
				Scatter Color
				Thickness
			
			Info from http://docs.daz3d.com/doku.php/artzone/pub/software/dazstudio/reference/st_lighting

			Plastic: The Plastic lighting model has additive Specular highlights. Highlights are generally very bright and reflect the color of light projected on to it.

			Metal: The Metal lighting model uses metallic (isotropic or elliptical) highlights, which are multiplied through the base surface color, thus tending to produce a slightly brighter tint of the same hue.

			Skin: The Skin lighting model uses sub-surface scattering to give the appearance of a semi-translucent layer (skin) with a blue sheen and a red opaque sub layer (blood/muscle).

			Glossy (Plastic): The Glossy (plastic) lighting model uses a Fresnel function to make the surface act more reflective at glancing angles. It also calculates specularity in a way that produces a more uniformly bright highlight (sharper), thus making the surface look glossy. Eyeballs are a great example.

			Matte: The Matte lighting model completely ignores the Specular channel.

			Glossy (Metallic): The Glossy (metallic) lighting model is similar to the Glossy (plastic) model, but the calculations are adjusted to produce a gloss that mimics the gloss of highly-polished metal.

			*/

			DTULightingModel shaderLightingModel = (DTULightingModel)lightingModel.Float;
			var shaderName = "";

			//Metal isn't really a "metal type" so we can just rely on the specular shader and skin shader instead
			switch(shaderLightingModel)
			{
				case DTULightingModel.Skin:
					shaderName = DTU_Constants.shaderNameIraySkin;
					break;
				default:
					shaderName = DTU_Constants.shaderNameSpecular;
					break;
			}
			
			bool isWet = IsDTUMaterialWet(dtuMaterial);

			if(isWet)
			{
				shaderName = DTU_Constants.shaderNameWet;
			}

			var shader = Shader.Find(shaderName);
			if(shader == null)
			{
				UnityEngine.Debug.LogError("Failed to locate shader: " + shaderName + " for mat: " + dtuMaterial.MaterialName);
				return null;
			}
			var mat = new Material(shader);
			
			var record = new Daz3DDTUImporter.ImportEventRecord();

			if(horizontalTile.Exists && mat.HasProperty("_Tiling"))
			{
				var tiling = new Vector2(horizontalTile.Float,verticalTile.Float);
				mat.SetVector("_Tiling",tiling);
			}
			if(horizontalOffset.Exists && mat.HasProperty("_Offset"))
			{
				var offset = new Vector2(horizontalOffset.Float,verticalOffset.Float);
				mat.SetVector("_Offset",offset);
			}


			bool isTransparent = opacityStrength.TextureExists() || opacityStrength.Float < 1.0f || (cutoutOpacity.Exists && cutoutOpacity.TextureExists());

			if (isWet)
			{

				isTransparent = true;

				mat.SetFloat("_Alpha", 0f);
				mat.SetFloat("_Coat",0.25f);
				mat.SetFloat("_IndexOfRefraction",indexOfRefraction.Float);
				//TODO: we can pull data from the existing object, but for now these values work well
				mat.SetFloat("_Smoothness",0.97f);
				mat.SetFloat("_Normal",normalMap.Float);
				mat.SetTexture("_NormalMap",ImportTextureFromPath(normalMap.Texture,materialDir, record, true));
				mat.SetFloat("_Height",bumpStrength.Float);
				mat.SetTexture("_HeightMap",ImportTextureFromPath(bumpStrength.Texture,materialDir, record, false,true));
				mat.SetFloat("_HeightOffset",0.25f);
			}
			else
			{


				mat.SetColor("_Diffuse",diffuseColor.Color);
				mat.SetTexture("_DiffuseMap",ImportTextureFromPath(diffuseColor.Texture,materialDir,record));

				//glossiness seems to have no real effect, so we'll just set everything to being rough
				mat.SetFloat("_Roughness",1.0f);

				mat.SetTexture("_NormalMap",ImportTextureFromPath(normalMap.Texture,materialDir, record, true));
				mat.SetFloat("_NormalStrength",normalMap.Float);
				mat.SetFloat("_Height",bumpStrength.Float);
				mat.SetTexture("_HeightMap",ImportTextureFromPath(bumpStrength.Texture,materialDir, record, false, true));
				mat.SetFloat("_HeightOffset",0.25f);
				mat.SetTexture("_AlphaMap",ImportTextureFromPath(opacityStrength.Texture,materialDir, record, false, true));
				mat.SetFloat("_Alpha",opacityStrength.Float);


				if(shaderLightingModel != DTULightingModel.Matte)
				{
					var specularColorValue = specularColor.Color;
					specularColorValue = Color.Lerp(Color.black,specularColorValue,specularStrength.Float);
					mat.SetColor("_SpecularColor",specularColorValue);
					mat.SetTexture("_SpecularColorMap",ImportTextureFromPath(specularColor.Texture,materialDir, record));

					mat.SetFloat("_SpecularStrength",specularStrength.Float);
					mat.SetTexture("_SpecularStrengthMap",ImportTextureFromPath(specularStrength.Texture,materialDir,record,false,true));
				}

				//mat.SetTexture("_AmbientOcclusionMap",ImportTextureFromPath(ambientColor.Texture,materialDir,record, false, true));

				if(shaderLightingModel == DTULightingModel.Skin)
				{
					//Sheen, Scatter, Thickness
					mat.SetTexture("_ThicknessMap",ImportTextureFromPath(thickness.Texture,materialDir,record,false,true));
				}


				if(mat.HasProperty("_Emission"))
				{
					var emissionColorValue = ambientColor.Color;

					mat.SetTexture("_EmissionMap",ImportTextureFromPath(ambientColor.Texture,materialDir,record));

					if(mat.HasProperty("_EmissionStrength"))
					{
						//if we support strength feed it directly
						mat.SetFloat("_EmissionStrength",ambientStrength.Float);
						mat.SetFloat("_EmissionExposureWeight",0.0f); //hardcoded

						if(mat.HasProperty("_EmissionStrengthMap"))
						{
							mat.SetTexture("_EmissionStrengthMap",ImportTextureFromPath(ambientStrength.Texture,materialDir,record,false,true));
						}
					}
					else
					{
						//if we don't multiply it into the base emission color (not quite right, but it's close enough), this means the maps will be wrong if used, but the color will be right
						emissionColorValue *= ambientStrength.Float;
					}
					mat.SetColor("_Emission",emissionColorValue);
				}
			}


			//Fallback handling
			if(cutoutOpacity.Exists && cutoutOpacity.TextureExists())
			{
				mat.SetFloat("_Alpha",cutoutOpacity.Float);
				mat.SetTexture("_AlphaMap",ImportTextureFromPath(cutoutOpacity.Texture,materialDir, record, false, true));
			}


			//TODO: DiffuseStrength, SpecularStrength, AmbientStrength, Neg/Pos Bump, Disp, Reflection, Refraction

			var matNameLower = dtuMaterial.MaterialName.ToLower();
			var assetNameLower = dtuMaterial.AssetName.ToLower();
			var valueLower = dtuMaterial.Value.ToLower();
			bool isDoubleSided = shaderLightingModel != DTULightingModel.Skin;
			bool isTranslucent = shaderLightingModel != DTULightingModel.Skin;

			bool hasDualLobeSpecularWeight = false;
			bool hasDualLobeSpecularReflectivity = false;
			bool hasGlossyLayeredWeight = false;
			bool hasGlossyColor = false;
			int sortingPriority = 0;

			ToggleCommonMaterialProperties(ref mat,matNameLower,isTransparent,isDoubleSided, hasDualLobeSpecularWeight, hasDualLobeSpecularReflectivity,sortingPriority,hasGlossyLayeredWeight,hasGlossyColor);

			mat.EnableKeyword("IRAYUBER_GLOSSYCOLORACTIVE");
		
			if (record.Tokens.Count > 0)
			{
				Daz3DDTUImporter.EventQueue.Enqueue(record);
			}


			return mat;
		}


		/// <summary>
		/// Converts a material using the PBR SP shader type
		/// </summary>
		/// <param name="dTUMaterial"></param>
		/// <returns></returns>
		public Material ConvertToUnityPBRSP(DTUMaterial dtuMaterial, string materialDir)
		{

			var metallicWeight = dtuMaterial.Get("Metallic Weight");
			var diffuseColor = dtuMaterial.Get("Diffuse Color");
			var glossyReflectivity = dtuMaterial.Get("Glossy Reflectivity");
			var glossyRoughness = dtuMaterial.Get("Glossy Roughness");
			var bumpStrength = dtuMaterial.Get("Bump Strength");
			var normalMap = dtuMaterial.Get("Normal Map");
			var cutoutOpacity = dtuMaterial.Get("Cutout Opacity");
			var roughnessSquared = dtuMaterial.Get("Roughness Squared");
			
			var horizontalTile = dtuMaterial.Get("Horizontal Tiles");
			var horizontalOffset = dtuMaterial.Get("Horizontal Offset");
			var verticalTile = dtuMaterial.Get("Vertical Tiles");
			var verticalOffset = dtuMaterial.Get("Vertical Offset");
			var uvSet = dtuMaterial.Get("UV Set");

			var shaderName = DTU_Constants.shaderNameMetal;

			var shader = Shader.Find(shaderName);
			if(shader == null)
			{
				UnityEngine.Debug.LogError("Failed to locate shader: " + shaderName + " for mat: " + dtuMaterial.MaterialName);
				return null;
			}
			var mat = new Material(shader);
			
			var record = new Daz3DDTUImporter.ImportEventRecord();


			if(horizontalTile.Exists && mat.HasProperty("_Tiling"))
			{
				var tiling = new Vector2(horizontalTile.Float,verticalTile.Float);
				mat.SetVector("_Tiling",tiling);
			}
			if(horizontalOffset.Exists && mat.HasProperty("_Offset"))
			{
				var offset = new Vector2(horizontalOffset.Float,verticalOffset.Float);
				mat.SetVector("_Offset",offset);
			}
			
			mat.SetColor("_Diffuse",diffuseColor.Color);
			mat.SetTexture("_DiffuseMap",ImportTextureFromPath(diffuseColor.Texture,materialDir,record));

			mat.SetTexture("_NormalMap",ImportTextureFromPath(normalMap.Texture,materialDir, record, true));
			mat.SetFloat("_NormalStrength",normalMap.Float);
			mat.SetFloat("_Height",bumpStrength.Float);
			mat.SetTexture("_HeightMap",ImportTextureFromPath(bumpStrength.Texture,materialDir, record, false, true));
			mat.SetFloat("_HeightOffset",0.25f);
			mat.SetTexture("_AlphaMap",ImportTextureFromPath(cutoutOpacity.Texture,materialDir, record, false, true));

			mat.SetFloat("_Roughness",glossyRoughness.Float);
			mat.SetTexture("_RoughnessMap",ImportTextureFromPath(glossyRoughness.Texture,materialDir,record,false,true));


			if(horizontalTile.Exists && mat.HasProperty("_Tiling"))
			{
				var tiling = new Vector2(horizontalTile.Float,verticalTile.Float);
				mat.SetVector("_Tiling",tiling);
			}
			if(horizontalOffset.Exists && mat.HasProperty("_Offset"))
			{
				var offset = new Vector2(horizontalOffset.Float,verticalOffset.Float);
				mat.SetVector("_Offset",offset);
			}


			var matNameLower = dtuMaterial.MaterialName.ToLower();
			var assetNameLower = dtuMaterial.AssetName.ToLower();
			var valueLower = dtuMaterial.Value.ToLower();
			bool isDoubleSided = true;
			bool isTranslucent = false;
			bool isTransparent = cutoutOpacity.TextureExists();

			bool hasDualLobeSpecularWeight = false;
			bool hasDualLobeSpecularReflectivity = false;
			bool hasGlossyLayeredWeight = false;
			bool hasGlossyColor = false;
			int sortingPriority = 0;

			ToggleCommonMaterialProperties(ref mat,matNameLower,isTransparent,isDoubleSided, hasDualLobeSpecularWeight, hasDualLobeSpecularReflectivity,sortingPriority,hasGlossyLayeredWeight,hasGlossyColor);

			if (record.Tokens.Count > 0)
			{
				Daz3DDTUImporter.EventQueue.Enqueue(record);
			}


			return mat;
		}



		/// <summary>
		/// Converts a material using the omUberSurface shader type
		/// </summary>
		/// <param name="dTUMaterial"></param>
		/// <returns></returns>
		public Material ConvertToUnityOmUberSurface(DTUMaterial dtuMaterial, string materialDir)
		{

			var diffuseColor = dtuMaterial.Get("Diffuse Color");
			var opacityStrength = dtuMaterial.Get("Opacity Strength");
			var bumpActive = dtuMaterial.Get("Bump Active");
			var bumpStrength = dtuMaterial.Get("Bump Strength");
			var bumpMinimum = dtuMaterial.Get("Bump Minimum");
			var bumpMaximum = dtuMaterial.Get("Bump Maximum");
			var displacementActive = dtuMaterial.Get("Displacement Active");
			var displacementMinimum = dtuMaterial.Get("Displacement Minimum");
			var displacementMaximum = dtuMaterial.Get("Displacement Maximum");
			var traceDisplacements = dtuMaterial.Get("Trace Displacements");
			var normalMap = dtuMaterial.Get("Normal Map");
			var diffuseActive = dtuMaterial.Get("Diffuse Active");
			var diffuseStrength = dtuMaterial.Get("Diffuse Strength");
			var diffuseRoughness = dtuMaterial.Get("Diffuse Roughness");
			var opacityActive = dtuMaterial.Get("Opacity Active");
			var opacityColor = dtuMaterial.Get("Opacity Color");
			var specularActive = dtuMaterial.Get("Specular Active");
			var specularColor = dtuMaterial.Get("Specular Color");
			var specularStrength = dtuMaterial.Get("Specular Strength");
			var glossiness = dtuMaterial.Get("Glossiness");
			var specularSharpness = dtuMaterial.Get("Specular Sharpness");
			var anisotropicActive = dtuMaterial.Get("Anisotropic Active");
			var anisotropicDirection = dtuMaterial.Get("Anisotropic Direction");
			var specular2Active = dtuMaterial.Get("Specular2 Active");
			var specular2Color = dtuMaterial.Get("Specular2 Color");
			var specular2Strength = dtuMaterial.Get("Specular2 Strength");
			var specular2Roughness = dtuMaterial.Get("Specular2 Roughness");
			var specular2Sharpness = dtuMaterial.Get("Specular2 Sharpness");
			var anisotropic2Active = dtuMaterial.Get("Anisotropic2 Active");
			var anisotropic2Direction = dtuMaterial.Get("Anisotropic2 Direction");
			var multiplySpecularThroughOpacity = dtuMaterial.Get("Multiply Specular Through Opacity");
			var amibentActive = dtuMaterial.Get("Ambient Active");
			var ambientColor = dtuMaterial.Get("Ambient Color");
			var ambientStrength = dtuMaterial.Get("Ambient Strength");
			var refractionActive = dtuMaterial.Get("Refraction Active");
			var indexOfRefraction = dtuMaterial.Get("Index of Refraction");
			var reflectionActive = dtuMaterial.Get("Reflection Active");
			var reflectionMode = dtuMaterial.Get("Reflection Mode");
			var reflectionEnvironmentMap = dtuMaterial.Get("Reflection Environment Map (Map in Lat/Long format)");
			var reflectionColor = dtuMaterial.Get("Reflection Color");
			var reflectionStrength = dtuMaterial.Get("Reflection Strength");
			var reflectionBlur = dtuMaterial.Get("Reflection Blur");
			var reflectionBlurSamples = dtuMaterial.Get("Reflection Blur Samples");
			var multiplyReflectionThroughOpacity = dtuMaterial.Get("Multiply Reflection Through Opacity");
			var fresnelActive = dtuMaterial.Get("Fresnel Active");
			var fresnelStrength = dtuMaterial.Get("Fresnel Strength");
			var fresnelFalloff = dtuMaterial.Get("Fresnel Falloff");
			var fresneslSharpness = dtuMaterial.Get("Fresnel Sharpness");
			var velvetActive = dtuMaterial.Get("Velvet Active");
			var velvetColor = dtuMaterial.Get("Velvet Color");
			var velvetStrength = dtuMaterial.Get("Velvet Strength");
			var velvetFalloff = dtuMaterial.Get("Velvet Falloff");
			var subsurfaceActive = dtuMaterial.Get("Subsurface Active");
			var subsurfaceStrength = dtuMaterial.Get("Subsurface Strength");
			var subsurfaceColor = dtuMaterial.Get("Subsurface Color");
			var subsurfaceRefraction = dtuMaterial.Get("Subsurface Refraction");
			var subsurfaceScale = dtuMaterial.Get("Subsurface Scale");
			var subsurfaceGroup = dtuMaterial.Get("Subsurface Group");
			var subsurfaceShadingRate = dtuMaterial.Get("Subsurface Shading Rate");
			var translucencyActive = dtuMaterial.Get("Translucency Active");
			var translucencyColor = dtuMaterial.Get("Translucency Color");
			var translucencyStrength = dtuMaterial.Get("Translucency Strength");
			var fantom = dtuMaterial.Get("Fantom");
			var raytrace = dtuMaterial.Get("Raytrace");
			var acceptShadows = dtuMaterial.Get("Accept Shadows");
			var occlusion = dtuMaterial.Get("Occlusion");
			var occlusionShadingRateMode = dtuMaterial.Get("Occlusion Shading Rate Mode");
			
			var horizontalTile = dtuMaterial.Get("Horizontal Tiles");
			//var horizontalTile = dtuMaterial.Get("Map Tiling U");
			//var horizontalOffset = dtuMaterial.Get("Horizontal Offset");
			//var verticalTile = dtuMaterial.Get("Vertical Tiles");
			var verticalTile = dtuMaterial.Get("Map Tiling V");
			//var verticalOffset = dtuMaterial.Get("Vertical Offset");
			var uvSet = dtuMaterial.Get("UV Set");





			bool isHair = IsDTUMaterialHair(dtuMaterial);
			bool isSkin = IsDTUMaterialSkin(dtuMaterial);
			bool isWet = IsDTUMaterialWet(dtuMaterial);
			bool isSclera = IsDTUMaterialSclera(dtuMaterial);

			var matNameLower = dtuMaterial.MaterialName.ToLower();
			var assetNameLower = dtuMaterial.AssetName.ToLower();
			var valueLower = dtuMaterial.Value.ToLower();
			bool isDoubleSided = true;
			bool isTranslucent = false;
			bool isTransparent = opacityActive.Float > 0f && opacityStrength.TextureExists();
			bool isSpecular = true;


			//this shader uses specular workflow, so we'll match with it
			var shaderName = DTU_Constants.shaderNameSpecular;


			if(isHair)
			{
				shaderName = DTU_Constants.shaderNameHair;
			}
			else if(isSkin)
			{
				shaderName = DTU_Constants.shaderNameIraySkin;
				isDoubleSided = false;
				isTransparent = false;
				isTranslucent = true;
			}
			else if(isWet)
			{
				shaderName = DTU_Constants.shaderNameWet;
			}

			var shader = Shader.Find(shaderName);
			if(shader == null)
			{
				UnityEngine.Debug.LogError("Failed to locate shader: " + shaderName + " for mat: " + dtuMaterial.MaterialName);
				return null;
			}
			var mat = new Material(shader);
			
			var record = new Daz3DDTUImporter.ImportEventRecord();
			if(horizontalTile.Exists && mat.HasProperty("_Tiling"))
			{
				var tiling = new Vector2(horizontalTile.Float,verticalTile.Float);
				mat.SetVector("_Tiling",tiling);
			}
			//if(horizontalOffset.Exists && mat.HasProperty("_Offset"))
			//{
			//	var offset = new Vector2(horizontalOffset.Float,verticalOffset.Float);
			//	mat.SetVector("_Offset",offset);
			//}



			//TODO: opacityColor defines the "opaque" value, in most cases I've seen it's white which is what we'd expect with most maps, but we need to add support for this at some point
			

			//TODO: we handle this nearly the same as iray uber, these switches should be combined instead and have the same logic applied

			if(isSclera)
			{
				//scleras are a little dark on our side, so we'll brighten them up
				mat.SetFloat("_DiffuseMultiplier",2.5f);
			}

			if (isHair)
			{
				//Hairs are pretty simple b/c we only care about a few properties, so we're forking here to deal with it
				isDoubleSided = true;
				isTransparent = true;

				mat.SetColor("_Diffuse",diffuseColor.Color);
				mat.SetTexture("_DiffuseMap",ImportTextureFromPath(diffuseColor.Texture,materialDir, record));
				mat.SetTexture("_NormalMap",ImportTextureFromPath(normalMap.Texture,materialDir, record, true));
				mat.SetFloat("_NormalStrength",normalMap.Float);
				mat.SetFloat("_Height",bumpStrength.Float);
				mat.SetTexture("_HeightMap",ImportTextureFromPath(bumpStrength.Texture,materialDir, record, false, true));
				mat.SetFloat("_HeightOffset",0.25f);
				mat.SetTexture("_CutoutOpacityMap",ImportTextureFromPath(opacityStrength.Texture,materialDir, record, false, true));

				mat.SetTexture("_GlossyRoughnessMap",ImportTextureFromPath(glossiness.Texture,materialDir, record, false, true));
				mat.SetFloat("_GlossyRoughness",glossiness.Float);

				mat.SetColor("_SpecularColor",specularColor.Color);

				//A few magic values that work for most hairs
				mat.SetFloat("_AlphaStrength",1.5f);
				mat.SetFloat("_AlphaOffset",0.35f);
				mat.SetFloat("_AlphaClip",0.75f);
				mat.SetFloat("_AlphaPower",0.4f);
			}
			else if(isWet)
			{
				isDoubleSided = false;
				isTransparent = true;

				mat.SetFloat("_Alpha", 0f);
				mat.SetFloat("_Coat",0.25f);
				mat.SetFloat("_IndexOfRefraction",indexOfRefraction.Float);
				//TODO: we can pull data from the existing object, but for now these values work well
				mat.SetFloat("_Smoothness",0.97f);
				mat.SetFloat("_Normal",normalMap.Float);
				mat.SetTexture("_NormalMap",ImportTextureFromPath(normalMap.Texture,materialDir, record, true));
				mat.SetFloat("_Height",bumpStrength.Float);
				mat.SetTexture("_HeightMap",ImportTextureFromPath(bumpStrength.Texture,materialDir, record, false,true));
				mat.SetFloat("_HeightOffset",0.25f);
			}
			else 
			{
				mat.SetColor("_Diffuse",diffuseColor.Color);
				mat.SetTexture("_DiffuseMap",ImportTextureFromPath(diffuseColor.Texture,materialDir, record));
				
				if(opacityActive.Float > 0f)
				{
					mat.SetFloat("_Alpha",opacityStrength.Float);
					mat.SetTexture("_AlphaMap",ImportTextureFromPath(opacityStrength.Texture,materialDir, record, false, true));
				}
				mat.SetTexture("_GlossyRoughnessMap",ImportTextureFromPath(glossiness.Texture,materialDir, record, false, true));
				mat.SetFloat("_GlossyRoughness",glossiness.Float);


				mat.SetTexture("_NormalMap",ImportTextureFromPath(normalMap.Texture,materialDir, record, true));
				mat.SetFloat("_Normal",normalMap.Float);

				if(bumpActive.Float > 0f)
				{
					mat.SetFloat("_Height",bumpStrength.Float);
					mat.SetTexture("_HeightMap",ImportTextureFromPath(bumpStrength.Texture,materialDir, record, false, true));
					mat.SetFloat("_HeightOffset",0.25f);
				}

				if(specularActive.Float> 0f)
				{
					mat.SetColor("_SpecularColor",specularColor.Color);
					mat.SetTexture("_SpecularColorMap",ImportTextureFromPath(specularColor.Texture,materialDir,record));

					mat.SetFloat("_SpecularStrength",specularStrength.Float);
					mat.SetTexture("_SpecularStrengthMap",ImportTextureFromPath(specularStrength.Texture,materialDir,record,false,true));
				}

				if(amibentActive.Float > 0f)
				{
					mat.SetColor("_Emission",ambientColor.Color);
					mat.SetTexture("_EmissionMap",ImportTextureFromPath(ambientColor.Texture,materialDir,record));
					mat.SetFloat("_EmissionStrength",ambientStrength.Float);
					mat.SetTexture("_EmissionStrengthMap",ImportTextureFromPath(ambientStrength.Texture,materialDir,record,false,true));
					mat.SetFloat("_EmissionExposureWeight",0.0f); //hardcoded
				}

				mat.SetTexture("_AmbientOcclusionMap",ImportTextureFromPath(occlusion.Texture,materialDir,record,false,true));
			}


			bool hasDualLobeSpecularWeight = false;
			bool hasDualLobeSpecularReflectivity = false;
			bool hasGlossyLayeredWeight = false;
			bool hasGlossyColor = false;
			int sortingPriority = 0;

			ToggleCommonMaterialProperties(ref mat,matNameLower,isTransparent,isDoubleSided, hasDualLobeSpecularWeight, hasDualLobeSpecularReflectivity,sortingPriority,hasGlossyLayeredWeight,hasGlossyColor);


			//omUberSurface is a glossiness/smoothness shader, not roughness, so we need to flip
			mat.EnableKeyword("ROUGHNESS_IS_SMOOTHNESS_ON");

			if (record.Tokens.Count > 0)
			{
				Daz3DDTUImporter.EventQueue.Enqueue(record);
			}


			return mat;

		}
		
		public Material ConvertToUnityOOTHairblendingHair(DTUMaterial dtuMaterial, string materialDir)
		{
			//This material type is used for hair in Daz so we can make a few assumptions
			//there are a lot of properties in this shader but we only support a few here

			var diffuseWeight = dtuMaterial.Get("Diffuse Weight");
			var diffuseColor = dtuMaterial.Get("Diffuse Color");
			var diffuseRoughness = dtuMaterial.Get("Diffuse Roughness");
			var translucencyWeight = dtuMaterial.Get("Translucency Weight");
			var baseColorEffect = dtuMaterial.Get("Base Color Effect");
			var translucencyColor = dtuMaterial.Get("Translucency Color");
			var sssReflectanceTint = dtuMaterial.Get("SSS Reflectance Tint");
			var glossyWeight = dtuMaterial.Get("Glossy Weight");
			var shareGlossyInputs = dtuMaterial.Get("Share Glossy Inputs");
			var glossyColor = dtuMaterial.Get("Glossy Color");
			var glossyColorEffect = dtuMaterial.Get("Glossy Color Effect");
			var glossyRoughness = dtuMaterial.Get("Glossy Roughness");
			var glossyAnisotropy = dtuMaterial.Get("Glossy Anisotropy");
			var backscatteringWeight = dtuMaterial.Get("Backscattering Weight");
			var backscatteringColor = dtuMaterial.Get("Backscattering Color");
			var backscatteringRoughness = dtuMaterial.Get("Backscattering Roughness");
			var backscatteringAnisotropy = dtuMaterial.Get("Backscattering Anisotropy");
			var refractionIndex = dtuMaterial.Get("Refraction Index");
			var abbe = dtuMaterial.Get("Abbe");
			var refractionWeight = dtuMaterial.Get("Refraction Weight");
			var refractionColor = dtuMaterial.Get("Refraction Color");
			var refractionRoughness = dtuMaterial.Get("Refraction Roughness");
			var glossyAnisotropyRotations = dtuMaterial.Get("Glossy Anisotropy Rotations");
			var bumpStrength = dtuMaterial.Get("Bump Strength");
			var normalMap = dtuMaterial.Get("Normal Map");
			var topCoatWeight = dtuMaterial.Get("Top Coat Weight");
			var topCoatColor = dtuMaterial.Get("Top Coat Color");
			var topCoatColorEffect = dtuMaterial.Get("Top Coat Color Effect");
			var topCoatRoughness = dtuMaterial.Get("Top Coat Roughness");
			var topCoatAnisotropy = dtuMaterial.Get("Top Coat Anisotropy");
			var topCoatRotations = dtuMaterial.Get("Top Coat Rotations");
			var topCoatBumpMode = dtuMaterial.Get("Top Coat Bump Mode");
			var thinWalled = dtuMaterial.Get("Thin Walled");
			var transmittedMeasurementDistance = dtuMaterial.Get("Transmitted Measurement Distance");
			var transmittedColor = dtuMaterial.Get("Transmitted Color");
			var scatteringMeasurementDistance = dtuMaterial.Get("Scattering Measurement Distance");
			var sssAmount = dtuMaterial.Get("SSS Amount");
			var sssDirection = dtuMaterial.Get("SSS Direction");
			var cutoutOpacity = dtuMaterial.Get("Cutout Opacity");
			var displacementStrength = dtuMaterial.Get("Displacement Strength");
			var minimumDisplacement = dtuMaterial.Get("Minimum Displacement");
			var maximumDisplacement = dtuMaterial.Get("Maximum Displacement");
			var subDDisplacementLevel = dtuMaterial.Get("SubD Displacement Level");

			var roughnessSquared = dtuMaterial.Get("Roughness Squared");
			var glossyOverlayColor = dtuMaterial.Get("Glossy Overlay Color");
			var translucencyOverlayColor = dtuMaterial.Get("Translucency Overlay Color");
			var diffuseOverlayColor = dtuMaterial.Get("Diffuse Overlay Color");
			var overlayMode = dtuMaterial.Get("Overlay Mode");
			var overlayAlphaMask = dtuMaterial.Get("Overlay Alpha Mask");
			var topCoatOverlayColor = dtuMaterial.Get("Top Coat Overlay Color");


			var horizontalTile = dtuMaterial.Get("Horizontal Tiles");
			var horizontalOffset = dtuMaterial.Get("Horizontal Offset");
			var verticalTile = dtuMaterial.Get("Vertical Tiles");
			var verticalOffset = dtuMaterial.Get("Vertical Offset");
			var uvSet = dtuMaterial.Get("UV Set");


			var matNameLower = dtuMaterial.MaterialName.ToLower();
			var assetNameLower = dtuMaterial.AssetName.ToLower();
			var valueLower = dtuMaterial.Value.ToLower();
			
			string shaderName = DTU_Constants.shaderNameHair;
			var shader = Shader.Find(shaderName);
			if(shader == null)
			{
				UnityEngine.Debug.LogError("Failed to locate shader: " + shaderName + " for mat: " + dtuMaterial.MaterialName);
				return null;
			}
			var mat = new Material(shader);
			var record = new Daz3DDTUImporter.ImportEventRecord();


			bool isDoubleSided = true;
			bool isTransparent = true;





			mat.SetColor("_Diffuse",diffuseColor.Color);
			mat.SetTexture("_DiffuseMap",ImportTextureFromPath(diffuseColor.Texture,materialDir, record));
			mat.SetTexture("_NormalMap",ImportTextureFromPath(normalMap.Texture,materialDir, record, true));
			mat.SetFloat("_NormalStrength",normalMap.Float);
			mat.SetFloat("_Height",bumpStrength.Float);
			mat.SetTexture("_HeightMap",ImportTextureFromPath(bumpStrength.Texture,materialDir, record, false, true));
			mat.SetFloat("_HeightOffset",0.25f);
			mat.SetTexture("_CutoutOpacityMap",ImportTextureFromPath(cutoutOpacity.Texture,materialDir, record, false, true));
			mat.SetTexture("_GlossyRoughnessMap",ImportTextureFromPath(glossyRoughness.Texture,materialDir, record, false, true));
			mat.SetFloat("_GlossyRoughness",glossyRoughness.Float);

			mat.SetTexture("_SpecularMap",ImportTextureFromPath(glossyColor.Texture,materialDir,record));
			mat.SetColor("_SpecularColor",glossyColor.Color);

			//A few magic values that work for most hairs
			mat.SetFloat("_AlphaStrength",1.5f);
			mat.SetFloat("_AlphaOffset",0.35f);
			mat.SetFloat("_AlphaClip",0.75f);
			mat.SetFloat("_AlphaPower",0.4f);


			bool hasDualLobeSpecularWeight = false;
			bool hasDualLobeSpecularReflectivity = false;
			bool hasGlossyLayeredWeight = false;
			bool hasGlossyColor = false;
			int sortingPriority = 0;

			ToggleCommonMaterialProperties(ref mat,matNameLower,isTransparent,isDoubleSided, hasDualLobeSpecularWeight, hasDualLobeSpecularReflectivity,sortingPriority,hasGlossyLayeredWeight,hasGlossyColor);

			if (record.Tokens.Count > 0)
			{
				Daz3DDTUImporter.EventQueue.Enqueue(record);
			}

			return mat;

		}
		

		/// <summary>
		/// Creates a unity material (and the physical asset on disk defined by the GetMaterialDir) from the json record inside a .dtu file
		/// </summary>
		/// <param name="dtuMaterial">The DTUMaterial object that exists in the array of mats inside the .dtu file</param>
		/// <returns></returns>
		public Material ConvertToUnity(DTUMaterial dtuMaterial)
		{
			var materialDir = GetMaterialDir(dtuMaterial);
			if(!System.IO.Directory.Exists(materialDir))
			{
				System.IO.Directory.CreateDirectory(materialDir);
			}
			var materialPath = materialDir + "/" + Utilities.ScrubKey(dtuMaterial.MaterialName) + ".mat";



			DTUMaterialType materialType = DTUMaterialType.Unknown;

			//Look at the shader name from Daz and see if it's one we are familiar with

			if(dtuMaterial.MaterialType == "PBR SP")
			{
				/**
				 * Properties
				 *
				 * Metallic Weight (w/ texture)
				 * Diffuse Color (w/ texture)
				 * Glossy Reflectivity (0.5)
				 * Glossy Roughness (0.5)
				 * Bump Strength (1.0)
				 * Normal Map (w/ texture)
				 * Cutout Opacity
				 * Roughness Squared (0)
				 */
				materialType = DTUMaterialType.PBRSP;

			}
			else if(dtuMaterial.MaterialType == "Iray Uber" || dtuMaterial.MaterialType == "Front")
			{
				materialType = DTUMaterialType.IrayUber;
			}
			else if(dtuMaterial.MaterialType == "DAZ Studio Default")
			{
				materialType = DTUMaterialType.DazStudioDefault;
			}
			else if(dtuMaterial.MaterialType == "omUberSurface" || dtuMaterial.MaterialType == "omHumanSurface")
			{
				materialType = DTUMaterialType.OmUberSurface;
			}
			else if(dtuMaterial.MaterialType == "OOT Hairblending Hair" || (dtuMaterial.MaterialType == "Cap" && dtuMaterial.Get("Cap Base Texture").Exists))
			{
				materialType = DTUMaterialType.OOTHairblendingHair;
			}
			else
			{
				//If we don't know what it is, we'll just try, but it's quite possible it won't work
				UnityEngine.Debug.LogWarning("Unknown material type: " + dtuMaterial.MaterialType + " for mat: " + dtuMaterial.MaterialName + " using default");
				materialType = DTUMaterialType.DazStudioDefault;
				//return null;
			}


			//Now we'll go to custom functions for specific shaders if we have one, otherwise we'll use a fallback that is generic

			if(materialType == DTUMaterialType.IrayUber)
			{
				//If we are using the Iray Uber shader, we have a different function to handle this
				var uberMat = ConvertToUnityIrayUber(dtuMaterial, materialDir);
				if(uberMat != null)
				{
					SaveMaterialAsAsset(uberMat,materialPath);
					return uberMat;
				}
			} else if(materialType == DTUMaterialType.DazStudioDefault)
			{
				var defaultMat = ConvertToUnityDazStudioDefault(dtuMaterial, materialDir);
				if(defaultMat != null)
				{
					SaveMaterialAsAsset(defaultMat,materialPath);
					return defaultMat;
				}
			} else if(materialType == DTUMaterialType.PBRSP)
			{
				var pbrspMat = ConvertToUnityPBRSP(dtuMaterial,materialDir);
				if(pbrspMat != null)
				{
					SaveMaterialAsAsset(pbrspMat,materialPath);
					return pbrspMat;
				}
			} else if(materialType == DTUMaterialType.OmUberSurface)
			{
				var localMat = ConvertToUnityOmUberSurface(dtuMaterial,materialDir);
				if(localMat != null)
				{
					SaveMaterialAsAsset(localMat,materialPath);
					return localMat;
				}
			} else if(materialType == DTUMaterialType.OOTHairblendingHair)
			{
				var localMat = ConvertToUnityOOTHairblendingHair(dtuMaterial,materialDir);
				if(localMat != null)
				{
					SaveMaterialAsAsset(localMat,materialPath);
					return localMat;
				}
			}

			UnityEngine.Debug.LogError("Unsupported materialType: " + materialType + " raw shade ris: " + dtuMaterial.MaterialType);
			return null;
		}

		public void SaveMaterialAsAsset(Material mat, string materialPath)
		{
			if(mat == null)
			{
				return;
			}

			UnityEngine.Debug.Log("Creating mat: " + mat.name + " at : " + materialPath);
			AssetDatabase.CreateAsset(mat,materialPath);

			//Works around a bug in HDRP, see: https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@7.1/manual/Creating-and-Editing-HDRP-Shader-Graphs.html "Known Issues"
			UnityEditor.Rendering.HighDefinition.HDShaderUtils.ResetMaterialKeywords(mat);
		}

		private static DTUMaterialProperty ExtractDTUMatProperty(ref DTUMaterial dtuMaterial, string key)
		{
			DTUMaterialProperty result;
			if (!dtuMaterial.Map.TryGetValue(key, out result))
				Debug.LogWarning("'" + key + "' property not found in Material: " + dtuMaterial.MaterialName);
			return result;
		}

		public string GetMaterialDir(DTUMaterial material)
		{
			var name = Utilities.ScrubKey(material.AssetName);
			return DTUDir + "/" + name;
		}


		/// <summary>
		/// Locates and or copies textures into the asset location
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public Texture2D ImportTextureFromPath(string path, string localAssetDir, Daz3DDTUImporter.ImportEventRecord record,
			bool isNormal = false, bool isLinear = false )
		{
			if(string.IsNullOrEmpty(path))
			{
				return null;
			}

			if(!System.IO.File.Exists(path))
			{
				UnityEngine.Debug.LogWarning("Asking to import texture: " + path + " but file does not exist");
				return null;
			}

			var dirname = System.IO.Path.GetDirectoryName(path);
			var filename = System.IO.Path.GetFileName(path);
			filename = Utilities.ScrubPath(filename);

			var md5Remote = Utilities.MD5(path);

			bool copyRemtoe = true;

			//Does this file already exist locally?
			var cleanPath = localAssetDir + "/" + filename;
			if(System.IO.File.Exists(cleanPath))
			{
				var md5Local = Utilities.MD5(cleanPath);

				if(md5Remote == md5Local)
				{
					copyRemtoe = false;
				}
			}
			
			bool dirty = false;




			if (copyRemtoe)
			{
				UnityEngine.Debug.Log("Copying file: " + path);
				System.IO.File.Copy(path,cleanPath);
				AssetDatabase.Refresh();
			}
			

			var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(cleanPath);
			var ti = TextureImporter.GetAtPath(cleanPath) as TextureImporter;

			if(ti == null)
			{
				UnityEngine.Debug.LogWarning("Failed to get a texture importer for path: " + cleanPath + " verify texture has the correct settings manually");
			} else {
				if(isNormal)
				{
					if(ti.textureType != TextureImporterType.NormalMap)
					{
						ti.textureType = TextureImporterType.NormalMap;
						dirty = true;
					}
					ti.textureCompression = TextureImporterCompression.Compressed;
				}

				if(ti.sRGBTexture != !isLinear)
				{
					ti.sRGBTexture = !isLinear;
					dirty = true;
				}


				if(dirty)
				{
					//This forces a reimport properly (it will immediately force a texture compression)
					ti.SaveAndReimport();
				}

				if (copyRemtoe)
				{
					record?.AddToken("Imported " + ti.textureType + " texture");
					record?.AddToken(tex.name, tex);
					record?.AddToken(" from Daz3D Studio assets folder " + path, null, true);
				}
			}

			return tex;
		}
	}

	public struct DTUMaterial
	{
		public float Version;
		public string AssetName;
		public string MaterialName;
		public string MaterialType;
		public string Value;

		public List<DTUMaterialProperty> Properties;

		private Dictionary<string,DTUMaterialProperty> _map;

		public Dictionary<string,DTUMaterialProperty> Map 
		{
			get
			{
				if(_map == null || _map.Count == 0)
				{
					_map = new Dictionary<string, DTUMaterialProperty>();
					foreach(var prop in Properties)
					{
						_map[prop.Name] = prop;
					}
				}

				return _map;
			}
		}

		public DTUMaterialProperty Get(string key)
		{
			if(Map.ContainsKey(key))
			{
				return Map[key];
			}
			return new DTUMaterialProperty();
		}

	}

	public struct DTUMaterialProperty
	{
		public string Name;
		public DTUValue Value;
		public string Texture;
		public bool TextureExists() { return !string.IsNullOrEmpty(Texture); }

		/// <summary>
		/// True if this property was found in the DTU
		/// </summary>
		public bool Exists;

		public Color Color 
		{
			get {
				return Value.AsColor;
			}
		}

		public float ColorStrength
		{
			get {
				return Daz3D.Utilities.GetStrengthFromColor(Color);
			}
		}

		public float Float
		{
			get {
				return (float)Value.AsDouble;
			}
		}

		public bool Boolean
		{
			get {
				return Value.AsDouble > 0.5;
			}
		}
	}

	public struct DTUValue
	{
		public enum DataType {
			Integer,
			Float,
			Double,
			Color,
			String,
			Texture,
		};

		public DataType Type;

		public int AsInteger;
		public float AsFloat;
		public double AsDouble;
		public Color AsColor;
		public string AsString;

		public override string ToString()
		{
			switch(Type)
			{
				case DataType.Integer:
					return "int:"+AsInteger.ToString();
				case DataType.Float:
					return "float:"+AsFloat.ToString();
				case DataType.Double:
					return "double:"+AsDouble.ToString();
				case DataType.Color:
					return "color:"+AsColor.ToString();
				case DataType.String:
					return AsString;
				default:
					throw new System.Exception("Unsupported type");
			}
		}
	}


	public class DTUConverter : Editor
	{
		public static DTU ParseDTUFile(string path)
		{

			var dtu = new DTU();
			dtu.DTUPath = path;

			if(!System.IO.File.Exists(path))
			{
				UnityEngine.Debug.LogError("DTU File: " + path + " does not exist");
				return dtu;
			}

			var text = System.IO.File.ReadAllText(path);

			if(text.Length<=0)
			{
				UnityEngine.Debug.LogError("DTU File: " + path + " is empty");
				return dtu;
			}
			//text = CleanJSON(text);
			//var dtu = JsonUtility.FromJson<DTU>(text);


			var root = SimpleJSON.JSON.Parse(text);

			dtu.AssetID = root["Asset Id"].Value;
			dtu.AssetName = root["Asset Name"].Value;
			dtu.AssetType = root["Asset Type"].Value;
			dtu.FBXFile = root["FBX File"].Value;
			dtu.ImportFolder = root["Import Folder"].Value;
			dtu.Materials = new List<DTUMaterial>();

			var materials = root["Materials"].AsArray;

			foreach(var matKVP in materials)
			{
				var mat = matKVP.Value;
				var dtuMat = new DTUMaterial();

				dtuMat.Version = mat["Version"].AsFloat;
				dtuMat.AssetName = mat["Asset Name"].Value;
				dtuMat.MaterialName = mat["Material Name"].Value;
				dtuMat.MaterialType = mat["Material Type"].Value;
				dtuMat.Value = mat["Value"].Value;
				dtuMat.Properties = new List<DTUMaterialProperty>();

				var properties = mat["Properties"];
				foreach(var propKVP in properties)
				{
					var prop = propKVP.Value;
					var dtuMatProp = new DTUMaterialProperty();

					//since this property was found, mark it
					dtuMatProp.Exists = true;

					dtuMatProp.Name = prop["Name"].Value;
					dtuMatProp.Texture = prop["Texture"].Value;
					var v = new DTUValue();

					var propDataType = prop["Data Type"].Value;
					if(propDataType == "Double")
					{
						v.Type = DTUValue.DataType.Double;
						v.AsDouble = prop["Value"].AsDouble;

					} else if(propDataType == "Integer")
					{
						v.Type = DTUValue.DataType.Integer;
						v.AsInteger = prop["Value"].AsInt;
					} else if(propDataType == "Float")
					{
						v.Type = DTUValue.DataType.Float;
						v.AsDouble = prop["Value"].AsFloat;
					} else if(propDataType == "String")
					{
						v.Type = DTUValue.DataType.String;
						v.AsString = prop["Value"].Value;
					} else if(propDataType == "Color")
					{
						v.Type = DTUValue.DataType.Color;
						var tmpStr = prop["Value"].Value;
						Color color;
						if(!ColorUtility.TryParseHtmlString(tmpStr,out color))
						{
							UnityEngine.Debug.LogError("Failed to parse color hex code: " + tmpStr);
							throw new System.Exception("Invalid color hex code");
						}
						v.AsColor = color;
					} else if(propDataType == "Texture")
					{
						v.Type = DTUValue.DataType.Texture;

						//these values will be hex colors 
						var tmpStr = prop["Value"].Value;
						Color color;
						if(!ColorUtility.TryParseHtmlString(tmpStr,out color))
						{
							UnityEngine.Debug.LogError("Failed to parse color hex code: " + tmpStr);
							throw new System.Exception("Invalid color hex code");
						}
						v.AsColor = color;
					}
					
					else
					{
						UnityEngine.Debug.LogError("Type: " + propDataType + " is not supported");
						throw new System.Exception("Unsupported type");
					}

					dtuMatProp.Value = v;

					dtuMat.Properties.Add(dtuMatProp);
				}

				dtu.Materials.Add(dtuMat);
			}

			return dtu;
		}


		/// <summary>
		/// Strips spaces from the json text in preparation for the JsonUtility (which doesn't handle spaces in keys)
		/// This won't appropriately handle the special Value/Data Type in the Properties array, but if you don't need that this cleaner may help you
		/// </summary>
		/// <param name="jsonRaw"></param>
		/// <returns></returns>
		protected static string CleanJSON(string jsonText)
		{
			//Converts something like "Asset Name" :  => "AssetName"
			// basically its... find something starting with whitespace, then a " then any space anywhere up to the next quote, but only the first occurance on the line
			// then only replace it with the first capture and third capture group, skipping the 2nd capture group (the space)
			var result = Regex.Replace(jsonText,"^(\\s+\"[^\"]+)([\\s]+)([^\"]+\"\\s*)","$1$3",RegexOptions.Multiline);
			return result;
			
		}

		/// <summary>
		/// Parses the DTU file and converts all materials and textures if dirty, will place next to DTU file
		/// </summary>
		[MenuItem("Assets/Daz3D/Extract materials")]
		[MenuItem("Daz3D/Extract materials from selected DTU")]
		public static void MenuItemConvert()
		{
			var activeObject = Selection.activeObject;
			var path = AssetDatabase.GetAssetPath(activeObject);

			var dtu = ParseDTUFile(path);
			
			UnityEngine.Debug.Log("DTU: " + dtu.AssetName + " contains: " + dtu.Materials.Count + " materials");

			foreach(var dtuMat in dtu.Materials)
			{
				dtu.ConvertToUnity(dtuMat);
			}

		}

		 

	}

}
