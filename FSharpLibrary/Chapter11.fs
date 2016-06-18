namespace FSharp.Chapter11

open System
open System.Windows
open System.Windows.Media
open System.Windows.Media.Media3D
open System.Windows.Controls

open System.ComponentModel
open System.Windows.Controls.Primitives
open System.Windows.Input

open System.Windows.Media.Animation

module tmp3 = 
    type UIElementCollection with
        member public this.to_seq = seq {for i in 0 .. this.Count - 1 -> this.[i]}
    
open tmp3   

type ResourceManager() =
    static let mutable internalResources = Application.LoadComponent(Uri("/Chapters;component/Chapter11/InternalResources.xaml", UriKind.Relative)) :?> ResourceDictionary
    
    static member InternalResources with get() = internalResources and set(x:ResourceDictionary) = internalResources <- x
    
    

type View3DElement() =
    inherit FrameworkElement()
    
    let viewport = ResourceManager.InternalResources.["3DViewport"] :?> Viewport3D
    let modelGroup = (((viewport.Children.[0] :?> ModelVisual3D).Content :?> Model3DGroup).Children.[1] :?> Model3DGroup)
    
    do
        modelGroup.Children.Add(ResourceManager.InternalResources.["PlaneModel"] :?> Model3D)
        
    (* Layout Overrides *)
    
    override this.MeasureOverride(availableSize) =
        if (availableSize.Width = Double.PositiveInfinity || availableSize.Height = Double.PositiveInfinity)
            then
                Size.Empty
            else
                viewport.Measure(availableSize)
                viewport.DesiredSize
                
    override this.ArrangeOverride(finalSize) = viewport.Arrange(Rect(finalSize)); finalSize
    
    override this.GetVisualChild(index) = if (index = 0) then viewport :> Visual else failwith "Bad Index"
    
    override this.VisualChildrenCount = 1

type GardenViewPanel3D() =
    inherit Panel()
    
    let viewport = ResourceManager.InternalResources.["3DViewport_Interactive"] :?> Viewport3D
    let modelContainer = viewport.Children.[0] :?> ContainerUIElement3D
    let random = Random()
    
    static let elementWidthProperty = DependencyProperty.Register("ElementWidth", typeof<double>, typeof<GardenViewPanel3D>)
    
    static let elementHeightProperty = DependencyProperty.Register("ElementHeight", typeof<double>, typeof<GardenViewPanel3D>)
    
    static let linkedModelProperty = DependencyProperty.Register("LinkedModel", typeof<ModelUIElement3D>, typeof<GardenViewPanel3D>)
    
    member this.ElementWidth 
        with get() = this.GetValue(elementWidthProperty) :?> double
        and set(x:double) = this.SetValue(elementWidthProperty,x)

    member this.ElementHeight
        with get() = this.GetValue(elementHeightProperty) :?> double
        and set(x:double) = this.SetValue(elementHeightProperty,x)
        
    
    (* Layout Overrides *)
    
    override this.MeasureOverride(availableSize) =
        if (availableSize.Width = Double.PositiveInfinity || availableSize.Height = Double.PositiveInfinity)
            then
                Size.Empty
            else
                viewport.Measure(availableSize)
                this.Children.to_seq |> Seq.iter(fun c -> c.Measure(Size(this.ElementWidth, this.ElementHeight)))
                availableSize
                
    override this.ArrangeOverride(finalSize) = 
        viewport.Arrange(Rect(finalSize))
        this.Children.to_seq |> Seq.iter(fun c -> c.Arrange(Rect(Size(this.ElementWidth, this.ElementHeight))))
        finalSize
    
    override this.GetVisualChild(index) = if (index = 0) then viewport :> Visual else failwith "Bad Index"
    
    override this.VisualChildrenCount
        with get() = if (base.VisualChildrenCount = 0) then 0 else 1
    
    override this.OnRender(dc) =
        dc.DrawRectangle(Brushes.Transparent, null, Rect(this.RenderSize))
    


    member this.createModel(visualAdded:DependencyObject) =
        let index = Math.Max(0, this.InternalChildren.Count - 1)
        let model = ModelUIElement3D()
        model.Transform <- TranslateTransform3D()
        
        // Prepare the GeometryModel
        let geomModel = (ResourceManager.InternalResources.["PlaneModel"] :?> GeometryModel3D).Clone()
        (geomModel.Material :?> DiffuseMaterial).Brush <- (VisualBrush(visualAdded :?> Visual))
        
        let zPos = -1. * (float index)
        let xPos = -(float index) / 2. + random.NextDouble() * (float index)
        
        let trans = (geomModel.Transform :?> TranslateTransform3D)
        trans.OffsetX <- xPos
        trans.OffsetZ <- zPos
        
        model.Model <- geomModel
        model
    
    override this.OnVisualChildrenChanged(visualAdded, visualRemoved) =
        base.OnVisualChildrenChanged(visualAdded, visualRemoved)
        
        if (visualAdded <> null && visualAdded <> (viewport :> DependencyObject))
            then
                let model = this.createModel(visualAdded)
                visualAdded.SetValue(linkedModelProperty, model)
                modelContainer.Children.Add(model)
            
        if (visualRemoved <> null && visualRemoved <> (viewport :> DependencyObject))
            then
                let model = visualRemoved.GetValue(linkedModelProperty) :?> ModelUIElement3D
                model.Model <- null
                visualRemoved.ClearValue(linkedModelProperty)
                modelContainer.Children.Remove(model) |> ignore
                
    
