#pragma once

BEGIN_NAMESPACE

public ref class ProcessLaunchInfo sealed
{
public:
	ProcessLaunchInfo(
		String^ exe,
		String^ commandLine, 
		String^ dir, 
		String^ environment, 
		String^ options, 
		DWORD launchFlags, 
		DWORD stdInput, 
		DWORD stdOutput, 
		DWORD stdError
		) :
		Exe(exe),
		CommandLine(commandLine), 
		Dir(dir), 
		Environment(environment), 
		Options(options), 
		LaunchFlags(launchFlags), 
		StdInput(stdInput), 
		StdOutput(stdOutput),
		StdError(stdError)
	{
	}

	initonly String^ Exe;
	initonly String^ CommandLine; 
	initonly String^ Dir; 
	initonly String^ Environment; 
	initonly String^ Options; 
	initonly DWORD LaunchFlags; 
	initonly DWORD StdInput; 
	initonly DWORD StdOutput; 
	initonly DWORD StdError;
};

END_NAMESPACE
