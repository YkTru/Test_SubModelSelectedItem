namespace Program

open System
open Serilog
open Serilog.Extensions.Logging
open Elmish.WPF

module TextBoxA =

    type Model = {
        Id: Guid
        Name: string
    }

    let init () = {
        Id = Guid.NewGuid()
        Name = "A_" + Random().Next(10000,100000).ToString()
    }

    type Msg = DummyMsg

    let update msg m =
        match msg with
        | DummyMsg -> m


module TextBoxB =

    type Model = {
        Id: Guid
        Name: string
    }

    let init () = {
        Id = Guid.NewGuid()
        Name = "B_" + Random().Next(10000,100000).ToString()
    }

    type Msg = DummyMsg

    let update msg m =
        match msg with
        | DummyMsg -> m


module TextBoxC =

    type Model = {
        Id: Guid
        Name: string
    }

    let init () = {
        Id = Guid.NewGuid()
        Name = "C_" + Random().Next(10000,100000).ToString()
    }

    type Msg = DummyMsg

    let update msg m =
        match msg with
        | DummyMsg -> m


module Form =

    type Components =
        | TextBoxA of TextBoxA.Model
        | TextBoxB of TextBoxB.Model
        | TextBoxC of TextBoxC.Model

    type Model = {
        Id: Guid
        Components: Components list
        SelectedComponent: Guid option

        TextBoxA_Model: TextBoxA.Model
        TextBoxB_Model: TextBoxB.Model
        TextBoxC_Model: TextBoxC.Model
    }

    module ModelM =
        module TextBoxA =
            let get m = m.TextBoxA_Model
        module TextBoxB =
            let get m = m.TextBoxB_Model
        module TextBoxC =
            let get m = m.TextBoxC_Model    


    let components_Mock: Components list = [
        TextBoxA { Id = Guid.NewGuid(); Name = "A_12345" }
        TextBoxB { Id = Guid.NewGuid(); Name = "B_45678" }
        TextBoxC { Id = Guid.NewGuid(); Name = "C_78901" }
    ]

    let getId (abc: Components) =
        match abc with
        | TextBoxA a -> a.Id
        | TextBoxB b -> b.Id
        | TextBoxC c -> c.Id

    let init () = {
        Id = Guid.NewGuid()
        Components = components_Mock
        SelectedComponent = components_Mock |> List.tryLast |> Option.map getId

        TextBoxA_Model = TextBoxA.init()
        TextBoxB_Model = TextBoxB.init()
        TextBoxC_Model = TextBoxC.init()
    }

    type Msg = 
        | AddTextBoxA
        | AddTextBoxB
        | AddTextBoxC
        | Select of Guid option
        | TextBoxA_Msg of TextBoxA.Msg
        | TextBoxB_Msg of TextBoxB.Msg
        | TextBoxC_Msg of TextBoxC.Msg
        

    let update msg m =
        match msg with
        | AddTextBoxA -> { m with Components = TextBoxA (TextBoxA.init()) :: m.Components}
        | AddTextBoxB -> { m with Components = TextBoxB (TextBoxB.init()) :: m.Components}
        | AddTextBoxC -> { m with Components = TextBoxC (TextBoxC.init()) :: m.Components }
        | Select entityId -> { m with SelectedComponent = entityId }
        | TextBoxA_Msg msg -> { m with TextBoxA_Model = TextBoxA.update msg m.TextBoxA_Model }
        | TextBoxB_Msg msg -> { m with TextBoxB_Model = TextBoxB.update msg m.TextBoxB_Model }
        | TextBoxC_Msg msg -> { m with TextBoxC_Model = TextBoxC.update msg m.TextBoxC_Model }


[<AutoOpen>]
module private Helpers_VM =

    let findComponentById id = function
        | Form.TextBoxA a -> a.Id = id
        | Form.TextBoxB b -> b.Id = id
        | Form.TextBoxC c -> c.Id = id

    let getComponentName = function
        | Form.TextBoxA a -> a.Name
        | Form.TextBoxB b -> b.Name
        | Form.TextBoxC c -> c.Name


