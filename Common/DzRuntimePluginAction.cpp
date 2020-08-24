#include <dzapp.h>
#include <dzscene.h>
#include <dzexportmgr.h>
#include <dzexporter.h>
#include <dzmainwindow.h>
#include <dzmaterial.h>
#include <dzproperty.h>
#include <QtCore/qfile.h>
#include <QtCore/qtextstream.h>
#include <dzimageproperty.h>
#include <dzstringproperty.h>
#include <dznumericproperty.h>
#include <dzcolorproperty.h>
#include <dzstringproperty.h>
#include <dzenumproperty.h>
#include <dzboolproperty.h>
#include <dzobject.h>
#include <dzskeleton.h>
#include <dzfigure.h>
#include <dzshape.h>
#include <dzassetmgr.h>
#include <dzuri.h>
#include <dzcontentmgr.h>
#include <dzassetmetadata.h>
#include <dzbone.h>
#include <dzskeleton.h>
#include <dzpresentation.h>

#include <QtCore/qdir.h>
#include <QtGui/qlineedit.h>
#include <QtNetwork/qudpsocket.h>
#include <QtNetwork/qabstractsocket.h>
#include <QtGui/qcheckbox.h>


#include "DzRuntimePluginAction.h"

DzRuntimePluginAction::DzRuntimePluginAction(const QString& text, const QString& desc) :
	 DzAction(text, desc)
{
	 ExportMorphs = false;
	 ExportSubdivisions = false;
	 ShowFbxDialog = false;
}

DzRuntimePluginAction::~DzRuntimePluginAction()
{
}

void DzRuntimePluginAction::Export()
{
	 // FBX Export
	 Selection = dzScene->getPrimarySelection();
	 if (!Selection)
		  return;

	 DzExportMgr* ExportManager = dzApp->getExportMgr();
	 DzExporter* Exporter = ExportManager->findExporterByClassName("DzFbxExporter");

	 if (Exporter)
	 {
		  DzFileIOSettings ExportOptions;
		  ExportOptions.setBoolValue("doSelected", true);
		  ExportOptions.setBoolValue("doVisible", false);
		  if (AssetType == "SkeletalMesh" || AssetType == "StaticMesh")
		  {
				ExportOptions.setBoolValue("doFigures", true);
				ExportOptions.setBoolValue("doProps", true);
		  }
		  else
		  {
				ExportOptions.setBoolValue("doFigures", true);
				ExportOptions.setBoolValue("doProps", false);
		  }
		  ExportOptions.setBoolValue("doLights", false);
		  ExportOptions.setBoolValue("doCameras", false);
		  ExportOptions.setBoolValue("doAnims", true);
		  if (AssetType == "SkeletalMesh" && ExportMorphs && MorphString != "")
		  {
				ExportOptions.setBoolValue("doMorphs", true);
				ExportOptions.setStringValue("rules", MorphString);
		  }
		  else
		  {
				ExportOptions.setBoolValue("doMorphs", false);
				ExportOptions.setStringValue("rules", "");
		  }

		  ExportOptions.setStringValue("format", FBXVersion);
		  ExportOptions.setIntValue("RunSilent", !ShowFbxDialog);

		  ExportOptions.setBoolValue("doEmbed", false);
		  ExportOptions.setBoolValue("doCopyTextures", false);
		  ExportOptions.setBoolValue("doDiffuseOpacity", false);
		  ExportOptions.setBoolValue("doMergeClothing", true);
		  ExportOptions.setBoolValue("doStaticClothing", false);
		  ExportOptions.setBoolValue("degradedSkinning", true);
		  ExportOptions.setBoolValue("degradedScaling", true);
		  ExportOptions.setBoolValue("doSubD", false);
		  ExportOptions.setBoolValue("doCollapseUVTiles", false);

		  // get the top level node for things like clothing so we don't get dupe material names
		  DzNode* Parent = Selection;
		  while (Parent->getNodeParent() != NULL)
		  {
				Parent = Parent->getNodeParent();
		  }

		  // rename duplicate material names
		  QList<QString> MaterialNames;
		  QMap<DzMaterial*, QString> OriginalMaterialNames;
		  RenameDuplicateMaterials(Parent, MaterialNames, OriginalMaterialNames);

		  QDir dir;
		  dir.mkpath(CharacterFolder);

		  SetExportOptions(ExportOptions);
		  Exporter->writeFile(CharacterFBX, &ExportOptions);

		  WriteConfiguration();

		  // Change back material names
		  UndoRenameDuplicateMaterials(Parent, MaterialNames, OriginalMaterialNames);
	 }
}

// If there are duplicate material names, save off the original and rename one
void DzRuntimePluginAction::RenameDuplicateMaterials(DzNode* Node, QList<QString>& MaterialNames, QMap<DzMaterial*, QString>& OriginalMaterialNames)
{
	 DzObject* Object = Node->getObject();
	 DzShape* Shape = Object ? Object->getCurrentShape() : NULL;

	 if (Shape)
	 {
		  for (int i = 0; i < Shape->getNumMaterials(); i++)
		  {
				DzMaterial* Material = Shape->getMaterial(i);
				if (Material)
				{
					 OriginalMaterialNames.insert(Material, Material->getName());
					 while (MaterialNames.contains(Material->getName()))
					 {
						  Material->setName(Material->getName() + "_1");
					 }
					 MaterialNames.append(Material->getName());
				}
		  }
	 }
	 DzNodeListIterator Iterator = Node->nodeChildrenIterator();
	 while (Iterator.hasNext())
	 {
		  DzNode* Child = Iterator.next();
		  RenameDuplicateMaterials(Child, MaterialNames, OriginalMaterialNames);
	 }
}

// Restore the original material names
void DzRuntimePluginAction::UndoRenameDuplicateMaterials(DzNode* Node, QList<QString>& MaterialNames, QMap<DzMaterial*, QString>& OriginalMaterialNames)
{
	 QMap<DzMaterial*, QString>::iterator iter;
	 for (iter = OriginalMaterialNames.begin(); iter != OriginalMaterialNames.end(); ++iter)
	 {
		  iter.key()->setName(iter.value());
	 }
}

#include "moc_DzRuntimePluginAction.cpp"
