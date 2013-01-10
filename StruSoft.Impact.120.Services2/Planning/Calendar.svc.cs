using System.Collections.Generic;
using System.Text;
using StruSoft.Impact.V120.DB;
using StruSoft.Impact.V120.DB.Query;
using StruSoft.Impact.V120.Planning.Common;
using System.Data.Common;

namespace StruSoft.Impact.V120.Services
{
    using StruSoft.Impact.DataTypes;
    /// <summary>
    /// Used to modify records of type RecCalendar.
    /// </summary>
    public partial class ProjectManager : ICalendar
    {
        /// <summary>
        /// Load all records of the same factory and project as the supplied record.
        /// </summary>
        /// <param name="record">A record with factory and project set.</param>
        /// <returns>A list of all mathcing records.</returns>
        public List<RecCalendar> LoadCalendar(RecCalendar record)
        {
            ImpactQuery query = new ImpactQuery()
            {
                Select =
				{
					ImpCalendar.Factory,
					ImpCalendar.Project,
					ImpCalendar.Name,
					ImpCalendar.Description,
					ImpCalendar.CreatedBy,
					ImpCalendar.CreatedDate,
					ImpCalendar.ChangedBy,
					ImpCalendar.ChangedDate,

				},
                From = { ImpCalendar.As("T1") },
                Where = { ImpCalendar.Factory.Equal(record.Factory), ImpCalendar.Project.Equal(record.Factory) }
            };

            string statement = query.ToString();

            List<RecCalendar> result;

            using (ImpactDatabase database = new ImpactDatabase())
            {
                result = database.GetAll( statement, ParseCalendar );
            }

            if (null != result)
            {
                foreach (RecCalendar recCalendar in result)
                {
                    recCalendar.WorkDays = LoadWorkDays(recCalendar );
                    recCalendar.InvalidDays = LoadInvalidDays(recCalendar);

                }
            }
            return result;
        }

        public List<RecCalendarWorkday> LoadWorkDays(RecCalendar record)
        {
            ImpactQuery query = new ImpactQuery()
            {
                Select =
				{
                   ImpCalendarWorkday.Factory,
                   ImpCalendarWorkday.Project , 
                   ImpCalendarWorkday.Calendar, 
                   ImpCalendarWorkday.Weekday , 
                   ImpCalendarWorkday.DayType , 
                   ImpCalendarWorkday.ChangedBy , 
                   ImpCalendarWorkday.ChangedDate 

				},
                From = { ImpCalendarWorkday.As("T1") },
                Where = { 
                    ImpCalendarWorkday.Factory.Equal(record.Factory), 
                    ImpCalendarWorkday.Project.Equal(record.Project), 
                    ImpCalendarWorkday.Calendar.Equal(record.Name) 
                }
            };

            string statement = query.ToString();

            List<RecCalendarWorkday> result;

            using (ImpactDatabase database = new ImpactDatabase())
            {
                result = database.GetAll(statement, WorkDayParse);
            }
            return result;
        }

        public List<RecCalendarInvalidDays> LoadInvalidDays(RecCalendar record)
        {
            ImpactQuery query = new ImpactQuery()
            {
                Select =
				{
                    
                    ImpCalendarInvalidDays.Factory,
                    ImpCalendarInvalidDays.Project,
                    ImpCalendarInvalidDays.Calendar,
                    ImpCalendarInvalidDays.Day, 
                    ImpCalendarInvalidDays.DayType,
                    ImpCalendarInvalidDays.ChangedBy,
                    ImpCalendarInvalidDays.ChangedDate 
                 

				},
                From = { ImpCalendarInvalidDays.As("T1") },
                Where = { 
                    ImpCalendarInvalidDays.Factory.Equal(record.Factory), 
                    ImpCalendarInvalidDays.Project.Equal(record.Project), 
                    ImpCalendarInvalidDays.Calendar.Equal(record.Name) 
                }
            };

            string statement = query.ToString();

            List<RecCalendarInvalidDays> result;

            using (ImpactDatabase database = new ImpactDatabase())
            {
                result = database.GetAll(statement, InvalidDaysParse);
            }
            return result;
        }

        /// <summary>
        /// Parses one row in <see cref="System.Data.Common.DbDataReader"/> into
        /// a new instance of <see cref="StruSoft.Impact.V120.Common.Records.RecCalendarWorkday"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>A new instance of <see cref="StruSoft.Impact.V120.Common.Records.RecCalendarWorkday"/>.</returns>
        public static RecCalendarWorkday WorkDayParse(DbDataReader dataReader)
        {
            var record = new RecCalendarWorkday();
            record.Factory = DataConverter.Cast<string>(dataReader[0]);
            record.Project = DataConverter.Cast<string>(dataReader[1]);
            record.Calendar = DataConverter.Cast<string>(dataReader[2]);
            record.Weekday = DataConverter.Cast<int>(dataReader[3]);
            record.DayType = DataConverter.Cast<int>(dataReader[4]);
            record.ChangedBy = DataConverter.Cast<string>(dataReader[5]);
            record.ChangedDate = DataConverter.Cast<System.DateTime?>(dataReader[6]);
            return record;
        }



