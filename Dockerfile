FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /src

COPY ["EduGame.sln", "./"]
COPY ["Backend/EduGame.API/EduGameProject.csproj", "Backend/EduGame.API/"]

RUN dotnet restore

COPY . .

WORKDIR /src/Backend/EduGame.API
RUN dotnet publish "EduGameProject.csproj" -c Release -o /app/out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build-env /app/out .

COPY Backend/EduGame.API/public.key ./

COPY Frontend/ ./wwwroot/
COPY Frontend/ ./Frontend/

RUN mkdir -p /app/ChatUploads

ENV ASPNETCORE_URLS="http://+:8080"
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "EduGameProject.dll"]