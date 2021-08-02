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

#include "DzUnrealDialog.h"
#include "DzUnrealMorphSelectionDialog.h"
#include "DzUnrealSubdivisionDialog.h"

/*****************************
Local definitions
*****************************/
#define DAZ_TO_UNREAL_PLUGIN_NAME "DazToUnreal"


DzUnrealDialog::DzUnrealDialog(QWidget *parent) :
	DzBasicDialog(parent, DAZ_TO_UNREAL_PLUGIN_NAME)
{
	 assetNameEdit = NULL;
	 projectEdit = NULL;
	 projectButton = NULL;
	 assetTypeCombo = NULL;
	 portEdit = NULL;
	 intermediateFolderEdit = NULL;
	 intermediateFolderButton = NULL;
	 morphsButton = NULL;
	 morphsEnabledCheckBox = NULL;
	 subdivisionButton = NULL;
	 subdivisionEnabledCheckBox = NULL;
	 advancedSettingsGroupBox = NULL;
	 fbxVersionCombo = NULL;
	 showFbxDialogCheckBox = NULL;

	settings = new QSettings("Daz 3D", "DazToUnreal");

	// Declarations
	int margin = style()->pixelMetric(DZ_PM_GeneralMargin);
	int wgtHeight = style()->pixelMetric(DZ_PM_ButtonHeight);
	int btnMinWidth = style()->pixelMetric(DZ_PM_ButtonMinWidth);

	// Set the dialog title
	setWindowTitle(tr("Daz To Unreal"));
	layout()->setSizeConstraint(QLayout::SetFixedSize);
	QFormLayout* mainLayout = new QFormLayout(this);


	advancedWidget = new QWidget();
	QHBoxLayout* advancedLayoutOuter = new QHBoxLayout(this);
	advancedLayoutOuter->addWidget(advancedWidget);
	QFormLayout* advancedLayout = new QFormLayout(this);
	advancedWidget->setLayout(advancedLayout);

	// Asset Name
	assetNameEdit = new QLineEdit(this);
	assetNameEdit->setValidator(new QRegExpValidator(QRegExp("[A-Za-z0-9_]*"), this));

	// Intermediate Folder
	QHBoxLayout* intermediateFolderLayout = new QHBoxLayout(this);
	intermediateFolderEdit = new QLineEdit(this);
	intermediateFolderButton = new QPushButton("...", this);
	connect(intermediateFolderButton, SIGNAL(released()), this, SLOT( HandleSelectIntermediateFolderButton() ));

	intermediateFolderLayout->addWidget(intermediateFolderEdit);
	intermediateFolderLayout->addWidget(intermediateFolderButton);

	// Asset Transfer Type
	assetTypeCombo = new QComboBox(this);
	assetTypeCombo->addItem("Skeletal Mesh");
	assetTypeCombo->addItem("Static Mesh");
	assetTypeCombo->addItem("Animation");
	assetTypeCombo->addItem("Environment");
	assetTypeCombo->addItem("Pose");
	assetTypeCombo->addItem("Material");

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

	// Ports
	portEdit = new QLineEdit("32345");
	connect(portEdit, SIGNAL(textChanged(const QString &)), this, SLOT(HandlePortChanged(const QString &)));

	// FBX Version
	fbxVersionCombo = new QComboBox(this);
	fbxVersionCombo->addItem("FBX 2014 -- Binary");
	fbxVersionCombo->addItem("FBX 2014 -- Ascii");
	fbxVersionCombo->addItem("FBX 2013 -- Binary");
	fbxVersionCombo->addItem("FBX 2013 -- Ascii");
	fbxVersionCombo->addItem("FBX 2012 -- Binary");
	fbxVersionCombo->addItem("FBX 2012 -- Ascii");
	fbxVersionCombo->addItem("FBX 2011 -- Binary");
	fbxVersionCombo->addItem("FBX 2011 -- Ascii");
	fbxVersionCombo->addItem("FBX 2010 -- Binary");
	fbxVersionCombo->addItem("FBX 2010 -- Ascii");
	fbxVersionCombo->addItem("FBX 2009 -- Binary");
	fbxVersionCombo->addItem("FBX 2009 -- Ascii");
	connect(fbxVersionCombo, SIGNAL(currentIndexChanged(const QString &)), this, SLOT(HandleFBXVersionChange(const QString &)));

	// Show FBX Dialog option
	showFbxDialogCheckBox = new QCheckBox("", this);
	connect(showFbxDialogCheckBox, SIGNAL(stateChanged(int)), this, SLOT(HandleShowFbxDialogCheckBoxChange(int)));
	
	// Export Material Property CSV option
	exportMaterialPropertyCSVCheckBox = new QCheckBox("", this);
	connect(exportMaterialPropertyCSVCheckBox, SIGNAL(stateChanged(int)), this, SLOT(HandleExportMaterialPropertyCSVCheckBoxChange(int)));

	// Add the widget to the basic dialog
	mainLayout->addRow("Asset Name", assetNameEdit);
	mainLayout->addRow("Asset Type", assetTypeCombo);
	mainLayout->addRow("Enable Morphs", morphsLayout);
	mainLayout->addRow("Enable Subdivision", subdivisionLayout);
	advancedLayout->addRow("Intermediate Folder", intermediateFolderLayout);
	advancedLayout->addRow("Port", portEdit);
	advancedLayout->addRow("FBX Version", fbxVersionCombo);
	advancedLayout->addRow("Show FBX Dialog", showFbxDialogCheckBox);
	advancedLayout->addRow("Export Material CSV", exportMaterialPropertyCSVCheckBox);
	addLayout(mainLayout);

	// Advanced
	advancedSettingsGroupBox = new QGroupBox("Advanced Settings", this);
	advancedSettingsGroupBox->setLayout(advancedLayoutOuter);
	advancedSettingsGroupBox->setCheckable(true);
	advancedSettingsGroupBox->setChecked(false);
	advancedSettingsGroupBox->setFixedWidth(500); // This is what forces the whole forms width
	addWidget(advancedSettingsGroupBox);
	advancedWidget->setHidden(true);
	connect(advancedSettingsGroupBox, SIGNAL(clicked(bool)), this, SLOT(HandleShowAdvancedSettingsCheckBoxChange(bool)));

	// Help
	assetNameEdit->setWhatsThis("This is the name the asset will use in Unreal.");
	assetTypeCombo->setWhatsThis("Skeletal Mesh for something with moving parts, like a character\nStatic Mesh for things like props\nAnimation for a character animation.");
	intermediateFolderEdit->setWhatsThis("DazToUnreal will collect the assets in a subfolder under this folder.  Unreal will import them from here.");
	intermediateFolderButton->setWhatsThis("DazToUnreal will collect the assets in a subfolder under this folder.  Unreal will import them from here.");
	portEdit->setWhatsThis("The UDP port used to talk to the DazToUnreal Unreal plugin.\nThis needs to match the port set in the Project Settings in Unreal.\nDefault is 32345.");
	fbxVersionCombo->setWhatsThis("The version of FBX to use when exporting assets.");
	showFbxDialogCheckBox->setWhatsThis("Checking this will show the FBX Dialog for adjustments before export.");
	exportMaterialPropertyCSVCheckBox->setWhatsThis("Checking this will write out a CSV of all the material properties.  Useful for reference when changing materials.");

	// Load Settings
	if (!settings->value("IntermediatePath").isNull())
	{
		intermediateFolderEdit->setText(settings->value("IntermediatePath").toString());
	}
	else
	{
		QString DefaultPath = QDesktopServices::storageLocation(QDesktopServices::DocumentsLocation) + QDir::separator() + "DazToUnreal";
		intermediateFolderEdit->setText(DefaultPath);
	}
	if (!settings->value("Port").isNull())
	{
		portEdit->setText(settings->value("Port").toString());
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
	if (!settings->value("ExportMaterialPropertyCSV").isNull())
	{
		exportMaterialPropertyCSVCheckBox->setChecked(settings->value("ExportMaterialPropertyCSV").toBool());
	}
	if (!settings->value("ShowAdvancedSettings").isNull())
	{
		advancedSettingsGroupBox->setChecked(settings->value("ShowAdvancedSettings").toBool());
		advancedWidget->setHidden(!advancedSettingsGroupBox->isChecked());
	}
	if (!settings->value("FBXExportVersion").isNull())
	{
		int index = fbxVersionCombo->findText(settings->value("FBXExportVersion").toString());
		if (index != -1)
		{
			fbxVersionCombo->setCurrentIndex(index);
		}
	}

	// Set Defaults
	DzNode* Selection = dzScene->getPrimarySelection();
	if (dzScene->getFilename().length() > 0)
	{
		QFileInfo fileInfo = QFileInfo(dzScene->getFilename());
		assetNameEdit->setText(fileInfo.baseName().remove(QRegExp("[^A-Za-z0-9_]")));
	}
	else if(dzScene->getPrimarySelection())
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

void DzUnrealDialog::HandleSelectIntermediateFolderButton()
{
	QString directoryName = QFileDialog::getExistingDirectory(this, tr("Choose Directory"),
		"/home",
		QFileDialog::ShowDirsOnly
		|QFileDialog::DontResolveSymlinks);

	if (directoryName != NULL)
	{
		intermediateFolderEdit->setText(directoryName);
		settings->setValue("IntermediatePath", directoryName);
	}
}

void DzUnrealDialog::HandlePortChanged(const QString& port)
{
	settings->setValue("Port", port);
}

void DzUnrealDialog::HandleChooseMorphsButton()
{
	DzUnrealMorphSelectionDialog *dlg = DzUnrealMorphSelectionDialog::Get(this);
	dlg->exec();
	morphString = dlg->GetMorphString();
	morphMapping = dlg->GetMorphRenaming();
}

void DzUnrealDialog::HandleChooseSubdivisionsButton()
{
	DzUnrealSubdivisionDialog *dlg = DzUnrealSubdivisionDialog::Get(this);
	dlg->exec();
}

QString DzUnrealDialog::GetMorphString()
{
	morphMapping = DzUnrealMorphSelectionDialog::Get(this)->GetMorphRenaming();
	return DzUnrealMorphSelectionDialog::Get(this)->GetMorphString();
}

void DzUnrealDialog::HandleMorphsCheckBoxChange(int state)
{
	settings->setValue("MorphsEnabled", state == Qt::Checked);
}

void DzUnrealDialog::HandleSubdivisionCheckBoxChange(int state)
{
	settings->setValue("SubdivisionEnabled", state == Qt::Checked);
}

void DzUnrealDialog::HandleFBXVersionChange(const QString& fbxVersion)
{
	settings->setValue("FBXExportVersion", fbxVersion);
}
void DzUnrealDialog::HandleShowFbxDialogCheckBoxChange(int state)
{
	 settings->setValue("ShowFBXDialog", state == Qt::Checked);
}
void DzUnrealDialog::HandleExportMaterialPropertyCSVCheckBoxChange(int state)
{
	settings->setValue("ExportMaterialPropertyCSV", state == Qt::Checked);
}

void DzUnrealDialog::HandleShowAdvancedSettingsCheckBoxChange(bool checked)
{
	settings->setValue("ShowAdvancedSettings", checked);
	advancedWidget->setHidden(!checked);
}
#include "moc_DzUnrealDialog.cpp"
