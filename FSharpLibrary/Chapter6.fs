namespace FSharp.Chapter6

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Documents
open System.Windows.Media
open System.Windows.Input
open System.Collections.ObjectModel

open System
open System.IO
open System.Xml
open System.Windows.Markup
open System.Windows.Shapes
open System.Windows.Controls
open System.Windows.Documents


type StringDataSource() = inherit ObservableCollection<string>()

type HoverAdorner(adornedElement:UIElement) =
    inherit Adorner(adornedElement)
    
    let mutable container = ContentPresenter()
    
    override this.MeasureOverride(_constraint) =
        container.Measure(_constraint)
        container.DesiredSize
    
    override this.ArrangeOverride(finalSize) =
        let left = this.AdornedElement.RenderSize.Width - container.DesiredSize.Width
        container.Arrange(Rect(Point(left, this.AdornedElement.RenderSize.Height / 2.), finalSize))
        finalSize
        
    override this.GetVisualChild(index) = container :> Visual
    
    override this.VisualChildrenCount = 1
    
    member this.Container with get() = container and set(x:ContentPresenter) = container <- x
    
    
type HoverInteractor() =
    inherit DependencyObject()
    
    static let useHoverProperty = 
                    DependencyProperty.RegisterAttached(
                     "UseHover", typeof<bool>, typeof<HoverInteractor>, 
                        PropertyMetadata(false, PropertyChangedCallback(
                                             fun d e ->
                                                match d with
                                                | :? ListBox as lb ->
                                                    if (e.NewValue :?> bool) 
                                                        then
                                                             lb.MouseMove.AddHandler(MouseEventHandler(HoverInteractor.ListBox_MouseMove))
                                                             lb.MouseLeave.AddHandler(MouseEventHandler(HoverInteractor.ListBox_MouseLeave))
                                                        else
                                                            lb.MouseMove.RemoveHandler(MouseEventHandler(HoverInteractor.ListBox_MouseMove))
                                                            lb.MouseLeave.RemoveHandler(MouseEventHandler(HoverInteractor.ListBox_MouseLeave))
                                                | _ -> ())))

    
    static let attachedAdornerProperty = DependencyProperty.Register("AttachedAdorner", typeof<AdornerInfo>, typeof<HoverInteractor>)    
    
    static member public SetUseHover(d:DependencyObject, _use:bool) = d.SetValue(useHoverProperty, _use)
    static member public GetUseHover(d:DependencyObject) = d.GetValue(useHoverProperty) :?> bool
    
    static member public SetAttachedAdorner(d:DependencyObject, adornerInfo:AdornerInfo) = d.SetValue(attachedAdornerProperty, adornerInfo)
    static member public GetAttachedAdorner(d:DependencyObject) = d.GetValue(attachedAdornerProperty) :?> AdornerInfo
    
    static member ListBox_MouseMove (sender:obj) (e:MouseEventArgs) =
        let lb = sender :?> ListBox
        match e.OriginalSource with
        | :? Visual as v ->
            match lb.ContainerFromElement(v) with
            | :? ListBoxItem as item ->
                let layer = AdornerLayer.GetAdornerLayer(lb)
                match lb.GetValue(attachedAdornerProperty) with
                | :? AdornerInfo as prevInfo when prevInfo.ListItem = item -> ()
                | :? AdornerInfo as prevInfo ->
                        layer.Remove(prevInfo.Adorner)
                        lb.ClearValue(attachedAdornerProperty)
                | _ -> 
                    let adorner = HoverAdorner(item)
                    adorner.Container.Content <- lb.ItemContainerGenerator.ItemFromContainer(item)
                    adorner.Container.ContentTemplate <- (item.FindResource("AdornerTemplate") :?> DataTemplate)
                    layer.Add(adorner)
                    
                    let info = AdornerInfo(Adorner = adorner, ListItem = item)
                    lb.SetValue(attachedAdornerProperty,info)
            | _ -> ()
        | _ -> ()
        
    static member ListBox_MouseLeave (sender:obj) (e:MouseEventArgs) =
        let lb = sender :?> ListBox
        match lb.GetValue(attachedAdornerProperty) with 
        | :? AdornerInfo as prevInfo ->
            let layer = AdornerLayer.GetAdornerLayer(lb) 
            layer.Remove(prevInfo.Adorner)
            lb.ClearValue(attachedAdornerProperty)
        | _ -> ()
        
