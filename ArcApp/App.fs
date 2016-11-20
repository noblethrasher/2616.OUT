namespace ArcApp
    
   open System
   open System.Web
    
   type appstate = AppRoute.AppState<MessageToken>     
   
   [<AbstractClass>]
   type page() =
        inherit appstate()
            
        interface Get.Allowed with
            member x.Accept (get, ctx) = x.GetRepresentation ctx
    

   type Location =
    | Found of string
    | Permenant of string
    | SeeOther of string  
    | Error of Representation     

   [<AbstractClass>]
   type post() =
        inherit appstate()
            
        abstract member GetRedirectRepresentation : HttpContextEx -> Location

        override x.GetRepresentation ctx = 
            
            let redirect loc code = { new Representation() 
                                        with member x.ProcessRequest ctx' = 
                                                    ctx'.Response.AddHeader("Location", loc)
                                                    ctx'.Response.StatusCode <- code }
            
            let loc = x.GetRedirectRepresentation ctx
                   
            match loc with
            | Found s -> redirect s 302
            | Permenant s -> redirect s 301
            | SeeOther s -> redirect s 303
            | Error rep -> rep

        interface Post.Allowed with
            member x.Accept (get, ctx) = x.GetRepresentation ctx    
    
    module Foo =
        
        type Jay() =
            inherit post()
                override x.GetRedirectRepresentation ctx = 
                    
                    let k  = read { let req =  ctx.Request
                            
                                    let! id = req.get_non_negative_int "id"
                                    let! friends = req.get_non_empty_int_list "friends"
                            
                                    return "sdfsdf" }
                    
                    
                    match k with
                    | Success s -> Location.Found ("https://iso.okstate.edu?k" + s)
                    | Failed err -> Location.Error null

        type Scoobie(name : string, role  : string,  friends : int list) as this =
            inherit JSON.Representation()
                do
                    this.Add("fullname", name)
                    this.Add("role", role)
                    this.Add("age", 19)
                    this.Add("active", true)
                    

                    if name = "Buffy Summers" then this.Add("so", new Scoobie("Spike", "Poet", []))


                    match seq { for n in friends -> n |> JSON.Representation.JSONValue.op_Implicit } |> Seq.toList
                        with
                        | [] -> ()
                        | x::xs -> this.Add("friends", x::xs |> Seq.toArray)
        
        type Baz() =
            inherit page()
                override x.GetRepresentation ctx = Arc.vb.Represent(Arc.vb.HelloScooby(4)) :> Representation
        
        type Bar() =
            inherit page()
                override x.GetRepresentation ctx = Arc.vb.Represent(Arc.vb.HelloScooby(3)) :> Representation

        type Foo() =
            inherit page()
                
                override x.Consider msg = 
                        match str msg with
                        | "bar" -> Bar() :> appstate
                        | "baz" -> Baz() :> appstate
                        | "jay" -> Jay() :> appstate
                        | "scoob" -> { new page() with override x.GetRepresentation ctx = new Scoobie("Buffy Summers", "The Slayer", [1; 3; 7; 99]) |> rep } :> appstate
                        | "types" ->
                            
                            { new page() with
                                  override x.GetRepresentation ctx = 
                                    ctx.Request.AcceptTypes 
                                    |> (fun xs -> System.String.Join(",", xs)) 
                                    |> write } :> appstate

                        | _ ->  null
                                
                override x.GetRepresentation ctx = AdHocRepresentation((fun c -> c.Response.Write("Foo!!!")))  :> Representation

        type Root() =
            inherit page()

                override x.Consider msg = if (str msg) = "foo" then (Foo() :> appstate ) else null
                
                override x.GetRepresentation ctx = write "Home..."
                
       

        
                






