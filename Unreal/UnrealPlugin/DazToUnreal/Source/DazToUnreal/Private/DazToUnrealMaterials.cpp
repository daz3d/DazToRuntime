#include "DazToUnrealMaterials.h"
#include "DazToUnrealSettings.h"
#include "DazToUnrealTextures.h"
#include "DazToUnrealUtils.h"

#include "Materials/MaterialInstanceConstant.h"
#include "Factories/MaterialInstanceConstantFactoryNew.h"
#include "Factories/SubsurfaceProfileFactory.h"
#include "Engine/SubsurfaceProfile.h"
#include "AssetRegistryModule.h"
#include "AssetToolsModule.h"
#include "Dom/JsonObject.h"
FSoftObjectPath FDazToUnrealMaterials::GetBaseMaterialForShader(FString ShaderName)
{
	const UDazToUnrealSettings* CachedSettings = GetDefault<UDazToUnrealSettings>();
	FSoftObjectPath BaseMaterialAssetPath = CachedSettings->BaseMaterial;
	if (CachedSettings->BaseShaderMaterials.Contains(ShaderName))
	{
		BaseMaterialAssetPath = CachedSettings->BaseShaderMaterials[ShaderName];
	}
	return BaseMaterialAssetPath;
}

FSoftObjectPath FDazToUnrealMaterials::GetSkinMaterialForShader(FString ShaderName)
{
	const UDazToUnrealSettings* CachedSettings = GetDefault<UDazToUnrealSettings>();
	FSoftObjectPath BaseMaterialAssetPath = CachedSettings->BaseSkinMaterial;
	if (CachedSettings->SkinShaderMaterials.Contains(ShaderName))
	{
		BaseMaterialAssetPath = CachedSettings->SkinShaderMaterials[ShaderName];
	}
	return BaseMaterialAssetPath;
}

FSoftObjectPath FDazToUnrealMaterials::GetBaseMaterial(FString MaterialName, TArray<FDUFTextureProperty > MaterialProperties)
{
	const UDazToUnrealSettings* CachedSettings = GetDefault<UDazToUnrealSettings>();

	// Find the proper Base Material
	FSoftObjectPath BaseMaterialAssetPath = CachedSettings->BaseMaterial;

	FString AssetType = "";
	FString ShaderName = "";
	FString Seperator;
	if ( CachedSettings->UseOriginalMaterialName)
	{
		Seperator = "";
	}
	else
	{
		Seperator = "_";
	}
	TArray<FDUFTextureProperty> Properties = MaterialProperties;
	for (FDUFTextureProperty Property : Properties)
	{
		if (Property.Name == TEXT("Asset Type"))
		{
			AssetType = Property.Value;
			ShaderName = Property.ShaderName;
		}
	}

	// Set the default material type
	if (CachedSettings->BaseShaderMaterials.Contains(ShaderName))
	{
		BaseMaterialAssetPath = CachedSettings->BaseShaderMaterials[ShaderName];
		//return BaseMaterialAssetPath;
	}
	
	if (AssetType == TEXT("Follower/Hair"))
	{
		BaseMaterialAssetPath = CachedSettings->BaseHairMaterial;
		if (MaterialName.EndsWith(Seperator + TEXT("scalp")))
		{
			BaseMaterialAssetPath = CachedSettings->BaseScalpMaterial;
		}
	}
	else if (AssetType == TEXT("Follower/Attachment/Head/Face/Eyelashes"))
	{
		if (MaterialName.Contains(Seperator + TEXT("EyeMoisture")))
		{
			BaseMaterialAssetPath = CachedSettings->BaseEyeMoistureMaterial;
		}
		else
		{
			BaseMaterialAssetPath = CachedSettings->BaseAlphaMaterial;
		}
	}
	else if (AssetType == TEXT("Follower/Attachment/Lower-Body/Hip/Front") &&
		MaterialName.Contains(Seperator + TEXT("Genitalia")))
	{
		BaseMaterialAssetPath = GetSkinMaterialForShader(ShaderName);
	}
	else if (AssetType == TEXT("Actor/Character"))
	{
		// Check for skin materials
		if (MaterialName.EndsWith(Seperator + TEXT("Face")) ||
			MaterialName.EndsWith(Seperator + TEXT("Head")) ||
			MaterialName.EndsWith(Seperator + TEXT("Lips")) ||
			MaterialName.EndsWith(Seperator + TEXT("Legs")) ||
			MaterialName.EndsWith(Seperator + TEXT("Hips")) ||
			MaterialName.EndsWith(Seperator + TEXT("Feet")) ||
			MaterialName.EndsWith(Seperator + TEXT("Torso")) ||
			MaterialName.EndsWith(Seperator + TEXT("Body")) ||
			MaterialName.EndsWith(Seperator + TEXT("Neck")) ||
			MaterialName.EndsWith(Seperator + TEXT("Shoulders")) ||
			MaterialName.EndsWith(Seperator + TEXT("Arms")) ||
			MaterialName.EndsWith(Seperator + TEXT("Forearms")) ||
			MaterialName.EndsWith(Seperator + TEXT("Hands")) ||
			MaterialName.EndsWith(Seperator + TEXT("EyeSocket")) ||
			MaterialName.EndsWith(Seperator + TEXT("Ears")) ||
			MaterialName.EndsWith(Seperator + TEXT("Fingernails")) ||
			MaterialName.EndsWith(Seperator + TEXT("Toenails")) ||
			MaterialName.EndsWith(Seperator + TEXT("Nipples")) ||
			MaterialName.EndsWith(Seperator + TEXT("Genitalia")))
		{
			BaseMaterialAssetPath = GetSkinMaterialForShader(ShaderName);
		}
		else if (MaterialName.Contains(Seperator + TEXT("EyeMoisture")))
		{
			BaseMaterialAssetPath = CachedSettings->BaseEyeMoistureMaterial;
		}
		else if (MaterialName.Contains(Seperator + TEXT("EyeReflection")))
		{
			BaseMaterialAssetPath = CachedSettings->BaseEyeMoistureMaterial;
		}
		else if (MaterialName.Contains(Seperator + TEXT("Tear")))
		{
			BaseMaterialAssetPath = CachedSettings->BaseEyeMoistureMaterial;
		}
		else if (MaterialName.EndsWith(Seperator + TEXT("EyeLashes")))
		{
			BaseMaterialAssetPath = CachedSettings->BaseAlphaMaterial;
		}
		else if (MaterialName.EndsWith(Seperator + TEXT("Eyelashes")))
		{
			BaseMaterialAssetPath = CachedSettings->BaseAlphaMaterial;
		}
		else if (MaterialName.EndsWith(Seperator + TEXT("Eyelash")))
		{
			BaseMaterialAssetPath = CachedSettings->BaseAlphaMaterial;
		}
		else if (MaterialName.EndsWith(Seperator + TEXT("cornea")))
		{
			BaseMaterialAssetPath = CachedSettings->BaseCorneaMaterial;
		}
		/*else if (MaterialName.EndsWith(TEXT("_sclera")))
		{
			BaseMaterialAssetPath = CachedSettings->BaseMaterial;
		}
		else if (MaterialName.EndsWith(TEXT("_irises")))
		{
			BaseMaterialAssetPath = CachedSettings->BaseMaterial;
		}
		else if (MaterialName.EndsWith(TEXT("_pupils")))
		{
			BaseMaterialAssetPath = CachedSettings->BaseMaterial;
		}*/
		else
		{
			//BaseMaterialAssetPath = CachedSettings->BaseMaterial;

			for (FDUFTextureProperty Property : Properties)
			{
				if (Property.Name == TEXT("Cutout Opacity Texture"))
				{
					BaseMaterialAssetPath = CachedSettings->BaseAlphaMaterial;
				}
			}
			
		}
	}
	else if (MaterialName.Contains(Seperator + TEXT("EyeMoisture")))
	{
		BaseMaterialAssetPath = CachedSettings->BaseEyeMoistureMaterial;
	}
	else
	{
		//BaseMaterialAssetPath = CachedSettings->BaseMaterial;

		for (FDUFTextureProperty Property : Properties)
		{
			if (Property.Name == TEXT("Cutout Opacity Texture"))
			{
				BaseMaterialAssetPath = CachedSettings->BaseAlphaMaterial;
			}
			if (Property.Name == TEXT("Opacity Strength") && Property.Value != TEXT("1"))
			{
				BaseMaterialAssetPath = CachedSettings->BaseAlphaMaterial;
			}
		}
		
	}
	if (MaterialName.EndsWith(Seperator + TEXT("NoDraw")))
	{
		BaseMaterialAssetPath = CachedSettings->NoDrawMaterial;
	}
	return BaseMaterialAssetPath;
}

