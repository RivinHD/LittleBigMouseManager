# Little Big Mouse Manager
## Description
This is a programm handeling restarting/reloading of [LittleBigMouse](https://github.com/mgth/LittleBigMouse) when the display change and the the screen is a diffrent one (e.g. extend projection to second monitor).
It is done by killing and starting or using the command line arguments `--start` and `--stop` of the LittleBigMouse_Deamon.

It also restarts LittleBigMouse after a crash (see Settings) or start LittleBigMouse if not runnning.

## Installation
Download the Installer in the [Release](https://github.com/RivinHD/LittleBigMouseManager/releases/latest) and execute.
It will automatically register a task in the Task Scheduler upon login like LittleBigMouse and a shortcut to the startmenu.


## Usage/Settings
The application will automatically start no actions are requiered.

The Settings will be put in the same path as the installtion/executable and is called `Manager_Settings.json`.
There are the following settings:
- **ProcessName**: name of the Process without .exe to find process if LBM is already running. This is by default `"LittleBigMouse_Daemon"` but may change in the future. See [LBM Wiki](https://github.com/mgth/LittleBigMouse/wiki#command-line).
- **ProcessPath**: by default `"C:\\Program Files\\LittleBigMouse\\LittleBigMouse_Daemon.exe"` this is need when LittlBigMouse need to be started or on crash. The Setting will adjust automatically if an instance of LittelBigMouse is already running.
- **RestartOnClose** [true/false]: Restart LBM if it was exited in **any** way
- **RestartOnUnwantedClose** [true/false]: Restart LDM if it was exited witout Exit Code 0 (LBM completed successfully with no problems).
- **KillLBM** [true/false]: Kill current LDM process and restart if set to true. If set to false it uses `--stop` wait with the setting SafetyTime and use `--start`.
- **SafetyTime** [time in milliseconds]: Time to wait before handeling a display change event or between executing `--stop` and `--start`. If the time of the display change event overlaps the latest will be used to execute the reloading/restarting.
