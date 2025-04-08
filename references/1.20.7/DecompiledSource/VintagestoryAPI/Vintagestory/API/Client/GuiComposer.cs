using System;
using System.Collections.Generic;
using Cairo;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.Client;

/// <summary>
/// Composes a dialog which are made from a set of elements
/// The composed dialog is cached, so to recompose you have to Recompose All elements or instantiate a new composer with doCache set to false
/// The caching allows the dialog using the composer to not worry about performance and just call compose whenever it has to display a new composed dialog
/// You add components by chaining the functions of the composer together for building the result.
/// </summary>
public class GuiComposer : IDisposable
{
	public Action<bool> OnFocusChanged;

	public static int Outlines;

	internal IGuiComposerManager composerManager;

	internal Dictionary<string, GuiElement> staticElements = new Dictionary<string, GuiElement>();

	internal Dictionary<string, GuiElement> interactiveElements = new Dictionary<string, GuiElement>();

	protected List<GuiElement> interactiveElementsInDrawOrder = new List<GuiElement>();

	protected int currentElementKey;

	protected int currentFocusableElementKey;

	public string DialogName;

	protected LoadedTexture staticElementsTexture;

	protected ElementBounds bounds;

	protected Stack<ElementBounds> parentBoundsForNextElement = new Stack<ElementBounds>();

	protected Stack<bool> conditionalAdds = new Stack<bool>();

	protected ElementBounds lastAddedElementBounds;

	protected GuiElement lastAddedElement;

	public bool Composed;

	internal bool recomposeOnRender;

	internal bool onlyDynamicRender;

	internal ElementBounds InsideClipBounds;

	public ICoreClientAPI Api;

	public float zDepth = 50f;

	private bool premultipliedAlpha = true;

	public Vec4f Color;

	internal bool IsCached;

	/// <summary>
	/// Whether or not the Tab-Key down event should be used and consumed to cycle-focus individual gui elements
	/// </summary>
	public bool Tabbable = true;

	public bool Enabled = true;

	private bool renderFocusHighlight;

	public string MouseOverCursor;

	public ElementBounds LastAddedElementBounds => lastAddedElementBounds;

	public ElementBounds CurParentBounds => parentBoundsForNextElement.Peek();

	/// <summary>
	/// A unique number assigned to each element
	/// </summary>
	public int CurrentElementKey => currentElementKey;

	public GuiElement LastAddedElement => lastAddedElement;

	/// <summary>
	/// Retrieve gui element by key. Returns null if not found.
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public GuiElement this[string key]
	{
		get
		{
			GuiElement elem = null;
			if (!interactiveElements.TryGetValue(key, out elem))
			{
				staticElements.TryGetValue(key, out elem);
			}
			return elem;
		}
	}

	public ElementBounds Bounds => bounds;

	/// <summary>
	/// Gets the currently tabbed index element, if there is one currently focused.
	/// </summary>
	public GuiElement CurrentTabIndexElement
	{
		get
		{
			foreach (GuiElement element in interactiveElements.Values)
			{
				if (element.Focusable && element.HasFocus)
				{
					return element;
				}
			}
			return null;
		}
	}

	public GuiElement FirstTabbableElement
	{
		get
		{
			foreach (GuiElement element in interactiveElements.Values)
			{
				if (element.Focusable)
				{
					return element;
				}
			}
			return null;
		}
	}

	/// <summary>
	/// Gets the maximum tab index of the components.
	/// </summary>
	public int MaxTabIndex
	{
		get
		{
			int tabIndex = -1;
			foreach (GuiElement element in interactiveElements.Values)
			{
				if (element.Focusable)
				{
					tabIndex = Math.Max(tabIndex, element.TabIndex);
				}
			}
			return tabIndex;
		}
	}

	/// <summary>
	/// Triggered when the gui scale changed or the game window was resized
	/// </summary>
	public event Action OnComposed;

