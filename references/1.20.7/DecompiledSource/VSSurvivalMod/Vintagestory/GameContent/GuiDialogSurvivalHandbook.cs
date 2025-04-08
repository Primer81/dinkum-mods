using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Config;

namespace Vintagestory.GameContent;

public class GuiDialogSurvivalHandbook : GuiDialogHandbook
{
	public override string DialogTitle => Lang.Get("Survival Handbook");

	public GuiDialogSurvivalHandbook(ICoreClientAPI capi, OnCreatePagesDelegate createPageHandlerAsync, OnComposePageDelegate composePageHandler)
		: base(capi, createPageHandlerAsync, composePageHandler)
	{
		setupReloadCommand(capi);
		capi.Event.HotkeysChanged += loadEntries;
		currentCatgoryCode = capi.Settings.String["currentSurvivalHandbookCategoryCode"];
	}

	protected virtual void setupReloadCommand(ICoreClientAPI capi)
	{
		capi.ChatCommands.GetOrCreate("debug").BeginSub("reloadhandbook").WithDesc("Reload handbook pages")
			.HandleWith(delegate
			{
				capi.Assets.Reload(AssetCategory.config);
				Lang.Load(capi.World.Logger, capi.Assets, capi.Settings.String["language"]);
				loadEntries();
				return TextCommandResult.Success("Lang file and handbook entries now reloaded");
			})
			.EndSub();
	}

	protected override HashSet<string> initCustomPages()
	{
		List<GuiHandbookTextPage> list = (from pair in capi.Assets.GetMany<GuiHandbookTextPage>(capi.Logger, "config/handbook")
			orderby pair.Key.ToString()
			select pair.Value).ToList();
		HashSet<string> categoryCodes = new HashSet<string>();
		foreach (GuiHandbookTextPage page2 in list)
		{
			page2.Init(capi);
			allHandbookPages.Add(page2);
		}
		capi.ModLoader.GetModSystem<ModSystemSurvivalHandbook>().TriggerOnInitCustomPages(allHandbookPages);
		for (int i = 0; i < allHandbookPages.Count; i++)
		{
			GuiHandbookPage page = allHandbookPages[i];
			categoryCodes.Add(page.CategoryCode);
			pageNumberByPageCode[page.PageCode] = i;
		}
		return categoryCodes;
	}

	protected override GuiTab[] genTabs(out int curTab)
	{
		List<GuiTab> tabs = new List<GuiTab>();
		tabs.Add(new HandbookTab
		{
			DataInt = 0,
			Name = Lang.Get("handbook-category-tutorials"),
			CategoryCode = "tutorial"
		});
		tabs.Add(new HandbookTab
		{
			PaddingTop = 20.0,
			DataInt = 0,
			Name = Lang.Get("handbook-category-everything"),
			CategoryCode = null
		});
		curTab = ((!(currentCatgoryCode == "tutorial")) ? 1 : 0);
		categoryCodes.Remove("tutorial");
		for (int i = 0; i < categoryCodes.Count; i++)
		{
			int index = tabs.Count;
			tabs.Add(new HandbookTab
			{
				DataInt = index,
				Name = Lang.Get("handbook-category-" + categoryCodes[i]),
				CategoryCode = categoryCodes[i]
			});
			if (currentCatgoryCode == categoryCodes[i])
			{
				curTab = index;
			}
		}
		return tabs.ToArray();
	}
}
