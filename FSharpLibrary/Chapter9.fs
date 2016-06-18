namespace FSharp.Chapter9

open System
open System.Diagnostics
open System.Windows
open System.Windows.Controls
open System.Windows.Input
open System.Windows.Media
open System.Windows.Media.Animation
open System.Windows.Media.Media3D
open System.Windows.Documents
open System.Windows.Shapes
open System.Windows.Markup

type AnimationParameters = 
    { DockDirection:string
      AnimationType:string }

module tmp3 = 
    type UIElementCollection with
        member public this.to_seq = seq {for i in 0 .. this.Count - 1 -> this.[i]}
    
open tmp3         

type DockSlidePresenter() =
    inherit FrameworkElement()
    
    let mutable _root = None
    let mutable _viewport = None
    let mutable _meshModel = None
    let mutable _frontChildContainer = None
    let mutable _backChildContainer = None
    
    let mutable frontChild = None
    let mutable backChild = None
    
    static let mutable _dictionary = None
    
    static let isChildDockedProperty =
        DependencyProperty.Register("IsChildDocked", typeof<bool>, typeof<DockSlidePresenter>,
            new PropertyMetadata(false, PropertyChangedCallback(DockSlidePresenter.onIsChildDockedChanged)))
    
    static do
        DockSlidePresenter.ClipToBoundsProperty.OverrideMetadata(typeof<DockSlidePresenter>, PropertyMetadata(true))
        _dictionary <- Some(Application.LoadComponent(Uri("/Chapters;component/Chapter09/InternalResources.xaml", UriKind.Relative)) :?> ResourceDictionary) 
    
    
    member this.root
        with get() = match _root with | Some(v) -> v | _ -> failwith "initialization failure" 
        and set(x:Grid) = _root <- Some(x)
        
    member this.meshModel
        with get() = match _meshModel with | Some(v) -> v | _ -> failwith "initialization failure" 
        and set(x:ModelUIElement3D) = _meshModel <- Some(x)

    member this.frontChildContainer
        with get() = match _frontChildContainer with | Some(v) -> v | _ -> failwith "initialization failure" 
        and set(x:Decorator) = _frontChildContainer <- Some(x)

    member this.backChildContainer
        with get() = match _backChildContainer with | Some(v) -> v | _ -> failwith "initialization failure" 
        and set(x:Decorator) = _backChildContainer <- Some(x)
        
    member this.viewport
        with get() = match _viewport with | Some(v) -> v | _ -> failwith "initialization failure" 
        and set(x:Viewport3D) = _viewport <- Some(x)

    static member private Dictionary
        with get() = match _dictionary with | Some(v) -> v | _ -> failwith "initialization failure" 
        and set(x:ResourceDictionary) = _dictionary <- Some(x)

   
    member this.FrontChild 
        with get() = match frontChild with | Some(v) -> v | _ -> failwith "initialization failure" 
        and set(x:FrameworkElement) = frontChild <- Some(x)
    
    member this.BackChild
        with get() = match backChild with | Some(v) -> v | _ -> failwith "initialization failure"
        and set(x:FrameworkElement) = backChild <- Some(x)
        
    static member onIsChildDockedChanged (d:DependencyObject) (e:DependencyPropertyChangedEventArgs) =
        let presenter = d :?> DockSlidePresenter
        let docked = e.NewValue :?> bool
        if (docked) then presenter.dockFrontChild() else presenter.undockFrontChild()
        
    override this.OnInitialized(e:EventArgs) = 
        this.buildVisualTree()
        this.AddVisualChild(this.root)
    
    member private this.buildVisualTree() =
        this.root <- (Application.LoadComponent(Uri("/Chapters;component/Chapter09/DockSlideVisualTree.xaml", UriKind.Relative)) :?> Grid)
        
        // Viewport
        this.viewport <- (this.root.FindName("Viewport") :?> Viewport3D)
        this.meshModel <- ((this.viewport.Children.[0] :?> ContainerUIElement3D).Children.[0] :?> ModelUIElement3D)
        
        // Texture for the model
        let brush = VisualBrush(this.FrontChild)
        ((this.meshModel.Model :?> GeometryModel3D).Material :?> DiffuseMaterial).Brush <- brush
        
        // Front child
        this.frontChildContainer <- (this.root.FindName("FrontChildContainer") :?> Decorator)
        this.frontChildContainer.Child <- this.FrontChild
        
        // Back Child
        this.backChildContainer <- (this.root.FindName("BackChildContainer") :?> Decorator)
        this.backChildContainer.Child <- this.BackChild
    
    member private this.adjustViewport3D() = this.PositionCamera(); this.adjustMeshModel()
    
    member private this.adjustMeshModel() =
        let aspect = this.ActualWidth / this.ActualHeight
        let positions = Point3DCollection()
        [Point3D(-aspect / 2., 0.5, 0.);
         Point3D(aspect / 2., 0.5, 0.);
         Point3D(aspect / 2., -0.5, 0.);
         Point3D(-aspect / 2., -0.5, 0.)] |> Seq.iter (fun p -> positions.Add(p))
         
        let model = this.meshModel.Model :?> GeometryModel3D
        let mesh = model.Geometry :?> MeshGeometry3D
        mesh.Positions <- positions;
        
        // Rotating about the left hinge
        ((model.Transform :?> Transform3DGroup).Children.[0] :?> RotateTransform3D).CenterX <- -aspect / 2.
         
    
    member private this.PositionCamera() =
        let camera = this.viewport.Camera :?> PerspectiveCamera
        let aspect = base.ActualWidth / base.ActualHeight
        let z = aspect * 0.866025404
        camera.Position <- Point3D(0.,0.,z)
    
    member private this.dockFrontChild() =
        // Hide the fron child container
        this.frontChildContainer.Visibility <- Visibility.Hidden
        this.viewport.Visibility <- Visibility.Visible
        
        let dockAnim = this.prepareStoryboard({DockDirection = "Dock"; AnimationType = "Dock"}) :> Storyboard
        dockAnim.Begin(this.viewport, true)
        
        // show the back child
        Panel.SetZIndex(this.backChildContainer, 1)
        
        let slideAnim = this.prepareStoryboard({DockDirection = "Dock"; AnimationType = "Slide"}) :> Storyboard
        slideAnim.Begin(this.backChildContainer, true)
    
    member private this.undockFrontChild() =
        let dockAnim = this.prepareStoryboard({DockDirection = "Undock";AnimationType = "Dock"}) :> Storyboard
        let rec eventHandler = EventHandler(fun o e -> 
            
            // Hide the Viewport
            this.viewport.Visibility <- Visibility.Hidden
            Panel.SetZIndex(this.backChildContainer, 0)
            
            // Show the FrontChild
            this.frontChildContainer.Visibility <- Visibility.Visible
            dockAnim.Completed.RemoveHandler(eventHandler)
            )
        dockAnim.Completed.AddHandler(eventHandler)
        dockAnim.Begin(this.viewport, true)
     
    member private this.prepareStoryboard(ap:AnimationParameters) =
        let anim = DockSlidePresenter.Dictionary.[if (ap.AnimationType = "Dock") then "DockAnimation" else "SlideAnimation"] :?> Storyboard // figure out why this is a problem
       // let anim = (this.getBaseStoryboard(if (ap.AnimationType = "Dock") then "DockAnimation" else "SlideAnimation") :?> Storyboard)
        let mutable from = 0.
        let mutable _to = 0.
        
        match ap.AnimationType with
        | "Dock" ->
             from <- if (ap.DockDirection = "Dock") then 0. else 90.
             _to <- if (ap.DockDirection = "Dock") then 90. else 0.
        | "Slide" ->
             from <- if (ap.DockDirection = "Dock") then -base.ActualWidth else 0.
             _to <- if (ap.DockDirection = "Dock") then 0. else -base.ActualWidth
             ((anim.Children.[1]) :?> DoubleAnimation).To <- if (ap.DockDirection = "Dock") then Nullable<float> 1. else Nullable<float> 0.
        | _ -> ()
        
        (anim.Children.[0] :?> DoubleAnimation).From <- Nullable<float> from
        (anim.Children.[0] :?> DoubleAnimation).To <- Nullable<float>  _to
        anim

    
    member private this.getBaseStoryboard(animationName:string) = DockSlidePresenter.Dictionary.[animationName] :?> Storyboard
        
    (* Layout Overrides *)
    override this.MeasureOverride(availableSize) = 
        this.root.Measure(availableSize)
        availableSize
    
    override this.ArrangeOverride(finalSize) =
        this.root.Arrange(Rect(finalSize))
        finalSize
    
    override this.GetVisualChild(index) = 
        match _root with
        | Some(v) -> v :> Visual
        | _ -> null
        

    
    override this.OnRenderSizeChanged(sizeInfo) = this.adjustViewport3D()
    
    override this.VisualChildrenCount = 1  

