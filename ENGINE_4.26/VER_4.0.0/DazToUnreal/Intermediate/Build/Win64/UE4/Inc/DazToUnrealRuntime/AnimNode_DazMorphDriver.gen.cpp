// Copyright Epic Games, Inc. All Rights Reserved.
/*===========================================================================
	Generated code exported from UnrealHeaderTool.
	DO NOT modify this manually! Edit the corresponding .h files instead!
===========================================================================*/

#include "UObject/GeneratedCppIncludes.h"
#include "DazToUnrealRuntime/Public/AnimNode_DazMorphDriver.h"
#ifdef _MSC_VER
#pragma warning (push)
#pragma warning (disable : 4883)
#endif
PRAGMA_DISABLE_DEPRECATION_WARNINGS
void EmptyLinkFunctionForGeneratedCodeAnimNode_DazMorphDriver() {}
// Cross Module References
	DAZTOUNREALRUNTIME_API UEnum* Z_Construct_UEnum_DazToUnrealRuntime_EDazMorphDriverRotationOrder();
	UPackage* Z_Construct_UPackage__Script_DazToUnrealRuntime();
	DAZTOUNREALRUNTIME_API UScriptStruct* Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver();
	ANIMGRAPHRUNTIME_API UScriptStruct* Z_Construct_UScriptStruct_FAnimNode_SkeletalControlBase();
	ENGINE_API UScriptStruct* Z_Construct_UScriptStruct_FBoneReference();
	ENGINE_API UClass* Z_Construct_UClass_UCurveFloat_NoRegister();
	ENGINE_API UEnum* Z_Construct_UEnum_Engine_EComponentType();
// End Cross Module References
	static UEnum* EDazMorphDriverRotationOrder_StaticEnum()
	{
		static UEnum* Singleton = nullptr;
		if (!Singleton)
		{
			Singleton = GetStaticEnum(Z_Construct_UEnum_DazToUnrealRuntime_EDazMorphDriverRotationOrder, Z_Construct_UPackage__Script_DazToUnrealRuntime(), TEXT("EDazMorphDriverRotationOrder"));
		}
		return Singleton;
	}
	template<> DAZTOUNREALRUNTIME_API UEnum* StaticEnum<EDazMorphDriverRotationOrder>()
	{
		return EDazMorphDriverRotationOrder_StaticEnum();
	}
	static FCompiledInDeferEnum Z_CompiledInDeferEnum_UEnum_EDazMorphDriverRotationOrder(EDazMorphDriverRotationOrder_StaticEnum, TEXT("/Script/DazToUnrealRuntime"), TEXT("EDazMorphDriverRotationOrder"), false, nullptr, nullptr);
	uint32 Get_Z_Construct_UEnum_DazToUnrealRuntime_EDazMorphDriverRotationOrder_Hash() { return 3973897413U; }
	UEnum* Z_Construct_UEnum_DazToUnrealRuntime_EDazMorphDriverRotationOrder()
	{
#if WITH_HOT_RELOAD
		UPackage* Outer = Z_Construct_UPackage__Script_DazToUnrealRuntime();
		static UEnum* ReturnEnum = FindExistingEnumIfHotReloadOrDynamic(Outer, TEXT("EDazMorphDriverRotationOrder"), 0, Get_Z_Construct_UEnum_DazToUnrealRuntime_EDazMorphDriverRotationOrder_Hash(), false);
#else
		static UEnum* ReturnEnum = nullptr;
#endif // WITH_HOT_RELOAD
		if (!ReturnEnum)
		{
			static const UE4CodeGen_Private::FEnumeratorParam Enumerators[] = {
				{ "EDazMorphDriverRotationOrder::Auto", (int64)EDazMorphDriverRotationOrder::Auto },
				{ "EDazMorphDriverRotationOrder::XYZ", (int64)EDazMorphDriverRotationOrder::XYZ },
				{ "EDazMorphDriverRotationOrder::XZY", (int64)EDazMorphDriverRotationOrder::XZY },
				{ "EDazMorphDriverRotationOrder::YXZ", (int64)EDazMorphDriverRotationOrder::YXZ },
				{ "EDazMorphDriverRotationOrder::YZX", (int64)EDazMorphDriverRotationOrder::YZX },
				{ "EDazMorphDriverRotationOrder::ZXY", (int64)EDazMorphDriverRotationOrder::ZXY },
				{ "EDazMorphDriverRotationOrder::ZYX", (int64)EDazMorphDriverRotationOrder::ZYX },
			};
#if WITH_METADATA
			const UE4CodeGen_Private::FMetaDataPairParam Enum_MetaDataParams[] = {
				{ "Auto.Name", "EDazMorphDriverRotationOrder::Auto" },
				{ "ModuleRelativePath", "Public/AnimNode_DazMorphDriver.h" },
				{ "XYZ.Name", "EDazMorphDriverRotationOrder::XYZ" },
				{ "XZY.Name", "EDazMorphDriverRotationOrder::XZY" },
				{ "YXZ.Name", "EDazMorphDriverRotationOrder::YXZ" },
				{ "YZX.Name", "EDazMorphDriverRotationOrder::YZX" },
				{ "ZXY.Name", "EDazMorphDriverRotationOrder::ZXY" },
				{ "ZYX.Name", "EDazMorphDriverRotationOrder::ZYX" },
			};
#endif
			static const UE4CodeGen_Private::FEnumParams EnumParams = {
				(UObject*(*)())Z_Construct_UPackage__Script_DazToUnrealRuntime,
				nullptr,
				"EDazMorphDriverRotationOrder",
				"EDazMorphDriverRotationOrder",
				Enumerators,
				UE_ARRAY_COUNT(Enumerators),
				RF_Public|RF_Transient|RF_MarkAsNative,
				EEnumFlags::None,
				UE4CodeGen_Private::EDynamicType::NotDynamic,
				(uint8)UEnum::ECppForm::EnumClass,
				METADATA_PARAMS(Enum_MetaDataParams, UE_ARRAY_COUNT(Enum_MetaDataParams))
			};
			UE4CodeGen_Private::ConstructUEnum(ReturnEnum, EnumParams);
		}
		return ReturnEnum;
	}

