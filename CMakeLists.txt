#**********************************************************************
#	Copyright (C) 2002-2020 Daz 3D, Inc. All Rights Reserved.
#
#	This file is part of the DAZ Studio SDK.
#
#	This file may be used only in accordance with the DAZ Studio SDK
#	license provided with the DAZ Studio SDK.
#
#	The contents of this file may not be disclosed to third parties,
#	copied or duplicated in any form, in whole or in part, without the
#	prior written permission of Daz 3D, Inc, except as explicitly
#	allowed in the DAZ Studio SDK license.
#
#	See http://www.daz3d.com to contact DAZ 3D or for more
#	information about the DAZ Studio SDK.
#**********************************************************************

cmake_minimum_required(VERSION 3.4.0)

if(APPLE)
	if(NOT CMAKE_OSX_ARCHITECTURES)
  		message( FATAL_ERROR "Mac needs CMAKE_OSX_ARCHITECTURES, set to i386 or x86_64" )	
		return()
	endif()
endif(APPLE)

project("DazRunTimePlugins")

set_property(GLOBAL PROPERTY USE_FOLDERS ON)

set(DAZ_STUDIO_EXE_DIR "" CACHE PATH "Path to DAZ Studio, needs to be installed to a writeable location" )
if(NOT DAZ_STUDIO_EXE_DIR )
	message("Location to DAZ Studio not provided. Projects will build locally.")
endif()

if(WIN32)
	set(DZ_LIB_SUFFIX ".lib")
	set(DZ_BIN_SUFFIX ".dll")
	set(DZ_LIB_PREFIX "")
	set(UTIL_EXT ".exe")
	if(CMAKE_SIZEOF_VOID_P EQUAL 4)
		set(DZ_PLATFORM x86)
		set(DZ_MIXED_PLATFORM Win32)
		set(DZ_OS_PLATFORM Win32)
	elseif(CMAKE_SIZEOF_VOID_P EQUAL 8)
		set(DZ_PLATFORM x64)
		set(DZ_MIXED_PLATFORM x64)
		set(DZ_OS_PLATFORM Win64)
	else()
		message(FATAL_ERROR "Unknown architecture")
	endif()
elseif(APPLE)
	set(DZ_LIB_SUFFIX ".dylib")
	set(DZ_BIN_SUFFIX ".dylib")
	set(DZ_LIB_PREFIX "lib")	
	set(UTIL_EXT "")
	if(CMAKE_SIZEOF_VOID_P EQUAL 4)
		set(DZ_PLATFORM x86)
		set(DZ_MIXED_PLATFORM Mac32)
		set(DZ_OS_PLATFORM Mac32)
	elseif(CMAKE_SIZEOF_VOID_P EQUAL 8)
		set(DZ_PLATFORM x64)
		set(DZ_MIXED_PLATFORM Mac64)
		set(DZ_OS_PLATFORM Mac64)	
	else()
		message(FATAL_ERROR "Unknown architecture")		
	endif()
	set(CMAKE_MACOSX_RPATH TRUE)
	set(CMAKE_BUILD_WITH_INSTALL_RPATH TRUE)
else()
	message(FATAL_ERROR "Unknown architecture")	
endif(WIN32)

set(DAZ_SDK_DIR_DEFAULT "")
set(DAZ_SDK_CORE_RELATIVE_PATH "lib/${DZ_MIXED_PLATFORM}/${DZ_LIB_PREFIX}dzcore${DZ_LIB_SUFFIX}")
if(NOT DAZ_SDK_DIR)
	if(EXISTS "${CMAKE_CURRENT_LIST_DIR}/DAZStudio4.5+ SDK/${DAZ_SDK_CORE_RELATIVE_PATH}")
		set( DAZ_SDK_DIR_DEFAULT "${CMAKE_CURRENT_LIST_DIR}/DAZStudio4.5+ SDK" )
	endif()
endif()

set(DAZ_SDK_DIR ${DAZ_SDK_DIR_DEFAULT} CACHE PATH "Path to root of the DAZ Studio SDK" )

if(NOT DAZ_SDK_DIR)
	message(FATAL_ERROR "Missing path to DAZ Studio SDK")
	return()
endif()

set(QT_BINARY_DIR_DEFAULT "" CACHE PATH "Path to directory with QT binaries")
if(NOT QT_BINARY_DIR_DEFAULT)
	if(EXISTS "${DAZ_SDK_DIR}/bin/${DZ_MIXED_PLATFORM}/qmake${UTIL_EXT}")
		set( QT_BINARY_DIR_DEFAULT "${DAZ_SDK_DIR}/bin/${DZ_MIXED_PLATFORM}" )
	endif()
