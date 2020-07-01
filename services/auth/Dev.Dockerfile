FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY backend.csproj backend.csproj
RUN dotnet restore backend.csproj
ENTRYPOINT ["dotnet", "run"]