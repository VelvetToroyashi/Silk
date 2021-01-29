# Silk! 
Silk is a simple, and, the fastest Discord bot written in C# until proven otherwise. Built on top of DSharpPlus for Discord API interactions. The goal of Silk! is be not only a great bot, but fill in the gaps that certain bots have. A bot your members will want to use, while not being a cookie-cutter game, moderation, or entertainment bot. Silk! is also made with large guilds in mind. Feel free to join the [Silk! server](https://discord.gg/HZfZb95) to ask any questions you may have or any general support you may need. Want this bot on your server? [Feel free to invite it!](https://discord.com/api/oauth2/authorize?client_id=721514294587424888&permissions=502656214&scope=bot)

[![CodeQuality](https://www.codefactor.io/repository/github/velvetthepanda/silk/badge)](https://www.codefactor.io/repository/github/velvetthepanda/silk)
![CodeSize](https://img.shields.io/github/languages/code-size/VelvetThePanda/Silk)
![Lines of code](https://img.shields.io/tokei/lines/github/VelvetThePanda/Silk)
![GitHub closed issues](https://img.shields.io/github/issues-closed-raw/VelvetThePanda/Silk)
![Discord](https://img.shields.io/discord/721518523704410202)
<br/>

---

<br/>

## Development 
Silk! Is an open source bot, obviously, and if you know C#, you're more than welcome to contribute! If you're on the server, and your PR gets merged, you get a nice light blue role on the server to signify your contributions. Active contributors are given a special role as well.

### **Resources**
- [PostgreSQL](https://www.postgresql.org/)
- [App Secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Discord Developer Portal](https://discord.com/developers)

### **Database**
Silk uses [PostgreSQL](https://www.postgresql.org/) for its backend database store. So if you don't have PostgreSQL installed, make sure to install it before trying to run the Visual Studio solution.

### **Configuration / Secrets Management**
- To manage storage of the needed ```Discord Token``` and ```Database Connection String```, Silk uses an ```appSettings.json``` file.

#### **Default**
1. When you clone Silk, before running the Visual Studio solution, you'll need to create this ```appSettings.json``` file in the following directory: ```Silk\src\Silk.Core```

2. Now that the configuration file has been created, you'll need to fill out the structure:
    - ```
      {
         "ConnectionStrings": {
            "dbConnection": "",
            "BotToken": ""
         }
      }
      ```
   - For the ```dbConnection``` property, in the double quotes, add your connection string.
      - Note: The PostgreSQL connection string will look something like this: ```Server=;Database=;Username=;Password=```
   
   - For the ```BotToken``` property, in the double quotes, add your ```Discord Bot Token```. 
     - If you don't have your token off-hand, you can get it from the [Discord Developer Portal](https://discord.com/developers); Select your application once logged in, then select ```Bot``` in the menu, and you should be able to Reveal your token from there.

#### **App Secrets**
- The alternative, and more recommended (but still **NOT** suitable for ```Production```) for managing the configuration file is using ```User Secrets```

- The great advantage of using User Secrets, is that the file is stored in a separate location from the project tree, and because of this, secrets also aren't checked into source control.

1. In Silk's Core project, a ```UserSecretsId``` is defined:
   - ```
     <UserSecretsId>VelvetThePanda-SilkBot</UserSecretsId>
     ```
2. The location of the ```secrets``` file is different between Operating Systems, but in a system-protected user profile folder on your computer.
   - Windows: ```%APPDATA%\Microsoft\UserSecrets\VelvetThePanda-SilkBot\secrets.json```
   - Linux / Mac: ```~/.microsoft/usersecrets/VelvetThePanda-SilkBot/secrets.json```
3. The structure is the same for the [Default](#default) - ```appSettings.json``` approach
   - Just fill out the skeleton and you're good to go!


### Running the Project

- If you're starting fresh, just cloned the repo, then you'll need to make sure you've done the needed [configuration](#configuration--secrets-management), before running or debugging Silk.


- If you're already using Silk, you may need to do any or all of the following to ensure that you have everything you need to run the latest and greatest!

<br/>

#### Update your PostgeSQL database to the latest migration
1. If you have `dotnet ef` command line tools installed, you can run the following command in the ```src/Silk.Core``` folder to apply the latest migration
   - ```
     dotnet ef database update
     ``` 
   - You can install the `dotnet ef` command line tool by following this document [Entity Framework Core Tools CLI](https://docs.microsoft.com/en-us/ef/core/cli/dotnet)

2. If you're using Visual Studio you can this command using the ` Package Manager Console`
   - ```
     Update-Database
     ``` 
   - You can install the `Package Manager Console` tools by following this document [Entity Framework Core Tools - Package Manager Console in Visual Studio](https://docs.microsoft.com/en-us/ef/core/cli/powershell)

---

<br/>

## ⚠️ A NOTE ABOUT SELF-HOSTING ⚠️
### Self-hosting Silk! is easy. Instructions have been provided to self-host locally.
## Docker: 
To run Silk! as a docker container, you can simply create an `appSettings.json` file in the root directory, and run `docker pull velvetthepanda/silk`, assuming you have a postgres database running already. If not, you can download a pre-configured `docker-compose` file [here](https://files.velvetthepanda.dev/docker-compose.yml). 

Both Postgres and Silk! will have to initialize on the first run, and if run without the `-d` flag, you will see an exception thrown as Silk! tries to access tables that don't exist (signifying it needs to migrate). Fear not, running `docker-compose up -d` again will have Postgres configured, and Silk! will create requisite tables if they do not already exist upon startup.

![](https://velvet.is-ne.at/mQW3nC.png)
