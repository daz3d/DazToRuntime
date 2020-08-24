#include "dzplugin.h"
#include "dzapp.h"

#include "version.h"
#include "DzUnrealAction.h"
#include "DzUnrealDialog.h"

DZ_PLUGIN_DEFINITION("DazToUnreal");

DZ_PLUGIN_AUTHOR("Daz 3D, Inc");

DZ_PLUGIN_VERSION(PLUGIN_MAJOR, PLUGIN_MINOR, PLUGIN_REV, PLUGIN_BUILD);

DZ_PLUGIN_DESCRIPTION(QString(
	"<a href=\"%1/aDazToUnreal/index.htm\">Documentation</a><br><br>"
).arg(dzApp->getDocumentationPath()));

DZ_PLUGIN_CLASS_GUID(DzUnrealAction, 569D1102-15DE-44EA-8C8B-A1506297DEC3);