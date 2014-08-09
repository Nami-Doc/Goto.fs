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

exception InvalidArgumentCount of int

[<EntryPoint>]
let main argv =
    let paths = dict <| List.map (split '=' >> list2tuple2) (readLines storageFile)
    match argv with
    | [| key |] ->
        printf "Going to %s" key
    | [| "--remove"; key |] ->
        printf "Removing %s from the dict" key
    | [| "--add"; key; value |] ->
        printf "Assigning %s to %s" key value
    //| [||] ->
    | _ ->
        printf """
        Utilisation:
            goto (Affiche ce message et la liste des chemins)
            goto <key> (Va vers le chemin appelé <key>)
            goto --add <key> <value> (Enregistre <value> au nom <key>)
            goto --remove <key> (Retire <key> des chemins)
        """
        for KeyValue(k, v) in paths do
            printf "%s=%s" k v
    // TODO: add argv to paths
    // TODO: save paths
    System.Console.ReadKey() |> ignore
    0 // retourne du code de sortie entier
