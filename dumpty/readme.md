## Dumpty

Dump tool for .Net.

### Work in progress

This started off as a tool I use at work, a collection of my ClrMD scripts. It's extremely spartan and needs some refactoring.

### Getting started

**Getting started with Git and GitHub**

 * People new to GitHub should consider using [GitHub for Windows](http://windows.github.com/).
 * If you decide not to use GHFW you will need to:
  1. [Set up Git and connect to GitHub](http://help.github.com/win-set-up-git/)
  2. [Fork the CompilerKit repository](http://help.github.com/fork-a-repo/)
 * Finally you should look into [git - the simple guide](http://rogerdudler.github.com/git-guide/)

**Rules for Our Git Repository**

 * We use ["A successful Git branching model"](http://nvie.com/posts/a-successful-git-branching-model/). What this means is that:
   * You need to branch off of the [develop branch](https://github.com/jcdickinson/dumpty) when creating new features or non-critical bug fixes.
   * Each logical unit of work must come from a single and unique branch:
     * A logical unit of work could be a set of related bugs or a feature.
     * You should wait for us to accept the pull request (or you can cancel it) before committing to that branch again.

### How To Use Dumpty

1. Execute the REPL command at the CLI: `dumpty -e repl`
2. Load a memory dump: `load "C:\dev\dumps\dump.dmp"`
3. Execute a command: `dumpstrings "C:\dev\strings"`

### Supported Commands

* **repl:** Creates a REPL
* **help:** When followed by another command, explains the usage of the command
* **load:** Loads a memory dump
* **dumpstrings:** Finds unique strings and dumps them to a directory as individual files
* **agcroot:** Aggregates GC roots. Helps with finding memory issues caused by large amounts of small objects.

### License

Dumpty is licensed under the MIT license, which can be found in [LICENSE](LICENSE).

**Additional Restrictions**

 * We only accept code that is compatible with the MIT license (essentially, BSD and Public Domain).
 * Copying copy-left (GPL-style) code is strictly forbidden.
