#pragma once
#include <dzaction.h>
#include <dznode.h>
#include <dzjsonwriter.h>
#include <QtCore/qfile.h>
#include <QtCore/qtextstream.h>
#include <DzRuntimePluginAction.h>
#include "DzUnitySubdivisionDialog.h"

class DzUnityAction : public DzRuntimePluginAction {
	 Q_OBJECT
public:
	 DzUnityAction();

protected:
	 DzUnitySubdivisionDialog* SubdivisionDialog;

	 void executeAction();
	 void WriteMaterials(DzNode* Node, DzJsonWriter& Stream);
	 void WriteConfiguration();
	 void SetExportOptions(DzFileIOSettings& ExportOptions);
	 void CreateEditorScripts();
	 void CreateShaders();
};