UMaterialInstanceConstant* FDazToUnrealMaterials::CreateMaterial(const FString CharacterMaterialFolder, const FString CharacterTexturesFolder, FString& MaterialName, TMap<FString, TArray<FDUFTextureProperty>> MaterialProperties, const DazCharacterType CharacterType, UMaterialInstanceConstant* ParentMaterial, USubsurfaceProfile* SubsurfaceProfile)
{
	const UDazToUnrealSettings* CachedSettings = GetDefault<UDazToUnrealSettings>();

	FSoftObjectPath BaseMaterialAssetPath = CachedSettings->BaseMaterial;
	// Prepare the material Properties
	if (MaterialProperties.Contains(MaterialName))
	{
		BaseMaterialAssetPath = GetBaseMaterial(MaterialName, MaterialProperties[MaterialName]);
	}
	
	FString ShaderName = "";
	FString AssetType = "";
	if (MaterialProperties.Contains(MaterialName))
	{
		TArray<FDUFTextureProperty> Properties = MaterialProperties[MaterialName];
		for (FDUFTextureProperty Property : Properties)
		{
			if (Property.Name == TEXT("Asset Type"))
			{
				AssetType = Property.Value;
				ShaderName = Property.ShaderName;
			}
		}
	}
	FString Seperator;
	if ( CachedSettings->UseOriginalMaterialName)
	{
		Seperator = "";
	}
	else
	{
		Seperator = "_";
	}
	if (AssetType == TEXT("Follower/Attachment/Head/Face/Eyelashes") ||
		AssetType == TEXT("Follower/Attachment/Head/Face/Eyes") ||
		AssetType == TEXT("Follower/Attachment/Head/Face/Eyes/Tear") ||
		AssetType == TEXT("Follower/Attachment/Head/Face/Tears"))
	{
		if (MaterialName.Contains(Seperator + TEXT("EyeMoisture")) || MaterialName.EndsWith(Seperator + TEXT("EyeReflection")))
		{
			//BaseMaterialAssetPath = CachedSettings->BaseEyeMoistureMaterial;
			SetMaterialProperty(MaterialName, TEXT("Metallic Weight"), TEXT("Double"), TEXT("1"), MaterialProperties);
			SetMaterialProperty(MaterialName, TEXT("Opacity Strength"), TEXT("Double"), FString::SanitizeFloat(CachedSettings->DefaultEyeMoistureOpacity), MaterialProperties);
			SetMaterialProperty(MaterialName, TEXT("Diffuse Color"), TEXT("Color"), TEXT("#bababa"), MaterialProperties);
			SetMaterialProperty(MaterialName, TEXT("Index of Refraction"), TEXT("Double"), TEXT("1.0"), MaterialProperties);
		}
		else if (MaterialName.EndsWith(Seperator + TEXT("Tear")) || MaterialName.EndsWith(Seperator + TEXT("Tears")))
		{
			//BaseMaterialAssetPath = CachedSettings->BaseCorneaMaterial;
			SetMaterialProperty(MaterialName, TEXT("Metallic Weight"), TEXT("Double"), TEXT("1"), MaterialProperties);
			SetMaterialProperty(MaterialName, TEXT("Opacity Strength"), TEXT("Double"), FString::SanitizeFloat(CachedSettings->DefaultEyeMoistureOpacity), MaterialProperties);
			SetMaterialProperty(MaterialName, TEXT("Index of Refraction"), TEXT("Double"), TEXT("1.0"), MaterialProperties);
		}
		else
		{
			//BaseMaterialAssetPath = CachedSettings->BaseAlphaMaterial;
			SetMaterialProperty(MaterialName, TEXT("Metallic Weight"), TEXT("Double"), TEXT("0"), MaterialProperties);
			SetMaterialProperty(MaterialName, TEXT("Glossy Layered Weight"), TEXT("Double"), TEXT("0"), MaterialProperties);
		}
	}
	else if (AssetType == TEXT("Follower/Attachment/Lower-Body/Hip/Front") &&
		MaterialName.Contains(Seperator + TEXT("Genitalia")))
	{
		//BaseMaterialAssetPath = CachedSettings->BaseSkinMaterial;
		SetMaterialProperty(MaterialName, TEXT("Subsurface Alpha Texture"), TEXT("Texture"), FDazToUnrealTextures::GetSubSurfaceAlphaTexture(CharacterType, MaterialName), MaterialProperties);
	}
	else if (AssetType == TEXT("Actor/Character"))
	{
		// Check for skin materials
		if (MaterialName.EndsWith(Seperator + TEXT("Face")) ||
			MaterialName.EndsWith(Seperator + TEXT("Head")) ||
			MaterialName.EndsWith(Seperator + TEXT("Lips")) ||
			MaterialName.EndsWith(Seperator + TEXT("Legs")) ||
			MaterialName.EndsWith(Seperator + TEXT("Torso")) ||
			MaterialName.EndsWith(Seperator + TEXT("Body")) ||
			MaterialName.EndsWith(Seperator + TEXT("Arms")) ||
			MaterialName.EndsWith(Seperator + TEXT("EyeSocket")) ||
			MaterialName.EndsWith(Seperator + TEXT("Ears")) ||
			MaterialName.EndsWith(Seperator + TEXT("Fingernails")) ||
			MaterialName.EndsWith(Seperator + TEXT("Toenails")) ||
			MaterialName.EndsWith(Seperator + TEXT("Genitalia")))
		{
			//BaseMaterialAssetPath = CachedSettings->BaseSkinMaterial;
			SetMaterialProperty(MaterialName, TEXT("Diffuse Subsurface Color Weight"), TEXT("Double"), FString::SanitizeFloat(CachedSettings->DefaultSkinDiffuseSubsurfaceColorWeight), MaterialProperties);
			SetMaterialProperty(MaterialName, TEXT("Subsurface Alpha Texture"), TEXT("Texture"), FDazToUnrealTextures::GetSubSurfaceAlphaTexture(CharacterType, MaterialName), MaterialProperties);
		}
		else if (MaterialName.Contains(Seperator + TEXT("EyeMoisture")))
		{
			//BaseMaterialAssetPath = CachedSettings->BaseEyeMoistureMaterial;
			SetMaterialProperty(MaterialName, TEXT("Metallic Weight"), TEXT("Double"), TEXT("1"), MaterialProperties);
			SetMaterialProperty(MaterialName, TEXT("Opacity Strength"), TEXT("Double"), FString::SanitizeFloat(CachedSettings->DefaultEyeMoistureOpacity), MaterialProperties);
			SetMaterialProperty(MaterialName, TEXT("Index of Refraction"), TEXT("Double"), TEXT("1.0"), MaterialProperties);
		}
		else if (MaterialName.EndsWith(Seperator + TEXT("EyeReflection")) || MaterialName.EndsWith(Seperator + TEXT("Tear")) || MaterialName.EndsWith(Seperator + TEXT("Tears")))
		{
			//BaseMaterialAssetPath = CachedSettings->BaseEyeMoistureMaterial;
			SetMaterialProperty(MaterialName, TEXT("Metallic Weight"), TEXT("Double"), TEXT("1"), MaterialProperties);
			SetMaterialProperty(MaterialName, TEXT("Opacity Strength"), TEXT("Double"), FString::SanitizeFloat(CachedSettings->DefaultEyeMoistureOpacity), MaterialProperties);
			SetMaterialProperty(MaterialName, TEXT("Index of Refraction"), TEXT("Double"), TEXT("1.0"), MaterialProperties);
		}
		else if (MaterialName.EndsWith(Seperator + TEXT("EyeLashes")) || MaterialName.EndsWith(Seperator + TEXT("Eyelashes")))
		{
			//BaseMaterialAssetPath = CachedSettings->BaseAlphaMaterial;
			SetMaterialProperty(MaterialName, TEXT("Metallic Weight"), TEXT("Double"), TEXT("0"), MaterialProperties);
			SetMaterialProperty(MaterialName, TEXT("Glossy Layered Weight"), TEXT("Double"), TEXT("0"), MaterialProperties);
			SetMaterialProperty(MaterialName, TEXT("Index of Refraction"), TEXT("Double"), TEXT("1.0"), MaterialProperties);
		}
		else if (MaterialName.EndsWith(Seperator + TEXT("cornea")))
		{
			//BaseMaterialAssetPath = CachedSettings->BaseCorneaMaterial;
			SetMaterialProperty(MaterialName, TEXT("Metallic Weight"), TEXT("Double"), TEXT("1"), MaterialProperties);
			SetMaterialProperty(MaterialName, TEXT("Opacity Strength"), TEXT("Double"), FString::SanitizeFloat(CachedSettings->DefaultEyeMoistureOpacity), MaterialProperties);
			SetMaterialProperty(MaterialName, TEXT("Index of Refraction"), TEXT("Double"), TEXT("1.0"), MaterialProperties);
		}
		else if (MaterialName.EndsWith(Seperator + TEXT("sclera")))
		{
			//BaseMaterialAssetPath = CachedSettings->BaseMaterial;
		}
		else if (MaterialName.EndsWith(Seperator + TEXT("irises")))
		{
			SetMaterialProperty(MaterialName, TEXT("Pixel Depth Offset"), TEXT("Double"), TEXT("0.1"), MaterialProperties);
			//BaseMaterialAssetPath = CachedSettings->BaseMaterial;
		}
		else if (MaterialName.EndsWith(Seperator + TEXT("pupils")))
		{
			SetMaterialProperty(MaterialName, TEXT("Pixel Depth Offset"), TEXT("Double"), TEXT("0.1"), MaterialProperties);
			//BaseMaterialAssetPath = CachedSettings->BaseMaterial;
		}
		else
		{
			//BaseMaterialAssetPath = CachedSettings->BaseMaterial;
			if (MaterialProperties.Contains(MaterialName))
			{
				TArray<FDUFTextureProperty> Properties = MaterialProperties[MaterialName];
				for (FDUFTextureProperty Property : Properties)
				{
					if (Property.Name == TEXT("Cutout Opacity Texture"))
					{
						//BaseMaterialAssetPath = CachedSettings->BaseAlphaMaterial;
					}
				}
			}
		}
	}
	else if (MaterialName.Contains(Seperator + TEXT("EyeMoisture")))
	{
		//BaseMaterialAssetPath = CachedSettings->BaseEyeMoistureMaterial;
		SetMaterialProperty(MaterialName, TEXT("Metallic Weight"), TEXT("Double"), TEXT("1"), MaterialProperties);
		SetMaterialProperty(MaterialName, TEXT("Opacity Strength"), TEXT("Double"), FString::SanitizeFloat(CachedSettings->DefaultEyeMoistureOpacity), MaterialProperties);
		SetMaterialProperty(MaterialName, TEXT("Index of Refraction"), TEXT("Double"), TEXT("1.0"), MaterialProperties);
	}
	else
	{
		//BaseMaterialAssetPath = CachedSettings->BaseMaterial;
		if (MaterialProperties.Contains(MaterialName))
		{
			TArray<FDUFTextureProperty> Properties = MaterialProperties[MaterialName];
			for (FDUFTextureProperty Property : Properties)
			{
				if (Property.Name == TEXT("Cutout Opacity Texture"))
				{
					//BaseMaterialAssetPath = CachedSettings->BaseAlphaMaterial;
				}
				if (Property.Name == TEXT("Opacity Strength") && Property.Value != TEXT("1"))
				{
					//BaseMaterialAssetPath = CachedSettings->BaseAlphaMaterial;
				}
			}
		}
	}
	if (MaterialName.EndsWith(Seperator + TEXT("NoDraw")))
	{
		//BaseMaterialAssetPath = CachedSettings->NoDrawMaterial;
	}

	// Create the Material Instance
	auto MaterialInstanceFactory = NewObject<UMaterialInstanceConstantFactoryNew>();

	UPackage* Package = CreatePackage(nullptr, *(CharacterMaterialFolder / MaterialName));
	UMaterialInstanceConstant* UnrealMaterialConstant = (UMaterialInstanceConstant*)MaterialInstanceFactory->FactoryCreateNew(UMaterialInstanceConstant::StaticClass(), Package, *MaterialName, RF_Standalone | RF_Public, NULL, GWarn);


	if (UnrealMaterialConstant != NULL)
	{
		// Notify the asset registry
		FAssetRegistryModule::AssetCreated(UnrealMaterialConstant);

		// Set the dirty flag so this package will get saved later
		Package->SetDirtyFlag(true);

		UObject* BaseMaterial = BaseMaterialAssetPath.TryLoad();
		if (ParentMaterial && ParentMaterial->Parent == BaseMaterial)
		{
			UnrealMaterialConstant->SetParentEditorOnly((UMaterial*)ParentMaterial);
		}
		else
		{

			UnrealMaterialConstant->SetParentEditorOnly((UMaterial*)BaseMaterial);
		}

		if (SubsurfaceProfile)
		{
			if (!ParentMaterial || ParentMaterial->SubsurfaceProfile != SubsurfaceProfile)
			{
				UnrealMaterialConstant->bOverrideSubsurfaceProfile = 1;
				UnrealMaterialConstant->SubsurfaceProfile = SubsurfaceProfile;
			}
			else
			{
				UnrealMaterialConstant->bOverrideSubsurfaceProfile = 0;
			}
		}

		// Set the MaterialInstance properties
		if (MaterialProperties.Contains(MaterialName))
		{

			// Rename properties based on the settings
			TArray<FDUFTextureProperty> MaterialInstanceProperties;
			for (FDUFTextureProperty MaterialProperty : MaterialProperties[MaterialName])
			{
				if (CachedSettings->MaterialPropertyMapping.Contains(MaterialProperty.Name))
				{
					MaterialProperty.Name = CachedSettings->MaterialPropertyMapping[MaterialProperty.Name];
				}
				MaterialInstanceProperties.Add(MaterialProperty);
			}

			// Apply the properties
			for (FDUFTextureProperty MaterialProperty : MaterialInstanceProperties)
			{
				if (MaterialProperty.Type == TEXT("Texture"))
				{
					FStringAssetReference TextureAssetPath(CharacterTexturesFolder / MaterialProperty.Value);
					UObject* TextureAsset = TextureAssetPath.TryLoad();
					if (TextureAsset == nullptr)
					{
						FStringAssetReference TextureAssetPathFull(MaterialProperty.Value);
						TextureAsset = TextureAssetPathFull.TryLoad();
					}
					FMaterialParameterInfo ParameterInfo(*MaterialProperty.Name);
					UnrealMaterialConstant->SetTextureParameterValueEditorOnly(ParameterInfo, (UTexture*)TextureAsset);
				}
				if (MaterialProperty.Type == TEXT("Double"))
				{
					float Value = FCString::Atof(*MaterialProperty.Value);
					FMaterialParameterInfo ParameterInfo(*MaterialProperty.Name);
					UnrealMaterialConstant->SetScalarParameterValueEditorOnly(ParameterInfo, Value);
				}
				if (MaterialProperty.Type == TEXT("Color"))
				{
					//FLinearColor Value = FDazToUnrealModule::FromHex(MaterialProperty.Value);
					//FColor Color = Value.ToFColor(true);
					FColor Color = FColor::FromHex(MaterialProperty.Value);
					FMaterialParameterInfo ParameterInfo(*MaterialProperty.Name);
					UnrealMaterialConstant->SetVectorParameterValueEditorOnly(ParameterInfo, Color);
				}
				if (MaterialProperty.Type == TEXT("Switch"))
				{
					FStaticParameterSet StaticParameters;
					UnrealMaterialConstant->GetStaticParameterValues(StaticParameters);

					for (int32 ParameterIdx = 0; ParameterIdx < StaticParameters.StaticSwitchParameters.Num(); ParameterIdx++)
					{
						for (int32 SwitchParamIdx = 0; SwitchParamIdx < StaticParameters.StaticSwitchParameters.Num(); SwitchParamIdx++)
						{
							FStaticSwitchParameter& StaticSwitchParam = StaticParameters.StaticSwitchParameters[SwitchParamIdx];

							if (StaticSwitchParam.ParameterInfo.Name.ToString() == MaterialProperty.Name)
							{
								StaticSwitchParam.bOverride = true;
								StaticSwitchParam.Value = MaterialProperty.Value.ToLower() == TEXT("true");
							}
						}
					}
					UnrealMaterialConstant->UpdateStaticPermutation(StaticParameters);
				}
			}
		}
	}


	return UnrealMaterialConstant;
}

