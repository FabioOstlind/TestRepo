using System.Collections.Generic;
using StruSoft.Impact.V120.Report.Common;
using StruSoft.Impact.V120.DB.Query;
using StruSoft.Impact.V120.DB;
using System.Data.Common;

namespace StruSoft.Impact.V120.Services
{
    using System;

    using StruSoft.Impact.V120.Services.Report;

    /// <summary>
    /// Loads report standard data and layout info
    /// </summary>
    public partial class ProjectManager : IReportManager
    {
        /// <summary>
        /// Returns report standard data and layout info
        /// </summary>
        /// <param name="userId"> </param>
        /// <param name="record">RecReportStd</param>
        /// <param name="factory"> </param>
        /// <param name="project"> </param>
        /// <returns>ReportTemplateData</returns>
        public ReportTemplateData LoadReportLayout( string factory, string project, string userId, RecReportStd record )
        {
            RecProject recProject = null;
            RecReportStd report = null;
            List<RecReportLayoutStd> layouts = null;
            var reports = LoadReport( record );
            if( null != reports && reports.Count > 0 )
            {
                report = reports[0];
                layouts = LoadReportDetails( report );

                var proj = new RecProject { Factory = factory, Project = project };

                var projs = LoadProject( proj );
                if( null != projs && projs.Count > 0 )
                {
                    recProject = projs[0];
                }
            }

            var folders = LoadFolder( new RecFolder() { Factory = factory, Project = project } );
            var recUser = LoadUser( userId );
            return new ReportTemplateData( recUser, recProject, report, layouts, folders );
        }

        /// <summary>
        /// Return report standard data
        /// </summary>
        /// <param name="record">RecReportStd</param>
        /// <returns></returns>
        public List<RecReportStd> LoadReport( RecReportStd record )
        {
            var company = Util.FactoryToCompany( record.Factory );
            var query = new ImpactQuery()
            {
                Select =
				{
					ImpReportStd.Factory,
					ImpReportStd.Project,
					ImpReportStd.ReportId,
					ImpReportStd.Name,
					ImpReportStd.ReportType,
					ImpReportStd.Filename,
					ImpReportStd.Foldername,
					ImpReportStd.PageMaxX,
					ImpReportStd.PageMaxY,
					ImpReportStd.ContinuousHeaderRow,
					ImpReportStd.ContinuousStartRow,
					ImpReportStd.ContinuousEndRow,
					ImpReportStd.ContinuousSumRow,

				},
                From = { ImpReportStd.As( "T1" ) },
                Where = { 
                            ImpReportStd.Factory.Equal( company ), 
                            ImpReportStd.Project.Equal( company ), // company, company 
                        }
            };

            if( !string.IsNullOrWhiteSpace( record.Name ) )
            {
                query.Where.Add( ImpReportStd.Name.Equal( record.Name ) );
            }

            if( record.ReportType > 0 )
            {
                query.Where.Add( ImpReportStd.ReportType.Equal( (int)record.ReportType ) );
            }

            var statement = query.ToString();

            List<RecReportStd> result;

            using( var database = new ImpactDatabase() )
            {
                result = database.GetAll( statement, ParseReport );
            }

            return result;
        }

        /// <summary>
        /// Parses one row in <see cref="System.Data.Common.DbDataReader"/> into
        /// a new instance of <see cref="Paths.Common.Records.RecReportStd"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>A new instance of <see cref="Paths.Common.Records.RecReportStd"/>.</returns>
        public static RecReportStd ParseReport( DbDataReader dataReader )
        {
            var record = new RecReportStd
                             {
                                 Factory = dataReader[0].Cast<string>(),
                                 Project = dataReader[1].Cast<string>(),
                                 ReportId = dataReader[2].Cast<int>(),
                                 Name = dataReader[3].Cast<string>(),
                                 ReportType = dataReader[4].Cast<int>(),
                                 Filename = dataReader[5].Cast<string>(),
                                 Foldername = dataReader[6].Cast<string>(),
                                 PageMaxX = dataReader[7].Cast<int>(),
                                 PageMaxY = dataReader[8].Cast<int>(),
                                 ContinuousHeaderRow = dataReader[9].Cast<int>(),
                                 ContinuousStartRow = dataReader[10].Cast<int>(),
                                 ContinuousEndRow = dataReader[11].Cast<int>(),
                                 ContinuousSumRow = dataReader[12].Cast<int>()
                             };
            return record;
        }

