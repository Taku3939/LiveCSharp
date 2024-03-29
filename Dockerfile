FROM mcr.microsoft.com/dotnet/sdk:5.0 as builder
COPY ./LiveServer /app/LiveServer
COPY ./LiveCoreLibrary /app/LiveCoreLibrary
RUN dotnet publish -c Relase -o /app/LiveServer/build /app/LiveServer/LiveServer.csproj
RUN rm /app/LiveServer/build/LiveServer.pdb

FROM mcr.microsoft.com/dotnet/sdk:5.0
COPY --from=builder /app/LiveServer/build /usr/local/LiveServer
ENV PATH $PATH:/usr/local/LiveServer
CMD ["/bin/bash", "-c", "dotnet /usr/local/LiveServer/LiveServer.dll"]