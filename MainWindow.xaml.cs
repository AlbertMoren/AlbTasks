using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AlbTasks.ViewModels;

namespace AlbTasks
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is LembreteViewModel vm)
            {
                if (DataContext is MainViewModel mainVm)
                {
                    mainVm.EditarDiariaCommand.Execute(vm.AtividadeOriginal);
                }
            }
        }
    }
}