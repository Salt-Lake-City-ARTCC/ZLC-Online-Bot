FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ZLCBotCore/ZLCBotCore.csproj", "ZLCBotCore/"]
RUN dotnet restore "ZLCBotCore/ZLCBotCore.csproj"
COPY . .
WORKDIR "/src/ZLCBotCore"
RUN dotnet build "ZLCBotCore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ZLCBotCore.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ZLCBotCore.dll"]
