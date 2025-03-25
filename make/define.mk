###############################################################################
# Functions
###############################################################################
read_json=$(shell python -c "import json; print(json.load(open('$(1)'))['$(2)'])" 2>/dev/null)

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
DOTNET?=dotnet
DOTNET_VSMOD_PACKAGE_NAME?=VintageStory.Mod.Templates
## Definitions
DOTNET_VSMODE=$(DOTNET) new vsmod
DOTNET_VSMOD_INSTALL_SENTINEL=\
    $(SENTINEL_TMP_DIR)/dotnet-vsmod-install$(SENTINEL_EXT)

###############################################################################
# Project
###############################################################################
## Configuration
PROJECT_NAME?=InvertHotbarScroll
## Definitions
### Directories
#### Common
PROJECT_DIR=src/$(PROJECT_NAME)
PROJECT_RELEASES_DIR=$(PROJECT_DIR)/Releases
#### Cake
PROJECT_CAKE_SRC_DIR=$(PROJECT_DIR)/CakeBuild
PROJECT_CAKE_BUILD_DIR=\
    $(PROJECT_CAKE_BIN_DIR)/$(PROJECT_BUILD_PROFILE)/net7.0
#### Mod
PROJECT_SRC_DIR=$(PROJECT_DIR)/$(PROJECT_NAME)
PROJECT_BUILD_DIR=\
    $(PROJECT_SRC_DIR)/bin/$(PROJECT_BUILD_PROFILE)/Mods/mod
### Version
PROJECT_VERSION=$(call read_json,$(PROJECT_SRC_DIR)/modinfo.json,version)
PROJECT_MODID=$(call read_json,$(PROJECT_SRC_DIR)/modinfo.json,modid)
### Recipes
#### Create
PROJECT_CREATE_SENTINEL=\
    $(SENTINEL_DIR)/project-create-$(PROJECT_NAME)$(SENTINEL_EXT)
PROJECT_CREATE_PREREQUISITES=\
    $(DOTNET_VSMOD_INSTALL_SENTINEL)
#### Build
PROJECT_BUILD_ALL_PREREQUISITES=\
    project-target-cake-all\
    project-target-mod-all\
    project-target-release-all
PROJECT_BUILD_CLEAN_PREREQUISITES=\
    project-target-cake-clean\
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
    $(PROJECT_CREATE_SENTINEL)
PROJECT_TARGET_RELEASE=\
    $(PROJECT_RELEASES_DIR)/$(PROJECT_MODID)_$(PROJECT_VERSION).zip