# User Management Module

This module controls acess to user information and actions that can be performed on user information. This module supports the following operations:

1. Getting a user (GET /api/user)
2. Creating a user (POST /api/user)
3. Modifying a user (PUT /api/user)
4. Deleting a user (DELETE /api/user)

These operations can be processed via http requests to `<HOST>:<PORT>/api/user`

## Project Structure

+ `/src`
   + contains the actual Azure function app
+ `/tests`
   + contains unit tests 

## Development

There are two ways to run the project

### Visual Studio (RECOMMENDED)

1. Open the User Management solution in Visual Studio (Open `UserManagement/src/src.sln`). 
2. Update your app settings locally by adding the following environmental variables to `UserManagement/src/local.settings.json`. To learn more, read [Microsoft Docs](https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library?tabs=v2%2Ccmd#environment-variables).
```
    "JWT_SECRET_KEY": "XXX",
    "DB_USER": "XXX",
    "DB_NAME": "XX",
    "DB_PORT": "XXXX",
    "DB_PASSWORD": "XXX",
    "DB_HOST": "XX",
    "LOGGING_URL": "XX"
```
At the end, your `local.settings.json` file should look like so:
```
{
    ...
    "Values": {
        ...
        "JWT_SECRET_KEY": "XXX",
        "DB_USER": "XXX",
        "DB_NAME": "XX",
        "DB_PORT": "XXXX",
        "DB_PASSWORD": "XXX",
        "DB_HOST": "XX",
        "LOGGING_URL": "XX"
      ...
    }
}
```
For an example `local.settings.json` file with the actual strings, see [HERE](https://docs.google.com/document/d/1S7zNXG8oSXmgGPoXYGBhGaOrKz-gJpsgRqvoTCJq2EM/edit#heading=h.v379irezafrq).

3. To start the function app locally, press the play button on the top left button, or press `Run > Start Debugging`. (`Run > Start Without Debugging` should also work.)
4. The console should list the function endpoints available to invoke via an HTTP request.

### Commandline
If you have edited your `local.settings.json` file as described in the previous `Visual Studio` section, you can skip to step 2 and just run `func start --build`.
1. Open the terminal and follow the instructions in [Supporting Dev/Prod Configurations](https://docs.google.com/document/d/13Nr3LMvaGWnZDy4Ml3qSfRj_WOOqy3JVxwGZzPo63CQ/edit#heading=h.r4t95qeccwvx) to add `DB_HOST`, `DB_NAME`, `DB_PASSWORD`, `DB_PORT`, `DB_USER`, `JWT_SECRET_KEY` variables to your local environment.
2. To run this module, cd into `src` and run `func start --build --csharp` in terminal. This will require installing the [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools#installing) installed beforehand.

## Test

### Visual Studio (WINDOWS ONLY)
1. Follow these [instructions](https://docs.microsoft.com/en-us/visualstudio/test/configure-unit-tests-by-using-a-dot-runsettings-file?view=vs-2019#specify-a-run-settings-file-in-the-ide) for setting up environment variables for unit testing. These instructions ONLY work for WINDOWS machines.
2. Open the User Management solution in Visual Studio (Open `UserManagement/src/src.sln`). 
3. To run ALL unit tests, press `Run > Run Unit Tests`. 
4. To run specific tests, you can also press `View > Tests`, and a sidebar will appear with the tests listed for you to run individually.

### Terminal
1. Open the terminal and follow the instructions in [Supporting Dev/Prod Configurations](https://docs.google.com/document/d/13Nr3LMvaGWnZDy4Ml3qSfRj_WOOqy3JVxwGZzPo63CQ/edit#heading=h.r4t95qeccwvx) to add `DB_HOST`, `DB_NAME`, `DB_PASSWORD`, `DB_PORT`, `DB_USER`, `JWT_SECRET_KEY` variables to your local environment.
2. In the SAME terminal, `cd` into the `tests` and run `dotnet test`.
3. If you want to combine steps 1 and 2, you can write a script that will both set the environment variables and run the test app at once. An example script containing the actual strings can be found [HERE](https://docs.google.com/document/d/1S7zNXG8oSXmgGPoXYGBhGaOrKz-gJpsgRqvoTCJq2EM/edit#heading=h.9eob6i7c2hi9).

For more information, see the [xUnit documentation](https://xunit.net/docs/getting-started/netcore/cmdline).