void FDazToUnrealMaterials::SetMaterialProperty(const FString& MaterialName, const FString& PropertyName, const FString& PropertyType, const FString& PropertyValue, TMap<FString, TArray<FDUFTextureProperty>>& MaterialProperties)
{
	if (!MaterialProperties.Contains(MaterialName))
	{
		MaterialProperties.Add(MaterialName, TArray<FDUFTextureProperty>());
	}
	TArray<FDUFTextureProperty>& Properties = MaterialProperties[MaterialName];
	for (FDUFTextureProperty& Property : Properties)
	{
		if (Property.Name == PropertyName)
		{
			Property.Value = PropertyValue;
			return;
		}
	}
	FDUFTextureProperty TextureProperty;
	TextureProperty.Name = PropertyName;
	TextureProperty.Type = PropertyType;
	TextureProperty.Value = PropertyValue;
	MaterialProperties[MaterialName].Add(TextureProperty);

}

FSoftObjectPath FDazToUnrealMaterials::GetMostCommonBaseMaterial(TArray<FString> MaterialNames, TMap<FString, TArray<FDUFTextureProperty>> MaterialProperties)
{
	TArray<FSoftObjectPath> BaseMaterials;
	for (FString MaterialName : MaterialNames)
	{
		BaseMaterials.Add(GetBaseMaterial(MaterialName, MaterialProperties[MaterialName]));
	}

	FSoftObjectPath MostCommonPath;
	int32 MostCommonCount = 0;
	for (FSoftObjectPath BaseMaterial : BaseMaterials)
	{
		int32 Count = 0;
		for (FSoftObjectPath BaseMaterialMatch : BaseMaterials)
		{
			if (BaseMaterialMatch == BaseMaterial)
			{
				Count++;
			}
		}
		if (Count > MostCommonCount)
		{
			MostCommonCount = Count;
			MostCommonPath = BaseMaterial;
		}
	}
	return MostCommonPath;
}

