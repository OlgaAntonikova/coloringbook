# Build: publishing the API also publishes the Blazor WebAssembly client it serves
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/ColoringBook.Api/ColoringBook.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
# QuestPDF needs fontconfig on Linux to render text labels
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /data/files && chown -R app:app /data
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080 \
    Storage__Path=/data/files
VOLUME /data
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "ColoringBook.Api.dll"]