and AdornerInfo() =
        [<DefaultValue(false)>]
        val mutable Adorner:HoverAdorner
        [<DefaultValue(false)>]
        val mutable ListItem:ListBoxItem


type IDropTargetAdvisor =
    abstract TargetUI:UIElement with get, set
    abstract ApplyMouseOffset:bool with get
    abstract IsValidDataObject:IDataObject -> bool
    abstract OnDropCompleted:IDataObject -> Point -> Unit
    abstract GetVisualFeedback:IDataObject -> UIElement
    abstract GetTopContainer:Unit -> UIElement        
    
type IDragSourceAdvisor =
    abstract SourceUI:UIElement with get, set
    abstract SupportedEffects:DragDropEffects with get
    abstract GetDataObject:UIElement -> DataObject
    abstract FinishDrag:UIElement -> DragDropEffects -> Unit
    abstract IsDraggable:UIElement -> bool
    abstract GetTopContainer:Unit -> UIElement
    

type DropPreviewAdorner(feedbackUI:UIElement, adornedElt:UIElement) as this =
    inherit Adorner(adornedElt)
    
    let mutable left = 0.
    let mutable top = 0.
    let mutable presenter = ContentPresenter(Content = feedbackUI, IsHitTestVisible = false)
    
    member private this.updatePosition() =
        match this.Parent with
        | :? AdornerLayer as layer -> layer.Update(this.AdornedElement)
        | _ -> () // source of error?

    member this.Left with get() = left and set(x:double) = left <- x; this.updatePosition()
    member this.Top with get() = top and set(x:double) = top <- x; this.updatePosition()
    
    override this.VisualChildrenCount = 1
    override this.GetVisualChild(index) = presenter :> Visual
    
    override this.MeasureOverride(_constraint) = presenter.Measure(_constraint); presenter.DesiredSize
    override this.ArrangeOverride(finalSize) = presenter.Arrange(Rect(finalSize)); finalSize
    override this.GetDesiredTransform(transform:GeneralTransform) =
        let result = GeneralTransformGroup()
        result.Children.Add(TranslateTransform(left,top))
        if (left > 0.) then this.Visibility <- Visibility.Visible
        result.Children.Add(base.GetDesiredTransform(transform))
        result :> GeneralTransform
        
    
