Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Media
Imports DevExpress.Xpf.Map

Namespace MapControl_RouteCalculator
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow
        Inherits Window

        Private helper As New RoutingHelper()

        Public Sub New()
            InitializeComponent()
        End Sub

        #Region "#EventHandlers"
        Private Sub LocationInformationReceived(ByVal sender As Object, ByVal e As LocationInformationReceivedEventArgs)
            If (e.Cancelled) AndAlso (e.Result.ResultCode <> RequestResultCode.Success) Then
                Return
            End If
            GenerateItems(e.Result.Locations)
        End Sub

        Private Sub SearchCompleted(ByVal sender As Object, ByVal e As BingSearchCompletedEventArgs)
            If e.Cancelled OrElse (e.RequestResult.ResultCode <> RequestResultCode.Success) Then
                Return
            End If

            If e.RequestResult.SearchResults.Count <> 0 Then
                GenerateItems(e.RequestResult.SearchResults)
            Else
                GenerateItems(New LocationInformation() { e.RequestResult.SearchRegion })
            End If
        End Sub

        Private Sub RouteCalculated(ByVal sender As Object, ByVal e As BingRouteCalculatedEventArgs)
            If e.Cancelled Then
                Return
            End If

            If e.CalculationResult.ResultCode <> RequestResultCode.Success Then
                Return
            End If
            helper.BuildRoute(e.CalculationResult.RouteResults(0).RoutePath)
            UpdateStorage()
        End Sub

        Private Sub GenerateItems(ByVal locations As IEnumerable(Of LocationInformation))
            UpdateStorage()
            For Each location In locations
                Dim pushpin As New MapPushpin() With {.Location = location.Location, .Information = location}
                AddHandler pushpin.MouseLeftButtonDown, AddressOf pushpin_MouseLeftButtonDown
                storage.Items.Add(pushpin)
            Next location
        End Sub

        Private Sub UpdateStorage()
            storage.Items.Clear()
            storage.Items.Add(helper.Route)
            For Each pushpin As MapPushpin In helper.Pushpins
                storage.Items.Add(pushpin)
            Next pushpin
        End Sub

        Private Sub pushpin_MouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
            Dim pushpin As MapPushpin = TryCast(sender, MapPushpin)
            If pushpin Is Nothing Then
                Return
            End If
            e.Handled = True
            helper.AddItem(pushpin)
            routeProvider.CalculateRoute(helper.Waypoints)
            RemoveHandler pushpin.MouseLeftButtonDown, AddressOf pushpin_MouseLeftButtonDown
        End Sub
        #End Region ' #EventHandlers

        Private Sub Button_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            helper.Clear()
            UpdateStorage()
        End Sub
    End Class

    #Region "#Helper"
    Friend Class RoutingHelper

        Private pushpins_Renamed As New List(Of MapPushpin)()

        Private route_Renamed As New MapPolyline() With { _
            .Stroke = New SolidColorBrush(Color.FromArgb(&HFF, &H8A, &HFB, &HFF)), .StrokeStyle = New StrokeStyle() With {.Thickness = 3} _
        }

        Private waypoints_Renamed As New List(Of RouteWaypoint)()
        Private currentLatter As Char = "A"c

        Public ReadOnly Property Pushpins() As List(Of MapPushpin)
            Get
                Return pushpins_Renamed
            End Get
        End Property
        Public ReadOnly Property Waypoints() As List(Of RouteWaypoint)
            Get
                Return waypoints_Renamed
            End Get
        End Property
        Public Property Route() As MapPolyline
            Get
                Return route_Renamed
            End Get
            Set(ByVal value As MapPolyline)
                route_Renamed = value
            End Set
        End Property
        Public ReadOnly Property Count() As Integer
            Get
                Return pushpins_Renamed.Count
            End Get
        End Property

        Public Sub BuildRoute(ByVal points As IEnumerable(Of GeoPoint))
            route_Renamed.Points.Clear()
            For Each point As GeoPoint In points
                route_Renamed.Points.Add(point)
            Next point
        End Sub

        Public Sub AddItem(ByVal pushpin As MapPushpin)
            pushpins_Renamed.Add(pushpin)
            waypoints_Renamed.Add(New RouteWaypoint(CType(pushpin.Information, LocationInformation).DisplayName, CType(pushpin.Location, GeoPoint)))
            pushpin.Text = (currentLatter).ToString()
            currentLatter = ChrW(AscW(currentLatter) + 1)
        End Sub

        Public Sub Clear()
            route_Renamed.Points.Clear()
            pushpins_Renamed.Clear()
            waypoints_Renamed.Clear()
            currentLatter = "A"c
        End Sub
    End Class
    #End Region ' #Helper
End Namespace
