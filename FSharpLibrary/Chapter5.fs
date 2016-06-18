namespace FSharp.Chapter5

open System
open System.Globalization
open System.ComponentModel;
open System.Windows
open System.Windows.Media
open System.Windows.Controls
open System.Windows.Media.Animation

type MarkersElement() =
    inherit FrameworkElement() 
    
    let mutable pen = Pen()
    let mutable brush = SolidColorBrush() :> Brush
    
    let mutable deltaAngle = 0.
    
    member this.Foreground with get() = brush and set(x:Brush) = brush <- x; pen <- Pen(brush,1.)
    member this.DeltaAngle with get() = deltaAngle and set(x:double) = deltaAngle <- x 
    
    override this.OnRender(dc) =
        let steps = int (360. / deltaAngle)
        let center = Point(this.RenderSize.Width / 2., this.RenderSize.Height / 2.)
        
        dc.DrawEllipse(null, pen, center, this.RenderSize.Width  / 2., this.RenderSize.Height / 2.)
        
        let getPoint angle offset =
            let center = Point(this.RenderSize.Width / 2., this.RenderSize.Height / 2.)
            let radius = this.RenderSize.Width / 2.
            let radAngle = (angle * Math.PI / 180.) + offset
            let x = center.X + radius * Math.Cos(radAngle)
            let y = center.Y - radius * Math.Sin(radAngle)
            Point(x,y)
        
        {0 .. steps} 
        |> Seq.iter(fun i -> 
            let angle = (float i) * this.DeltaAngle
            let p1 = getPoint angle 0.
            
            // Lines and Circles
            dc.DrawLine(pen, center, p1)
            let radius = this.RenderSize.Width*0.5 * (float i / float steps)
            dc.DrawEllipse(null, pen, center, radius, radius)
            let text = 
                FormattedText("" + ((float i) * this.DeltaAngle).ToString(), CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, Typeface("Verdana"), 12., brush)
            let p2 = 
                let p2 = getPoint angle (Math.Max(text.Width / 2., text.Height / 2.))
                Point(p2.X - text.Width / 2., p2.Y - text.Height)
            dc.DrawText(text,p2))
            
            
type Enemy() as this =

    let mutable location = Point(0.,0.)
    let mutable velocity = Vector(0.,0.)
    let mutable _type = ""
    let mutable angle = 0.
    
    let propertyChanged = Event<_,_>()
    let notify obj s = propertyChanged.Trigger(obj, PropertyChangedEventArgs(s))
    
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member this.PropertyChanged = propertyChanged.Publish
    
    member this.Location with get() = location and set(x:Point) = location <- x; notify this "Location"
    member this.Angle with get() = angle and set(x:double) = angle <- x; notify this "Angle"
    member this.Type with get() = _type and set(x:string) = _type <- x; notify this "Type"
    member this.Velocity 
        with get() = velocity 
        and set(x:Vector) = 
            velocity <- x; 
            this.Angle <- Math.Atan2(velocity.Y, velocity.X) * 180./Math.PI
            notify this "Velocity"

type Guage() =
    inherit FrameworkElement()
    
    let mutable ticks = 0
    let mutable tickSize = Size(0.,0.)
    let mutable tickBrush = SolidColorBrush() :> Brush
    
    member this.Ticks with get() = ticks and set(x:int) = ticks <- x
    member this.TickSize with get() = tickSize and set(x:Size) = tickSize <- x
    member this.TickBrush with get() = tickBrush and set(x:Brush) = tickBrush <- x
    
    override this.OnRender(dc) = 
        let radius = Math.Min((this.RenderSize.Width - this.TickSize.Width) / 2.,
                                (this.RenderSize.Height - this.TickSize.Height) / 2.)
        let center = Point((this.RenderSize.Width - this.TickSize.Width) / 2.,
                                (this.RenderSize.Height - this.TickSize.Height) / 2.)
        for i in {0..this.Ticks} do
            let ratio = double i / double this.Ticks
            let x = center.X + radius * Math.Cos(Math.PI * ratio)
            let y = center.Y - radius * Math.Sin(Math.PI * ratio)
            
            let currRect = Rect(this.TickSize)
            currRect.Offset(x,y)
            let strokeCenter = Point(currRect.X + 0.5 * currRect.Width,
                                       currRect.Y + 0.5 * currRect.Height)
            let rotation = RotateTransform(90. - 180. * ratio, strokeCenter.X, strokeCenter.Y)
            dc.PushTransform(rotation)
            dc.DrawRectangle(this.TickBrush, null, currRect)
            dc.Pop()
            
