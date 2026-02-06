using Spectre.Console;

AnsiConsole.Write(
    new FigletText("Hello World!")
        .Centered()
        .Color(Color.Green));

AnsiConsole.MarkupLine("[bold yellow]Welcome[/] to the [underline blue]Spectre.Console[/] HelloWorld app!");
