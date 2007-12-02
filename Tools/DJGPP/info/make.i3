This is make.info, produced by makeinfo version 4.0 from make.texinfo.

INFO-DIR-SECTION GNU Packages
START-INFO-DIR-ENTRY
* Make: (make).            Remake files automatically.
END-INFO-DIR-ENTRY

   This file documents the GNU Make utility, which determines
automatically which pieces of a large program need to be recompiled,
and issues the commands to recompile them.

   This is Edition 0.55, last updated 04 April 2000, of `The GNU Make
Manual', for `make', Version 3.79.

   Copyright (C) 1988, '89, '90, '91, '92, '93, '94, '95, '96, '97,
'98, '99, 2000         Free Software Foundation, Inc.

   Permission is granted to make and distribute verbatim copies of this
manual provided the copyright notice and this permission notice are
preserved on all copies.

   Permission is granted to copy and distribute modified versions of
this manual under the conditions for verbatim copying, provided that
the entire resulting derived work is distributed under the terms of a
permission notice identical to this one.

   Permission is granted to copy and distribute translations of this
manual into another language, under the above conditions for modified
versions, except that this permission notice may be stated in a
translation approved by the Free Software Foundation.


File: make.info,  Node: Double-Colon,  Next: Automatic Prerequisites,  Prev: Static Pattern,  Up: Rules

Double-Colon Rules
==================

   "Double-colon" rules are rules written with `::' instead of `:'
after the target names.  They are handled differently from ordinary
rules when the same target appears in more than one rule.

   When a target appears in multiple rules, all the rules must be the
same type: all ordinary, or all double-colon.  If they are
double-colon, each of them is independent of the others.  Each
double-colon rule's commands are executed if the target is older than
any prerequisites of that rule.  This can result in executing none,
any, or all of the double-colon rules.

   Double-colon rules with the same target are in fact completely
separate from one another.  Each double-colon rule is processed
individually, just as rules with different targets are processed.

   The double-colon rules for a target are executed in the order they
appear in the makefile.  However, the cases where double-colon rules
really make sense are those where the order of executing the commands
would not matter.

   Double-colon rules are somewhat obscure and not often very useful;
they provide a mechanism for cases in which the method used to update a
target differs depending on which prerequisite files caused the update,
and such cases are rare.

   Each double-colon rule should specify commands; if it does not, an
implicit rule will be used if one applies.  *Note Using Implicit Rules:
Implicit Rules.


File: make.info,  Node: Automatic Prerequisites,  Prev: Double-Colon,  Up: Rules

Generating Prerequisites Automatically
======================================

   In the makefile for a program, many of the rules you need to write
often say only that some object file depends on some header file.  For
example, if `main.c' uses `defs.h' via an `#include', you would write:

     main.o: defs.h

You need this rule so that `make' knows that it must remake `main.o'
whenever `defs.h' changes.  You can see that for a large program you
would have to write dozens of such rules in your makefile.  And, you
must always be very careful to update the makefile every time you add
or remove an `#include'.

   To avoid this hassle, most modern C compilers can write these rules
for you, by looking at the `#include' lines in the source files.
Usually this is done with the `-M' option to the compiler.  For
example, the command:

     cc -M main.c

generates the output:

     main.o : main.c defs.h

Thus you no longer have to write all those rules yourself.  The
compiler will do it for you.

   Note that such a prerequisite constitutes mentioning `main.o' in a
makefile, so it can never be considered an intermediate file by implicit
rule search.  This means that `make' won't ever remove the file after
using it; *note Chains of Implicit Rules: Chained Rules..

   With old `make' programs, it was traditional practice to use this
compiler feature to generate prerequisites on demand with a command like
`make depend'.  That command would create a file `depend' containing
all the automatically-generated prerequisites; then the makefile could
use `include' to read them in (*note Include::).

   In GNU `make', the feature of remaking makefiles makes this practice
obsolete--you need never tell `make' explicitly to regenerate the
prerequisites, because it always regenerates any makefile that is out
of date.  *Note Remaking Makefiles::.

   The practice we recommend for automatic prerequisite generation is
to have one makefile corresponding to each source file.  For each
source file `NAME.c' there is a makefile `NAME.d' which lists what
files the object file `NAME.o' depends on.  That way only the source
files that have changed need to be rescanned to produce the new
prerequisites.

   Here is the pattern rule to generate a file of prerequisites (i.e.,
a makefile) called `NAME.d' from a C source file called `NAME.c':

     %.d: %.c
             set -e; $(CC) -M $(CPPFLAGS) $< \
                       | sed 's/\($*\)\.o[ :]*/\1.o $@ : /g' > $@; \
                     [ -s $@ ] || rm -f $@

