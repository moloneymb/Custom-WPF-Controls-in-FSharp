namespace FSharp.Chapter16

open System
open System.Collections.Generic
open System.ComponentModel
open System.Windows

open System.Globalization
open System.Windows.Media
open System.Windows.Data

type AddressBook() =
    let mutable contactNames = List<string>()

    let propertyChange = Event<_,_>()
    
    let Notify obj s = propertyChange.Trigger(obj, PropertyChangedEventArgs(s))
    
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member this.PropertyChanged = propertyChange.Publish

    member this.ContactNames 
        with get() = contactNames
        and set(x:List<string>) = contactNames <- x; Notify this "ContactNames"
        
type LineChartModel() =
    let mutable dataPoints = List<Point>()
    
    do
        let variation = 0.1
        let random = new Random()
        let totalPoints = 100
        {0 .. totalPoints - 1 } |> Seq.fold (fun acc i ->
                                        let y = match acc + (-variation + 2. * variation * random.NextDouble()) with
                                                | y when y <= 0.0 -> variation
                                                | y when y >= 1.0 -> 1. - variation
                                                | y -> y
                                        dataPoints.Add(Point(float i / (float totalPoints - 1.), y))
                                        y)  0.5 |> ignore
                                        
    member this.DataPoints with get() = dataPoints and set(x:List<Point>) = dataPoints <- x
                                        
        
type LineSegmentConverter() =
    interface IMultiValueConverter with
        member this.Convert(values,targetType,parameter,culture) = 
            let prev = (if (values.[0] = null) then values.[1] else values.[0]) :?> Point
            let curr = values.[1] :?> Point
            
            if (values.[2] = DependencyProperty.UnsetValue || values.[3] = DependencyProperty.UnsetValue) 
                then null
                else
                    let maxWidth =  values.[2] :?> float
                    let maxHeight = values.[3] :?> float
                
                    LineGeometry(Point(prev.X * maxWidth, prev.Y * maxHeight), Point(curr.X * maxWidth, curr.Y * maxHeight)) :> obj
                    
        member this.ConvertBack(_,_,_,_) = failwith "Not Implemented Exception"
        

    
    