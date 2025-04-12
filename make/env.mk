ifndef DINKUM_INSTALL
    ifeq ($(OS),Windows_NT)
        PROGRAMFILES_X86:=$(shell powershell -NoProfile -Command '$${env:ProgramFiles(x86)}')
        ifeq ($(PROGRAMFILES_X86),)
            $(error PROGRAMFILES_X86 is undefined. Needed to define DINKUM_INSTALL)
        endif
        export STEAM_PATH=$(PROGRAMFILES_X86)/Steam
        export STEAM_COMMON=$(STEAM_PATH)/steamapps/common
    else
        ifeq ($(HOME),)
            $(error HOME is undefined. Needed to define DINKUM_INSTALL)
        endif
        UNAME_S=$(shell uname -s)
        ifeq ($(UNAME_S),Darwin)
            # macOS
            export STEAM_PATH=$(HOME)/Library/Application\ Support/Steam
            export STEAM_COMMON=$(STEAM_PATH)/steamapps/common
        else
            # Linux
            export STEAM_PATH=$(HOME)/.local/share/Steam
            export STEAM_COMMON=$(STEAM_PATH)/steamapps/common
        endif
    endif
    export DINKUM_INSTALL=$(STEAM_COMMON)/Dinkum
endif
export DINKUM_INSTALL:=$(subst \,/,$(DINKUM_INSTALL))

PYTHON_CHECK := $(shell which python3 2>/dev/null || which python 2>/dev/null)
$(if $(PYTHON_CHECK),,$(error Python is not installed. Please install Python before continuing.))