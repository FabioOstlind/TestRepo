namespace MysticFlavour.CommandTest
{
    using System;
    using System.Windows;

    using MysticFlavour.CommandTest.Models;
    using MysticFlavour.CommandTest.ViewModels;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Methods

        /// <summary>
        /// The on startup.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnStartup( StartupEventArgs e )
        {
            try
            {
                base.OnStartup( e );

                var data = new TestData
                               {
                                   Name = "Test", 
                                   Id = 123
                               };

                var window = new MainWindow
                {
                    DataContext = new TestVM( data )
                };

                window.ShowDialog();

            }
            catch( Exception exception )
            {
                MessageBox.Show( exception.Message );
            }
        }

        #endregion
    }
}