static_assert(std::is_polymorphic<FAnimNode_DazMorphDriver>() == std::is_polymorphic<FAnimNode_SkeletalControlBase>(), "USTRUCT FAnimNode_DazMorphDriver cannot be polymorphic unless super FAnimNode_SkeletalControlBase is polymorphic");

class UScriptStruct* FAnimNode_DazMorphDriver::StaticStruct()
{
	static class UScriptStruct* Singleton = NULL;
	if (!Singleton)
	{
		extern DAZTOUNREALRUNTIME_API uint32 Get_Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Hash();
		Singleton = GetStaticStruct(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver, Z_Construct_UPackage__Script_DazToUnrealRuntime(), TEXT("AnimNode_DazMorphDriver"), sizeof(FAnimNode_DazMorphDriver), Get_Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Hash());
	}
	return Singleton;
}
template<> DAZTOUNREALRUNTIME_API UScriptStruct* StaticStruct<FAnimNode_DazMorphDriver>()
{
	return FAnimNode_DazMorphDriver::StaticStruct();
}
static FCompiledInDeferStruct Z_CompiledInDeferStruct_UScriptStruct_FAnimNode_DazMorphDriver(FAnimNode_DazMorphDriver::StaticStruct, TEXT("/Script/DazToUnrealRuntime"), TEXT("AnimNode_DazMorphDriver"), false, nullptr, nullptr);
static struct FScriptStruct_DazToUnrealRuntime_StaticRegisterNativesFAnimNode_DazMorphDriver
{
	FScriptStruct_DazToUnrealRuntime_StaticRegisterNativesFAnimNode_DazMorphDriver()
	{
		UScriptStruct::DeferCppStructOps(FName(TEXT("AnimNode_DazMorphDriver")),new UScriptStruct::TCppStructOps<FAnimNode_DazMorphDriver>);
	}
} ScriptStruct_DazToUnrealRuntime_StaticRegisterNativesFAnimNode_DazMorphDriver;
	struct Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics
	{
#if WITH_METADATA
		static const UE4CodeGen_Private::FMetaDataPairParam Struct_MetaDataParams[];
#endif
		static void* NewStructOps();
#if WITH_METADATA
		static const UE4CodeGen_Private::FMetaDataPairParam NewProp_SourceBone_MetaData[];
#endif
		static const UE4CodeGen_Private::FStructPropertyParams NewProp_SourceBone;
#if WITH_METADATA
		static const UE4CodeGen_Private::FMetaDataPairParam NewProp_DrivingCurve_MetaData[];
#endif
		static const UE4CodeGen_Private::FObjectPropertyParams NewProp_DrivingCurve;
#if WITH_METADATA
		static const UE4CodeGen_Private::FMetaDataPairParam NewProp_Multiplier_MetaData[];
#endif
		static const UE4CodeGen_Private::FFloatPropertyParams NewProp_Multiplier;
#if WITH_METADATA
		static const UE4CodeGen_Private::FMetaDataPairParam NewProp_RangeMin_MetaData[];
#endif
		static const UE4CodeGen_Private::FFloatPropertyParams NewProp_RangeMin;
#if WITH_METADATA
		static const UE4CodeGen_Private::FMetaDataPairParam NewProp_RangeMax_MetaData[];
#endif
		static const UE4CodeGen_Private::FFloatPropertyParams NewProp_RangeMax;
#if WITH_METADATA
		static const UE4CodeGen_Private::FMetaDataPairParam NewProp_RemappedMin_MetaData[];
#endif
		static const UE4CodeGen_Private::FFloatPropertyParams NewProp_RemappedMin;
#if WITH_METADATA
		static const UE4CodeGen_Private::FMetaDataPairParam NewProp_RemappedMax_MetaData[];
#endif
		static const UE4CodeGen_Private::FFloatPropertyParams NewProp_RemappedMax;
#if WITH_METADATA
		static const UE4CodeGen_Private::FMetaDataPairParam NewProp_MorphName_MetaData[];
#endif
		static const UE4CodeGen_Private::FNamePropertyParams NewProp_MorphName;
#if WITH_METADATA
		static const UE4CodeGen_Private::FMetaDataPairParam NewProp_SourceComponent_MetaData[];
#endif
		static const UE4CodeGen_Private::FBytePropertyParams NewProp_SourceComponent;
		static const UE4CodeGen_Private::FBytePropertyParams NewProp_RotationConversionOrder_Underlying;
#if WITH_METADATA
		static const UE4CodeGen_Private::FMetaDataPairParam NewProp_RotationConversionOrder_MetaData[];
#endif
		static const UE4CodeGen_Private::FEnumPropertyParams NewProp_RotationConversionOrder;
#if WITH_METADATA
		static const UE4CodeGen_Private::FMetaDataPairParam NewProp_bUseRange_MetaData[];
#endif
		static void NewProp_bUseRange_SetBit(void* Obj);
		static const UE4CodeGen_Private::FBoolPropertyParams NewProp_bUseRange;
		static const UE4CodeGen_Private::FPropertyParamsBase* const PropPointers[];
		static const UE4CodeGen_Private::FStructParams ReturnStructParams;
	};
