using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdataErrorValidation.ViewModel
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using IdataErrorValidation.Annotations;
    using IdataErrorValidation.Model;

   public enum Property : byte 
    {
       Invalid =0x00,
       ModelName = 0x01,
       RegNumber = 0x02
    }


    public class ProductVM : INotifyPropertyChanged , IDataErrorInfo
    {
        public ProductVM()
        {
            this.product = new Product();
        }
      
        private readonly Product product;

        public int RegNumber
        {
            get
            {
                return this.product.RegNumber;
            }
            set
            {
                if (this.product.RegNumber.Equals(value))
                {
                    return;
                }
                this.product.RegNumber = value;
                this.OnPropertyChanged();
            }
        }
        public string ModelName
        {
            get
            {
                return this.product.ModelName;
            }
            set
            {
              
                this.product.ModelName = value;
                this.OnPropertyChanged();
            }
        }

        //public ProductVM(Product product)
        //{
        //    this.product = new Product { ModelName = product.ModelName, RegNumber = product.RegNumber };
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string Error
        {
            get
            {
                return null;
            }
        }

        public string this[string propertyName]
        {
            get
            {
                switch (propertyName)
                {
                    case "ModelName":
                        {
                            bool isValid = true;
                            string modelName = this.ModelName;
                            if (modelName == null)
                            {
                                isValid = false;
                            }
                            
                            foreach (char c in modelName)
                            {
                                if (Char.IsLetterOrDigit(c))
                                {
                                    continue;
                                }
                                isValid = false;
                                break;
                            }
                            
                            return !isValid ? "The Modelname can only contain letters and numbers" : null;
                        }
                    case "RegNumber":
                        {
                            int value = this.RegNumber;
                            bool isInRange = value >= 0 && value <= 9999;

                            if (!isInRange)
                            {
                                return "Value is not in range";
                            }
                            return null;
                        }
                    default:
                        return "something bad happened";

                }
            }
        }

    }
}
