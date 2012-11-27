for /d /r . %%d in (bin,obj,Debug,Release) do @if exist "%%d" rd /s/q "%%d"
