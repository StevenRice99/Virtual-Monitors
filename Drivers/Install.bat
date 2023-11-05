@cd /d "%~dp0"
if not exist Virtual-Monitor-Drivers (
	if not exist virtual-monitor-drivers.zip (
		powershell -Command "Invoke-WebRequest https://www.amyuni.com/downloads/usbmmidd_v2.zip -OutFile virtual-monitor-drivers.zip"
	)
	powershell Expand-Archive virtual-monitor-drivers.zip -DestinationPath Virtual-Monitor-Drivers
)
if exist virtual-monitor-drivers.zip (
	del virtual-monitor-drivers.zip
)

@cd Virtual-Monitor-Drivers
@cd usbmmidd_v2

@goto %PROCESSOR_ARCHITECTURE%
@exit

:AMD64
@cmd /c deviceinstaller64.exe install usbmmidd.inf usbmmidd
@goto end

:x86
@cmd /c deviceinstaller.exe install usbmmidd.inf usbmmidd

:end
