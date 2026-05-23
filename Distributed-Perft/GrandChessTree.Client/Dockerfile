# Use a base .NET runtime image
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Define an ARG to capture the target platform
ARG TARGETARCH

# Use the .NET SDK for building the application
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH

WORKDIR /src

# Copy project files
COPY ["GrandChessTree.Client/GrandChessTree.Client.csproj", "GrandChessTree.Client/"]
COPY ["GrandChessTree.Shared/GrandChessTree.Shared.csproj", "GrandChessTree.Shared/"]
RUN dotnet restore "./GrandChessTree.Client/GrandChessTree.Client.csproj"

# Copy all source code
COPY . .

WORKDIR "/src/GrandChessTree.Client"

# Conditionally define constants based on architecture
RUN if [ "$TARGETARCH" = "arm64" ]; then \
        dotnet build "./GrandChessTree.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build -p:DefineConstants="ARM"; \
    else \
        dotnet build "./GrandChessTree.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build; \
    fi

# Publish the app
FROM build AS publish
RUN if [ "$TARGETARCH" = "arm64" ]; then \
        dotnet publish "./GrandChessTree.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish -p:DefineConstants="ARM" /p:UseAppHost=false; \
    else \
        dotnet publish "./GrandChessTree.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false; \
    fi

# Final image for running the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV containerized="true"

ENTRYPOINT ["dotnet", "GrandChessTree.Client.dll"]
