namespace FSharp.Chapter2

open System
open System.IO
open System.Text
open System.Xml
open System.Windows
open System.Windows.Media
open System.Windows.Shapes
open System.Windows.Controls
open System.Windows.Markup

type SectorShape() = 
    class
        inherit Shape()
        override x.DefiningGeometry = x.GetSectorGeometry()
        member x.GetSectorGeometry() = 
            let geometry = new StreamGeometry();
            using (geometry.Open()) (fun c -> 
                c.BeginFigure(new Point(200.,200.), true, true);
                //First line
                c.LineTo(new Point(175.,50.), true, true);
                // Bottom arc
                c.ArcTo(new Point(50., 150.), new Size(1.,1.), 0., true, SweepDirection.Counterclockwise, true, true );
                // Second line
                c.LineTo(new Point(200., 200.), true, true);
                )
            geometry :> Geometry
    end

type SectorVisual() = 
    class
        inherit DrawingVisual()        
        do
            let geometry = new StreamGeometry();
            using (geometry.Open()) (fun c -> 
                c.BeginFigure(new Point(200.,200.), true, true);
                //First line
                c.LineTo(new Point(175.,50.), true, true);
                // Bottom arc
                c.ArcTo(new Point(50., 150.), new Size(1.,1.), 0., true, SweepDirection.Counterclockwise, true, true );
                // Second line
                c.LineTo(new Point(200., 200.), true, true);
                )
            using (base.RenderOpen()) (fun context ->
                let pen = new Pen(Brushes.Black, 1.);
                context.DrawGeometry(Brushes.CornflowerBlue, pen, geometry)
            )

    end
    
type VisualContainer() =
    class
        inherit FrameworkElement()
        let _visual = new SectorVisual()
        override x.GetVisualChild(index) = _visual :> Visual
        override x.VisualChildrenCount = 1
    end