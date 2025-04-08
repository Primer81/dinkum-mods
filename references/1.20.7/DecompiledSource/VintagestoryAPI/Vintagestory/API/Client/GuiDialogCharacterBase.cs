using System;
using System.Collections.Generic;

namespace Vintagestory.API.Client;

public abstract class GuiDialogCharacterBase : GuiDialog
{
	public abstract List<GuiTab> Tabs { get; }

	public abstract List<Action<GuiComposer>> RenderTabHandlers { get; }

	public abstract event Action ComposeExtraGuis;

	public abstract event Action<int> TabClicked;

	public GuiDialogCharacterBase(ICoreClientAPI capi)
		: base(capi)
	{
	}

	public virtual void OnTitleBarClose()
	{
		TryClose();
	}
}