        /// <summary>
        /// Loads report layout
        /// </summary>
        /// <param name="record">RecReportStd</param>
        /// <returns>List<RecReportLayoutStd></returns>
        public List<RecReportLayoutStd> LoadReportDetails( RecReportStd record )
        {
            string company = Util.FactoryToCompany( record.Factory );
            ImpactQuery query = new ImpactQuery()
            {
                Select =
				{
					ImpReportLayoutStd.Factory,
					ImpReportLayoutStd.Project,
					ImpReportLayoutStd.ReportId,
					ImpReportLayoutStd.FieldName,
					ImpReportLayoutStd.CellX,
					ImpReportLayoutStd.CellY,
					ImpReportLayoutStd.FieldType,

				},
                From = { ImpReportLayoutStd.As( "T1" ) },
                Where = { 
                            ImpReportLayoutStd.Factory.Equal( company ), 
                            ImpReportLayoutStd.Project.Equal( company ),
                            ImpReportLayoutStd.ReportId.Equal( record.ReportId )
                        },
                OrderBy = { 
                            ImpReportLayoutStd.FieldType, 
                            ImpReportLayoutStd.CellX 
                          },
            };

            string statement = query.ToString();

            List<RecReportLayoutStd> result;

            using( ImpactDatabase database = new ImpactDatabase() )
            {
                result = database.GetAll( statement, Parselayout );
            }

            return result;
        }

        /// <summary>
        /// Parses one row in <see cref="System.Data.Common.DbDataReader"/> into
        /// a new instance of <see cref="Paths.Common.Records.RecReportLayoutStd"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>A new instance of <see cref="Paths.Common.Records.RecReportLayoutStd"/>.</returns>
        public static RecReportLayoutStd Parselayout( DbDataReader dataReader )
        {
            var record = new RecReportLayoutStd
                             {
                                 Factory = dataReader[0].Cast<string>(),
                                 Project = dataReader[1].Cast<string>(),
                                 ReportId = dataReader[2].Cast<int>(),
                                 FieldName = dataReader[3].Cast<string>(),
                                 CellX = dataReader[4].Cast<int>(),
                                 CellY = dataReader[5].Cast<int>(),
                                 FieldType = dataReader[6].Cast<int>()
                             };
            return record;
        }

        /// <summary>
        /// Load all records of the same factory and project as the supplied record.
        /// </summary>
        /// <param name="record">A record with factory and project set.</param>
        /// <returns>A list of all mathcing records.</returns>
        public List<RecProject> LoadProject( RecProject record )
        {
            var query = new ImpactQuery()
            {
                Select =
				{
					ImpProject.Factory,
					ImpProject.Project,
					ImpProject.ProjectType,
					ImpProject.Status,
					ImpProject.Name,
					ImpProject.Description,
					ImpProject.Text3,
					ImpProject.Text4,
					ImpProject.Text5,
					ImpProject.Text6,
					ImpProject.CheckedBy,
					ImpProject.RegisterDate,
					ImpProject.RevisionDate,
					ImpProject.ErectionDate,
					ImpProject.BuildingSiteName,
					ImpProject.BuildingSiteStreet,
					ImpProject.BuildingSiteZipcode,
					ImpProject.BuildingSiteCity,
					ImpProject.BuildingOwnerName,
					ImpProject.BuildingOwnerStreet,
					ImpProject.BuildingOwnerZipcode,
					ImpProject.BuildingOwnerCity,
					ImpProject.FolderPath,
					ImpProject.CreatedBy,
					ImpProject.ChangedBy,

				},
                From = { ImpProject.As( "T1" ) },
                Where = { ImpProject.Factory.Equal( record.Factory ), ImpProject.Project.Equal( record.Project ) }
            };

            var statement = query.ToString();

            List<RecProject> result;

            using( var database = new ImpactDatabase() )
            {
                result = database.GetAll( statement, ParseProject );
            }

            return result;
        }