*Note Pattern Rules::, for information on defining pattern rules.  The
`-e' flag to the shell makes it exit immediately if the `$(CC)' command
fails (exits with a nonzero status).  Normally the shell exits with the
status of the last command in the pipeline (`sed' in this case), so
`make' would not notice a nonzero status from the compiler.

   With the GNU C compiler, you may wish to use the `-MM' flag instead
of `-M'.  This omits prerequisites on system header files.  *Note
Options Controlling the Preprocessor: (gcc.info)Preprocessor Options,
for details.

   The purpose of the `sed' command is to translate (for example):

     main.o : main.c defs.h

into:

     main.o main.d : main.c defs.h

This makes each `.d' file depend on all the source and header files
that the corresponding `.o' file depends on.  `make' then knows it must
regenerate the prerequisites whenever any of the source or header files
changes.

   Once you've defined the rule to remake the `.d' files, you then use
the `include' directive to read them all in.  *Note Include::.  For
example:

     sources = foo.c bar.c
     
     include $(sources:.c=.d)

(This example uses a substitution variable reference to translate the
list of source files `foo.c bar.c' into a list of prerequisite
makefiles, `foo.d bar.d'.  *Note Substitution Refs::, for full
information on substitution references.)  Since the `.d' files are
makefiles like any others, `make' will remake them as necessary with no
further work from you.  *Note Remaking Makefiles::.


File: make.info,  Node: Commands,  Next: Using Variables,  Prev: Rules,  Up: Top

Writing the Commands in Rules
*****************************

   The commands of a rule consist of shell command lines to be executed
one by one.  Each command line must start with a tab, except that the
first command line may be attached to the target-and-prerequisites line
with a semicolon in between.  Blank lines and lines of just comments
may appear among the command lines; they are ignored.  (But beware, an
apparently "blank" line that begins with a tab is _not_ blank!  It is an
empty command; *note Empty Commands::.)

   Users use many different shell programs, but commands in makefiles
are always interpreted by `/bin/sh' unless the makefile specifies
otherwise.  *Note Command Execution: Execution.

   The shell that is in use determines whether comments can be written
on command lines, and what syntax they use.  When the shell is
`/bin/sh', a `#' starts a comment that extends to the end of the line.
The `#' does not have to be at the beginning of a line.  Text on a line
before a `#' is not part of the comment.

* Menu:

* Echoing::                     How to control when commands are echoed.
* Execution::                   How commands are executed.
* Parallel::                    How commands can be executed in parallel.
* Errors::                      What happens after a command execution error.
* Interrupts::                  What happens when a command is interrupted.
* Recursion::                   Invoking `make' from makefiles.
* Sequences::                   Defining canned sequences of commands.
* Empty Commands::              Defining useful, do-nothing commands.


File: make.info,  Node: Echoing,  Next: Execution,  Up: Commands

Command Echoing
===============

   Normally `make' prints each command line before it is executed.  We
call this "echoing" because it gives the appearance that you are typing
the commands yourself.

   When a line starts with `@', the echoing of that line is suppressed.
The `@' is discarded before the command is passed to the shell.
Typically you would use this for a command whose only effect is to print
something, such as an `echo' command to indicate progress through the
makefile:

     @echo About to make distribution files

   When `make' is given the flag `-n' or `--just-print' it only echoes
commands, it won't execute them.  *Note Summary of Options: Options
Summary.  In this case and only this case, even the commands starting
with `@' are printed.  This flag is useful for finding out which
commands `make' thinks are necessary without actually doing them.

   The `-s' or `--silent' flag to `make' prevents all echoing, as if
all commands started with `@'.  A rule in the makefile for the special
target `.SILENT' without prerequisites has the same effect (*note
Special Built-in Target Names: Special Targets.).  `.SILENT' is
essentially obsolete since `@' is more flexible.


File: make.info,  Node: Execution,  Next: Parallel,  Prev: Echoing,  Up: Commands

Command Execution
=================

   When it is time to execute commands to update a target, they are
executed by making a new subshell for each line.  (In practice, `make'
may take shortcuts that do not affect the results.)

   *Please note:* this implies that shell commands such as `cd' that
set variables local to each process will not affect the following
command lines. (1)  If you want to use `cd' to affect the next command,
put the two on a single line with a semicolon between them.  Then
`make' will consider them a single command and pass them, together, to
a shell which will execute them in sequence.  For example:

     foo : bar/lose
             cd bar; gobble lose > ../foo

   If you would like to split a single shell command into multiple
lines of text, you must use a backslash at the end of all but the last
subline.  Such a sequence of lines is combined into a single line, by
deleting the backslash-newline sequences, before passing it to the
shell.  Thus, the following is equivalent to the preceding example:

     foo : bar/lose
             cd bar;  \
             gobble lose > ../foo

   The program used as the shell is taken from the variable `SHELL'.
By default, the program `/bin/sh' is used.

   On MS-DOS, if `SHELL' is not set, the value of the variable
`COMSPEC' (which is always set) is used instead.

   The processing of lines that set the variable `SHELL' in Makefiles
