![Dinkum Mods](img/Misc/VintageStoryModsLogoProfilePicTransparent.png)

## Build Requirements
- Dinkum installed via Steam
- `dotnet` command line tool
- `python` command line tool
- `make` command line tool
- Basic CLI commands are necessary as well including:
    - `cp`
    - `rm`
    - `touch`
    - `mkdir`

## Make commands
Basic `make` commands are defined under `make/default.mk`. Each can be made
to apply to a specific set of projects by adding `PROJECT_NAME_LIST=<names...>`
before the command. These include:
- `all`: Builds debug and release configurations.
- `clean`: Cleans debug and release configurations.
- `rebuild`: Rebuilds debug and release configurations.
- `install`: Installs the release build into the active Dinkum
installation (WIP).
- `uninstall`: Uninstalls the release build from the active Dinkum
installation (WIP).
- `name="<project-name...>"`: Add prior to any other command to specify which
projects to target. Defaults to a list containing all mod projects under
`src` except `DataDumper`.

The following `make` commands can only be used with one project at a time
which can be specified by adding `PROJECT_NAME=<name>` before the command
- `run`: Launches Dinkum with the debug build of the mod loaded in (WIP).
- `name="<project-name>"`: Add prior to any other command to specify which
project to target. Defaults to `DataDumper`.

If any of the specified project names do not yet exist, they will be created
automatically under the `src` directory.

Other basic `make` commands include:
- `decompile`: Decompiles the active Dinkum installation DLL's into the
`references/new` directory using `ilspycmd`.

Advanced project commands are defined under `make/project.mk`. The basic
`make` commands are defined in terms of the more advanced commands. Some
commands do not have a basic equivalent such as `project-run-server`. Refer
to `make/project.mk` for more details.

## Future improvements
- Make it easier to launch Dinkum with multiple development projects
loaded in at once using the `run` command.