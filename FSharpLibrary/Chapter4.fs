namespace FSharp.Chapter4

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media;
open System.Windows.Markup;


module tmp =
    type UIElementCollection with
        member public this.to_seq = {0 .. this.Count - 1} |> Seq.map(fun i -> this.[i])        
open tmp
         
[<ContentProperty("Children")>]
type ZOrderControl() as this =
    inherit Panel()
    let childSize = Size(200.,100.)
    let mutable children = None
    let mutable isOrderReversed = false
    let mutable offset = 50.
    do
        children <- Some(UIElementCollection(this, this))
    
    // properties     
    member this.Children with get() = match children with | Some(v) -> v | _ -> failwith "initialization fail"
    member this.IsOrderReversed with get() = isOrderReversed and set(x) = isOrderReversed <- x
    member this.Offset with get() = offset and set(x) = offset <- x            
            
    override this.MeasureOverride(_constraint) =
        if (_constraint.Width = Double.PositiveInfinity || _constraint.Height = Double.PositiveInfinity) 
            then Size.Empty  
            else 
                this.Children.to_seq |> Seq.iter(fun c -> c.Measure(childSize))
                _constraint
            
    override this.ArrangeOverride(finalSize) = 
        this.Children.to_seq |> Seq.iteri(fun i c -> c.Arrange(Rect(Point((float i)*offset, 0.), childSize)))
        finalSize
    
    override this.GetVisualChild(index) = 
        if (index < 0 || index >= this.Children.Count)
            then failwith "Bad Index"
            else
                if (isOrderReversed) 
                    then this.Children.[this.Children.Count - 1 - index] :> Visual
                    else this.Children.[index] :> Visual
                
    
    override this.VisualChildrenCount = this.Children.Count
    
    
[<ContentProperty("Children")>]
type ZOrderControl2() as this =
    inherit Panel()
    
    
    static let isOrderReversedProperty = 
            DependencyProperty.Register("IsOrderReversed", typeof<bool>, typeof<ZOrderControl2>,  
                                        FrameworkPropertyMetadata(
                                            false, FrameworkPropertyMetadataOptions.AffectsArrange,PropertyChangedCallback(
                                                fun d e -> (ZOrderControl2.reparent(d :?> ZOrderControl2)))))
      
    let childSize = Size(200.,100.)
    let mutable children = None
    let mutable offset = 50.
    
   
   
    do
        children <- Some(UIElementCollection(this, this))
    
    // properties     
    member this.Children with get() = match children with | Some(v) -> v | _ -> failwith "initialization fail"
    
    static member private reparent(control:ZOrderControl2) =
        for i in [0..control.Children.Count-1] do
            control.RemoveVisualChild(control.Children.[i])
        for i in [0..control.Children.Count-1] do
            control.AddVisualChild(control.Children.[i])
            
    
    member public this.IsOrderReversed
        with get()      = this.GetValue(isOrderReversedProperty) :?> bool
        and  set(x:bool) = this.SetValue(isOrderReversedProperty,x) |> ignore 
            
    member this.Offset with get() = offset and set(x) = offset <- x            
    
            
    override this.MeasureOverride(_constraint) =
        if (_constraint.Width = Double.PositiveInfinity || _constraint.Height = Double.PositiveInfinity) 
            then Size.Empty  
            else 
                this.Children.to_seq |> Seq.iter(fun c -> c.Measure(childSize))
                _constraint
            
    override this.ArrangeOverride(finalSize) = 
        this.Children.to_seq |> Seq.iteri(fun i c -> c.Arrange(Rect(Point((float i)*offset, 0.), childSize)))
        finalSize
    
    override this.GetVisualChild(index) = 
        if (index < 0 || index >= this.Children.Count)
            then failwith "Bad Index"
            else
                if (this.IsOrderReversed) 
                    then this.Children.[this.Children.Count - 1 - index] :> Visual
                    else this.Children.[index] :> Visual
                
    
    override this.VisualChildrenCount = this.Children.Count
    
