###############################################################################
# Common Definitions
###############################################################################
define newline


endef

###############################################################################
# Common Functions
###############################################################################
read_json=$(shell python -c "import json; print(json.load(open('$(1)'))['$(2)'])" 2>/dev/null)
read_xml=$(shell python -c "import xml.etree.ElementTree as ET; print(elem.text ET.parse('$(1)').getroot().find('.//$(2)').text)" 2>/dev/null)
define zip
    python -c "import shutil; shutil.make_archive('$(if $(2),$(2),$(basename $(1)))', 'zip', '$(1)')"
endef
lowercase=$(shell echo $(1) | tr A-Z a-z)

###############################################################################
# Images
###############################################################################
IMG_DIR=img
IMG_PROFILE_PICTURE=$(IMG_DIR)/Misc/ProfilePicture.png

###############################################################################
# Sentinels
###############################################################################
SENTINEL_DIR=sentinels
SENTINEL_TMP_DIR=$(SENTINEL_DIR)/tmp
SENTINEL_EXT=.sentinel

###############################################################################
# Dotnet
###############################################################################
## Configuration
export DOTNET?=dotnet
export DOTNET_BUILD?=dotnet build
export DOTNET_CLEAN?=dotnet clean
export DOTNET_RUN?=dotnet run
export DOTNET_PUBLISH?=dotnet publish

###############################################################################
# Dotnet vsmod
###############################################################################
## Configuration
export DOTNET_VSMOD_PACKAGE_NAME?=VintageStory.Mod.Templates
## Definitions
DOTNET_VSMOD=$(DOTNET) new vsmod
DOTNET_VSMOD_INSTALL_SENTINEL=\
    $(SENTINEL_TMP_DIR)/dotnet-vsmod-install$(SENTINEL_EXT)

###############################################################################
# Dotnet bepinex
###############################################################################
## Configuration (reference: https://github.com/BepInEx/BepInEx.Templates/blob/master/README.md)
export DOTNET_BEPINEX_SOURCE?=https://nuget.bepinex.dev/v3/index.json
export DOTNET_BEPINEX_PACKAGE_NAME?=BepInEx.Templates
## Definitions
DOTNET_BEPINEX=$(DOTNET) new bepinex5plugin
DOTNET_BEPINEX_INSTALL_SENTINEL=\
    $(SENTINEL_TMP_DIR)/dotnet-bepinex-install$(SENTINEL_EXT)

###############################################################################
# Dotnet ilspycmd
###############################################################################
## Functions
ilspycmd_add_dll=\
    $(eval DOTNET_ILSPYCMD_TARGETS+="$(1)")\
    $(eval DOTNET_ILSPYCMD_TARGETS_INDEX+=\
        $(shell echo $$(( $(words $(DOTNET_ILSPYCMD_TARGETS_INDEX)) + $(words $(1)) ))))
## Definitions
### Targets
DOTNET_ILSPYCMD_TARGETS=
DOTNET_ILSPYCMD_TARGETS_INDEX=
$(call ilspycmd_add_dll,$(DINKUM_INSTALL)/Dinkum_Data/Managed/Assembly-CSharp.dll)
### Commands
DOTNET_ILSPYCMD=ilspycmd
DOTNET_ILSPYCMD_INSTALL_SENTINEL=\
    $(SENTINEL_TMP_DIR)/dotnet-ilspycmd-install$(SENTINEL_EXT)
### Directories
DOTNET_ILSPYCMD_OUTPUT_DIR=references/$(DOTNET_ILSPYCMD_OUTPUT_VERSION)/DecompiledSource
### Recipes
DOTNET_ILSPYCMD_DECOMPILE_PREREQUISITES=\
    | $(DOTNET_ILSPYCMD_INSTALL_SENTINEL)
