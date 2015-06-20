open System
open System.IO

// constants
let STORAGE_FILE = "paths.txt"
let SPLIT = '='
let EXIT_ERROR = 1
let EXIT_SUCCESS = 0
let CONTENT = """
@echo off
cd "{0}"
"""
let HELP_MESSAGE = """
Usage:
    goto (displays this message)
    goto add <key> <value> (adds the path <value> with the name <key>)
    goto remove <key> (removes <key> from the list of paths)

Existing paths:
"""

// helpers
let readLines file = File.ReadLines file |> List.ofSeq
let split (by: char) (str: string) = List.ofArray <| str.Split([|by|], StringSplitOptions.RemoveEmptyEntries)
let list2tuple2 (ary: 'a list) = (List.nth ary 0, List.nth ary 1)
let trim (str: String) = str.Trim()

let readKvFile file sep = List.map (split sep >> list2tuple2) (readLines file)
let serializeKv paths sep = Map.fold (fun cur k v -> cur + k + sep.ToString() + v + "\n") "" paths

let writeFile filename content = File.WriteAllText (filename, content)

// and off we go...
let fileNameFor key = key + ".bat"

let generate storagePath paths =
    // first off, create the .bat files
    Map.iter (fun k (v: string) -> writeFile (fileNameFor k) (trim <| String.Format(CONTENT, v))) paths
    // then store the file content itself
    writeFile STORAGE_FILE (serializeKv paths SPLIT)

[<EntryPoint>]
let main argv =
    let paths = Map.ofSeq <| readKvFile STORAGE_FILE SPLIT
    match argv with
    | [| "add"; key; value |] ->
        printf "Assigning %s to %s" key value
        generate STORAGE_FILE (Map.add key value paths)
        EXIT_SUCCESS

    | [| "remove"; key |] ->
        if Map.containsKey key paths then
            File.Delete (fileNameFor key)
            generate STORAGE_FILE (Map.remove key paths)
            printf "Removed %s from the dict" key
        else
            printf "%s is not a valid path" key
        EXIT_SUCCESS

    | [| "gen" |] ->
        printf "Regenerating files..."
        generate STORAGE_FILE paths
        EXIT_SUCCESS

    | _ ->
        Console.Write HELP_MESSAGE
        Map.iter (fun k v -> printf "path %s goes to %s\n" k v) paths
        EXIT_SUCCESS