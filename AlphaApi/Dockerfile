FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /app
COPY AlphaApi.csproj .
RUN dotnet restore
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
COPY . .
ENV ASPNETCORE_URLS=http://+:5221
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
EXPOSE 5221
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh
CMD ["/entrypoint.sh"]
