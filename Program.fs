open System
open System.IO

// constants
let STORAGE_FILE = "paths.txt"
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

let readKvFile file sep = List.map (split sep >> list2tuple2) (readLines file)
let serializeKv paths sep = "1"

// and off we go...
let exec cmd args =
    let psi = new System.Diagnostics.ProcessStartInfo(cmd)
    psi.Arguments <- sprintf "/d %s" args
    psi.UseShellExecute <- false
    let p = System.Diagnostics.Process.Start(psi)
    p.WaitForExit()
    p.ExitCode

let fileNameFor key = key + ".bat"

let trim (str: String) = str.Trim()

let generate storagePath paths =
    // first off, create the .bat files
    for KeyValue(k, v : string) in paths do
        use file = new StreamWriter(fileNameFor k, true)
        printf "hey it's %s" (fileNameFor k)
        file.WriteLine (trim <| String.Format(CONTENT, v))
    // then store the file content itself
    use file = new StreamWriter(STORAGE_FILE, true)
    file.WriteLine (serializeKv storagePath paths)

[<EntryPoint>]
let main argv =
    let paths = dict <| readKvFile STORAGE_FILE '='
    match argv with
    | [| "remove"; key |] ->
        if paths.ContainsKey key then
            assert (paths.Remove key)
            File.Delete (fileNameFor key)
            generate STORAGE_FILE paths
            printf "Removed %s from the dict" key
        else
            printf "%s is not a valid path" key

        EXIT_SUCCESS
    | [| "add"; key; value |] ->
        printf "Assigning %s to %s" key value
        paths.Add (key, value)
        generate STORAGE_FILE paths
        EXIT_SUCCESS
    | [| "gen" |] ->
        ignore <| generate paths
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
            printf "path %s goes to %s" k v

        EXIT_SUCCESS