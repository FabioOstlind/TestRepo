using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandBindings.Model
{
   public class InfoModel
   {
       /// <summary>
       /// Initializes a new instance of the <see cref="T:System.Object"/> class.
       /// </summary>
       public InfoModel(string filename , DateTime creationDate)
       {
           this.filename = filename;
           this.creationDate = creationDate;
       }

       /// <summary>
       /// Initializes a new instance of the <see cref="T:System.Object"/> class.
       /// </summary>
       public InfoModel()
       {
       }

       
       private string filename;

       public string Filename
       {
           get
           {
               return this.filename;
           }
           set
           {
               this.filename = value;
           }
       }

       private DateTime creationDate;

       public DateTime CreationDate
       {
           get
           {
               return this.creationDate;
           }
           set
           {
               this.creationDate = value;
           }
       }
   }
}