        /// <summary>
        /// Parses one row in <see cref="System.Data.Common.DbDataReader"/> into
        /// a new instance of <see cref="StruSoft.Impact.V120.Common.Records.RecCalendarInvalidDays"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>A new instance of <see cref="StruSoft.Impact.V120.Common.Records.RecCalendarInvalidDays"/>.</returns>
        public static RecCalendarInvalidDays InvalidDaysParse(DbDataReader dataReader)
        {
            var record = new RecCalendarInvalidDays();
            record.Factory = DataConverter.Cast<string>(dataReader[0]);
            record.Project = DataConverter.Cast<string>(dataReader[1]);
            record.Calendar = DataConverter.Cast<string>(dataReader[2]);
            record.Day = DataConverter.Cast<System.DateTime>(dataReader[3]);
            record.DayType = DataConverter.Cast<int>(dataReader[4]);
            record.ChangedBy = DataConverter.Cast<string>(dataReader[5]);
            record.ChangedDate = DataConverter.Cast<System.DateTime?>(dataReader[6]);
            return record;
        }
        /// <summary>
        /// Insert the specified record into the database.
        /// </summary>
        /// <param name="record">The record to insert into the database.</param>
        /// <returns>The number of affected records.</returns>
        public int InsertCalendar(RecCalendar record)
        {
            var insert = new ImpactInsert(ImpCalendar.Instance)
            {
                Columns = 
				{
					{ ImpCalendar.Factory, record.Factory },
					{ ImpCalendar.Project, record.Factory }, // Factory level!
					{ ImpCalendar.Name, record.Name },
					{ ImpCalendar.Description, record.Description },
					{ ImpCalendar.CreatedBy, record.CreatedBy },
					{ ImpCalendar.CreatedDate, record.CreatedDate },
				}
            };

            string statement = insert.ToString();

            int result;

            using (ImpactDatabase database = new ImpactDatabase())
            {
                result = database.ExecuteNonQuery(statement);
            }

            return result;
        }


        /// <summary>
        /// Parses one row in <see cref="System.Data.Common.DbDataReader"/> into
        /// a new instance of <see cref="StruSoft.Impact.V120.Common.Records.RecCalendar"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>A new instance of <see cref="StruSoft.Impact.V120.Common.Records.RecCalendar"/>.</returns>
        public static RecCalendar ParseCalendar(DbDataReader dataReader)
        {
            var record = new RecCalendar();
            record.Factory = DataConverter.Cast<string>(dataReader[0]);
            record.Project = DataConverter.Cast<string>(dataReader[1]);
            record.Name = DataConverter.Cast<string>(dataReader[2]);
            record.Description = DataConverter.Cast<string>(dataReader[3]);
            record.CreatedBy = DataConverter.Cast<string>(dataReader[4]);
            record.CreatedDate = DataConverter.Cast<System.DateTime?>(dataReader[5]);
            record.ChangedBy = DataConverter.Cast<string>(dataReader[6]);
            record.ChangedDate = DataConverter.Cast<System.DateTime?>(dataReader[7]);
            return record;
        }


        /// <summary>
        /// Delete the specified record from the database.
        /// </summary>
        /// <param name="record">The record to delete from the database.</param>
        /// <returns>The number of affected records.</returns>
        public int DeleteCalendar(RecCalendar record)
        {
            var delete = new ImpactDelete(ImpCalendar.Instance)
            {
                Where = 
				{
					{ ImpCalendar.Factory.Equal( record.Factory )},
					{ ImpCalendar.Project.Equal( record.Project )},
                    { ImpCalendar.Name.Equal( record.Name )},
				}
            };

            string statement = delete.ToString();

            int result;

            using (ImpactDatabase database = new ImpactDatabase())
            {
                result = database.ExecuteNonQuery(statement);
            }

            return result;
        }
        /// <summary>
        /// Delete the specified list of records from the database.
        /// </summary>
        /// <param name="list">The list of records to delete from the database.</param>
        /// <returns>The number of affected  records.</returns>
        public int BulkDeleteCalendar(List<RecCalendar> list)
        {
            int result = 0;

            foreach (var record in list)
            {
                result += this.DeleteCalendar(record);
            }

            return result;
        }
        /// <summary>
        /// Update the specified record in the database.
        /// </summary>
        /// <param name="record">The record to update.</param>
        /// <returns></returns>
        public int UpdateCalendar(RecCalendar record)
        {

            var update = new ImpactUpdate(ImpCalendar.Instance)
            {
                Columns = 
				{
					{ ImpCalendar.Description, record.Description },
					{ ImpCalendar.ChangedBy, record.ChangedBy },
					{ ImpCalendar.ChangedDate, record.ChangedDate },
				},
                Where = 
				{
					{ ImpCalendar.Factory.Equal( record.Factory ) },
					{ ImpCalendar.Project.Equal( record.Factory ) },// Factory level!
                    { ImpCalendar.Name.Equal( record.Name ) },
				},
            };

            string statement = update.ToString();

            int result;

            using (ImpactDatabase database = new ImpactDatabase())
            {
                result = database.ExecuteNonQuery(statement);
            }

            return result;
        }

        public int BulkUpdateCalendar(List<RecCalendar> list)
        {
            int result = 0;

            foreach (var record in list)
            {
                result += this.UpdateCalendar(record);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inserted"></param>
        /// <param name="updated"></param>
        /// <returns></returns>
        public int BulkSaveCalendar( List<RecCalendar> inserted, List<RecCalendar> updated )
        {
            var result = 0;
            if( null != inserted && inserted.Count > 0 )
            {
                foreach( var recCalendar in inserted )
                {
                    result += this.InsertCalendar( recCalendar );
                }
            }
            if (null != updated && updated.Count > 0)
            {
                result += this.BulkUpdateCalendar( updated );
            }            

            return result;
        }
    }

}
