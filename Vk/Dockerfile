FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["Vk/Vk.csproj", "Vk/"]
RUN dotnet restore "Vk/Vk.csproj"
COPY . .
WORKDIR "/src/Vk"
RUN dotnet build "Vk.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Vk.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Vk.dll"]