[<ContentProperty("Children")>]
type VanishingPointPanel() as this =
    inherit Panel()
    
    static let zFactorProperty = 
            DependencyProperty.Register(
                "ZFactor", typeof<double>, typeof<VanishingPointPanel>,  
                FrameworkPropertyMetadata(
                    0.8, FrameworkPropertyMetadataOptions.AffectsArrange ||| 
                    FrameworkPropertyMetadataOptions.AffectsMeasure))
    
    static let itemHeightProperty = 
            DependencyProperty.Register(
                "ItemHeight", typeof<double>, typeof<VanishingPointPanel>,  
                FrameworkPropertyMetadata(
                    30., FrameworkPropertyMetadataOptions.AffectsArrange ||| 
                    FrameworkPropertyMetadataOptions.AffectsMeasure))

    member public this.ZFactor
        with get()         = this.GetValue(zFactorProperty) :?> double
        and  set(x:double) = this.SetValue(zFactorProperty, x) |> ignore 
    
    member public this.ItemHeight
        with get()         = this.GetValue(itemHeightProperty) :?> double
        and  set(x:double) = this.SetValue(itemHeightProperty, x) |> ignore       
      
    override this.MeasureOverride(_constraint) =
        if (_constraint.Width = Double.PositiveInfinity || _constraint.Height = Double.PositiveInfinity) 
            then Size.Empty  
            else 
                this.InternalChildren.to_seq |> Seq.iter(fun c -> c.Measure(Size(_constraint.Width, this.ItemHeight)))
                Size(_constraint.Width, this.ItemHeight * (double this.InternalChildren.Count))
            
    override this.ArrangeOverride(finalSize) = 
        let CalculateRect (panelSize:Size) (index:int) =
            let zFactor = Math.Pow(this.ZFactor, (float index))
            let itemSize = Size(panelSize.Width*zFactor, this.ItemHeight*zFactor)
            let left = (panelSize.Width - itemSize.Width) * 0.5
            let top = panelSize.Height - ([0 .. index] |> List.map(fun i -> Math.Pow(this.ZFactor, (float i)) * this.ItemHeight) |> List.sum)
            Rect(Size = itemSize, Location = Point(left,top))
        
        
        this.Children.to_seq |> Seq.iteri (fun i c -> c.Arrange(CalculateRect finalSize i))
        finalSize

[<ContentProperty("Children")>]
type WeightedPanel() as this =
    inherit Panel()
    
    static let weightProperty = 
        DependencyProperty.RegisterAttached(
            "Weight", typeof<double>, typeof<WeightedPanel>, 
                new FrameworkPropertyMetadata(
                    1., 
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure |||
                    FrameworkPropertyMetadataOptions.AffectsParentArrange))
    
    static let orientationProeprty = 
        DependencyProperty.RegisterAttached(
            "Orientation", typeof<Orientation>, typeof<WeightedPanel>, 
                new FrameworkPropertyMetadata(
                    Orientation.Horizontal, 
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure |||
                    FrameworkPropertyMetadataOptions.AffectsParentArrange))
    
    member private this.CalculateItemRects (f:UIElement->Rect->unit) (size:Size) =
        let weightSum = this.InternalChildren.to_seq |> Seq.map(fun c -> c.GetValue(weightProperty) :?> double) |> Seq.sum 
        this.InternalChildren.to_seq |> Seq.fold (fun acc c -> 
            let normalWeight = (c.GetValue(weightProperty) :?> double) /  weightSum
            let (rect,dist) = 
                 if (this.Orientation = Orientation.Horizontal)
                    then 
                        let width = size.Width*normalWeight
                        let rect = Rect(acc,0.,width,size.Height)
                        (rect, width)
                    else
                        let height = size.Height*normalWeight
                        let rect = Rect(0.,acc,size.Width,height)
                        (rect, height)
            f c rect
            acc + dist) 0. |> ignore
    
    member public this.Weight with set(x:double) = this.SetValue(weightProperty,x) |> ignore 
    
    
    static member public SetWeight(bar:ProgressBar, progress:double) = bar.SetValue(weightProperty, progress)|> ignore
    static member public GetWeight(obj:DependencyObject) = (obj :?> UIElement).GetValue(weightProperty) :?> double
    
    member public this.Orientation
        with get()              = this.GetValue(orientationProeprty):?> Orientation
        and  set(x:Orientation) = this.SetValue(orientationProeprty,x) |> ignore 

    override this.MeasureOverride(_constraint) =
        if (_constraint.Width = Double.PositiveInfinity || _constraint.Height = Double.PositiveInfinity) 
            then Size.Empty  
            else 
                this.CalculateItemRects (fun c rect -> c.Measure(rect.Size)) _constraint
                _constraint           
            
    override this.ArrangeOverride(finalSize) = 
        this.CalculateItemRects (fun c rect -> c.Arrange(rect)) finalSize
        finalSize
    
