#pragma once

#include "CoreMinimal.h"
#include "DazToUnrealEnums.h"


class FDazToUnrealPoses
{
public:
	static void CreatePoseAsset(UAnimSequence* SourceAnimation, TArray<FString> PoseNames);
};