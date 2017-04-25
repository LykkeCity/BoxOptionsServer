dotnet publish -c Release -o bin\docker;
cd bin\docker;
docker rmi -f lykkex/boxoptionsserver
docker build -t lykkex/boxoptionsserver .
cd ..\..;
