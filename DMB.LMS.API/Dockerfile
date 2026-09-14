# Build from DMB-LMS repo root
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DMB.LMS.API/DMB.LMS.API.csproj", "DMB.LMS.API/"]
COPY ["DMB.LMS.Service/DMB.LMS.Service.csproj", "DMB.LMS.Service/"]
COPY ["DMB.LMS.DATA/DMB.LMS.DATA.csproj", "DMB.LMS.DATA/"]
COPY ["DMB.LMS.MODEL/DMB.LMS.MODEL.csproj", "DMB.LMS.MODEL/"]
RUN dotnet restore "DMB.LMS.API/DMB.LMS.API.csproj"
COPY . .
RUN dotnet publish "DMB.LMS.API/DMB.LMS.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
CMD ASPNETCORE_URLS=http://0.0.0.0:$PORT dotnet DMB.LMS.API.dll