type StageIndicator() =
    inherit Decorator()
    
    let mutable _tabControl = None   // TabControl
    let mutable _prevRect = None
    
    static let backgroundProperty = DependencyProperty.Register("Background", typeof<Brush>, typeof<StageIndicator>)
    static let borderBrushProperty = DependencyProperty.Register("BorderBrush", typeof<Brush>, typeof<StageIndicator>)
    static let itemRectProperty = DependencyProperty.Register("ItemRect", typeof<Rect>, typeof<StageIndicator>, FrameworkPropertyMetadata(Rect(), FrameworkPropertyMetadataOptions.AffectsRender))
    
    member this.Background with get() = this.GetValue(backgroundProperty) :?> Brush and set(x:Brush) = this.SetValue(backgroundProperty,x)
    member this.BorderBrush with get() = this.GetValue(borderBrushProperty) :?> Brush  and set(x:Brush) = this.SetValue(borderBrushProperty,x)
    
    member private this.tabControl
        with get() = match _tabControl with | Some(v) -> v | _ -> failwith "Initialization failure" 
        and set(x:TabControl) = _tabControl <- Some(x)
    
    member private this.prevRect 
        with get() = match _prevRect with | Some(v) -> v | _ -> failwith "Initialization failure" 
        and set(x:Rect) = _prevRect <- Some(x)
        
    
    
    override this.OnInitialized(e) =
        match this.TemplatedParent with
        | :? TabControl as v -> 
            this.tabControl <- v
            
            let rec eventHandler = RoutedEventHandler(fun o e -> 
                this.tabControl.Loaded.RemoveHandler(eventHandler)
                this.tabControl.SelectionChanged.Add(fun e ->
                    let sb = new Storyboard()
                    let newRect = this.getItemRect()
                    let anim = RectAnimation(this.prevRect, newRect, Duration(TimeSpan.FromMilliseconds(250.)))
                    Storyboard.SetTargetProperty(anim, PropertyPath(itemRectProperty))
                    sb.FillBehavior <- FillBehavior.Stop
                    sb.Children.Add(anim)
                    sb.Begin(this)
                    this.prevRect <- newRect
                    )
                this.prevRect <- this.getItemRect()
                this.SetValue(itemRectProperty, this.prevRect)
                )
            this.tabControl.Loaded.AddHandler(eventHandler)
        | _ -> ()
        
    
    override this.ArrangeOverride(arrangeSize) =
        this.prevRect <- this.getItemRect()
        this.SetValue(itemRectProperty, this.prevRect)
        arrangeSize
    
    override this.OnRender(dc) =
        if (_tabControl = None) then
            dc.DrawText(FormattedText("This panel can only be present inside the ControlTemplate for a TabControl", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Typeface("Verdana"), 14., Brushes.Red), Point())
        else
            let itemRect = this.GetValue(itemRectProperty) :?> Rect
            dc.DrawGeometry(this.Background, Pen(this.BorderBrush, 1.), this.createGeometry(itemRect))
    
    member private this.getItemRect() =
        let item = this.tabControl.ItemContainerGenerator.ContainerFromItem(this.tabControl.SelectedItem) :?> TabItem
        Rect(item.TranslatePoint(Point(), this.tabControl), Size(item.ActualWidth, item.ActualHeight))
    
    member private this.createGeometry(rect:Rect) =
        let geom = StreamGeometry()
        let containerRect = Rect(this.RenderSize)
        use context = geom.Open()
        context.BeginFigure(Point(rect.Left, 0.), true, true)
        context.LineTo(containerRect.BottomLeft, true, true)
        context.LineTo(containerRect.BottomRight, true, true)
        context.LineTo(Point(rect.Right, 0.), true, true)
        geom
        
        
    
        