type DragDropManager() =
    static let DragOffsetFormat = "DnD.DragOffset"
    
    static let dragSourceAdvisorPorperty =  
        DependencyProperty.RegisterAttached("DragSourceAdvisor", typeof<IDragSourceAdvisor>, 
                                            typeof<DragDropManager>,
                                            FrameworkPropertyMetadata(
                                                PropertyChangedCallback(DragDropManager.onDragSourceAdvisorChanged)))
    static let dropTargetAdvisorProperty = 
        DependencyProperty.RegisterAttached("DropTargetAdvisor", typeof<IDropTargetAdvisor>, 
                                            typeof<DragDropManager>,
                                            FrameworkPropertyMetadata(
                                                PropertyChangedCallback(DragDropManager.onDropTargetAdvisorChanged)))
    
    
    static let mutable draggedElt = None
    static let mutable adornerPosition = Point()
    static let mutable dragStartPoint = Point()
    static let mutable isMouseDown = false
    static let mutable offsetPoint = Point()
    static let mutable overlayElt = None
    static let mutable currentDragSourceAdvisor = None
    static let mutable currentDropTargetAdvisor = None
    
    (* Dependency Properties Getter/Setter *)
    static member public SetDragSourceAdvisor(d:DependencyObject, advisor:IDragSourceAdvisor) = d.SetValue(dragSourceAdvisorPorperty, advisor)
    static member public GetDragSourceAdvisor(d:DependencyObject) = d.GetValue(dragSourceAdvisorPorperty) :?> IDragSourceAdvisor
    
    static member public SetDropTargetAdvisor(d:DependencyObject, advisor:IDragSourceAdvisor) = d.SetValue(dropTargetAdvisorProperty, advisor)
    static member public GetDropTargetAdvisor(d:DependencyObject) = d.GetValue(dropTargetAdvisorProperty) :?> IDropTargetAdvisor
    


    (* Properties *)  
    
    static member private AdornerPosition
        with get() = adornerPosition
        and set(x:Point) = adornerPosition <- x

    static member private DragStartPoint
        with get() = dragStartPoint
        and set(x:Point) = dragStartPoint <- x

    static member private OffsetPoint
        with get() = offsetPoint
        and set(x:Point) = offsetPoint <- x
      
    static member private DraggedElt 
        with get() = match draggedElt with | Some(v) -> v | _ -> failwith "initialization fail"
        and set(x:UIElement) = draggedElt <- Some(x)
      
    static member private OverlayElt
        with get() = match overlayElt with | Some(v) -> v | _ -> failwith "initialization fail"
        and set(x:DropPreviewAdorner) = overlayElt <- Some(x)      
        
    static member private CurrentDragSourceAdvisor
        with get() = match currentDragSourceAdvisor with | Some(v) -> v | _ -> failwith "initialization fail"
        and set(x:IDragSourceAdvisor) = currentDragSourceAdvisor <- Some(x)
        
    static member private CurrentDropTargetAdvisor
        with get() = match currentDropTargetAdvisor with | Some(v) -> v | _ -> failwith "initialization fail"
        and set(x:IDropTargetAdvisor) = currentDropTargetAdvisor <- Some(x)          

   
    (* Property Change handlers *)
    static member private onDragSourceAdvisorChanged (depObj:DependencyObject) (args:DependencyPropertyChangedEventArgs) =  
        let sourceElt = depObj :?> UIElement
        if (args.NewValue <> null && args.OldValue = null)
            then 
                sourceElt.PreviewMouseLeftButtonDown.AddHandler(MouseButtonEventHandler(DragDropManager.DragSource_PreviewMouseLeftButtonDown))
                sourceElt.PreviewMouseMove.AddHandler(MouseEventHandler(DragDropManager.DragSource_PreviewMouseMove))
                sourceElt.PreviewMouseUp.AddHandler(MouseButtonEventHandler(DragDropManager.DragSource_PreviewMouseUp))
                
                // Set the Drag source UI
                (args.NewValue :?> IDragSourceAdvisor).SourceUI <- sourceElt
            else if (args.NewValue = null && args.OldValue <> null)
                    then 
                        sourceElt.PreviewMouseLeftButtonDown.RemoveHandler(MouseButtonEventHandler(DragDropManager.DragSource_PreviewMouseLeftButtonDown))
                        sourceElt.PreviewMouseMove.RemoveHandler(MouseEventHandler(DragDropManager.DragSource_PreviewMouseMove))
                        sourceElt.PreviewMouseUp.RemoveHandler(MouseButtonEventHandler(DragDropManager.DragSource_PreviewMouseUp))


    static member private onDropTargetAdvisorChanged (depObj:DependencyObject) (args:DependencyPropertyChangedEventArgs) =
        let targetElt = depObj :?> UIElement
        if (args.NewValue <> null && args.OldValue = null)
            then 
                targetElt.PreviewDragEnter.AddHandler(DragEventHandler(DragDropManager.DropTarget_PreviewDragEnter))
                targetElt.PreviewDragOver.AddHandler(DragEventHandler(DragDropManager.DropTarget_PreviewDragOver))
                targetElt.PreviewDragLeave.AddHandler(DragEventHandler(DragDropManager.DropTarget_PreviewDragLeave))
                targetElt.PreviewDrop.AddHandler(DragEventHandler(DragDropManager.DropTarget_PreviewDrop))
                targetElt.AllowDrop <- true
                
                // Set the Drag source UI
                (args.NewValue :?> IDropTargetAdvisor).TargetUI <- targetElt
            else if (args.NewValue = null && args.OldValue <> null)
                    then
                        targetElt.PreviewDragEnter.RemoveHandler(DragEventHandler(DragDropManager.DropTarget_PreviewDragEnter))
                        targetElt.PreviewDragOver.RemoveHandler(DragEventHandler(DragDropManager.DropTarget_PreviewDragOver))
                        targetElt.PreviewDragLeave.RemoveHandler(DragEventHandler(DragDropManager.DropTarget_PreviewDragLeave))
                        targetElt.PreviewDrop.RemoveHandler(DragEventHandler(DragDropManager.DropTarget_PreviewDrop))
                        targetElt.AllowDrop <- false

    
    
    (* Drop Target events *)
    static member DropTarget_PreviewDrop (sender:obj) (e:DragEventArgs) =
        DragDropManager.UpdateEffects(e)
        
        let position = e.GetPosition(sender :?> UIElement)
        
        // Calculate displacement for (Left, Top)
        let offset = e.GetPosition(DragDropManager.OverlayElt)
        
        let dropPoint = Point(position.X - offset.X, position.Y - offset.Y)
        
        DragDropManager.RemovePreviewAdorner()
        offsetPoint <- new Point(0.,0.)
        if (DragDropManager.CurrentDropTargetAdvisor.IsValidDataObject(e.Data)) then DragDropManager.CurrentDropTargetAdvisor.OnDropCompleted e.Data dropPoint
        e.Handled <- true
        
 
    
    static member private DropTarget_PreviewDragLeave (sender:obj) (e:DragEventArgs) =
        DragDropManager.UpdateEffects(e)
        DragDropManager.RemovePreviewAdorner()
        e.Handled <- true

    static member private DropTarget_PreviewDragOver (sender:obj) (e:DragEventArgs) =
        DragDropManager.UpdateEffects(e)
        
        // Update position of the preview Adorner
        adornerPosition <- e.GetPosition(sender :?> UIElement)
        DragDropManager.PositionAdorner()
        e.Handled <- true
    
    static member private DropTarget_PreviewDragEnter (sender:obj) (e:DragEventArgs) =
         // Get the current drop target advisor
         DragDropManager.CurrentDropTargetAdvisor <- DragDropManager.GetDropTargetAdvisor(sender :?> DependencyObject)
         DragDropManager.UpdateEffects(e)
         
         // Setup the preview Adorner
         offsetPoint <- new Point()
         if (DragDropManager.CurrentDropTargetAdvisor.ApplyMouseOffset && (e.Data.GetData(DragOffsetFormat) <> null)) // parenthesis 
            then 
               offsetPoint <- (e.Data.GetData(DragOffsetFormat) :?> Point)
                
         DragDropManager.CreatePreviewAdorner (sender :?> UIElement) e.Data
         e.Handled <- true
        
         
         
    
    static member private UpdateEffects (e:DragEventArgs) =
        if (DragDropManager.CurrentDropTargetAdvisor.IsValidDataObject(e.Data) = false) 
            then e.Effects <- DragDropEffects.None
            else if ((e.AllowedEffects &&& DragDropEffects.Move) = (enum 0) &&
                     (e.AllowedEffects &&& DragDropEffects.Copy) = (enum 0))
                    then e.Effects <- DragDropEffects.None
            else if ((e.AllowedEffects &&& DragDropEffects.Move) <> (enum 0) &&
                     (e.AllowedEffects &&& DragDropEffects.Copy) <> (enum 0))
                    then e.Effects <-  (if ((e.KeyStates &&& DragDropKeyStates.ControlKey) <> (enum 0))  then DragDropEffects.Copy else DragDropEffects.Move)


    (* Drag Source events *)
    
    static member private DragSource_PreviewMouseLeftButtonDown (sender:obj) (e:MouseEventArgs) =
        DragDropManager.CurrentDragSourceAdvisor <- DragDropManager.GetDragSourceAdvisor(sender :?> DependencyObject)
        
        if (DragDropManager.CurrentDragSourceAdvisor.IsDraggable(e.Source :?> UIElement) = true)
            then
                DragDropManager.DraggedElt <- (e.Source :?> UIElement)
                dragStartPoint <- e.GetPosition(DragDropManager.CurrentDragSourceAdvisor.GetTopContainer())
                offsetPoint <- e.GetPosition(DragDropManager.DraggedElt)
                isMouseDown <- true


    static member private DragSource_PreviewMouseMove (sender:obj) (e:MouseEventArgs) =
        if (isMouseDown && DragDropManager.IsDragGesture(e.GetPosition(DragDropManager.CurrentDragSourceAdvisor.GetTopContainer())))
            then
                DragDropManager.DragStarted(sender :?> UIElement)

    static member private DragSource_PreviewMouseUp (sender:obj) (e:MouseButtonEventArgs) =
        isMouseDown <- false
        Mouse.Capture(null) |> ignore

    static member private DragStarted(uiElt:UIElement) =
        isMouseDown <- false
        Mouse.Capture(uiElt) |> ignore
        let data = DragDropManager.CurrentDragSourceAdvisor.GetDataObject(DragDropManager.DraggedElt)
        data.SetData(DragOffsetFormat, offsetPoint)
        let supportedEffects = DragDropManager.CurrentDragSourceAdvisor.SupportedEffects
        
        // Perform DragDrop
        
        let effects = DragDrop.DoDragDrop(DragDropManager.DraggedElt, data, supportedEffects)
        DragDropManager.CurrentDragSourceAdvisor.FinishDrag DragDropManager.DraggedElt effects
        
        // Clean up
        DragDropManager.RemovePreviewAdorner()
        Mouse.Capture(null) |> ignore
        draggedElt <- None
        

    static member private IsDragGesture (point:Point) =
        let hGesture = Math.Abs(point.X - DragDropManager.DragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance
        let vGesture = Math.Abs(point.Y - DragDropManager.DragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance
        hGesture || vGesture
    
    (* Utility functions *)
    
    static member private CreatePreviewAdorner (adornedElt:UIElement) (data:IDataObject) =
        match overlayElt with
        | Some(x) -> ()
        | _ ->             
            let layer = AdornerLayer.GetAdornerLayer(DragDropManager.CurrentDropTargetAdvisor.GetTopContainer())
            let feedbackUI = DragDropManager.CurrentDropTargetAdvisor.GetVisualFeedback(data)
            overlayElt <- Some(DropPreviewAdorner(feedbackUI, adornedElt))
            DragDropManager.PositionAdorner()
            layer.Add(DragDropManager.OverlayElt)
    
    static member private PositionAdorner() =
        match overlayElt with
        | Some(x) -> 
            x.Left <- DragDropManager.AdornerPosition.X - DragDropManager.OffsetPoint.X
            x.Top <- DragDropManager.AdornerPosition.Y - DragDropManager.OffsetPoint.Y
            overlayElt <- Some(x) // do I need this?
        | _ -> ()
        
    static member private RemovePreviewAdorner() =
        match overlayElt with
        | Some(x) -> AdornerLayer.GetAdornerLayer(DragDropManager.CurrentDropTargetAdvisor.GetTopContainer()).Remove(x); overlayElt <- None
        | _ -> ()
            

type CanvasDragDropAdvisor() =
    
    let mutable sourceAndTargetElt = None
    
    let ExtractElement(ob:IDataObject) = XamlReader.Load(XmlReader.Create(new StringReader(string (ob.GetData("CanvasExamle"))))) :?> UIElement

    
        
    member this.SourceAndTargetElt 
        with get() = match sourceAndTargetElt with | Some(v) -> v | _ -> failwith "initialization fail"
        and set(x:UIElement) = sourceAndTargetElt <- Some(x)
        
    interface IDragSourceAdvisor with
    
        member this.SourceUI
            with get() = this.SourceAndTargetElt
            and set(x:UIElement) = this.SourceAndTargetElt <- x
        
        member this.SupportedEffects with get() = DragDropEffects.Move
        
        member this.GetDataObject(draggedElt) = DataObject("CanvasExamle", XamlWriter.Save(draggedElt))
        
        member this.FinishDrag draggedElt finalEffects=
            if ((finalEffects &&& DragDropEffects.Move) = DragDropEffects.Move)
                then (this.SourceAndTargetElt :?> Canvas).Children.Remove(draggedElt)
        
        member this.IsDraggable dragElt = match dragElt with | :? Canvas -> false | _ -> true
        
        member this.GetTopContainer() = this.SourceAndTargetElt
        
    interface IDropTargetAdvisor with
        
        member this.TargetUI 
            with get() = this.SourceAndTargetElt
            and set(x:UIElement) = this.SourceAndTargetElt <- x
        
        member this.ApplyMouseOffset = true
        // to do - figure this out!!
        //member this.IsValidDataObject(ob) =  ob.GetDataPresent("CanvasExample")
        member this.IsValidDataObject(ob) = true
        
        member this.GetVisualFeedback(ob) = 
            let elt = ExtractElement(ob)
            let t = elt.GetType()
            Rectangle(Width = ((t.GetProperty("Width").GetValue(elt, null)) :?> double), 
                      Height = ((t.GetProperty("Height").GetValue(elt, null)) :?> double),
                      Fill = VisualBrush(elt),
                      Opacity = 0.5,
                      IsHitTestVisible = false) :> UIElement
                                    
        member this.OnDropCompleted ob dropPoint =
            let canvas = this.SourceAndTargetElt :?> Canvas
            let elt = ExtractElement(ob)
            canvas.Children.Add(elt) |> ignore           
            Canvas.SetLeft(elt, dropPoint.X)
            Canvas.SetTop(elt, dropPoint.Y)
            
        member this.GetTopContainer() = this.SourceAndTargetElt
            
            