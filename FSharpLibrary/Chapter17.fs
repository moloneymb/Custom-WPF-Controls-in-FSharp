namespace FSharp.Chapter17
open System
open System.Text.RegularExpressions
open System.Windows
open System.Windows.Markup
open System.Windows.Media

type LinearGradientBrushExtension() =
    inherit MarkupExtension()
    
    let mutable colors = ""
    let mutable angle = 0.
    
    static let gradientStopsRegex = 
        Regex(
            @"(?<GradientStop>\( (?<Offset>\d (\.\d)?) \| (?<Color>\#([0-9a-fA-F]{6} | [0-9a-fA-F]{8}))\))", 
            RegexOptions.Compiled ||| RegexOptions.CultureInvariant ||| RegexOptions.Singleline ||| RegexOptions.IgnorePatternWhitespace ||| RegexOptions.IgnoreCase)
            
    member this.Colors with get() = colors and set(x:string) = colors <- x
    member this.Angle with get() = angle and set(x:float) = angle <- x
    
    member this.LinearGradientBrushExtensions(colors:string) = this.Colors <- colors
    
    override this.ProvideValue(serviceProvider) =
        let m = gradientStopsRegex.Matches(colors) 
        
        let stops = GradientStopCollection()
        
        for _match in m do
            let offset = Double.Parse(_match.Groups.["Offset"].Value)
            let hexColor = _match.Groups.["Color"].Value
            let color = ColorConverter.ConvertFromString(hexColor) :?> Color
            stops.Add(GradientStop(color,offset))
        
        LinearGradientBrush(stops,this.Angle) :> obj