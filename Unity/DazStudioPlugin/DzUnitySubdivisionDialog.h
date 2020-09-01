#pragma once
#include "dzbasicdialog.h"
#include <QtGui/qlineedit.h>
#include <QtGui/qcombobox.h>
#include <QtCore/qsettings.h>
#include "dznode.h"
#include <dzjsonwriter.h>

class QListWidget;
class QListWidgetItem;
class QTreeWidget;
class QTreeWidgetItem;
class QLineEdit;
class QComboBox;
class QGridLayout;


class DzUnitySubdivisionDialog : public DzBasicDialog {
	Q_OBJECT
public:

	/** Constructor **/
	 DzUnitySubdivisionDialog(QWidget *parent);

	void PrepareDialog();

	/** Destructor **/
	virtual ~DzUnitySubdivisionDialog() {}

	static DzUnitySubdivisionDialog* Get(QWidget* Parent)
	{
		if (singleton == nullptr)
		{
			singleton = new DzUnitySubdivisionDialog(Parent);
		}
		singleton->PrepareDialog();
		return singleton;
	}

	QGridLayout* subdivisionItemsGrid;

	void LockSubdivisionProperties(bool subdivisionEnabled);
	void WriteSubdivisions(DzJsonWriter& Writer);
	DzNode* FindObject(DzNode* Node, QString Name);

public slots:
	void HandleSubdivisionLevelChanged(const QString& text);

private:
	void CreateList(DzNode* Node);

	void SavePresetFile(QString filePath);

	QSize minimumSizeHint() const override;

	QString presetsFolder;

	QList<QComboBox*> SubdivisionCombos;

	QMap<QString, int> SubdivisionLevels;

	static DzUnitySubdivisionDialog* singleton;
};