is different on MS-DOS.  The stock shell, `command.com', is
ridiculously limited in its functionality and many users of `make' tend
to install a replacement shell.  Therefore, on MS-DOS, `make' examines
the value of `SHELL', and changes its behavior based on whether it
points to a Unix-style or DOS-style shell.  This allows reasonable
functionality even if `SHELL' points to `command.com'.

   If `SHELL' points to a Unix-style shell, `make' on MS-DOS
additionally checks whether that shell can indeed be found; if not, it
ignores the line that sets `SHELL'.  In MS-DOS, GNU `make' searches for
the shell in the following places:

  1. In the precise place pointed to by the value of `SHELL'.  For
     example, if the makefile specifies `SHELL = /bin/sh', `make' will
     look in the directory `/bin' on the current drive.

  2. In the current directory.

  3. In each of the directories in the `PATH' variable, in order.


   In every directory it examines, `make' will first look for the
specific file (`sh' in the example above).  If this is not found, it
will also look in that directory for that file with one of the known
extensions which identify executable files.  For example `.exe',
`.com', `.bat', `.btm', `.sh', and some others.

   If any of these attempts is successful, the value of `SHELL' will be
set to the full pathname of the shell as found.  However, if none of
these is found, the value of `SHELL' will not be changed, and thus the
line that sets it will be effectively ignored.  This is so `make' will
only support features specific to a Unix-style shell if such a shell is
actually installed on the system where `make' runs.

   Note that this extended search for the shell is limited to the cases
where `SHELL' is set from the Makefile; if it is set in the environment
or command line, you are expected to set it to the full pathname of the
shell, exactly as things are on Unix.

   The effect of the above DOS-specific processing is that a Makefile
that says `SHELL = /bin/sh' (as many Unix makefiles do), will work on
MS-DOS unaltered if you have e.g. `sh.exe' installed in some directory
along your `PATH'.

   Unlike most variables, the variable `SHELL' is never set from the
environment.  This is because the `SHELL' environment variable is used
to specify your personal choice of shell program for interactive use.
It would be very bad for personal choices like this to affect the
functioning of makefiles.  *Note Variables from the Environment:
Environment.  However, on MS-DOS and MS-Windows the value of `SHELL' in
the environment *is* used, since on those systems most users do not set
this variable, and therefore it is most likely set specifically to be
used by `make'.  On MS-DOS, if the setting of `SHELL' is not suitable
for `make', you can set the variable `MAKESHELL' to the shell that
`make' should use; this will override the value of `SHELL'.

   ---------- Footnotes ----------

   (1) On MS-DOS, the value of current working directory is *global*,
so changing it _will_ affect the following command lines on those
systems.


File: make.info,  Node: Parallel,  Next: Errors,  Prev: Execution,  Up: Commands

Parallel Execution
==================

   GNU `make' knows how to execute several commands at once.  Normally,
`make' will execute only one command at a time, waiting for it to
finish before executing the next.  However, the `-j' or `--jobs' option
tells `make' to execute many commands simultaneously.

   On MS-DOS, the `-j' option has no effect, since that system doesn't
support multi-processing.

   If the `-j' option is followed by an integer, this is the number of
commands to execute at once; this is called the number of "job slots".
If there is nothing looking like an integer after the `-j' option,
there is no limit on the number of job slots.  The default number of job
slots is one, which means serial execution (one thing at a time).

   One unpleasant consequence of running several commands
simultaneously is that output generated by the commands appears
whenever each command sends it, so messages from different commands may
be interspersed.

   Another problem is that two processes cannot both take input from the
same device; so to make sure that only one command tries to take input
from the terminal at once, `make' will invalidate the standard input
streams of all but one running command.  This means that attempting to
read from standard input will usually be a fatal error (a `Broken pipe'
signal) for most child processes if there are several.

   It is unpredictable which command will have a valid standard input
stream (which will come from the terminal, or wherever you redirect the
standard input of `make').  The first command run will always get it
first, and the first command started after that one finishes will get
it next, and so on.

   We will change how this aspect of `make' works if we find a better
alternative.  In the mean time, you should not rely on any command using
standard input at all if you are using the parallel execution feature;
but if you are not using this feature, then standard input works
normally in all commands.

   Finally, handling recursive `make' invocations raises issues.  For
more information on this, see *Note Communicating Options to a
Sub-`make': Options/Recursion.

   If a command fails (is killed by a signal or exits with a nonzero
status), and errors are not ignored for that command (*note Errors in
Commands: Errors.), the remaining command lines to remake the same
target will not be run.  If a command fails and the `-k' or
`--keep-going' option was not given (*note Summary of Options: Options
Summary.), `make' aborts execution.  If make terminates for any reason
(including a signal) with child processes running, it waits for them to
finish before actually exiting.

   When the system is heavily loaded, you will probably want to run
fewer jobs than when it is lightly loaded.  You can use the `-l' option
to tell `make' to limit the number of jobs to run at once, based on the
load average.  The `-l' or `--max-load' option is followed by a
floating-point number.  For example,

     -l 2.5

