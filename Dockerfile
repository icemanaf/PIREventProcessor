FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY PIREventProcessor.sln ./
COPY KafkaConsumer/PIREventProcessor.csproj KafkaConsumer/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/KafkaConsumer
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PIREventProcessor.dll"]
