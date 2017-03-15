# InteractivePrompt

This is a very small library to help implement your own REPL.  

![image](http://cint.io/interactiveprompt.gif)

## Installation
Add the library from NuGet, or simply extend what's in this repo.

## Example
The gif above is based on the code below.  Simply provide the prompt, startup message, and function to handle the input, and you're off!

```
static void Main(string[] args)
{
    var prompt = "cool> ";
    var startupMsg = "Welcome to my interactive Prompt!";
    InteractivePrompt.Run(
        ((strCmd, listCmd) =>
        {
            var handleInput = "(((--> " + strCmd + " <--)))";
            return handleInput + Environment.NewLine;
        }), prompt, startupMsg);
}
```
