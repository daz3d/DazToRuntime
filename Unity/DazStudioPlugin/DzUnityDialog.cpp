#include <QtGui/QLayout>
#include <QtGui/QLabel>
#include <QtGui/QGroupBox>
#include <QtGui/QPushButton>
#include <QtGui/QMessageBox>
#include <QtGui/QToolTip>
#include <QtGui/QWhatsThis>
#include <QtGui/qlineedit.h>
#include <QtGui/qboxlayout.h>
#include <QtGui/qfiledialog.h>
#include <QtCore/qsettings.h>
#include <QtGui/qformlayout.h>
#include <QtGui/qcombobox.h>
#include <QtGui/qdesktopservices.h>
#include <QtGui/qcheckbox.h>
#include <QtGui/qlistwidget.h>
#include <QtGui/qgroupbox.h>

#include "dzapp.h"
#include "dzscene.h"
#include "dzstyle.h"
#include "dzmainwindow.h"
#include "dzactionmgr.h"
#include "dzaction.h"
#include "dzskeleton.h"

#include "DzUnityDialog.h"
#include "DzUnityMorphSelectionDialog.h"
#include "DzUnitySubdivisionDialog.h"

/*****************************
Local definitions
*****************************/
#define DAZ_TO_UNITY_PLUGIN_NAME "DazToUnity"


DzUnityDialog::DzUnityDialog(QWidget* parent) :
	 DzBasicDialog(parent, DAZ_TO_UNITY_PLUGIN_NAME)
{
	 assetNameEdit = NULL;
	 projectEdit = NULL;
	 projectButton = NULL;
	 assetTypeCombo = NULL;
	 assetsFolderEdit = NULL;
	 assetsFolderButton = NULL;
	 morphsButton = NULL;
	 morphsEnabledCheckBox = NULL;
	 subdivisionButton = NULL;
	 subdivisionEnabledCheckBox = NULL;
	 advancedSettingsGroupBox = NULL;
	 showFbxDialogCheckBox = NULL;

	 settings = new QSettings("Daz 3D", "DazToUnity");

	 // Declarations
	 int margin = style()->pixelMetric(DZ_PM_GeneralMargin);
	 int wgtHeight = style()->pixelMetric(DZ_PM_ButtonHeight);
	 int btnMinWidth = style()->pixelMetric(DZ_PM_ButtonMinWidth);

	 // Set the dialog title
	 setWindowTitle(tr("Daz To Unity"));

	 QFormLayout* mainLayout = new QFormLayout(this);
	 QFormLayout* advancedLayout = new QFormLayout(this);

	 // Asset Name
	 assetNameEdit = new QLineEdit(this);
	 assetNameEdit->setValidator(new QRegExpValidator(QRegExp("[A-Za-z0-9_]*"), this));

	 // Intermediate Folder
	 QHBoxLayout* assetsFolderLayout = new QHBoxLayout(this);
	 assetsFolderEdit = new QLineEdit(this);
	 assetsFolderButton = new QPushButton("...", this);
	 connect(assetsFolderButton, SIGNAL(released()), this, SLOT(HandleSelectAssetsFolderButton()));

	 assetsFolderLayout->addWidget(assetsFolderEdit);
	 assetsFolderLayout->addWidget(assetsFolderButton);

	 // Asset Transfer Type
	 assetTypeCombo = new QComboBox(this);
	 assetTypeCombo->addItem("Skeletal Mesh");
	 assetTypeCombo->addItem("Static Mesh");
	 assetTypeCombo->addItem("Animation");
	 //assetTypeCombo->addItem("Pose");

	 // Morphs
	 QHBoxLayout* morphsLayout = new QHBoxLayout(this);
	 morphsButton = new QPushButton("Choose Morphs", this);
	 connect(morphsButton, SIGNAL(released()), this, SLOT(HandleChooseMorphsButton()));
	 morphsEnabledCheckBox = new QCheckBox("", this);
	 morphsEnabledCheckBox->setMaximumWidth(25);
	 morphsLayout->addWidget(morphsEnabledCheckBox);
	 morphsLayout->addWidget(morphsButton);
	 connect(morphsEnabledCheckBox, SIGNAL(stateChanged(int)), this, SLOT(HandleMorphsCheckBoxChange(int)));

	 // Subdivision
	 QHBoxLayout* subdivisionLayout = new QHBoxLayout(this);
	 subdivisionButton = new QPushButton("Choose Subdivisions", this);
	 connect(subdivisionButton, SIGNAL(released()), this, SLOT(HandleChooseSubdivisionsButton()));
	 subdivisionEnabledCheckBox = new QCheckBox("", this);
	 subdivisionEnabledCheckBox->setMaximumWidth(25);
	 subdivisionLayout->addWidget(subdivisionEnabledCheckBox);
	 subdivisionLayout->addWidget(subdivisionButton);
	 connect(subdivisionEnabledCheckBox, SIGNAL(stateChanged(int)), this, SLOT(HandleSubdivisionCheckBoxChange(int)));

	 // Show FBX Dialog option
	 showFbxDialogCheckBox = new QCheckBox("", this);

	 // Add the widget to the basic dialog
	 mainLayout->addRow("Asset Name", assetNameEdit);
	 mainLayout->addRow("Asset Type", assetTypeCombo);
	 mainLayout->addRow("Enable Morphs", morphsLayout);
	 mainLayout->addRow("Enable Subdivision", subdivisionLayout);
	 advancedLayout->addRow("Unity Assets Folder", assetsFolderLayout);
	 advancedLayout->addRow("Show FBX Dialog", showFbxDialogCheckBox);
	 addLayout(mainLayout);

	 // Advanced
	 advancedSettingsGroupBox = new QGroupBox("Advanced Settings", this);
	 advancedSettingsGroupBox->setLayout(advancedLayout);
	 addWidget(advancedSettingsGroupBox);


	 // Make the dialog fit its contents, with a minimum width, and lock it down
	 resize(QSize(500, 0).expandedTo(minimumSizeHint()));
	 setFixedWidth(width());
	 setFixedHeight(height());

	 // Help
	 assetNameEdit->setWhatsThis("This is the name the asset will use in Unity.");
	 assetTypeCombo->setWhatsThis("Skeletal Mesh for something with moving parts, like a character\nStatic Mesh for things like props\nAnimation for a character animation.");
	 assetsFolderEdit->setWhatsThis("Unity Assets folder. DazToUnity will collect the assets in a subfolder under this folder.");
	 assetsFolderButton->setWhatsThis("Unity Assets folder. DazToUnity will collect the assets in a subfolder under this folder.");

	 // Load Settings
	 if (!settings->value("AssetsPath").isNull())
	 {
		  assetsFolderEdit->setText(settings->value("AssetsPath").toString());
	 }
	 else
	 {
		  QString DefaultPath = QDesktopServices::storageLocation(QDesktopServices::DocumentsLocation) + QDir::separator() + "DazToUnity";
		  assetsFolderEdit->setText(DefaultPath);
	 }
	 if (!settings->value("MorphsEnabled").isNull())
	 {
		  morphsEnabledCheckBox->setChecked(settings->value("MorphsEnabled").toBool());
	 }
	 if (!settings->value("SubdivisionEnabled").isNull())
	 {
		  subdivisionEnabledCheckBox->setChecked(settings->value("SubdivisionEnabled").toBool());
	 }
	 if (!settings->value("ShowFBXDialog").isNull())
	 {
		  showFbxDialogCheckBox->setChecked(settings->value("ShowFBXDialog").toBool());
	 }

	 // Set Defaults
	 DzNode* Selection = dzScene->getPrimarySelection();
	 if (dzScene->getFilename().length() > 0)
	 {
		  QFileInfo fileInfo = QFileInfo(dzScene->getFilename());
		  assetNameEdit->setText(fileInfo.baseName().remove(QRegExp("[^A-Za-z0-9_]")));
	 }
	 else if (dzScene->getPrimarySelection())
	 {
		  assetNameEdit->setText(Selection->getLabel().remove(QRegExp("[^A-Za-z0-9_]")));
	 }

	 if (qobject_cast<DzSkeleton*>(Selection))
	 {
		  assetTypeCombo->setCurrentIndex(0);
	 }
	 else
	 {
		  assetTypeCombo->setCurrentIndex(1);
	 }
}