TArray<FDUFTextureProperty> FDazToUnrealMaterials::GetMostCommonProperties(TArray<FString> MaterialNames, TMap<FString, TArray<FDUFTextureProperty>> MaterialProperties)
{
	// Get a list of property names
	TArray<FString> PossibleProperties;
	for (FString MaterialName : MaterialNames)
	{
		for (FDUFTextureProperty Property : MaterialProperties[MaterialName])
		{
			if (Property.Name != TEXT("Asset Type"))
			{
				PossibleProperties.AddUnique(Property.Name);
			}
		}
	}

	TArray<FDUFTextureProperty> MostCommonProperties;
	for (FString PossibleProperty : PossibleProperties)
	{
		FDUFTextureProperty MostCommonProperty = GetMostCommonProperty(PossibleProperty, MaterialNames, MaterialProperties);
		if (MostCommonProperty.Name != TEXT(""))
		{
			MostCommonProperties.Add(MostCommonProperty);
		}
	}

	return MostCommonProperties;
}

FDUFTextureProperty FDazToUnrealMaterials::GetMostCommonProperty(FString PropertyName, TArray<FString> MaterialNames, TMap<FString, TArray<FDUFTextureProperty>> MaterialProperties)
{
	TArray<FDUFTextureProperty> PossibleProperties;

	// Only include properties that exist on all the child materials
	int32 PropertyCount = 0;
	for (FString MaterialName : MaterialNames)
	{
		// Gather all the options
		for (FDUFTextureProperty Property : MaterialProperties[MaterialName])
		{
			if (Property.Name == PropertyName)
			{
				PropertyCount++;
				PossibleProperties.Add(Property);
			}
		}
	}


	FDUFTextureProperty MostCommonProperty;
	int32 MostCommonCount = 0;
	if (PropertyCount == MaterialNames.Num())
	{
		for (FDUFTextureProperty PropertyToCount : PossibleProperties)
		{
			int32 Count = 0;
			for (FDUFTextureProperty PropertyToMatch : PossibleProperties)
			{
				if (PropertyToMatch == PropertyToCount)
				{
					Count++;
				}
			}
			if (Count > MostCommonCount)
			{
				MostCommonCount = Count;
				MostCommonProperty = PropertyToCount;
			}
		}
	}

	if (MostCommonCount <= 1)
	{
		MostCommonProperty.Name = TEXT("");
	}
	return MostCommonProperty;
}

