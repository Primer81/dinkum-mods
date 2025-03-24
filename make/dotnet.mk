.PHONY: dotnet-vsmod-search
dotnet-vsmod-search:
	$(DOTNET) new search vsmod

.PHONY: dotnet-vsmod-uninstall
dotnet-vsmod-uninstall:
	$(DOTNET) new uninstall $(DOTNET_VSMOD_PACKAGE_NAME)
	rm -f $(DOTNET_VSMOD_INSTALL_SENTINEL)

.PHONY: dotnet-vsmod-install
dotnet-vsmod-install: $(DOTNET_VSMOD_INSTALL_SENTINEL)
$(DOTNET_VSMOD_INSTALL_SENTINEL):
	$(shell mkdir -p $(dir $@))
	$(DOTNET) new install $(DOTNET_VSMOD_PACKAGE_NAME)
	touch $@