void DzUnityDialog::HandleSelectAssetsFolderButton()
{
	 QString directoryName = QFileDialog::getExistingDirectory(this, tr("Choose Directory"),
		  "/home",
		  QFileDialog::ShowDirsOnly
		  | QFileDialog::DontResolveSymlinks);

	 if (directoryName != NULL)
	 {
		  QDir parentDir = QFileInfo(directoryName).dir();
		  if (!parentDir.exists())
		  {
				QMessageBox::warning(0, tr("Error"), tr("Please select Unity Root Assets Folder."), QMessageBox::Ok);
				return;
		  }
		  else
		  {
				bool found1 = false;
				bool found2 = false;
				QFileInfoList list = parentDir.entryInfoList(QDir::NoDot | QDir::NoDotDot | QDir::Dirs);
				for (int i = 0; i < list.size(); i++)
				{
					 QFileInfo file = list[i];
					 if (file.baseName() == QString("ProjectSettings"))
						  found1 = true;
					 if (file.baseName() == QString("Library"))
						  found2 = true;
				}

				if (!found1 || !found2)
				{
					 QMessageBox::warning(0, tr("Error"), tr("Please select Unity Root Assets Folder."), QMessageBox::Ok);
					 return;
				}

				assetsFolderEdit->setText(directoryName);
				settings->setValue("AssetsPath", directoryName);
		  }
	 }
}

void DzUnityDialog::HandleChooseMorphsButton()
{
	 DzUnityMorphSelectionDialog* dlg = DzUnityMorphSelectionDialog::Get(this);
	 dlg->exec();
	 morphString = dlg->GetMorphString();
	 morphMapping = dlg->GetMorphRenaming();
}

void DzUnityDialog::HandleChooseSubdivisionsButton()
{
	 DzUnitySubdivisionDialog* dlg = DzUnitySubdivisionDialog::Get(this);
	 dlg->exec();
}

QString DzUnityDialog::GetMorphString()
{
	 morphMapping = DzUnityMorphSelectionDialog::Get(this)->GetMorphRenaming();
	 return DzUnityMorphSelectionDialog::Get(this)->GetMorphString();
}

void DzUnityDialog::HandleMorphsCheckBoxChange(int state)
{
	 settings->setValue("MorphsEnabled", state == Qt::Checked);
}

void DzUnityDialog::HandleSubdivisionCheckBoxChange(int state)
{
	 settings->setValue("SubdivisionEnabled", state == Qt::Checked);
}

void DzUnityDialog::HandleShowFbxDialogCheckBoxChange(int state)
{
	 settings->setValue("ShowFBXDialog", state == Qt::Checked);
}
#include "moc_DzUnityDialog.cpp"
