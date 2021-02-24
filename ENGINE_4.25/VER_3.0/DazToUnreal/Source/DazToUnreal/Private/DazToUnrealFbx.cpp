#include "DazToUnrealFbx.h"



void FDazToUnrealFbx::RenameDuplicateBones(FbxNode* RootNode)
{
	TMap<FString, int> ExistingBones;
	RenameDuplicateBones(RootNode, ExistingBones);
}

void FDazToUnrealFbx::RenameDuplicateBones(FbxNode* RootNode, TMap<FString, int>& ExistingBones)
{
	if (RootNode == nullptr) return;

	FbxNodeAttribute* Attr = RootNode->GetNodeAttribute();
	if (Attr && Attr->GetAttributeType() == FbxNodeAttribute::eSkeleton)
	{
		FString BoneName = UTF8_TO_TCHAR(RootNode->GetName());
		if (ExistingBones.Contains(BoneName))
		{
			ExistingBones[BoneName] += 1;
			BoneName = FString::Printf(TEXT("%s_RENAMED_%d"), *BoneName, ExistingBones[BoneName]);
			RootNode->SetName(TCHAR_TO_UTF8(*BoneName));
		}
		else
		{
			ExistingBones.Add(BoneName, 1);
		}
	}

	for (int ChildIndex = 0; ChildIndex < RootNode->GetChildCount(); ++ChildIndex)
	{
		FbxNode * ChildNode = RootNode->GetChild(ChildIndex);
		RenameDuplicateBones(ChildNode, ExistingBones);
	}
}

void FDazToUnrealFbx::FixBoneRotations(FbxNode* RootNode)
{
	if (RootNode == nullptr) return;


	RootNode->LclRotation.Set(RootNode->PostRotation.Get());


	for (int ChildIndex = 0; ChildIndex < RootNode->GetChildCount(); ++ChildIndex)
	{
		FbxNode * ChildNode = RootNode->GetChild(ChildIndex);
		FixBoneRotations(ChildNode);
	}
}

void FDazToUnrealFbx::FixBindPose(FbxScene* Scene, FbxNode* RootNode)
{
	if (RootNode == nullptr) return;

	for (int PoseIndex = 0; PoseIndex < Scene->GetPoseCount(); PoseIndex++)
	{
		FbxPose* Pose = Scene->GetPose(PoseIndex);
		int PoseNodeIndex = Pose->Find(RootNode);
		if (PoseNodeIndex != -1)
		{
			FbxMatrix Matrix = Pose->GetMatrix(PoseNodeIndex);

			FbxVector4 TargetPosition;
			FbxVector4 TargetRotation;
			FbxVector4 TargetShearing;
			FbxVector4 TargetScale;
			double Sign;
			Matrix.GetElements(TargetPosition, TargetRotation, TargetShearing, TargetScale, Sign);

			FbxDouble3 Rotation = RootNode->PostRotation.Get();

			TargetRotation.Set(Rotation[0], Rotation[1], Rotation[2]);
			Matrix.SetTRS(TargetPosition, TargetRotation, TargetScale);

			Pose->Remove(PoseNodeIndex);
			Pose->Add(RootNode, Matrix, true);
		}
		/*for (int PoseNodeIndex = 0; PoseNodeIndex < Pose->GetCount(); PoseNodeIndex++)
		{
			FbxNode* PoseNode = Pose->GetNode(PoseIndex);
			if (PoseNode == RootNode)
			{

			}
		}*/
	}
	//RootNode->LclRotation.Set(RootNode->PostRotation.Get());


	for (int ChildIndex = 0; ChildIndex < RootNode->GetChildCount(); ++ChildIndex)
	{
		FbxNode * ChildNode = RootNode->GetChild(ChildIndex);
		FixBindPose(Scene, ChildNode);
	}
}