using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace WpfDataGrid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Categories> Categories { get; set; }
        public List<Categories> Categories2 { get; set; }
        //public ObservableCollection<Customer> customer { get; set; }
        //public ObservableCollection<Customer> Customers { get; set; } 
        public MainWindow()
        {
            TestDBEntities tdbe = new TestDBEntities();
            Categories = TestClass.GetObservableCategories(tdbe);
            //var tupleList = TestClass.GetUnitCost(tdbe);
           // Categories2= new TestClass.GetCategories(); ;
           // Categories = new TestClass.GetObservableCategories();
            InitializeComponent();
            this.DataContext = this;
            //dgCust.ItemsSource = tupleList[0].Item3;
            
            
        }


   }
}
