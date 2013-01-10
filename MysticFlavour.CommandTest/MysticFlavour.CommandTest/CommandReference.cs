namespace MysticFlavour.CommandTest
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// The command reference.
    /// </summary>
    public class CommandReference : Freezable, ICommand
    {
        #region Static Fields

        /// <summary>
        /// The command property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register( "Command", typeof( ICommand ), typeof( CommandReference ), new PropertyMetadata( new PropertyChangedCallback( OnCommandChanged ) ) );

        #endregion

        #region Public Events

        /// <summary>
        /// The can execute changed.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        public ICommand Command
        {
            get
            {
                return (ICommand)this.GetValue( CommandProperty );
            }

            set
            {
                this.SetValue( CommandProperty, value );
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The can execute.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool CanExecute( object parameter )
        {
            if( this.Command != null )
            {
                return this.Command.CanExecute( parameter );
            }

            return false;
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        public void Execute( object parameter )
        {
            this.Command.Execute( parameter );
        }

        #endregion

        #region Methods

        /// <summary>
        /// The create instance core.
        /// </summary>
        /// <returns>
        /// The <see cref="Freezable"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// Thrown because the method is not implemented.
        /// </exception>
        protected override Freezable CreateInstanceCore()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The on command changed.
        /// </summary>
        /// <param name="d">
        /// The d.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void OnCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            CommandReference commandReference = d as CommandReference;
            ICommand oldCommand = e.OldValue as ICommand;
            ICommand newCommand = e.NewValue as ICommand;

            if( oldCommand != null )
            {
                oldCommand.CanExecuteChanged -= commandReference.CanExecuteChanged;
            }

            if( newCommand != null )
            {
                newCommand.CanExecuteChanged += commandReference.CanExecuteChanged;
            }
        }

        #endregion
    }
}