will not let `make' start more than one job if the load average is
above 2.5.  The `-l' option with no following number removes the load
limit, if one was given with a previous `-l' option.

   More precisely, when `make' goes to start up a job, and it already
has at least one job running, it checks the current load average; if it
is not lower than the limit given with `-l', `make' waits until the load
average goes below that limit, or until all the other jobs finish.

   By default, there is no load limit.


File: make.info,  Node: Errors,  Next: Interrupts,  Prev: Parallel,  Up: Commands

Errors in Commands
==================

   After each shell command returns, `make' looks at its exit status.
If the command completed successfully, the next command line is executed
in a new shell; after the last command line is finished, the rule is
finished.

   If there is an error (the exit status is nonzero), `make' gives up on
the current rule, and perhaps on all rules.

   Sometimes the failure of a certain command does not indicate a
problem.  For example, you may use the `mkdir' command to ensure that a
directory exists.  If the directory already exists, `mkdir' will report
an error, but you probably want `make' to continue regardless.

   To ignore errors in a command line, write a `-' at the beginning of
the line's text (after the initial tab).  The `-' is discarded before
the command is passed to the shell for execution.

   For example,

     clean:
             -rm -f *.o

This causes `rm' to continue even if it is unable to remove a file.

   When you run `make' with the `-i' or `--ignore-errors' flag, errors
are ignored in all commands of all rules.  A rule in the makefile for
the special target `.IGNORE' has the same effect, if there are no
prerequisites.  These ways of ignoring errors are obsolete because `-'
is more flexible.

   When errors are to be ignored, because of either a `-' or the `-i'
flag, `make' treats an error return just like success, except that it
prints out a message that tells you the status code the command exited
with, and says that the error has been ignored.

   When an error happens that `make' has not been told to ignore, it
implies that the current target cannot be correctly remade, and neither
can any other that depends on it either directly or indirectly.  No
further commands will be executed for these targets, since their
preconditions have not been achieved.

   Normally `make' gives up immediately in this circumstance, returning
a nonzero status.  However, if the `-k' or `--keep-going' flag is
specified, `make' continues to consider the other prerequisites of the
pending targets, remaking them if necessary, before it gives up and
returns nonzero status.  For example, after an error in compiling one
object file, `make -k' will continue compiling other object files even
though it already knows that linking them will be impossible.  *Note
Summary of Options: Options Summary.

   The usual behavior assumes that your purpose is to get the specified
targets up to date; once `make' learns that this is impossible, it
might as well report the failure immediately.  The `-k' option says
that the real purpose is to test as many of the changes made in the
program as possible, perhaps to find several independent problems so
that you can correct them all before the next attempt to compile.  This
is why Emacs' `compile' command passes the `-k' flag by default.

   Usually when a command fails, if it has changed the target file at
all, the file is corrupted and cannot be used--or at least it is not
completely updated.  Yet the file's timestamp says that it is now up to
date, so the next time `make' runs, it will not try to update that
file.  The situation is just the same as when the command is killed by a
signal; *note Interrupts::.  So generally the right thing to do is to
delete the target file if the command fails after beginning to change
the file.  `make' will do this if `.DELETE_ON_ERROR' appears as a
target.  This is almost always what you want `make' to do, but it is
not historical practice; so for compatibility, you must explicitly
request it.


File: make.info,  Node: Interrupts,  Next: Recursion,  Prev: Errors,  Up: Commands

Interrupting or Killing `make'
==============================

   If `make' gets a fatal signal while a command is executing, it may
delete the target file that the command was supposed to update.  This is
done if the target file's last-modification time has changed since
`make' first checked it.

   The purpose of deleting the target is to make sure that it is remade
from scratch when `make' is next run.  Why is this?  Suppose you type
`Ctrl-c' while a compiler is running, and it has begun to write an
object file `foo.o'.  The `Ctrl-c' kills the compiler, resulting in an
incomplete file whose last-modification time is newer than the source
file `foo.c'.  But `make' also receives the `Ctrl-c' signal and deletes
this incomplete file.  If `make' did not do this, the next invocation
of `make' would think that `foo.o' did not require updating--resulting
in a strange error message from the linker when it tries to link an
object file half of which is missing.

   You can prevent the deletion of a target file in this way by making
the special target `.PRECIOUS' depend on it.  Before remaking a target,
`make' checks to see whether it appears on the prerequisites of
`.PRECIOUS', and thereby decides whether the target should be deleted
if a signal happens.  Some reasons why you might do this are that the
target is updated in some atomic fashion, or exists only to record a
modification-time (its contents do not matter), or must exist at all
times to prevent other sorts of trouble.


File: make.info,  Node: Recursion,  Next: Sequences,  Prev: Interrupts,  Up: Commands

Recursive Use of `make'
=======================

   Recursive use of `make' means using `make' as a command in a
