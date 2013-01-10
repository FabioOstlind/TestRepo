namespace IdataErrorValidation.ViewModel
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;

    public class PriceIntRangeRule : ValidationRule
    {
        private decimal min = 0;

        private decimal max = decimal.MaxValue;
        public decimal Min
        {
            get
            {
                return this.min;
            }
            set
            {
                this.min = value;
            }

        }
        public decimal Max
        {
            get
            {
                return this.max;
            }
            set
            {
                this.max = value;
            }

        }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            decimal price = 0;

            try
            {
                var s = (string)value;
                if(!string.IsNullOrEmpty(s))
                {
                    price = decimal.Parse((string)value, NumberStyles.Any, cultureInfo);
                }
            }
            catch (Exception)
            {

                return new ValidationResult(false, "illigal character");
            }

            if ((price > this.min) || (price > this.max))
            {
                return new ValidationResult(false, string.Format("not inte the range {0} to {1}", this.min, this.max));
            }
            else
            {
                return new ValidationResult(true, null);
            }






        }
    }
}
