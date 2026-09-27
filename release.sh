#!/bin/bash 
project_base="$(pwd)" 
project="TransactionOCR" 

echo "Compiling release build for linux_64-bit..." 
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=none -p:DebugSymbols=false 
echo -e "\nCompiling release build for windows_64-bit..." 
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=none -p:DebugSymbols=false 

echo -e "\nRemoving pdb files from publish directory..." 
rm -fv $project_base/bin/Release/net10.0/win-x64/publish/*.pdb 
rm -fv $project_base/bin/Release/net10.0/linux-x64/publish/*.pdb 

# Initial app creation thoughts, later tried to fix an issue with tesseractapi on windows 
# and saw that publish/x64 had dll files. # # echo "Copying dll files for windows." 
# # Add binary download functionality to this later 
# cp -v winfiles/libtesseract-5.dll bin/Release/net10.0/win-x64/publish/ 
# cp -v winfiles/libleptonica-6.dll bin/Release/net10.0/win-x64/publish/ 
# echo -e "\nCopying .so binarys for linux..." 
# cp -v /usr/lib/libleptonica.so.6 bin/Release/net10.0/linux-x64/publish/ 
# cp -v /usr/lib/libtesseract.so.5 bin/Release/net10.0/linux-x64/publish 

echo "\nCopying traindata files to release build..."
cp -aR tessdata bin/Release/net10.0/win-x64/publish/
cp -aR tessdata bin/Release/net10.0/linux-x64/publish/

echo -n "\nLinux release build size: "
du -sh bin/Release/net10.0/linux-x64/publish/ | cut -f1 
echo -n "\nWindows release build size: "
du -sh bin/Release/net10.0/win-x64/publish/ | cut -f1

echo -e "\nArchiving Windows" 
mkdir -p /tmp/$project 
cp -aR bin/Release/net10.0/win-x64/publish/. /tmp/$project
7z a -t7z -m0=lzma2 -mx=9 Releases/$project-windows.7z -w /tmp/$project/ 
rm -rf /tmp/$project 

echo -e "\nArchiving Linux" 
tar -cJfv $project_base/Releases/$project-linux.tar.xz --transform='s,^bin/Release/net10.0/linux-x64/publish,TransactionOCR/,' $project_base/bin/Release/net10.0/linux-x64/publish/