makefile.  This technique is useful when you want separate makefiles for
various subsystems that compose a larger system.  For example, suppose
you have a subdirectory `subdir' which has its own makefile, and you
would like the containing directory's makefile to run `make' on the
subdirectory.  You can do it by writing this:

     subsystem:
             cd subdir && $(MAKE)

or, equivalently, this (*note Summary of Options: Options Summary.):

     subsystem:
             $(MAKE) -C subdir

   You can write recursive `make' commands just by copying this example,
but there are many things to know about how they work and why, and about
how the sub-`make' relates to the top-level `make'.

   For your convenience, GNU `make' sets the variable `CURDIR' to the
pathname of the current working directory for you.  If `-C' is in
effect, it will contain the path of the new directory, not the
original.  The value has the same precedence it would have if it were
set in the makefile (by default, an environment variable `CURDIR' will
not override this value).  Note that setting this variable has no
effect on the operation of `make'

* Menu:

* MAKE Variable::               The special effects of using `$(MAKE)'.
* Variables/Recursion::         How to communicate variables to a sub-`make'.
* Options/Recursion::           How to communicate options to a sub-`make'.
* -w Option::                   How the `-w' or `--print-directory' option
                                 helps debug use of recursive `make' commands.


File: make.info,  Node: MAKE Variable,  Next: Variables/Recursion,  Up: Recursion

How the `MAKE' Variable Works
-----------------------------

   Recursive `make' commands should always use the variable `MAKE', not
the explicit command name `make', as shown here:

     subsystem:
             cd subdir && $(MAKE)

   The value of this variable is the file name with which `make' was
invoked.  If this file name was `/bin/make', then the command executed
is `cd subdir && /bin/make'.  If you use a special version of `make' to
run the top-level makefile, the same special version will be executed
for recursive invocations.

   As a special feature, using the variable `MAKE' in the commands of a
rule alters the effects of the `-t' (`--touch'), `-n' (`--just-print'),
or `-q' (`--question') option.  Using the `MAKE' variable has the same
effect as using a `+' character at the beginning of the command line.
*Note Instead of Executing the Commands: Instead of Execution.

   Consider the command `make -t' in the above example.  (The `-t'
option marks targets as up to date without actually running any
commands; see *Note Instead of Execution::.)  Following the usual
definition of `-t', a `make -t' command in the example would create a
file named `subsystem' and do nothing else.  What you really want it to
do is run `cd subdir && make -t'; but that would require executing the
command, and `-t' says not to execute commands.

   The special feature makes this do what you want: whenever a command
line of a rule contains the variable `MAKE', the flags `-t', `-n' and
`-q' do not apply to that line.  Command lines containing `MAKE' are
executed normally despite the presence of a flag that causes most
commands not to be run.  The usual `MAKEFLAGS' mechanism passes the
flags to the sub-`make' (*note Communicating Options to a Sub-`make':
Options/Recursion.), so your request to touch the files, or print the
commands, is propagated to the subsystem.


File: make.info,  Node: Variables/Recursion,  Next: Options/Recursion,  Prev: MAKE Variable,  Up: Recursion

Communicating Variables to a Sub-`make'
---------------------------------------

   Variable values of the top-level `make' can be passed to the
sub-`make' through the environment by explicit request.  These
variables are defined in the sub-`make' as defaults, but do not
override what is specified in the makefile used by the sub-`make'
makefile unless you use the `-e' switch (*note Summary of Options:
Options Summary.).

   To pass down, or "export", a variable, `make' adds the variable and
its value to the environment for running each command.  The sub-`make',
in turn, uses the environment to initialize its table of variable
values.  *Note Variables from the Environment: Environment.

   Except by explicit request, `make' exports a variable only if it is
either defined in the environment initially or set on the command line,
and if its name consists only of letters, numbers, and underscores.
Some shells cannot cope with environment variable names consisting of
characters other than letters, numbers, and underscores.

   The special variables `SHELL' and `MAKEFLAGS' are always exported
(unless you unexport them).  `MAKEFILES' is exported if you set it to
anything.

   `make' automatically passes down variable values that were defined
on the command line, by putting them in the `MAKEFLAGS' variable.
*Note Options/Recursion::.

   Variables are _not_ normally passed down if they were created by
default by `make' (*note Variables Used by Implicit Rules: Implicit
Variables.).  The sub-`make' will define these for itself.

   If you want to export specific variables to a sub-`make', use the
`export' directive, like this:

     export VARIABLE ...

If you want to _prevent_ a variable from being exported, use the
`unexport' directive, like this:

     unexport VARIABLE ...

As a convenience, you can define a variable and export it at the same
time by doing:

     export VARIABLE = value

has the same result as:

     VARIABLE = value
     export VARIABLE

and

     export VARIABLE := value

has the same result as:

     VARIABLE := value
     export VARIABLE

   Likewise,

     export VARIABLE += value

is just like:

     VARIABLE += value
     export VARIABLE

*Note Appending More Text to Variables: Appending.

   You may notice that the `export' and `unexport' directives work in
