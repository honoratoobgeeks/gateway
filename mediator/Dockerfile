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

RUN dotnet restore

# Copie o restante do código da aplicação
COPY . .

# Compile a aplicação
RUN dotnet publish -c Release -o out

# Use uma imagem base do .NET SDK para rodar a aplicação e permitir o uso do dotnet-trace
FROM mcr.microsoft.com/dotnet/sdk:7.0
ENV ASPNETCORE_URLS=http://+:80


# Defina o diretório de trabalho no contêiner
WORKDIR /app

# Copie os arquivos publicados da etapa de build
COPY --from=build-env /app/out .

# Exponha a porta que a aplicação vai usar
EXPOSE 80

# Comando para iniciar a aplicação
ENTRYPOINT ["dotnet", "Presentation.API.dll"]