type InteractiveGardenViewPanel3D() as this =
    inherit Panel()
    
    let viewport = ResourceManager.InternalResources.["3DViewport_Interactive"] :?> Viewport3D
    let modelContainer = viewport.Children.[0] :?> ContainerUIElement3D
    let random = Random()
    
    let mutable hitModel = None
    let mutable prevHitModel = None
    
    do 
        modelContainer.MouseLeftButtonDown.AddHandler(MouseButtonEventHandler(this.modelContainer_MouseLeftButtonDown))
    
    static let elementWidthProperty = DependencyProperty.Register("ElementWidth", typeof<double>, typeof<GardenViewPanel3D>)
    
    static let elementHeightProperty = DependencyProperty.Register("ElementHeight", typeof<double>, typeof<GardenViewPanel3D>)
    
    static let linkedModelProperty = DependencyProperty.Register("LinkedModel", typeof<ModelUIElement3D>, typeof<GardenViewPanel3D>)
    
    member this.ElementWidth 
        with get() = this.GetValue(elementWidthProperty) :?> double
        and set(x:double) = this.SetValue(elementWidthProperty,x)

    member this.ElementHeight
        with get() = this.GetValue(elementHeightProperty) :?> double
        and set(x:double) = this.SetValue(elementHeightProperty,x)
        
    member private this.hitModel
        with get() = match hitModel with | Some(v) -> v | _ -> failwith "initialization failure" 
        and set(x:ModelUIElement3D) = hitModel <- Some(x)
        
    member private this.prevHitModel
        with get() = match prevHitModel with | Some(v) -> v | _ -> failwith "initialization failure" 
        and set(x:ModelUIElement3D) = prevHitModel <- Some(x)
        
    override this.OnInitialized(e) = this.AddVisualChild(viewport)
    
    member private this.modelContainer_MouseLeftButtonDown (sender:obj) (e:MouseButtonEventArgs) =
        match prevHitModel with
        | None -> ()
        | Some(v) ->
            let prevIndex = modelContainer.Children.IndexOf(v)
            let anim = this.constructStoryboard(prevIndex) :> Storyboard
            (anim.Children.[0] :?> DoubleAnimation).To <- Nullable<float> 0.
            anim.FillBehavior <- FillBehavior.Stop
            anim.Begin(viewport)
        let hitModel = e.Source :?> ModelUIElement3D
        let rect = hitModel.TransformToAncestor(this).TransformBounds(hitModel.Model.Bounds)
        let index = modelContainer.Children.IndexOf(hitModel)
        this.constructStoryboard(index).Begin(viewport)
        prevHitModel <- Some(hitModel)


    
    member private this.constructPropertyPath(index:int) =
        // The reason why we need to use a PropertyDescriptor is because ContainerUIElement3D.Children
        // property is not a DependencyProperty
        let childDesc = TypeDescriptor.GetProperties(modelContainer).Find("Children",true)
        PropertyPath("(0)[0].(1)[" + index.ToString() + "].(2).(3).(4)",
                        Viewport3D.ChildrenProperty,
                        childDesc,
                        ModelUIElement3D.ModelProperty,
                        GeometryModel3D.TransformProperty,
                        TranslateTransform3D.OffsetYProperty)
    
    member private this.constructStoryboard(index:int) =
        let anim = ResourceManager.InternalResources.["PlaneJump_Animation"] :?> Storyboard
        let path = this.constructPropertyPath(index)
        Storyboard.SetTargetProperty((anim.Children.[0] :?> DoubleAnimation), path)
        anim
    
    member private this.viewport_HitTestResult(result:HitTestResult) =
        match result.VisualHit with
        | :? ModelUIElement3D as v -> 
            this.hitModel <- v
            HitTestResultBehavior.Stop
        | _ -> HitTestResultBehavior.Continue

    member private this.locateModelContainer() = viewport.Children.[0] :?> ContainerUIElement3D
    
    (* Layout Overrides *)
    override this.MeasureOverride(availableSize) =
        if (availableSize.Width = Double.PositiveInfinity || availableSize.Height = Double.PositiveInfinity)
            then
                Size.Empty
            else
                viewport.Measure(availableSize)
                this.Children.to_seq |> Seq.iter(fun c -> c.Measure(Size(this.ElementWidth, this.ElementHeight)))
                availableSize
                
    override this.ArrangeOverride(finalSize) = 
        viewport.Arrange(Rect(finalSize))
        this.Children.to_seq |> Seq.iter(fun c -> c.Arrange(Rect(Size(this.ElementWidth, this.ElementHeight))))
        finalSize
    
    override this.GetVisualChild(index) = if (index = 0) then viewport :> Visual else failwith "Bad Index"
    
    override this.VisualChildrenCount
        with get() = if (base.VisualChildrenCount = 0) then 0 else 1
    
    override this.OnRender(dc) =
        dc.DrawRectangle(Brushes.Transparent, null, Rect(this.RenderSize))
    


    member this.createModel(visualAdded:DependencyObject) =
        let index = Math.Max(0, this.InternalChildren.Count - 1)
        let model = ModelUIElement3D()
        model.Transform <- TranslateTransform3D()
        
        // Prepare the GeometryModel
        let geomModel = (ResourceManager.InternalResources.["PlaneModel"] :?> GeometryModel3D).Clone()
        (geomModel.Material :?> DiffuseMaterial).Brush <- (VisualBrush(visualAdded :?> Visual))
        
        let zPos = -1. * (float index)
        let xPos = -(float index) / 2. + random.NextDouble() * (float index)
        
        let trans = (geomModel.Transform :?> TranslateTransform3D)
        trans.OffsetX <- xPos
        trans.OffsetZ <- zPos
        
        model.Model <- geomModel
        model
    
    override this.OnVisualChildrenChanged(visualAdded, visualRemoved) =
        base.OnVisualChildrenChanged(visualAdded, visualRemoved)
        
        if (visualAdded <> null && visualAdded <> (viewport :> DependencyObject))
            then
                let model = this.createModel(visualAdded)
                visualAdded.SetValue(linkedModelProperty, model)
                modelContainer.Children.Add(model)
            
        if (visualRemoved <> null && visualRemoved <> (viewport :> DependencyObject))
            then
                let model = visualRemoved.GetValue(linkedModelProperty) :?> ModelUIElement3D
                model.Model <- null
                visualRemoved.ClearValue(linkedModelProperty)
                modelContainer.Children.Remove(model) |> ignore