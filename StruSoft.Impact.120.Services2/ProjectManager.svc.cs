namespace StruSoft.Impact.V120.Services
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Data;
    using DataTypes;
    using Settings;
    using DB;
    using DB.Query;
    using V120.ProjectManager.Core;
    using V120.ProjectManager.Core.ModelBuilder;
    using V120.ProjectManager.Core.ProjectBrowserData;
    using V120.ProjectManager.Core.TableGrid;

    /// <summary>
    /// The IMPACT Project Manager Service. Takes care of loading and updating all data.
    /// </summary>
    public partial class ProjectManager : IProjectManager
    {
        /// <summary>
        /// Initializes static members of the <see cref="ProjectManager" /> class. Sets default connection string.
        /// </summary>
        static ProjectManager()
        {
            var connectionStrings = ConfigurationManager.ConnectionStrings["ftConnString"];

            if( null != connectionStrings )
            {
                string connectionString = connectionStrings.ConnectionString;
                string providerName = connectionStrings.ProviderName;

                ImpactDatabase.SetDefaultConnection( connectionString, providerName );
            }
            else
            {
                ImpactSettings struSoftSettings = new ImpactSettings();

                if( struSoftSettings.Load() )
                {
                    if( !string.IsNullOrWhiteSpace( struSoftSettings.UserName ) && !string.IsNullOrWhiteSpace( struSoftSettings.Password ) )
                    {
                        string connectionString = string.Format( "UID={0};PWD={1};DSN={2};", struSoftSettings.UserName, struSoftSettings.Password, "IMPACT_12" );

                        ImpactDatabase.SetDefaultConnection( connectionString, "System.Data.Odbc" );
                    }
                    else
                    {
                        ImpactDatabase.SetDefaultConnection( "DSN=IMPACT_12", "System.Data.Odbc" );
                    }
                }
                else
                {
                    ImpactDatabase.SetDefaultConnection( "DSN=IMPACT_12", "System.Data.Odbc" );
                }
            }
        }

        #region IProjectManager Members

        /// <summary>
        /// Load Company, Factory and Project data for the specified user.
        /// </summary>
        /// <param name="user">
        /// The user name. This will be used to determine user rights.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="OpenProjectData"/> filled with Company, Factory and Project data.
        /// </returns>
        public CompanyDataCollection LoadCompanyFactoryProjectData( string user )
        {
            return OpenProjectLoader.Load( user );
        }

        /// <summary>
        /// Updates the current project data in the database.
        /// </summary>
        /// <param name="currentProject">
        /// The project data to be used in the update
        /// </param>
        public void UpdateCurrentProject( CurrentProject currentProject )
        {
            OpenProjectLoader.UpdateCurrentProject( currentProject );
        }

        /// <summary>
        /// Load data for the specified project, of the specified factory.
        /// </summary>
        /// <param name="factory">
        /// The factory to which the project belongs.
        /// </param>
        /// <param name="project">
        /// The project to load.
        /// </param>
        /// <returns>
        /// Returns a <see cref="ProjectRootData"/>.
        /// </returns>
        public ProjectRootData LoadProjectData( string factory, string project )
        {
            return ProjectBrowserLoader.Load( factory, project );
        }

        /// <summary>
        /// The load geometry data.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.ProjectManager.Core.ModelBuilder.ModelBuilderData.
        /// </returns>
        public ModelBuilderData LoadGeometryData( string factory, string project )
        {
            ModelBuilderLoader mb = new ModelBuilderLoader( factory, project );

            return mb.LoadGeometryData();
        }

        /// <summary>
        /// Returns a new erections sequence number for the specified project, of the specified factory.
        /// </summary>
        /// <param name="factory">
        /// The factory to which the project belongs.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The next available erection sequence number.
        /// </returns>
        public int GetNewErectionSequenceNumber( string factory, string project )
        {
            project = project.PadLeft( 12 );

            ImpactQuery query = new ImpactQuery
                                {
                Select = {
                            Aggregate.Max( ImpModelPlanning.ErectionSequenceNo ) 
                         }, 
                From = {
                          ImpModelPlanning.As( "T1" ) 
                       }, 
                Where =
                {
                    ImpModelPlanning.Factory.Equal( factory ), 
                    ImpModelPlanning.Project.Equal( project ), 
                }, 
            };

            string statement = query.ToString();

            using( var database = new ImpactDatabase() )
            {
                var erectionSequenceList = database.GetAll( statement, column => column[0].Cast<int>() );

                int maxEs = 0;

                if( erectionSequenceList.Count != 0 )
                {
                    maxEs = erectionSequenceList[0];
                }

                maxEs++;

                return maxEs;
            }
        }

        /// <summary>
        /// The update erection sequence.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <param name="erectionSequenceList">
        /// The erection sequence list.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public bool UpdateErectionSequence( string factory, string project, List<KeyValuePair<int, int>> erectionSequenceList )
        {
            project = project.PadLeft( 12 );

            List<string> statementList = new List<string>( erectionSequenceList.Count );

            using( var database = new ImpactDatabase() )
            {
                var allIdArray = erectionSequenceList.Select( x => (object)x.Key ).ToArray();
                ImpactQuery query = new ImpactQuery
                                    {
                    Select = {
                                ImpModelPlanning.ElementId 
                             }, 
                    From = {
                              ImpModelPlanning.As( "T1" ) 
                           }, 
                    Where =
                    {
                        ImpModelPlanning.Factory.Equal( factory ), 
                        ImpModelPlanning.Project.Equal( project ), 
                        ImpModelPlanning.ElementId.In( allIdArray ), 
                    }, 
                };

                string statement = query.ToString();
                var existingPlanningList = database.GetAll( statement, column => column[0].Cast<int>() );
                var groupedByInsertUpdate = erectionSequenceList.GroupBy( x => existingPlanningList.Remove( x.Key ) ).ToList();

                var updateList = groupedByInsertUpdate.Find( x => x.Key );

                var insertList = groupedByInsertUpdate.Find( x => !x.Key );

                if( null != updateList )
                {
                    foreach( var item in updateList )
                    {
                        var update = new ImpactUpdate( ImpModelPlanning.Instance )
                        {
                            Columns = {
                                         { ImpModelPlanning.ErectionSequenceNo, item.Value } 
                                      }, 
                            Where =
                            {
                                ImpModelPlanning.Factory.Equal( factory ), 
                                ImpModelPlanning.Project.Equal( project ), 
                                ImpModelPlanning.ElementId.Equal( item.Key ), 
                            }
                        };
                        statementList.Add( update.ToString() );
                    }
                }

                if( null != insertList )
                {
                    foreach( var item in insertList )
                    {
                        var insert = new ImpactInsert( ImpModelPlanning.Instance )
                        {
                            Columns =
                            {
                            { ImpModelPlanning.Factory, factory }, 
                            { ImpModelPlanning.Project, project }, 
                            { ImpModelPlanning.ElementId, item.Key }, 
                            { ImpModelPlanning.ErectionSequenceNo, item.Value }, 
                            }, 
                        };

                        statementList.Add( insert.ToString() );
                    }
                }

                int result = database.ExecuteNonQuery( statementList.ToArray() );

                return result > 0;
            }
        }

        /// <summary>
        /// The load user data.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.ProjectManager.Core.ProjectBrowserData.UserData.
        /// </returns>
        public UserData LoadUserData( string user )
        {
            user = user.ToLower();

            ImpactQuery query = new ImpactQuery( true )
            {
                From = {
                          ImpUser.As( "U" ) 
                       }, 
                Select = {
                            ImpUserGroup.Company, ImpUserGroupStd.RoleId 
                         }, 
                Join =
                {
                    Join.Inner( ImpUserGroup.As( "UG" ), ImpUser.Userid.Equal( ImpUserGroup.Userid ) ), 
                    Join.Inner( ImpUserGroupStd.As( "UGS" ), ImpUserGroup.UserGroup.Equal( ImpUserGroupStd.Name ) ), 
                }, 
                Where = {
                           ImpUser.Userid.Equal( user ) 
                        }, 
            };

            string statement = query.ToString();

            UserData userData = new UserData
                                {
                UserId = user
            };

            using( var database = new ImpactDatabase() )
            {
                var groupedCompanyRoleList = database.GetAll( 
                    statement, 
                    column => new
                    {
                        Company = column[0].Cast<string>(), 
                        Role = column[1].Cast<UserRole>(), 
                    }

                ).GroupBy( x => x.Company ).ToList();

                foreach( var companyGroup in groupedCompanyRoleList )
                {
                    UserRoleData urd = new UserRoleData( companyGroup.Key, from item in companyGroup select item.Role );

                    userData.UserRoles.Add( urd );
                }
            }

            return userData;
        }

        /// <summary>
        /// The load planning table data.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="idList">
        /// The id list.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.ProjectManager.Core.TableGrid.PlanningData.
        /// </returns>
        public PlanningData LoadPlanningTableData( string factory, string project, string user, int[] idList )
        {
            return ProjectBrowserLoader.LoadPlanningTableData( factory, project, user, idList );
        }

        /// <summary>
        /// The update planning element data.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <param name="planningElementList">
        /// The planning element list.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the database source is unknown.
        /// </exception>
        public void UpdatePlanningElementData( string factory, string project, List<PlanningElementDataUpdate> planningElementList )
        {
            project = Util.CorrectProjectName( project );
                      
            switch( ImpactDatabase.DataSource )
            {
                case DataSource.Ingres92:
                case DataSource.Ingres100:
                {
                    // Create a query that return all the id's of those elements that aldready has planning data.
                    ImpactQuery query = new ImpactQuery()
                        {
                            From = { ImpModelPlanning.As( "T1" ) },
                            Select = { ImpModelPlanning.ElementId },
                            Where =
                                    {
                                        ImpModelPlanning.Factory.Equal(factory),
                                        ImpModelPlanning.Project.Equal(project),
                                        ImpModelPlanning.ElementId.In<int>( planningElementList.Select( element => element.ElementId ) )
                                    }
                        };

                    using( var database = new ImpactDatabase() )
                    {
                        var existingIds = database.GetAll( query.ToString(), reader => reader[0].Cast<int>() );

                        var statementList = new List<string>( planningElementList.Count );

                        foreach( var element in planningElementList )
                        {
                            string producingFactory;

                            if( 0 == string.Compare( Factory.External.Number, element.ProducingFactory, StringComparison.OrdinalIgnoreCase ) )
                            {
                                producingFactory = ProjectBrowserLoader.ProducingFactoryExternalValue;
                            }
                            else
                            {
                                producingFactory = element.ProducingFactory;
                            }

                            if( existingIds.Contains( element.ElementId ) )
                            {
                                // We have an update.
                                var update = new ImpactUpdate( ImpModelPlanning.Instance )
                                {
                                    Where = 
                                        {
                                            ImpModelPlanning.Factory.Equal( factory ), 
                                            ImpModelPlanning.Project.Equal( project ), 
                                            ImpModelPlanning.ElementId.Equal( element.ElementId ), 
                                        },
                                    Columns =
                                        {
                                            { ImpModelPlanning.ProductionFactory, producingFactory ?? string.Empty }, 
                                            { ImpModelPlanning.DivisionProduction, element.Division ?? string.Empty }, 
                                            { ImpModelPlanning.ProductionDate, element.ProductionDate }, 
                                            { ImpModelPlanning.DeliveryDate, element.DeliveryDate }, 
                                            { ImpModelPlanning.ErectionSequenceNo, element.ErectionSequenceNo }, 
                                            { ImpModelPlanning.PlannedDrawingDate, element.PlannedDrawingDate }, 
                                            { ImpModelPlanning.PlannedProductionDate, element.PlannedProductionDate }, 
                                            { ImpModelPlanning.PlannedReadyForDeliveryDate, element.PlannedStorageDate }, 
                                            { ImpModelPlanning.PlannedDeliveryDate, element.PlannedDeliveryDate }, 
                                            { ImpModelPlanning.PlannedErectionDate, element.PlannedErectionDate }, 
                                            { ImpModelPlanning.ElementIdStatus, element.Status }, 
                                        }
                                };

                                statementList.Add( update.ToString() );
                            }
                            else
                            {
                                // We must insert a new row.
                                var insert = new ImpactInsert( ImpModelPlanning.Instance )
                                {
                                    Columns =
                                        {
                                            { ImpModelPlanning.Factory, factory }, 
                                            { ImpModelPlanning.Project, project }, 
                                            { ImpModelPlanning.ElementId, element.ElementId }, 
                                            { ImpModelPlanning.ProductionFactory, producingFactory ?? string.Empty }, 
                                            { ImpModelPlanning.DivisionProduction, element.Division ?? string.Empty }, 
                                            { ImpModelPlanning.ProductionDate, element.ProductionDate }, 
                                            { ImpModelPlanning.DeliveryDate, element.DeliveryDate }, 
                                            { ImpModelPlanning.ErectionSequenceNo, element.ErectionSequenceNo }, 
                                            { ImpModelPlanning.PlannedDrawingDate, element.PlannedDrawingDate }, 
                                            { ImpModelPlanning.PlannedProductionDate, element.PlannedProductionDate }, 
                                            { ImpModelPlanning.PlannedReadyForDeliveryDate, element.PlannedStorageDate }, 
                                            { ImpModelPlanning.PlannedDeliveryDate, element.PlannedDeliveryDate }, 
                                            { ImpModelPlanning.PlannedErectionDate, element.PlannedErectionDate }, 
                                            { ImpModelPlanning.ElementIdStatus, element.Status }, 
                                        }
                                };

                                statementList.Add( insert.ToString() );
                            }
                        }

                        database.ExecuteNonQuery( statementList.ToArray() );
                    }
                    break;
                }
                case DataSource.SqlServer:
                case DataSource.SqlServerExpress:
                {
                    List<string> statementList = new List<string>( planningElementList.Count );

                    foreach( var element in planningElementList )
                    {
                        string producingFactory;

                        if( 0 == string.Compare( Factory.External.Number, element.ProducingFactory, StringComparison.OrdinalIgnoreCase ) )
                        {
                            producingFactory = ProjectBrowserLoader.ProducingFactoryExternalValue;
                        }
                        else
                        {
                            producingFactory = element.ProducingFactory;
                        }

                        ImpactInsertOrUpdate insertOrUpdate = new ImpactInsertOrUpdate( ImpModelPlanning.Instance )
                        {
                            Keys =
                            {
                                { ImpModelPlanning.Factory, factory }, 
                                { ImpModelPlanning.Project, project }, 
                                { ImpModelPlanning.ElementId, element.ElementId }, 
                            }, 
                            Columns =
                            {
                                { ImpModelPlanning.ProductionFactory, producingFactory ?? string.Empty }, 
                                { ImpModelPlanning.DivisionProduction, element.Division ?? string.Empty }, 
                                { ImpModelPlanning.ProductionDate, element.ProductionDate }, 
                                { ImpModelPlanning.DeliveryDate, element.DeliveryDate }, 
                                { ImpModelPlanning.ErectionSequenceNo, element.ErectionSequenceNo }, 
                                { ImpModelPlanning.PlannedDrawingDate, element.PlannedDrawingDate }, 
                                { ImpModelPlanning.PlannedProductionDate, element.PlannedProductionDate }, 
                                { ImpModelPlanning.PlannedReadyForDeliveryDate, element.PlannedStorageDate }, 
                                { ImpModelPlanning.PlannedDeliveryDate, element.PlannedDeliveryDate }, 
                                { ImpModelPlanning.PlannedErectionDate, element.PlannedErectionDate }, 
                                { ImpModelPlanning.ElementIdStatus, element.Status }, 
                            }
                        };

                        statementList.Add( insertOrUpdate.ToString() );
                    }

                    using( var database = new ImpactDatabase() )
                    {
                        database.ExecuteNonQuery( statementList.ToArray() );
                    }

                    break;
                }

                default:
                {
                    throw new InvalidOperationException( "Unknown database source." );
                }
            }
        }

        #endregion
    }
}
