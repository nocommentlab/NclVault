# Generates a pfx certificate on the build machine
# dotnet dev-certs https -ep [Path of the certificate]-p [Password]
# docker run -p 80:80 -p 443:443 -e Kestrel__Certificates__Default__Path=/app/Infrastructure/Certificate/cert.pfx -e Kestrel__Certificates__Default__Password=mypassword -e "ASPNETCORE_URLS=https://+;http://+" -v <path that contains the certificate>:/app/Infrastructure/Certificate ncl/vault
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base  
WORKDIR /app  
EXPOSE 80  
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build  
WORKDIR /src  
RUN git clone https://github.com/nocommentlab/NclVault.git
WORKDIR /src/NclVault
RUN git checkout develop

WORKDIR /src/NclVault/NclVault
#COPY ["NclVaultAPIServer/NclVaultAPIServer.csproj", "NclVaultAPIServer/"]  
RUN dotnet restore "NclVaultAPIServer/NclVaultAPIServer.csproj"  
COPY . .  
WORKDIR /src/NclVault/NclVault/NclVaultAPIServer
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install --global dotnet-ef
RUN dotnet ef migrations add initialization
RUN dotnet ef database update

RUN dotnet build "NclVaultAPIServer.csproj" -c Release -o /app/build  

FROM build AS publish  
RUN dotnet publish "NclVaultAPIServer.csproj" -c Release -o /app/publish  

FROM base AS final  
WORKDIR /app  
COPY --from=publish /app/publish .
COPY --from=build  /src/NclVault/NclVault/NclVaultAPIServer/vault.db .
ENTRYPOINT ["dotnet", "NclVaultAPIServer.dll"]  