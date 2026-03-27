using System.Windows;
using AntiTrendForecast.Directive.Handlers;
using AntiTrendForecast.Directive.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AntiTrendForecast.Directive;

/// <summary>
/// Interaction logic for MainWindow.xaml (WPF version)
/// </summary>
public partial class MainWindow : Window
{
    public DashboardViewModel ViewModel { get; }

    public MainWindow(ITrendInputHandler handler)
    {
        InitializeComponent();
        
        // Constructor injection of the handler, but we use the VM
        ViewModel = new DashboardViewModel(handler);
        this.DataContext = ViewModel;
    }

    private async void OnAnalyzeClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.AnalyzeCommand.CanExecute(null))
        {
            await ViewModel.AnalyzeCommand.ExecuteAsync(null);
        }
    }
}
