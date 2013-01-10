using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGridTemplates.Model;
namespace DataGridTemplates.ViewModel
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ImpCalendarVM : INotifyPropertyChanged 
    {
        private readonly IMP_CALENDAR instance;

        public  string Factory
        {
            get
            {
                return this.instance.FACTORY;
            }
            set
            {
                this.instance.FACTORY = value;
            }
        }

      public string Project
      {
          get
          {
              return this.instance.PROJECT;
          }
          set
          {
              this.instance.PROJECT = value;
          }
      }

      public string Name
      {
          get
          {
              return this.instance.NAME;
          }
          set
          {
              this.instance.NAME = value;
          }
      }

      public string Description
      {
          get
          {
              return this.instance.DESCRIPTION;
          }
          set
          {
              this.instance.DESCRIPTION = value;
          }
      }

      public string CreatedBy
      {
          get
          {
              return this.instance.CREATED_BY;
          }
          set
          {
              this.instance.CREATED_BY = value;
          }
      }

      public DateTime? CreatedDate
      {
          get
          {
              return this.instance.CREATED_DATE;
          }
          set
          {
              this.instance.CREATED_DATE = value;
          }
      }

      public string ChangedBy
      {
          get
          {
              return this.instance.CHANGED_BY;
          }
          set
          {
              this.instance.CHANGED_BY = value;
          }
      }

      public DateTime? ChangedDate
      {
          get
          {
              return this.instance.CHANGED_DATE;
          }
          set
          {
              this.instance.CHANGED_DATE = value;
          }
      }

      public ImpCalendarVM()
      {
          this.instance = new IMP_CALENDAR();
      }
      //public ImpCalendarVM(IMP_CALENDAR calendar)
      //{
      //    this.instance = new IMP_CALENDAR
      //                        {
      //                            CHANGED_BY = calendar.CHANGED_BY,
      //                            CHANGED_DATE = calendar.CHANGED_DATE,
      //                            DESCRIPTION = calendar.DESCRIPTION,
      //                            FACTORY = calendar.FACTORY,
      //                            NAME = calendar.NAME,
      //                            PROJECT = calendar.PROJECT,
      //                            CREATED_BY = calendar.CREATED_BY,
      //                            CREATED_DATE = calendar.CREATED_DATE
      //                        };
         
      //}
        

      private bool isSelected;

  

      public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (isSelected.Equals(value))
                {
                    return;
                }
                isSelected = value;
                this.OnPropertyChanged();
            }

        }

      public event PropertyChangedEventHandler PropertyChanged;
      protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
      {
          if (this.PropertyChanged != null)
          {
              this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
          }
      }
    }
}
