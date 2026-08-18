using System.Windows;
using AlbTasks.ViewModels;

namespace AlbTasks
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Aqui conectamos a tela aos nossos dados usando o padrão MVVM
            DataContext = new MainViewModel();
        }
    }
}