#include <QtGui/QLayout>
#include <QtGui/QLabel>
#include <QtGui/QGroupBox>
#include <QtGui/QPushButton>
#include <QtGui/QToolTip>
#include <QtGui/QWhatsThis>
#include <QtGui/qlineedit.h>
#include <QtGui/qboxlayout.h>
#include <QtGui/qfiledialog.h>
#include <QtCore/qsettings.h>
#include <QtGui/qformlayout.h>
#include <QtGui/qcombobox.h>
#include <QtGui/qdesktopservices.h>
#include <QtGui/qlistwidget.h>
#include <QtGui/qtreewidget.h>
#include <QtGui/qcheckbox.h>
#include <QtGui/qdesktopservices.h>
#include <QtCore/qdiriterator.h>

#include "dzapp.h"
#include "dzscene.h"
#include "dzstyle.h"
#include "dzmainwindow.h"
#include "dzactionmgr.h"
#include "dzaction.h"
#include "dzskeleton.h"
#include "dzobject.h"
#include "dzshape.h"
#include "dzmodifier.h"
#include "dzpresentation.h"
#include "dzassetmgr.h"
#include "dzproperty.h"
#include "dzsettings.h"
#include "dzmorph.h"
#include "DzUnrealMorphSelectionDialog.h"

#include "QtGui/qlayout.h"
#include "QtGui/qlineedit.h"

#include <QtCore/QDebug.h>

/*****************************
Local definitions
*****************************/
#define DAZ_TO_UNREAL_PLUGIN_NAME		"DazToUnreal"


DzUnrealMorphSelectionDialog* DzUnrealMorphSelectionDialog::singleton = nullptr;

// For sorting the lists
class SortingListItem : public QListWidgetItem {

public:
	virtual bool operator< (const QListWidgetItem &otherItem) const
	{
		if (this->checkState() != otherItem.checkState())
		{
			return (this->checkState() == Qt::Checked);
		}
		return QListWidgetItem::operator<(otherItem);
	}
};

