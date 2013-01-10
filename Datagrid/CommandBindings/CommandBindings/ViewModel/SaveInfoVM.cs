using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandBindings.ViewModel
{
    using System.Collections.ObjectModel;

    class SaveInfoVM
    {
        private readonly ObservableCollection<InfoModelVM> infoModelVms;
        public ObservableCollection<InfoModelVM> InfoModelVms
        {
            get
            {
                return this.infoModelVms;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SaveInfoVM(ObservableCollection<InfoModelVM> infoModelVms)
        {
            this.infoModelVms = infoModelVms;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SaveInfoVM()
        {
        }
    }
}