#if WITH_METADATA
	const UE4CodeGen_Private::FMetaDataPairParam Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::Struct_MetaDataParams[] = {
		{ "Comment", "/**\n * This is the runtime version of a bone driven controller, which maps part of the state from one bone to another (e.g., 2 * source.x -> target.z)\n */" },
		{ "ModuleRelativePath", "Public/AnimNode_DazMorphDriver.h" },
		{ "ToolTip", "This is the runtime version of a bone driven controller, which maps part of the state from one bone to another (e.g., 2 * source.x -> target.z)" },
	};
#endif
	void* Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewStructOps()
	{
		return (UScriptStruct::ICppStructOps*)new UScriptStruct::TCppStructOps<FAnimNode_DazMorphDriver>();
	}
#if WITH_METADATA
	const UE4CodeGen_Private::FMetaDataPairParam Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_SourceBone_MetaData[] = {
		{ "Category", "Source (driver)" },
		{ "Comment", "// Bone to use as controller input\n" },
		{ "ModuleRelativePath", "Public/AnimNode_DazMorphDriver.h" },
		{ "ToolTip", "Bone to use as controller input" },
	};
#endif
	const UE4CodeGen_Private::FStructPropertyParams Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_SourceBone = { "SourceBone", nullptr, (EPropertyFlags)0x0010000000000001, UE4CodeGen_Private::EPropertyGenFlags::Struct, RF_Public|RF_Transient|RF_MarkAsNative, 1, STRUCT_OFFSET(FAnimNode_DazMorphDriver, SourceBone), Z_Construct_UScriptStruct_FBoneReference, METADATA_PARAMS(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_SourceBone_MetaData, UE_ARRAY_COUNT(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_SourceBone_MetaData)) };
