export  executable=MIT.Fwk.WebApi.dll
export  mypath=`pwd`
export mypath=$mypath/$executable
echo $mypath
dotnet  "$mypath" -key Mae2019! > temp.txt
export mykey=`cat temp.txt`
rm temp.txt
dotnet  "$mypath" -lic $mykey -v 3