FROM mcr.microsoft.com/dotnet/core/runtime:3.0.0-buster-slim-arm32v7 AS base
COPY /tmp/qemu-arm-static /usr/bin/qemu-arm-static
RUN apt-get update && apt-get install -y librdkafka-dev librdkafka1
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY PIREventProcessor.sln ./
COPY KafkaConsumer/PIREventProcessor.csproj KafkaConsumer/
COPY . .

RUN dotnet restore -nowarn:msb3202,nu1503

WORKDIR /src/KafkaConsumer
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PIREventProcessor.dll"]