[<ContentProperty("Children")>]
type ParallaxHelper() =
    inherit DependencyObject()
    
    
    static let attachTransforms (panel:Panel) = panel.Children.to_seq |> Seq.iter(fun c -> c.RenderTransform <- TranslateTransform())
    
    static let canMove (child:UIElement) (panel:Panel) = 
        let childRect = Rect(child.RenderSize)
        let containerRect = Rect(panel.RenderSize)        
        not (Rect.Union(childRect, containerRect).Equals(containerRect))
    
    static let applyParallax((panel:Panel), (factor:Point)) =
        panel.Children.to_seq |> Seq.iter(fun c ->
            if (canMove c panel) 
                then
                    let childRect = Rect(c.RenderSize)
                    let canvasRect = Rect(panel.RenderSize)
                    
                    let x = (childRect.Width - canvasRect.Width) * factor.X
                    let y = (childRect.Height - canvasRect.Height) * factor.Y
                    let mutable xform = c.RenderTransform :?> TranslateTransform
                    xform.X <- -x
                    xform.Y <- -y
                else ())
    

        
    static let onCanvasMouseMove (sender:obj) (e:MouseEventArgs) =
        let panel = (sender :?> Panel)
        let mutable factor = e.GetPosition(panel)
        factor.X <- factor.X / panel.RenderSize.Width
        factor.Y <- factor.Y / panel.RenderSize.Height
        applyParallax(panel,factor)

    static let onApplyParralaxChanged (d:DependencyObject) (e:DependencyPropertyChangedEventArgs) =
        if (e.NewValue :?> bool) 
            then
                let panel = (d :?> Panel)
                let rec handler = RoutedEventHandler(fun o e -> 
                    panel.Loaded.RemoveHandler(handler)
                    attachTransforms(panel)
                    panel.MouseMove.AddHandler(MouseEventHandler(onCanvasMouseMove))
                    
                    applyParallax(panel, Point())
                    )
                panel.Loaded.AddHandler(handler)
    
    static let applyParallaxProperty =
        DependencyProperty.RegisterAttached("ApplyParallax", typeof<bool>, typeof<ParallaxHelper>, PropertyMetadata(false, PropertyChangedCallback(onApplyParralaxChanged)))
        
    
    static member SetApplyParallax (panel:Panel) (apply:bool) = panel.SetValue(applyParallaxProperty, apply)
    

