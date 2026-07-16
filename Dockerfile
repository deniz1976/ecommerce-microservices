ARG PROJECT
ARG ASSEMBLY_NAME

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG PROJECT
WORKDIR /src
COPY . .
RUN dotnet restore ECommerce.sln
RUN dotnet publish ${PROJECT} --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
ARG ASSEMBLY_NAME
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASSEMBLY_NAME=${ASSEMBLY_NAME}
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["sh", "-c", "exec dotnet \"$ASSEMBLY_NAME.dll\""]
