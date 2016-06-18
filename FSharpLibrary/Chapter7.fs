namespace FSharp.Chapter7

open System
open System.Linq
open System.Text
open System.Windows
open System.Windows.Media
open System.Windows.Media.Animation
open System.Windows.Controls
open System.Windows.Controls.Primitives
open System.Collections.Generic
open System.Collections.ObjectModel

module tmp3 = 
    type UIElementCollection with
        member public this.to_seq = seq {for i in 0 .. this.Count - 1 -> this.[i]}
    
open tmp3

type TestCollection() = inherit ObservableCollection<string>()

type HScrollingPanel() =
    inherit Panel()
    

    
    static let childSizeProperty =  
        DependencyProperty.Register("ChildSize", typeof<Size>, 
                                    typeof<HScrollingPanel>,
                                    PropertyMetadata(Size(200.,100.)))                           
                                    
    static let viewportSizeProperty = 
        DependencyProperty.Register("ViewportSize", typeof<double>, 
                                    typeof<HScrollingPanel>)
                                    
    static let extendSizeProperty = 
        DependencyProperty.Register("ExtentSize", typeof<double>, 
                                    typeof<HScrollingPanel>)
                                    
    static let horizontalOffsetProperty = 
        DependencyProperty.Register("HorizontalOffset", typeof<double>, 
                                    typeof<HScrollingPanel>,
                                    PropertyMetadata(0.0, 
                                        PropertyChangedCallback(HScrollingPanel.onHorizontalOffsetChanged)))
    let  _trans = new TranslateTransform()            
                        
    do
        base.RenderTransform <- _trans
    
    member private this.trans with get() = _trans 
    
    static member private onHorizontalOffsetChanged d e = (d :?> HScrollingPanel).trans.X <- -1. * (e.NewValue :?> double)
    
    member this.ChildSize 
        with get() = this.GetValue(childSizeProperty) :?> Size
        and set(x:Size) = this.SetValue(childSizeProperty,x) 
        
    member this.HorizontalOffset 
        with get() = this.GetValue(horizontalOffsetProperty) :?> double
        and set(x:double) = this.SetValue(horizontalOffsetProperty,x) 
        
    member this.ViewportSize 
        with get() = this.GetValue(viewportSizeProperty) :?> double
        and set(x:double) = this.SetValue(viewportSizeProperty,x)         

    member this.ExtentSize 
        with get() = this.GetValue(extendSizeProperty) :?> double
        and set(x:double) = this.SetValue(extendSizeProperty,x)  
     

    override this.MeasureOverride(_constraint) =
        if (_constraint.Width = Double.PositiveInfinity || _constraint.Height = Double.PositiveInfinity) 
            then Size.Empty  
            else 
                this.UpdateScrollInfo(_constraint)
                base.InternalChildren.to_seq |> Seq.iter(fun c -> c.Measure(this.ChildSize))
                _constraint
    
    override this.ArrangeOverride(finalSize) =
        this.UpdateScrollInfo(finalSize)
        base.InternalChildren.to_seq |> Seq.iteri(fun i c -> c.Arrange(Rect(Point((float i) * this.ChildSize.Width, 0.), this.ChildSize)))
        finalSize
    
                
    member private this.UpdateScrollInfo(size:Size) =
        if (size.Width <> this.ViewportSize) then this.ViewportSize <- size.Width
        let extent = (float base.InternalChildren.Count) * this.ChildSize.Width - this.ViewportSize
        if (extent <> this.ExtentSize) then this.ExtentSize <- extent
        
        
        
    
