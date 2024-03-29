ARG DOTNET_VERSION=8.0


FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS base
WORKDIR /source
COPY src .
RUN dotnet restore


FROM base as build
WORKDIR /source
RUN dotnet build -c Release --no-restore


FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore --no-build


FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine AS release
RUN addgroup -g 10001 -S redsail && adduser -u 10001 -S redsail -G redsail
USER redsail
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "CallNCache.dll" ]
