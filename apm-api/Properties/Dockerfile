FROM mcr.microsoft.com/dotnet/aspnet:5.0 as base

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 as build

WORKDIR /app

COPY ./ ./

RUN dotnet publish -c Release -o out

FROM base

COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "FooApi.dll"]

#ENTRYPOINT /bin/bash