#if WITH_METADATA
	const UE4CodeGen_Private::FMetaDataPairParam Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_DrivingCurve_MetaData[] = {
		{ "Category", "Mapping" },
		{ "Comment", "/** Curve used to map from the source attribute to the driven attributes if present (otherwise the Multiplier will be used) */" },
		{ "ModuleRelativePath", "Public/AnimNode_DazMorphDriver.h" },
		{ "ToolTip", "Curve used to map from the source attribute to the driven attributes if present (otherwise the Multiplier will be used)" },
	};
#endif
	const UE4CodeGen_Private::FObjectPropertyParams Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_DrivingCurve = { "DrivingCurve", nullptr, (EPropertyFlags)0x0010000000000001, UE4CodeGen_Private::EPropertyGenFlags::Object, RF_Public|RF_Transient|RF_MarkAsNative, 1, STRUCT_OFFSET(FAnimNode_DazMorphDriver, DrivingCurve), Z_Construct_UClass_UCurveFloat_NoRegister, METADATA_PARAMS(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_DrivingCurve_MetaData, UE_ARRAY_COUNT(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_DrivingCurve_MetaData)) };
#if WITH_METADATA
	const UE4CodeGen_Private::FMetaDataPairParam Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_Multiplier_MetaData[] = {
		{ "Category", "Mapping" },
		{ "Comment", "// Multiplier to apply to the input value (Note: Ignored when a curve is used)\n" },
		{ "ModuleRelativePath", "Public/AnimNode_DazMorphDriver.h" },
		{ "ToolTip", "Multiplier to apply to the input value (Note: Ignored when a curve is used)" },
	};
#endif
	const UE4CodeGen_Private::FFloatPropertyParams Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_Multiplier = { "Multiplier", nullptr, (EPropertyFlags)0x0010000000000001, UE4CodeGen_Private::EPropertyGenFlags::Float, RF_Public|RF_Transient|RF_MarkAsNative, 1, STRUCT_OFFSET(FAnimNode_DazMorphDriver, Multiplier), METADATA_PARAMS(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_Multiplier_MetaData, UE_ARRAY_COUNT(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_Multiplier_MetaData)) };
#if WITH_METADATA
	const UE4CodeGen_Private::FMetaDataPairParam Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RangeMin_MetaData[] = {
		{ "Category", "Mapping" },
		{ "Comment", "// Minimum limit of the input value (mapped to RemappedMin, only used when limiting the source range)\n// If this is rotation, the unit is radian\n" },
		{ "DisplayName", "Source Range Min" },
		{ "EditCondition", "bUseRange" },
		{ "ModuleRelativePath", "Public/AnimNode_DazMorphDriver.h" },
		{ "ToolTip", "Minimum limit of the input value (mapped to RemappedMin, only used when limiting the source range)\nIf this is rotation, the unit is radian" },
	};
