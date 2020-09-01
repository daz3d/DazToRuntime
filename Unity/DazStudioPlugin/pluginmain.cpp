#include "dzplugin.h"
#include "dzapp.h"

#include "version.h"
#include "DzUnityAction.h"
#include "DzUnityDialog.h"

DZ_PLUGIN_DEFINITION("DazToUnity");

DZ_PLUGIN_AUTHOR("Daz 3D, Inc");

DZ_PLUGIN_VERSION(PLUGIN_MAJOR, PLUGIN_MINOR, PLUGIN_REV, PLUGIN_BUILD);

DZ_PLUGIN_DESCRIPTION(QString(
	"<a href=\"%1/aDazToUnity/index.htm\">Documentation</a><br><br>"
).arg(dzApp->getDocumentationPath()));

DZ_PLUGIN_CLASS_GUID(DzUnityAction, 2C2AA695-652C-4FA9-BE48-E0AB954E28AB);