DzUnrealMorphSelectionDialog::DzUnrealMorphSelectionDialog(QWidget *parent) :
	DzBasicDialog(parent, DAZ_TO_UNREAL_PLUGIN_NAME)
{
	 morphListWidget = NULL;
	 morphExportListWidget = NULL;
	 morphTreeWidget = NULL;
	 filterEdit = NULL;
	 presetCombo = NULL;
	 fullBodyMorphTreeItem = NULL;
	 charactersTreeItem = NULL;

	// Set the dialog title 
	setWindowTitle(tr("Select Morphs"));

	// Setup folder
	presetsFolder = QDesktopServices::storageLocation(QDesktopServices::DocumentsLocation) + QDir::separator() + "DazToUnreal" + QDir::separator() + "Presets";


	QVBoxLayout* mainLayout = new QVBoxLayout(this);

	// Left tree with morph structure
	morphTreeWidget = new QTreeWidget(this);
	morphTreeWidget->setHeaderHidden(true);

	// Center list showing morhps for selected tree items
	morphListWidget = new QListWidget(this);
	morphListWidget->setSelectionMode(QAbstractItemView::ExtendedSelection);

	// Right list showing morphs that will export
	morphExportListWidget = new QListWidget(this);
	morphExportListWidget->setSelectionMode(QAbstractItemView::ExtendedSelection);

	// Quick filter box
	QHBoxLayout* filterLayout = new QHBoxLayout(this);
	filterLayout->addWidget(new QLabel("filter"));
	filterEdit = new QLineEdit();
	connect(filterEdit, SIGNAL(textChanged(const QString &)), this, SLOT(FilterChanged(const QString &)));
	filterLayout->addWidget(filterEdit);

	// Presets
	QHBoxLayout* settingsLayout = new QHBoxLayout(this);
	presetCombo = new QComboBox(this);
	QPushButton* savePresetButton = new QPushButton("Save Preset", this);
	connect(savePresetButton, SIGNAL(released()), this, SLOT(HandleSavePreset()));
	settingsLayout->addWidget(new QLabel("Choose Preset"));
	settingsLayout->addWidget(presetCombo);
	settingsLayout->addWidget(savePresetButton);
	settingsLayout->addStretch();

	// All Morphs
	QHBoxLayout* morphsLayout = new QHBoxLayout(this);

	// Left Tree
	QVBoxLayout* treeLayout = new QVBoxLayout(this);
	treeLayout->addWidget(new QLabel("Morph Groups"));
	treeLayout->addWidget(new QLabel("Select to see available morphs"));
	treeLayout->addWidget(morphTreeWidget);

	// Buttons for quickly adding certain JCMs
	QGroupBox* JCMGroupBox = new QGroupBox("Add JCMs", this);
	JCMGroupBox->setLayout(new QGridLayout(this));
	QPushButton* ArmsJCMButton = new QPushButton("Arms");
	QPushButton* LegsJCMButton = new QPushButton("Legs");
	QPushButton* TorsoJCMButton = new QPushButton("Torso");
	((QGridLayout*)JCMGroupBox->layout())->addWidget(ArmsJCMButton, 0, 0);
	((QGridLayout*)JCMGroupBox->layout())->addWidget(LegsJCMButton, 0, 1);
	((QGridLayout*)JCMGroupBox->layout())->addWidget(TorsoJCMButton, 0, 2);

	connect(ArmsJCMButton, SIGNAL(released()), this, SLOT(HandleArmJCMMorphsButton()));
	connect(LegsJCMButton, SIGNAL(released()), this, SLOT(HandleLegJCMMorphsButton()));
	connect(TorsoJCMButton, SIGNAL(released()), this, SLOT(HandleTorsoJCMMorphsButton()));

	treeLayout->addWidget(JCMGroupBox);
	morphsLayout->addLayout(treeLayout);


	// Center List of morphs based on tree selection
	QVBoxLayout* morphListLayout = new QVBoxLayout(this);
	morphListLayout->addWidget(new QLabel("Morphs in Group"));
	morphListLayout->addWidget(new QLabel("Select and click Add for Export"));
	morphListLayout->addLayout(filterLayout);
	morphListLayout->addWidget(morphListWidget);

	// Button for adding morphs
	QPushButton* addMorphsButton = new QPushButton("Add For Export", this);
	connect(addMorphsButton, SIGNAL(released()), this, SLOT(HandleAddMorphsButton()));
	morphListLayout->addWidget(addMorphsButton);
	morphsLayout->addLayout(morphListLayout);

	// Right List of morphs that will export
	QVBoxLayout* selectedListLayout = new QVBoxLayout(this);
	selectedListLayout->addWidget(new QLabel("Morphs to Export"));
	selectedListLayout->addWidget(morphExportListWidget);

	// Button for clearing morphs from export
	QPushButton* removeMorphsButton = new QPushButton("Remove From Export", this);
	connect(removeMorphsButton, SIGNAL(released()), this, SLOT(HandleRemoveMorphsButton()));
	selectedListLayout->addWidget(removeMorphsButton);
	morphsLayout->addLayout(selectedListLayout);

	mainLayout->addLayout(settingsLayout);
	mainLayout->addLayout(morphsLayout);

	this->addLayout(mainLayout);
	resize(QSize(800, 800));//.expandedTo(minimumSizeHint()));
	setFixedWidth(width());
	setFixedHeight(height());
	RefreshPresetsCombo();

	connect(morphListWidget, SIGNAL(itemChanged(QListWidgetItem*)),
		this, SLOT(ItemChanged(QListWidgetItem*)));

	connect(morphTreeWidget, SIGNAL(itemSelectionChanged()),
		this, SLOT(ItemSelectionChanged()));

}

QSize DzUnrealMorphSelectionDialog::minimumSizeHint() const
{
	return QSize(800, 800);
}

