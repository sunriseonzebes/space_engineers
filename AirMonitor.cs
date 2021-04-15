public class ShipSystems {
    public List<IMyTextPanel> textPanels;
    public List<IMyButtonPanel> buttonPanels;
	public List<IMyAirVent> airVents;

    public void Init(IMyGridTerminalSystem gts, IMyCubeGrid desiredGrid) {
        this.buttonPanels = GetButtonPanels(gts, desiredGrid);
        this.textPanels = GetTextPanels(gts, desiredGrid);
        this.airVents = GetAirVents(gts, desiredGrid);
    }

    public List<IMyTextSurface> GetTextSurfacesWithMetadata(string metadata) {
        List<IMyTextSurface> surfaceList = new List<IMyTextSurface>();
        for (int i = 0; i < this.textPanels.Count; i++) {
            if (this.textPanels[i].CustomData.Contains(metadata)) {
                surfaceList.Add((IMyTextSurface)this.textPanels[i]);
            }
        }
        for (int i = 0; i < this.buttonPanels.Count; i++) {
            if (this.buttonPanels[i].CustomData.Contains(metadata)) {
                surfaceList.Add(((IMyTextSurfaceProvider)this.buttonPanels[i]).GetSurface(0));
            }
        }

        return surfaceList;
    }
	
	private List<IMyAirVent> GetAirVents(IMyGridTerminalSystem gts, IMyCubeGrid desiredGrid){
        List<IMyAirVent> ventList = new List<IMyAirVent>();

        List<IMyTerminalBlock> ventBlocks = new List<IMyTerminalBlock>{};
        gts.GetBlocksOfType<IMyAirVent>(ventBlocks);

        for (int i = 0; i < ventBlocks.Count; i++) {
            IMyAirVent currentVent = (IMyAirVent)ventBlocks[i];
            if (currentVent.CubeGrid != desiredGrid)
                continue;  // Ignore doors not on this grid
            ventList.Add(currentVent);
        }
        return ventList;
    }

    private List<IMyButtonPanel> GetButtonPanels(IMyGridTerminalSystem gts, IMyCubeGrid desiredGrid){
        List<IMyButtonPanel> buttonList = new List<IMyButtonPanel>();

        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>{};
        gts.GetBlocksOfType<IMyButtonPanel>(blocks);

        for (int i = 0; i < blocks.Count; i++) {
            IMyButtonPanel currentButton = (IMyButtonPanel)blocks[i];
            if (currentButton.CubeGrid != desiredGrid)
                continue;  // Ignore panels not on this grid
            buttonList.Add(currentButton);
        }
        return buttonList;
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

public struct TSurfaceOption {
    public IMyTextSurface surface;
    public bool changeBackground;
}

public void ShowAirVentStatusOnTextSurfaces(ShipSystems shipSystems) {
    List <IMyTextSurface> textSurfaces = shipSystems.GetTextSurfacesWithMetadata("AirMonitor.WriteText: True");
    List <IMyTextSurface> textSurfacesBackgrounds = shipSystems.GetTextSurfacesWithMetadata("AirMonitor.UpdateBGColor: True");

    foreach (IMyAirVent airVent in shipSystems.airVents) {
        if (airVent.CustomData.Contains("AirMonitor.Monitor: True")) {
            float percent = airVent.GetOxygenLevel()*100f;
            foreach (IMyTextSurface textSurface in textSurfaces) {
                if (!(airVent.Enabled)) {
                    textSurface.WriteText("Air Vent Off");
                }
                else {
                    string LCDMessage = String.Format("\nBunk Pressure\n{0:0.00}%", percent);
                    textSurface.WriteText(LCDMessage);
                }
            }
            foreach (IMyTextSurface textSurface in textSurfacesBackgrounds) {
                if (!(airVent.Enabled)) {
                    textSurface.BackgroundColor = Color.Black;
                }
                else {
                    Color newBackgroundColor = Color.Red;
                    if (percent > 50f)
                        newBackgroundColor = Color.DarkOrange;
                    if (percent > 75f)
                        newBackgroundColor = Color.Green;  
                    textSurface.BackgroundColor = newBackgroundColor;
                }
            }
        }
    }
}

public void Main(string argument) {
    ShipSystems shipSystems = new ShipSystems();
    shipSystems.Init(GridTerminalSystem, Me.CubeGrid);

    ShowAirVentStatusOnTextSurfaces(shipSystems);
}