type MagnifyingAdorner(adornedElement:UIElement) =
    inherit Adorner(adornedElement)
    
    let mutable container = None
    let mutable magView = None
    let mutable trans = None
    let mutable magBrush = None
    
    let mutable magnifierTemplate = None
    let mutable containerSize = None
    let mutable scalingFactor = None
    
    member private this.container
        with get() = match container with | Some(v) -> v | None -> failwith "initialization failure"
        and set(x:ContentControl) = container <- Some(x)
        
    member private this.magView
        with get() = match magView with | Some(v) -> v | None -> failwith "initialization failure"
        and set(x:Ellipse) = magView <- Some(x)

    member private this.trans
        with get() = match trans with | Some(v) -> v | None -> failwith "initialization failure"
        and set(x:TranslateTransform) = trans <- Some(x)
        
    member private this.magBrush
        with get() = match magBrush with | Some(v) -> v | None -> failwith "initialization failure"
        and set(x:VisualBrush) = magBrush <- Some(x)
    
    member this.MagnifierTemplate 
        with get() = match magnifierTemplate with | Some(v) -> v | None -> failwith "initialization failure"
        and set(x:ControlTemplate) = magnifierTemplate <- Some(x)
    
    member this.ContainerSize
        with get() = match containerSize with | Some(v) -> v | None -> failwith "initialization failure"
        and set(x:Size) = containerSize <- Some(x)
        
    member this.ScalingFactor
        with get() = match scalingFactor with |Some(v) -> v | None -> failwith "initalization failure"
        and set(x:float) = scalingFactor <- Some(x)
    

    member this.Prepare() =
        container <- Some(ContentControl(IsHitTestVisible = false, Template = this.MagnifierTemplate))
        match magnifierTemplate with 
        | None -> ()
        | Some(v) -> 
            this.container.ApplyTemplate() |> ignore
            this.trans <- TranslateTransform()
            this.container.RenderTransform <- this.trans
            this.magView <- (this.container.Template.FindName("PART_MagnifyingView", this.container) :?> Ellipse)
            let visualBrush =
                let tmp = VisualBrush(base.AdornedElement)
                tmp.RelativeTransform <- ScaleTransform(this.ScalingFactor, this.ScalingFactor, 0.5, 0.5)
                tmp.Viewbox <- Rect(Point(), this.ContainerSize)
                tmp.ViewboxUnits <- BrushMappingMode.Absolute
                tmp
            this.magBrush <- visualBrush
            base.AdornedElement.MouseMove.AddHandler(MouseEventHandler(this.onMouseMove))
            this.magView.Fill <- this.magBrush 
    
    member private this.onMouseMove (sender:obj) (e:MouseEventArgs) =
        this.updateMagnifier()
    
    member private this.updateMagnifier() =
        let p = Mouse.GetPosition(base.AdornedElement)
        this.trans.X <- p.X - this.ContainerSize.Width / 2.
        this.trans.Y <- p.Y - this.ContainerSize.Height / 2.
        
        this.magBrush.Viewbox <- Rect(Point(this.trans.X, this.trans.Y), this.ContainerSize)
        this.InvalidateArrange()
            
    override this.MeasureOverride(_) = this.container.Measure(this.ContainerSize); this.ContainerSize
    
    override this.ArrangeOverride(_) = this.container.Arrange(Rect(this.ContainerSize)); this.ContainerSize
    
    override this.VisualChildrenCount = 1
    
    override this.GetVisualChild(index:int) = match index with | 0 -> this.container :> Visual| _ -> null


