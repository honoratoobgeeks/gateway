# Use a imagem base do .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

# Defina o diretório de trabalho no contêiner
WORKDIR /app

# Copie os arquivos .csproj e restaure as dependências
COPY *.sln ./
COPY Presentation.API/*.csproj ./Presentation.API/
COPY Domain/*.csproj ./Domain/
COPY Application/*.csproj ./Application/
COPY Infra.Data/*.csproj ./Infra.Data/
COPY Infra.IoC/*.csproj ./Infra.IoC/
COPY Presentation.API/aspnetapp.pfx /app/aspnetapp.pfx


RUN dotnet restore

# Copie o restante do código da aplicação
COPY . .

# Compile a aplicação
RUN dotnet publish -c Release -o /out

# Crie uma imagem mínima para execução
FROM mcr.microsoft.com/dotnet/aspnet:7.0

# Defina o diretório de trabalho no contêiner
WORKDIR /app

# Copie os arquivos publicados da etapa de build
COPY --from=build-env /out .

# Defina a variável de ambiente
ENV ASPNETCORE_URLS=https://+:5116;http://+:5000
ENV ASPNETCORE_HTTPS_PORT=5116
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="00cc00cc"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/app/aspnetapp.pfx"
COPY ./Presentation.API/aspnetapp.pfx /app/aspnetapp.pfx


# Exponha a porta que a aplicação vai usar
EXPOSE 5116

# Comando para iniciar a aplicação
ENTRYPOINT ["dotnet", "Presentation.API.dll"]
