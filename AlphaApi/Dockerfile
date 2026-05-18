FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /app
COPY AlphaApi.csproj .
RUN dotnet restore
COPY . .
ENV ASPNETCORE_URLS=http://+:5221
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
EXPOSE 5221
CMD ["dotnet", "watch", "run", "--non-interactive"]
