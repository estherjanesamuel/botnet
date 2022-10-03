# BOTNET
Telegram Bot written in .NET

## Build and Run
#### Visual Studio

1. Open `BotNet.sln` in Visual Studio
2. Right click `BotNet` project in Solution Explorer, select `Manage User Secrets`
3. In the opened `secrets.json`, add your bot token to following properties:

```json
{
  "BotOptions:AccessToken": "yourtoken",
  "HostingOptions:UseLongPolling": true
}
```

4. Run the project by pressing F5

#### Visual Studio Code

1. Clone the project ` git clone https://github.com/teknologi-umum/botnet.git`
2. Change directory to `botnet` 
```powershell
PS D:\project\github>cd .\botnet\
```
3. Open `botnet` directory in Visual Studio Code with type `code .` on your terminal.
4. Open VSCode terminal with `ctrl + j` and run :  
```powershell
PS D:\project\github\botnet>dotnet build
```
5. Right click `BotNet.csproj` (  make sure you've already installed [.NET Core User Secrets](https://marketplace.visualstudio.com/items?itemName=adrianwilczynski.user-secrets) extensions ), then select `Manage User Secrets`
6. In the opened `secrets.json`, add your bot token to following properties:

```json
{
  "BotOptions:AccessToken": "yourtoken",
  "HostingOptions:UseLongPolling": true
}
```
7. Navigate to the BotNet directory. and Run the project by `dotnet run`
```powershell
PS D:\project\github\botnet\BotNet>dotnet run
```