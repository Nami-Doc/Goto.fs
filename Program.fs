// Obtenez des informations sur F# via http://fsharp.net
// Voir le projet 'Didacticiel F#' pour obtenir de l'aide.

open System
open System.IO

// # Config
let storageFile = "paths.txt"

// # Helpers
let readLines file = File.ReadLines file |> List.ofSeq
let split (by: char) (str: string) = List.ofArray <| str.Split([|by|], StringSplitOptions.RemoveEmptyEntries)
let list2tuple2 (ary: 'a list) = (List.nth ary 0, List.nth ary 1)

let exec cmd args =
    let psi = new System.Diagnostics.ProcessStartInfo(cmd)
    psi.Arguments <- sprintf "/d %s" args
    psi.UseShellExecute <- false
    let p = System.Diagnostics.Process.Start(psi)
    p.WaitForExit()
    p.ExitCode

exception InvalidArgumentCount of int

[<EntryPoint>]
let main argv =
    let paths = dict <| List.map (split '=' >> list2tuple2) (readLines storageFile)
    match argv with
    | [| key |] ->
        let (found, value) = paths.TryGetValue(key)
        if found then
            ignore <| exec "mycd.bat" value
            Environment.CurrentDirectory <- value
            0
        else
            printf "Unable to find path %s" key
            1 // error !
    | [| "--remove"; key |] ->
        printf "Removing %s from the dict" key
        0
    | [| "--add"; key; value |] ->
        printf "Assigning %s to %s" key value
        0
    //| [||] ->
    | _ ->
        printf """
Utilisation :
    goto (Affiche ce message et la liste des chemins)
    goto <key> (Va vers le chemin appelé <key>)
    goto --add <key> <value> (Enregistre <value> au nom <key>)
    goto --remove <key> (Retire <key> des chemins)

Chemins existants :
        """
        for KeyValue(k, v) in paths do
            printf "nom: %s, destination: %s" k v

        0