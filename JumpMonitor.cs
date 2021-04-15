public class ShipSystems {
    public List<IMyTextPanel> textPanels;
	public List<IMyJumpDrive> jumpDrives;

    public void Init(IMyGridTerminalSystem gts, IMyCubeGrid desiredGrid) {
        this.textPanels = GetTextPanels(gts, desiredGrid);
        this.jumpDrives = GetJumpDrives(gts, desiredGrid);
    }
	
	private List<IMyJumpDrive> GetJumpDrives(IMyGridTerminalSystem gts, IMyCubeGrid desiredGrid){
        List<IMyJumpDrive> jdList = new List<IMyJumpDrive>();

        List<IMyTerminalBlock> jdBlocks = new List<IMyTerminalBlock>{};
        gts.GetBlocksOfType<IMyJumpDrive>(jdBlocks);

        for (int i = 0; i < jdBlocks.Count; i++) {
            IMyJumpDrive currentJD = (IMyJumpDrive)jdBlocks[i];
            if (currentJD.CubeGrid != desiredGrid)
                continue;  // Ignore drives not on this grid
            jdList.Add(currentJD);
        }
        return jdList;
    }

    private List<IMyTextPanel> GetTextPanels(IMyGridTerminalSystem gts, IMyCubeGrid desiredGrid){
        List<IMyTextPanel> tpList = new List<IMyTextPanel>();

        List<IMyTerminalBlock> tpBlocks = new List<IMyTerminalBlock>{};
        gts.GetBlocksOfType<IMyTextPanel>(tpBlocks);

        for (int i = 0; i < tpBlocks.Count; i++) {
            IMyTextPanel currentTP = (IMyTextPanel)tpBlocks[i];
            if (currentTP.CubeGrid != desiredGrid)
                continue;  // Ignore panels not on this grid
            tpList.Add(currentTP);
        }
        return tpList;
    }
}

public void ShowJumpDriveInfoOnTextPanels(List<IMyJumpDrive> jumpDrives, List<IMyTextPanel> textPanels) {
    string jumpDriveText = "";
    for (int i = 0; i < jumpDrives.Count; i++) {
        jumpDriveText += String.Format("{0, -20}", jumpDrives[i].CustomName);
        jumpDriveText += String.Format("{0, -8}", Math.Floor((jumpDrives[i].CurrentStoredPower / jumpDrives[i].MaxStoredPower) * 100).ToString() + "%");
        jumpDriveText += String.Format("{0, -8}", jumpDrives[i].DetailedInfo.Split('\n')[5].Substring(20));
        jumpDriveText += "\n";
    }
    for (int i = 0; i < textPanels.Count; i++) {
        if (textPanels[i].CustomData.Contains("JumpDriveMonitor.UpdateDisplay: True")) {
            textPanels[i].WriteText(jumpDriveText);
        }
    }
}

public void Main(string argument, UpdateType updateSource)
{
	ShipSystems shipSystems = new ShipSystems();
    shipSystems.Init(GridTerminalSystem, Me.CubeGrid);
    ShowJumpDriveInfoOnTextPanels(shipSystems.jumpDrives, shipSystems.textPanels);
}
