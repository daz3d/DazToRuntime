#pragma once
#include "dzbasicdialog.h"
#include <QtGui/qcombobox.h>
#include <QtCore/qsettings.h>

class QPushButton;
class QLineEdit;
class QCheckBox;
class QComboBox;
class QGroupBox;

class DzUnrealDialog : public DzBasicDialog {
	Q_OBJECT
public:

	/** Constructor **/
	 DzUnrealDialog(QWidget *parent);

	/** Destructor **/
	virtual ~DzUnrealDialog() {}

	QLineEdit* assetNameEdit;
	QLineEdit* projectEdit;
	QPushButton* projectButton;
	QComboBox* assetTypeCombo;
	QLineEdit* portEdit;
	QLineEdit* intermediateFolderEdit;
	QPushButton* intermediateFolderButton;
	QPushButton* morphsButton;
	QCheckBox* morphsEnabledCheckBox;
	QPushButton* subdivisionButton;
	QCheckBox* subdivisionEnabledCheckBox;
	QGroupBox* advancedSettingsGroupBox;
	QComboBox* fbxVersionCombo;
	QCheckBox* showFbxDialogCheckBox;
	QCheckBox* exportMaterialPropertyCSVCheckBox;

	// Pass so the DazTRoUnrealAction can access it from the morph dialog
	QString GetMorphString();

	// Pass so the DazTRoUnrealAction can access it from the morph dialog
	QMap<QString,QString> GetMorphMapping() { return morphMapping; }

	void Accepted();
private slots:
	void HandleSelectIntermediateFolderButton();
	void HandlePortChanged(const QString& port);
	void HandleChooseMorphsButton();
	void HandleMorphsCheckBoxChange(int state);
	void HandleChooseSubdivisionsButton();
	void HandleSubdivisionCheckBoxChange(int state);
	void HandleFBXVersionChange(const QString& fbxVersion);
	void HandleShowFbxDialogCheckBoxChange(int state);
	void HandleExportMaterialPropertyCSVCheckBoxChange(int state);

private:
	QSettings* settings;

	// These are clumsy leftovers from before the dialog were singletons
	QString morphString;
	QMap<QString,QString> morphMapping;
};