[<AllowNullLiteral>]
type TextBoxA_VM (args) =
    inherit ViewModelBase<TextBoxA.Model, TextBoxA.Msg>(args)
    
    member _.Name = base.Get() (Binding.OneWayT.id >> Binding.mapModel _.Name )


[<AllowNullLiteral>]
type TextBoxB_VM (args) =
    inherit ViewModelBase<TextBoxB.Model, TextBoxB.Msg>(args)
    
    member _.Name = base.Get() (Binding.OneWayT.id >> Binding.mapModel _.Name )


[<AllowNullLiteral>]
type TextBoxC_VM (args) =
    inherit ViewModelBase<TextBoxC.Model, TextBoxC.Msg>(args)
    
    member _.Name = base.Get() (Binding.OneWayT.id >> Binding.mapModel _.Name )


[<AllowNullLiteral>]
type Form_VM (args) =
    inherit ViewModelBase<Form.Model, Form.Msg>(args)

    // ### Problem: I guess I should use SubModelSeqKeyedT.id, but what to put as VM?
    // 1- Should I pattern match to get the different TextBoxABC_VMs? 
    // 2- Or Make a Component_VM so it wraps the other VMs because Entities = Components? (here I feel confused honnestly)
    // Nothing I've tried in these directions has worked until now (most probably due to a lack of skill/understanding).
   
    // Sample: "SelectedEntity" |> Binding.subModelSelectedItem("Entities", (fun m -> m.Selected), Select)
    let selectedComponent_Binding =
        Binding.SubModelSeqKeyedT.id (*???*) _.Id
        >> Binding.addLazy (=) // ### add this I guess too?
        >> Binding.mapModel (fun (model: Form.Model) -> model.SelectedComponent)
        >> Binding.mapMsg Form.Select
    

    // ### Hard to tell if working other then at init()
    let selectedItemText_Binding =
        Binding.OneWayT.id >> Binding.mapModel (fun (m: Form.Model) ->
            match m.SelectedComponent with
            | Some selectedId ->
                m.Components
                |> List.tryFind (findComponentById selectedId)
                |> Option.map getComponentName
                |> Option.defaultValue ""
            | None -> "No component selected"
        )

    //• members
    member _.Components = base.Get() (Binding.OneWayT.id >> Binding.mapModel _.Components)

    member _.SelectedComponent
        with get() = base.Get() selectedComponent_Binding
        and set(v) = base.Set(v) selectedComponent_Binding

    member _.SelectedItemText = base.Get() selectedItemText_Binding 

    //• Commands
    member _.AddTextBoxA = base.Get() (Binding.CmdT.setAlways Form.AddTextBoxA )
    member _.AddTextBoxB = base.Get() (Binding.CmdT.setAlways Form.AddTextBoxB )
    member _.AddTextBoxC = base.Get() (Binding.CmdT.setAlways Form.AddTextBoxC )
    
    //• SubVM
    member _.TextBoxA_VM = base.Get() (Binding.SubModelT.req TextBoxA_VM >> Binding.mapModel Form.ModelM.TextBoxA.get >> Binding.mapMsg Form.TextBoxA_Msg)
    member _.TextBoxB_VM = base.Get() (Binding.SubModelT.req TextBoxB_VM >> Binding.mapModel Form.ModelM.TextBoxB.get >> Binding.mapMsg Form.TextBoxB_Msg)
    member _.TextBoxC_VM = base.Get() (Binding.SubModelT.req TextBoxC_VM >> Binding.mapModel Form.ModelM.TextBoxC.get >> Binding.mapMsg Form.TextBoxC_Msg)


module Program =
    let main window =
        let logger =
            LoggerConfiguration()
                .MinimumLevel.Override("Elmish.WPF.Update", Events.LogEventLevel.Verbose)
                .MinimumLevel.Override("Elmish.WPF.Bindings", Events.LogEventLevel.Verbose)
                .MinimumLevel.Override("Elmish.WPF.Performance", Events.LogEventLevel.Verbose)
                .WriteTo.Console()
                .CreateLogger()

        WpfProgram.mkSimpleT Form.init Form.update Form_VM
        |> WpfProgram.withLogger (new SerilogLoggerFactory(logger))
        |> WpfProgram.startElmishLoop window