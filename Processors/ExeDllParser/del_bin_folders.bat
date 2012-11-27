for /d /r . %%d in (bin,obj,debug,release,Debug,Release) do @if exist "%%d" rd /s/q "%%d"
del /s/q *.ncb
del /s/q *.obj
del /s/q *.exe
