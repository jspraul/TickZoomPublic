copy /Y ExamplesPlugin.dll "REPLACE\PluginFolder"
if not exist CustomPlugin.pdb GOTO END
copy /Y ExamplesPlugin.pdb "REPLACE\PluginFolder"
:END
