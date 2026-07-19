FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG PROJECT
WORKDIR /src

COPY . .
RUN dotnet publish "$PROJECT" --configuration Release --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

ARG APP_DLL
ENV APP_DLL=${APP_DLL}
ENV ASPNETCORE_URLS=http://+:8080

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["sh", "-c", "dotnet $APP_DLL"]