// Build out the Left morphs tree based on the current selection
void DzUnrealMorphSelectionDialog::PrepareDialog()
{
	DzNode* Selection = dzScene->getPrimarySelection();
	morphs.clear();
	morphList = GetAvailableMorphs(Selection);
	for (int ChildIndex = 0; ChildIndex < Selection->getNumNodeChildren(); ChildIndex++)
	{
		DzNode* ChildNode = Selection->getNodeChild(ChildIndex);
		morphList.append(GetAvailableMorphs(ChildNode));
	}
	UpdateMorphsTree();
	HandlePresetChanged("LastUsed.csv");
}

// When the filter text is changed, update the center list
void DzUnrealMorphSelectionDialog::FilterChanged(const QString& filter)
{
	morphListWidget->clear();
	QString newFilter = filter;
	morphListWidget->clear();
	foreach(MorphInfo morphInfo, selectedInTree)
	{
		if (newFilter == NULL || newFilter.isEmpty() || morphInfo.Label.contains(newFilter, Qt::CaseInsensitive))
		{
			SortingListItem* item = new SortingListItem();// modLabel, morphListWidget);
			item->setText(morphInfo.Label);
			item->setData(Qt::UserRole, morphInfo.Name);

			morphListWidget->addItem(item);
		}
	}

	morphListWidget->sortItems();
}

// Build a list of availaboe morphs for the node
// TODO: This function evolved a lot as I figured out where to find the morphs.
// There may be dead code in here.
QStringList DzUnrealMorphSelectionDialog::GetAvailableMorphs(DzNode* Node)
{
	QStringList newMorphList;

	DzObject* Object = Node->getObject();
	DzShape* Shape = Object ? Object->getCurrentShape() : NULL;

	for (int index = 0; index < Node->getNumProperties(); index++)
	{
		DzProperty* property = Node->getProperty(index);
		QString propName = property->getName();
		QString propLabel = property->getLabel();
		DzPresentation* presentation = property->getPresentation();
		if (presentation)
		{
			MorphInfo morphInfo;
			morphInfo.Name = propName;
			morphInfo.Label = propLabel;
			morphInfo.Path = Node->getLabel() + "/" + property->getPath();
			morphInfo.Type = presentation->getType();
			if (!morphs.contains(morphInfo.Name))
			{
				morphs.insert(morphInfo.Name, morphInfo);
			}
			//qDebug() << "Property Name " << propName << " Label " << propLabel << " Presentation Type:" << presentation->getType() << "Path: " << property->getPath();
			//qDebug() << "Path " << property->getGroupOnlyPath();
		}
		if (presentation && presentation->getType() == "Modifier/Shape")
		{
			SortingListItem* item = new SortingListItem();// modLabel, morphListWidget);
			item->setText(propLabel);
			item->setFlags(item->flags() | Qt::ItemIsUserCheckable);
			if (morphList.contains(propLabel))
			{
				item->setCheckState(Qt::Checked);
				newMorphList.append(propName);
			}
			else
			{
				item->setCheckState(Qt::Unchecked);
			}
			item->setData(Qt::UserRole, propName);
			morphNameMapping.insert(propName, propLabel);
		}
	}

	if (Object)
	{
		for (int index = 0; index < Object->getNumModifiers(); index++)
		{
			DzModifier* modifier = Object->getModifier(index);
			QString modName = modifier->getName();
			QString modLabel = modifier->getLabel();
			DzMorph* mod = qobject_cast<DzMorph*>(modifier);
			if (mod)
			{
				for (int propindex = 0; propindex < modifier->getNumProperties(); propindex++)
				{
					DzProperty* property = modifier->getProperty(propindex);
					QString propName = property->getName();
					QString propLabel = property->getLabel();
					DzPresentation* presentation = property->getPresentation();
					if (presentation)
					{
						MorphInfo morphInfoProp;
						morphInfoProp.Name = modName;
						morphInfoProp.Label = propLabel;
						morphInfoProp.Path = Node->getLabel() + "/" + property->getPath();
						morphInfoProp.Type = presentation->getType();
						if (!morphs.contains(morphInfoProp.Name))
						{
							morphs.insert(morphInfoProp.Name, morphInfoProp);
						}
						//qDebug() << "Modifier Name " << modName << " Label " << propLabel << " Presentation Type:" << presentation->getType() << " Path: " << property->getPath();
						//qDebug() << "Path " << property->getGroupOnlyPath();
					}
				}
				
			}

		}
	}
	
	return newMorphList;
}