	internal GuiComposer(ICoreClientAPI api, ElementBounds bounds, string dialogName)
	{
		staticElementsTexture = new LoadedTexture(api);
		DialogName = dialogName;
		this.bounds = bounds;
		Api = api;
		parentBoundsForNextElement.Push(bounds);
	}

	/// <summary>
	/// Creates an empty GuiComposer.
	/// </summary>
	/// <param name="api">The Client API</param>
	/// <returns>An empty GuiComposer.</returns>
	public static GuiComposer CreateEmpty(ICoreClientAPI api)
	{
		return new GuiComposer(api, ElementBounds.Empty, null).Compose();
	}

	/// <summary>
	/// On by default, is passed on to the gui elements as well. Disabling it means has a performance impact. Recommeded to leave enabled, but may need to be disabled to smoothly alpha blend text elements. Must be called before adding elements and before composing.
	/// Notice! Most gui elements even yet support non-premul alpha mode
	/// </summary>
	/// <param name="enable"></param>
	/// <returns></returns>
	public GuiComposer PremultipliedAlpha(bool enable)
	{
		premultipliedAlpha = enable;
		return this;
	}

	/// <summary>
	/// Adds a condition for adding a group of items to the GUI- eg: if you have a crucible in the firepit, add those extra slots.  Should always pair with an EndIf()
	/// </summary>
	/// <param name="condition">When the following slots should be added</param>
	public GuiComposer AddIf(bool condition)
	{
		conditionalAdds.Push(condition);
		return this;
	}

	/// <summary>
	/// End of the AddIf block.
	/// </summary>
	public GuiComposer EndIf()
	{
		if (conditionalAdds.Count > 0)
		{
			conditionalAdds.Pop();
		}
		return this;
	}

	/// <summary>
	/// Runs given method
	/// </summary>
	/// <param name="method"></param>
	/// <returns></returns>
	public GuiComposer Execute(Action method)
	{
		if (conditionalAdds.Count > 0 && !conditionalAdds.Peek())
		{
			return this;
		}
		method();
		return this;
	}

	/// <summary>
	/// Starts a set of child elements.
	/// </summary>
	/// <param name="bounds">The bounds for the child elements.</param>
	public GuiComposer BeginChildElements(ElementBounds bounds)
	{
		if (conditionalAdds.Count > 0 && !conditionalAdds.Peek())
		{
			return this;
		}
		parentBoundsForNextElement.Peek().WithChild(bounds);
		parentBoundsForNextElement.Push(bounds);
		string key = "element-" + ++currentElementKey;
		staticElements.Add(key, new GuiElementParent(Api, bounds));
		return this;
	}

	/// <summary>
	/// Starts a set of child elements.
	/// </summary>
	public GuiComposer BeginChildElements()
	{
		if (conditionalAdds.Count > 0 && !conditionalAdds.Peek())
		{
			return this;
		}
		parentBoundsForNextElement.Push(lastAddedElementBounds);
		return this;
	}

	/// <summary>
	/// End of the current set of child elements.
	/// </summary>
	public GuiComposer EndChildElements()
	{
		if (conditionalAdds.Count > 0 && !conditionalAdds.Peek())
		{
			return this;
		}
		if (parentBoundsForNextElement.Count > 1)
		{
			parentBoundsForNextElement.Pop();
		}
		return this;
	}

	/// <summary>
	/// Sets the render to Dynamic components only
	/// </summary>
	public GuiComposer OnlyDynamic()
	{
		onlyDynamicRender = true;
		return this;
	}

	/// <summary>
	/// Rebuilds the Composed GUI.  
	/// </summary>
	public void ReCompose()
	{
		Composed = false;
		Compose(focusFirstElement: false);
	}

	internal void UnFocusElements()
	{
		composerManager.UnfocusElements();
		OnFocusChanged?.Invoke(obj: false);
	}

