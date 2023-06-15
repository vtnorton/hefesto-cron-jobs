FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated7.0 AS base
LABEL maintainer="vítor norton"
LABEL description="hefesto-cron-jobs"
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/runtime:6.0 as runtime6.0
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
COPY --from=runtime6.0 /usr/share/dotnet/host /usr/share/dotnet/host
COPY --from=runtime6.0 /usr/share/dotnet/shared /usr/share/dotnet/shared
WORKDIR /src
COPY ["./CronJobsForHefesto/CronJobsForHefesto.csproj", "CronJobsForHefesto/"]
RUN dotnet restore "CronJobsForHefesto/CronJobsForHefesto.csproj"
COPY . .
WORKDIR "/src/CronJobsForHefesto"
RUN dotnet build "CronJobsForHefesto.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CronJobsForHefesto.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true