bool FDazToUnrealMaterials::HasMaterialProperty(const FString& PropertyName, const  TArray<FDUFTextureProperty>& MaterialProperties)
{
	for (FDUFTextureProperty MaterialProperty : MaterialProperties)
	{
		if (MaterialProperty.Name == PropertyName)
		{
			return true;
		}
	}
	return false;
}

FString FDazToUnrealMaterials::GetMaterialProperty(const FString& PropertyName, const TArray<FDUFTextureProperty>& MaterialProperties)
{
	for (FDUFTextureProperty MaterialProperty : MaterialProperties)
	{
		if (MaterialProperty.Name == PropertyName)
		{
			return MaterialProperty.Value;
		}
	}
	return FString();
}

USubsurfaceProfile* FDazToUnrealMaterials::CreateSubsurfaceBaseProfileForCharacter(const FString CharacterMaterialFolder, TMap<FString, TArray<FDUFTextureProperty>>& MaterialProperties)
{
	const UDazToUnrealSettings* CachedSettings = GetDefault<UDazToUnrealSettings>();
	
	FString Seperator;
	if ( CachedSettings->UseOriginalMaterialName)
	{
		Seperator = "";
	}
	else
	{
		Seperator = "_";
	}

	// Find the torso material.
	for (TPair<FString, TArray<FDUFTextureProperty>> Pair : MaterialProperties)
	{
		FString AssetType;
		for (FDUFTextureProperty Property : Pair.Value)
		{
			if (Property.Name == TEXT("Asset Type"))
			{
				AssetType = Property.Value;
			}
		}

		if (AssetType == TEXT("Actor/Character"))
		{
			if (Pair.Key.EndsWith(Seperator + TEXT("Torso")) || Pair.Key.EndsWith(Seperator + TEXT("Body")))
			{
				return CreateSubsurfaceProfileForMaterial(Pair.Key, CharacterMaterialFolder, Pair.Value);
			}

		}
	}
	return nullptr;
}

USubsurfaceProfile* FDazToUnrealMaterials::CreateSubsurfaceProfileForMaterial(const FString MaterialName, const FString CharacterMaterialFolder, const TArray<FDUFTextureProperty > MaterialProperties)
{
	// Create the Material Instance
	//auto SubsurfaceProfileFactory = NewObject<USubsurfaceProfileFactory>();

	//Only create for the PBRSkin base material
	FString ShaderName;
	for (FDUFTextureProperty Property : MaterialProperties)
	{
		if (Property.Name == TEXT("Asset Type"))
		{
			ShaderName = Property.ShaderName;
		}
	}
	if (ShaderName != TEXT("PBRSkin"))
	{
		return nullptr;
	}

	FString SubsurfaceProfileName = MaterialName + TEXT("_Profile");
	UPackage* Package = CreatePackage(nullptr, *(CharacterMaterialFolder / MaterialName));
	//USubsurfaceProfile* SubsurfaceProfile = (USubsurfaceProfile*)SubsurfaceProfileFactory->FactoryCreateNew(USubsurfaceProfile::StaticClass(), Package, *MaterialName, RF_Standalone | RF_Public, NULL, GWarn);
	FAssetToolsModule& AssetToolsModule = FModuleManager::GetModuleChecked<FAssetToolsModule>("AssetTools");
	USubsurfaceProfile* SubsurfaceProfile = Cast<USubsurfaceProfile>(AssetToolsModule.Get().CreateAsset(SubsurfaceProfileName, FPackageName::GetLongPackagePath(*(CharacterMaterialFolder / MaterialName)), USubsurfaceProfile::StaticClass(), NULL));
	if (HasMaterialProperty(TEXT("Specular Lobe 1 Roughness"), MaterialProperties))
	{
		SubsurfaceProfile->Settings.Roughness0 = FCString::Atof(*GetMaterialProperty(TEXT("Specular Lobe 1 Roughness"), MaterialProperties));
	}
	if (HasMaterialProperty(TEXT("Specular Lobe 2 Roughness Mult"), MaterialProperties))
	{
		SubsurfaceProfile->Settings.Roughness1 = FCString::Atof(*GetMaterialProperty(TEXT("Specular Lobe 2 Roughness Mult"), MaterialProperties));
	}
	if (HasMaterialProperty(TEXT("Dual Lobe Specular Ratio"), MaterialProperties))
	{
		SubsurfaceProfile->Settings.LobeMix = FCString::Atof(*GetMaterialProperty(TEXT("Dual Lobe Specular Ratio"), MaterialProperties));
	}
	if (HasMaterialProperty(TEXT("SSS Color"), MaterialProperties))
	{
		SubsurfaceProfile->Settings.SubsurfaceColor = FColor::FromHex(*GetMaterialProperty(TEXT("SSS Color"), MaterialProperties));
	}
	if (HasMaterialProperty(TEXT("SSS Color"), MaterialProperties))
	{
		SubsurfaceProfile->Settings.SubsurfaceColor = FColor::FromHex(*GetMaterialProperty(TEXT("SSS Color"), MaterialProperties));
	}
	if (HasMaterialProperty(TEXT("Transmitted Color"), MaterialProperties))
	{
		SubsurfaceProfile->Settings.FalloffColor = FColor::FromHex(*GetMaterialProperty(TEXT("Transmitted Color"), MaterialProperties));
	}
	return SubsurfaceProfile;
}