// Build out the left tree
void DzUnrealMorphSelectionDialog::UpdateMorphsTree()
{
	morphTreeWidget->clear();
	morphsForNode.clear();
	foreach(QString morph, morphs.keys())
	{
		QString path = morphs[morph].Path;
		QTreeWidgetItem* parentItem = nullptr;
		foreach(QString pathPart, path.split("/"))
		{
			if (pathPart == "") continue;
			parentItem = FindTreeItem(parentItem, pathPart);

			if (!morphsForNode.keys().contains(parentItem))
			{
				morphsForNode.insert(parentItem, QList<MorphInfo>());
			}
			morphsForNode[parentItem].append(morphs[morph]);
		}
	}
}

// This function could be better named.  It will find the node matching the property path
// but it will also create the structure of that path in the tree as needed as it searches
QTreeWidgetItem* DzUnrealMorphSelectionDialog::FindTreeItem(QTreeWidgetItem* parent, QString name)
{
	if (parent == nullptr)
	{
		for(int i = 0; i < morphTreeWidget->topLevelItemCount(); i++)
		{
			QTreeWidgetItem* item = morphTreeWidget->topLevelItem(i);
			if (item->text(0) == name)
			{
				return item;
			}
		}

		QTreeWidgetItem* newItem = new QTreeWidgetItem(morphTreeWidget);
		newItem->setText(0, name);
		newItem->setExpanded(true);
		morphTreeWidget->addTopLevelItem(newItem);
		return newItem;
	}
	else
	{
		for (int i = 0; i < parent->childCount(); i++)
		{
			QTreeWidgetItem* item = parent->child(i);
			if (item->text(0) == name)
			{
				return item;
			}
		}

		QTreeWidgetItem* newItem = new QTreeWidgetItem(parent);
		newItem->setText(0, name);
		newItem->setExpanded(true);
		parent->addChild(newItem);
		return newItem;
	}
}

// For selection changes in the Left Tree
void DzUnrealMorphSelectionDialog::ItemSelectionChanged()
{
	selectedInTree.clear();
	foreach(QTreeWidgetItem* selectedItem, morphTreeWidget->selectedItems())
	{
		SelectMorphsInNode(selectedItem);
	}

	FilterChanged(filterEdit->text());
}

// Updates the list of selected morphs in the Left Tree
// including any children
void DzUnrealMorphSelectionDialog::SelectMorphsInNode(QTreeWidgetItem* item)
{
	if (morphsForNode.keys().contains(item))
	{
		selectedInTree.append(morphsForNode[item]);
	}
}

// Add Morphs for export
void DzUnrealMorphSelectionDialog::HandleAddMorphsButton()
{
	foreach(QListWidgetItem* selectedItem, morphListWidget->selectedItems())
	{
		QString morphName = selectedItem->data(Qt::UserRole).toString();
		if (morphs.contains(morphName) && !morphsToExport.contains(morphs[morphName]))
		{
			morphsToExport.append(morphs[morphName]);
		}
	}
	RefreshExportMorphList();
	RefreshPresetsCombo();
}

// Remove morph from export list
void DzUnrealMorphSelectionDialog::HandleRemoveMorphsButton()
{
	foreach(QListWidgetItem* selectedItem, morphExportListWidget->selectedItems())
	{
		QString morphName = selectedItem->data(Qt::UserRole).toString();
		if (morphs.keys().contains(morphName))
		{
			morphsToExport.removeAll(morphs[morphName]);
		}
	}
	RefreshExportMorphList();
	RefreshPresetsCombo();
}

