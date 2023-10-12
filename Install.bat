@cd /d "%~dp0"
if not exist Virtual-Monitors (
	if not exist virtual-monitors.zip (
		powershell -Command "Invoke-WebRequest https://www.amyuni.com/downloads/usbmmidd_v2.zip -OutFile virtual-monitors.zip"
	)
	powershell Expand-Archive virtual-monitors.zip -DestinationPath Virtual-Monitors
)
if exist virtual-monitors.zip (
	del virtual-monitors.zip
)

@cd Virtual-Monitors
@cd usbmmidd_v2

@goto %PROCESSOR_ARCHITECTURE%
@exit

:AMD64
@cmd /c deviceinstaller64.exe install usbmmidd.inf usbmmidd
@goto end

:x86
@cmd /c deviceinstaller.exe install usbmmidd.inf usbmmidd

:end
