module CSharpLanguageServer.Program

open Microsoft.Build.Locator
open Argu
open System.Reflection

[<EntryPoint>]
let entry args =
    try
        let argParser = ArgumentParser.Create<Options.CLIArguments>(programName = "csharp-ls")
        let serverArgs = argParser.Parse args

        serverArgs.TryGetResult(<@ Options.CLIArguments.Version @>)
            |> Option.iter (fun _ -> printfn "csharp-ls, %s"
                                             (Assembly.GetExecutingAssembly().GetName().Version |> string)
                                     exit 0)

        MSBuildLocator.RegisterDefaults() |> ignore

        let parseLogLevel (s: string) =
            match s.ToLowerInvariant() with
            | "log" -> LanguageServerProtocol.Types.MessageType.Log
            | "info" -> LanguageServerProtocol.Types.MessageType.Info
            | "warning" -> LanguageServerProtocol.Types.MessageType.Warning
            | "error" -> LanguageServerProtocol.Types.MessageType.Error

        // default the verbosity to warning
        let serverOptions: Server.Options = {
            SolutionPath = serverArgs.TryGetResult(<@ Options.CLIArguments.Solution @>)
            LogLevel = serverArgs.TryGetResult(<@ Options.CLIArguments.LogLevel @>)
                       |> Option.defaultValue "info"
                       |> parseLogLevel
        }

        Server.start serverOptions
    with
    | :? ArguParseException as ex ->
        printfn "%s" ex.Message

        match ex.ErrorCode with
        | ErrorCode.HelpText -> 0
        | _ -> 1  // Unrecognised arguments

    | e ->
        printfn "Server crashing error - %s \n %s" e.Message e.StackTrace
        3
