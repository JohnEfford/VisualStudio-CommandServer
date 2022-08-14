# Command-Server

Adds support for running arbitrary commands via file-based RPC. Designed for use with voice-control systems such as Talon. 

# Features 

* On start-up of Visual Studio, creates a communication directory in the users temp directory, called visual-studio-commandServer
* Then waits for the CommandServer.ReceiveCommand command to be issued.
* Once the server is triggered the command server will read the request.json file in the communication directory.
* The command in the JSON file is then executed and a response written to response.json. **Note** that we write the JSON response on a single line, with a trailing newline, so that the client can repeatedly try to read the file until it finds a final newline to indicate that the write is complete.

Note that the command server will refuse to execute a command if the request file is older than 3 seconds.

Requests look as follows:

```JSON
{
  "commandId": "some-command-id",
  "args": ["some-argument"]
  "uuid": "aguid-aguid-aguid-aguid-aguid"
}
```
* The ```uuid``` is used by the command server to create the response file so that the system creating the request files can correlate the ```request.json``` and ```response.json``` files.

* The command servers default command handler recognises commands which start with an ```commandId``` of VSCommand, for example the command file below issues the Find command (similar to pressing ctrl-f), the advantage being that by using the command directly we are not dependant keyboard bindings.

```JSON
{
  "commandId": "VSCommand",
  "args": [
    "Edit.Find",
    " \\doc"
  ],
  "uuid": "708dfb4c-0f68-4b88-b229-4225d21534fa"
} 
```
* The Extension adds a new Output Window which logs data from the extension:
![](2022-08-14-16-26-46.png)
* For details of adding command handlers for different command see [Contracts](\CommandServerContracts\readme.md)