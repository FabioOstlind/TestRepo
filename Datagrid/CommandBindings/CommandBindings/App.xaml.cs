using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CommandBindings
{
    using System.Collections.ObjectModel;

    using CommandBindings.ViewModel;
    using CommandBindings.Model;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                List<InfoModel> models = new List<InfoModel>
                                         {
                                             new InfoModel
                                             {
                                                 Filename = "hejsan.txt",
                                                 CreationDate = DateTime.Now
                                             },
                                               new InfoModel
                                             {
                                                 Filename = "svejsan.txt",
                                                 CreationDate = DateTime.Now
                                             }
                                         };

                var infoModelsVMList = models.Select(InfoModelVM => new InfoModelVM(InfoModelVM));

                ObservableCollection<InfoModelVM> collection = new ObservableCollection<InfoModelVM>( infoModelsVMList );
                SaveInfoVM saveInfoVM = new SaveInfoVM(collection );
                MainWindow window = new MainWindow
                                    {
                                        DataContext = saveInfoVM
                                    };
                window.ShowDialog();
            }
            catch( InvalidOperationException ex )
            {
                MessageBox.Show( ex.Message );
                throw;
            }
        }
    }
}
