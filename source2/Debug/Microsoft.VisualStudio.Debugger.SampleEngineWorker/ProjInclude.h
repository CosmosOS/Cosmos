#pragma once

using namespace System;
using namespace System::Diagnostics;
#include <vcclr.h>
#pragma warning(push)
#pragma warning(disable:4091)
#include <msclr\lock.h>
#pragma warning(pop)

#define ASSERT(cond) _ASSERT(cond)
#ifdef DEBUG
	#define VERIFY(statement) Debug::Assert((statement) != 0, #statement)
#else
	#define VERIFY(statement) statement
#endif

#include "ComponentException.h"
#include "WorkerUtil.h"
