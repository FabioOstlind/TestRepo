namespace MysticFlavour.CommandTest.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    using MysticFlavour.CommandTest.Annotations;
    using MysticFlavour.CommandTest.Models;

    /// <summary>
    /// The test vm.
    /// </summary>
    internal class TestVM : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// The model.
        /// </summary>
        private readonly TestData model;

        /// <summary>
        /// The reverse name command.
        /// </summary>
        private readonly ICommand reverseName;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestVM"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public TestVM( TestData model )
            : this()
        {
            this.model = model;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="TestVM"/> class from being created.
        /// </summary>
        private TestVM()
        {
            this.reverseName = new CustomCommand( this.ReverseNameCanExectute, this.ReverseNameExecuted );
        }

        #endregion

        #region Public Events

        /// <summary>
        /// The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int Id
        {
            get
            {
                return this.model.Id;
            }

            set
            {
                if( object.Equals( value, this.model.Id ) )
                {
                    return;
                }

                this.model.Id = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get
            {
                return this.model.Name;
            }

            set
            {
                if( object.Equals( value, this.model.Name ) )
                {
                    return;
                }

                this.model.Name = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the reverse name command.
        /// </summary>
        public ICommand ReverseName
        {
            get
            {
                return this.reverseName;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            var handler = this.PropertyChanged;

            if( handler != null )
            {
                handler( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        /// <summary>
        /// The reverse name can exectute.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool ReverseNameCanExectute()
        {
            return !string.IsNullOrWhiteSpace( this.Name );
        }

        /// <summary>
        /// The reverse name.
        /// </summary>
        private void ReverseNameExecuted()
        {
            this.Name = new string( this.Name.Reverse().ToArray() );
        }

        #endregion
    }
}
