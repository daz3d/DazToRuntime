#pragma once
#include <dzaction.h>
#include <dznode.h>
#include <dzjsonwriter.h>
#include <QtCore/qfile.h>
#include <QtCore/qtextstream.h>
#include <DzRuntimePluginAction.h>
#include "DzUnrealSubdivisionDialog.h"

class DzUnrealAction : public DzRuntimePluginAction {
	 Q_OBJECT
public:
	 DzUnrealAction();

protected:
	 int Port;
	 DzUnrealSubdivisionDialog* SubdivisionDialog;

	 void executeAction();
	 void WriteMaterials(DzNode* Node, DzJsonWriter& Stream);
	 void WriteConfiguration();
	 void SetExportOptions(DzFileIOSettings& ExportOptions);
};