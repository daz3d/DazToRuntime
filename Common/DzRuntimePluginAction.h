#pragma once
#include <dzaction.h>
#include <dznode.h>
#include <DzFileIOSettings.h>

#include "QtCore/qfile.h"
#include "QtCore/qtextstream.h"

class DzRuntimePluginAction : public DzAction {
	 Q_OBJECT
public:

	 DzRuntimePluginAction(const QString& text = QString::null, const QString& desc = QString::null);
	 virtual ~DzRuntimePluginAction();

protected:
	 QString CharacterName;
	 QString ImportFolder;
	 QString CharacterFolder;
	 QString CharacterFBX;
	 QString AssetType;
	 QString MorphString;
	 QString FBXVersion;
	 QMap<QString,QString> MorphMapping;

	 bool ExportMorphs;
	 bool ExportSubdivisions;
	 bool ShowFbxDialog;
	 DzNode* Selection;

	 virtual QString getActionGroup() const { return tr("Bridges"); }
	 virtual QString getDefaultMenuPath() const { return tr("&File/Send To"); }

	 virtual void Export();

	 virtual void WriteConfiguration() = 0;
	 virtual void SetExportOptions(DzFileIOSettings &ExportOptions) = 0;

	 // Need to temporarily rename surfaces if there is a name collision
	 void RenameDuplicateMaterials(DzNode* Node, QList<QString>& MaterialNames, QMap<DzMaterial*, QString>& OriginalMaterialNames);
	 void UndoRenameDuplicateMaterials(DzNode* Node, QList<QString>& MaterialNames, QMap<DzMaterial*, QString>& OriginalMaterialNames);
};