bool FDazToUnrealMaterials::SubsurfaceProfilesAreIdentical(USubsurfaceProfile* A, USubsurfaceProfile* B)
{
	if (A == nullptr || B == nullptr) return false;
	if (A->Settings.Roughness0 != B->Settings.Roughness0) return false;
	if (A->Settings.Roughness1 != B->Settings.Roughness1) return false;
	if (A->Settings.LobeMix != B->Settings.LobeMix) return false;
	if (A->Settings.SubsurfaceColor != B->Settings.SubsurfaceColor) return false;
	if (A->Settings.FalloffColor != B->Settings.FalloffColor) return false;
	return true;
}

bool FDazToUnrealMaterials::SubsurfaceProfilesWouldBeIdentical(USubsurfaceProfile* ExistingSubsurfaceProfile, const TArray<FDUFTextureProperty > MaterialProperties)
{
	if (ExistingSubsurfaceProfile == nullptr) return false;
	if (ExistingSubsurfaceProfile->Settings.Roughness0 != FCString::Atof(*GetMaterialProperty(TEXT("Specular Lobe 1 Roughness"), MaterialProperties))) return false;
	if (ExistingSubsurfaceProfile->Settings.Roughness1 != FCString::Atof(*GetMaterialProperty(TEXT("Specular Lobe 2 Roughness Mult"), MaterialProperties))) return false;
	if (ExistingSubsurfaceProfile->Settings.LobeMix != FCString::Atof(*GetMaterialProperty(TEXT("Dual Lobe Specular Ratio"), MaterialProperties))) return false;
	if (ExistingSubsurfaceProfile->Settings.SubsurfaceColor != FColor::FromHex(*GetMaterialProperty(TEXT("SSS Color"), MaterialProperties))) return false;
	if (ExistingSubsurfaceProfile->Settings.FalloffColor != FColor::FromHex(*GetMaterialProperty(TEXT("Transmitted Color"), MaterialProperties))) return false;
	return true;
}

