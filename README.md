# SimcoeCountyGeoServerService
Windows Service to start/stop GeoServer

This works with the "Platform Independent Binary" downloaded from: http://geoserver.org/release/stable/

Since the GeoServer dev team is no longer able to provide the windows installer that also came with a windows service, there is no elegant way to start and stop GeoServer.  This will also allow for the Automatic Startup of GeoServer when your Windows server reboots.  It also writes to the event viewer and will report any errors there for startup/shutdown failures.

## Installation

1. Ensure you have the GEOSERVER_HOME environment variable set.  e.g. c:\geoserver The service will reference the bin folder to run the startup.bat and shutdown.bat.
2. Clone the repo.
3. Copy the contents of the compiledExe folder of the repo to a place on your server where the service exe will live on.  Once the service is installed it will reference the location of the GeoServerService.exe to start and stop it.
4. Install the Service.  InstallUtil.exe should be part of any recent .Net Framework.  I tested this with v4 of the .Net Framework.

Run the following command as an administrator.

"c:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe" "c:\<YourOwnPath>\GeoServerService.exe"

5. Start the service.  The installer will not automatically start the service, but will on reboot.
6. Check the event viewer.  You should see entries for "GeoServerService".  If you have an errors, they will show up in the event viewer.
