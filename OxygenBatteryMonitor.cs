public void Main(string argument) {
    IMyTextSurface LCD = null;
    LCD = GetTextSurface(argument);
    if (LCD == null)
        return; // Don't do anything if a bad name was provided for the LCD.

    float batteryPercent = GetBatteryPercent("Pioneer");    
    float oxygenPercent = GetGasPercent("Pioneer Oxygen");
    string LCDMessage = String.Format("\n\n\nBatteries: {0:0.00}%\nOxygen: {1:0.00}%", batteryPercent, oxygenPercent);
    
    WriteFancy(LCD, LCDMessage);
}

public IMyTextSurface GetTextSurface(string surfaceName) {
    IMyTextSurface returnSurface = null;
    if (surfaceName == "Debug")
        returnSurface = Me.GetSurface(0);
    else
        returnSurface = (IMyTextSurface)GridTerminalSystem.GetBlockWithName(surfaceName);

    if (returnSurface == null)
        return null;
        
    //Configure the surface
    returnSurface.ContentType = ContentType.TEXT_AND_IMAGE;
			    returnSurface.FontSize = 2;
			    returnSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;

    return returnSurface;
}

public float GetBatteryPercent(string startsWith) {
    List<IMyTerminalBlock> batteries = new List<IMyTerminalBlock>{};
    GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries);
    float stored = 0f;
    float total = 0f;
    for (int i = 0; i < batteries.Count; i++) {
        IMyBatteryBlock currentBattery = (IMyBatteryBlock)batteries[i];
        if (!(currentBattery.CustomName.StartsWith(startsWith)))
            continue;
        stored += currentBattery.CurrentStoredPower;
        total += currentBattery.MaxStoredPower;
    }
        float percent = (stored / total) * 100f;
    return percent;
}


public float GetGasPercent(string startsWith) {
    List<IMyTerminalBlock> gasTanks = new List<IMyTerminalBlock>{};
    GridTerminalSystem.GetBlocksOfType<IMyGasTank>(gasTanks);
    float stored = 0f;
    float total = 0f;
    for (int i = 0; i < gasTanks.Count; i++) {
        IMyGasTank currentTank = (IMyGasTank)gasTanks[i];
        if (!(currentTank.CustomName.StartsWith(startsWith)))
            continue;
        stored += (float)(currentTank.Capacity * currentTank.FilledRatio);
        total += currentTank.Capacity;
    }
        float percent = (stored / total) * 100f;
    return percent;
}

public void WriteFancy(IMyTextSurface surface, string LCDMessage) {
    surface.WriteText(LCDMessage);
}