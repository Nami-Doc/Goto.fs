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

// exception specifications
exception InvalidArgumentCount of int

// helpers
let readLines file = File.ReadLines file |> List.ofSeq
let split (by: char) (str: string) = List.ofArray <| str.Split([|by|], StringSplitOptions.RemoveEmptyEntries)
let list2tuple2 (ary: 'a list) = (List.nth ary 0, List.nth ary 1)
let trim (str: String) = str.Trim()

let readKvFile file sep = List.map (split sep >> list2tuple2) (readLines file)
let serializeKv paths sep = Map.fold (fun cur k v -> cur + k + sep.ToString() + v + "\n") "" paths

let writeFile filename content = File.WriteAllText (filename, content)

// and off we go...
let exec cmd args =
    let psi = new System.Diagnostics.ProcessStartInfo(cmd)
    psi.Arguments <- sprintf "/d %s" args
    psi.UseShellExecute <- false
    let p = System.Diagnostics.Process.Start(psi)
    p.WaitForExit()
    p.ExitCode

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
    | [| "remove"; key |] ->
        if paths.ContainsKey key then
            let paths = Map.remove key paths
            File.Delete (fileNameFor key)
            generate STORAGE_FILE paths
            printf "Removed %s from the dict" key
        else
            printf "%s is not a valid path" key

        EXIT_SUCCESS
    | [| "add"; key; value |] ->
        printf "Assigning %s to %s" key value
        let paths = Map.add key value paths
        generate STORAGE_FILE paths
        EXIT_SUCCESS
    | [| "gen" |] ->
        printf "Regenerating files..."
        generate STORAGE_FILE paths
        EXIT_SUCCESS
    | _ ->
        printf """
Usage:
    goto (displays this message)
    goto add <key> <value> (adds the path <value> with the name <key>)
    goto remove <key> (removes <key> from the list of paths)

Existing paths:
"""

        for KeyValue(k, v) in paths do
            printf "path %s goes to %s\n" k v

        EXIT_SUCCESS