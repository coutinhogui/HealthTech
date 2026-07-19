FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src
COPY . .
RUN dotnet publish src/front/HealthTech.Front.csproj --configuration Release --output /app/publish

FROM nginx:1.27-alpine AS runtime

COPY docker/nginx-front.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

EXPOSE 80
