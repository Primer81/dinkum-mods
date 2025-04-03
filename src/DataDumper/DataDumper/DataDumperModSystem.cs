﻿using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.Client.Gui;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using Vintagestory.Client.NoObf;
using Vintagestory.Client;

namespace DataDumper;

public class DataDumperModSystem : ModSystem
{
    ICoreClientAPI clientApi;

    public override void StartClientSide(ICoreClientAPI api)
    {
        clientApi = api;
        Dump(new HotKeysDump(clientApi.Input.HotKeys));
        Dump(new LoadedGuisDump(clientApi.Gui.LoadedGuis));
    }

    private void Dump(object data)
    {
        clientApi.StoreModConfig(
            data,
            Path.Combine(
                $"{nameof(DataDumper)}",
                $"{data.GetType().Name}.json"));
    }
}

class HotKeyDump
{
    public string Name;
    public string Code;
    public string DefaultMapping;
    public bool TriggerOnUpAlso;

    public HotKeyDump(HotKey hotKey)
    {
        Name = hotKey.Name;
        Code = hotKey.Code;
        DefaultMapping = hotKey.DefaultMapping.ToString();
        TriggerOnUpAlso = hotKey.TriggerOnUpAlso;
    }
}

class HotKeysDump
{
    public List<HotKeyDump> HotKeys = new List<HotKeyDump>();

    public HotKeysDump(OrderedDictionary<string, HotKey> hotKeys)
    {
        foreach (var hotKey in  hotKeys)
        {
            HotKeys.Add(new HotKeyDump(hotKey.Value));
        }
    }
}

class GuiDialogDump
{
    public string DebugName;
    public string DialogType;
    public bool DisableMouseGrab;
    public double DrawOrder;
    public bool Focusable;
    public double InputOrder;
    public bool PrefersUngrabbedMouse;
    public string ToggleKeyCombinationCode;
    public bool UnregisterOnClose;
    public float ZSize;

    public GuiDialogDump(GuiDialog guiDialog)
    {
        DebugName = guiDialog.DebugName;
        DialogType = guiDialog.DialogType.ToString();
        DisableMouseGrab = guiDialog.DisableMouseGrab;
        DrawOrder = guiDialog.DrawOrder;
        Focusable = guiDialog.Focusable;
        InputOrder = guiDialog.InputOrder;
        PrefersUngrabbedMouse = guiDialog.PrefersUngrabbedMouse;
        ToggleKeyCombinationCode = guiDialog.ToggleKeyCombinationCode;
        UnregisterOnClose = guiDialog.UnregisterOnClose;
        ZSize = guiDialog.ZSize;
    }
}

class LoadedGuisDump
{
    public List<GuiDialogDump> LoadedGuis = new List<GuiDialogDump>();

    public LoadedGuisDump(List<GuiDialog> loadedGuis)
    {
        foreach (var loadedGui in loadedGuis)
        {
            LoadedGuis.Add(new GuiDialogDump(loadedGui));
        }
    }
}

class GuiComposerDump
{
    public bool Composed;
    public string DialogName;
    public bool Enabled;
    public string MouseOverCursor;
    public bool Tabbable;
    public ElementBounds Bounds;
    public ElementBounds CurParentBounds;
    public int CurrentElementKey;
    public string CurrentTabIndexElement;
    public string FirstTabbableElement;
    public string LastAddedElement;
    public ElementBounds LastAddedElementBounds;
    public int MaxTabIndex;

    public GuiComposerDump(GuiComposer guiComposer)
    {
        Composed = guiComposer.Composed;
        DialogName = guiComposer.DialogName;
        Enabled = guiComposer.Enabled;
        MouseOverCursor = guiComposer.MouseOverCursor;
        Tabbable = guiComposer.Tabbable;
        Bounds = guiComposer.Bounds;
        CurParentBounds = guiComposer.CurParentBounds;
        CurrentElementKey = guiComposer.CurrentElementKey;
        CurrentTabIndexElement = guiComposer.CurrentTabIndexElement.ToString();
        FirstTabbableElement = guiComposer.FirstTabbableElement.ToString();
        LastAddedElement = guiComposer.LastAddedElement.ToString();
        LastAddedElementBounds = guiComposer.LastAddedElementBounds;
        MaxTabIndex = guiComposer.MaxTabIndex;
    }
}

class ComposersDump
{
    Dictionary<string, GuiComposerDump> Composers =
        new Dictionary<string, GuiComposerDump>();

    public ComposersDump(Dictionary<string, GuiComposer> composers)
    {
        foreach (var composer in composers)
        {
            Composers.Add(composer.Key, new GuiComposerDump(composer.Value));
        }
    }
}