`make' in the same way they work in the shell, `sh'.

   If you want all variables to be exported by default, you can use
`export' by itself:

     export

This tells `make' that variables which are not explicitly mentioned in
an `export' or `unexport' directive should be exported.  Any variable
given in an `unexport' directive will still _not_ be exported.  If you
use `export' by itself to export variables by default, variables whose
names contain characters other than alphanumerics and underscores will
not be exported unless specifically mentioned in an `export' directive.

   The behavior elicited by an `export' directive by itself was the
default in older versions of GNU `make'.  If your makefiles depend on
this behavior and you want to be compatible with old versions of
`make', you can write a rule for the special target
`.EXPORT_ALL_VARIABLES' instead of using the `export' directive.  This
will be ignored by old `make's, while the `export' directive will cause
a syntax error.

   Likewise, you can use `unexport' by itself to tell `make' _not_ to
export variables by default.  Since this is the default behavior, you
would only need to do this if `export' had been used by itself earlier
(in an included makefile, perhaps).  You *cannot* use `export' and
`unexport' by themselves to have variables exported for some commands
and not for others.  The last `export' or `unexport' directive that
appears by itself determines the behavior for the entire run of `make'.

   As a special feature, the variable `MAKELEVEL' is changed when it is
passed down from level to level.  This variable's value is a string
which is the depth of the level as a decimal number.  The value is `0'
for the top-level `make'; `1' for a sub-`make', `2' for a
sub-sub-`make', and so on.  The incrementation happens when `make' sets
up the environment for a command.

   The main use of `MAKELEVEL' is to test it in a conditional directive
(*note Conditional Parts of Makefiles: Conditionals.); this way you can
write a makefile that behaves one way if run recursively and another
way if run directly by you.

   You can use the variable `MAKEFILES' to cause all sub-`make'
commands to use additional makefiles.  The value of `MAKEFILES' is a
whitespace-separated list of file names.  This variable, if defined in
the outer-level makefile, is passed down through the environment; then
it serves as a list of extra makefiles for the sub-`make' to read
before the usual or specified ones.  *Note The Variable `MAKEFILES':
MAKEFILES Variable.


File: make.info,  Node: Options/Recursion,  Next: -w Option,  Prev: Variables/Recursion,  Up: Recursion

Communicating Options to a Sub-`make'
-------------------------------------

   Flags such as `-s' and `-k' are passed automatically to the
sub-`make' through the variable `MAKEFLAGS'.  This variable is set up
automatically by `make' to contain the flag letters that `make'
received.  Thus, if you do `make -ks' then `MAKEFLAGS' gets the value
`ks'.

   As a consequence, every sub-`make' gets a value for `MAKEFLAGS' in
its environment.  In response, it takes the flags from that value and
processes them as if they had been given as arguments.  *Note Summary
of Options: Options Summary.

   Likewise variables defined on the command line are passed to the
sub-`make' through `MAKEFLAGS'.  Words in the value of `MAKEFLAGS' that
contain `=', `make' treats as variable definitions just as if they
appeared on the command line.  *Note Overriding Variables: Overriding.

   The options `-C', `-f', `-o', and `-W' are not put into `MAKEFLAGS';
these options are not passed down.

   The `-j' option is a special case (*note Parallel Execution:
Parallel.).  If you set it to some numeric value `N' and your operating
system supports it (most any UNIX system will; others typically won't),
the parent `make' and all the sub-`make's will communicate to ensure
that there are only `N' jobs running at the same time between them all.
Note that any job that is marked recursive (*note Instead of Executing
the Commands: Instead of Execution.)  doesn't count against the total
jobs (otherwise we could get `N' sub-`make's running and have no slots
left over for any real work!)

   If your operating system doesn't support the above communication,
then `-j 1' is always put into `MAKEFLAGS' instead of the value you
specified.  This is because if the `-j' option were passed down to
sub-`make's, you would get many more jobs running in parallel than you
asked for.  If you give `-j' with no numeric argument, meaning to run
as many jobs as possible in parallel, this is passed down, since
multiple infinities are no more than one.

   If you do not want to pass the other flags down, you must change the
value of `MAKEFLAGS', like this:

     subsystem:
             cd subdir && $(MAKE) MAKEFLAGS=

   The command line variable definitions really appear in the variable
