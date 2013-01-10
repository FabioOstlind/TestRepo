using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;

namespace wpfBindings
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
       public class MyData {public string name { get; set; } public int Age { get; set; }    }
       public class Person { public string name { get; set; } public int Age { set; get; } public bool Member { set; get; }
       public Person()
       {
           
       }
       }



       public ObservableCollection<Person> PersonList
       {
           get;
           set;
       }  
        
        public MainWindow()
        {
           
            InitializeComponent();
            PersonList = this.ReturnPeopleList();
            this.DataContext = this;
           
           
           
            
            Datagrid1.CanUserAddRows = true;
            Datagrid1.CanUserDeleteRows = true;
            Datagrid1.CanUserReorderColumns = true;


        }

        public ObservableCollection<Person> ReturnPeopleList()
        {
            return new ObservableCollection<Person>
                   {

                new Person { name = "john", Age = 25, Member = true},
                new Person { name = "jill", Age = 25,  Member = false},
                new Person { name = "bill", Age = 15,  Member = true}
            };

         
        }
        public void SetUpGrid()
        {
            Datagrid1.Items.Add(new MyData { name = "nisse", Age = 32 });
            Datagrid1.Items.Add(new MyData { name = "anton", Age = 44 });
            
            Datagrid1.Items.Add( new DataGridRow() );
            DataGridTextColumn col2 = new DataGridTextColumn();
            DataGridTextColumn col3 = new DataGridTextColumn();
         
          //col1.Binding = new Binding(".");
          col2.Binding = new Binding("name");
          col3.Binding = new Binding("age");
          col2.Header = "name";
          col3.Header = "age";
         // Datagrid1.Columns.Add(col1);
          Datagrid1.Columns.Add(col2);
          Datagrid1.Columns.Add(col3);
            //Datagrid1.Items.Add("fabbe");
            //Datagrid1.Items.Add("sten");
     
        


        }

      

        private void Datagrid1_OnBeginningEdit(object sender, DataGridBeginningEditEventArgs e )
        {
           // Om column är name och har tagen readonly canc
            if( 0 == string.CompareOrdinal( e.Column.Header.ToString(), "Member" ) )
            {
                var dataGrid = sender as DataGrid;
                return;
            }

            if (((DataGridRow)e.Row).Tag != null && ((DataGridRow)e.Row).Tag.ToString() == "ReadOnly")
            {
                e.Cancel = true;
            }
        }
    }
}