        /// <summary>
        /// Parses one row in <see cref="System.Data.Common.DbDataReader"/> into
        /// a new instance of <see cref="RecProject"/>.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public static RecProject ParseProject( DbDataReader dataReader )
        {
            var record = new RecProject
                             {
                                 Factory = dataReader[0].Cast<string>(),
                                 Project = dataReader[1].Cast<string>(),
                                 ProjectType = dataReader[2].Cast<int>(),
                                 Status = dataReader[3].Cast<int>(),
                                 Name = dataReader[4].Cast<string>(),
                                 Description = dataReader[5].Cast<string>(),
                                 Text3 = dataReader[6].Cast<string>(),
                                 Text4 = dataReader[7].Cast<string>(),
                                 Text5 = dataReader[8].Cast<string>(),
                                 Text6 = dataReader[9].Cast<string>(),
                                 CheckedBy = dataReader[10].Cast<string>(),
                                 RegisterDate = dataReader[11].Cast<DateTime?>(),
                                 RevisionDate = dataReader[12].Cast<DateTime?>(),
                                 ErectionDate = dataReader[13].Cast<DateTime?>(),
                                 BuildingSiteName = dataReader[14].Cast<string>(),
                                 BuildingSiteStreet = dataReader[15].Cast<string>(),
                                 BuildingSiteZipcode = dataReader[16].Cast<string>(),
                                 BuildingSiteCity = dataReader[17].Cast<string>(),
                                 BuildingOwnerName = dataReader[18].Cast<string>(),
                                 BuildingOwnerStreet = dataReader[19].Cast<string>(),
                                 BuildingOwnerZipcode = dataReader[20].Cast<string>(),
                                 BuildingOwnerCity = dataReader[21].Cast<string>(),
                                 FolderPath = dataReader[22].Cast<string>(),
                                 CreatedBy = dataReader[23].Cast<string>(),
                                 ChangedBy = dataReader[24].Cast<string>()
                             };
            return record;
        }

        /// <summary>
        /// Load all records of the same factory and project as the supplied record.
        /// </summary>
        /// <param name="record">A record with factory and project set.</param>
        /// <returns>A list of all mathcing records.</returns>
        public List<RecFolder> LoadFolder( RecFolder record )
        {
            ImpactQuery query = new ImpactQuery()
            {
                Select =
				{
					ImpFolder.Factory,
					ImpFolder.Project,
					ImpFolder.ConnectionId,
					ImpFolder.FolderPath,

				},
                From = { ImpFolder.As( "T1" ) },
                Where = { ImpFolder.Factory.Equal( record.Factory ), 
                          ImpFolder.Project.Equal( record.Project ) }
            };

            string statement = query.ToString();

            List<RecFolder> result;

            using( ImpactDatabase database = new ImpactDatabase() )
            {
                result = database.GetAll( statement, ParseFolder );
            }

            return result;
        }

        /// <summary>
        /// Parses one row in <see cref="System.Data.Common.DbDataReader"/> into
        /// a new instance of <see cref="Paths.Common.Records.RecFolder"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>A new instance of <see cref="Paths.Common.Records.RecFolder"/>.</returns>
        public static RecFolder ParseFolder( DbDataReader dataReader )
        {
            var record = new RecFolder
                             {
                                 Factory = dataReader[0].Cast<string>(),
                                 Project = dataReader[1].Cast<string>(),
                                 ConnectionId = dataReader[2].Cast<int>(),
                                 FolderPath = dataReader[3].Cast<string>()
                             };
            return record;
        }