[<ContentProperty("Children")>]
type TransitionContainer() =
    inherit ContentControl()
    
    let (handler,event) = Event.create<(obj*EventArgs)>()
    
    static let transitionProperty =
        DependencyProperty.Register("Transition", typeof<TransitionBase>, typeof<TransitionContainer>)
        
    let childContainer = Grid()
    let rootContainer = Grid()
    let transitionContainer = Grid()
    
    let mutable nextChild = None
    let mutable prevChild = None
    
    

    
    do
        rootContainer.Children.Add(transitionContainer) |> ignore
        rootContainer.Children.Add(childContainer) |> ignore
        base.Content <- rootContainer
        
    member this.TransitionCompleted = event
    
    member private this.nextChild
        with get() = match nextChild with | Some(v) -> v | None -> failwith "initialization failure"
        and set(x:UIElement) = nextChild <- Some(x)
        
    member private this.prevChild
        with get() = match prevChild with | Some(v) -> v | None -> failwith "initialization failure"
        and set(x:UIElement) = prevChild <- Some(x)
        
    member this.Children = childContainer.Children
    
    member this.Transition
        with get() =  this.GetValue(transitionProperty) :?> TransitionBase
        and set(x:TransitionBase) = this.SetValue(transitionProperty,x)
        
    member this.ApplyTransition((p:UIElement), (n:UIElement)) =
        match p with
        | null -> failwith "p cannot be null"
        | _ ->
            match n with
            | null -> failwith "n cannot be null"
            | _ ->
                prevChild <- Some(p)
                nextChild <- Some(n)
                this.startTransition()
    
    member private this.startTransition() =
        // Make the children Visible, so that the VisualBrus will not be blank
        this.prevChild.Visibility <- Visibility.Visible
        this.nextChild.Visibility <- Visibility.Visible
        
        // Switch to transition-mode
        let root = this.Transition.SetupVisuals (this.createBrush(this.prevChild)) (this.createBrush(this.nextChild))
        transitionContainer.Children.Add(root) |> ignore
        transitionContainer.Visibility <- Visibility.Visible
        childContainer.Visibility <- Visibility.Hidden
        
        // Get Storyboard to play
        let sb = this.Transition.PrepareStoryboard(this)
        let rec handler = EventHandler(fun o e -> 
            sb.Completed.RemoveHandler(handler)
            this.FinishTransition()
            )
        sb.Completed.AddHandler(handler)
        sb.Begin(transitionContainer)

    member private this.createBrush(elt:UIElement) =
        let brush = VisualBrush(elt)
        RenderOptions.SetCachingHint(brush, CachingHint.Cache)
        brush.Viewbox <- Rect(0.,0., base.ActualWidth, base.ActualHeight)
        brush.ViewboxUnits <- BrushMappingMode.Absolute
        brush 
    
    member this.FinishTransition() =
        // Bring the next-child on top
        this.changeChildrenStackOrder()
        
        transitionContainer.Visibility <- Visibility.Hidden
        childContainer.Visibility <- Visibility.Visible
        
        transitionContainer.Children.Clear()
        
        (* to do *)
        // this.NotifyTransitionCompleted()

    member private this.changeChildrenStackOrder() =
        Panel.SetZIndex(this.nextChild, 1)
        childContainer.Children.to_seq |> Seq.iter(fun element -> 
            if (element <> this.nextChild)
                then
                    Panel.SetZIndex(element,0)
                    element.Visibility <- Visibility.Hidden)
                    
    member private this.notifyTransitionCompleted() = handler(this,null)