// Brings up a dialgo for choosing a preset name
void DzUnrealMorphSelectionDialog::HandleSavePreset()
{
	QString filters("CSV Files (*.csv)");
	QString defaultFilter("CSV Files (*.csv)");
	QDir dir;
	dir.mkpath(presetsFolder);

	QString presetName = QFileDialog::getSaveFileName(this, QString("Save Preset"),
		presetsFolder,
		filters,
		&defaultFilter);

	if (presetName != NULL)
	{
		SavePresetFile(presetName);
	}
}

// Saves out a preset.  If the path isn't supplied, it's saved as the last selection
void DzUnrealMorphSelectionDialog::SavePresetFile(QString filePath)
{
	QDir dir;
	dir.mkpath(presetsFolder);
	if (filePath == NULL)
	{
		filePath = presetsFolder + QDir::separator() + "LastUsed.csv";
	}

	QFile file(filePath);
	file.open(QIODevice::WriteOnly | QIODevice::Text);
	QTextStream out(&file);
	out << GetMorphCSVString();

	// optional, as QFile destructor will already do it:
	file.close();
	RefreshPresetsCombo();

}

// Hard coded list of morphs for Genesis 3 and 8
// It just adds them all, the other functions will ignore any that don't fit the character
void DzUnrealMorphSelectionDialog::HandleArmJCMMorphsButton()
{
	QStringList MorphsToAdd;

	MorphsToAdd.append("pJCMCollarTwist_n30_L");
	MorphsToAdd.append("pJCMCollarTwist_n30_R");
	MorphsToAdd.append("pJCMCollarTwist_p30_L");
	MorphsToAdd.append("pJCMCollarTwist_p30_R");
	MorphsToAdd.append("pJCMCollarUp_55_L");
	MorphsToAdd.append("pJCMCollarUp_55_R");
	MorphsToAdd.append("pJCMCollarUp_50_L");
	MorphsToAdd.append("pJCMCollarUp_50_R");
	MorphsToAdd.append("pJCMForeArmFwd_135_L");
	MorphsToAdd.append("pJCMForeArmFwd_135_R");
	MorphsToAdd.append("pJCMForeArmFwd_75_L");
	MorphsToAdd.append("pJCMForeArmFwd_75_R");
	MorphsToAdd.append("pJCMHandDwn_70_L");
	MorphsToAdd.append("pJCMHandDwn_70_R");
	MorphsToAdd.append("pJCMHandUp_80_L");
	MorphsToAdd.append("pJCMHandUp_80_R");
	MorphsToAdd.append("pJCMShldrDown_40_L");
	MorphsToAdd.append("pJCMShldrDown_40_R");
	MorphsToAdd.append("pJCMShldrDown_75_L");
	MorphsToAdd.append("pJCMShldrDown_75_R");
	MorphsToAdd.append("pJCMShldrDown2_75_L");
	MorphsToAdd.append("pJCMShldrDown2_75_R");
	MorphsToAdd.append("pJCMShldrFront_n110_Bend_n40_L");
	MorphsToAdd.append("pJCMShldrFront_n110_Bend_p90_L");
	MorphsToAdd.append("pJCMShldrFront_p110_Bend_n90_R");
	MorphsToAdd.append("pJCMShldrFront_p110_Bend_p40_R");
	MorphsToAdd.append("pJCMShldrFwdDwn_110_75_L");
	MorphsToAdd.append("pJCMShldrFwdDwn_110_75_R");
	MorphsToAdd.append("pJCMShldrFwd_110_L");
	MorphsToAdd.append("pJCMShldrFwd_110_R");
	MorphsToAdd.append("pJCMShldrFwd_95_L");
	MorphsToAdd.append("pJCMShldrFwd_95_R");
	MorphsToAdd.append("pJCMShldrUp_90_L");
	MorphsToAdd.append("pJCMShldrUp_90_R");
	MorphsToAdd.append("pJCMShldrUp_35_L");
	MorphsToAdd.append("pJCMShldrUp_35_R");

	// Add the list for export
	foreach(QString MorphName, MorphsToAdd)
	{
		if (morphs.contains(MorphName) && !morphsToExport.contains(morphs[MorphName]))
		{
			morphsToExport.append(morphs[MorphName]);
		}
	}
	RefreshExportMorphList();
}

