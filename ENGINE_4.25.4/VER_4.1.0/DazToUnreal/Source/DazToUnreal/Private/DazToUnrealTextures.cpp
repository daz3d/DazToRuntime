#include "DazToUnrealTextures.h"
#include "DazToUnrealSettings.h"

// Gets a texture specified in the settings for sub sarface aplha for a specified character and body part.
FString FDazToUnrealTextures::GetSubSurfaceAlphaTexture(const DazCharacterType CharacterType, const FString& MaterialName)
{
	const UDazToUnrealSettings* CachedSettings = GetDefault<UDazToUnrealSettings>();
	if (CharacterType == DazCharacterType::Genesis8Male)
	{
		if (MaterialName.EndsWith(TEXT("_Arms")) ||
			MaterialName.EndsWith(TEXT("_Fingernails")))
		{
			return CachedSettings->ArmsSubSurfaceOpacityGenesis8MaleTexture.ToString();
		}
		if (MaterialName.EndsWith(TEXT("_Face")) ||
			MaterialName.EndsWith(TEXT("_Lips")) ||
			MaterialName.EndsWith(TEXT("_Ears")))
		{
			return CachedSettings->FaceSubSurfaceOpacityGenesis8MaleTexture.ToString();
		}
		if (MaterialName.EndsWith(TEXT("_Torso")) ||
			MaterialName.EndsWith(TEXT("_Genitalia")))
		{
			return CachedSettings->TorsoSubSurfaceOpacityGenesis8MaleTexture.ToString();
		}
		if (MaterialName.EndsWith(TEXT("_Legs")) ||
			MaterialName.EndsWith(TEXT("_ToeNails")))
		{
			return CachedSettings->LegsSubSurfaceOpacityGenesis8MaleTexture.ToString();
		}
	}

	if (CharacterType == DazCharacterType::Genesis8Female)
	{
		if (MaterialName.EndsWith(TEXT("_Arms")) ||
			MaterialName.EndsWith(TEXT("_Fingernails")))
		{
			return CachedSettings->ArmsSubSurfaceOpacityGenesis8FemaleTexture.ToString();
		}
		if (MaterialName.EndsWith(TEXT("_Face")) ||
			MaterialName.EndsWith(TEXT("_Lips")) ||
			MaterialName.EndsWith(TEXT("_Ears")))
		{
			return CachedSettings->FaceSubSurfaceOpacityGenesis8FemaleTexture.ToString();
		}
		if (MaterialName.EndsWith(TEXT("_Torso")) ||
			MaterialName.EndsWith(TEXT("_Genitalia")))
		{
			return CachedSettings->TorsoSubSurfaceOpacityGenesis8FemaleTexture.ToString();
		}
		if (MaterialName.EndsWith(TEXT("_Legs")) ||
			MaterialName.EndsWith(TEXT("_ToeNails")))
		{
			return CachedSettings->LegsSubSurfaceOpacityGenesis8FemaleTexture.ToString();
		}
	}

	if (CharacterType == DazCharacterType::Genesis3Male)
	{
		if (MaterialName.EndsWith(TEXT("_Arms")) ||
			MaterialName.EndsWith(TEXT("_Fingernails")))
		{
			return CachedSettings->ArmsSubSurfaceOpacityGenesis3MaleTexture.ToString();
		}
		if (MaterialName.EndsWith(TEXT("_Face")) ||
			MaterialName.EndsWith(TEXT("_Lips")) ||
			MaterialName.EndsWith(TEXT("_Ears")))
		{
			return CachedSettings->FaceSubSurfaceOpacityGenesis3MaleTexture.ToString();
		}
		if (MaterialName.EndsWith(TEXT("_Torso")) ||
			MaterialName.EndsWith(TEXT("_Genitalia")))
		{
			return CachedSettings->TorsoSubSurfaceOpacityGenesis3MaleTexture.ToString();
		}
		if (MaterialName.EndsWith(TEXT("_Legs")) ||
			MaterialName.EndsWith(TEXT("_ToeNails")))
		{
			return CachedSettings->LegsSubSurfaceOpacityGenesis3MaleTexture.ToString();
		}
	}

	if (CharacterType == DazCharacterType::Genesis3Female)
	{
		if (MaterialName.EndsWith(TEXT("_Arms")) ||
			MaterialName.EndsWith(TEXT("_Fingernails")))
		{
			return CachedSettings->ArmsSubSurfaceOpacityGenesis3FemaleTexture.ToString();
		}
		if (MaterialName.EndsWith(TEXT("_Face")) ||
			MaterialName.EndsWith(TEXT("_Lips")) ||
			MaterialName.EndsWith(TEXT("_Ears")))
		{
			return CachedSettings->FaceSubSurfaceOpacityGenesis3FemaleTexture.ToString();
		}
		if (MaterialName.EndsWith(TEXT("_Torso")) ||
			MaterialName.EndsWith(TEXT("_Genitalia")))
		{
			return CachedSettings->TorsoSubSurfaceOpacityGenesis3FemaleTexture.ToString();
		}
		if (MaterialName.EndsWith(TEXT("_Legs")) ||
			MaterialName.EndsWith(TEXT("_ToeNails")))
		{
			return CachedSettings->LegsSubSurfaceOpacityGenesis3FemaleTexture.ToString();
		}
	}
	return TEXT("");
}