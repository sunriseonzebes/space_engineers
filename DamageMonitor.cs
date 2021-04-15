public struct TSurfaceOption {
    public IMyTextSurface surface;
}

public void Main(string argument) {
    List<TSurfaceOption> LCDs = GetTextSurfaces(argument);
    
    if (LCDs.Count == 0)
        throw new Exception("No Displays Found.");

    List<IMyTerminalBlock> damagedBlocks = GetDamagedBlocks();
    if (damagedBlocks.Count == 0) {
        WriteFancy(LCDs, "\n\n\n\n- No Damage -");
    }
    else {
        string LCDMessage = FormatDamagedList(damagedBlocks);
        WriteFancy(LCDs, LCDMessage);
    }
    
}

public List<TSurfaceOption> GetTextSurfaces(string surfaceTag) {
    List<TSurfaceOption> surfaces = new List<TSurfaceOption>{};
    if (surfaceTag == "Debug") {
        TSurfaceOption debugOption = new TSurfaceOption();
        debugOption.surface = Me.GetSurface(0);
        ConfigureSurface(debugOption.surface);
        surfaces.Add(debugOption);
    }
    else {
        List<IMyTerminalBlock> textPanels = new List<IMyTerminalBlock>{};
        GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(textPanels);
        for (int i = 0; i < textPanels.Count; i++) {
            IMyTextPanel currentPanel = (IMyTextPanel)textPanels[i];
            if (!(currentPanel.CustomData.Contains(surfaceTag)))
                continue;
            TSurfaceOption newOption = new TSurfaceOption();
            newOption.surface = (IMyTextSurface)currentPanel;
            ConfigureSurface(newOption.surface);
            surfaces.Add(newOption);
        }
    }
    return surfaces;
}

public void ConfigureSurface(IMyTextSurface surface) {
    surface.ContentType = ContentType.TEXT_AND_IMAGE;
			    surface.FontSize = 2;
			    surface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.LEFT;
}

public List<IMyTerminalBlock> GetDamagedBlocks() {
    List<IMyTerminalBlock> damagedBlocks = new List<IMyTerminalBlock>{};
    List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>{};
    GridTerminalSystem.GetBlocks(allBlocks);
    foreach (IMyTerminalBlock block in allBlocks) {
        if (block.CubeGrid != Me.CubeGrid)
            continue; // only interested in cubes on this grid.
        if (block.IsFunctional)
            continue; // not interested in healthy blocks
        damagedBlocks.Add(block);
    }
    return damagedBlocks;
}

public string FormatDamagedList(List<IMyTerminalBlock> damagedBlocks) {
    string returnString = "";
    int maxLines = 8;
    for (int i = 0; i < damagedBlocks.Count && i < maxLines; i++) {
        returnString += String.Format("{0} Damaged\n", damagedBlocks[i].CustomName.Replace("Pioneer", ""));
    }
    return returnString;
}

public void WriteFancy(List<TSurfaceOption> surfaceOptions, string LCDMessage) {
    foreach (TSurfaceOption surfaceOption in surfaceOptions) {
        surfaceOption.surface.WriteText(LCDMessage);
    }
}