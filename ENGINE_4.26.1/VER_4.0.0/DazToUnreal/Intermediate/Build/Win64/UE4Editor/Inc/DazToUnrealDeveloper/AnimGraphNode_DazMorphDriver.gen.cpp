// Copyright Epic Games, Inc. All Rights Reserved.
/*===========================================================================
	Generated code exported from UnrealHeaderTool.
	DO NOT modify this manually! Edit the corresponding .h files instead!
===========================================================================*/

#include "UObject/GeneratedCppIncludes.h"
#include "DazToUnrealDeveloper/Public/AnimGraphNode_DazMorphDriver.h"
#include "Serialization/ArchiveUObjectFromStructuredArchive.h"
#ifdef _MSC_VER
#pragma warning (push)
#pragma warning (disable : 4883)
#endif
PRAGMA_DISABLE_DEPRECATION_WARNINGS
void EmptyLinkFunctionForGeneratedCodeAnimGraphNode_DazMorphDriver() {}
// Cross Module References
	DAZTOUNREALDEVELOPER_API UClass* Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_NoRegister();
	DAZTOUNREALDEVELOPER_API UClass* Z_Construct_UClass_UAnimGraphNode_DazMorphDriver();
	ANIMGRAPH_API UClass* Z_Construct_UClass_UAnimGraphNode_SkeletalControlBase();
	UPackage* Z_Construct_UPackage__Script_DazToUnrealDeveloper();
	DAZTOUNREALRUNTIME_API UScriptStruct* Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver();
// End Cross Module References
	void UAnimGraphNode_DazMorphDriver::StaticRegisterNativesUAnimGraphNode_DazMorphDriver()
	{
	}
	UClass* Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_NoRegister()
	{
		return UAnimGraphNode_DazMorphDriver::StaticClass();
	}
	struct Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_Statics
	{
		static UObject* (*const DependentSingletons[])();
#if WITH_METADATA
		static const UE4CodeGen_Private::FMetaDataPairParam Class_MetaDataParams[];
#endif
#if WITH_METADATA
		static const UE4CodeGen_Private::FMetaDataPairParam NewProp_Node_MetaData[];
#endif
		static const UE4CodeGen_Private::FStructPropertyParams NewProp_Node;
		static const UE4CodeGen_Private::FPropertyParamsBase* const PropPointers[];
		static const FCppClassTypeInfoStatic StaticCppClassTypeInfo;
		static const UE4CodeGen_Private::FClassParams ClassParams;
	};
	UObject* (*const Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_Statics::DependentSingletons[])() = {
		(UObject* (*)())Z_Construct_UClass_UAnimGraphNode_SkeletalControlBase,
		(UObject* (*)())Z_Construct_UPackage__Script_DazToUnrealDeveloper,
	};
#if WITH_METADATA
	const UE4CodeGen_Private::FMetaDataPairParam Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_Statics::Class_MetaDataParams[] = {
		{ "Comment", "/**\n * This is the 'source version' of a bone driven controller, which maps part of the state from one bone to another (e.g., 2 * source.x -> target.z)\n */" },
		{ "IncludePath", "AnimGraphNode_DazMorphDriver.h" },
		{ "ModuleRelativePath", "Public/AnimGraphNode_DazMorphDriver.h" },
		{ "ToolTip", "This is the 'source version' of a bone driven controller, which maps part of the state from one bone to another (e.g., 2 * source.x -> target.z)" },
	};
#endif
#if WITH_METADATA
	const UE4CodeGen_Private::FMetaDataPairParam Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_Statics::NewProp_Node_MetaData[] = {
		{ "Category", "Settings" },
		{ "ModuleRelativePath", "Public/AnimGraphNode_DazMorphDriver.h" },
	};
#endif
	const UE4CodeGen_Private::FStructPropertyParams Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_Statics::NewProp_Node = { "Node", nullptr, (EPropertyFlags)0x0010000000000001, UE4CodeGen_Private::EPropertyGenFlags::Struct, RF_Public|RF_Transient|RF_MarkAsNative, 1, STRUCT_OFFSET(UAnimGraphNode_DazMorphDriver, Node), Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver, METADATA_PARAMS(Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_Statics::NewProp_Node_MetaData, UE_ARRAY_COUNT(Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_Statics::NewProp_Node_MetaData)) };
	const UE4CodeGen_Private::FPropertyParamsBase* const Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_Statics::PropPointers[] = {
		(const UE4CodeGen_Private::FPropertyParamsBase*)&Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_Statics::NewProp_Node,
	};
	const FCppClassTypeInfoStatic Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_Statics::StaticCppClassTypeInfo = {
		TCppClassTypeTraits<UAnimGraphNode_DazMorphDriver>::IsAbstract,
	};
	const UE4CodeGen_Private::FClassParams Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_Statics::ClassParams = {
		&UAnimGraphNode_DazMorphDriver::StaticClass,
		nullptr,
		&StaticCppClassTypeInfo,
		DependentSingletons,
		nullptr,
		Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_Statics::PropPointers,
		nullptr,
		UE_ARRAY_COUNT(DependentSingletons),
		0,
		UE_ARRAY_COUNT(Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_Statics::PropPointers),
		0,
		0x001000A0u,
		METADATA_PARAMS(Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_Statics::Class_MetaDataParams, UE_ARRAY_COUNT(Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_Statics::Class_MetaDataParams))
	};
	UClass* Z_Construct_UClass_UAnimGraphNode_DazMorphDriver()
	{
		static UClass* OuterClass = nullptr;
		if (!OuterClass)
		{
			UE4CodeGen_Private::ConstructUClass(OuterClass, Z_Construct_UClass_UAnimGraphNode_DazMorphDriver_Statics::ClassParams);
		}
		return OuterClass;
	}
	IMPLEMENT_CLASS(UAnimGraphNode_DazMorphDriver, 3252281798);
	template<> DAZTOUNREALDEVELOPER_API UClass* StaticClass<UAnimGraphNode_DazMorphDriver>()
	{
		return UAnimGraphNode_DazMorphDriver::StaticClass();
	}
	static FCompiledInDefer Z_CompiledInDefer_UClass_UAnimGraphNode_DazMorphDriver(Z_Construct_UClass_UAnimGraphNode_DazMorphDriver, &UAnimGraphNode_DazMorphDriver::StaticClass, TEXT("/Script/DazToUnrealDeveloper"), TEXT("UAnimGraphNode_DazMorphDriver"), false, nullptr, nullptr, nullptr);
	DEFINE_VTABLE_PTR_HELPER_CTOR(UAnimGraphNode_DazMorphDriver);
	IMPLEMENT_FSTRUCTUREDARCHIVE_SERIALIZER(UAnimGraphNode_DazMorphDriver)
PRAGMA_ENABLE_DEPRECATION_WARNINGS
#ifdef _MSC_VER
#pragma warning (pop)
#endif
