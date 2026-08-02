FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish "Gnosis.WebApi/Gnosis.WebApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
# Render enruta tráfico al puerto que el contenedor exponga; sin esto Kestrel escucha en el 8080
# por defecto de la imagen pero no queda declarado explícitamente para la plataforma.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gnosis.WebApi.dll"]
