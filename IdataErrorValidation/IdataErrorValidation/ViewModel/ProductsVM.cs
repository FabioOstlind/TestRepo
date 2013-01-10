using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdataErrorValidation.ViewModel
{
    using System.Collections.ObjectModel;

    using IdataErrorValidation.Annotations;
    using IdataErrorValidation.Model;

    public class ProductsVM
    {
        public ObservableCollection<ProductVM> Products {get; set; }
           
            

        private IEnumerable<Product> GetProducts()
        {
            List<Product> list = new List<Product> { new Product { ModelName = "Audio", RegNumber = 2345 },
            new Product { ModelName = "Audio", RegNumber = 2345 },
            new Product { ModelName = "Volvo", RegNumber = 3567 },
            new Product { ModelName = "BMW", RegNumber = 1346 }};

            return list;
        }
        public ProductsVM()
        {
            var productsList = this.GetProducts();

            var productVMList =
                productsList.Select(
                    product => new ProductVM (){ ModelName = product.ModelName, RegNumber = product.RegNumber }).ToList();

            this.Products = new ObservableCollection<ProductVM>(productVMList);
        }
    }
}
