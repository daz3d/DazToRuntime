#include "DazToUnrealUtils.h"
#include "GenericPlatform/GenericPlatformFile.h"
#include "Misc/Paths.h"

FString FDazToUnrealUtils::SanitizeName(FString OriginalName)
{
	return OriginalName.Replace(TEXT(" "), TEXT("_"))
		.Replace(TEXT("("), TEXT("_"))
		.Replace(TEXT(")"), TEXT("_"))
		.Replace(TEXT("."), TEXT("_"))
		.Replace(TEXT("&"), TEXT("_"))
		.Replace(TEXT("!"), TEXT("_"))
		.Replace(TEXT("*"), TEXT("_"))
		.Replace(TEXT("<"), TEXT("_"))
		.Replace(TEXT(">"), TEXT("_"))
		.Replace(TEXT("?"), TEXT("_"))
		.Replace(TEXT("\\"), TEXT("_"));
}


FString FDazToUnrealUtils::MaterialName(FString OriginalName, FString AssetName, bool UseOriginalMat)
{
	if (!UseOriginalMat)
	{
		OriginalName = TEXT("M_") + AssetName + TEXT("_") + OriginalName;
	}
	else
	{
		OriginalName = TEXT("M_") + OriginalName;
	}

	return FDazToUnrealUtils::SanitizeName(OriginalName);
}

FString FDazToUnrealUtils::TextureName(FString OgMaterialName, FString PropertyName, FString AssetName)
{
	FString TextureName;
	TextureName = TEXT("T_") + AssetName +  TEXT("_") + OgMaterialName + TEXT("_") + PropertyName;
	return FDazToUnrealUtils::SanitizeName(TextureName);
}
FString FDazToUnrealUtils::MeshName(FString OriginalName)
{
	return OriginalName;
}

bool FDazToUnrealUtils::MakeDirectoryAndCheck(FString& Directory)
{
	IPlatformFile& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();
	if (!FPaths::DirectoryExists(Directory))
	{
		PlatformFile.CreateDirectory(*Directory);
		if (!FPaths::DirectoryExists(Directory))
		{
			return false;
		}
	}
	return true;
}