#endif
	const UE4CodeGen_Private::FFloatPropertyParams Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RangeMin = { "RangeMin", nullptr, (EPropertyFlags)0x0010000000000001, UE4CodeGen_Private::EPropertyGenFlags::Float, RF_Public|RF_Transient|RF_MarkAsNative, 1, STRUCT_OFFSET(FAnimNode_DazMorphDriver, RangeMin), METADATA_PARAMS(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RangeMin_MetaData, UE_ARRAY_COUNT(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RangeMin_MetaData)) };
#if WITH_METADATA
	const UE4CodeGen_Private::FMetaDataPairParam Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RangeMax_MetaData[] = {
		{ "Category", "Mapping" },
		{ "Comment", "// Maximum limit of the input value (mapped to RemappedMax, only used when limiting the source range)\n// If this is rotation, the unit is radian\n" },
		{ "DisplayName", "Source Range Max" },
		{ "EditCondition", "bUseRange" },
		{ "ModuleRelativePath", "Public/AnimNode_DazMorphDriver.h" },
		{ "ToolTip", "Maximum limit of the input value (mapped to RemappedMax, only used when limiting the source range)\nIf this is rotation, the unit is radian" },
	};
#endif
	const UE4CodeGen_Private::FFloatPropertyParams Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RangeMax = { "RangeMax", nullptr, (EPropertyFlags)0x0010000000000001, UE4CodeGen_Private::EPropertyGenFlags::Float, RF_Public|RF_Transient|RF_MarkAsNative, 1, STRUCT_OFFSET(FAnimNode_DazMorphDriver, RangeMax), METADATA_PARAMS(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RangeMax_MetaData, UE_ARRAY_COUNT(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RangeMax_MetaData)) };
#if WITH_METADATA
	const UE4CodeGen_Private::FMetaDataPairParam Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RemappedMin_MetaData[] = {
		{ "Category", "Mapping" },
		{ "Comment", "// Minimum value to apply to the destination (remapped from the input range)\n// If this is rotation, the unit is radian\n" },
		{ "DisplayName", "Mapped Range Min" },
		{ "EditCondition", "bUseRange" },
		{ "ModuleRelativePath", "Public/AnimNode_DazMorphDriver.h" },
		{ "ToolTip", "Minimum value to apply to the destination (remapped from the input range)\nIf this is rotation, the unit is radian" },
	};
#endif
	const UE4CodeGen_Private::FFloatPropertyParams Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RemappedMin = { "RemappedMin", nullptr, (EPropertyFlags)0x0010000000000001, UE4CodeGen_Private::EPropertyGenFlags::Float, RF_Public|RF_Transient|RF_MarkAsNative, 1, STRUCT_OFFSET(FAnimNode_DazMorphDriver, RemappedMin), METADATA_PARAMS(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RemappedMin_MetaData, UE_ARRAY_COUNT(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RemappedMin_MetaData)) };
#if WITH_METADATA
	const UE4CodeGen_Private::FMetaDataPairParam Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RemappedMax_MetaData[] = {
		{ "Category", "Mapping" },
		{ "Comment", "// Maximum value to apply to the destination (remapped from the input range)\n// If this is rotation, the unit is radian\n" },
		{ "DisplayName", "Mapped Range Max" },
		{ "EditCondition", "bUseRange" },
		{ "ModuleRelativePath", "Public/AnimNode_DazMorphDriver.h" },
		{ "ToolTip", "Maximum value to apply to the destination (remapped from the input range)\nIf this is rotation, the unit is radian" },
	};
