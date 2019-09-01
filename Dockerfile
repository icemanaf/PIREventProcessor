FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim-arm32v7 AS base
COPY /tmp/qemu-arm-static /usr/bin/qemu-arm-static
RUN apt-get update && apt-get install -y librdkafka-dev librdkafka1
#RUN apt-get install openssl
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch-arm32v7 AS build
COPY /tmp/qemu-arm-static /usr/bin/qemu-arm-static
WORKDIR /src
COPY PIREventProcessor.sln ./
COPY KafkaConsumer/PIREventProcessor.csproj KafkaConsumer/
COPY . .

RUN dotnet restore -nowarn:msb3202,nu1503

WORKDIR /src/KafkaConsumer
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM build as TEST
RUN uname -m

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PIREventProcessor.dll"]