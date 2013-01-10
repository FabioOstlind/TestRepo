namespace StruSoft.Impact.V120.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;

    using StruSoft.Impact.Data;
    using StruSoft.Impact.DataTypes;
    using StruSoft.Impact.UnitSystem;
    using StruSoft.Impact.V120.DB;
    using StruSoft.Impact.V120.DB.Query;
    using StruSoft.Impact.V120.ProjectManager.Core.ProjectBrowser;
    using StruSoft.Impact.V120.ProjectManager.Core.ProjectBrowserData;
    using StruSoft.Impact.V120.ProjectManager.Core.TableGrid;

    /// <summary>
    /// Loads data used in the Project browser.
    /// </summary>
    internal static class ProjectBrowserLoader
    {
        #region Constants

        /// <summary>
        /// The producing factory external value.
        /// </summary>
        internal const string ProducingFactoryExternalValue = "<EX>";

        /// <summary>
        /// The unit system setting id.
        /// </summary>
        private const string UnitSystemSettingId = "UNIT_SYSTEM";

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Loads all the data necessesary for creating the "Project Browser" tree.
        /// </summary>
        /// <param name="factory">
        /// Which factory the project belongs to.
        /// </param>
        /// <param name="project">
        /// Which project to load.
        /// </param>
        /// <returns>
        /// The <see cref="ProjectRootData"/>.
        /// </returns>
        public static ProjectRootData Load( string factory, string project )
        {
            ProjectRootData projectRoot = new ProjectRootData( factory, project.Trim() );

            project = Util.CorrectProjectName( project );
            string company = Util.FactoryToCompany( factory );

            using( ImpactDatabase database = new ImpactDatabase() )
            {
                var titleAndDescription = database.GetFirst( 
                    GetProjectInfoQuery( factory, project ).ToString(), 
                    column => new
                        {
                            Title = column[0].Cast<string>(),
                            Description = column[1].Cast<string>(),
                            FactoryName = column[2].Cast<string>()
                        } );

                projectRoot.Title = titleAndDescription.Title;
                projectRoot.Description = titleAndDescription.Description;
                projectRoot.FactoryName = titleAndDescription.FactoryName;

                // Get and set the element status standard.
                projectRoot.ElementStatusStandard = database.GetAll( 
                    GetElementStatusStandardQuery( company ), 
                    column => new ElementStatusStandard
                              {
                                  StatusId    = column[0].Cast<int>(),
                                  Description = column[1].Cast<string>(),
                                  ColorIndex  = column[2].Cast<int>(),
                              }

                    ).ToDictionary( item => item.StatusId );

                // Get all elements.
                var elementDataList = database.GetAll( GetProjectElementDataQuery( factory, project ), ReadElementData );

                foreach (var elementGroup in elementDataList.GroupBy(data => data.ElementMark) )
                {
                    var count = elementGroup.Count();
                    foreach (var elementData in elementGroup)
                    {
                        elementData.Quantity = count;
                    }
                }
                
                // Get all levels and buildings.
                var tempBuildingFloorList = database.GetAll( 
                    GetBuildingLevelDataQuery( factory, project ), 
                    column => new
                    {
                        Building             = column[0].Cast<string>().Trim(), 
                        FloorId              = column[1].Cast<int>(), 
                        FloorName            = column[2].Cast<string>(), 
                        ElevationFloor       = column[3].Cast<double>(), 
                        Topping              = column[4].Cast<double>(), 
                        FloorHeight          = column[5].Cast<double>(), 
                        FloorToHeadOfOpening = column[6].Cast<double>(), 
                        FloorToCeiling       = column[7].Cast<double>(), 
                        WallBlockUp          = column[8].Cast<double>(), 
                    });

                // Phases.
                projectRoot.Phases = database.GetAll( GetPhaseQuery( factory, project ), column => column[0].Cast<string>().Trim() ).OrderBy( x => x.GetAlphaNumericOrderToken() ).ToList();

                // Elevations.
                var elevationList = database.GetAll( GetElevationQuery( factory, project ).ToString(), column => column[0].Cast<string>().Trim() ).OrderBy( x => x.GetAlphaNumericOrderToken() ).ToList();

                var elementsGrouping = ( from row in elementDataList
                                         orderby row.ElementMark.GetAlphaNumericOrderToken() ascending
                                         group row by new { row.Building, row.FloorId, row.ElementType } into g
                                         select g ).ToList();

                var buildingDataList = ( from row in tempBuildingFloorList
                                         group row by new { row.Building } into buildingGroup
                                         orderby buildingGroup.Key.Building.GetAlphaNumericOrderToken()
                                         select new BuildingData( buildingGroup.Key.Building )
                                         {
                                             Levels =
                                               ( from level in buildingGroup
                                                 orderby level.ElevationFloor
                                                 select new LevelData
                                                        {
                                                     Building              = level.Building, 
                                                     Id                    = level.FloorId, 
                                                     Name                  = level.FloorName, 
                                                     Elevation             = level.ElevationFloor, 
                                                     Height                = level.FloorHeight, 
                                                     HeightToHeadOfOpening = level.FloorToHeadOfOpening, 
                                                     HeightToCeiling       = level.FloorToCeiling, 
                                                     WallBlockUp           = level.WallBlockUp, 

                                                     ElementCategories =
                                                         ( from elementGroup in elementsGrouping
                                                           where
                                                           elementGroup.Key.Building == level.Building &&
                                                           elementGroup.Key.FloorId == level.FloorId
                                                           select new ElementCategoryData( elementGroup.Key.ElementType, elementGroup.ToList() ) ).ToList()
                                                 }

                                                 ).ToList()
                                         }).ToList();

                // Set the building data list.
                projectRoot.Model = new ModelData( buildingDataList, elevationList );

                // Get Company Settings.
                CompanySettings companySettings = GetCompanySettings( company, database );

                projectRoot.CompanySettings = companySettings;
            }

            return projectRoot;
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
        public static PlanningData LoadPlanningTableData( string factory, string project, string user, int[] idList )
        {
            string company = Util.FactoryToCompany( factory );
            project = Util.CorrectProjectName( project );

            List<PlanningElementData> planningElementDataList;

            using( ImpactDatabase service = new ImpactDatabase() )
            {
                string query = GetPlanningElementDataQuery( factory, project, idList ).ToString();
                string factoryQuery = GetFactoryQuery( factory, project, user ).ToString();
                string divisionQuery = GetPlanningDivisionQuery( company ).ToString();

                // Get all the specified elements.
                planningElementDataList = service.GetAll( query, ReadPlanningElementData );

                var elementStatusStandard = service.GetAll( 
                    GetElementStatusStandardQuery( company ), 
                    column => new ElementStatusStandard
                        {
                            StatusId    = column[0].Cast<int>(), 
                            Description = column[1].Cast<string>(), 
                            ColorIndex  = column[2].Cast<int>(), 
                        }).ToDictionary( item => item.StatusId );

                var factoryDataList = service.GetAll( factoryQuery, column => new Factory( column[0].Cast<string>(), column[1].Cast<string>() ) );
                factoryDataList.Add( Factory.External );

                var divisionDataList = service.GetAll( divisionQuery, column => column[0].Cast<string>() );

                foreach( var planningElement in planningElementDataList )
                {
                    ElementStatusStandard standard;

                    if( elementStatusStandard.TryGetValue( planningElement.Status, out standard ) )
                    {
                        planningElement.StatusDescription = standard.Description;
                    }

                    if( 0 == string.Compare( planningElement.ProducingFactory, ProjectBrowserLoader.ProducingFactoryExternalValue, true ) )
                    {
                        planningElement.ProducingFactory = Factory.External.Number;
                    }
                }

                PlanningData planningData = new PlanningData( planningElementDataList, divisionDataList, factoryDataList );

                return planningData;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The create default unit data.
        /// </summary>
        /// <param name="unitSystem">
        /// The unit system.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.List`1[T -&gt; StruSoft.Impact.V120.ProjectManager.Core.ProjectBrowser.CompanyUnit].
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        private static List<CompanyUnit> CreateDefaultUnitData( UnitSystemType unitSystem )
        {
            if( unitSystem == UnitSystemType.Metric )
            {
                return new List<CompanyUnit>
                    {
                        new CompanyUnit( UnitParameter.Angle, UnitType.Decimal, (int)MetricUnits.Angle.Degree, 3 ), 
                        new CompanyUnit( UnitParameter.Area, UnitType.Decimal, (int)MetricUnits.Area.SquareMeter, 2 ), 
                        new CompanyUnit( UnitParameter.Density, UnitType.Decimal, (int)MetricUnits.Density.TonPerCubicMeter, 3 ), 
                        new CompanyUnit( UnitParameter.Force, UnitType.Decimal, (int)MetricUnits.Force.KiloNewton, 3 ), 
                        new CompanyUnit( UnitParameter.Length, UnitType.Decimal, (int)MetricUnits.Length.Millimeter, 0 ), 
                        new CompanyUnit( UnitParameter.Mass, UnitType.Decimal, (int)MetricUnits.Mass.Tons, 2 ), 
                        new CompanyUnit( UnitParameter.Volume, UnitType.Decimal, (int)MetricUnits.Volume.CubicMeter, 3 ), 
                        new CompanyUnit( UnitParameter.Weight, UnitType.Decimal, (int)MetricUnits.Weight.KiloNewtonPerCubicMeter, 3 ), 
                    };
            }

            if( unitSystem == UnitSystemType.Imperial )
            {
                return new List<CompanyUnit>
                    {
                        new CompanyUnit( UnitParameter.Angle, UnitType.Decimal, (int)ImperialUnits.Angle.Degree, 3 ), 
                        new CompanyUnit( UnitParameter.Area, UnitType.Decimal, (int)ImperialUnits.Area.SquareInches, 2 ), 
                        new CompanyUnit( UnitParameter.Density, UnitType.Decimal, (int)ImperialUnits.Density.PoundsPerCubeInch, 3 ), 
                        new CompanyUnit( UnitParameter.Force, UnitType.Decimal, (int)ImperialUnits.Force.PoundForce, 3 ), 
                        new CompanyUnit( UnitParameter.Length, UnitType.Architectural, (int)ImperialUnits.Length.Inches, 0 ), 
                        new CompanyUnit( UnitParameter.Mass, UnitType.Decimal, (int)ImperialUnits.Mass.Tons, 3 ), 
                        new CompanyUnit( UnitParameter.Volume, UnitType.Decimal, (int)ImperialUnits.Volume.CubeFeet, 3 ), 
                        new CompanyUnit( UnitParameter.Weight, UnitType.Decimal, (int)ImperialUnits.Weight.PoundForcePerCubicFeet, 3 ), 
                    };
            }

            throw new ArgumentException( string.Format( "Unsupported unit system '{0}'.", unitSystem ), "unitSystem" );
        }

        /// <summary>
        /// Get the query for fetching the building and level data for the project browser.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// Sql query string.
        /// </returns>
        private static string GetBuildingLevelDataQuery( string factory, string project )
        {
            ImpactQuery query = new ImpactQuery
                                {
                Select = 
                {
                    ImpModelFloor.Building, 
                    ImpModelFloor.FloorId, 
                    ImpModelFloor.FloorName, 
                    ImpModelFloor.ElevationFloor, 
                    ImpModelFloor.Topping, 
                    ImpModelFloor.FloorHeight, 
                    ImpModelFloor.FloorToHeadOfOpening, 
                    ImpModelFloor.FloorToCeiling, 
                    ImpModelFloor.WallBlockUp, 
                }, 

                From = {
                          ImpModelFloor.As( "T1" ) 
                       }, 

                Where =
                {
                    ImpModelFloor.Factory.Equal( factory ), 
                    ImpModelFloor.Project.Equal( project ), 
                    ImpModelFloor.FloorLevel.Equal( "-" ), 
                }, 

                OrderBy =
                {
                    { ImpModelFloor.Building, OrderBy.Ascending }, 
                    { ImpModelFloor.FloorId, OrderBy.Ascending }, 
                }, 
            };

            string statement = query.ToString();

            return statement;
        }

        /// <summary>
        /// The get company settings.
        /// </summary>
        /// <param name="company">
        /// The company.
        /// </param>
        /// <param name="database">
        /// The database.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.ProjectManager.Core.ProjectBrowser.CompanySettings.
        /// </returns>
        private static CompanySettings GetCompanySettings( string company, ImpactDatabase database )
        {
            string companySettingsQuery = GetCompanySettingsQuery( company ).ToString();

            string settingValue = database.GetFirst( companySettingsQuery, column => column[0].Cast<string>() );

            UnitSystemType unitSystem;

            CompanySettings companySettings = new CompanySettings();

            if( !Enum.TryParse( settingValue, true, out unitSystem ) )
            {
                // Default to metric if we failed conversion.
                unitSystem = UnitSystemType.Metric;

                // Since we could not find a unit system in the database for the specified company,
                // we must assume there are no company units either. So we create default company settings.
                companySettings.UnitSystem = unitSystem;

                foreach( var item in CreateDefaultUnitData( unitSystem ) )
                {
                    companySettings[item.Parameter] = item;
                }
            }
            else
            {
                companySettings.UnitSystem = unitSystem;

                // Get the company units from the database.
                string companyUnitsQuery = GetCompanyUnitsQuery( company ).ToString();

                var companyUnitList = database.GetAll( 
                    companyUnitsQuery, 
                    column =>
                        {
                            UnitParameter parameter;

                            if( !Enum.TryParse( column[0].Cast<string>(), true, out parameter ) )
                            {
                                return null;
                            }

                            UnitType unitType = column[1].Cast<UnitType>();

                            return new CompanyUnit( parameter, unitType, column[2].Cast<int>(), column[3].Cast<int>() );
                        });

                // Join the default unit data with the unit data from the database. If any unit is
                // missing from the database, then we'll use the default unit instead.
                var leftJoin = ( from defaultUnit in CreateDefaultUnitData( unitSystem )
                                 join databaseUnit in companyUnitList
                                     on defaultUnit.Parameter equals databaseUnit.Parameter into joinedList
                                 from companyUnit in joinedList.DefaultIfEmpty( defaultUnit )
                                 select companyUnit ).ToList();

                foreach( var item in leftJoin )
                {
                    companySettings[item.Parameter] = item;
                }
            }

            ImpactQuery dateFormatQuery = new ImpactQuery( true )
                {
                    From = {
                               ImpCompany.As( "T1" ) 
                           }, 
                    Select = {
                                 ImpCompany.DateFormat 
                             }, 
                    Where =
                        {
                            ImpCompany.Company.Equal( company ), 
                        }
                };

            var dateFormatList = database.GetAll( dateFormatQuery.ToString(), column => column[0].Cast<CompanyDateFormat>() );

            if( dateFormatList.Count > 0 )
            {
                companySettings.DateFormat = dateFormatList[0];
            }
            else
            {
                companySettings.DateFormat = CompanyDateFormat.Sweden;
            }

            return companySettings;
        }

        /// <summary>
        /// The get company settings query.
        /// </summary>
        /// <param name="company">
        /// The company.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        private static ImpactQuery GetCompanySettingsQuery( string company )
        {
            ImpactQuery query = new ImpactQuery
                                {
                From = {
                          ImpCompanySettings.As( "T1" ) 
                       }, 
                Select = {
                            ImpCompanySettings.SettingValue 
                         }, 
                Where = 
                { 
                    ImpCompanySettings.Company.Equal( company ), 
                    ImpCompanySettings.SettingId.Equal( UnitSystemSettingId )
                }
            };

            return query;
        }

        /// <summary>
        /// The get company units query.
        /// </summary>
        /// <param name="company">
        /// The company.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        private static ImpactQuery GetCompanyUnitsQuery( string company )
        {
            ImpactQuery query = new ImpactQuery
                                {
                From = {
                          ImpCompanyUnits.As( "T1" ) 
                       }, 
                Select = 
                { 
                    ImpCompanyUnits.Dimension, 
                    ImpCompanyUnits.UnitType, 
                    ImpCompanyUnits.Unit, 
                    ImpCompanyUnits.Precision, 
                }, 
                Where = 
                { 
                    ImpCompanyUnits.Company.Equal( company ), 
                }
            };

            return query;
        }

        /// <summary>
        /// Get the query for fetching the element id status standard.
        /// </summary>
        /// <param name="company">
        /// The company which standard we want to fetch.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        private static string GetElementStatusStandardQuery( string company )
        {
            ImpactQuery query = new ImpactQuery
                {
                    Select =
                        {
                            ImpElementIdStatusStd.StatusId, 
                            ImpElementIdStatusStd.Description, 
                            ImpElementIdStatusStd.ColorIndex, 
                        }, 

                    From = {
                               ImpElementIdStatusStd.As( "T1" ) 
                           }, 

                    Where = {
                                ImpElementIdStatusStd.Factory.Equal( company ), ImpElementIdStatusStd.Project.Equal( company ) 
                            }, 

                    OrderBy = {
                                  { ImpElementIdStatusStd.StatusId, OrderBy.Ascending } 
                              }, 
                };

            string statement = query.ToString();

            return statement;
        }

        /// <summary>
        /// The get elevation query.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        private static ImpactQuery GetElevationQuery( string factory, string project )
        {
            ImpactQuery query = new ImpactQuery( true )
                {
                    From = {
                               ImpModelGeometry.As( "T1" ) 
                           }, 
                    Select = {
                                 ImpModelGeometry.Elevation 
                             }, 
                    Where =
                        {
                            ImpModelGeometry.Factory.Equal( factory ), 
                            ImpModelGeometry.Project.Equal( project ), 
                            ImpModelGeometry.Elevation.NotEqual( string.Empty ), 
                            ImpModelGeometry.Deleted.Equal( false ), 
                        }, 
                };

            return query;
        }

        /// <summary>
        /// The get factory query.
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
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        private static ImpactQuery GetFactoryQuery( string factory, string project, string user )
        {
            string company = Util.FactoryToCompany( factory );

            ImpactQuery query = new ImpactQuery( true )
                {
                    Select = 
                        { 
                            ImpFactory.Factory, 
                            ImpFactory.Name, 
                        }, 

                    From = {
                               ImpFactory.As( "T1" ) 
                           }, 

                    Join = 
                        { 
                            Join.Inner( ImpUserCompany.As( "T2" ), ImpFactory.Company.Equal( ImpUserCompany.Company ) ), 

                            Join.Inner( ImpUserGroup.As( "T5" ), ImpUserCompany.Userid.Equal( ImpUserGroup.Userid ) ), 

                            Join.Inner( ImpUserGroupStd.As( "T6" ), ImpUserGroup.UserGroup.Equal( ImpUserGroupStd.Name ) ), 
                        }, 

                    Where = 
                        {  
                            ImpFactory.Company.Equal( company ), 
                            ImpUserCompany.Userid.Equal( user ), 
                            ImpUserGroupStd.RoleId.In( new[] { 1, 2, 3 } ), 
                        }, 

                    OrderBy =
                        {
                            { ImpFactory.Factory, OrderBy.Ascending }, 
                        }
                };

            return query;
        }

        /// <summary>
        /// Get the query for fetching all phases for the specified project.
        /// </summary>
        /// <param name="factory">
        /// Which factory the project belongs to.
        /// </param>
        /// <param name="project">
        /// Which project to load the phases from.
        /// </param>
        /// <returns>
        /// Sql query statement.
        /// </returns>
        private static string GetPhaseQuery( string factory, string project )
        {
            ImpactQuery query = new ImpactQuery
                {
                    Select = 
                        { 
                            ImpModelPhase.Name, 
                        }, 

                    From = {
                               ImpModelPhase.As( "T1" ) 
                           }, 

                    Where = 
                        {  
                            ImpModelPhase.Factory.Equal( factory ), 
                            ImpModelPhase.Project.Equal( project ), 
                        }, 

                    OrderBy =
                        {
                            { ImpModelPhase.Name, OrderBy.Ascending }, 
                        }
                };

            return query.ToString();
        }

        /// <summary>
        /// The get planning division query.
        /// </summary>
        /// <param name="company"> </param>
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        private static ImpactQuery GetPlanningDivisionQuery( string company )
        {
            ImpactQuery query = new ImpactQuery( true )
            {
                From = {
                          ImpProductionDivisionStd.As( "T1" ) 
                       }, 
                Select = {
                            ImpProductionDivisionStd.Name 
                         }, 
                Where =
                {
                    ImpProductionDivisionStd.Factory.Equal( company ), 
                    ImpProductionDivisionStd.Project.Equal( company ), 
                    ImpProductionDivisionStd.DivisionType.Equal(2),
                }, 
            };

            return query;
        }

        /// <summary>
        /// The get planning element data query.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <param name="idList">
        /// The id list.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        private static ImpactQuery GetPlanningElementDataQuery( string factory, string project, int[] idList = null )
        {
            ImpactQuery query = new ImpactQuery
                {
                    Select = 
                        {
                            ImpModelGeometry.ElementMark, 
                            ImpModelGeometry.ElementId, 
                            ImpElement.ElementType, 
                            ImpElement.ElementGrp, 
                            ImpElement.DrawingName, 

                            Aggregate.Max( ImpDrawingRevision.Revision ), 
                            ImpDrawing.Status, 

                            ImpElement.Product, 
                            ImpElement.ElementLength, 
                            ImpElement.ElementWidth, 
                            ImpElement.ElementHeight, 
                            ImpElement.GrossArea, 
                            ImpElement.Mass, 
                            ImpModelGeometry.ReadyForProduction, 
                            ImpModelGeometry.ReadyForProductionDate, 

                            ImpModelPlanning.ProductionFactory, 
                            ImpModelPlanning.DivisionProduction, 
                            ImpModelPlanning.ProductionDate, 
                            ImpModelPlanning.DeliveryDate, 
                            ImpModelPlanning.ErectionSequenceNo, 
                            ImpModelPlanning.TransportId, 
                            ImpModelPlanning.StackId, 
                            ImpModelPlanning.CastId, 
                            ImpModelPlanning.PlannedDrawingDate, 
                            ImpModelPlanning.PlannedProductionDate, 
                            ImpModelPlanning.PlannedReadyForDeliveryDate, 
                            ImpModelPlanning.PlannedDeliveryDate, 
                            ImpModelPlanning.PlannedErectionDate, 
                            ImpModelPlanning.ElementIdStatus, 
                        }, 

                    From = {
                               ImpModelGeometry.As( "T1" ) 
                           }, 

                    Join = 
                        {
                            Join.Inner(
                                ImpElement.As( "T2" ), 
                                ImpModelGeometry.Factory.Equal( ImpElement.Factory ), 
                                ImpModelGeometry.Project.Equal( ImpElement.Project ), 
                                ImpModelGeometry.ElementMark.Equal( ImpElement.ElementMark ) ), 

                            Join.Left(
                                ImpDrawing.As( "T3" ), 
                                ImpElement.Factory.Equal( ImpDrawing.Factory ), 
                                ImpElement.Project.Equal( ImpDrawing.Project ), 
                                ImpElement.DrawingName.Equal( ImpDrawing.DrawingName ) ), 

                            Join.Left(
                                ImpDrawingRevision.As( "T4" ), 
                                ImpElement.Factory.Equal( ImpDrawingRevision.Factory ), 
                                ImpElement.Project.Equal( ImpDrawingRevision.Project ), 
                                ImpElement.DrawingName.Equal( ImpDrawingRevision.DrawingName ) ), 

                            Join.Left(
                                ImpModelPlanning.As( "T5" ), 
                                ImpModelGeometry.Factory.Equal( ImpModelPlanning.Factory ), 
                                ImpModelGeometry.Project.Equal( ImpModelPlanning.Project ), 
                                ImpModelGeometry.ElementId.Equal( ImpModelPlanning.ElementId ) ), 
                        }, 

                    Where = 
                        { 
                            ImpModelGeometry.Factory.Equal( factory ), 
                            ImpModelGeometry.Project.Equal( project ), 
                            ImpModelGeometry.Deleted.Equal( false ), 
                        }, 

                    GroupBy = 
                        {
                            ImpModelGeometry.ElementMark, 
                            ImpModelGeometry.ElementId,            
                            ImpElement.ElementType, 
                            ImpElement.ElementGrp, 
                            ImpElement.DrawingName, 
                            ImpDrawing.Status, 

                            ImpElement.Product, 
                            ImpElement.ElementLength, 
                            ImpElement.ElementWidth, 
                            ImpElement.ElementHeight, 
                            ImpElement.GrossArea, 
                            ImpElement.Mass, 
                            ImpModelGeometry.ReadyForProduction, 
                            ImpModelGeometry.ReadyForProductionDate, 

                            ImpModelPlanning.ProductionFactory, 
                            ImpModelPlanning.DivisionProduction, 

                            ImpModelPlanning.ProductionDate, 
                            ImpModelPlanning.DeliveryDate, 
                            ImpModelPlanning.ErectionSequenceNo, 
                            ImpModelPlanning.TransportId, 
                            ImpModelPlanning.StackId, 
                            ImpModelPlanning.CastId, 
                            ImpModelPlanning.PlannedDrawingDate, 
                            ImpModelPlanning.PlannedProductionDate, 
                            ImpModelPlanning.PlannedReadyForDeliveryDate, 
                            ImpModelPlanning.PlannedDeliveryDate, 
                            ImpModelPlanning.PlannedErectionDate, 
                            ImpModelPlanning.ElementIdStatus, 
                        }, 

                    OrderBy = 
                        { 
                            { ImpModelGeometry.ElementMark, OrderBy.Ascending }, 
                            { ImpModelGeometry.ElementId, OrderBy.Ascending }, 
                        }, 
                };

            if( null != idList && idList.Length > 0 )
            {
                query.Where.Add( ImpModelGeometry.ElementId.In( idList ) );
            }

            return query;
        }

        /// <summary>
        /// Get the query for fetching element data for the project browser.
        /// </summary>
        /// <param name="factory">
        /// The current factory.
        /// </param>
        /// <param name="project">
        /// The current project.
        /// </param>
        /// <param name="idList">
        /// A list of element id for the elements we want to get. Pass null to get all elements.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        private static string GetProjectElementDataQuery( string factory, string project, int[] idList = null )
        {
            ImpactQuery query = new ImpactQuery
                {
                    Select = 
                        {
                            ImpModelGeometry.ElementMark, 
                            ImpModelGeometry.ElementId, 
                            ImpModelGeometry.ElementGuid, 
                            ImpElement.ElementType, 
                            ImpModelGeometry.Building, 
                            ImpModelGeometry.FloorId, 
                            ImpModelGeometry.Phase, 
                            ImpElement.ElementGrp, 
                            ImpElement.DrawingName, 
                    
                            Aggregate.Max( ImpDrawingRevision.Revision ), 
                            ImpDrawing.DesignedBy, 
                            ImpDrawing.Status, 
                    
                            ImpElement.Product, 
                            ImpElement.ElementLength, 
                            ImpElement.ElementWidth, 
                            ImpElement.ElementHeight, 
                            ImpElement.GrossArea, 
                            ImpElement.Mass, 
                            ImpModelGeometry.ReadyForProduction, 
                            ImpModelGeometry.ReadyForProductionDate, 
                    
                            ImpModelPlanning.ProductionDate, 
                            ImpModelPlanning.DeliveryDate, 
                            ImpModelPlanning.ErectionSequenceNo, 
                            ImpModelPlanning.TransportId, 
                            ImpModelPlanning.StackId, 
                            ImpModelPlanning.StackSequenceNo, 
                            ImpModelPlanning.PlannedDrawingDate, 
                            ImpModelPlanning.PlannedProductionDate, 
                            ImpModelPlanning.PlannedReadyForDeliveryDate, 
                            ImpModelPlanning.PlannedDeliveryDate, 
                            ImpModelPlanning.PlannedErectionDate, 
                            ImpModelPlanning.ElementIdStatus, 
                    
                            ImpModelGeometry.Elevation, 

                            ImpModelPlanning.CastId, 
                            ImpModelPlanning.BedX, 
                            ImpModelPlanning.BedY, 
                            ImpModelPlanning.BedZ, 
                            ImpModelPlanning.BedRotation, 
                            ImpModelPlanning.BedSequenceNo, 
                    
                            ImpElement.Style, 
                            ImpElement.Lift, 
                            ImpElement.Strandptn, 
                            ImpElement.LockStatus, 
                            ImpElement.Remark, 
                    
                            ImpDrawing.CheckedBy, 
                            ImpModelGeometry.ModelGeometryZ, 
                            ImpModelGeometry.DrawInModel, 
                    
                            ImpModelPlanning.ProductionFactory, 
                            ImpModelPlanning.DivisionProduction, 
                        }, 
                
                    From = {
                               ImpModelGeometry.As( "T1" ) 
                           }, 
                
                    Join = 
                        {
                            Join.Inner(
                                ImpElement.As( "T2" ), 
                                ImpModelGeometry.Factory.Equal( ImpElement.Factory ), 
                                ImpModelGeometry.Project.Equal( ImpElement.Project ), 
                                ImpModelGeometry.ElementMark.Equal( ImpElement.ElementMark ) ), 
                        
                            Join.Left(
                                ImpDrawing.As( "T3" ), 
                                ImpElement.Factory.Equal( ImpDrawing.Factory ), 
                                ImpElement.Project.Equal( ImpDrawing.Project ), 
                                ImpElement.DrawingName.Equal( ImpDrawing.DrawingName ) ), 
                        
                            Join.Left(
                                ImpDrawingRevision.As( "T4" ), 
                                ImpElement.Factory.Equal( ImpDrawingRevision.Factory ), 
                                ImpElement.Project.Equal( ImpDrawingRevision.Project ), 
                                ImpElement.DrawingName.Equal( ImpDrawingRevision.DrawingName ) ), 
                        
                            Join.Left(
                                ImpModelPlanning.As( "T5" ), 
                                ImpModelGeometry.Factory.Equal( ImpModelPlanning.Factory ), 
                                ImpModelGeometry.Project.Equal( ImpModelPlanning.Project ), 
                                ImpModelGeometry.ElementId.Equal( ImpModelPlanning.ElementId ) ), 
                        }, 
                
                    Where = 
                        { 
                            ImpModelGeometry.Factory.Equal( factory ), 
                            ImpModelGeometry.Project.Equal( project ), 
                            ImpModelGeometry.Deleted.Equal( false ), 
                            //ImpModelGeometry.DrawInModel.Equal( true ), 
                        }, 
                
                    GroupBy = 
                        {
                            ImpModelGeometry.ElementMark, 
                            ImpModelGeometry.ElementId, 
                            ImpModelGeometry.ElementGuid, 
                            ImpElement.ElementType, 
                            ImpModelGeometry.Building, 
                            ImpModelGeometry.FloorId, 
                            ImpModelGeometry.Phase, 
                            ImpElement.ElementGrp, 
                            ImpElement.DrawingName, 
                    
                            ImpDrawing.DesignedBy, 
                            ImpDrawing.Status, 
                    
                            ImpElement.Product, 
                            ImpElement.ElementLength, 
                            ImpElement.ElementWidth, 
                            ImpElement.ElementHeight, 
                            ImpElement.GrossArea, 
                            ImpElement.Mass, 
                            ImpModelGeometry.ReadyForProduction, 
                            ImpModelGeometry.ReadyForProductionDate, 
                    
                            ImpModelPlanning.ProductionDate, 
                            ImpModelPlanning.DeliveryDate, 
                            ImpModelPlanning.ErectionSequenceNo, 
                            ImpModelPlanning.TransportId, 
                            ImpModelPlanning.StackId, 
                            ImpModelPlanning.StackSequenceNo, 
                            ImpModelPlanning.PlannedDrawingDate, 
                            ImpModelPlanning.PlannedProductionDate, 
                            ImpModelPlanning.PlannedReadyForDeliveryDate, 
                            ImpModelPlanning.PlannedDeliveryDate, 
                            ImpModelPlanning.PlannedErectionDate, 
                            ImpModelPlanning.ElementIdStatus, 

                            ImpModelGeometry.Elevation, 

                            ImpModelPlanning.CastId, 
                            ImpModelPlanning.BedX, 
                            ImpModelPlanning.BedY, 
                            ImpModelPlanning.BedZ, 
                            ImpModelPlanning.BedRotation, 
                            ImpModelPlanning.BedSequenceNo, 
 
                            ImpElement.Style, 
                            ImpElement.Lift, 
                            ImpElement.Strandptn, 
                            ImpElement.LockStatus, 
                            ImpElement.Remark, 
                    
                            ImpDrawing.CheckedBy, 
                            ImpModelGeometry.ModelGeometryZ, 
                            ImpModelGeometry.DrawInModel, 
                    
                            ImpModelPlanning.ProductionFactory, 
                            ImpModelPlanning.DivisionProduction, 
                        }, 
                
                    OrderBy = 
                        { 
                            { ImpModelGeometry.ElementId, OrderBy.Ascending }, 
                            { ImpModelGeometry.ElementMark, OrderBy.Ascending } 
                        }, 
                };

            if( null != idList && idList.Length > 0 )
            {
                query.Where.Add( ImpModelGeometry.ElementId.In( idList ) );
            }

            string statement = query.ToString();

            return statement;
        }

        /// <summary>
        /// Gets the query for returning project name, project description and factory name.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        private static ImpactQuery GetProjectInfoQuery( string factory, string project )
        {
            ImpactQuery query = new ImpactQuery( true )
                {
                    From = 
                    {
                        ImpProject.As( "T1" ) 
                    },
                    Select = 
                    {
                        ImpProject.Name,
                        ImpProject.Description,
                        ImpFactory.Name
                    },

                    Join =
                        {
                            Join.Inner( ImpFactory.As( "T2" ), ImpFactory.Factory.Equal( ImpProject.Factory ) )
                        },
                    Where =
                    {
                        ImpProject.Factory.Equal( factory ), ImpProject.Project.Equal( project ) 
                    }
                };

            return query;
        }

        /// <summary>
        /// The read element data.
        /// </summary>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.ProjectManager.Core.ProjectBrowserData.ElementData.
        /// </returns>
        private static ElementData ReadElementData( DbDataReader column )
        {
            var element = new ElementData
            {
                ElementMark                 = column[0].Cast<string>().Trim() ?? string.Empty, 
                Id                          = column[1].Cast<int>(), 
                ElementGuid                 = column[2].Cast<string>() ?? string.Empty, 
                ElementType                 = column[3].Cast<string>() ?? string.Empty, 
                Building                    = column[4].Cast<string>().Trim() ?? string.Empty, 
                FloorId                     = column[5].Cast<int>(), 
                Phase                       = column[6].Cast<string>().Trim() ?? string.Empty, 
                ElementGrp                  = column[7].Cast<string>() ?? string.Empty, 
                DrawingName                 = column[8].Cast<string>() ?? string.Empty, 
                DrawingRevision             = column[9].Cast<string>() ?? string.Empty, 
                DrawingDesignedBy           = column[10].Cast<string>() ?? string.Empty, 
                DrawingStatus               = column[11].Cast<string>() ?? string.Empty, 
                Product                     = column[12].Cast<string>() ?? string.Empty, 
                Length                      = column[13].Cast<double>(), 
                Width                       = column[14].Cast<double>(), 
                Height                      = column[15].Cast<double>(), 
                GrossArea                   = column[16].Cast<double>(), 
                Mass                        = column[17].Cast<double>(), 
                ReadyForProduction          = column[18].Cast<int>(), 
                ReadyForProductionDate      = column[19].Cast<DateTime?>(), 
                ProductionDate              = column[20].Cast<DateTime?>(), 
                DeliveryDate                = column[21].Cast<DateTime?>(), 
                ErectionSequenceNo          = column[22].Cast<int?>(), 
                TransportId                 = column[23].Cast<int?>(), 
                StackNo                     = column[24].Cast<int?>(), 
                StackSequenceNo             = column[25].Cast<int?>(), 
                PlannedDrawingDate          = column[26].Cast<DateTime?>(), 
                PlannedProductionDate       = column[27].Cast<DateTime?>(), 
                PlannedReadyForDeliveryDate = column[28].Cast<DateTime?>(), 
                PlannedDeliveryDate         = column[29].Cast<DateTime?>(), 
                PlannedErectionDate         = column[30].Cast<DateTime?>(), 
                ElementIdStatus             = column[31].Cast<int?>(), 
                ElevationName               = column[32].Cast<string>() ?? string.Empty, 
                CastId                      = column[33].Cast<int?>() ?? 0, 
                BedX                        = column[34].Cast<double?>() ?? 0.0d, 
                BedY                        = column[35].Cast<double?>() ?? 0.0d, 
                BedZ                        = column[36].Cast<double?>() ?? 0.0d, 
                BedRotation                 = column[37].Cast<double?>() ?? 0.0d, 
                BedSequenceNo               = column[38].Cast<int?>() ?? 0, 
                Style                       = column[39].Cast<string>() ?? string.Empty, 
                Lift                        = column[40].Cast<string>() ?? string.Empty, 
                StrandPattern               = column[41].Cast<string>() ?? string.Empty, 
                LockStatus                  = column[42].Cast<int?>() ?? 0, 
                Remark                      = column[43].Cast<string>() ?? string.Empty, 
                DrawingCheckedBy            = column[44].Cast<string>() ?? string.Empty, 
                ElevationZ                  = column[45].Cast<double?>() ?? 0.0d, 
                DrawInModel                 = column[46].Cast<int?>() ?? 0, 
                ProductionFactory           = column[47].Cast<string>() ?? string.Empty, 
                DivisionProduction          = column[48].Cast<string>() ?? string.Empty, 
            };

            return element;
        }

        /// <summary>
        /// The read planning element data.
        /// </summary>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.ProjectManager.Core.TableGrid.PlanningElementData.
        /// </returns>
        private static PlanningElementData ReadPlanningElementData( DbDataReader column )
        {
            PlanningElementData planningElement = new PlanningElementData( 
                column[0].Cast<string>().Trim(), 
                column[1].Cast<int>(), 
                (ElementType)column[2].Cast<string>() );

            int index = 3;

            planningElement.Group                  = column[index++].Cast<string>() ?? string.Empty;
            planningElement.Drawing                = column[index++].Cast<string>() ?? string.Empty;
            planningElement.DrawingRevision        = column[index++].Cast<string>() ?? string.Empty;
            planningElement.DrawingStatus          = column[index++].Cast<string>() ?? string.Empty;
            planningElement.Product                = column[index++].Cast<string>() ?? string.Empty;
            planningElement.Length                 = column[index++].Cast<double>();
            planningElement.Width                  = column[index++].Cast<double>();
            planningElement.Height                 = column[index++].Cast<double>();
            planningElement.Area                   = column[index++].Cast<double>();
            planningElement.Mass                   = column[index++].Cast<double>();
            planningElement.ReadyForProduction     = column[index++].Cast<int>() == 0 ? false : true;
            planningElement.ReadyForProductionDate = column[index++].Cast<DateTime?>();
            planningElement.ProducingFactory       = column[index++].Cast<string>() ?? string.Empty;
            planningElement.Division               = column[index++].Cast<string>() ?? string.Empty;
            planningElement.ProductionDate         = column[index++].Cast<DateTime?>();
            planningElement.DeliveryDate           = column[index++].Cast<DateTime?>();
            planningElement.ErectionSequenceNo     = column[index++].Cast<int?>() ?? 0;
            planningElement.LoadNo                 = column[index++].Cast<int?>() ?? 0;
            planningElement.StackNo                = column[index++].Cast<int?>() ?? 0;
            planningElement.CastId                 = column[index++].Cast<int?>() ?? 0;
            planningElement.PlannedDrawingDate     = column[index++].Cast<DateTime?>();
            planningElement.PlannedProductionDate  = column[index++].Cast<DateTime?>();
            planningElement.PlannedStorageDate     = column[index++].Cast<DateTime?>();
            planningElement.PlannedDeliveryDate    = column[index++].Cast<DateTime?>();
            planningElement.PlannedErectionDate    = column[index++].Cast<DateTime?>();
            planningElement.Status                 = column[index++].Cast<int?>() ?? 0;

            return planningElement;
        }

        #endregion
    }
}