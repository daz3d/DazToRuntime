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


class DzUnrealSubdivisionDialog : public DzBasicDialog {
	Q_OBJECT
public:

	/** Constructor **/
	 DzUnrealSubdivisionDialog(QWidget *parent);

	void PrepareDialog();

	/** Destructor **/
	virtual ~DzUnrealSubdivisionDialog() {}

	static DzUnrealSubdivisionDialog* Get(QWidget* Parent)
	{
		if (singleton == nullptr)
		{
			singleton = new DzUnrealSubdivisionDialog(Parent);
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

	static DzUnrealSubdivisionDialog* singleton;
};
