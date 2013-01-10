namespace MysticFlavour.CommandTest
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// The custom command.
    /// </summary>
    public class CustomCommand : ICommand
    {
        #region Fields

        /// <summary>
        /// The can execute.
        /// </summary>
        private readonly Func<object, bool> canExecute;

        /// <summary>
        /// The execute.
        /// </summary>
        private readonly Action<object> execute;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomCommand"/> class,
        /// with command parameters.
        /// </summary>
        /// <param name="canExecute">
        /// The can execute.
        /// </param>
        /// <param name="execute">
        /// The execute.
        /// </param>
        public CustomCommand( Func<object, bool> canExecute, Action<object> execute )
        {
            this.canExecute = canExecute;
            this.execute = execute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomCommand"/> class,
        /// with a command that always can be executed and has command parameter.
        /// </summary>
        /// <param name="execute">
        /// The execute.
        /// </param>
        public CustomCommand( Action<object> execute )
            : this( null, execute )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomCommand"/> class,
        /// with a command that always can be executed and has no command parameters.
        /// </summary>
        /// <param name="execute">
        /// The execute.
        /// </param>
        public CustomCommand( Action execute )
            : this( null, execute )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomCommand"/> class,
        /// with no command parameters.
        /// </summary>
        /// <param name="canExecute">
        /// The can execute.
        /// </param>
        /// <param name="execute">
        /// The execute.
        /// </param>
        public CustomCommand( Func<bool> canExecute, Action execute )
        {
            this.canExecute = o => canExecute();
            this.execute = o => execute();
        }

        #endregion

        #region Public Events

        /// <summary>
        /// The can execute changed.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">
        /// Data used by the command.  If the command does not require data to be passed, this object can be set to null.
        /// </param>
        public bool CanExecute( object parameter )
        {
            return null == this.canExecute || this.canExecute( parameter );
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command.  If the command does not require data to be passed, this object can be set to null.
        /// </param>
        public void Execute( object parameter )
        {
            this.execute( parameter );
        }

        #endregion
    }
}
