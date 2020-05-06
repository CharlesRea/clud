FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

RUN curl -sL https://deb.nodesource.com/setup_14.x | bash
RUN curl -sS https://dl.yarnpkg.com/debian/pubkey.gpg | apt-key add -
RUN echo "deb https://dl.yarnpkg.com/debian/ stable main" | tee /etc/apt/sources.list.d/yarn.list
RUN apt-get update && apt-get install -y nodejs yarn

COPY ./src/Web/package.json ./src/Web/package.json
COPY ./src/Web/yarn.lock ./src/Web/yarn.lock
WORKDIR ./src/Web
RUN yarn install

WORKDIR /app
COPY ./src/Api/*.csproj  ./src/Api/
COPY ./src/Web/*.csproj  ./src/Web/
COPY ./src/Shared/*.csproj  ./src/Shared/
RUN dotnet restore ./src/Api/Api.csproj

COPY src/ ./src/

WORKDIR ./src/Api
RUN dotnet publish -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Api.dll"]