// Hard coded list of morphs for Genesis 3 and 8
// It just adds them all, the other functions will ignore any that don't fit the character
void DzUnrealMorphSelectionDialog::HandleLegJCMMorphsButton()
{
	QStringList MorphsToAdd;

	MorphsToAdd.append("pJCMBigToeDown_45_L");
	MorphsToAdd.append("pJCMBigToeDown_45_R");
	MorphsToAdd.append("pJCMFootDwn_75_L");
	MorphsToAdd.append("pJCMFootDwn_75_R");
	MorphsToAdd.append("pJCMFootUp_40_L");
	MorphsToAdd.append("pJCMFootUp_40_R");
	MorphsToAdd.append("pJCMShinBend_155_L");
	MorphsToAdd.append("pJCMShinBend_155_R");
	MorphsToAdd.append("pJCMShinBend_90_L");
	MorphsToAdd.append("pJCMShinBend_90_R");
	MorphsToAdd.append("pJCMThighBack_35_L");
	MorphsToAdd.append("pJCMThighBack_35_R");
	MorphsToAdd.append("pJCMThighFwd_115_L");
	MorphsToAdd.append("pJCMThighFwd_115_R");
	MorphsToAdd.append("pJCMThighFwd_57_L");
	MorphsToAdd.append("pJCMThighFwd_57_R");
	MorphsToAdd.append("pJCMThighSide_85_L");
	MorphsToAdd.append("pJCMThighSide_85_R");
	MorphsToAdd.append("pJCMToesUp_60_L");
	MorphsToAdd.append("pJCMToesUp_60_R");

	// Add the list for export
	foreach(QString MorphName, MorphsToAdd)
	{
		if (morphs.contains(MorphName) && !morphsToExport.contains(morphs[MorphName]))
		{
			morphsToExport.append(morphs[MorphName]);
		}
	}
	RefreshExportMorphList();
}

// Hard coded list of morphs for Genesis 3 and 8
// It just adds them all, the other functions will ignore any that don't fit the character
void DzUnrealMorphSelectionDialog::HandleTorsoJCMMorphsButton()
{
	QStringList MorphsToAdd;

	MorphsToAdd.append("pJCMAbdomen2Fwd_40");
	MorphsToAdd.append("pJCMAbdomen2Side_24_L");
	MorphsToAdd.append("pJCMAbdomen2Side_24_R");
	MorphsToAdd.append("pJCMAbdomenFwd_35");
	MorphsToAdd.append("pJCMAbdomenLowerFwd_Navel");
	MorphsToAdd.append("pJCMAbdomenUpperFwd_Navel");
	MorphsToAdd.append("pJCMHeadBack_27");
	MorphsToAdd.append("pJCMHeadFwd_25");
	MorphsToAdd.append("pJCMNeckBack_27");
	MorphsToAdd.append("pJCMNeckFwd_35");
	MorphsToAdd.append("pJCMNeckLowerSide_40_L");
	MorphsToAdd.append("pJCMNeckLowerSide_40_R");
	MorphsToAdd.append("pJCMNeckTwist_22_L");
	MorphsToAdd.append("pJCMNeckTwist_22_R");
	MorphsToAdd.append("pJCMNeckTwist_Reverse");
	MorphsToAdd.append("pJCMPelvisFwd_25");
	MorphsToAdd.append("pJCMChestFwd_35");
	MorphsToAdd.append("pJCMChestSide_20_L");
	MorphsToAdd.append("pJCMChestSide_20_R");

	// Add the list for export
	foreach(QString MorphName, MorphsToAdd)
	{
		if (morphs.contains(MorphName) && !morphsToExport.contains(morphs[MorphName]))
		{
			morphsToExport.append(morphs[MorphName]);
		}
	}
	RefreshExportMorphList();
}