[<ContentProperty("Children")>]
type EllipticalPanel() as this =
    inherit Panel()
    
    static let itemWidthProperty = 
        DependencyProperty.RegisterAttached(
            "ItemWidth", typeof<double>, typeof<EllipticalPanel>, 
                new FrameworkPropertyMetadata( 0., 
                    FrameworkPropertyMetadataOptions.AffectsMeasure |||
                    FrameworkPropertyMetadataOptions.AffectsArrange))
                    
    static let itemHeightProperty = 
        DependencyProperty.RegisterAttached(
            "ItemHeight", typeof<double>, typeof<EllipticalPanel>, 
                new FrameworkPropertyMetadata( 0., 
                    FrameworkPropertyMetadataOptions.AffectsMeasure |||
                    FrameworkPropertyMetadataOptions.AffectsArrange))        
    
    static let useFarrisWheelLayoutProperty = 
        DependencyProperty.RegisterAttached(
            "UseFerrisWheelLayout", typeof<bool>, typeof<EllipticalPanel>, 
                new FrameworkPropertyMetadata( false, 
                    FrameworkPropertyMetadataOptions.AffectsArrange))  

    member public this.ItemWidth 
        with get()  = this.GetValue(itemWidthProperty) :?> double
        and set(x:double) = this.SetValue(itemWidthProperty, x) |> ignore 
        
    member public this.ItemHeight 
        with get()  = this.GetValue(itemHeightProperty) :?> double
        and set(x:double) = this.SetValue(itemHeightProperty,x) |> ignore 
    
    member public this.UseFerrisWheelLayout 
        with get() = this.GetValue(useFarrisWheelLayoutProperty) :?> bool
        and set(x:bool) = this.SetValue(useFarrisWheelLayoutProperty, x) |> ignore 
        
  
    override this.MeasureOverride(_constraint) =
        if (_constraint.Width = Double.PositiveInfinity || _constraint.Height = Double.PositiveInfinity) 
            then Size.Empty  
            else 
                this.InternalChildren.to_seq |> Seq.iter(fun c -> c.Measure(Size(this.ItemWidth,this.ItemHeight)))
                _constraint           
            
    override this.ArrangeOverride(finalSize) = 
        let radiusX = (finalSize.Width - this.ItemWidth) * 0.5
        let radiusY = (finalSize.Height - this.ItemHeight) * 0.5
        
        let count = this.InternalChildren.Count
        
        let deltaAngle = 2. * Math.PI / (float count)
        
        let center = Point(finalSize.Width / 2., finalSize.Height / 2.)
        
        this.InternalChildren.to_seq |> Seq.iteri (fun i c -> 
            let angle = (float i) * deltaAngle
            let x = center.X + radiusX * Math.Cos(angle) - this.ItemWidth / 2.
            let y = center.Y + radiusY * Math.Sin(angle) - this.ItemWidth / 2.
            if (this.UseFerrisWheelLayout)
                then
                    c.RenderTransform <- null
                else
                    c.RenderTransformOrigin <- Point(0.5,0.5)
                    c.RenderTransform <- RotateTransform(angle * 180. / Math.PI)
            c.Arrange(Rect(x,y,this.ItemWidth,this.ItemHeight)))
        finalSize