and 
    [<AbstractClass>]
    TransitionBase() =
    let mutable duration = Duration(TimeSpan.FromSeconds(1.))
    
    member this.Duration
        with get() = duration
        and set(x:Duration) = duration <- x
    
    abstract member PrepareStoryboard:TransitionContainer -> Storyboard
    abstract member SetupVisuals: VisualBrush -> VisualBrush -> FrameworkElement
    
type FadeTransition() =
    inherit TransitionBase()
    
    let mutable nextRect = None
    let mutable prevRect = None
    let mutable rectContainer = None
    
    member private this.nextRect
        with get() = match nextRect with | Some(v) -> v | None -> failwith "initialization failure"
        and set(x:Rectangle) = nextRect <- Some(x)
        
    member private this.prevRect
        with get() = match prevRect with | Some(v) -> v | None -> failwith "initialization failure"
        and set(x:Rectangle) = prevRect <- Some(x)
        
    member private this.rectContainer
        with get() = match rectContainer with | Some(v) -> v | None -> failwith "initialization failure"
        and set(x:Grid) = rectContainer <- Some(x)
        
    
    override this.SetupVisuals p n  =
        this.prevRect <- Rectangle()
        this.prevRect.Fill <- p
        
        this.nextRect <- Rectangle()
        this.nextRect.Fill <- n
        
        this.rectContainer <- Grid()
        this.rectContainer.ClipToBounds <- true
        this.rectContainer.Children.Add(this.nextRect) |> ignore
        this.rectContainer.Children.Add(this.prevRect) |> ignore
        
        this.rectContainer :> FrameworkElement
        
    override this.PrepareStoryboard(container) =
        let animator = Storyboard()
        
        let prevAnim = DoubleAnimation()
        Storyboard.SetTarget(prevAnim, this.prevRect)
        Storyboard.SetTargetProperty(prevAnim, PropertyPath(UIElement.OpacityProperty))
        prevAnim.Duration <- this.Duration
        prevAnim.From <- Nullable<float> 1.
        prevAnim.To <- Nullable<float> 0.
        
        let nextAnim = DoubleAnimation()
        Storyboard.SetTarget(nextAnim, this.nextRect)
        Storyboard.SetTargetProperty(nextAnim, PropertyPath(UIElement.OpacityProperty))
        nextAnim.Duration <- this.Duration
        nextAnim.From <- Nullable<float> 0.
        nextAnim.To <- Nullable<float> 1.
        
        animator.Children.Add(prevAnim)
        animator.Children.Add(nextAnim)
        animator
        
        
