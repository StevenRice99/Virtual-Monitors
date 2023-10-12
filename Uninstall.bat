@cd /d "%~dp0"
if not exist Virtual-Monitors (
	@goto stop
)
@cd Virtual-Monitors
if not exist usbmmidd_v2 (
	@goto stop
)
@cd usbmmidd_v2

@goto %PROCESSOR_ARCHITECTURE%
@exit

:AMD64
@cmd /c deviceinstaller64.exe enableidd 0
@cmd /c deviceinstaller64.exe enableidd 0
@cmd /c deviceinstaller64.exe enableidd 0
@cmd /c deviceinstaller64.exe enableidd 0
@cmd /c deviceinstaller64.exe stop usbmmidd
@cmd /c deviceinstaller64.exe remove usbmmidd
@goto end

:x86
@cmd /c deviceinstaller.exe enableidd 0
@cmd /c deviceinstaller.exe enableidd 0
@cmd /c deviceinstaller.exe enableidd 0
@cmd /c deviceinstaller.exe enableidd 0
@cmd /c deviceinstaller.exe stop usbmmidd
@cmd /c deviceinstaller.exe remove usbmmidd

:end
@cd ..
@cd ..
@RD /S /Q Virtual-Monitors

:stop
