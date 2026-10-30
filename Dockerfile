FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/back/TareaApi/TareaApi.csproj TareaApi/
RUN dotnet restore TareaApi/TareaApi.csproj
COPY src/back/TareaApi/ TareaApi/
RUN dotnet publish TareaApi/TareaApi.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
VOLUME /data
ENTRYPOINT ["dotnet", "TareaApi.dll"]
