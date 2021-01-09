public class ShipSystems {
    public List<IMyAirVent> airVents;
    public List<IMyButtonPanel> buttons;
    public List<IMyDoor> doors;

    public void Init(IMyGridTerminalSystem gts, IMyCubeGrid desiredGrid) {
        this.airVents = GetAirVents(gts, desiredGrid);
        this.buttons = GetButtons(gts, desiredGrid);
        this.doors = GetDoors(gts, desiredGrid);
    }

    private List<IMyDoor> GetDoors(IMyGridTerminalSystem gts, IMyCubeGrid desiredGrid){
        List<IMyDoor> doorList = new List<IMyDoor>();

        List<IMyTerminalBlock> doorBlocks = new List<IMyTerminalBlock>{};
        gts.GetBlocksOfType<IMyDoor>(doorBlocks);

        for (int i = 0; i < doorBlocks.Count; i++) {
            IMyDoor currentDoor = (IMyDoor)doorBlocks[i];
            if (currentDoor.CubeGrid != desiredGrid)
                continue;  // Ignore doors not on this grid
            doorList.Add(currentDoor);
        }
        return doorList;
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


    private List<IMyButtonPanel> GetButtons(IMyGridTerminalSystem gts, IMyCubeGrid desiredGrid){
        List<IMyButtonPanel> buttonList = new List<IMyButtonPanel>();

        List<IMyTerminalBlock> buttonBlocks = new List<IMyTerminalBlock>{};
        gts.GetBlocksOfType<IMyButtonPanel>(buttonBlocks);

        for (int i = 0; i < buttonBlocks.Count; i++) {
            IMyButtonPanel currentButton = (IMyButtonPanel)buttonBlocks[i];
            if (currentButton.CubeGrid != desiredGrid)
                continue;  // Ignore buttons not on this grid
            buttonList.Add(currentButton);
        }
        return buttonList;
    }
}

public enum AirlockState {
    OpenToAir,
    OpenToSpace
}

public class AirlockSystem {
    public IMyDoor airSideDoor = null;
    public IMyDoor spaceSideDoor = null;
    public IMyAirVent airVent = null;
    public IMyButtonPanel button = null;
    public AirlockState desiredState;

    public bool AssignMembersFromButton(IMyButtonPanel newButton, ShipSystems shipSystems) {
        this.button = newButton;

        string desiredStateString = GetKeyValue(newButton.CustomData, "AirlockController.DesiredState");
        if (desiredStateString == null)
            return false;
        this.desiredState = getStateFromString(desiredStateString);

        string airVentName = GetKeyValue(newButton.CustomData, "AirlockController.AirVentName");
        if (airVentName == null)
            return false;
        for (int i = 0; i < shipSystems.airVents.Count; ++i) {
            if (shipSystems.airVents[i].CustomName == airVentName)
                this.airVent = shipSystems.airVents[i];
        }
        if (this.airVent == null)
            return false;  // Specified air vent wasn't found.

        string airSideDoorName = GetKeyValue(newButton.CustomData, "AirlockController.AirSideDoorName");
        if (airSideDoorName == null)
            return false;
        for (int i = 0; i < shipSystems.doors.Count; ++i) {
            if (shipSystems.doors[i].CustomName == airSideDoorName)
                this.airSideDoor = shipSystems.doors[i];
        }
        if (this.airSideDoor == null)
            return false;  // Specified door wasn't found.

        string spaceSideDoorName = GetKeyValue(newButton.CustomData, "AirlockController.SpaceSideDoorName");
        if (spaceSideDoorName == null)
            return false;
        for (int i = 0; i < shipSystems.doors.Count; ++i) {
            if (shipSystems.doors[i].CustomName == spaceSideDoorName)
                this.spaceSideDoor = shipSystems.doors[i];
        }
        if (this.spaceSideDoor == null)
            return false;  // Specified door wasn't found.

        return true;
    }

    public string GetKeyValue(string textBlob, string key) {
        string[] lines = textBlob.Split(new string[] { "\n" }, StringSplitOptions.None);
        for (int i = 0; i < lines.Length; ++i) {
            if (lines[i].StartsWith(key)) {
                return lines[i]
                        .Substring(key.Length)
                        .Trim()
                        .Substring(1)
                        .Trim()
                        ;    
            }
        }
        return null;
    }

    public AirlockState getStateFromString(string stateString) {
        if (stateString == "OpenToSpace")
            return AirlockState.OpenToSpace;
        else
            return AirlockState.OpenToAir;
    }

    public void PutIntoDesiredState() {
        if (this.desiredState == AirlockState.OpenToAir)
            PutIntoOpenAirState();
        if (this.desiredState == AirlockState.OpenToSpace)
            PutIntoOpenSpaceState();
    }

    public void PutIntoOpenAirState() {
        if (!(this.airVent.Enabled))
            this.airVent.Enabled = true;
        if (this.spaceSideDoor.OpenRatio > 0f) {
            if (!(this.spaceSideDoor.Enabled))
                this.spaceSideDoor.Enabled = true;
            this.spaceSideDoor.CloseDoor();
        }
        if (this.spaceSideDoor.OpenRatio == 0f)
            this.spaceSideDoor.Enabled = false;
        if (this.airVent.GetOxygenLevel() < 0.95f) {
            if (this.airSideDoor.OpenRatio > 0f) {
                if (!(this.airSideDoor.Enabled))
                    this.airSideDoor.Enabled = true;
                this.airSideDoor.CloseDoor();
            }
            if (this.airSideDoor.OpenRatio == 0f) {
                this.airSideDoor.Enabled = false;
                this.airVent.Depressurize = false;
            }
        }
        else {
            if (!(this.airSideDoor.Enabled))
                this.airSideDoor.Enabled = true;
            if (this.airSideDoor.OpenRatio < 1f)
                this.airSideDoor.OpenDoor();
        }
    }
    
    public void PutIntoOpenSpaceState() {
        if (!(this.airVent.Enabled))
            this.airVent.Enabled = true;
        if (this.airSideDoor.OpenRatio > 0f) {
            if (!(this.airSideDoor.Enabled))
                this.airSideDoor.Enabled = true;
            this.airSideDoor.CloseDoor();
        }
        if (this.airSideDoor.OpenRatio == 0f)
            this.airSideDoor.Enabled = false;
        if (this.airVent.GetOxygenLevel() > 0f) {
            if (this.spaceSideDoor.OpenRatio > 0f) {
                if (!(this.spaceSideDoor.Enabled))
                    this.spaceSideDoor.Enabled = true;
                this.spaceSideDoor.CloseDoor();
            }
            if (this.spaceSideDoor.OpenRatio == 0f) {
                this.spaceSideDoor.Enabled = false;
                this.airVent.Depressurize = true;
            }
        }
        else {
            if (!(this.spaceSideDoor.Enabled))
                this.spaceSideDoor.Enabled = true;
            if (this.spaceSideDoor.OpenRatio < 1f)
                this.spaceSideDoor.OpenDoor();
        }
    }
}


public List<AirlockSystem> GetAirlockSystems(ShipSystems shipSystems) {
    List<AirlockSystem> airlockSystems = new List<AirlockSystem>();
    for (int i = 0; i < shipSystems.buttons.Count; ++i) {

        AirlockSystem newAirlockSystem = new AirlockSystem();
        if (newAirlockSystem.AssignMembersFromButton(shipSystems.buttons[i], shipSystems))
            airlockSystems.Add(newAirlockSystem);
    }

    return airlockSystems;
}

public void ToggleAirlockDesiredState(string buttonName, ShipSystems shipSystems) {
	IMyButtonPanel button = null;
	foreach (IMyButtonPanel currentButton in shipSystems.buttons) {
		if (currentButton.CustomName == buttonName) {
			button = currentButton;
			break;
		}
	}
	
	if (button == null)
		return;
	
	string[] oldButtonData = button.CustomData.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
	string[] newButtonData = new string[oldButtonData.Length];
	for (int i = 0; i < oldButtonData.Length; ++i) {
		if (oldButtonData[i].Contains("AirlockController.DesiredState")) {
			if (oldButtonData[i].Contains("OpenToSpace"))
				newButtonData[i] = oldButtonData[i].Replace("OpenToSpace", "OpenToAir");
			else if (oldButtonData[i].Contains("OpenToAir"))
				newButtonData[i] = oldButtonData[i].Replace("OpenToAir", "OpenToSpace");
		}
		else
			newButtonData[i] = oldButtonData[i];
	}
	
	button.CustomData = String.Join("\n", newButtonData);
}

public void Main(string argument, UpdateType updateSource)
{
	ShipSystems shipSystems = new ShipSystems();
    shipSystems.Init(GridTerminalSystem, Me.CubeGrid);
    List<AirlockSystem> airlockSystems = GetAirlockSystems(shipSystems);
    
	if (argument != "") {
		ToggleAirlockDesiredState(argument, shipSystems);
	}
	else {
		for (int i = 0; i < airlockSystems.Count; ++i) {
			airlockSystems[i].PutIntoDesiredState();
		}
	}
}
