#pragma once

#include "CoreMinimal.h"


class FDazToUnrealUtils
{
public:
	static FString SanitizeName(FString OriginalName);
	static FString MaterialName(FString OriginalName, FString AssetName, bool UseOriginalMat);
	static FString TextureName(FString OgMaterialName, FString PropertyName, FString AssetNam);
	static FString MeshName(FString OriginalName);
	static bool MakeDirectoryAndCheck(FString& Directory);
};