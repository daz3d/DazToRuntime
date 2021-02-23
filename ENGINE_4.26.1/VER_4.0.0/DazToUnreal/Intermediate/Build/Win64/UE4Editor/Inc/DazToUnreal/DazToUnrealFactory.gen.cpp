// Copyright Epic Games, Inc. All Rights Reserved.
/*===========================================================================
	Generated code exported from UnrealHeaderTool.
	DO NOT modify this manually! Edit the corresponding .h files instead!
===========================================================================*/

#include "UObject/GeneratedCppIncludes.h"
#include "DazToUnreal/Public/DazToUnrealFactory.h"
#ifdef _MSC_VER
#pragma warning (push)
#pragma warning (disable : 4883)
#endif
PRAGMA_DISABLE_DEPRECATION_WARNINGS
void EmptyLinkFunctionForGeneratedCodeDazToUnrealFactory() {}
// Cross Module References
	DAZTOUNREAL_API UClass* Z_Construct_UClass_UDazToUnrealFactory_NoRegister();
	DAZTOUNREAL_API UClass* Z_Construct_UClass_UDazToUnrealFactory();
	UNREALED_API UClass* Z_Construct_UClass_UFactory();
	UPackage* Z_Construct_UPackage__Script_DazToUnreal();
// End Cross Module References
	void UDazToUnrealFactory::StaticRegisterNativesUDazToUnrealFactory()
	{
	}
	UClass* Z_Construct_UClass_UDazToUnrealFactory_NoRegister()
	{
		return UDazToUnrealFactory::StaticClass();
	}
	struct Z_Construct_UClass_UDazToUnrealFactory_Statics
	{
		static UObject* (*const DependentSingletons[])();
#if WITH_METADATA
		static const UE4CodeGen_Private::FMetaDataPairParam Class_MetaDataParams[];
#endif
		static const FCppClassTypeInfoStatic StaticCppClassTypeInfo;
		static const UE4CodeGen_Private::FClassParams ClassParams;
	};
	UObject* (*const Z_Construct_UClass_UDazToUnrealFactory_Statics::DependentSingletons[])() = {
		(UObject* (*)())Z_Construct_UClass_UFactory,
		(UObject* (*)())Z_Construct_UPackage__Script_DazToUnreal,
	};
#if WITH_METADATA
	const UE4CodeGen_Private::FMetaDataPairParam Z_Construct_UClass_UDazToUnrealFactory_Statics::Class_MetaDataParams[] = {
		{ "Comment", "// The Daz Studio plugin creates a dtu file with the same string that's send over the network.  This factory allows that file to be imported to start the same process.\n" },
		{ "IncludePath", "DazToUnrealFactory.h" },
		{ "ModuleRelativePath", "Public/DazToUnrealFactory.h" },
		{ "ToolTip", "The Daz Studio plugin creates a dtu file with the same string that's send over the network.  This factory allows that file to be imported to start the same process." },
	};
#endif
	const FCppClassTypeInfoStatic Z_Construct_UClass_UDazToUnrealFactory_Statics::StaticCppClassTypeInfo = {
		TCppClassTypeTraits<UDazToUnrealFactory>::IsAbstract,
	};
	const UE4CodeGen_Private::FClassParams Z_Construct_UClass_UDazToUnrealFactory_Statics::ClassParams = {
		&UDazToUnrealFactory::StaticClass,
		nullptr,
		&StaticCppClassTypeInfo,
		DependentSingletons,
		nullptr,
		nullptr,
		nullptr,
		UE_ARRAY_COUNT(DependentSingletons),
		0,
		0,
		0,
		0x000000A0u,
		METADATA_PARAMS(Z_Construct_UClass_UDazToUnrealFactory_Statics::Class_MetaDataParams, UE_ARRAY_COUNT(Z_Construct_UClass_UDazToUnrealFactory_Statics::Class_MetaDataParams))
	};
	UClass* Z_Construct_UClass_UDazToUnrealFactory()
	{
		static UClass* OuterClass = nullptr;
		if (!OuterClass)
		{
			UE4CodeGen_Private::ConstructUClass(OuterClass, Z_Construct_UClass_UDazToUnrealFactory_Statics::ClassParams);
		}
		return OuterClass;
	}
	IMPLEMENT_CLASS(UDazToUnrealFactory, 410160570);
	template<> DAZTOUNREAL_API UClass* StaticClass<UDazToUnrealFactory>()
	{
		return UDazToUnrealFactory::StaticClass();
	}
	static FCompiledInDefer Z_CompiledInDefer_UClass_UDazToUnrealFactory(Z_Construct_UClass_UDazToUnrealFactory, &UDazToUnrealFactory::StaticClass, TEXT("/Script/DazToUnreal"), TEXT("UDazToUnrealFactory"), false, nullptr, nullptr, nullptr);
	DEFINE_VTABLE_PTR_HELPER_CTOR(UDazToUnrealFactory);
PRAGMA_ENABLE_DEPRECATION_WARNINGS
#ifdef _MSC_VER
#pragma warning (pop)
#endif
