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

namespace WpfApplication1
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Customer> Customers { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Customers = new ObservableCollection<Customer>
                            {
                                new Customer
                                    {
                                        Name = "Kalle", Age = "10", Parts = new ObservableCollection<Part> { new Part{Name = "Book", Age = "3" }, new Part{Name = "Computer", Age = "1" } }
                                    },
                                new Customer
                                    {
                                        Name = "Pelle", Age = "30", Parts = new ObservableCollection<Part> { new Part{Name = "Book2", Age = "5" }, new Part{Name = "Computer2", Age = "6" } }
                                    },
                            };

            this.DataContext = this;
        }

        
    }

    public class Customer
    {
        public ObservableCollection<Part> Parts { get; set; }

        public string Name { get; set; }
        public string Age { get; set; }
        public Customer()
        {
            Parts = new ObservableCollection<Part>();
        }

    }
    public class Part
    {
        public string Name { get; set; }
        public string Age { get; set; }
    }
}
