# LiveCSharp
This is p2p Server using tcp and udp protocol.
## Project info

 `./LiveServer`     : Server project for live <br>
 `./LiveClient`     : Client that can communicate with p2p<br>
 `./ChatClient`     : Client that chat with multiple people.(Use test)<br>
 `./AdminConsole`   : WebClient to set playing time of live <br>
 `./Test`           : Test to see if the server works properly <br>
 `./LiveCoreLibrary`: Library for these project<br>

## How to use

### server run

```
cd LiveServer
dotnet run
```

case of docker 
```
docker run -d -p 25565:25565 -p 25501:25501/udp live-server
```

# client run

```
cd LiveClient
dotnet run
```

### server test

using [xunit](https://xunit.net/) that works with c# <br>


```
dotnet test
```

### server build

Use [docker](https://www.docker.com/). Therefore, it is necessary to build a docker environment. <br>
Build based on this [Dockerfile](https://github.com/Taku3939/LiveCSharp/blob/master/Dockerfile)

```
docker build -t "live-server" .
```

if you set Github Actions, image auto generate and push container repository of Gcp based on this [cloudbuild.yml](https://github.com/Taku39339/LiveCSharp/blob/master/cloudbuild.yml)

### chat client run

```
cd ChatClient
dotnet run
```

## License
This software is released under the MIT License, see LICENSE.

## Author

Taku : https://github.com/Taku3939
