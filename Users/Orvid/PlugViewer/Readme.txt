
~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Debug Definitions ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

There are various debug definitions you can enable by defining the constants globally.

#define DebugTreeNodeLoading ~ Enables log messages from the tree node loading.
#define DebugErrors ~ Enables log messages from all errors.
#define DebugWarnings ~ Enables log messages from all warnings. 


The next 2 can be enabled by simply un-commenting the defines in TestRunner.cs,
rather than defining them globally.

#define DebugErrorLoading ~ Enables logging when loading Errors.
#define DebugWarningsLoading ~ Enables logging when loading Warnings.



~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Notes on Errors ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

When creating an error, make sure to set the icon of the method to the error icon.



~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Notes on Warnings ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

When creating a warning, make sure to first check that an error wasn't
already triggered, before setting the icon index to the warning icon.