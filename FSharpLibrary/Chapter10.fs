namespace FSharp.Chapter10

open System
open System.Windows
open System.Windows.Input
open System.Windows.Controls

module tmp3 = 
    type UIElementCollection with
        member public this.to_seq = seq {for i in 0 .. this.Count - 1 -> this.[i]}
    
open tmp3   

type SkinThemeControl() =
    inherit ContentControl()
    static do
        ContentControl.DefaultStyleKeyProperty.OverrideMetadata(typeof<SkinThemeControl>, new FrameworkPropertyMetadata(typeof<SkinThemeControl>))
        
[<TemplatePart(Name = "PART_Close", Type = typeof<Button>)>]
[<TemplatePart(Name = "PART_Minimize", Type = typeof<Button>)>]
[<TemplatePart(Name = "PART_Maximize", Type = typeof<Button>)>]
type CustomChromeWindow() as this =
    inherit Window()
    static let hoverImageProperty = DependencyProperty.RegisterAttached("HoverImage", typeof<string>, typeof<CustomChromeWindow>)
    static let normalImageProperty = DependencyProperty.RegisterAttached("NormalImage", typeof<string>, typeof<CustomChromeWindow>)
    static let pressedImageProperty = DependencyProperty.RegisterAttached("PressedImage", typeof<string>, typeof<CustomChromeWindow>)
      
    static do
        CustomChromeWindow.DefaultStyleKeyProperty.OverrideMetadata(typeof<CustomChromeWindow>, FrameworkPropertyMetadata(typeof<CustomChromeWindow>))

    override this.OnApplyTemplate() =
        base.OnApplyTemplate()
        this.attachToVisualTree()        
    
    static member public SetHoverImage(d:DependencyObject, source) = d.SetValue(hoverImageProperty, source)
    static member public GetHoverImage(d:DependencyObject) = d.GetValue(hoverImageProperty) :?> string
    
    static member public SetNormalImage(d:DependencyObject, source) = d.SetValue(normalImageProperty, source)
    static member public GetNormalImage(d:DependencyObject) = d.GetValue(normalImageProperty) :?> string
    
    static member public SetPressedImage(d:DependencyObject, source) = d.SetValue(pressedImageProperty, source)
    static member public GetPressedImage(d:DependencyObject) = d.GetValue(pressedImageProperty) :?> string
//    
    member private this.attachToVisualTree() =
        match this.GetTemplateChild("PART_Close") with
        | null -> ()
        | v -> (v :?> Button).Click.AddHandler(RoutedEventHandler(this.onCloseButtonClick))


        match this.GetTemplateChild("PART_Minimize") with
        | null -> ()
        | v -> (v :?> Button).Click.AddHandler(RoutedEventHandler(this.onMinimizeButtonClick))
        
        match this.GetTemplateChild("PART_Maximize") with
        | null -> ()
        | v -> (v :?> Button).Click.AddHandler(RoutedEventHandler(this.onMaximizeButtonClick))

        
        match this.GetTemplateChild("PART_TitleBar") with
        | null -> ()
        | v -> (v :?> Panel).MouseLeftButtonDown.AddHandler(MouseButtonEventHandler(this.onTitleBarMouseDown))
             
        

    member private this.onMouseButtonClick (sender:obj) (e:MouseButtonEventArgs) = this.toggleMaximize()
    
    member private this.toggleMaximize() =
        this.WindowState <- if (this.WindowState = WindowState.Maximized) then WindowState.Normal else WindowState.Maximized
    
    member private this.onTitleBarMouseDown (sender:obj) (e:MouseButtonEventArgs) =
        if (this.ResizeMode <> ResizeMode.NoResize && e.ClickCount = 2)
            then
                this.toggleMaximize()
            else
                this.DragMove()

    member private this.onMaximizeButtonClick (sender:obj) (e:RoutedEventArgs) = this.toggleMaximize()
    
    member private this.onCloseButtonClick (sender:obj) (e:RoutedEventArgs) = this.Close()
    
    member private this.onMinimizeButtonClick (sender:obj) (e:RoutedEventArgs) = this.WindowState <- WindowState.Minimized
    

    
    