void FDazToUnrealMaterials::ImportMaterial(TSharedPtr<FJsonObject> JsonObject)
{
	TMap<FString, TArray<FDUFTextureProperty>> MaterialProperties;

	FString FBXPath = JsonObject->GetStringField(TEXT("FBX File"));
	FString AssetName = FDazToUnrealUtils::SanitizeName(JsonObject->GetStringField(TEXT("Asset Name")));
	FString ImportFolder = JsonObject->GetStringField(TEXT("Import Folder"));
	FString ImportCharacterFolder = FPaths::GetPath(FBXPath);
	FString ImportCharacterTexturesFolder = FPaths::GetPath(FBXPath) / TEXT("Textures");
	FString ImportCharacterMaterialFolder = FPaths::GetPath(FBXPath) / TEXT("Materials");
	FString FBXFile = FBXPath;

	const UDazToUnrealSettings* CachedSettings = GetDefault<UDazToUnrealSettings>();

	FString DAZImportFolder = CachedSettings->ImportDirectory.Path;
	FString DAZAnimationImportFolder = CachedSettings->AnimationImportDirectory.Path;
	FString CharacterFolder = DAZImportFolder / AssetName;
	FString CharacterTexturesFolder = CharacterFolder / TEXT("Textures");
	FString CharacterMaterialFolder = CharacterFolder / TEXT("Materials");

	IPlatformFile& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();

	// Use the maps file to find the textures to load
	TMap<FString, FString> TextureFileSourceToTarget;
	TArray<FString> IntermediateMaterials;
	TArray<FString> MaterialNames;
	TArray<TSharedPtr<FJsonValue>> matList = JsonObject->GetArrayField(TEXT("Materials"));
	for (int32 i = 0; i < matList.Num(); i++)
	{
		TSharedPtr<FJsonObject> material = matList[i]->AsObject();
		int32 Version = material->GetIntegerField(TEXT("Version"));

		// Version 1 "Version, Material, Type, Color, Opacity, File"
		if (Version == 1)
		{
			FString MaterialName = AssetName + TEXT("_") + material->GetStringField(TEXT("Material Name"));
			MaterialName = FDazToUnrealUtils::SanitizeName(MaterialName);
			FString TexturePath = material->GetStringField(TEXT("Texture"));
			FString TextureName = FDazToUnrealUtils::SanitizeName(FPaths::GetBaseFilename(TexturePath));

			if (!MaterialProperties.Contains(MaterialName))
			{
				MaterialProperties.Add(MaterialName, TArray<FDUFTextureProperty>());
				MaterialNames.Add(MaterialName);
			}
			FDUFTextureProperty Property;
			Property.Name = material->GetStringField(TEXT("Name"));
			Property.Type = material->GetStringField(TEXT("Data Type"));
			Property.Value = material->GetStringField(TEXT("Value"));
			if (Property.Type == TEXT("Texture"))
			{
				Property.Type = TEXT("Color");
			}

			MaterialProperties[MaterialName].Add(Property);
			if (!TextureName.IsEmpty())
			{
				// If a texture is attached add a texture property
				FDUFTextureProperty TextureProperty;
				TextureProperty.Name = material->GetStringField(TEXT("Name")) + TEXT(" Texture");
				TextureProperty.Type = TEXT("Texture");

				if (!TextureFileSourceToTarget.Contains(TexturePath))
				{
					int32 TextureCount = 0;
					FString NewTextureName = FString::Printf(TEXT("%s_%02d_%s"), *TextureName, TextureCount, *AssetName);
					while (TextureFileSourceToTarget.FindKey(NewTextureName) != nullptr)
					{
						TextureCount++;
						NewTextureName = FString::Printf(TEXT("%s_%02d_%s"), *TextureName, TextureCount, *AssetName);
					}
					TextureFileSourceToTarget.Add(TexturePath, NewTextureName);
				}

				TextureProperty.Value = TextureFileSourceToTarget[TexturePath];
				MaterialProperties[MaterialName].Add(TextureProperty);
				//TextureFiles.AddUnique(TexturePath);

				// and a switch property for things like Specular that could come from different channels
				FDUFTextureProperty SwitchProperty;
				SwitchProperty.Name = material->GetStringField(TEXT("Name")) + TEXT(" Texture Active");
				SwitchProperty.Type = TEXT("Switch");
				SwitchProperty.Value = TEXT("true");
				MaterialProperties[MaterialName].Add(SwitchProperty);
			}
		}

		// Version 2 "Version, ObjectName, Material, Type, Color, Opacity, File"
		if (Version == 2)
		{
			FString ObjectName = material->GetStringField(TEXT("Asset Name"));
			ObjectName = FDazToUnrealUtils::SanitizeName(ObjectName);
			IntermediateMaterials.AddUnique(ObjectName + TEXT("_BaseMat"));
			FString ShaderName = material->GetStringField(TEXT("Material Type"));
			FString MaterialName = material->GetStringField(TEXT("Material Name"));
			FString OgMaterialName = material->GetStringField(TEXT("Material Name"));
			FString PropertyName = material->GetStringField(TEXT("Name"));
			bool UseOriginalMat = CachedSettings->UseOriginalMaterialName;
			MaterialName = FDazToUnrealUtils::MaterialName(MaterialName, AssetName, UseOriginalMat);

			FString TexturePath = material->GetStringField(TEXT("Texture"));
			FString TextureName = FDazToUnrealUtils::SanitizeName(FPaths::GetBaseFilename(TexturePath));

			if (!MaterialProperties.Contains(MaterialName))
			{
				MaterialProperties.Add(MaterialName, TArray<FDUFTextureProperty>());
				MaterialNames.Add(MaterialName);
			}
			FDUFTextureProperty Property;
			Property.Name = material->GetStringField(TEXT("Name"));
			Property.Type = material->GetStringField(TEXT("Data Type"));
			Property.Value = material->GetStringField(TEXT("Value"));
			Property.ObjectName = ObjectName;
			Property.ShaderName = ShaderName;
			if (Property.Type == TEXT("Texture"))
			{
				Property.Type = TEXT("Color");
			}

			// Properties that end with Enabled are switches for functionality
			if (Property.Name.EndsWith(TEXT(" Enable")))
			{
				Property.Type = TEXT("Switch");
				if (Property.Value == TEXT("0"))
				{
					Property.Value = TEXT("false");
				}
				else
				{
					Property.Value = TEXT("true");
				}
			}

			MaterialProperties[MaterialName].Add(Property);
			if (!TextureName.IsEmpty())
			{
				// If a texture is attached add a texture property
				FDUFTextureProperty TextureProperty;
				TextureProperty.Name = material->GetStringField(TEXT("Name")) + TEXT(" Texture");
				TextureProperty.Type = TEXT("Texture");
				TextureProperty.ObjectName = ObjectName;
				TextureProperty.ShaderName = ShaderName;

				if (!TextureFileSourceToTarget.Contains(TexturePath))
				{
					int32 TextureCount = 0;
					FString NewTextureName = FDazToUnrealUtils::TextureName(OgMaterialName, PropertyName, AssetName);
					TextureFileSourceToTarget.Add(TexturePath, NewTextureName);
				}

				TextureProperty.Value = TextureFileSourceToTarget[TexturePath];
				MaterialProperties[MaterialName].Add(TextureProperty);
				//TextureFiles.AddUnique(TexturePath);

				// and a switch property for things like Specular that could come from different channels
				FDUFTextureProperty SwitchProperty;
				SwitchProperty.Name = material->GetStringField(TEXT("Name")) + TEXT(" Texture Active");
				SwitchProperty.Type = TEXT("Switch");
				SwitchProperty.Value = TEXT("true");
				SwitchProperty.ObjectName = ObjectName;
				SwitchProperty.ShaderName = ShaderName;
				MaterialProperties[MaterialName].Add(SwitchProperty);
			}
		}

		// Version 3 "Version, ObjectName, Material, Type, Asset Value, 
		if (Version == 3)
		{
			FString ObjectName = material->GetStringField(TEXT("Asset Name"));
			ObjectName = FDazToUnrealUtils::SanitizeName(ObjectName);
			IntermediateMaterials.AddUnique(ObjectName + TEXT("_BaseMat"));
			FString ShaderName = material->GetStringField(TEXT("Material Type"));
			FString MaterialName = material->GetStringField(TEXT("Material Name"));
			FString OgMaterialName = material->GetStringField(TEXT("Material Name"));
			bool UseOriginalMat = CachedSettings->UseOriginalMaterialName;
			MaterialName = FDazToUnrealUtils::MaterialName(MaterialName, AssetName, UseOriginalMat);
			TArray<TSharedPtr<FJsonValue>> Properties = material->GetArrayField(TEXT("Properties"));
			for (int32 j = 0; j < Properties.Num(); j++)
			{
				TSharedPtr<FJsonObject> propertyInfo = Properties[j]->AsObject();
				FString PropertyName = propertyInfo->GetStringField(TEXT("Name"));
				FString TexturePath = propertyInfo->GetStringField(TEXT("Texture"));
				FString TextureName = FDazToUnrealUtils::SanitizeName(FPaths::GetBaseFilename(TexturePath));

				if (!MaterialProperties.Contains(MaterialName))
				{
					MaterialProperties.Add(MaterialName, TArray<FDUFTextureProperty>());
				}
				FDUFTextureProperty Property;
				Property.Name = propertyInfo->GetStringField(TEXT("Name"));
				Property.Type = propertyInfo->GetStringField(TEXT("Data Type"));
				Property.Value = propertyInfo->GetStringField(TEXT("Value"));
				Property.ObjectName = ObjectName;
				Property.ShaderName = ShaderName;
				if (Property.Type == TEXT("Texture"))
				{
					Property.Type = TEXT("Color");
				}
				// Properties that end with Enabled are switches for functionality
				if (Property.Name.EndsWith(TEXT(" Enable")))
				{
					Property.Type = TEXT("Switch");
					if (Property.Value == TEXT("0"))
					{
						Property.Value = TEXT("false");
					}
					else
					{
						Property.Value = TEXT("true");
					}
				}

				MaterialProperties[MaterialName].Add(Property);
				if (!TextureName.IsEmpty())
				{
					// If a texture is attached add a texture property
					FDUFTextureProperty TextureProperty;
					TextureProperty.Name = propertyInfo->GetStringField(TEXT("Name")) + TEXT(" Texture");
					TextureProperty.Type = TEXT("Texture");
					TextureProperty.ObjectName = ObjectName;
					TextureProperty.ShaderName = ShaderName;

					if (!TextureFileSourceToTarget.Contains(TexturePath))
					{
						int32 TextureCount = 0;
						FString NewTextureName = FDazToUnrealUtils::TextureName(OgMaterialName, PropertyName, AssetName);
						TextureFileSourceToTarget.Add(TexturePath, NewTextureName);
					}

					TextureProperty.Value = TextureFileSourceToTarget[TexturePath];
					MaterialProperties[MaterialName].Add(TextureProperty);

					// and a switch property for things like Specular that could come from different channels
					FDUFTextureProperty SwitchProperty;
					SwitchProperty.Name = propertyInfo->GetStringField(TEXT("Name")) + TEXT(" Texture Active");
					SwitchProperty.Type = TEXT("Switch");
					SwitchProperty.Value = TEXT("true");
					SwitchProperty.ObjectName = ObjectName;
					SwitchProperty.ShaderName = ShaderName;
					MaterialProperties[MaterialName].Add(SwitchProperty);
				}
			}
		}

	}

	TArray<FString> TexturesFilesToImport;
	for (auto TexturePair : TextureFileSourceToTarget)
	{
		FString SourceFileName = TexturePair.Key;
		FString TargetFileName = ImportCharacterTexturesFolder / TexturePair.Value + FPaths::GetExtension(SourceFileName, true);
		PlatformFile.CopyFile(*TargetFileName, *SourceFileName);
		TexturesFilesToImport.Add(TargetFileName);
	}
	FDazToUnrealMaterials::ImportTextureAssets(TexturesFilesToImport, CharacterTexturesFolder);

	DazCharacterType CharacterType = DazCharacterType::Unknown;

	// Create a default Master Subsurface Profile if needed
	USubsurfaceProfile* MasterSubsurfaceProfile = FDazToUnrealMaterials::CreateSubsurfaceBaseProfileForCharacter(CharacterMaterialFolder, MaterialProperties);

	for (FString IntermediateMaterialName : IntermediateMaterials)
	{
		TArray<FString> ChildMaterials;
		FString ChildMaterialFolder = CharacterMaterialFolder;
		for (FString ChildMaterialName : MaterialNames)
		{
			if (MaterialProperties.Contains(ChildMaterialName))
			{
				for (FDUFTextureProperty ChildProperty : MaterialProperties[ChildMaterialName])
				{
					if ((ChildProperty.ObjectName + TEXT("_BaseMat")) == IntermediateMaterialName)
					{
						ChildMaterialFolder = CharacterMaterialFolder / ChildProperty.ObjectName;
						ChildMaterials.AddUnique(ChildMaterialName);
					}
				}
			}
		}

		if (ChildMaterials.Num() > 1)
		{


			// Copy Material Properties
			TArray<FDUFTextureProperty> MostCommonProperties = FDazToUnrealMaterials::GetMostCommonProperties(ChildMaterials, MaterialProperties);
			MaterialProperties.Add(IntermediateMaterialName, MostCommonProperties);
			//MaterialProperties[IntermediateMaterialName] = MaterialProperties[ChildMaterials[0]];


			// Create Material
			FSoftObjectPath BaseMaterialPath = FDazToUnrealMaterials::GetMostCommonBaseMaterial(ChildMaterials, MaterialProperties);//FDazToUnrealMaterials::GetBaseMaterial(ChildMaterials[0], MaterialProperties[IntermediateMaterialName]);
			UObject* BaseMaterial = BaseMaterialPath.TryLoad();
			UMaterialInstanceConstant* UnrealMaterialConstant = FDazToUnrealMaterials::CreateMaterial(CharacterMaterialFolder, CharacterTexturesFolder, IntermediateMaterialName, MaterialProperties, CharacterType, nullptr, MasterSubsurfaceProfile);
			UnrealMaterialConstant->SetParentEditorOnly((UMaterial*)BaseMaterial);
			for (FString MaterialName : ChildMaterials)
			{
				USubsurfaceProfile* SubsurfaceProfile = MasterSubsurfaceProfile;
				if (!FDazToUnrealMaterials::SubsurfaceProfilesWouldBeIdentical(MasterSubsurfaceProfile, MaterialProperties[MaterialName]))
				{
					SubsurfaceProfile = FDazToUnrealMaterials::CreateSubsurfaceProfileForMaterial(MaterialName, ChildMaterialFolder, MaterialProperties[MaterialName]);
				}

				if (FDazToUnrealMaterials::GetBaseMaterial(MaterialName, MaterialProperties[MaterialName]) == BaseMaterialPath)
				{

					int32 Length = MaterialProperties[MaterialName].Num();
					for (int32 Index = Length - 1; Index >= 0; Index--)
					{
						FDUFTextureProperty ChildPropertyForRemoval = MaterialProperties[MaterialName][Index];
						if (ChildPropertyForRemoval.Name == TEXT("Asset Type")) continue;
						for (FDUFTextureProperty ParentProperty : MaterialProperties[IntermediateMaterialName])
						{
							if (ParentProperty.Name == ChildPropertyForRemoval.Name && ParentProperty.Value == ChildPropertyForRemoval.Value)
							{
								MaterialProperties[MaterialName].RemoveAt(Index);
								break;
							}
						}
					}

					FDazToUnrealMaterials::CreateMaterial(ChildMaterialFolder, CharacterTexturesFolder, MaterialName, MaterialProperties, CharacterType, UnrealMaterialConstant, SubsurfaceProfile);
				}
				else
				{
					FDazToUnrealMaterials::CreateMaterial(ChildMaterialFolder, CharacterTexturesFolder, MaterialName, MaterialProperties, CharacterType, nullptr, SubsurfaceProfile);
				}
			}
		}
		else if (ChildMaterials.Num() == 1)
		{
			USubsurfaceProfile* SubsurfaceProfile = FDazToUnrealMaterials::CreateSubsurfaceProfileForMaterial(ChildMaterials[0], CharacterMaterialFolder / ChildMaterials[0], MaterialProperties[ChildMaterials[0]]);
			FDazToUnrealMaterials::CreateMaterial(CharacterMaterialFolder / IntermediateMaterialName, CharacterTexturesFolder, ChildMaterials[0], MaterialProperties, CharacterType, nullptr, SubsurfaceProfile);
		}

	}

}

bool FDazToUnrealMaterials::ImportTextureAssets(TArray<FString>& SourcePaths, FString& ImportLocation)
{
	FAssetToolsModule& AssetToolsModule = FModuleManager::GetModuleChecked<FAssetToolsModule>("AssetTools");
	TArray<UObject*> ImportedAssets = AssetToolsModule.Get().ImportAssets(SourcePaths, ImportLocation);
	if (ImportedAssets.Num() > 0)
	{
		return true;
	}
	return false;
}