## Configuration
export DOTNET_ILSPYCMD_PACKAGE_NAME?=ilspycmd
export DOTNET_ILSPYCMD_PACKAGE_VERSION?=8.2
export DOTNET_ILSPYCMD_REFERENCES?=
export DOTNET_ILSPYCMD_OUTPUT_VERSION?=new
export DOTNET_ILSPYCMD_FLAGS?=\
    --project\
    --disable-updatecheck\
    --nested-directories\
    --use-varnames-from-pdb\
    $(foreach ref,$(DOTNET_ILSPYCMD_REFERENCES),\
        -r $(ref)\
    )\
    -o $(DOTNET_ILSPYCMD_OUTPUT_DIR)

###############################################################################
# Project
###############################################################################
## Sources
SRC_DIR=src
## Templates
TEMPLATE_DIR=template
TEMPLATE_GIT_IGNORE=$(TEMPLATE_DIR)/.gitignore
TEMPLATE_MOD_INFO_JSON=$(TEMPLATE_DIR)/modinfo.json
TEMPLATE_MOD_SOLUTION_FILE=$(TEMPLATE_DIR)/template-solution-file.sln
TEMPLATE_ASSETS=$(TEMPLATE_DIR)/assets
## Configuration
ifndef name
    ### Defaults
    export PROJECT_NAME_LIST?=$(filter-out DataDumper, $(patsubst $(SRC_DIR)/%,%,$(wildcard $(SRC_DIR)/*)))
    export PROJECT_NAME?=DataDumper
else
    ### Shorthand
    export PROJECT_NAME_LIST?=$(name)
    export PROJECT_NAME?=$(name)
endif
## Definitions
### Directories
#### Common
PROJECT_DIR=$(SRC_DIR)/$(PROJECT_NAME)
PROJECT_RELEASES_DIR=$(PROJECT_DIR)/Releases
#### Mod
PROJECT_SRC_DIR=$(PROJECT_DIR)/$(PROJECT_NAME)
PROJECT_BUILD_DIR=\
    $(PROJECT_SRC_DIR)/bin/$(PROJECT_BUILD_PROFILE)/Mods/mod
PROJECT_CSPROJ_FILE=$(PROJECT_SRC_DIR)/$(PROJECT_NAME).csproj
### Version
PROJECT_MODID=$(call lowercase,$(PROJECT_NAME))
PROJECT_DESCRIPTION=$(call read_xml,$(PROJECT_SRC_DIR)/$(PROJECT_NAME).csproj,Description)
PROJECT_VERSION=$(call read_xml,$(PROJECT_SRC_DIR)/$(PROJECT_NAME).csproj,Version)
ifeq ($(PROJECT_VERSION),)
    PROJECT_VERSION=1.0.0
endif
### Icon
PROJECT_MOD_ICON_DEFAULT=$(IMG_PROFILE_PICTURE)
PROJECT_MOD_ICON=$(PROJECT_SRC_DIR)/modicon.png
### Recipes
#### Create
PROJECT_CREATE_SENTINEL=\
    $(SENTINEL_DIR)/project-create-$(PROJECT_NAME)$(SENTINEL_EXT)
PROJECT_CREATE_PREREQUISITES=\
    | $(DOTNET_BEPINEX_INSTALL_SENTINEL)
#### Build
PROJECT_BUILD_ALL_PREREQUISITES=\
    project-target-mod-all\
    project-target-release-all
PROJECT_BUILD_CLEAN_PREREQUISITES=\
    project-target-mod-clean\
    project-target-release-clean
#### Install
PROJECT_INSTALL_PREREQUISITES=\
    project-target-release-all
#### Run
PROJECT_RUN_PREREQUISITES=\
    project-build-all
#### Target
PROJECT_TARGET_PREREQUISITES=\
    | $(PROJECT_CREATE_SENTINEL)
PROJECT_TARGET_RELEASE=\
    $(PROJECT_RELEASES_DIR)/$(PROJECT_MODID)_$(PROJECT_VERSION).zip