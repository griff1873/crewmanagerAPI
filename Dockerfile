# Use the official .NET 8 SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["CrewManagerAPI/CrewManagerAPI.csproj", "CrewManagerAPI/"]
COPY ["CrewManagerData/CrewManagerData.csproj", "CrewManagerData/"]
RUN dotnet restore "CrewManagerAPI/CrewManagerAPI.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/CrewManagerAPI"
RUN dotnet build "CrewManagerAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CrewManagerAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 8 ASP.NET Core Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configure for Google Cloud Run (listen on port 8080)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CrewManagerAPI.dll"]