`MAKEOVERRIDES', and `MAKEFLAGS' contains a reference to this variable.
If you do want to pass flags down normally, but don't want to pass
down the command line variable definitions, you can reset
`MAKEOVERRIDES' to empty, like this:

     MAKEOVERRIDES =

This is not usually useful to do.  However, some systems have a small
fixed limit on the size of the environment, and putting so much
information into the value of `MAKEFLAGS' can exceed it.  If you see
the error message `Arg list too long', this may be the problem.  (For
strict compliance with POSIX.2, changing `MAKEOVERRIDES' does not
affect `MAKEFLAGS' if the special target `.POSIX' appears in the
makefile.  You probably do not care about this.)

   A similar variable `MFLAGS' exists also, for historical
compatibility.  It has the same value as `MAKEFLAGS' except that it
does not contain the command line variable definitions, and it always
begins with a hyphen unless it is empty (`MAKEFLAGS' begins with a
hyphen only when it begins with an option that has no single-letter
version, such as `--warn-undefined-variables').  `MFLAGS' was
traditionally used explicitly in the recursive `make' command, like
this:

     subsystem:
             cd subdir && $(MAKE) $(MFLAGS)

but now `MAKEFLAGS' makes this usage redundant.  If you want your
makefiles to be compatible with old `make' programs, use this
technique; it will work fine with more modern `make' versions too.

   The `MAKEFLAGS' variable can also be useful if you want to have
certain options, such as `-k' (*note Summary of Options: Options
Summary.), set each time you run `make'.  You simply put a value for
`MAKEFLAGS' in your environment.  You can also set `MAKEFLAGS' in a
makefile, to specify additional flags that should also be in effect for
that makefile.  (Note that you cannot use `MFLAGS' this way.  That
variable is set only for compatibility; `make' does not interpret a
value you set for it in any way.)

   When `make' interprets the value of `MAKEFLAGS' (either from the
environment or from a makefile), it first prepends a hyphen if the value
does not already begin with one.  Then it chops the value into words
separated by blanks, and parses these words as if they were options
given on the command line (except that `-C', `-f', `-h', `-o', `-W',
and their long-named versions are ignored; and there is no error for an
invalid option).

   If you do put `MAKEFLAGS' in your environment, you should be sure not
to include any options that will drastically affect the actions of
`make' and undermine the purpose of makefiles and of `make' itself.
For instance, the `-t', `-n', and `-q' options, if put in one of these
variables, could have disastrous consequences and would certainly have
at least surprising and probably annoying effects.


File: make.info,  Node: -w Option,  Prev: Options/Recursion,  Up: Recursion

The `--print-directory' Option
------------------------------

   If you use several levels of recursive `make' invocations, the `-w'
or `--print-directory' option can make the output a lot easier to
understand by showing each directory as `make' starts processing it and
as `make' finishes processing it.  For example, if `make -w' is run in
the directory `/u/gnu/make', `make' will print a line of the form:

     make: Entering directory `/u/gnu/make'.

before doing anything else, and a line of the form:

     make: Leaving directory `/u/gnu/make'.

when processing is completed.

   Normally, you do not need to specify this option because `make' does
it for you: `-w' is turned on automatically when you use the `-C'
option, and in sub-`make's.  `make' will not automatically turn on `-w'
if you also use `-s', which says to be silent, or if you use
`--no-print-directory' to explicitly disable it.


File: make.info,  Node: Sequences,  Next: Empty Commands,  Prev: Recursion,  Up: Commands

Defining Canned Command Sequences
=================================

   When the same sequence of commands is useful in making various
targets, you can define it as a canned sequence with the `define'
directive, and refer to the canned sequence from the rules for those
targets.  The canned sequence is actually a variable, so the name must
not conflict with other variable names.

   Here is an example of defining a canned sequence of commands:

     define run-yacc
     yacc $(firstword $^)
     mv y.tab.c $@
     endef

Here `run-yacc' is the name of the variable being defined; `endef'
marks the end of the definition; the lines in between are the commands.
The `define' directive does not expand variable references and
function calls in the canned sequence; the `$' characters, parentheses,
variable names, and so on, all become part of the value of the variable
you are defining.  *Note Defining Variables Verbatim: Defining, for a
complete explanation of `define'.

   The first command in this example runs Yacc on the first
prerequisite of whichever rule uses the canned sequence.  The output
file from Yacc is always named `y.tab.c'.  The second command moves the
output to the rule's target file name.

   To use the canned sequence, substitute the variable into the
commands of a rule.  You can substitute it like any other variable
(*note Basics of Variable References: Reference.).  Because variables
defined by `define' are recursively expanded variables, all the
variable references you wrote inside the `define' are expanded now.
For example:

     foo.c : foo.y
             $(run-yacc)

`foo.y' will be substituted for the variable `$^' when it occurs in
`run-yacc''s value, and `foo.c' for `$@'.

   This is a realistic example, but this particular one is not needed in
