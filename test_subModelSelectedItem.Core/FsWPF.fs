namespace FsWPF

open System
open System.Windows.Data
open Program
open Form

type ComponentsToNameConverter() =
    interface IValueConverter with
    
        member _.Convert(value: obj, targetType: Type, parameter: obj, culture: Globalization.CultureInfo) =
            match value with
            | :? Form.Components as component_ ->
                match component_ with
                | TextBoxA a -> a.Name :> obj
                | TextBoxB b -> b.Name :> obj
                | TextBoxC c -> c.Name :> obj
            | _ -> null

        member _.ConvertBack(value: obj, targetType: Type, parameter: obj, culture: System.Globalization.CultureInfo) =
            raise (NotImplementedException())
