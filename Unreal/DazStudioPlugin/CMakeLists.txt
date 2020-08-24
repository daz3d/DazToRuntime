set(CMAKE_AUTOMOC ON)
set(CMAKE_INCLUDE_CURRENT_DIR ON)

set(DZ_PLUGIN_TGT_NAME dzunrealbridge)
set(DZ_PLUGIN_PROJECT_NAME "DzUnrealBridge")

# if building a plugin and you want the compiled result placed in the Daz Studio ./plugins directory
if(DAZ_STUDIO_EXE_DIR)
	set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${DAZ_STUDIO_EXE_DIR}/plugins)
	set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${DAZ_STUDIO_EXE_DIR}/plugins)
endif()

if(WIN32)
	set(OS_SOURCES 
		DzUnrealBridge.rc
		DzUnrealBridge.def
	)
elseif(APPLE)	
	set(OS_SOURCES "")
endif()

add_library( ${DZ_PLUGIN_TGT_NAME} SHARED	
	DzUnrealIcon.cpp
	DzUnrealAction.cpp
	DzUnrealAction.h
	DzUnrealDialog.cpp
	DzUnrealDialog.h
	DzUnrealMorphSelectionDialog.cpp
	DzUnrealMorphSelectionDialog.h
	pluginmain.cpp
	DzUnrealSubdivisionDialog.cpp
	DzUnrealSubdivisionDialog.h
	version.h
	${OS_SOURCES}
)

target_include_directories(common PUBLIC ${COMMON_LIB_INCLUDE_DIR})

target_link_libraries(${DZ_PLUGIN_TGT_NAME}
	PRIVATE 
	dzcore
	common
	${DZSDK_QT_CORE_TARGET}
	${DZSDK_QT_GUI_TARGET}
	${DZSDK_QT_SCRIPT_TARGET}
	${DZSDK_QT_NETWORK_TARGET}
)

set_target_properties (${DZ_PLUGIN_TGT_NAME}
	PROPERTIES
	FOLDER ""
	PROJECT_LABEL ${DZ_PLUGIN_PROJECT_NAME}
)