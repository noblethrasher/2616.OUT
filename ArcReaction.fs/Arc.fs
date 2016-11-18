namespace ArcReaction.fs
    
    open ArcRx
    open System
    open System.Web

    [<AutoOpen>]
    module ArcReaction =

        let inline str o = o.ToString()

        let (|Int|NonNegativeInt|Word|Guid|) obj =
            let s = obj.ToString()
            match Int32.TryParse(s) with
            | (true, n) -> if n >= 0 then Int n else NonNegativeInt n
            | _  -> 
                    match Guid.TryParse(s) with
                    | (true, g) -> Guid g
                    | _ -> Word s


        let inline rep (r : 't when 't :> Representation) = r :> Representation

        let inline write (s : string) = { new ArcRx.PlainText.Representation()
                                            with override x._ProcessRequest ctx = ctx.Response.Write(s) } |> rep
    
        type ReadAttempt<'a> =
            | Success of 'a
            | Failed of string

        type FormBuilder () =
            member x.Bind (s, f) =
                match s with
                | Success v ->
                    let u = f v
                    match u with 
                    | Success t -> Success t
                    | Failed txt -> Failed txt
                | Failed txt -> Failed txt

            member x.Return u = Success u

        let read = FormBuilder()

        let inline get_whatever s (req : HttpRequestBase) (f : string -> bool * 'a) error =
            let s' = req.[s]

            if String.IsNullOrWhiteSpace s'
            then
                Failed ("Expected a value for '"  + s + "'.")
            else
                match f s' with 
                | true, s'' -> Success s''
                | _ -> 
                    Failed (if error <> null then error else ("Unable to convert the value '"+ s' + "' to " + (typeof<'a>).FullName) )

        let get_non_negative_int s req = get_whatever s req Int32.TryParse null
        let get_double s req = get_whatever s req Double.TryParse null
        let get_date s req =  get_whatever s req DateTime.TryParse null
        let get_float s req = get_whatever s req Single.TryParse null
        let get_decimal s req =  get_whatever s req Decimal.TryParse null
        let get_guid s req = get_whatever s req Guid.TryParse null
        let get_byte s req =  get_whatever s req Byte.TryParse null        

        let get_non_empty_string s (req : HttpRequestBase) =
            let s' = req.[s]
            if String.IsNullOrWhiteSpace s' 
                then Failed ("Expected a non-empty string for key: '" + s + "'")
                else Success s'

        let get_non_empty_int_list s (req : HttpRequestBase) =
            match get_non_empty_string s req with
            | Success t ->
                let rec f strings =
                    match strings with
                    | [] ->  []
                    | x::xs ->
                        match x with
                        | true, x' -> x' :: f xs
                        | _ -> []

                seq { for x in t.Split(',') -> Int32.TryParse(x) } 
                |> Seq.toList |> f
                |> function | x::xs -> Success (x::xs) | [] -> Failed ("'"+s+"' does not containt a non-empty list of integers.")

                
            | Failed msg -> Failed msg
        
        type HttpRequestBase with
            member x.get_non_negative_int s = get_non_negative_int s x
            member x.get_double s = get_double s x
            member x.get_date s = get_date s x
            member x.get_float s =  get_float s x
            member x.get_decimal s = get_decimal s x
            member x.get_guid s =  get_guid s x
            member x.get_get_byte s = get_byte s x

            member x.get_non_empty_int_list s = get_non_empty_int_list s x
            member x.get_non_empty_string s =  get_non_empty_string s x


            