endif()

if(NOT QT_BINARY_DIR_DEFAULT)
	message(FATAL_ERROR "Missing path QT binaries. Check QT_BINARY_DIR_DEFAULT path")
	return()
endif()

find_package(OpenGL REQUIRED)	

#we only have release libraries for dzcore/qt so make sure even in debug they we use MD and undef debug
if(WIN32)
	add_compile_options( "/MD" "/U_DEBUG" )
endif()

# Set dzcore as import target
set(DZ_SDK_INCLUDE "${DAZ_SDK_DIR}/include" CACHE FILEPATH "path to daz sdk includes" )
set(DAZ_SDK_LIB "${DAZ_SDK_DIR}/${DAZ_SDK_CORE_RELATIVE_PATH}" CACHE FILEPATH "path to dzcore" )
if(NOT EXISTS ${DAZ_SDK_LIB})
	message(FATAL_ERROR "The library dzcore could not be located.  Check the path for DAZ_SDK_DIR.")
	return()
endif()

add_library(dzcore SHARED IMPORTED)
if(WIN32)
	set_property(TARGET dzcore APPEND PROPERTY IMPORTED_IMPLIB ${DAZ_SDK_LIB})
else()
	set_property(TARGET dzcore APPEND PROPERTY IMPORTED_LOCATION ${DAZ_SDK_LIB})
endif(WIN32)
set_property(TARGET dzcore APPEND PROPERTY INTERFACE_INCLUDE_DIRECTORIES "${DZ_SDK_INCLUDE}" )

# Set dpc as import target
set(DAZ_SDK_DPC_EXE "${DAZ_SDK_DIR}/bin/${DZ_MIXED_PLATFORM}/dpc${UTIL_EXT}" CACHE FILEPATH "path to sdk dpc" )
if(NOT EXISTS ${DAZ_SDK_DPC_EXE})
	message(FATAL_ERROR "The executable dpc could not be located.  Check the path for DAZ_SDK_DIR.")
	return()
endif()
add_executable(dpc IMPORTED)
set_property(TARGET dpc APPEND PROPERTY IMPORTED_LOCATION ${DAZ_SDK_DPC_EXE})

# Setup Qt from the DAZ SDK
if(WIN32)
	set(DAZ_SDK_QTCORE_LIBRARY "${DAZ_SDK_DIR}/lib/${DZ_MIXED_PLATFORM}/QtCore4.lib")
elseif(APPLE)
	set(DAZ_SDK_QTCORE_LIBRARY "${DAZ_SDK_DIR}/lib/${DZ_MIXED_PLATFORM}/QtCore.framework")
endif()

set(QT_QTCORE_LIBRARY_RELEASE ${DAZ_SDK_QTCORE_LIBRARY})
set(QT_BINARY_DIR "${QT_BINARY_DIR_DEFAULT}")
set(QT_QMAKE_EXECUTABLE "${QT_BINARY_DIR_DEFAULT}/qmake${UTIL_EXT}")
#set(QT_QMAKE_EXECUTABLE "${DAZ_SDK_DIR}/bin/${DZ_MIXED_PLATFORM}/qmake${UTIL_EXT}")
#set(QT_BINARY_DIR "${DAZ_SDK_DIR}/bin/${DZ_MIXED_PLATFORM}")
set(QT_HEADERS_DIR "${DAZ_SDK_DIR}/include")
set(QT_QTCORE_INCLUDE_DIR "${DAZ_SDK_DIR}/include/QtCore")

# the qt find module needs this folder but our build does not so just fake it
file(MAKE_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}/DUMMY_MKSPECS/default" )
set(QT_MKSPECS_DIR "${CMAKE_CURRENT_BINARY_DIR}/DUMMY_MKSPECS")

find_package(Qt4 4.8.7 REQUIRED QtCore QtGui QtScript QtOpenGL QtNetwork QtSql QtXml)

set(DZSDK_QT_CORE_TARGET Qt4::QtCore)
set(DZSDK_QT_GUI_TARGET Qt4::QtGui)
set(DZSDK_QT_SCRIPT_TARGET Qt4::QtScript)
set(DZSDK_QT_OPENGL_TARGET Qt4::QtOpenGL)
set(DZSDK_QT_NETWORK_TARGET Qt4::QtNetwork)
set(DZSDK_QT_SQL_TARGET	Qt4::QtSql)
set(DZSDK_QT_XML_TARGET	Qt4::QtXml)

list(APPEND CMAKE_AUTOMOC_MOC_OPTIONS -i)

add_subdirectory("Common")
#add_subdirectory("Unity")
add_subdirectory("Unreal")