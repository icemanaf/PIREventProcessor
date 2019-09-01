FROM mcr.microsoft.com/dotnet/core/runtime:2.2 AS base
RUN apt-get update && apt-get install -y librdkafka-dev librdkafka1
#RUN apt-get install openssl
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY PIREventProcessor.sln ./
COPY KafkaConsumer/PIREventProcessor.csproj KafkaConsumer/
COPY . .
COPY /tmp/qemu-arm-static /usr/bin/qemu-arm-static
RUN dotnet restore -nowarn:msb3202,nu1503

WORKDIR /src/KafkaConsumer
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PIREventProcessor.dll"]