type RowsPanel() =
    inherit Panel()
    
    static let rowHeightProperty =  
        DependencyProperty.Register("RowHeight", typeof<double>, 
                                    typeof<RowsPanel>,
                                    PropertyMetadata(30.))
                       
    let mutable animateScroll = false;
    
    let _trans = new TranslateTransform()
    let mutable scrollOwner = None
    let mutable canHScroll = false
    let mutable canVScroll = false
    let mutable extent = Size(0.,0.)
    let mutable viewport = Size(0.,0.)
    let mutable offset = Point(0.,0.)
    
    do
        base.RenderTransform <- _trans
    
    member this.RowHeight
        with get() = this.GetValue(rowHeightProperty) :?> double
        and set(x:double) = this.SetValue(rowHeightProperty, x)
    
    override this.MeasureOverride(_constraint) =
       if (_constraint.Width = Double.PositiveInfinity || _constraint.Height = Double.PositiveInfinity) 
            then Size.Empty  
            else 
                this.UpdateScrollInfo(_constraint)
                base.InternalChildren.to_seq |> Seq.iter(fun c -> c.Measure(Size(_constraint.Width, this.RowHeight)))
                _constraint
    
    override this.ArrangeOverride(finalSize) =
        this.UpdateScrollInfo(finalSize)
        base.InternalChildren.to_seq |> Seq.iteri(fun i c -> c.Arrange(Rect(0.,(float i) * this.RowHeight, finalSize.Width, this.RowHeight)))
        finalSize
        
    (* Layout specific code *)
    member this.CalculateExtent (availableSize:Size) (itemCount:int) = Size(availableSize.Width, this.RowHeight * (float itemCount))
    
    member this.ArrangeChild (itemIndex:int) (child:UIElement) (finalSize:Size) = 
        child.Arrange(Rect(0., (float itemIndex) * this.RowHeight, finalSize.Width, this.RowHeight))
    
    member this.AnimateScroll with get() = animateScroll and set(x:bool) = animateScroll <- x
    
    interface IScrollInfo with

        member this.ScrollOwner 
            with get() = match scrollOwner with | Some(v) -> v | _ -> failwith "initialization fail"
            and set(x:ScrollViewer) = scrollOwner <- Some(x)
        
        member this.CanHorizontallyScroll with get() = canHScroll and set(x:bool) = canHScroll <- x
        
        member this.CanVerticallyScroll with get() = canVScroll and set(x:bool) = canVScroll <- x
        
        member this.ExtentHeight with get() = extent.Height
        
        member this.ExtentWidth with get() = extent.Width
        

        member this.SetHorizontalOffset(_offset) =
            let _offset = this.CalculateHorizontalOffset(_offset)
            offset.X <- _offset
            
            match scrollOwner with
            | Some(v:ScrollViewer) -> v.InvalidateScrollInfo()
            | _ -> ()
            
            this.Scroll _offset (this :> IScrollInfo).VerticalOffset
            this.InvalidateMeasure()
        
        member this.SetVerticalOffset(_offset) =
            let _offset = this.CalculateVerticalOffset(_offset)
            offset.Y <- _offset
            
            match scrollOwner with
            | Some(v) -> v.InvalidateScrollInfo()
            | _ -> ()
            
            this.Scroll (this :> IScrollInfo).HorizontalOffset _offset
        
        member this.LineUp() = (this :> IScrollInfo).SetVerticalOffset((this :> IScrollInfo).VerticalOffset - 10.) 
        
        member this.LineDown() = (this :> IScrollInfo).SetVerticalOffset((this :> IScrollInfo).VerticalOffset + 10.)
        
        member this.PageUp() = (this :> IScrollInfo).SetVerticalOffset((this :> IScrollInfo).VerticalOffset - (this :> IScrollInfo).ViewportHeight)
        
        member this.PageDown() = (this :> IScrollInfo).SetVerticalOffset((this :> IScrollInfo).VerticalOffset + (this :> IScrollInfo).ViewportHeight)
        
        member this.MouseWheelUp() = (this :> IScrollInfo).SetVerticalOffset((this :> IScrollInfo).VerticalOffset - 10.)
        
        member this.MouseWheelDown() = (this :> IScrollInfo).SetVerticalOffset((this :> IScrollInfo).VerticalOffset + 10.)
        
        member this.LineLeft() = (this :> IScrollInfo).SetHorizontalOffset((this :> IScrollInfo).HorizontalOffset - 10.)
        
        member this.LineRight() = (this :> IScrollInfo).SetHorizontalOffset((this :> IScrollInfo).HorizontalOffset + 10.)
        
        member this.MakeVisible(visual:Visual, rectangle:Rect) = rectangle
        
        member this.MouseWheelLeft() = (this :> IScrollInfo).SetHorizontalOffset((this :> IScrollInfo).HorizontalOffset - 10.)
        
        member this.MouseWheelRight() = (this :> IScrollInfo).SetHorizontalOffset((this :> IScrollInfo).HorizontalOffset + 10.)
        
        member this.PageLeft() = (this :> IScrollInfo).SetHorizontalOffset((this :> IScrollInfo).HorizontalOffset - (this :> IScrollInfo).ViewportWidth)
        
        member this.PageRight() = (this :> IScrollInfo).SetHorizontalOffset((this :> IScrollInfo).HorizontalOffset + (this :> IScrollInfo).ViewportWidth)
    


        member this.ViewportHeight with get() = viewport.Height

        member this.ViewportWidth with get() = viewport.Width

        member this.HorizontalOffset with get() = offset.X

        member this.VerticalOffset with get() = offset.Y
    

        
    member this.CalculateVerticalOffset(offset) =
        if (offset < 0. || viewport.Height >= extent.Height) 
            then 0.
            else if (offset + viewport.Height > extent.Height)
                 then extent.Height - viewport.Height
                 else offset
    
    member this.CalculateHorizontalOffset(offset) =
        if (offset < 0. || viewport.Width >= extent.Width) 
        then 0.
        else if (offset + viewport.Width > extent.Width)
             then extent.Width - viewport.Width
             else offset
             
    member this.Scroll (xOffset:double) (yOffset:double) =
        if (this.AnimateScroll)
            then
                let anim = DoubleAnimation(-yOffset, Duration(TimeSpan.FromMilliseconds(500.)))
                let p = PropertyPath("(0).(1)", RowsPanel.RenderTransformProperty, TranslateTransform.YProperty)
                Storyboard.SetTargetProperty(anim,p)
                let sb = Storyboard()
                sb.Children.Add(anim)
                let rec eventHandler = EventHandler(fun o e -> 
                    sb.Completed.RemoveHandler(eventHandler)
                    sb.Remove(this)
                    _trans.X <- -xOffset
                    _trans.Y <- -yOffset
                    )
                sb.Completed.AddHandler(eventHandler)
                sb.Begin(this, true)
                
            else
                _trans.X <- -xOffset
                _trans.Y <- -yOffset
    
    
    member this.UpdateScrollInfo(availableSize) =
        let itemCount = base.InternalChildren.Count
        let mutable viewportChanged = false
        let mutable extentChanged = false
        
        let _extent = this.CalculateExtent availableSize itemCount
        
        if (_extent <> extent) 
            then extent <- _extent
                 extentChanged <- true
        
        if (availableSize <> viewport)
            then viewport <- availableSize
                 viewportChanged <- true
        
        if ((extentChanged || viewportChanged) && scrollOwner <> None)
            then
                offset.Y <- this.CalculateVerticalOffset((this :> IScrollInfo).VerticalOffset)
                offset.X <- this.CalculateHorizontalOffset((this :> IScrollInfo).HorizontalOffset)
                match scrollOwner with
                | Some(v:ScrollViewer) -> v.InvalidateScrollInfo()
                | _ -> ()
                this.Scroll (this :> IScrollInfo).HorizontalOffset (this :> IScrollInfo).VerticalOffset
                          
                
        