type Direction =
    | LeftToRight
    | RightToLeft
    
type SlideTransition(direction:Direction) =
    inherit TransitionBase()
    
    let mutable _direction = direction
    let mutable _nextRect = None
    let mutable _prevRect = None
    let mutable _rectContainer = None
    
    new() = SlideTransition(Direction.LeftToRight)
            
    member private this.nextRect 
        with get() = match _nextRect with | Some(v) -> v | _ -> failwith "initialization failure"
        and set(x:Rectangle) = _nextRect <- Some(x)
        
    member private this.prevRect 
        with get() = match _prevRect with | Some(v) -> v | _ -> failwith "initialization failure"
        and set(x:Rectangle) = _prevRect <- Some(x)
        
    member private this.rectContainer 
        with get() = match _rectContainer with | Some(v) -> v | _ -> failwith "initialization failure" 
        and set(x:Grid) = _rectContainer <- Some(x)
    
   
        
    override this.SetupVisuals prevBrush nextBrush =
        this.prevRect <- Rectangle()
        this.prevRect.Fill <- prevBrush
        this.prevRect.RenderTransform <- TranslateTransform()
        
        this.nextRect <- Rectangle()
        this.nextRect.Fill <- nextBrush
        this.nextRect.RenderTransform <- TranslateTransform()
        
        this.rectContainer <- Grid()
        this.rectContainer.ClipToBounds <- true
        this.rectContainer.Children.Add(this.nextRect) |> ignore
        this.rectContainer.Children.Add(this.prevRect) |> ignore
        this.rectContainer :> FrameworkElement
        
    override this.PrepareStoryboard(container) =
        let animator = Storyboard()
        let prevAnim = DoubleAnimation()
        Storyboard.SetTarget(prevAnim, this.prevRect) // Hilight this in the chapter!
        Storyboard.SetTargetProperty(prevAnim, PropertyPath("(0).(1)", UIElement.RenderTransformProperty, TranslateTransform.XProperty))
        prevAnim.Duration <- this.Duration
        prevAnim.From <- Nullable<float> 0.
        prevAnim.To <- Nullable<float>  (if (_direction = Direction.RightToLeft) then -1. * container.ActualWidth else container.ActualWidth)
        
        let nextAnim = DoubleAnimation()
        Storyboard.SetTarget(nextAnim, this.nextRect)
        Storyboard.SetTargetProperty(nextAnim, PropertyPath("(0).(1)", UIElement.RenderTransformProperty, TranslateTransform.XProperty))
        nextAnim.Duration <- this.Duration
        nextAnim.From <- Nullable<float> (if (_direction = Direction.RightToLeft) then container.ActualWidth else container.ActualWidth * -1.)
        nextAnim.To <- Nullable<float> 0.
        
        animator.Children.Add(prevAnim)
        animator.Children.Add(nextAnim)
        
        animator
        
        
    