// Refresh the Right export list
void DzUnrealMorphSelectionDialog::RefreshExportMorphList()
{
	morphExportListWidget->clear();
	foreach(MorphInfo morphInfo, morphsToExport)
	{
		SortingListItem* item = new SortingListItem();
		item->setText(morphInfo.Label);
		item->setData(Qt::UserRole, morphInfo.Name);

		morphExportListWidget->addItem(item);
	}
	SavePresetFile(NULL);
}

// Refresh the list of preset csvs from the files in the folder
void DzUnrealMorphSelectionDialog::RefreshPresetsCombo()
{
	disconnect(presetCombo, SIGNAL(currentIndexChanged(const QString &)), this, SLOT(HandlePresetChanged(const QString &)));

	presetCombo->clear();
	presetCombo->addItem("None");

	QDirIterator it(presetsFolder, QStringList() << "*.csv", QDir::NoFilter, QDirIterator::NoIteratorFlags);
	while (it.hasNext()) 
	{
		QString Path = it.next();
		QString NewPath = Path.right(Path.length() - presetsFolder.length() - 1);
		presetCombo->addItem(NewPath);
	}
	connect(presetCombo, SIGNAL(currentIndexChanged(const QString &)), this, SLOT(HandlePresetChanged(const QString &)));
}

// Call when the preset combo is changed by the user
void DzUnrealMorphSelectionDialog::HandlePresetChanged(const QString& presetName)
{
	morphsToExport.clear();
	QString PresetFilePath = presetsFolder + QDir::separator() + presetName;

	QFile file(PresetFilePath);
	if (!file.open(QIODevice::ReadOnly)) {
		// TODO: should be an error dialog
		return;
	}

	// load the selected csv from disk into the export list on the right
	QTextStream InStream(&file);

	while (!InStream.atEnd()) {
		QString MorphLine = InStream.readLine();
		if (MorphLine.endsWith("\"Export\""))
		{
			QStringList Items = MorphLine.split(",");
			QString MorphName = Items[0].replace("\"", "");
			if (morphs.contains(MorphName))
			{
				morphsToExport.append(morphs[MorphName]);
			}
		}
	}

	RefreshExportMorphList();
	file.close();
}

// Get the morph string in the format for the Daz FBX Export
QString DzUnrealMorphSelectionDialog::GetMorphString()
{
	if (morphsToExport.length() == 0)
	{
		return "";
	}
	QStringList morphNamesToExport;
	foreach(MorphInfo exportMorph, morphsToExport)
	{
		morphNamesToExport.append(exportMorph.Name);
	}
	QString morphString = morphNamesToExport.join("\n1\n");
	morphString += "\n1\n.CTRLVS\n2\nAnything\n0";
	return morphString;
}

// Get the morph string in the format used for presets
QString DzUnrealMorphSelectionDialog::GetMorphCSVString()
{
	morphList.clear();
	QString morphString;
	foreach(MorphInfo exportMorph, morphsToExport)
	{
		morphList.append(exportMorph.Name);
		morphString += "\"" + exportMorph.Name + ",\"Export\"\n";
	}
	morphString += "\".CTRLVS\", \"Ignore\"\n";
	morphString += "\"Anything\", \"Bake\"\n";
	return morphString;
}

// Get the morph string in an internal name = friendly name format
// Used to rename them to the friendly name in Unreal
QMap<QString,QString> DzUnrealMorphSelectionDialog::GetMorphRenaming()
{
	morphNameMapping.clear();
	foreach(MorphInfo exportMorph, morphsToExport)
	{
		morphNameMapping.insert(exportMorph.Name, exportMorph.Label);
	}

	return morphNameMapping;
}

#include "moc_DzUnrealMorphSelectionDialog.cpp"
