#include "DazToUnrealMorphs.h"
#include "DazJointControlledMorphAnimInstance.h"
#include "Serialization/JsonReader.h"
#include "Dom/JsonObject.h"
#include "Serialization/JsonSerializer.h"
#include "AssetRegistryModule.h"
#include "Kismet2/KismetEditorUtilities.h"
#include "Animation/AnimBlueprint.h"
#include "Animation/AnimBlueprintGeneratedClass.h"
#include "EdGraphSchema_K2_Actions.h"
#include "AnimGraphNode_LinkedInputPose.h"
#include "Kismet2/BlueprintEditorUtils.h"

UDazJointControlledMorphAnimInstance* FDazToUnrealMorphs::CreateJointControlAnimation(TSharedPtr<FJsonObject> JsonObject, FString Folder, FString CharacterName, USkeleton* Skeleton)
{
	// Only only create the JCM Anim if the JointLinks object exists
	const TArray<TSharedPtr<FJsonValue>>* JointLinkList;// = JsonObject->GetArrayField(TEXT("JointLinks"));
	if (JsonObject->TryGetArrayField(TEXT("JointLinks"), JointLinkList))
	{
		const FString AssetName = CharacterName + TEXT("_JCMAnim");
		const FString PackageName = FPackageName::ObjectPathToPackageName(Folder) / AssetName;
		const FString ObjectName = FPackageName::ObjectPathToObjectName(PackageName);

		// Create the new blueprint next to the SkeletalMesh and get the AnimInstance from it.
		UPackage* NewPackage = CreatePackage(*PackageName);
		UAnimBlueprint* AnimBlueprint = CreateBlueprint(NewPackage, FName(AssetName), Skeleton);
		UDazJointControlledMorphAnimInstance* AnimInstance = Cast<UDazJointControlledMorphAnimInstance>(AnimBlueprint->GetAnimBlueprintGeneratedClass()->ClassDefaultObject);

		// Get the joint link info for each link
		for (int i = 0; i < JointLinkList->Num(); i++)
		{
			FDazJointControlLink NewJointLink;
			const TSharedPtr<FJsonValue> JointLinkValue = (*JointLinkList)[i];
			TSharedPtr<FJsonObject> JointLink = JointLinkValue->AsObject();
			NewJointLink.BoneName = FName(JointLink->GetStringField(TEXT("Bone")));

			// Unreal ends up with different axis
			FString DazAxis = JointLink->GetStringField(TEXT("Axis"));
			if (DazAxis == TEXT("XRotate")) 
			{
				NewJointLink.PrimaryAxis = EDazMorphAnimInstanceDriver::RotationY;
			}
			if (DazAxis == TEXT("YRotate"))
			{
				NewJointLink.PrimaryAxis = EDazMorphAnimInstanceDriver::RotationZ;
			}
			if (DazAxis == TEXT("ZRotate"))
			{
				NewJointLink.PrimaryAxis = EDazMorphAnimInstanceDriver::RotationX;
			}

			NewJointLink.MorphName = FName(JointLink->GetStringField(TEXT("Morph")));
			NewJointLink.Scalar = JointLink->GetNumberField(TEXT("Scalar"));

			// These two are flipped when we get in Unreal
			if (NewJointLink.PrimaryAxis == EDazMorphAnimInstanceDriver::RotationY)
			{
				NewJointLink.Scalar = NewJointLink.Scalar * -1.0f;
			}
			if (NewJointLink.PrimaryAxis == EDazMorphAnimInstanceDriver::RotationZ)
			{
				NewJointLink.Scalar = NewJointLink.Scalar * -1.0f;
			}
			NewJointLink.Alpha = JointLink->GetNumberField(TEXT("Alpha"));

			// Get the Keys if they exist
			const TArray<TSharedPtr<FJsonValue>>* JointLinkKeys;
			if (JointLink->TryGetArrayField(TEXT("Keys"), JointLinkKeys))
			{
				for (int KeyIndex = 0; KeyIndex < JointLinkKeys->Num(); KeyIndex++)
				{
					FDazJointControlLinkKey NewJointLinkKey;
					const TSharedPtr<FJsonValue> JointLinkKeyValue = (*JointLinkKeys)[KeyIndex];
					TSharedPtr<FJsonObject> JointLinkKey = JointLinkKeyValue->AsObject();
					NewJointLinkKey.BoneRotation = JointLinkKey->GetNumberField(TEXT("Angle"));
					if (NewJointLink.PrimaryAxis == EDazMorphAnimInstanceDriver::RotationY)
					{
						NewJointLinkKey.BoneRotation = NewJointLinkKey.BoneRotation * -1.0f;
					}
					if (NewJointLink.PrimaryAxis == EDazMorphAnimInstanceDriver::RotationZ)
					{
						NewJointLinkKey.BoneRotation = NewJointLinkKey.BoneRotation * -1.0f;
					}
					NewJointLinkKey.MorphAlpha = JointLinkKey->GetNumberField(TEXT("Value"));
					NewJointLink.Keys.Add(NewJointLinkKey);
				}
			}

			AnimInstance->ControlLinks.Add(NewJointLink);
		}

		// Setup Secondary Axis.  We need to know the primary and secondary axis 
		// because if we check the rotation in the wrong order the morphs can pop.
		for (FDazJointControlLink& PrimaryAxisLink : AnimInstance->ControlLinks)
		{
			for (FDazJointControlLink& SecondaryAxisLink : AnimInstance->ControlLinks)
			{
				if (PrimaryAxisLink.BoneName == SecondaryAxisLink.BoneName &&
					PrimaryAxisLink.PrimaryAxis != SecondaryAxisLink.PrimaryAxis)
				{
					PrimaryAxisLink.SecondaryAxis = SecondaryAxisLink.PrimaryAxis;
					SecondaryAxisLink.SecondaryAxis = PrimaryAxisLink.PrimaryAxis;
					break;
				}
			}
		}

		// Recompile the AnimBlueprint and return the updated AnimInstance
		FBlueprintEditorUtils::MarkBlueprintAsModified(AnimBlueprint);
		FKismetEditorUtilities::CompileBlueprint(AnimBlueprint);
		AnimInstance = Cast<UDazJointControlledMorphAnimInstance>(AnimBlueprint->GetAnimBlueprintGeneratedClass()->ClassDefaultObject);
		return AnimInstance;
	}
	return nullptr;
}

