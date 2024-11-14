# About Drunken Master Mocking Proxy

Drunken Master is designed to make UI development easier and to eliminate blocking issues caused by one or more API features not being available to develop against. It is essentially a reverse proxy (meaning requests are forwarded upstream) with a mocking feature. You can set an upstream URL to e.g. a QA or Dev environment and use the Drunken Master UI to write simple JSON resonse mocks on routes that you specify. Note that it is very basic and a work in progress. Any suggestions or collaboration would be appreciated.

The service is meant to be run locally, either in a Docker container or in a terminal.

## Architecture
The Drunken Master backend uses Microsoft [YARP](https://microsoft.github.io/reverse-proxy/) for request forwarding and acts as a transparent reverse proxy. A request on any route that isn't associated with a mock will simply forward to the upstream URL intact and the response will be returned. Mocks are stored on a MongoDB server in the following format:

```
{
  "_id": {
    "$oid": "672d0553f52377a8941eecf8"
  },
  "enabled": true,
  "method": "Get",
  "mock": "{\"hello\":\"mock\"}",
  "path": "/some/path",
  "routeId": {
    "$binary": {
      "base64": "bAvQ077TS3aaDKm+T9yiDg==",
      "subType": "04"
    }
  }
}
```


The _id field is the internal MongoDb index and isn't returned as part of the DTO. The routeId is used as an identifier in the DTO as a GUID/UUID value. The rest of the fields are fairly self-explanitory. Any GET requests to /foo/api/bar will return a JSON response of `{"hello": "mock"}`. At this point, all responses come back as 200. Custom status codes aren't yet supported.

The user interface uses [React Bootstrap](https://react-bootstrap.netlify.app/) and a [Vite](https://vite.dev/]) based build environament

## Running in a Docker container (recommended)
Running Drunken Master in a docker environment makes deployment much easier as you don't need a local instance of MongoDb. Assuming that you have Docker Engine running on your development machine, do the following.

- Make a copy of the `appsettings.Docker.json.example` file and remove the `.example` part from the file name.
- Change the `UpstreamUrl` value to point to the API you are developing against. Here's an example:
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "DrunkenMaster": {
    "Host": "http://*",
    "Port": "5001",
    "UpstreamUrl": "https://example.com",
    "MongoDbUri": "mongodb://user:pass@database",
    "DbName": "drunken-master"
  }
}
```
- Don't edit any of the other DrunkenMaster settings as the ports and etc. need to jive with the docker environment
- Run `docker-compose up` in a terminal from the root project directory
- Navigate to http://localhost:8080

## Running Locally
You'll need [MongoDb Community Edition](https://www.mongodb.com/try/download/community) database server installed locally.
- Find out what your `ASPNETCORE_ENVIRONMENT` environment variable is set to. It's usually going to be "Local" or "Development" but you can set it to what you want in various ways.
- Make a copy of `appsettings.json` as `appsettings.Development.json` or `appsettings.Local.json` deponding on what you find
- The default values should work for a locally installed MongoDb instance. You should only need to change the upstream URL
- From the ./backend directory run the following in a terminal (or run in your IDE)
    `dotnet run`
- From ./frontend directory run the following in a terminal
    `npm run dev`
- Note that the Vite development server has it's own proxy in place as in the `vite.config.ts` file so be aware of it if you go fiddling with the backend port
    ```
    export default defineConfig({
    server: {
        proxy: {
        "/drunken-master": "http://localhost:5001",
        }
    },
    plugins: [react()]
    })
    ```
- Navigate to the URL shown in the frontend console

## Mocking a route
- Once you navigate to the UI you should see the following page.
  ![alt text](/docs/image.png)

- Now navigate to the Mocks tab and click the plus-sign next to the heading
- Select the method, enter the path, add a JSON formatted response, and click submit
- You should now have a mock route in place like so
![alt text](/docs/image-1.png)

You are winning! Now you have a route at http://localhost:5001/some/path that will respond with 200 OK `{"hello": "drunken master"}`. Build that sweet UI.

## Alternatives
Drunken Master isn't the only mocking, proxying, inebriated kung fu master in town. There are viable alternative to accomplish some of the same.
- [tweak](https://chromewebstore.google.com/detail/tweak-mock-and-modify-htt/feahianecghpnipmhphmfgmpdodhcapi?hl=en) is a Chrome extension that allows you to configure and return mocks right in the browser. Smart.
- [Mock Server](https://github.com/mock-server/mockserver) is a more feature rich but similar approach written in Java
