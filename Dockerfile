FROM mcr.microsoft.com/dotnet/runtime:5.0.3-buster-slim-arm32v7 AS base

COPY /tmp/qemu-arm-static /usr/bin/qemu-arm-static
RUN apt-get update && apt-get install -y librdkafka-dev librdkafka1
WORKDIR /app

FROM  mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY PIREventProcessor.sln ./
COPY EventProcessor/EventProcessor.csproj EventProcessor/
COPY . .

RUN dotnet restore -nowarn:msb3202,nu1503

WORKDIR /src/EventProcessor
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "EventProcessor.dll"]