        /// <summary>
        /// Load all records of the same factory and project as the supplied record.
        /// </summary>
        /// <param name="userId"> </param>
        /// <returns>A list of all mathcing records.</returns>
        public RecUser LoadUser( string userId )
		{
			var query = new ImpactQuery()
			{
				Select =
				{
					ImpUser.Userid,
					ImpUser.CurrentFactory,
					ImpUser.CurrentProject,
					ImpUser.Signature,
					ImpUser.FirstName,
					ImpUser.LastName,
					ImpUser.Title,
					ImpUser.Email,
					ImpUser.Phone,
					ImpUser.Mobile,
					ImpUser.LockedDate,
					ImpUser.LockStatus,
					ImpUser.SysAdmin,

				},
				From  = { ImpUser.As( "T1" ) },
                Where =
                {
                    ImpUser.Userid.Equal( userId ), 
                }
				
			};

			var statement = query.ToString();

			List<RecUser> result;

			using( var database = new ImpactDatabase() )
			{
				result = database.GetAll( statement, ParseUser );
			}

            if( null != result && result.Count > 0 )
            {
                return result[0];
            }

			return null;
		}

		/// <summary>
		/// Parses one row in <see cref="System.Data.Common.DbDataReader"/> into
		/// a new instance of <see cref="Paths.Common.Records.RecUser"/>.
		/// </summary>
		/// <param name="dataReader">The data reader.</param>
		/// <returns>A new instance of <see cref="Paths.Common.Records.RecUser"/>.</returns>
		public static RecUser ParseUser( DbDataReader dataReader )
		{
			var record = new RecUser
			                 {
			                     Userid = dataReader[0].Cast<string>(),
			                     CurrentFactory = dataReader[1].Cast<string>(),
			                     CurrentProject = dataReader[2].Cast<string>(),
			                     Signature = dataReader[3].Cast<string>(),
			                     FirstName = dataReader[4].Cast<string>(),
			                     LastName = dataReader[5].Cast<string>(),
			                     Title = dataReader[6].Cast<string>(),
			                     Email = dataReader[7].Cast<string>(),
			                     Phone = dataReader[8].Cast<string>(),
			                     Mobile = dataReader[9].Cast<string>(),
			                     LockedDate = dataReader[10].Cast<DateTime?>(),
			                     LockStatus = dataReader[11].Cast<int>(),
			                     SysAdmin = dataReader[12].Cast<int>()
			                 };
		    return record;
		}

        /// <summary>
        /// Load Products
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public List<RecProductStd> LoadProduct( string factory, string project )
        {
            var query = new ImpactQuery( true )
            {
                Select =
				{
					ImpElement.Product,
				},
                From = { ImpElement.As( "T1" ) },
                Where = { ImpElement.Factory.Equal( factory ), ImpElement.Project.Equal( project ) }
            };

            var statement = query.ToString();

            List<RecProductStd> result;

            using( var database = new ImpactDatabase() )
            {
                result = database.GetAll( statement, column => new RecProductStd{ Product = column[0].Cast<string>() } );
            }

            return result;
        }

        /// <summary>
        /// LoadPhase
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public List<RecModelPhase> LoadPhase( string factory, string project )
        {
            var query = new ImpactQuery( true )
            {
                Select =
				{
					ImpModelPhase.Name,
				},
                From = { ImpModelPhase.As( "T1" ) },
                Where = { ImpModelPhase.Factory.Equal( factory ), ImpModelPhase.Project.Equal( project ) }
            };

            var statement = query.ToString();

            List<RecModelPhase> result;

            using( var database = new ImpactDatabase() )
            {
                result = database.GetAll( statement, column => new RecModelPhase{ Name = column[0].Cast<string>(), } );
            }

            return result;
        }

