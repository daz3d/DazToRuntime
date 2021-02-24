#include "DazToUnrealEnvironment.h"
#include "DazToUnrealSettings.h"
#include "DazToUnrealUtils.h"
#include "Dom/JsonObject.h"
#include "EditorLevelLibrary.h"
#include "Math/UnrealMathUtility.h"

void FDazToUnrealEnvironment::ImportEnvironment(TSharedPtr<FJsonObject> JsonObject)
{
	// Get the list of instances
	const UDazToUnrealSettings* CachedSettings = GetDefault<UDazToUnrealSettings>();
	TArray<TSharedPtr<FJsonValue>> InstanceList = JsonObject->GetArrayField(TEXT("Instances"));
	TMap<FString, AActor*> GuidToActor;
	TMap<FString, FString> ParentToChild;
	for (int32 Index = 0; Index< InstanceList.Num(); Index++)
	{
		TSharedPtr<FJsonObject> Instance = InstanceList[Index]->AsObject();

		// Get the instance details
		int32 Version = Instance->GetIntegerField(TEXT("Version"));
		FString InstanceAssetName = FDazToUnrealUtils::SanitizeName(Instance->GetStringField(TEXT("InstanceAsset")));
		double InstanceXPos = Instance->GetNumberField(TEXT("TranslationX"));
		double InstanceYPos = Instance->GetNumberField(TEXT("TranslationZ"));
		double InstanceZPos = Instance->GetNumberField(TEXT("TranslationY"));

		double InstanceXRot = FMath::RadiansToDegrees(Instance->GetNumberField(TEXT("RotationX")));
		double InstanceYRot = FMath::RadiansToDegrees(Instance->GetNumberField(TEXT("RotationY")));
		double InstanceZRot = FMath::RadiansToDegrees(Instance->GetNumberField(TEXT("RotationZ")));

		double ScaleXPos = Instance->GetNumberField(TEXT("ScaleX"));
		double ScaleYPos = Instance->GetNumberField(TEXT("ScaleZ"));
		double ScaleZPos = Instance->GetNumberField(TEXT("ScaleY"));

		FString ParentId = Instance->GetStringField(TEXT("ParentID"));
		FString InstanceId = Instance->GetStringField(TEXT("Guid"));

		// Find the asset for this instance
		FString AssetPath = CachedSettings->ImportDirectory.Path / InstanceAssetName / InstanceAssetName + TEXT(".") + InstanceAssetName;
		UObject* InstanceObject = LoadObject<UObject>(NULL, *AssetPath, NULL, LOAD_None, NULL);

		// Spawn the object into the scene
		if (InstanceObject)
		{
			FVector Location = FVector(InstanceXPos, InstanceYPos, InstanceZPos);
			FRotator Rotation = FRotator(InstanceXRot, InstanceYRot, InstanceZRot);
			AActor* NewActor = UEditorLevelLibrary::SpawnActorFromObject(InstanceObject, Location, Rotation);
			if (NewActor)
			{
				NewActor->SetActorScale3D(FVector(ScaleXPos, ScaleYPos, ScaleZPos));
				GuidToActor.Add(InstanceId, NewActor);
				ParentToChild.Add(ParentId, InstanceId);
			}
		}
	}

	// Re-create the hierarchy from Daz Studio
	for (auto Pair : ParentToChild)
	{
		if (GuidToActor.Contains(Pair.Key) && GuidToActor.Contains(Pair.Value))
		{
			AActor* ParentActor = GuidToActor[Pair.Key];
			AActor* ChildActor = GuidToActor[Pair.Value];
			ChildActor->AttachToActor(ParentActor, FAttachmentTransformRules::KeepWorldTransform);
		}
	}

}