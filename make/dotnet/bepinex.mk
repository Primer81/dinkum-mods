###############################################################################
# Install
###############################################################################
.PHONY: dotnet-bepinex-help
dotnet-bepinex-help: $(DOTNET_BEPINEX_INSTALL_SENTINEL)
	$(DOTNET_BEPINEX) --help

.PHONY: dotnet-bepinex-search
dotnet-bepinex-search: $(DOTNET_BEPINEX_INSTALL_SENTINEL)
	$(DOTNET) new search bepinex*

.PHONY: dotnet-bepinex-uninstall
dotnet-bepinex-uninstall:
	$(DOTNET) new uninstall $(DOTNET_BEPINEX_PACKAGE_NAME)
	rm -f $(DOTNET_BEPINEX_INSTALL_SENTINEL)

.PHONY: dotnet-bepinex-install
dotnet-bepinex-install: $(DOTNET_BEPINEX_INSTALL_SENTINEL)
$(DOTNET_BEPINEX_INSTALL_SENTINEL):
	$(shell mkdir -p $(dir $@))
	$(DOTNET) new install $(DOTNET_BEPINEX_PACKAGE_NAME) --nuget-source $(DOTNET_BEPINEX_SOURCE)
	touch $@

.PHONY: dotnet-bepinex-update
dotnet-bepinex-update:
	# The only way to update specific template packages
	# is to uninstall and reinstall. `dotnet new update` does not provide
	# any method to update specific packages.
	$(MAKE) dotnet-bepinex-uninstall
	$(MAKE) dotnet-bepinex-install

###############################################################################
# Debug
###############################################################################
.PHONY: dotnet-bepinex-debug-all
dotnet-bepinex-debug-all:
	$(call write_ini,$(DINKUM_BEPINEX_CONFIG_PATH),Logging.Console,Enabled,true)

.PHONY: dotnet-bepinex-debug-clean
dotnet-bepinex-debug-clean:
	$(call write_ini,$(DINKUM_BEPINEX_CONFIG_PATH),Logging.Console,Enabled,false)

.PHONY: dotnet-bepinex-debug-rebuild
dotnet-bepinex-debug-rebuild:
	$(MAKE) dotnet-bepinex-debug-clean
	$(MAKE) dotnet-bepinex-debug-all