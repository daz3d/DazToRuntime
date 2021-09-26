#pragma once

#include "CoreMinimal.h"
#include "DazToUnrealEnums.h"
#include "Dom/JsonObject.h"

class UDazJointControlledMorphAnimInstance;
class USkeleton;

class FDazToUnrealMorphs
{
public:
	// Called to create the JCM AnimInstance
	static UDazJointControlledMorphAnimInstance* CreateJointControlAnimation(TSharedPtr<FJsonObject> JsonObject, FString Folder, FString CharacterName, USkeleton* Skeleton);

private:

	// Internal function for creating the AnimBlueprint for the AnimInstance
	static UAnimBlueprint* CreateBlueprint(UObject* InParent, FName Name, USkeleton* Skeleton);
};