#pragma once

#define VERSION_MAJOR 1
#define VERSION_MINOR 0
#define VERSION_BUILD 3

#define _STR2(X) #X
#define _STR(X) _STR2(X)
#define VERSION_STR _STR(VERSION_MAJOR) "." _STR(VERSION_MINOR) "." _STR(VERSION_BUILD)
