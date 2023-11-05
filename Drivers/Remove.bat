@cd /d "%~dp0"
if not exist Virtual-Monitor-Drivers (
	@goto stop
)
@cd Virtual-Monitor-Drivers
if not exist usbmmidd_v2 (
	@goto stop
)
@cd usbmmidd_v2

@goto %PROCESSOR_ARCHITECTURE%
@exit

:AMD64
@cmd /c deviceinstaller64.exe enableidd 0
@goto stop

:x86
@cmd /c deviceinstaller.exe enableidd 0

:stop