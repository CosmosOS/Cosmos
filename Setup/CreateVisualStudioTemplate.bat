del Cosmos.vsi
cd ProjectTemplate
..\..\Build\tools\7zip\7za.exe -r a ..\CosmosBoot.zip *.*
cd ..
..\Build\tools\7zip\7za.exe a -tzip Cosmos.vsi Cosmos.vscontent CosmosBoot.zip
del CosmosBoot.zip