practice because `make' has an implicit rule to figure out these
commands based on the file names involved (*note Using Implicit Rules:
Implicit Rules.).

   In command execution, each line of a canned sequence is treated just
as if the line appeared on its own in the rule, preceded by a tab.  In
particular, `make' invokes a separate subshell for each line.  You can
use the special prefix characters that affect command lines (`@', `-',
and `+') on each line of a canned sequence.  *Note Writing the Commands
in Rules: Commands.  For example, using this canned sequence:

     define frobnicate
     @echo "frobnicating target $@"
     frob-step-1 $< -o $@-step-1
     frob-step-2 $@-step-1 -o $@
     endef

`make' will not echo the first line, the `echo' command.  But it _will_
echo the following two command lines.

   On the other hand, prefix characters on the command line that refers
to a canned sequence apply to every line in the sequence.  So the rule:

     frob.out: frob.in
             @$(frobnicate)

does not echo _any_ commands.  (*Note Command Echoing: Echoing, for a
full explanation of `@'.)


File: make.info,  Node: Empty Commands,  Prev: Sequences,  Up: Commands

Using Empty Commands
====================

   It is sometimes useful to define commands which do nothing.  This is
done simply by giving a command that consists of nothing but
whitespace.  For example:

     target: ;

defines an empty command string for `target'.  You could also use a
line beginning with a tab character to define an empty command string,
but this would be confusing because such a line looks empty.

   You may be wondering why you would want to define a command string
that does nothing.  The only reason this is useful is to prevent a
target from getting implicit commands (from implicit rules or the
`.DEFAULT' special target; *note Implicit Rules:: and *note Defining
Last-Resort Default Rules: Last Resort.).

   You may be inclined to define empty command strings for targets that
are not actual files, but only exist so that their prerequisites can be
remade.  However, this is not the best way to do that, because the
prerequisites may not be remade properly if the target file actually
does exist.  *Note Phony Targets: Phony Targets, for a better way to do
this.


File: make.info,  Node: Using Variables,  Next: Conditionals,  Prev: Commands,  Up: Top

How to Use Variables
********************

   A "variable" is a name defined in a makefile to represent a string
of text, called the variable's "value".  These values are substituted
by explicit request into targets, prerequisites, commands, and other
parts of the makefile.  (In some other versions of `make', variables
are called "macros".)

   Variables and functions in all parts of a makefile are expanded when
read, except for the shell commands in rules, the right-hand sides of
variable definitions using `=', and the bodies of variable definitions
using the `define' directive.

   Variables can represent lists of file names, options to pass to
compilers, programs to run, directories to look in for source files,
directories to write output in, or anything else you can imagine.

   A variable name may be any sequence of characters not containing `:',
`#', `=', or leading or trailing whitespace.  However, variable names
containing characters other than letters, numbers, and underscores
should be avoided, as they may be given special meanings in the future,
and with some shells they cannot be passed through the environment to a
sub-`make' (*note Communicating Variables to a Sub-`make':
Variables/Recursion.).

   Variable names are case-sensitive.  The names `foo', `FOO', and
`Foo' all refer to different variables.

   It is traditional to use upper case letters in variable names, but we
recommend using lower case letters for variable names that serve
internal purposes in the makefile, and reserving upper case for
parameters that control implicit rules or for parameters that the user
should override with command options (*note Overriding Variables:
Overriding.).

   A few variables have names that are a single punctuation character or
just a few characters.  These are the "automatic variables", and they
have particular specialized uses.  *Note Automatic Variables: Automatic.

* Menu:

* Reference::                   How to use the value of a variable.
* Flavors::                     Variables come in two flavors.
* Advanced::                    Advanced features for referencing a variable.
* Values::                      All the ways variables get their values.
* Setting::                     How to set a variable in the makefile.
* Appending::                   How to append more text to the old value
                                  of a variable.
* Override Directive::          How to set a variable in the makefile even if
                                  the user has set it with a command argument.
* Defining::                    An alternate way to set a variable
                                  to a verbatim string.
* Environment::                 Variable values can come from the environment.
* Target-specific::             Variable values can be defined on a per-target
                                  basis.
* Pattern-specific::            Target-specific variable values can be applied
                                  to a group of targets that match a pattern.
* Automatic::                   Some special variables have predefined
                                  meanings for use with implicit rules.


File: make.info,  Node: Reference,  Next: Flavors,  Up: Using Variables

Basics of Variable References
=============================

   To substitute a variable's value, write a dollar sign followed by
the name of the variable in parentheses or braces: either `$(foo)' or
`${foo}' is a valid reference to the variable `foo'.  This special
significance of `$' is why you must write `$$' to have the effect of a
single dollar sign in a file name or command.

   Variable references can be used in any context: targets,
prerequisites, commands, most directives, and new variable values.
Here is an example of a common case, where a variable holds the names
of all the object files in a program:

     objects = program.o foo.o utils.o
     program : $(objects)
             cc -o program $(objects)
     
     $(objects) : defs.h

   Variable references work by strict textual substitution.  Thus, the
rule

     foo = c
     prog.o : prog.$(foo)
             $(foo)$(foo) -$(foo) prog.$(foo)

could be used to compile a C program `prog.c'.  Since spaces before the
variable value are ignored in variable assignments, the value of `foo'
is precisely `c'.  (Don't actually write your makefiles this way!)

   A dollar sign followed by a character other than a dollar sign,
open-parenthesis or open-brace treats that single character as the
variable name.  Thus, you could reference the variable `x' with `$x'.
However, this practice is strongly discouraged, except in the case of
the automatic variables (*note Automatic Variables: Automatic.).

