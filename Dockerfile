FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PlataformaCreditos/PlataformaCreditos.csproj PlataformaCreditos/
RUN dotnet restore PlataformaCreditos/PlataformaCreditos.csproj

COPY . .
WORKDIR /src/PlataformaCreditos
RUN dotnet publish PlataformaCreditos.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PlataformaCreditos.dll"]