// Copyright Epic Games, Inc. All Rights Reserved.
/*===========================================================================
	Generated code exported from UnrealHeaderTool.
	DO NOT modify this manually! Edit the corresponding .h files instead!
===========================================================================*/

#include "UObject/ObjectMacros.h"
#include "UObject/ScriptMacros.h"

PRAGMA_DISABLE_DEPRECATION_WARNINGS
#ifdef DAZTOUNREALRUNTIME_AnimNode_DazMorphDriver_generated_h
#error "AnimNode_DazMorphDriver.generated.h already included, missing '#pragma once' in AnimNode_DazMorphDriver.h"
#endif
#define DAZTOUNREALRUNTIME_AnimNode_DazMorphDriver_generated_h

#define HostProject_Plugins_DazToUnreal_Source_DazToUnrealRuntime_Public_AnimNode_DazMorphDriver_h_34_GENERATED_BODY \
	friend struct Z_Construct_UScriptStruct_FAnimNode_DazMorphDriver_Statics; \
	static class UScriptStruct* StaticStruct(); \
	typedef FAnimNode_SkeletalControlBase Super;


template<> DAZTOUNREALRUNTIME_API UScriptStruct* StaticStruct<struct FAnimNode_DazMorphDriver>();

#undef CURRENT_FILE_ID
#define CURRENT_FILE_ID HostProject_Plugins_DazToUnreal_Source_DazToUnrealRuntime_Public_AnimNode_DazMorphDriver_h


#define FOREACH_ENUM_EDAZMORPHDRIVERROTATIONORDER(op) \
	op(EDazMorphDriverRotationOrder::Auto) \
	op(EDazMorphDriverRotationOrder::XYZ) \
	op(EDazMorphDriverRotationOrder::XZY) \
	op(EDazMorphDriverRotationOrder::YXZ) \
	op(EDazMorphDriverRotationOrder::YZX) \
	op(EDazMorphDriverRotationOrder::ZXY) \
	op(EDazMorphDriverRotationOrder::ZYX) 

enum class EDazMorphDriverRotationOrder : uint8;
template<> DAZTOUNREALRUNTIME_API UEnum* StaticEnum<EDazMorphDriverRotationOrder>();

PRAGMA_ENABLE_DEPRECATION_WARNINGS
