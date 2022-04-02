# Launcher

*This breakdown is subject to change as the project progresses.*

Made with [Windows Presentation Foundation](https://en.wikipedia.org/wiki/Windows_Presentation_Foundation), this project aims the following:

• Be standalone. There should be no dependencies needed on the system (except .NET) and any would be embedded as libraries/assemblies or in sub-directories of the application path.

• Be light. The launcher should hardly go beyond 20MB all things (repository tools, theme assets, logs, configs, binaries) considered.

• Provide control over the install location.

**[Versioning]**

  - Provide interfacing for various versioning systems (Subversion, Git).
  - Allow checking out a repository or switch revisions.
  - Provide formatted project changelogs, download and update logs.
  - Provide a cleanup process for exceptional errors bricking the working copies.

• Provide full control over themes/styling.

• Provide full control over the purpose of the application (changing title, icon, repository or even versioning system).


Any available setting can be stored in a configuration file (Options.config).

# To Do

- Abstract Subversion calls.
- Implement Git interface.
- Implement full theme support.
- Implement download/update logs formatting (can use colors from themes).
- Change logs box display process from converting `StringBuidler` to setting and/or appending the document's runs. Should increase performance and fix weird threading issues.
- Fix update process not displaying/doing anything.
- Implement cleanup process.