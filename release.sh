#!/bin/bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=none -p:DebugSymbols=false 
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=none -p:DebugSymbols=false 

echo "Removing pdb files"
rm -fv bin/Release/net10.0/win-x64/publish/*.pdb
rm -fv bin/Release/net10.0/linux-x64/publish/*.pdb

echo "Copying dll files for windows."
# Add binary download functionality to this later
cp -v winfiles/libtesseract-5.dll bin/Release/net10.0/win-x64/publish/ 
cp -v winfiles/libleptonica-6.dll bin/Release/net10.0/win-x64/publish/

echo "Copying .so binarys for linux"
cp -v /usr/lib/libleptonica.so.6 bin/Release/net10.0/linux-x64/publish/
cp -v /usr/lib/libtesseract.so.5 bin/Release/net10.0/linux-x64/publish

du -sh bin/Release/net10.0/linux-x64/publish/
du -sh bin/Release/net10.0/win-x64/publish/

echo "Archiving Windows"
7z a -t7z -m0=lzma2 -mx=9 Releases/TransactionOCR-windows.7z bin/Release/net10.0/win-x64/publish/

echo "Archiving Linux"
tar -cJfv Releases/TransactionOCR-linux.tar.xz -C bin/Release/net10.0/linux-x64/ publish/
