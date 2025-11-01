FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Clinic-System.API/Clinic-System.API.csproj", "Clinic-System.API/"]
COPY ["Clinic-System.Application/Clinic-System.Application.csproj", "Clinic-System.Application/"]
COPY ["Clinic-System.Domain/Clinic-System.Domain.csproj", "Clinic-System.Domain/"]
COPY ["Clinic-System.Infrastructure/Clinic-System.Infrastructure.csproj", "Clinic-System.Infrastructure/"]
RUN dotnet restore "Clinic-System.API/Clinic-System.API.csproj"
COPY . .
WORKDIR "/src/Clinic-System.API"
RUN dotnet publish "Clinic-System.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY ["Clinic-System.API/.env", "."]       
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Clinic-System.API.dll"]
