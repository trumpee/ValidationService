FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./ ./

ARG NUGET_API_KEY
ARG SOLUTION_NAME

RUN dotnet nuget add source https://nuget.pkg.github.com/trumpee/index.json \
    --name github \
    --username trumpee \
    --password $NUGET_API_KEY \
    --store-password-in-clear-text

RUN dotnet restore "$SOLUTION_NAME.sln" \
    --source "https://api.nuget.org/v3/index.json" \
    --source "https://nuget.pkg.github.com/trumpee/index.json"

RUN dotnet build "$SOLUTION_NAME.sln" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "$SOLUTION_NAME.sln" -c Release -o /app/publish --self-contained -r linux-x64

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ValidationService.dll"]
