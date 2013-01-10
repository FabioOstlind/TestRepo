namespace CommandBindings.ViewModel
{
    using System;

    using CommandBindings.Model;
    using System.ComponentModel;

    internal class InfoModelVM : INotifyPropertyChanged
    {
        private readonly InfoModel infoModel;
        private string filename;

        public string Filename
        {
            get
            {
                return this.infoModel.Filename;
            }
            set
            {
                bool isSame = object.Equals( this.infoModel.Filename, value );
                bool isNullorEmpty = String.IsNullOrEmpty( value );
                if( isSame || isNullorEmpty )
                {
                    return;
                }
                this.infoModel.Filename = value;
                this.OnPropertyChanged();
            }
        }

        private DateTime creationDate;

        public DateTime CreationDate
        {
            get
            {
                return this.infoModel.CreationDate;
            }
            set
            {
               
                //var isSameDate = this.infoModel.CreationDate.Equals( value );
                //var isBeforeToDay = this.infoModel.CreationDate.CompareTo( value ) == -1;
                //if( isSameDate || isBeforeToDay )
                //{
                //    return;
                //}
                this.infoModel.CreationDate = value;
                this.OnPropertyChanged();
            }
        }
 

        public event PropertyChangedEventHandler PropertyChanged;


        #region Ctr
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public InfoModelVM()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public InfoModelVM( InfoModel infoModel )
        {
            this.infoModel = infoModel;
        }

        #endregion

        protected virtual void OnPropertyChanged( string propertyName = null )
        {
            var handler = this.PropertyChanged;
            if( handler != null )
            {
                handler( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }
}
}
