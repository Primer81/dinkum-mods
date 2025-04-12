.PHONY: debug
debug:
	$(foreach v, $(.VARIABLES), $(info $(v) = $($(v))))