	/// <summary>
	/// marks an element as in focus.  
	/// </summary>
	/// <param name="tabIndex">The tab index to focus at.</param>
	/// <returns>Whether or not the focus could be done.</returns>
	public bool FocusElement(int tabIndex)
	{
		GuiElement newFocusedElement = null;
		foreach (GuiElement element in interactiveElements.Values)
		{
			if (element.Focusable && element.TabIndex == tabIndex)
			{
				newFocusedElement = element;
				break;
			}
		}
		if (newFocusedElement != null)
		{
			UnfocusOwnElementsExcept(newFocusedElement);
			newFocusedElement.OnFocusGained();
			OnFocusChanged?.Invoke(obj: true);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Unfocuses the elements within this GUI composer.
	/// </summary>
	public void UnfocusOwnElements()
	{
		UnfocusOwnElementsExcept(null);
	}

	/// <summary>
	/// Unfocuses all elements except one specific element.
	/// </summary>
	/// <param name="elem">The element to remain in focus.</param>
	public void UnfocusOwnElementsExcept(GuiElement elem)
	{
		foreach (GuiElement element in interactiveElements.Values)
		{
			if (element != elem && element.Focusable && element.HasFocus)
			{
				element.OnFocusLost();
				OnFocusChanged?.Invoke(obj: false);
			}
		}
	}

	/// <summary>
	/// Tells the composer to compose the gui.
	/// </summary>
	/// <param name="focusFirstElement">Whether or not to put the first element in focus.</param>
	public GuiComposer Compose(bool focusFirstElement = true)
	{
		if (Composed)
		{
			if (focusFirstElement && MaxTabIndex >= 0)
			{
				FocusElement(0);
			}
			return this;
		}
		foreach (GuiElement value in staticElements.Values)
		{
			value.BeforeCalcBounds();
		}
		bounds.Initialized = false;
		try
		{
			bounds.CalcWorldBounds();
		}
		catch (Exception e)
		{
			Api.Logger.Error("Exception thrown when trying to calculate world bounds for gui composite " + DialogName + ":");
			Api.Logger.Error(e);
		}
		bounds.IsDrawingSurface = true;
		int wdt = (int)bounds.OuterWidth;
		int hgt = (int)bounds.OuterHeight;
		if (staticElementsTexture.TextureId != 0)
		{
			wdt = Math.Max(wdt, staticElementsTexture.Width);
			hgt = Math.Max(hgt, staticElementsTexture.Height);
		}
		ImageSurface surface = new ImageSurface(Format.Argb32, wdt, hgt);
		Context ctx = new Context(surface);
		ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.0);
		ctx.Paint();
		ctx.Antialias = Antialias.Best;
		foreach (GuiElement value2 in staticElements.Values)
		{
			value2.ComposeElements(ctx, surface);
		}
		interactiveElementsInDrawOrder.Clear();
		foreach (GuiElement element in interactiveElements.Values)
		{
			int insertPos = 0;
			foreach (GuiElement addedElem in interactiveElementsInDrawOrder)
			{
				if (element.DrawOrder >= addedElem.DrawOrder)
				{
					insertPos++;
					continue;
				}
				break;
			}
			interactiveElementsInDrawOrder.Insert(insertPos, element);
		}
		if (!premultipliedAlpha)
		{
			surface.DemulAlpha();
		}
		Api.Gui.LoadOrUpdateCairoTexture(surface, linearMag: true, ref staticElementsTexture);
		ctx.Dispose();
		surface.Dispose();
		Composed = true;
		if (focusFirstElement && MaxTabIndex >= 0)
		{
			FocusElement(0);
		}
		this.OnComposed?.Invoke();
		return this;
	}

	/// <summary>
	/// Fires the OnMouseUp events.
	/// </summary>
	/// <param name="mouse">The mouse information.</param>
	public void OnMouseUp(MouseEvent mouse)
	{
		if (!Enabled)
		{
			return;
		}
		foreach (GuiElement value in interactiveElements.Values)
		{
			value.OnMouseUp(Api, mouse);
		}
	}

	/// <summary>
	/// Fires the OnMouseDown events.
	/// </summary>
	/// <param name="mouseArgs">The mouse information.</param>
	public void OnMouseDown(MouseEvent mouseArgs)
	{
		if (!Enabled)
		{
			return;
		}
		bool beforeHandled = false;
		bool nowHandled = false;
		renderFocusHighlight = false;
		foreach (GuiElement element in interactiveElements.Values)
		{
			if (!beforeHandled)
			{
				element.OnMouseDown(Api, mouseArgs);
				nowHandled = mouseArgs.Handled;
			}
			if (!beforeHandled && nowHandled)
			{
				if (element.Focusable && !element.HasFocus)
				{
					element.OnFocusGained();
					if (element.HasFocus)
					{
						OnFocusChanged?.Invoke(obj: true);
					}
				}
			}
			else if (element.Focusable && element.HasFocus)
			{
				element.OnFocusLost();
			}
			beforeHandled = nowHandled;
		}
	}

	/// <summary>
	/// Fires the OnMouseMove events.
	/// </summary>
	/// <param name="mouse">The mouse information.</param>
	public void OnMouseMove(MouseEvent mouse)
	{
		if (!Enabled)
		{
			return;
		}
		foreach (GuiElement value in interactiveElements.Values)
		{
			value.OnMouseMove(Api, mouse);
			if (mouse.Handled)
			{
				break;
			}
		}
	}

	public bool OnMouseEnterSlot(ItemSlot slot)
	{
		if (!Enabled)
		{
			return false;
		}
		foreach (GuiElement value in interactiveElements.Values)
		{
			if (value.OnMouseEnterSlot(Api, slot))
			{
				return true;
			}
		}
		return false;
	}

	public bool OnMouseLeaveSlot(ItemSlot slot)
	{
		if (!Enabled)
		{
			return false;
		}
		foreach (GuiElement value in interactiveElements.Values)
		{
			if (value.OnMouseLeaveSlot(Api, slot))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Fires the OnMouseWheel events.
	/// </summary>
	/// <param name="mouse">The mouse wheel information.</param>
	public void OnMouseWheel(MouseWheelEventArgs mouse)
	{
		if (!Enabled)
		{
			return;
		}
		foreach (KeyValuePair<string, GuiElement> interactiveElement in interactiveElements)
		{
			GuiElement element = interactiveElement.Value;
			if (element.IsPositionInside(Api.Input.MouseX, Api.Input.MouseY))
			{
				element.OnMouseWheel(Api, mouse);
			}
			if (mouse.IsHandled)
			{
				return;
			}
		}
		foreach (GuiElement value in interactiveElements.Values)
		{
			value.OnMouseWheel(Api, mouse);
			if (mouse.IsHandled)
			{
				break;
			}
		}
	}

	/// <summary>
	/// Fires the OnKeyDown events.
	/// </summary>
	/// <param name="args">The keyboard information.</param>
	/// <param name="haveFocus">Whether or not the gui has focus.</param>
	public void OnKeyDown(KeyEvent args, bool haveFocus)
	{
		if (!Enabled)
		{
			return;
		}
		foreach (GuiElement value in interactiveElements.Values)
		{
			value.OnKeyDown(Api, args);
			if (args.Handled)
			{
				break;
			}
		}
		if (haveFocus && !args.Handled && args.KeyCode == 52 && Tabbable)
		{
			renderFocusHighlight = true;
			GuiElement elem = CurrentTabIndexElement;
			if (elem != null && MaxTabIndex > 0)
			{
				int dir = ((!args.ShiftPressed) ? 1 : (-1));
				int tb = GameMath.Mod(elem.TabIndex + dir, MaxTabIndex + 1);
				FocusElement(tb);
				args.Handled = true;
			}
			else if (MaxTabIndex > 0)
			{
				FocusElement(args.ShiftPressed ? GameMath.Mod(-1, MaxTabIndex + 1) : 0);
				args.Handled = true;
			}
		}
		if (!args.Handled && (args.KeyCode == 49 || args.KeyCode == 82) && CurrentTabIndexElement is GuiElementEditableTextBase)
		{
			UnfocusOwnElementsExcept(null);
		}
	}

	/// <summary>
	/// Fires the OnKeyDown events.
	/// </summary>
	/// <param name="args">The keyboard information.</param>
	public void OnKeyUp(KeyEvent args)
	{
		if (!Enabled)
		{
			return;
		}
		foreach (GuiElement value in interactiveElements.Values)
		{
			value.OnKeyUp(Api, args);
			if (args.Handled)
			{
				break;
			}
		}
	}

	/// <summary>
	/// Fires the OnKeyPress event.
	/// </summary>
	/// <param name="args">The keyboard information</param>
	public void OnKeyPress(KeyEvent args)
	{
		if (!Enabled)
		{
			return;
		}
		foreach (GuiElement value in interactiveElements.Values)
		{
			value.OnKeyPress(Api, args);
			if (args.Handled)
			{
				break;
			}
		}
	}

	public void Clear(ElementBounds newBounds)
	{
		foreach (KeyValuePair<string, GuiElement> interactiveElement in interactiveElements)
		{
			interactiveElement.Value.Dispose();
		}
		foreach (KeyValuePair<string, GuiElement> staticElement in staticElements)
		{
			staticElement.Value.Dispose();
		}
		interactiveElements.Clear();
		interactiveElementsInDrawOrder.Clear();
		staticElements.Clear();
		conditionalAdds.Clear();
		parentBoundsForNextElement.Clear();
		bounds = newBounds;
		if (bounds.ParentBounds == null)
		{
			bounds.ParentBounds = Api.Gui.WindowBounds;
		}
		parentBoundsForNextElement.Push(bounds);
		lastAddedElementBounds = null;
		lastAddedElement = null;
		Composed = false;
	}

	/// <summary>
	/// Fires the PostRender event.
	/// </summary>
	/// <param name="deltaTime">The change in time.</param>
	public void PostRender(float deltaTime)
	{
		if (!Enabled || Api.Render.FrameWidth == 0 || Api.Render.FrameHeight == 0)
		{
			return;
		}
		if (bounds.ParentBounds.RequiresRecalculation)
		{
			Api.Logger.Notification("Window probably resized, recalculating dialog bounds and recomposing " + DialogName + "...");
			bounds.MarkDirtyRecursive();
			bounds.ParentBounds.CalcWorldBounds();
			if (bounds.ParentBounds.OuterWidth == 0.0 || bounds.ParentBounds.OuterHeight == 0.0)
			{
				return;
			}
			bounds.CalcWorldBounds();
			ReCompose();
		}
		foreach (GuiElement item in interactiveElementsInDrawOrder)
		{
			item.PostRenderInteractiveElements(deltaTime);
		}
	}

	/// <summary>
	/// Fires the render event.
	/// </summary>
	/// <param name="deltaTime">The change in time.</param>
	public void Render(float deltaTime)
	{
		if (!Enabled)
		{
			return;
		}
		if (recomposeOnRender)
		{
			ReCompose();
			recomposeOnRender = false;
		}
		if (!onlyDynamicRender)
		{
			int wdt = Math.Max(bounds.OuterWidthInt, staticElementsTexture.Width);
			int hgt = Math.Max(bounds.OuterHeightInt, staticElementsTexture.Height);
			Api.Render.Render2DTexture(staticElementsTexture.TextureId, (int)bounds.renderX, (int)bounds.renderY, wdt, hgt, zDepth, Color);
		}
		MouseOverCursor = null;
		foreach (GuiElement element2 in interactiveElementsInDrawOrder)
		{
			element2.RenderInteractiveElements(deltaTime);
			if (element2.IsPositionInside(Api.Input.MouseX, Api.Input.MouseY))
			{
				MouseOverCursor = element2.MouseOverCursor;
			}
		}
		foreach (GuiElement element in interactiveElementsInDrawOrder)
		{
			if (element.HasFocus && renderFocusHighlight)
			{
				element.RenderFocusOverlay(deltaTime);
			}
		}
		if (Outlines == 1)
		{
			Api.Render.RenderRectangle((int)bounds.renderX, (int)bounds.renderY, 500f, (int)bounds.OuterWidth, (int)bounds.OuterHeight, -1);
			foreach (GuiElement value in staticElements.Values)
			{
				value.RenderBoundsDebug();
			}
		}
		if (Outlines != 2)
		{
			return;
		}
		foreach (GuiElement value2 in interactiveElements.Values)
		{
			value2.RenderBoundsDebug();
		}
	}

	internal static double scaled(double value)
	{
		return value * (double)RuntimeEnv.GUIScale;
	}

	/// <summary>
	/// Adds an interactive element to the composer.
	/// </summary>
	/// <param name="element">The element to add.</param>
	/// <param name="key">The name of the element. (default: null)</param>
	public GuiComposer AddInteractiveElement(GuiElement element, string key = null)
	{
		if (conditionalAdds.Count > 0 && !conditionalAdds.Peek())
		{
			return this;
		}
		element.RenderAsPremultipliedAlpha = premultipliedAlpha;
		if (key == null)
		{
			int num = ++currentElementKey;
			key = "element-" + num;
		}
		interactiveElements.Add(key, element);
		staticElements.Add(key, element);
		if (element.Focusable)
		{
			element.TabIndex = currentFocusableElementKey++;
		}
		else
		{
			element.TabIndex = -1;
		}
		element.InsideClipBounds = InsideClipBounds;
		if (parentBoundsForNextElement.Peek() == element.Bounds)
		{
			throw new ArgumentException($"Fatal: Attempting to add a self referencing bounds->child bounds reference. This would cause a stack overflow. Make sure you don't re-use the same bounds for a parent and child element (key {key})");
		}
		parentBoundsForNextElement.Peek().WithChild(element.Bounds);
		lastAddedElementBounds = element.Bounds;
		lastAddedElement = element;
		return this;
	}

	/// <summary>
	/// Adds a static element to the composer.
	/// </summary>
	/// <param name="element">The element to add.</param>
	/// <param name="key">The name of the element (default: null)</param>
	public GuiComposer AddStaticElement(GuiElement element, string key = null)
	{
		if (conditionalAdds.Count > 0 && !conditionalAdds.Peek())
		{
			return this;
		}
		element.RenderAsPremultipliedAlpha = premultipliedAlpha;
		if (key == null)
		{
			int num = ++currentElementKey;
			key = "element-" + num;
		}
		staticElements.Add(key, element);
		parentBoundsForNextElement.Peek().WithChild(element.Bounds);
		lastAddedElementBounds = element.Bounds;
		lastAddedElement = element;
		element.InsideClipBounds = InsideClipBounds;
		return this;
	}

	/// <summary>
	/// Gets the element by name.
	/// </summary>
	/// <param name="key">The name of the element to get.</param>
	public GuiElement GetElement(string key)
	{
		if (interactiveElements.ContainsKey(key))
		{
			return interactiveElements[key];
		}
		if (staticElements.ContainsKey(key))
		{
			return staticElements[key];
		}
		return null;
	}

	public void Dispose()
	{
		foreach (KeyValuePair<string, GuiElement> interactiveElement in interactiveElements)
		{
			interactiveElement.Value.Dispose();
		}
		foreach (KeyValuePair<string, GuiElement> staticElement in staticElements)
		{
			staticElement.Value.Dispose();
		}
		staticElementsTexture.Dispose();
		Composed = false;
		lastAddedElement = null;
	}
}