        /// <summary>
        /// LoadBuilding
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public List<RecModelFloor> LoadBuilding( string factory, string project )
        {
            var query = new ImpactQuery() // true means Distinct
            {
                Select =
				{
					ImpModelFloor.Building,
				},
                From = { ImpModelFloor.As( "T1" ) },
                Where = { ImpModelFloor.Factory.Equal( factory ), ImpModelFloor.Project.Equal( project ) }
            };

            var statement = query.ToString();

            List<RecModelFloor> result;

            using( var database = new ImpactDatabase() )
            {
                result = database.GetAll( statement, column => new RecModelFloor { Building = column[0].Cast<string>() } );
            }

            return result;
        }

        /// <summary>
        /// LoadBuilding
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public List<RecModelFloor> LoadFloor( string factory, string project )
        {
            var query = new ImpactQuery( true )
            {
                Select =
				{
                    ImpModelFloor.Factory,
                    ImpModelFloor.Project,
                    ImpModelFloor.Building,
					ImpModelFloor.FloorId,
                    ImpModelFloor.FloorLevel,
                    ImpModelFloor.FloorName,
				},
                From = { ImpModelFloor.As( "T1" ) },
                Where = { ImpModelFloor.Factory.Equal( factory ), ImpModelFloor.Project.Equal( project ) }
            };

            var statement = query.ToString();

            List<RecModelFloor> result;

            using( var database = new ImpactDatabase() )
            {
                result = database.GetAll( statement, column => new RecModelFloor
                            {
                                Factory = column[0].Cast<string>(),
                                Project = column[1].Cast<string>(),
                                Building = column[2].Cast<string>(),
                                FloorId = column[3].Cast<int>(),
                                FloorLevel = column[4].Cast<string>(),
                                FloorName = column[5].Cast<string>(),
                            } );
            }

            return result;
        }

        /// <summary>
        /// LoadDrawingStatusStd
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public List<RecDrawingStatusStd> LoadDrawingStatusStd( string factory, string project )
        {
            var query = new ImpactQuery( true )
            {
                Select =
				{
                    ImpDrawingStatusStd.Factory,
                    ImpDrawingStatusStd.Project,
                    ImpDrawingStatusStd.Name,
                    ImpDrawingStatusStd.Description,
					ImpDrawingStatusStd.StatusType,
				},
                From = { ImpDrawingStatusStd.As( "T1" ) },
                Where = { ImpDrawingStatusStd.Factory.Equal( factory ), ImpDrawingStatusStd.Project.Equal( project ) }
            };

            var statement = query.ToString();

            List<RecDrawingStatusStd> result;

            using( var database = new ImpactDatabase() )
            {
                result = database.GetAll( statement, column => new RecDrawingStatusStd 
                { 
                    Factory = column[0].Cast<string>(),
                    Project = column[1].Cast<string>(),
                    Name = column[2].Cast<string>(),
                    Description = column[3].Cast<string>(),
                    StatusType = column[4].Cast<int>(),
                } );
            }

            return result;
        }

        /// <summary>
        /// LoadReport Filter Data
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public ReportFilterData LoadReportFilterData( string factory, string project )
        {
            var reportFilterData = new ReportFilterData
                {
                    StandardReports = this.LoadReport( new RecReportStd { Factory = factory, Project = project } ),
                    Products = this.LoadProduct( factory, project ),
                    Phases = this.LoadPhase( factory, project ),
                    Floors = this.LoadFloor( factory, project ),
                    DrawingStatus = this.LoadDrawingStatusStd( factory, project )
                };

            return reportFilterData;
        }

        //-//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Old Reports
        
        /// <summary>
        /// LoadDrawingReportData
        /// </summary>
        /// <param name="reportFilter"></param>
        /// <returns></returns>
        public DrawingReportData LoadDrawingReportData( ReportFilter reportFilter )
        {
            return new DrawingReport().Load( reportFilter );
        }
    }
}