// This is a stripped down version of UAnimBlueprintFactory::FactoryCreateNew
UAnimBlueprint* FDazToUnrealMorphs::CreateBlueprint(UObject* InParent, FName Name, USkeleton* Skeleton)
{
	// Create the Blueprint
	UClass* ClassToUse = UDazJointControlledMorphAnimInstance::StaticClass();
	UAnimBlueprint* NewBP = CastChecked<UAnimBlueprint>(FKismetEditorUtilities::CreateBlueprint(ClassToUse, InParent, Name, BPTYPE_Normal, UAnimBlueprint::StaticClass(), UBlueprintGeneratedClass::StaticClass(), NAME_None));

	NewBP->TargetSkeleton = Skeleton;

	// Because the BP itself didn't have the skeleton set when the initial compile occured, it's not set on the generated classes either
	if (UAnimBlueprintGeneratedClass* TypedNewClass = Cast<UAnimBlueprintGeneratedClass>(NewBP->GeneratedClass))
	{
		TypedNewClass->TargetSkeleton = Skeleton;
	}
	if (UAnimBlueprintGeneratedClass* TypedNewClass_SKEL = Cast<UAnimBlueprintGeneratedClass>(NewBP->SkeletonGeneratedClass))
	{
		TypedNewClass_SKEL->TargetSkeleton = Skeleton;
	}

	// Need to add the Pose Input Node and connect it to the Output Node in the AnimGraph
	TArray<UEdGraph*> Graphs;
	NewBP->GetAllGraphs(Graphs);
	for (UEdGraph* Graph : Graphs)
	{
		if (Graph->GetFName() == UEdGraphSchema_K2::GN_AnimGraph)
		{
			UEdGraphNode* OutNode = nullptr;
			for (UEdGraphNode* Node : Graph->Nodes)
			{
				OutNode = Node;
			}
			// If the Blueprint isn't open in an Editor Window adding the UAnimGraphNode_LinkedInputPose node will crash.
			GEditor->GetEditorSubsystem<UAssetEditorSubsystem>()->OpenEditorForAsset(NewBP);
			UAnimGraphNode_LinkedInputPose* const InputTemplate = NewObject<UAnimGraphNode_LinkedInputPose>();
			UEdGraphNode* const NewNode = FEdGraphSchemaAction_NewNode::SpawnNodeFromTemplate<UAnimGraphNode_LinkedInputPose>(Graph, InputTemplate, FVector2D(0.0f, 0.0f), false);
			if (NewNode && OutNode)
			{
				UEdGraphPin* NextPin = NewNode->FindPin(TEXT("Pose"));
				UEdGraphPin* OutPin = OutNode->FindPin(TEXT("Result"));

				NextPin->MakeLinkTo(OutPin);
			}

			// Don't need the editor open anymore, so close it now.
			// Note: Turns out I can't close it because part of the action happens on a tick
			//GEditor->GetEditorSubsystem<UAssetEditorSubsystem>()->CloseAllEditorsForAsset(NewBP);
		}
	}

	return NewBP;

	
}