#endif
	const UE4CodeGen_Private::FFloatPropertyParams Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RemappedMax = { "RemappedMax", nullptr, (EPropertyFlags)0x0010000000000001, UE4CodeGen_Private::EPropertyGenFlags::Float, RF_Public|RF_Transient|RF_MarkAsNative, 1, STRUCT_OFFSET(FAnimNode_DazMorphDriver, RemappedMax), METADATA_PARAMS(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RemappedMax_MetaData, UE_ARRAY_COUNT(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RemappedMax_MetaData)) };
#if WITH_METADATA
	const UE4CodeGen_Private::FMetaDataPairParam Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_MorphName_MetaData[] = {
		{ "Category", "Destination (driven)" },
		{ "Comment", "/** Name of Morph Target to drive using the source attribute */" },
		{ "ModuleRelativePath", "Public/AnimNode_DazMorphDriver.h" },
		{ "ToolTip", "Name of Morph Target to drive using the source attribute" },
	};
#endif
	const UE4CodeGen_Private::FNamePropertyParams Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_MorphName = { "MorphName", nullptr, (EPropertyFlags)0x0010000000000001, UE4CodeGen_Private::EPropertyGenFlags::Name, RF_Public|RF_Transient|RF_MarkAsNative, 1, STRUCT_OFFSET(FAnimNode_DazMorphDriver, MorphName), METADATA_PARAMS(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_MorphName_MetaData, UE_ARRAY_COUNT(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_MorphName_MetaData)) };
#if WITH_METADATA
	const UE4CodeGen_Private::FMetaDataPairParam Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_SourceComponent_MetaData[] = {
		{ "Category", "Source (driver)" },
		{ "Comment", "// Transform component to use as input\n" },
		{ "ModuleRelativePath", "Public/AnimNode_DazMorphDriver.h" },
		{ "ToolTip", "Transform component to use as input" },
	};
#endif
	const UE4CodeGen_Private::FBytePropertyParams Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_SourceComponent = { "SourceComponent", nullptr, (EPropertyFlags)0x0010000000000001, UE4CodeGen_Private::EPropertyGenFlags::Byte, RF_Public|RF_Transient|RF_MarkAsNative, 1, STRUCT_OFFSET(FAnimNode_DazMorphDriver, SourceComponent), Z_Construct_UEnum_Engine_EComponentType, METADATA_PARAMS(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_SourceComponent_MetaData, UE_ARRAY_COUNT(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_SourceComponent_MetaData)) };
	const UE4CodeGen_Private::FBytePropertyParams Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RotationConversionOrder_Underlying = { "UnderlyingType", nullptr, (EPropertyFlags)0x0000000000000000, UE4CodeGen_Private::EPropertyGenFlags::Byte, RF_Public|RF_Transient|RF_MarkAsNative, 1, 0, nullptr, METADATA_PARAMS(nullptr, 0) };
#if WITH_METADATA
	const UE4CodeGen_Private::FMetaDataPairParam Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RotationConversionOrder_MetaData[] = {
		{ "Category", "Source (driver)" },
		{ "Comment", "// Transform component to use as input\n" },
		{ "ModuleRelativePath", "Public/AnimNode_DazMorphDriver.h" },
		{ "ToolTip", "Transform component to use as input" },
	};
#endif
	const UE4CodeGen_Private::FEnumPropertyParams Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RotationConversionOrder = { "RotationConversionOrder", nullptr, (EPropertyFlags)0x0010000000000001, UE4CodeGen_Private::EPropertyGenFlags::Enum, RF_Public|RF_Transient|RF_MarkAsNative, 1, STRUCT_OFFSET(FAnimNode_DazMorphDriver, RotationConversionOrder), Z_Construct_UEnum_DazToUnrealRuntime_EDazMorphDriverRotationOrder, METADATA_PARAMS(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RotationConversionOrder_MetaData, UE_ARRAY_COUNT(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RotationConversionOrder_MetaData)) };
#if WITH_METADATA
	const UE4CodeGen_Private::FMetaDataPairParam Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_bUseRange_MetaData[] = {
		{ "Category", "Mapping" },
		{ "Comment", "// Whether or not to clamp the driver value and remap it before scaling it\n" },
		{ "DisplayName", "Remap Source" },
		{ "ModuleRelativePath", "Public/AnimNode_DazMorphDriver.h" },
		{ "ToolTip", "Whether or not to clamp the driver value and remap it before scaling it" },
	};
