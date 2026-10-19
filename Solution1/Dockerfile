FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY SmartX.API/SmartX.API.csproj SmartX.API/
RUN dotnet restore SmartX.API/SmartX.API.csproj
COPY SmartX.API/ SmartX.API/
RUN dotnet publish SmartX.API/SmartX.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5056
EXPOSE 5056
ENTRYPOINT ["dotnet", "SmartX.API.dll"]