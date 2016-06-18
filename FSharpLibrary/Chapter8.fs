namespace FSharp.Chapter8

open System
open System.Windows
open System.Windows.Media
open System.Windows.Controls
open System.Windows.Controls.Primitives
open System.Diagnostics

module tmp3 = 
    type UIElementCollection with
        member public this.to_seq = seq {for i in 0 .. this.Count - 1 -> this.[i]}
    
open tmp3

type StaggeredPanel() =
    inherit VirtualizingPanel()
    
    let mutable itemsOwner = None
    let mutable transform = TranslateTransform()
    let mutable startIndex = 0
    let mutable endIndex = 0
    
    // Scroller
    let mutable canVerticallyScroll = false
    let mutable canHorizontallyScroll = false
    let mutable extentWidth = 0.
    let mutable extentHeight = 0.
    let mutable viewportWidth = 0.
    let mutable viewportHeight = 0.
    let mutable horizontalOffset = 0.
    let mutable verticalOffset = 0.
    let mutable scrollOwner = None
    
    
    
    let itemWidthProperty = 
        DependencyProperty.Register("ItemWidth", typeof<double>, 
                                typeof<StaggeredPanel>,
                                FrameworkPropertyMetadata(200., 
                                    FrameworkPropertyMetadataOptions.AffectsMeasure ||| 
                                    FrameworkPropertyMetadataOptions.AffectsArrange))

    let itemHeightProperty = 
        DependencyProperty.Register("ItemHeight", typeof<double>, 
                                typeof<StaggeredPanel>,
                                FrameworkPropertyMetadata(100., 
                                    FrameworkPropertyMetadataOptions.AffectsMeasure ||| 
                                    FrameworkPropertyMetadataOptions.AffectsArrange))

    let staggerValueProperty = 
        DependencyProperty.Register("StaggeredPanel", typeof<double>, 
                                typeof<StaggeredPanel>, PropertyMetadata(0.2))
    
    do
        base.RenderTransform <- transform

    interface IScrollInfo with
        member this.CanVerticallyScroll with get() = canVerticallyScroll and set(x:bool) = canVerticallyScroll <- x
        member this.CanHorizontallyScroll with get() = canHorizontallyScroll and set(x:bool) = canHorizontallyScroll <- x
        member this.ExtentWidth with get() = extentWidth 
        member this.ExtentHeight with get() = extentHeight 
        member this.ViewportWidth with get() = viewportWidth 
        member this.ViewportHeight with get() = viewportHeight 
        member this.VerticalOffset with get() = verticalOffset 
        member this.HorizontalOffset with get() = horizontalOffset 

        member this.ScrollOwner 
            with get() = match scrollOwner with | Some(v) -> v | _ -> failwith "initialization failure"
            and set(x:ScrollViewer) = scrollOwner <- Some(x)
            
        member this.LineUp() = ()
        member this.LineDown() = ()
        member this.PageUp() = ()
        member this.PageDown() = ()
        member this.MouseWheelUp() = (this :> IScrollInfo).LineLeft()
        member this.MouseWheelDown() = (this :> IScrollInfo).LineRight()
        
        member this.SetVerticalOffset(_) = ()
        
        member this.LineLeft() = (this :> IScrollInfo).SetHorizontalOffset((this :> IScrollInfo).HorizontalOffset - 16.)
        member this.LineRight() = (this :> IScrollInfo).SetHorizontalOffset((this :> IScrollInfo).HorizontalOffset + 16.)
        
        member this.PageLeft() = (this :> IScrollInfo).SetHorizontalOffset((this :> IScrollInfo).HorizontalOffset - (this :> IScrollInfo).ViewportWidth)
        member this.PageRight() = (this :> IScrollInfo).SetHorizontalOffset((this :> IScrollInfo).HorizontalOffset + (this :> IScrollInfo).ViewportWidth)
        
        member this.MouseWheelLeft() = (this :> IScrollInfo).SetHorizontalOffset((this :> IScrollInfo).HorizontalOffset - (float SystemParameters.WheelScrollLines))
        member this.MouseWheelRight() = (this :> IScrollInfo).SetHorizontalOffset((this :> IScrollInfo).HorizontalOffset + (float SystemParameters.WheelScrollLines))
        
        member this.SetHorizontalOffset(offset:double) =
            horizontalOffset <- this.CalculateHorizontalOffset(offset)
            
            match scrollOwner with
            | Some(v) -> v.InvalidateScrollInfo()
            | None -> ()
            
            transform.X <- -1. * (this :> IScrollInfo).HorizontalOffset
            
            // Force us to realize the correct children
            this.InvalidateMeasure()
            
        member this.MakeVisible((visual:Visual),(rectangle:Rect)) = rectangle
            
    member this.CalculateHorizontalOffset(offset:double) =
        if (offset < 0. || (this :> IScrollInfo).ViewportWidth >= (this :> IScrollInfo).ExtentWidth) 
            then 0.
            else if (offset + (this :> IScrollInfo).ViewportWidth = (this :> IScrollInfo).ExtentWidth)
                    then
                        (this :> IScrollInfo).ExtentWidth - (this :> IScrollInfo).ViewportWidth
                    else
                        offset
    
    member this.ItemWidth 
        with get() = this.GetValue(itemWidthProperty) :?> double
        and set(x:double) = this.SetValue(itemWidthProperty, x)

    member this.ItemHeight
        with get() = this.GetValue(itemHeightProperty) :?> double
        and set(x:double) = this.SetValue(itemHeightProperty, x)

    member this.StaggerValue
        with get() = this.GetValue(staggerValueProperty) :?> double
        and set(x:double) = this.SetValue(staggerValueProperty, x)


        // properties
    member this.StartIndex with get() = startIndex and set(x:int) = startIndex <- x
    member this.EndIndex with get() = endIndex and set(x:int) = endIndex <- x
    

    member this.ItemsOwner
        with get() = match itemsOwner with | Some(v) -> v | _ -> failwith "initialization failure"
        and set(x:ItemsControl) = itemsOwner <- Some(x)   
        
    override this.OnInitialized(e:EventArgs) = this.ItemsOwner <- ItemsControl.GetItemsOwner(this)
    
    override this.MeasureOverride(availableSize) =
        if (availableSize.Width = Double.PositiveInfinity || availableSize.Height = Double.PositiveInfinity) 
            then Size.Empty  
            else 
                this.UpdateScrollInfo(availableSize)

                // Virtualize items
                this.VirtualizeItems()

                // Measure
                this.InternalChildren.to_seq |> Seq.iter(fun c -> c.Measure(Size(this.ItemWidth, this.ItemHeight)))

                // Cleanup
                this.CleanupItems()
                availableSize
    
    override this.ArrangeOverride(finalSize) =
        this.InternalChildren.to_seq |> Seq.iteri(fun i c -> 
            let index = this.StartIndex + i
            let left = (float index) * this.ItemWidth * this.StaggerValue
            let arrangeRect = Rect(left, 0., this.ItemWidth, this.ItemHeight)
            c.Arrange(arrangeRect))
        finalSize
        
    member this.UpdateScrollInfo(availableSize:Size) =
        
        // See how many items there are
        let itemCount = this.ItemsOwner.Items.Count
        let mutable viewportChanged = false
        let mutable extentChanged = false
        
        let extent = this.CalculateExtent availableSize itemCount
        
        // Update extent
        if (extent <> (this :> IScrollInfo).ExtentWidth)
            then 
                extentWidth <- extent
                extentChanged <- true
        
        // Update viewport
        if (availableSize.Width <> (this :> IScrollInfo).ViewportWidth)
            then
                viewportWidth <- availableSize.Width
                viewportChanged <- true
        
        if ((extentChanged || viewportChanged) && (match scrollOwner with | Some(v) -> true | _ -> false))
            then
                horizontalOffset <- this.CalculateHorizontalOffset((this :> IScrollInfo).HorizontalOffset)
                (this :> IScrollInfo).ScrollOwner.InvalidateScrollInfo()
                transform.X <- -1.*(this :> IScrollInfo).HorizontalOffset
                
    member this.CalculateExtent (size:Size) (count:int) =
        if (count = 0) 
            then 0.
            else
                let visibleArea = this.ItemWidth * this.StaggerValue
                (float (count - 1)) * visibleArea + this.ItemWidth
    
    member this.VirtualizeItems() =
        this.UpdateIndexRange()

    
    
        let generator = this.ItemsOwner.ItemContainerGenerator :> IItemContainerGenerator
        
        let startPos = generator.GeneratorPositionFromIndex(this.StartIndex)
        
        let mutable childIndex = if (startPos.Offset = 0) then startPos.Index else startPos.Index + 1
        
        use g = generator.StartAt(startPos, GeneratorDirection.Forward, true) 
        
       
        for i in [this.StartIndex .. this.EndIndex] do
            let mutable isNewlyRealized = false
            let child = generator.GenerateNext(&isNewlyRealized) :?> UIElement 
            if (isNewlyRealized)
                then
                    if (childIndex >= this.InternalChildren.Count)
                        then
                            this.AddInternalChild(child)
                        else
                            this.InsertInternalChild(childIndex, child)
                    generator.PrepareItemContainer(child)
            childIndex <- childIndex + 1
        
       
    member this.CleanupItems() =
        let generator = this.ItemContainerGenerator
        let count = this.InternalChildren.Count - 1
        for i in [count .. 0] do
            let position = GeneratorPosition(i,0)
            let itemIndex = generator.IndexFromGeneratorPosition(position)
            if (itemIndex < this.StartIndex || itemIndex > this.EndIndex)
                then
                    generator.Remove(position, 1)
                    this.RemoveInternalChildRange(i,1)
                    

    member this.CalculateIndexFromOffset(offset:double) =
        if (offset >= (this :> IScrollInfo).ExtentWidth - this.ItemWidth && offset <= (this :> IScrollInfo).ExtentWidth)
            then
                this.ItemsOwner.Items.Count - 1
            else
                let visibleArea = this.ItemWidth * this.StaggerValue
                Math.Floor(offset/visibleArea) |> int
    
    member this.UpdateIndexRange() =
        let left = (this :> IScrollInfo).HorizontalOffset
        let right = Math.Min((this :> IScrollInfo).ExtentWidth, (this :> IScrollInfo).HorizontalOffset + (this :> IScrollInfo).ViewportWidth)
        this.StartIndex <- this.CalculateIndexFromOffset(left)
        this.EndIndex <- this.CalculateIndexFromOffset(right)
        Debug.WriteLine("Index Range : [ " + this.StartIndex.ToString() + ", " + this.EndIndex.ToString() + " ]")
        
 