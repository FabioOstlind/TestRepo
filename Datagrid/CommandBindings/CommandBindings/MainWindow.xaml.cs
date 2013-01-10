using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommandBindings
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
           
            InitializeComponent();
            CommandBinding binding = new CommandBinding(ApplicationCommands.New);
            binding.Executed += BindingOnExecuted;
            this.CommandBindings.Add( binding );
            binding.PreviewExecuted += (sender, args) => { MessageBox.Show("this occurs before executing the bindingOnexecuted handler"); };
        }

        private void BindingOnExecuted( object sender, ExecutedRoutedEventArgs executedRoutedEventArgs )
        {
            var test = ApplicationCommands.New;
            string name = test.Name;
            MessageBox.Show( "New command triggered by:" + name + ": " + executedRoutedEventArgs.Source.ToString() );
        }
    }
}
