namespace StruSoft.Impact.V120.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DB;
    using DB.Query;

    using StruSoft.Impact.Data;
    using StruSoft.Impact.DataTypes;

    /// <summary>
    /// The open project loader.
    /// </summary>
    internal static class OpenProjectLoader
    {
        /// <summary>
        /// Get the query for fetching company, factory and project data available for the specified user.
        /// </summary>
        /// <param name="user">
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        static string GetCompanyFactoryProjectQuery( string user )
        {
            ImpactQuery query = new ImpactQuery( true )
            {
                Select = 
				{ 
					ImpCompany.Company, 
					ImpCompany.Name, 
					ImpFactory.Factory, 
					ImpFactory.Name, 
					ImpProject.Project, 
					ImpProject.Name, 
					ImpProject.Description, 
					ImpProject.RegisterDate, 
					ImpProject.ProjectType, 
					ImpProject.Status, 
				},

                From = {
              ImpFactory.As( "T1" ) 
           },

                Join = 
				{ 
					Join.Inner( ImpUserCompany.As( "T2" ), ImpFactory.Company.Equal( ImpUserCompany.Company ) ), 

					Join.Inner( ImpCompany.As( "T3" ), ImpFactory.Company.Equal( ImpCompany.Company ) ), 

					Join.Inner( ImpProject.As( "T4" ), ImpFactory.Factory.Equal( ImpProject.Factory ) ), 

					Join.Inner( ImpUserGroup.As( "T5" ), ImpUserCompany.Userid.Equal( ImpUserGroup.Userid ) ), 

					Join.Inner( ImpUserGroupStd.As( "T6" ), ImpUserGroup.UserGroup.Equal( ImpUserGroupStd.Name ) ), 
				},

                Where = 
				{  
					ImpUserCompany.Userid.Equal( user ), 
					ImpFactory.Factory.NotEqual( ImpProject.Project ), 
					ImpUserGroupStd.RoleId.In( new[] { 1, 2, 3 } ), 
				},

                OrderBy =
				{
					{ ImpCompany.Company, OrderBy.Ascending }, 
					{ ImpFactory.Factory, OrderBy.Ascending }, 
					{ ImpProject.Project, OrderBy.Ascending },			
				}
            };

            return query.ToString();
        }

        /// <summary>
        /// Get the query for fetching the phases for each project available to the specified user.
        /// </summary>
        /// <param name="user">
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        static string GetAllProjectPhaseQuery( string user )
        {
            ImpactQuery query = new ImpactQuery
                                {
                                    Select = 
				{ 
					ImpModelPhase.Factory, 
					ImpModelPhase.Project, 
					ImpModelPhase.Name, 
				},

                                    From = {
              ImpFactory.As( "T1" ) 
           },

                                    Join = 
				{ 
					Join.Inner( ImpUserCompany.As( "T2" ), ImpFactory.Company.Equal( ImpUserCompany.Company ) ), 

					Join.Inner( ImpCompany.As( "T3" ), ImpFactory.Company.Equal( ImpCompany.Company ) ), 

					Join.Inner( ImpProject.As( "T4" ), ImpFactory.Factory.Equal( ImpProject.Factory ) ), 

					Join.Inner( ImpModelPhase.As( "T5" ), ImpProject.Factory.Equal( ImpModelPhase.Factory ),  ImpProject.Project.Equal( ImpModelPhase.Project ) ), 
				},

                                    Where = 
				{  
					ImpUserCompany.Userid.Equal( user ), 
					ImpFactory.Factory.NotEqual( ImpProject.Project ), 
				},

                                    OrderBy =
				{
					{ ImpModelPhase.Factory, OrderBy.Descending }, 
					{ ImpModelPhase.Project, OrderBy.Descending }, 
					{ ImpModelPhase.Name, OrderBy.Descending }, 
				}
                                };

            return query.ToString();
        }

        /// <summary>
        /// Loads all companies, factories and projects available to the specified user.
        /// </summary>
        /// <param name="user">
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.Data.CompanyDataCollection.
        /// </returns>
        public static CompanyDataCollection Load( string user )
        {
            // Make sure the user name is in lower case. ( Doesn't matter for SQL Server, but might be needed for Ingres )
            user = user.ToLower();

            List<Company> result = new List<Company>();

            string statement = GetCompanyFactoryProjectQuery( user );

            ImpactQuery query = new ImpactQuery
                                {
                                    Select =
				{
					ImpUser.CurrentFactory, 
					ImpUser.CurrentProject
				},
                                    From = {
              ImpUser.As( "T1" ) 
           },
                                    Where = {
               ImpUser.Userid.Equal( user ) 
            },
                                };

            string userFactoryProject = query.ToString();

            CompanyDataCollection data = null;

            using( ImpactDatabase database = new ImpactDatabase() )
            {
                var tempDataList = database.GetAll( statement, column => new
                {
                    Company = column[0].Cast<string>(),
                    CompanyName = column[1].Cast<string>(),
                    Factory = column[2].Cast<string>(),
                    FactoryName = column[3].Cast<string>(),
                    Project = column[4].Cast<string>().Trim(),
                    ProjectName = column[5].Cast<string>(),
                    Description = column[6].Cast<string>(),
                    Date = column[7].Cast<DateTime?>(),
                    ProjectType = column[8].Cast<ProjectType>(),
                    Status = column[9].Cast<ProjectStatus>(),
                } );

                var phaseList = database.GetAll( GetAllProjectPhaseQuery( user ), column => new
                {
                    Factory = column[0].Cast<string>(),
                    Project = column[1].Cast<string>().Trim(),
                    Phase = column[2].Cast<string>().Trim(),
                } );

                result = ( from row in tempDataList
                           orderby row.Company.GetAlphaNumericOrderToken()
                           group row by new { row.Company, row.CompanyName } into companyGroup
                           select new Company( companyGroup.Key.Company, companyGroup.Key.CompanyName )
                           {
                               Factories =
                                 ( from row in companyGroup
                                   orderby row.Factory.GetAlphaNumericOrderToken()
                                   group row by new { row.Factory, row.FactoryName } into factoryGroup
                                   select new Factory( factoryGroup.Key.Factory, factoryGroup.Key.FactoryName )
                                   {
                                       Projects =
                                       ( from row in factoryGroup
                                         orderby row.Date descending
                                         // Find all phases mathching this project and the project's factory, select the phase and order by ascending.
                                         let phases =
                                             phaseList.FindAll( phase => phase.Factory == row.Factory && phase.Project == row.Project ).Select(
                                                 phase => phase.Phase ).OrderBy( phase => phase.GetAlphaNumericOrderToken() )
                                         select
                                             new Project( row.Project, row.ProjectName, row.Description, row.Date, row.ProjectType, row.Status,
                                                          phases.ToList() ) ).ToList()

                                   } ).ToList(),

                           } ).ToList();

                CurrentProject currentProject = database.GetFirst( userFactoryProject, column => new CurrentProject
                                                                                                 {
                                                                                                     User = user,
                                                                                                     Factory = column[0].Cast<string>(),
                                                                                                     Project = ( column[1].Cast<string>() ?? string.Empty ).Trim(),
                                                                                                 } );

                if( null == currentProject )
                {
                    data = new CompanyDataCollection( user, result );
                }
                else
                {
                    data = new CompanyDataCollection( currentProject, result );
                }
            }

            return data;
        }

        /// <summary>
        /// The update current project.
        /// </summary>
        /// <param name="currentProject">
        /// The current project.
        /// </param>
        public static void UpdateCurrentProject( CurrentProject currentProject )
        {
            if( null == currentProject )
                return;

            using( ImpactDatabase database = new ImpactDatabase() )
            {
                ImpactUpdate update = new ImpactUpdate( ImpUser.Instance )
                {
                    Columns =
					 {
						 { ImpUser.CurrentFactory, currentProject.Factory }, 
						 { ImpUser.CurrentProject, currentProject.Project.PadLeft( 12 ) }, 
					 },
                    Where = {
                 ImpUser.Userid.Equal( currentProject.User ) 
              }
                };

                string statement = update.ToString();

                database.ExecuteNonQuery( statement );
            }
        }
    }
}