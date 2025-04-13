.PHONY: dinkum-save-backup-all
dinkum-save-backup-all:
	$(shell mkdir -p $(BACKUP_DINKUM))
	cp -r "$(DINKUM_SAVES)"/* "$(BACKUP_DINKUM)"