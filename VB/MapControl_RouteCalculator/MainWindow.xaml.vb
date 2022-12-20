Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Media
Imports DevExpress.Xpf.Map

Namespace MapControl_RouteCalculator

    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Public Partial Class MainWindow
        Inherits System.Windows.Window

        Private helper As MapControl_RouteCalculator.RoutingHelper = New MapControl_RouteCalculator.RoutingHelper()

        Public Sub New()
            Me.InitializeComponent()
        End Sub

'#Region "#EventHandlers"
        Private Sub LocationInformationReceived(ByVal sender As Object, ByVal e As DevExpress.Xpf.Map.LocationInformationReceivedEventArgs)
            If(e.Cancelled) AndAlso (e.Result.ResultCode <> DevExpress.Xpf.Map.RequestResultCode.Success) Then Return
            Me.GenerateItems(e.Result.Locations)
        End Sub

        Private Sub SearchCompleted(ByVal sender As Object, ByVal e As DevExpress.Xpf.Map.BingSearchCompletedEventArgs)
            If e.Cancelled OrElse (e.RequestResult.ResultCode <> DevExpress.Xpf.Map.RequestResultCode.Success) Then Return
            If e.RequestResult.SearchResults.Count <> 0 Then
                Me.GenerateItems(e.RequestResult.SearchResults)
            Else
                Me.GenerateItems(New DevExpress.Xpf.Map.LocationInformation() {e.RequestResult.SearchRegion})
            End If
        End Sub

        Private Sub RouteCalculated(ByVal sender As Object, ByVal e As DevExpress.Xpf.Map.BingRouteCalculatedEventArgs)
            If e.Cancelled Then Return
            If e.CalculationResult.ResultCode <> DevExpress.Xpf.Map.RequestResultCode.Success Then Return
            Me.helper.BuildRoute(e.CalculationResult.RouteResults(CInt((0))).RoutePath)
            Me.UpdateStorage()
        End Sub

        Private Sub GenerateItems(ByVal locations As System.Collections.Generic.IEnumerable(Of DevExpress.Xpf.Map.LocationInformation))
            Me.UpdateStorage()
            For Each location In locations
                Dim pushpin As DevExpress.Xpf.Map.MapPushpin = New DevExpress.Xpf.Map.MapPushpin() With {.Location = location.Location, .Information = location}
                AddHandler pushpin.MouseLeftButtonDown, AddressOf Me.pushpin_MouseLeftButtonDown
                Me.storage.Items.Add(pushpin)
            Next
        End Sub

        Private Sub UpdateStorage()
            Me.storage.Items.Clear()
            Me.storage.Items.Add(Me.helper.Route)
            For Each pushpin As DevExpress.Xpf.Map.MapPushpin In Me.helper.Pushpins
                Me.storage.Items.Add(pushpin)
            Next
        End Sub

        Private Sub pushpin_MouseLeftButtonDown(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
            Dim pushpin As DevExpress.Xpf.Map.MapPushpin = TryCast(sender, DevExpress.Xpf.Map.MapPushpin)
            If pushpin Is Nothing Then Return
            e.Handled = True
            Me.helper.AddItem(pushpin)
            Me.routeProvider.CalculateRoute(Me.helper.Waypoints)
            RemoveHandler pushpin.MouseLeftButtonDown, AddressOf Me.pushpin_MouseLeftButtonDown
        End Sub

'#End Region  ' #EventHandlers
        Private Sub Button_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
            Me.helper.Clear()
            Me.UpdateStorage()
        End Sub
    End Class

'#Region "#Helper"
    Friend Class RoutingHelper

        Private pushpinsField As System.Collections.Generic.List(Of DevExpress.Xpf.Map.MapPushpin) = New System.Collections.Generic.List(Of DevExpress.Xpf.Map.MapPushpin)()

        Private routeField As DevExpress.Xpf.Map.MapPolyline = New DevExpress.Xpf.Map.MapPolyline() With {.Stroke = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(&HFF, &H8A, &HFB, &HFF)), .StrokeStyle = New DevExpress.Xpf.Map.StrokeStyle() With {.Thickness = 3}}

        Private waypointsField As System.Collections.Generic.List(Of DevExpress.Xpf.Map.RouteWaypoint) = New System.Collections.Generic.List(Of DevExpress.Xpf.Map.RouteWaypoint)()

        Private currentLatter As Char = "A"c

        Public ReadOnly Property Pushpins As List(Of DevExpress.Xpf.Map.MapPushpin)
            Get
                Return Me.pushpinsField
            End Get
        End Property

        Public ReadOnly Property Waypoints As List(Of DevExpress.Xpf.Map.RouteWaypoint)
            Get
                Return Me.waypointsField
            End Get
        End Property

        Public Property Route As MapPolyline
            Get
                Return Me.routeField
            End Get

            Set(ByVal value As MapPolyline)
                Me.routeField = value
            End Set
        End Property

        Public ReadOnly Property Count As Integer
            Get
                Return Me.pushpinsField.Count
            End Get
        End Property

        Public Sub BuildRoute(ByVal points As System.Collections.Generic.IEnumerable(Of DevExpress.Xpf.Map.GeoPoint))
            Me.routeField.Points.Clear()
            For Each point As DevExpress.Xpf.Map.GeoPoint In points
                Me.routeField.Points.Add(point)
            Next
        End Sub

        Public Sub AddItem(ByVal pushpin As DevExpress.Xpf.Map.MapPushpin)
            Me.pushpinsField.Add(pushpin)
            Me.waypointsField.Add(New DevExpress.Xpf.Map.RouteWaypoint(CType(pushpin.Information, DevExpress.Xpf.Map.LocationInformation).DisplayName, CType(pushpin.Location, DevExpress.Xpf.Map.GeoPoint)))
            pushpin.Text =(System.Math.Min(System.Threading.Interlocked.Increment(Me.currentLatter), Me.currentLatter - 1)).ToString()
        End Sub

        Public Sub Clear()
            Me.routeField.Points.Clear()
            Me.pushpinsField.Clear()
            Me.waypointsField.Clear()
            Me.currentLatter = "A"c
        End Sub
    End Class
'#End Region  ' #Helper
End Namespace
