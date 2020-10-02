using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace Daz3D
{
	public static class DTU_Constants
	{
		public const string shaderNameIrayOpaque = "Daz3D/IrayUber";
		public const string shaderNameIrayTransparent = "Daz3D/IrayUberTransparent";
		public const string shaderNameIraySkin = "Daz3D/IrayUberSkin";
		public const string shaderNameHair = "Daz3D/Hair";
		public const string shaderNameInvisible = "Daz3D/Invisible";        //special shader that doesn't render anything
		public const string shaderNameMetal = "Daz3D/IrayUberMetal";
		public const string shaderNameSpecular = "Daz3D/IrayUberSpec";
		public const string shaderNameSpecularTranslucent = "Daz3D/IrayUberTranslucent";
		public const string shaderNameWet = "Daz3D/Wet";

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


			if(matNameLower.Contains("cornea") || matNameLower.Contains("eyemoisture"))
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

			//TODO: this is a bit naive as people will use the dual lobe properties for non skin, but for now it's ok
			if(
				!string.IsNullOrEmpty(dualLobeSpecularWeight.Texture)
				|| !string.IsNullOrEmpty(dualLobeSpecularReflectivity.Texture)
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
			var baseMixingProp = dtuMaterial.Map["Base Mixing"];
			var baseMixingVal = (int) baseMixingProp.Value.AsDouble;
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
			var horizontalTile = dtuMaterial.Get("Horizontal Tile");
			var horizontalOffset = dtuMaterial.Get("Horizontal Offset");
			var verticalTile = dtuMaterial.Get("Vertical Tile");
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
					//shaderName =shaderNameSpecularTranslucent;
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

			//TODO: the V2 is temporary as I rebuild the shader sets
			if(shaderName.ToLower().Contains("iray")){
				shaderName += "V2";
			}
			var shader = Shader.Find(shaderName);
			if(shader == null)
			{
				UnityEngine.Debug.LogError("Failed to locate shader: " + shaderName + " for mat: " + dtuMaterial.MaterialName);
				return null;
			}
			var mat = new Material(shader);


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
						glossyRoughnessValue = 1f - glossiness.Float;
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
			else if(dtuMaterial.MaterialType == "Iray Uber")
			{
				materialType = DTUMaterialType.IrayUber;
			}
			else if(dtuMaterial.MaterialType == "DAZ Studio Default")
			{
				/**
				 * Properties
				 *
				 * Diffuse Color (w/ texture)
				 * Diffuse Strength (typically 1)
				 * Glossiness (typically 1)
				 * Specular Color (typically black)
				 * Specular Strength (typically 1)
				 * Multiply Specular Through Opacity
				 * Ambient Color (typically black)
				 * Ambient Strength (typically 1)
				 * Opacity Strength (w/ texture)
				 * Bump Strength
				 * Negative Bump
				 * Positive Bump
				 * Displacement Strength
				 * Minimum Displacement
				 * Maximum Displacement
				 * Normal Map
				 * Reflection Color
				 * Reflection Strength
				 * Refraction Color
				 * Refraction Strength 
				 * Index of Refraction
				 * Sheen Color
				 * Thickness
				 */
				materialType = DTUMaterialType.DazStudioDefault;
			}
			else if(dtuMaterial.MaterialType == "omUberSurface")
			{
				materialType = DTUMaterialType.OmUberSurface;
				
				//for now we're going to force this to Daz Studio Default which has similar handling
				materialType = DTUMaterialType.DazStudioDefault;
			}
			else
			{
				//If we don't know what it is, we'll just try, but it's quite possible it won't work
				UnityEngine.Debug.LogWarning("Unknown material type: " + dtuMaterial.MaterialType + " for mat: " + dtuMaterial.MaterialName + " using default");
				materialType = DTUMaterialType.DazStudioDefault;
				//return null;
			}

			if(materialType == DTUMaterialType.IrayUber)
			{
				//If we are using the Iray Uber shader, we have a different function to handle this
				var uberMat = ConvertToUnityIrayUber(dtuMaterial, materialDir);
				if(uberMat != null)
				{
					SaveMaterialAsAsset(uberMat,materialPath);
					return uberMat;
				}
			}


			//If we're here, it means the material wasn't an Iray Uber shader, there are many other shaders such as PBR SP and Daz Studio Default
			
			var matNameLower = dtuMaterial.MaterialName.ToLower();
			var assetNameLower = dtuMaterial.AssetName.ToLower();
			var valueLower = dtuMaterial.Value.ToLower();

			bool isTransparent = false;
			bool isDoubleSided = false;
			bool isSkin = false;
			bool isInvisible = false;
			bool isHair = false;

			//Some mats the roughness values are too high, such as cornea and eye moistures
			// when this is set the max roughness is clamped so the clear object is smoother
			// then the properties explictly set via the shader params in Daz
			bool shouldClampRoughness = false;




			
			//let's try to guess if we're a skin shader or not
			DTUMaterialProperty dualLobeSpecularWeight = new DTUMaterialProperty();
			DTUMaterialProperty dualLobeSpecularReflectivity = new DTUMaterialProperty();
			DTUMaterialProperty glossiness = new DTUMaterialProperty();
			DTUMaterialProperty glossyRoughness = new DTUMaterialProperty();
			DTUMaterialProperty metallicWeight = new DTUMaterialProperty();
			DTUMaterialProperty cutoutOpacity = new DTUMaterialProperty();
			DTUMaterialProperty diffuseRoughness = new DTUMaterialProperty();
			DTUMaterialProperty glossyColor = new DTUMaterialProperty();
			DTUMaterialProperty diffuseWeight = new DTUMaterialProperty();
			DTUMaterialProperty glossyWeight = new DTUMaterialProperty();


			string shaderNameIrayOpaque = "Daz3D/IrayUber";
			string shaderNameIrayTransparent = "Daz3D/IrayUberTransparent";
			string shaderNameIraySkin = "Daz3D/IrayUberSkin";
			string shaderNameHair = "Daz3D/Hair";
			string shaderNameInvisible = "Daz3D/Invisible";
			
			string shaderName = shaderNameIrayOpaque; //default choice

			DTUBaseMixing baseMixing = DTUBaseMixing.Unknown;


			if(dtuMaterial.Map.ContainsKey("Glossy Color"))
			{
				glossyColor = dtuMaterial.Map["Glossy Color"];
			}
			if(dtuMaterial.Map.ContainsKey("Glossy Roughness")){
				glossyRoughness = dtuMaterial.Map["Glossy Roughness"]; //1==rough, 0==smooth
			}
			if(dtuMaterial.Map.ContainsKey("Glossiness"))
			{
				glossiness = dtuMaterial.Map["Glossiness"];
			}
			if(dtuMaterial.Map.ContainsKey("Glossy Weight"))
			{
				glossyWeight = dtuMaterial.Map["Glossy Weight"];
			}
			if(dtuMaterial.Map.ContainsKey("Diffuse Weight"))
			{
				diffuseWeight = dtuMaterial.Map["Diffuse Weight"];
			}

			
			if(materialType == DTUMaterialType.IrayUber)
			{
				dualLobeSpecularWeight = dtuMaterial.Map["Dual Lobe Specular Weight"];
				dualLobeSpecularReflectivity = dtuMaterial.Map["Dual Lobe Specular Reflectivity"];
				diffuseRoughness = dtuMaterial.Map["Diffuse Roughness"];

				if(!string.IsNullOrEmpty(dualLobeSpecularWeight.Texture) || !string.IsNullOrEmpty(dualLobeSpecularReflectivity.Texture))
				{
					isSkin = true;
				}

				if(isSkin)
				{
					shaderName = shaderNameIraySkin;
				}


				var baseMixingProp = dtuMaterial.Map["Base Mixing"];
				var baseMixingVal = (int) baseMixingProp.Value.AsDouble;
				baseMixing = (DTUBaseMixing)baseMixingVal;
			}
			else if(materialType == DTUMaterialType.DazStudioDefault)
			{
				shaderName = shaderNameIrayTransparent;
			}

			int sortingPriority = 0;

#region material_hacks
			//material hacks, avoid using these as much as possible
			if(matNameLower.Contains("cornea") || matNameLower.Contains("eyemoisture"))
			{
				isTransparent = true;
				shouldClampRoughness = true; //only really necessary on some eyes but it's a safe enough thing to clamp unless you want hazy/cloudy eyes
				shaderName = shaderNameIrayTransparent;
			}

			if(matNameLower.Contains("eyelash"))
			{
				sortingPriority = 2;
			}
			else if (matNameLower.Contains("cornea"))
			{
				sortingPriority = 1;
			}

			//No reason to render this
			if(matNameLower.Contains("eyereflection")){
				isInvisible = true;
			}

			isHair = IsDTUMaterialHair(dtuMaterial);

#endregion material_hacks

			if(isInvisible)
			{
				shaderName = shaderNameInvisible;
			}

			//var shader = Shader.Find("HDRP/Lit");
			var shader = Shader.Find(shaderName);
			var mat = new Material(shader);
			mat.name = dtuMaterial.MaterialName;


			bool processProperties = !isInvisible;

			var record = new Daz3DDTUImporter.ImportEventRecord();

			if (processProperties)
			{


				
				//Base color
				var diffuseColor = dtuMaterial.Get("Diffuse Color");
				mat.SetColor("_Diffuse",diffuseColor.Value.AsColor);
				if(!string.IsNullOrEmpty(diffuseColor.Texture))
				{
					var tex = ImportTextureFromPath(diffuseColor.Texture,materialDir, record);
					mat.SetTexture("_DiffuseMap",tex);
				}

				//Metalness
				if(dtuMaterial.Map.ContainsKey("Metallic Weight")){
					metallicWeight = dtuMaterial.Get("Metallic Weight");
					mat.SetFloat("_Metallic",(float)metallicWeight.Value.AsDouble);
					if(!string.IsNullOrEmpty(metallicWeight.Texture))
					{
						//R=Metal, G=AO, B=Detail Mask, A=Smoothness (for regular metal textures in unity, but we're using a Daz style shader)
						var tex = ImportTextureFromPath(metallicWeight.Texture,materialDir,record,false,true);
						mat.SetTexture("_MetallicMap",tex);
					}
				}


				//Roughness

				//This is quite complicated to read and set, it's based on the shader and mixinig modes and weights

				//glossy rough is used if base mix is pbr/metal
				//glossiness is used otherwise

				//if we're only using glossiness value vs a glossyrough + texture
				//if(glossyRoughness.Value.AsDouble > 0f || materialType == DTUMaterialType.PBRSP)
				if( (materialType == DTUMaterialType.IrayUber && baseMixing == DTUBaseMixing.PBRMetalRoughness) || materialType == DTUMaterialType.PBRSP)
				{
					//This means we're using a texture map
					if(!string.IsNullOrEmpty(glossyRoughness.Texture))
					{
						var tex = ImportTextureFromPath(glossyRoughness.Texture,materialDir,record,false,true);
						mat.SetTexture("_GlossyRoughnessMap",tex);
						//force our property to map to 1 instead of flipping it for roughness
						mat.SetFloat("_GlossyRoughness",1.0f);
					}
					else
					{

						var roughness = (float)glossyRoughness.Value.AsDouble;
						//convert perceptual rough to real
						//roughness *= roughness;
						mat.SetFloat("_GlossyRoughness",roughness);
					}

				}
				else
				{

					float roughness = 0.5f;

					if(glossyWeight.Exists && materialType == DTUMaterialType.IrayUber)
					{
						float glossValue = (float)glossiness.Value.AsDouble;
						float roughnessValue = (float)glossyRoughness.Value.AsDouble;

						//convert gloss to roughness
						glossValue = 1.0f - glossValue;

						roughness = Mathf.Lerp(roughnessValue, glossValue,(float)glossyWeight.Value.AsDouble);
					}
					//this value is glossiness/smoothness, so invert it for roughness
					//var perceptualRoughness = 1f - (float)glossiness.Value.AsDouble;
					//convert "perceptual roughness" to real "roughness", this makes things smoother roughly
					//var roughness = perceptualRoughness * perceptualRoughness;
					//var roughness = perceptualRoughness;
					mat.SetFloat("_GlossyRoughness",roughness);
				}


				//some materials get roughness from diffuseRoughness and oddly glossyColor
				if(diffuseRoughness.Exists && materialType == DTUMaterialType.IrayUber && diffuseRoughness.Value.AsDouble > 0 &&  mat.GetFloat("_GlossyRoughness") <= Mathf.Epsilon && mat.GetTexture("_GlossyRoughnessMap") == null)
				{

					//mat.SetFloat("_GlossyRoughness", (float)diffuseRoughness.Value.AsDouble);

					if(!string.IsNullOrEmpty(diffuseRoughness.Texture))
					{
						var tex = ImportTextureFromPath(diffuseRoughness.Texture,materialDir,record,false,true);
						mat.SetTexture("_GlossyRoughnessMap",tex);
					}
				}
				else if(glossyColor.Exists && materialType == DTUMaterialType.IrayUber && !string.IsNullOrEmpty(glossyColor.Texture) &&  mat.GetFloat("_GlossyRoughness") <= Mathf.Epsilon && mat.GetTexture("_GlossyRoughnessMap") == null)
				{
					var tex = ImportTextureFromPath(glossyColor.Texture,materialDir, record);
					mat.SetTexture("_GlossyRoughnessMap",tex);
					//mat.SetFloat("_GlossyRoughness",1.0f); //TODO: read color data as black and white and convert to 0->1
				}




				

				//some mats like the cornea can be a little too rough, so we'll restrict their max values if set
				if(shouldClampRoughness && mat.HasProperty("_GlossyRoughness"))
				{
					var currentRoughness = mat.GetFloat("_GlossyRoughness");
					currentRoughness = Mathf.Clamp(0.0f,0.0825f,currentRoughness);
					mat.SetFloat("_GlossyRoughness",currentRoughness);
				}


				//Normal
				var normal = dtuMaterial.Get("Normal Map");

				if(normal.Value.Type == DTUValue.DataType.Double)
				{
					mat.SetFloat("_NormalStrength",(float)normal.Value.AsDouble);
				}
				else
				{
					//TODO: convert color to float if set as well, commonly this will be #ffffff
					mat.SetFloat("_NormalStrength",1.0f);
				}

				if(!string.IsNullOrEmpty(normal.Texture))
				{
					var tex = ImportTextureFromPath(normal.Texture,materialDir,record,true);
					mat.SetTexture("_NormalMap",tex);
				}

				//Height
				var height = dtuMaterial.Get("Bump Strength");
				mat.SetFloat("_HeightStrength",(float)height.Value.AsDouble);
				if(!string.IsNullOrEmpty(height.Texture))
				{
					var tex = ImportTextureFromPath(height.Texture,materialDir,record,false,true);
					mat.SetTexture("_HeightMap",tex);
				}

				//alpha
				if(dtuMaterial.Map.ContainsKey("Cutout Opacity")){
					cutoutOpacity = dtuMaterial.Get("Cutout Opacity");
					mat.SetFloat("_CutoutOpacity",(float)cutoutOpacity.Value.AsDouble);
					if(!string.IsNullOrEmpty(cutoutOpacity.Texture))
					{
						var tex = ImportTextureFromPath(cutoutOpacity.Texture,materialDir,record,false,true);
						mat.SetTexture("_CutoutOpacityMap",tex);

						isTransparent = true;
						isDoubleSided = true;
					}
				}
				
				if(dtuMaterial.Map.ContainsKey("Opacity Strength"))
				{
					//TODO: DTU didn't contain this field
					var opacityStrength = dtuMaterial.Get("Opacity Strength");
					mat.SetFloat("_OpacityStrength",(float)opacityStrength.Value.AsDouble);
					if(!string.IsNullOrEmpty(opacityStrength.Texture))
					{
						var tex =  ImportTextureFromPath(opacityStrength.Texture,materialDir,record,false,true);
						mat.SetTexture("_OpacityStrengthMap",tex);
						
						isTransparent = true;
						isDoubleSided = true;
					}
				}
				else
				{
					if(isTransparent)
					{
						//bit of a hack as we have to flip this around for transparent objects
						mat.SetFloat("_OpacityStrength",0.0f);
					}
					else
					{
						mat.SetFloat("_OpacityStrength",1.0f);
					}
				}

				if(materialType == DTUMaterialType.IrayUber)
				{
					var refractionIndex = dtuMaterial.Get("Refraction Index");
					var refractionWeight = dtuMaterial.Get("Refraction Weight");
					if(refractionWeight.Value.AsDouble > 0)
					{
						mat.SetFloat("_IndexOfRefraction",(float)refractionIndex.Value.AsDouble);
					}
				}


				//Dual Lobe and Specular handling
				bool hasDualLobeSpecularWeight = false;
				bool hasDualLobeSpecularReflectivity = false;
				bool hasGlossyLayeredWeight = false;
				bool hasGlossyColor = false;




				if(materialType == DTUMaterialType.IrayUber)
				{
					if(!string.IsNullOrEmpty(dualLobeSpecularWeight.Texture))
					{
						var tex = ImportTextureFromPath(dualLobeSpecularWeight.Texture,materialDir,record,false,true);
						mat.SetTexture("_DualLobeSpecularWeightMap",tex);

						hasDualLobeSpecularWeight = true;
					}
					if(dualLobeSpecularWeight.Value.AsDouble > 0)
					{
						hasDualLobeSpecularWeight = true;
					}

					if(hasDualLobeSpecularWeight)
					{
						if(!string.IsNullOrEmpty(dualLobeSpecularReflectivity.Texture))
						{
							var tex = ImportTextureFromPath(dualLobeSpecularReflectivity.Texture,materialDir,record,false,true);
							mat.SetTexture("_DualLobeSpecularReflectivityMap",tex);

							hasDualLobeSpecularReflectivity = true;
						}
					}


					var glossyLayeredWeight = dtuMaterial.Get("Glossy Layered Weight");
					var glossySpecular = dtuMaterial.Get("Glossy Specular");

					hasGlossyLayeredWeight = glossyLayeredWeight.Value.AsDouble > 0;
					if(!string.IsNullOrEmpty(glossyColor.Texture))
					{
						var tex = ImportTextureFromPath(glossyColor.Texture,materialDir, record);
						mat.SetTexture("_GlossyColorMap",tex);
					}
					else
					{
						mat.SetColor("_GlossyColor",glossyColor.Value.AsColor);
					}

					mat.SetColor("_GlossySpecular",glossySpecular.Value.AsColor);


				}


				
				
				
				//are we using SSS?
				if(isSkin){
					bool hasTranslucencyColor = false;

					var translucencyColor = dtuMaterial.Get("Translucency Color");
					var transmittedColor = dtuMaterial.Get("Transmitted Color");


					mat.SetColor("_TranslucencyColor",translucencyColor.Value.AsColor);
					if(!string.IsNullOrEmpty(translucencyColor.Texture))
					{
						var tex = ImportTextureFromPath(translucencyColor.Texture,materialDir, record);
						mat.SetTexture("_TranslucencyColorMap",tex);

						hasTranslucencyColor = true;
					}

					mat.SetColor("_TransmittedColor",transmittedColor.Value.AsColor);

					//If your assets appear wrong, make sure you assign a Diffusion Profile, this should be set automatically to the IrayUberSkin profile in Assets/Daz3D/Resources/IrayUberSkinDiffusionProfile
					//Also make sure this is in your list of Diffusion Profile List under Material in your HDRenderPipelineAsset

					//mat.SetFloatArray("_DiffusionProfileHash",);
					//mat.SetVectorArray("_DiffusionProfileAsset",);

				}

				if(!(isSkin || isTransparent))
				{
					//right now anything that isn't skin and transparent is considered double sided
					isDoubleSided = true;
				}

				if(isHair)
				{
					//our hair shader/mats break convention from the typical iray style shader...

					isDoubleSided = true;
					isTransparent = true;

					var hairMat = new Material(Shader.Find(shaderNameHair));
					hairMat.SetTexture("_DiffuseMap",mat.GetTexture("_DiffuseMap"));
					hairMat.SetTexture("_NormalMap",mat.GetTexture("_NormalMap"));

					if(mat.HasProperty("_NormalStrength"))
					{
						hairMat.SetFloat("_NormalStrength",mat.GetFloat("_NormalStrength"));
					}

					Texture opacityTex = null;

					if(mat.HasProperty("_OpacityStrengthMap")){
						opacityTex = mat.GetTexture("_OpacityStrengthMap");
					}
					if(opacityTex == null && mat.HasProperty("_CutoutOpacityMap"))
					{
						opacityTex = mat.GetTexture("_CutoutOpacityMap");
					}

					hairMat.SetTexture("_CutoutOpacityMap",opacityTex);
					hairMat.SetTexture("_GlossyRoughnessMap",mat.GetTexture("_GlossyRoughnessMap"));
					hairMat.SetFloat("_GlossyRoughness",mat.GetFloat("_GlossyRoughness"));

					Color specularColor = Color.black;
					if(materialType == DTUMaterialType.DazStudioDefault)
					{
						var colorProp = dtuMaterial.Get("Specular Color");
						specularColor = colorProp.Value.AsColor;
					}
					else if(materialType == DTUMaterialType.IrayUber)
					{
						var colorProp = dtuMaterial.Get("Glossy Color");
						specularColor = colorProp.Value.AsColor;
					}
					else
					{
						UnityEngine.Debug.LogWarning("Unsupported material type for reading spec color for mat: " + dtuMaterial.MaterialName + " type: " + materialType);
					}

					hairMat.SetColor("_SpecularColor",specularColor);

					//These work for many hairs, but don't work for all of them
					hairMat.SetFloat("_AlphaStrength",1.5f);
					hairMat.SetFloat("_AlphaOffset",0.35f);
					hairMat.SetFloat("_AlphaClip",0.75f);
					hairMat.SetFloat("_AlphaPower",0.4f);

					GameObject.DestroyImmediate(mat);
					mat = null;
					mat = hairMat;
				}


				ToggleCommonMaterialProperties(ref mat,matNameLower,isTransparent,isDoubleSided,hasDualLobeSpecularWeight,hasDualLobeSpecularReflectivity,sortingPriority,hasGlossyLayeredWeight,hasGlossyColor);
			}

			SaveMaterialAsAsset(mat,materialPath);

			if (record.Tokens.Count > 0)
				Daz3DDTUImporter.EventQueue.Enqueue(record);

			return mat;
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
