using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfDataGrid
{
    public class TestClass
    {


        public static List<Categories> GetCategories()
        {
            TestDBEntities tdbe = new TestDBEntities();
            var result = tdbe.Categories.Select(x => x).ToList();
            return result;
        }
        public static ObservableCollection<Categories> GetObservableCategories(TestDBEntities context)
        {
            ObservableCollection<Categories> tempCat = new ObservableCollection<Categories>();
           
            var result = context.Categories.Select(category => category ) ; 
            foreach (var category in result)
            {
                tempCat.Add(category);
            }
            ObservableCollection<Categories> Categories = tempCat;
            return Categories ;
        }
        public static List<Tuple<Nullable<decimal>, string, IEnumerable<Products>>> GetUnitCost(TestDBEntities context)
        {
            var query = from category in context.Categories
                        join products in context.Products
                        on category.CategoryID equals products.CategoryID into matches
                        from match in matches
                        select new Tuple<Nullable<decimal>, string, IEnumerable<Products>>(match.UnitCost, match.ModelName,matches);

            return query.ToList();   
        


        }
        internal class UnitModel
        { 
            public Nullable<decimal> UnitCost {get;set;}
            public string ModelName { get; set; }
        }
        public ObservableCollection<Customer> Customers
        {
            get
            {
                return new ObservableCollection<Customer> 
            { 
                new Customer{ FirstName = "Joe", LastName = "Bush"}, 
                new Customer{ FirstName = "Joe2", LastName = "Bush2"}, 
            };
            }
        }
    }
    public class Customer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

    }
}
