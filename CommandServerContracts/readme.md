# Command Server Contracts

This package supplies the interface required to add a new command server command handler

```CSharp
    /// <summary>
    /// Command dispatch which plugs into the Command Server plug in
    /// </summary>
    public interface ICommandServerDispatch
    {
        
        string Name { get; }

        /// <summary>
        /// Can this implementation process the command. Implementations are selected using the following rule
        /// 1 Select the implementation which name forms part of the command and can handle the the command
        /// 2 Select he first implementation in the pipeline which says it can handle the command
        /// </summary>
        /// <param name="commandId">The command id to be handled</param>
        /// <returns>True if the CommandServerDispatch can handle the payload.</returns>
        Boolean CanProcess(string  commandId);
        
        /// <summary>
        /// Process the command and return any results. Most commands will not return any results.
        /// </summary>
        /// <param name="payload">JSON representation of the command</param>
        /// <returns>JSON representation of the result</returns>
        Task<CommandServerResult> Process(JObject payload);

    }
}
```

## Description

* To add a class into the [Command Server]() you need to create a Visual Studio extension which implements ```ICommandServerDispatch``` 
* The extension needs to be signify that it is a MEF Component
* The extension needs to implement the interface above.
* The extension needs to make the implementation as exported via MEF

The example below is a minimum implication which  

```CSharp
Export(typeof(ICommandServerDispatch))]
public class CursorlessCommandHandler: ICommandServerDispatch
{
    public string Name { get; } = "cursorless";

    public bool CanProcess(
        string commandId) => commandId == "cursorless.command";
   

    public Task<CommandServerResult> Process(
        JObject payload)
    {
        ///The first parameter of the CommandServerResult will be logged in the Command Server output window.
        ///The second parameter is any data to write to the response file
        return Task.FromResult(new CommandServerResult("Handled by cursorless command handler",
            CommandServerResult.None));
    }
}
```

* A command sent to the command server will be handled by zero or one command handlers
* In the event that multiple handlers say they can handle a command:
   * We select the first command handler which has its name as part of the command.
   * This should stop 'catch all' command handlers from stealing your commands.


