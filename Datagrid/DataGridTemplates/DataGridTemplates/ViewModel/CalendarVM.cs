namespace DataGridTemplates.ViewModel
{
    using System.Linq;
    using System.Collections.ObjectModel;

    using DataGridTemplates.Model;

    public class CalendarVM
    {

        public ObservableCollection<ImpCalendarVM> CalendarList { get; set; }
        public CalendarVM()
        {
            Impact_1200_Latin1Entities s = new Impact_1200_Latin1Entities();
            var list = s.IMP_CALENDAR.Select(calendar => new ImpCalendarVM()
                                                             {
                                                                 ChangedBy = calendar.CHANGED_BY,
                                                                 ChangedDate = calendar.CHANGED_DATE,
                                                                 Description = calendar.DESCRIPTION,
                                                                 Factory = calendar.FACTORY,
                                                                 Name = calendar.NAME,
                                                                 Project = calendar.PROJECT,
                                                                 CreatedBy = calendar.CREATED_BY,
                                                                 CreatedDate = calendar.CREATED_DATE,
                                                                 IsSelected = false
                                                             }).ToList();
           
            this.CalendarList = new ObservableCollection<ImpCalendarVM>(list);
        }
    }   

}