#endif
	void Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_bUseRange_SetBit(void* Obj)
	{
		((FAnimNode_DazMorphDriver*)Obj)->bUseRange = 1;
	}
	const UE4CodeGen_Private::FBoolPropertyParams Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_bUseRange = { "bUseRange", nullptr, (EPropertyFlags)0x0010000000000001, UE4CodeGen_Private::EPropertyGenFlags::Bool , RF_Public|RF_Transient|RF_MarkAsNative, 1, sizeof(uint8), sizeof(FAnimNode_DazMorphDriver), &Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_bUseRange_SetBit, METADATA_PARAMS(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_bUseRange_MetaData, UE_ARRAY_COUNT(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_bUseRange_MetaData)) };
	const UE4CodeGen_Private::FPropertyParamsBase* const Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::PropPointers[] = {
		(const UE4CodeGen_Private::FPropertyParamsBase*)&Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_SourceBone,
		(const UE4CodeGen_Private::FPropertyParamsBase*)&Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_DrivingCurve,
		(const UE4CodeGen_Private::FPropertyParamsBase*)&Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_Multiplier,
		(const UE4CodeGen_Private::FPropertyParamsBase*)&Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RangeMin,
		(const UE4CodeGen_Private::FPropertyParamsBase*)&Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RangeMax,
		(const UE4CodeGen_Private::FPropertyParamsBase*)&Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RemappedMin,
		(const UE4CodeGen_Private::FPropertyParamsBase*)&Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RemappedMax,
		(const UE4CodeGen_Private::FPropertyParamsBase*)&Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_MorphName,
		(const UE4CodeGen_Private::FPropertyParamsBase*)&Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_SourceComponent,
		(const UE4CodeGen_Private::FPropertyParamsBase*)&Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RotationConversionOrder_Underlying,
		(const UE4CodeGen_Private::FPropertyParamsBase*)&Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_RotationConversionOrder,
		(const UE4CodeGen_Private::FPropertyParamsBase*)&Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::NewProp_bUseRange,
	};
	const UE4CodeGen_Private::FStructParams Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::ReturnStructParams = {
		(UObject* (*)())Z_Construct_UPackage__Script_DazToUnrealRuntime,
		Z_Construct_UScriptStruct_FAnimNode_SkeletalControlBase,
		&NewStructOps,
		"AnimNode_DazMorphDriver",
		sizeof(FAnimNode_DazMorphDriver),
		alignof(FAnimNode_DazMorphDriver),
		Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::PropPointers,
		UE_ARRAY_COUNT(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::PropPointers),
		RF_Public|RF_Transient|RF_MarkAsNative,
		EStructFlags(0x00000201),
		METADATA_PARAMS(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::Struct_MetaDataParams, UE_ARRAY_COUNT(Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::Struct_MetaDataParams))
	};
	UScriptStruct* Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver()
	{
#if WITH_HOT_RELOAD
		extern uint32 Get_Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Hash();
		UPackage* Outer = Z_Construct_UPackage__Script_DazToUnrealRuntime();
		static UScriptStruct* ReturnStruct = FindExistingStructIfHotReloadOrDynamic(Outer, TEXT("AnimNode_DazMorphDriver"), sizeof(FAnimNode_DazMorphDriver), Get_Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Hash(), false);
#else
		static UScriptStruct* ReturnStruct = nullptr;
#endif
		if (!ReturnStruct)
		{
			UE4CodeGen_Private::ConstructUScriptStruct(ReturnStruct, Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics::ReturnStructParams);
		}
		return ReturnStruct;
	}
	uint32 Get_Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Hash() { return 3980138696U; }
PRAGMA_ENABLE_DEPRECATION_WARNINGS
#ifdef _MSC_VER
#pragma warning (pop)
#endif
