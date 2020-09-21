using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;
using UnityEditor.Rendering.HighDefinition;

using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Daz3D
{

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


			//We currently support the following material types:
			// Iray Uber
			// PBR SP
			// Daz Studio Default
			//See: DTUMaterialType


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
				/**
				 * Properties
				 *
				 * Glossy Layered Weight - Similar to glossy level, seems fair to multiply these two together, value 1.0 => smooth, value = 0.0 => rough

				 */
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
				
				//for now we're going to force this to Daz Studio Default which has the same mappings
				materialType = DTUMaterialType.DazStudioDefault;
			}
			else
			{
				//If we don't know what it is, we'll just try, but it's quite possible it won't work
				UnityEngine.Debug.LogWarning("Unknown material type: " + dtuMaterial.MaterialType + " for mat: " + dtuMaterial.MaterialName + " using default");
				materialType = DTUMaterialType.DazStudioDefault;
				//return null;
			}


			/**
			 * Important Properties
			 *
			 * Refer to: http://docs.daz3d.com/doku.php/public/software/dazstudio/4/referenceguide/interface/panes/surfaces/shaders/iray_uber_shader/shader_general_concepts/start
			 *
			 * Two Sided Light - Cull Off
			 * Thin Walled - 1 = very thin, 0 = thick
			 * Cutout Opacity - 
			 * UV Set - which uv channel should we be using (defaults to 0)
			 * Diffuse Color - the base color we should use, often times uses a texture
			 * Base Mixing 
			 *   0 - PBR Metal/Rough
			 *   1 - PBR Spec/Gloss
			 *   2 - Weighted (loosely mdl 3ds max based)
			 *
			 *
			 * In the shaders...
			 *  _CullMode => 0->None, 1->Front, 2->Back
			 *  _CullModeForward => 0->None, 1->Front, 2->Back
			 *  _AlphaCutoffEnable => 0->Off, 1->Enable
			 *  _AlphaDstBlend => 0->Off, 10->Enable
			 *  _ZWrite => 0->Off (alpha), 1->On (opaque)
			 *  _ZTestDepthEqualForOpaque -> 4 (both)
			 *  _ZTestGBuffer -> 4 (opaque), 3 (transparent)
			 *  _ZTestTransparent -> 4 (both)
			 *  _SrcBlend -> 1 (both)
			 *  _DstBlend -> 0 (opaque), 10 (transparent)
			 *  _DoubleSidedEnable -> 1 (cull off), 0 (cull back)
			 *  _DoubleSidedNormalMode -> 2 (both)
			 *  _RenderQueueType -> 1 (opaque), 5 (transparent)
			 *  _SurfaceType -> 0 (opaque), 1 (transparent)
			 *
			 * Keywords with alpha: _ALPHATEST_ON _BLENDMODE_ALPHA _DOUBLESIDED_ON _ENABLE_FOG_ON_TRANSPARENT _SURFACE_TYPE_TRANSPARENT
			 * Keywords with opaque: (nothing, no keywords needed)
			 */
			
			//HDRP/Lit shader properties are defined here
			//https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@10.0/manual/Lit-Shader.html

			//notes:
			// if we use depth offset to aid in zfighting for things like clothing, the shadow maps cause banding on things like directional lights


			
			//let's try to guess if we're a skin shader or not
			DTUMaterialProperty dualLobeSpecularWeight = new DTUMaterialProperty();
			DTUMaterialProperty dualLobeSpecularReflectivity = new DTUMaterialProperty();
			DTUMaterialProperty glossiness = new DTUMaterialProperty();
			DTUMaterialProperty glossyRoughness = new DTUMaterialProperty();
			DTUMaterialProperty metallicWeight = new DTUMaterialProperty();
			DTUMaterialProperty cutoutOpacity = new DTUMaterialProperty();


			string shaderNameIrayOpaque = "Daz3D/IrayUber";
			string shaderNameIrayTransparent = "Daz3D/IrayUberTransparent";
			string shaderNameIraySkin = "Daz3D/IrayUberSkin";
			string shaderNameHair = "Daz3D/Hair";
			string shaderNameInvisible = "Daz3D/Invisible";
			
			string shaderName = shaderNameIrayOpaque; //default choice

			DTUBaseMixing baseMixing = DTUBaseMixing.Unknown;


			
			if(materialType == DTUMaterialType.IrayUber)
			{
				dualLobeSpecularWeight = dtuMaterial.Map["Dual Lobe Specular Weight"];
				dualLobeSpecularReflectivity = dtuMaterial.Map["Dual Lobe Specular Reflectivity"];

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

			//hair requires some special changes b/c it's not rendered the same as in Daz Studio
			// this is one of the outlier material types that doesn't use the same property types
			//TODO: this lookup feels a bit silly, we should make a function where we pass in this mat and it does something smarter
			if(
				valueLower.Contains("hair") || assetNameLower.EndsWith("hair") || matNameLower.Contains("hair") 
				|| valueLower.Contains("moustache") || assetNameLower.EndsWith("moustache") || matNameLower.Contains("moustache") 
				|| valueLower.Contains("beard") || assetNameLower.EndsWith("beard") || matNameLower.Contains("beard") 
			)
			{
				isHair = true;
			}

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

			if(processProperties)
			{


				
				//Base color
				var diffuseColor = dtuMaterial.Map["Diffuse Color"];
				mat.SetColor("_Diffuse",diffuseColor.Value.AsColor);
				if(!string.IsNullOrEmpty(diffuseColor.Texture))
				{
					var tex = ImportTextureFromPath(diffuseColor.Texture,materialDir);
					mat.SetTexture("_DiffuseMap",tex);
				}

				//Metalness
				if(dtuMaterial.Map.ContainsKey("Metallic Weight")){
					metallicWeight = dtuMaterial.Map["Metallic Weight"];
					mat.SetFloat("_Metallic",(float)metallicWeight.Value.AsDouble);
					if(!string.IsNullOrEmpty(metallicWeight.Texture))
					{
						//R=Metal, G=AO, B=Detail Mask, A=Smoothness (for regular metal textures in unity, but we're using a Daz style shader)
						var tex = ImportTextureFromPath(metallicWeight.Texture,materialDir,false,true);
						mat.SetTexture("_MetallicMap",tex);
					}
				}


				//Roughness
				if(dtuMaterial.Map.ContainsKey("Glossy Roughness")){
					glossyRoughness = dtuMaterial.Map["Glossy Roughness"]; //1==rough, 0==smooth
				}
				if(dtuMaterial.Map.ContainsKey("Glossiness"))
				{
					glossiness = dtuMaterial.Map["Glossiness"];
				}

				//glossy rough is used if base mix is pbr/metal
				//glossiness is used otherwise

				//if we're only using glossiness value vs a glossyrough + texture
				//if(glossyRoughness.Value.AsDouble > 0f || materialType == DTUMaterialType.PBRSP)
				if( (materialType == DTUMaterialType.IrayUber && baseMixing == DTUBaseMixing.PBRMetalRoughness) || materialType == DTUMaterialType.PBRSP)
				{
					//This means we're using a texture map
					if(!string.IsNullOrEmpty(glossyRoughness.Texture))
					{
						var tex = ImportTextureFromPath(glossyRoughness.Texture,materialDir,false,true);
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
					//this value is glossiness/smoothness, so invert it for roughness
					var perceptualRoughness = 1f - (float)glossiness.Value.AsDouble;
					//convert "perceptual roughness" to real "roughness", this makes things smoother roughly
					//var roughness = perceptualRoughness * perceptualRoughness;
					var roughness = perceptualRoughness;
					mat.SetFloat("_GlossyRoughness",roughness);
				}

				//some mats like the cornea can be a little too rough, so we'll restrict their max values if set
				if(shouldClampRoughness && mat.HasProperty("_GlossyRoughness"))
				{
					var currentRoughness = mat.GetFloat("_GlossyRoughness");
					currentRoughness = Mathf.Clamp(0.0f,0.0825f,currentRoughness);
					mat.SetFloat("_GlossyRoughness",currentRoughness);
				}


				//Normal
				var normal = dtuMaterial.Map["Normal Map"];

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
					var tex = ImportTextureFromPath(normal.Texture,materialDir,true);
					mat.SetTexture("_NormalMap",tex);
				}

				//Height
				var height = dtuMaterial.Map["Bump Strength"];
				mat.SetFloat("_HeightStrength",(float)height.Value.AsDouble);
				if(!string.IsNullOrEmpty(height.Texture))
				{
					var tex = ImportTextureFromPath(height.Texture,materialDir);
					mat.SetTexture("_HeightMap",tex);
				}

				//alpha
				if(dtuMaterial.Map.ContainsKey("Cutout Opacity")){
					cutoutOpacity = dtuMaterial.Map["Cutout Opacity"];
					mat.SetFloat("_CutoutOpacity",(float)cutoutOpacity.Value.AsDouble);
					if(!string.IsNullOrEmpty(cutoutOpacity.Texture))
					{
						var tex = ImportTextureFromPath(cutoutOpacity.Texture,materialDir);
						mat.SetTexture("_CutoutOpacityMap",tex);

						isTransparent = true;
						isDoubleSided = true;
					}
				}
				
				if(dtuMaterial.Map.ContainsKey("Opacity Strength"))
				{
					//TODO: DTU didn't contain this field
					var opacityStrength = dtuMaterial.Map["Opacity Strength"];
					mat.SetFloat("_OpacityStrength",(float)opacityStrength.Value.AsDouble);
					if(!string.IsNullOrEmpty(opacityStrength.Texture))
					{
						var tex =  ImportTextureFromPath(opacityStrength.Texture,materialDir);
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
					var refractionIndex = dtuMaterial.Map["Refraction Index"];
					var refractionWeight = dtuMaterial.Map["Refraction Weight"];
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
						var tex = ImportTextureFromPath(dualLobeSpecularWeight.Texture,materialDir);
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
							var tex = ImportTextureFromPath(dualLobeSpecularReflectivity.Texture,materialDir);
							mat.SetTexture("_DualLobeSpecularReflectivityMap",tex);

							hasDualLobeSpecularReflectivity = true;
						}
					}


					var glossyLayeredWeight = dtuMaterial.Map["Glossy Layered Weight"];
					var glossyColor = dtuMaterial.Map["Glossy Color"];
					var glossySpecular = dtuMaterial.Map["Glossy Specular"];

					hasGlossyLayeredWeight = glossyLayeredWeight.Value.AsDouble > 0;
					if(!string.IsNullOrEmpty(glossyColor.Texture))
					{
						var tex = ImportTextureFromPath(glossyColor.Texture,materialDir);
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

					var translucencyColor = dtuMaterial.Map["Translucency Color"];
					var transmittedColor = dtuMaterial.Map["Transmitted Color"];


					mat.SetColor("_TranslucencyColor",translucencyColor.Value.AsColor);
					if(!string.IsNullOrEmpty(translucencyColor.Texture))
					{
						var tex = ImportTextureFromPath(translucencyColor.Texture,materialDir);
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
						var colorProp = dtuMaterial.Map["Specular Color"];
						specularColor = colorProp.Value.AsColor;
					}
					else if(materialType == DTUMaterialType.IrayUber)
					{
						var colorProp = dtuMaterial.Map["Glossy Color"];
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




				//Toggle various states and parameters if we're transparent, opaque, and/or double sided
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


			AssetDatabase.CreateAsset(mat,materialPath);

			//Works around a bug in HDRP, see: https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@7.1/manual/Creating-and-Editing-HDRP-Shader-Graphs.html "Known Issues"
			UnityEditor.Rendering.HighDefinition.HDShaderUtils.ResetMaterialKeywords(mat);

			return mat;
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
		public Texture2D ImportTextureFromPath(string path, string localAssetDir, bool isNormal = false, bool isLinear = false)
		{
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

			if(copyRemtoe)
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

	}

	public struct DTUMaterialProperty
	{
		public string Name;
		public DTUValue Value;
		public string Texture;

		/// <summary>
		/// True if this property was found in the DTU
		/// </summary>
		public bool Exists;
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
			var text = System.IO.File.ReadAllText(path);
			//text = CleanJSON(text);
			//var dtu = JsonUtility.FromJson<DTU>(text);

			var dtu = new DTU();
			dtu.DTUPath = path;

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
		[MenuItem("Daz3D/Extract Mats From Selected DTU")]
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
