using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data.Common;
using System.ServiceModel.Activation;
using StruSoft.Impact.V120.Planning.Common;
using StruSoft.Impact.V120.DB;
using StruSoft.Impact.V120.DB.Query;

namespace StruSoft.Impact.V120.Services
{
    using StruSoft.Impact.V120.Report.Common;

    /// <summary>
    /// Business logic service
    /// </summary>
    public partial class ProjectManager : ITransportManager
    {
        /// <summary>
        /// Creates transports based on the given set of templates
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="templates"></param>
        /// <returns></returns>
        public int CreateTransport( PlanningFilter filter, List<RecTransport> templates )
        {
            if( filter == null )
            {
                throw new ArgumentNullException( "Transport" );
            }
            // Load template details 
            LoadTemplateDetails( templates );

            TMLoader loader = new TMLoader();
            List<TMTransport> transportList = new List<TMTransport>();
            loader.CreateTransportFromTemplate( filter, templates, 0, transportList );
            loader.SaveToDb( transportList, true );
            return transportList.Count;
        }

        /// <summary>
        /// Automatic transportation management
        /// </summary>
        /// <returns></returns>
        public int AutoTransport( List<RecTransport> templates, List<RecTMElement> elements )
        {
            return AutoTransport( null, templates, elements );
        }

        /// <summary>
        /// Automatic transportation management
        /// </summary>
        /// <returns></returns>
        public int AutoTransport( PlanningFilter filter, List<RecTransport> templates, List<RecTMElement> elements )
        {
            if( filter == null )
            {
                throw new ArgumentNullException( "Transport" );
            }
            elements = GetUntransportedElement( filter.Factory, filter.Project, elements );
            if( elements == null || elements.Count == 0 )
            {
                return 0;
            }

            elements.Sort( RecTMElement.CompareErectionSequence );

            // Load template details 
            LoadTemplateDetails( templates );

            bool generateErectionSequence = false;
            TMLoader loader = new TMLoader();
            return loader.Load( elements, templates, filter, generateErectionSequence );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templates"></param>
        private void LoadTemplateDetails( List<RecTransport> templates )
        {
            foreach( RecTransport tpl in templates )
            {
                ProjectManager svc = new ProjectManager();
                tpl.Vehicles = svc.LoadTransportVehicles( tpl );

                // Load vehicle stacks
                foreach( RecTransportVehicleStd veh in tpl.Vehicles )
                {
                    RecTransportVehicleStackStd recStack = new RecTransportVehicleStackStd();
                    recStack.Factory = veh.Factory;
                    recStack.Project = veh.Project;
                    recStack.Name = veh.Name;
                    veh.Stacks = new ProjectManager().LoadTransportVehicleStackStd( recStack );
                    if( veh.Stacks.Count == 0 )
                    {
                        string msg = string.Format( "There are no stacks defined for the standard vehicle \"{0}\" !", veh.Name );
                        throw new FaultException<BusinessFaultContract>( new BusinessFaultContract() { Argument = msg }, "There are no stacks defined!" );
                    }
                }
                if( tpl.Vehicles.Count == 0 )
                {
                    string msg = string.Format( "There are no standard vehicles associated with the transport template \"{0}\" !", tpl.Description );
                    throw new FaultException<BusinessFaultContract>( new BusinessFaultContract() { Argument = msg }, "The transport temmplate is missing vehicles!" );
                }
            }
        }

        /// <summary>
        /// GetUntransportedElement
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="project"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        private List<RecTMElement> GetUntransportedElement( string factory, string project, List<RecTMElement> elements )
        {
            if( elements == null || elements.Count == 0 )
            {
                return null;
            }
            int[] InVect = new int[elements.Count];
            // Linq is to be used
            //InVect = (int[])(from row in elements select row.ElementId);
            //int i = 0;
            //foreach (RecTMElement rec in elements)
            //{
            //  InVect[i] = rec.ElementId;
            //  i++;
            //}
            string whereIn = "";
            foreach( RecTMElement rec in elements )
            {
                whereIn += rec.ElementId.ToString() + ",";
            }
            if( whereIn.Length > 0 )
            {
                whereIn = whereIn.Substring( 0, whereIn.Length - 1 );
            }

            List<RecTMElement> transported = null;

            using( ImpactDatabase database = new ImpactDatabase() )
            {
                ImpactQuery query = new ImpactQuery()
                {
                    From = { ImpModelPlanning.As( "T1" ) },
                    Select = { ImpModelPlanning.ElementId },
                    Where = { ImpModelPlanning.Factory.Equal(factory), 
								ImpModelPlanning.Project.Equal(project), 
								ImpModelPlanning.TransportId.GreaterThan(0) 
							} //ImpModelPlanning.ElementId.In(InVect) //To be used in stead of the code below!
                };

                string statement = query.ToString();
                if( whereIn.Length > 0 )
                {
                    statement += " AND T1.ELEMENT_ID " + "IN(" + whereIn + ")";
                }

                transported = database.GetAll( statement, column => new RecTMElement()
                {
                    ElementId = DataConverter.Cast<int>( column[0] )
                } );
            }
            List<RecTMElement> result = elements;
            if( transported != null && transported.Count > 0 )
            {
                //Some of our elements in the list are already transported
                //so remove them from the incoming list
                result = new List<RecTMElement>();
                foreach( RecTMElement rec in elements )
                {
                    if( !IsTransported( rec, transported ) )
                    {
                        result.Add( rec );
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// IsTransported
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="transported"></param>
        /// <returns></returns>
        private bool IsTransported( RecTMElement rec, List<RecTMElement> transported )
        {
            foreach( RecTMElement cur in transported )
            {
                if( cur.ElementId == rec.ElementId )
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Load Element status Settings
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public List<RecElementIdStatusStd> LoadStandardSettings( RecElementIdStatusStd record )
        {
            if( record == null || record.Factory == null )
            {
                return null;
            }
            string company = record.Factory.Substring( 0, 2 ) + "00"; //Is this really OK ??!!


            // Prepare Factory, Project statement
            ImpactQuery projectQuery = new ImpactQuery()
            {
                Select = {
					        ImpElementIdStatusStd.Factory,
					        ImpElementIdStatusStd.Project,
					        ImpElementIdStatusStd.StatusId,
					        ImpElementIdStatusStd.Description,
					        ImpElementIdStatusStd.StatusType,
					        ImpElementIdStatusStd.ColorIndex,
				        },
                From = { ImpElementIdStatusStd.As( "T2" ) },
                Where = {
                            { ImpElementIdStatusStd.Factory.Equal( record.Factory )},
					        { ImpElementIdStatusStd.Project.Equal( record.Project )},
                        },
            };
            string projectStatement = projectQuery.ToString();

            // Prepare Factory, Factory statement
            ImpactQuery factoryQuery = new ImpactQuery()
            {
                Select = {
					        ImpElementIdStatusStd.Factory,
					        ImpElementIdStatusStd.Project,
					        ImpElementIdStatusStd.StatusId,
					        ImpElementIdStatusStd.Description,
					        ImpElementIdStatusStd.StatusType,
					        ImpElementIdStatusStd.ColorIndex,
				        },
                From = { ImpElementIdStatusStd.As( "T1" ) },
                Where = {
                            { ImpElementIdStatusStd.Factory.Equal( record.Factory )},
					        { ImpElementIdStatusStd.Project.Equal( record.Factory )},
                        },
            };
            string factoryStatement = factoryQuery.ToString();
            ImpactQuery projectSubquery = new ImpactQuery()
            {
                Select = { ImpElementIdStatusStd.StatusId },
                From = { ImpElementIdStatusStd.As( "T1" ) },
                Where = {
                            { ImpElementIdStatusStd.Factory.Equal( record.Factory )},
					        { ImpElementIdStatusStd.Project.Equal( record.Project )},
                        },
            };
            factoryStatement += " AND T1.Status_Id Not In (" + projectSubquery.ToString() + ")";

            // Prepare Company, Company statement
            ImpactQuery companyQuery = new ImpactQuery()
            {
                Select = {
					        ImpElementIdStatusStd.Factory,
					        ImpElementIdStatusStd.Project,
					        ImpElementIdStatusStd.StatusId,
					        ImpElementIdStatusStd.Description,
					        ImpElementIdStatusStd.StatusType,
					        ImpElementIdStatusStd.ColorIndex,
				        },
                From = { ImpElementIdStatusStd.As( "T1" ) },
                Where = {
                            { ImpElementIdStatusStd.Factory.Equal( company )},
					        { ImpElementIdStatusStd.Project.Equal( company )},
                        },
            };
            string companyStatement = companyQuery.ToString();
            ImpactQuery factorySubquery = new ImpactQuery()
            {
                Select = { ImpElementIdStatusStd.StatusId },
                From = { ImpElementIdStatusStd.As( "T1" ) },
                Where = {
                            { ImpElementIdStatusStd.Factory.Equal( record.Factory )},
					        { ImpElementIdStatusStd.Project.Equal( record.Factory )},
                        },
            };
            companyStatement += " AND T1.Status_Id Not In (" + projectSubquery.ToString() + ")"
                             + " AND T1.Status_Id Not In (" + factorySubquery.ToString() + ")";
            string statement = projectStatement + " Union " + factoryStatement + " Union " + companyStatement + " Order By T2.Status_Id";

            List<RecElementIdStatusStd> result;
            using( ImpactDatabase database = new ImpactDatabase() )
            {
                result = database.GetAll( statement, SettingParse );
            }
            return result;
        }

        /// <summary>
        /// SettingParse
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public static RecElementIdStatusStd SettingParse( DbDataReader dataReader )
        {
            var record = new RecElementIdStatusStd();
            record.Factory = DataConverter.Cast<string>( dataReader[0] );
            record.Project = DataConverter.Cast<string>( dataReader[1] );
            record.StatusId = DataConverter.Cast<int>( dataReader[2] );
            record.Description = DataConverter.Cast<string>( dataReader[3] );
            record.StatusType = DataConverter.Cast<int>( dataReader[4] );
            record.ColorIndex = DataConverter.Cast<int>( dataReader[5] );

            return record;
        }

        /// <summary>
        /// Load data
        /// </summary>
        /// <param name="rec"> </param>
        /// <returns></returns>
        public List<RecTransport> LoadTemplates( RecTMElement rec )
        {
            if( rec == null )
            {
                throw new ArgumentNullException( "RecTransportManager" );
            }
            List<RecTransport> result = new List<RecTransport>();

            using( ImpactDatabase database = new ImpactDatabase() )
            {
                ImpactQuery query = new ImpactQuery()
                {
                    From = { ImpTransport.As( "T1" ) },
                    Where =
					{
						ImpTransport.Factory.Equal( rec.Factory ),
						ImpTransport.Project.Equal( rec.Project ),
						ImpTransport.IsTemplate.Equal( true ),
						ImpTransport.IsVisible.Equal( true ),
					},
                    OrderBy = { ImpTransport.TransportId },
                };

                string statement = query.ToString();

                result = database.GetAll( statement, rdr => new RecTransport()
                {
                    Factory = DataConverter.Cast<string>( rdr["FACTORY"] ),
                    Project = DataConverter.Cast<string>( rdr["PROJECT"] ),
                    TransportStatus = DataConverter.Cast<int>( rdr["Transport_status"] ),
                    Description = DataConverter.Cast<string>( rdr["Description"] ),
                    TransportId = DataConverter.Cast<int>( rdr["Transport_Id"] ),
                    LoadDate = DataConverter.Cast<DateTime>( rdr["Load_Date"] ),
                    DeliveryDate = DataConverter.Cast<DateTime>( rdr["Delivery_Date"] ),
                    TimeInterval = DataConverter.Cast<int>( rdr["Time_Interval"] ),
                    IsTemplate = DataConverter.Cast<int>( rdr["Is_Template"] ),
                    TemplateId = DataConverter.Cast<int>( rdr["Template_Id"] ),
                    IsVisible = DataConverter.Cast<int>( rdr["Is_Visible"] ),
                } );
            }

            return result;
        }

        /// <summary>
        /// Load of Transport items
        /// </summary>
        /// <returns></returns>
        public List<RecTMTransport> LoadTransports( PlanningFilter filter, ReportFilter reportFilter )
        {
            if( filter == null )
            {
                throw new ArgumentNullException( "Transport" );
            }

            var where = WhereGroup.And( ImpTransport.Factory.Equal( filter.Factory ) );

            if( !string.IsNullOrWhiteSpace( filter.Project ) )
            {
                where.Add( ImpTransport.Project.Equal( filter.Project ) );
            }

            WhereGroup whereStatus = WhereGroup.Or( new Where[] { } );

            if( filter.NoStatus )
            {
                whereStatus.Add( ImpTransport.TransportStatus.Equal( TransportStatus.NoStatus ) );
            }
            if( filter.Planned )
            {
                whereStatus.Add( ImpTransport.TransportStatus.Equal( TransportStatus.Planned ) );
            }
            if( filter.CallOff )
            {
                whereStatus.Add( ImpTransport.TransportStatus.Equal( TransportStatus.CallOff ) );
            }
            if( filter.Delivered )
            {
                whereStatus.Add( ImpTransport.TransportStatus.Equal( TransportStatus.Delivered ) );
            }

            if( whereStatus.Count > 0 )
            {
                where.Add( whereStatus );
            }

            where.Add( ImpTransport.IsTemplate.Equal( false ) );
            WhereGroup wgDeleted = WhereGroup.Or( ImpModelGeometry.Deleted.NotEqual( 1 ),
                                                  ImpModelGeometry.Deleted.IsNull() );

            where.Add( wgDeleted );

            if( filter.UseLoadDateFrom )
            {
                where.Add( ImpTransport.LoadDate.GreaterThanOrEqual( filter.LoadDateFrom ) );
            }
            if( filter.UseLoadDateTo )
            {
                where.Add( ImpTransport.LoadDate.LessThanOrEqual( filter.LoadDateTo ) );
            }
            if( filter.UseDeliveryDateFrom )
            {
                where.Add( ImpTransport.DeliveryDate.GreaterThanOrEqual( filter.DeliveryDateFrom ) );
            }
            if( filter.UseDeliveryDateTo )
            {
                where.Add( ImpTransport.DeliveryDate.LessThanOrEqual( filter.DeliveryDateTo ) );
            }

            if( reportFilter != null && reportFilter.Ranges.Count > 0 )
            {
                var list = new List<Where>();
                foreach( var range in reportFilter.Ranges )
                {
                    if( !string.IsNullOrEmpty( range.From ) && !string.IsNullOrEmpty( range.To ) )
                    {
                        list.Add( ImpTransport.TransportId.Between( range.From, range.To ) );
                    }
                }

                if( list.Count > 0 )
                {
                    where.Add( WhereGroup.Or( list.ToArray() ) );
                }
            }

            where.Add( ImpTransport.TransportId.GreaterThan( 0 ) );

            ImpactQuery query = new ImpactQuery()
            {
                Select =
				{
					ImpTransport.Project,
					ImpTransport.TransportId,
					ImpTransport.Description,
					ImpTransport.LoadDate,
					ImpTransport.DeliveryDate,
					ImpTransport.TransportStatus,

					ImpTransportVehicle.VehicleId,
					ImpTransportVehicleStd.Name,
					ImpTransportVehicleStd.MaxMass,
					ImpTransportVehicle.TransportType,
					ImpTransportVehicleStd.VehicleType,

					ImpTransportStack.StackId,
					//ImpTransportStack.Rack,
					ImpTransportStack.MaxMass,
					ImpTransportStack.Description,
					ImpTransportStack.StackType,


					ImpModelPlanning.ElementId,
					ImpModelPlanning.ErectionSequenceNo,
					ImpModelPlanning.StackSequenceNo,
					ImpModelPlanning.StackId,
                    ImpModelPlanning.ElementIdStatus,
                    ImpModelPlanning.PlannedDeliveryDate,
                    ImpModelPlanning.DeliveryDate,

                    ImpModelGeometry.ElementMark,
					ImpModelGeometry.Building,
                    ImpModelGeometry.FloorId,
                    ImpModelGeometry.Phase,

					ImpElement.ElementType,
					ImpElement.Product,
                    ImpElement.Style,
					ImpElement.Mass,
					ImpElement.ElementLength,
					ImpElement.ElementWidth,
					ImpElement.ElementHeight,
					ImpElement.GrossArea,
                    ImpElement.NetArea,
				},
                //ELEMENT_TYPE, STYLE  BOUNDING_BOX_AREA 
                From = { ImpTransport.As( "TRA" ) },

                Join =
				{
					Join.Left( ImpTransportVehicle.As( "TRV" ),	
						ImpTransport.Factory.Equal( ImpTransportVehicle.Factory ),
						ImpTransport.Project.Equal( ImpTransportVehicle.Project ),
						ImpTransport.TransportId.Equal( ImpTransportVehicle.TransportId ) ),

					Join.Left( ImpTransportVehicleStd.As( "VST" ),	
						ImpTransport.Factory.Equal( ImpTransportVehicleStd.Factory ),
						ImpTransport.Factory.Equal( ImpTransportVehicleStd.Project ),  // Factory, factory only so far (Needs to be complated with company, company)
						ImpTransportVehicle.Vehicle.Equal( ImpTransportVehicleStd.Name ) ),

					Join.Left( ImpTransportStack.As( "RCK" ),	
						ImpTransport.Factory.Equal( ImpTransportStack.Factory ),
						ImpTransport.Project.Equal( ImpTransportStack.Project ),
						ImpTransport.TransportId.Equal( ImpTransportStack.TransportId ),
						ImpTransportVehicle.VehicleId.Equal( ImpTransportStack.VehicleId )),

					Join.Left( ImpModelPlanning.As( "MPL" ),	
						ImpTransport.Factory.Equal( ImpModelPlanning.Factory ),
						ImpTransport.Project.Equal( ImpModelPlanning.Project ),
						ImpTransport.TransportId.Equal( ImpModelPlanning.TransportId ),
						ImpTransportVehicle.VehicleId.Equal( ImpModelPlanning.VehicleId ) ,
						ImpTransportStack.StackId.Equal( ImpModelPlanning.StackId ) ),

					Join.Left( ImpModelGeometry.As( "MGO" ),	
						ImpModelPlanning.Factory.Equal( ImpModelGeometry.Factory ),
						ImpModelPlanning.Project.Equal( ImpModelGeometry.Project ),
						ImpModelPlanning.ElementId.Equal( ImpModelGeometry.ElementId ) ),

					Join.Left( ImpElement.As( "ELM" ),	
						ImpModelGeometry.Factory.Equal( ImpElement.Factory ),
						ImpModelGeometry.Project.Equal( ImpElement.Project ),
						ImpModelGeometry.ElementMark.Equal( ImpElement.ElementMark ) ),
				},

                Where = { where },

                OrderBy = 
				{ 
					{ ImpTransport.TransportId, OrderBy.Descending },
					{ ImpTransportVehicle.VehicleId }, 
					{ ImpTransportStack.StackId },
					{ ImpModelPlanning.StackSequenceNo, OrderBy.Descending }
				},
            };

            string statement = query.ToString();

            List<RecTMTransport> tmList = new List<RecTMTransport>();
            TranportParser parser = new TranportParser( tmList );

            using( ImpactDatabase database = new ImpactDatabase() )
            {
                var list = database.GetAll( statement, column =>
                    {
                        string project = DataConverter.Cast<string>( column[0] );
                        int transportId = DataConverter.Cast<int>( column[1] );

                        return new
                        {
                            // TODO: THEIB, you must handle null dates in your record class.
                            transport = new RecTransport()
                            {
                                Factory = filter.Factory,
                                Project = project,
                                TransportId = transportId,
                                Description = DataConverter.Cast<string>( column[2] ),
                                LoadDate = DataConverter.Cast<DateTime>( column[3] ),
                                DeliveryDate = DataConverter.Cast<DateTime?>( column[4] ),
                                TransportStatus = DataConverter.Cast<int>( column[5] ),
                            },
                            veh = new RecTransportVehicleStd()
                            {
                                Factory = filter.Factory,
                                Project = project,
                                VehicleId = DataConverter.Cast<int?>( column[6] ) ?? 0,
                                Name = DataConverter.Cast<string>( column[7] ),
                                MaxMass = DataConverter.Cast<double?>( column[8] ) ?? 0d,
                                TransportId = transportId,
                                TransportType = DataConverter.Cast<int?>( column[9] ) ?? 0,
                                VehicleType = DataConverter.Cast<int?>( column[10] ) ?? 0
                            },
                            stack = new RecTransportVehicleStackStd()
                            {
                                StackId = DataConverter.Cast<int?>( column[11] ) ?? 0,
                                MaxMass = DataConverter.Cast<double?>( column[12] ) ?? 0,
                                Description = DataConverter.Cast<string>( column[13] ),
                                StackType = DataConverter.Cast<int?>( column[14] ) ?? 0
                            },
                            elem = new RecTMElement()
                            {
                                Factory = filter.Factory,
                                Project = project,

                                ElementId = DataConverter.Cast<int?>( column[15] ) ?? 0,
                                ErectionSequenceNo = DataConverter.Cast<int?>( column[16] ) ?? 0,
                                StackSequenceNo = DataConverter.Cast<int?>( column[17] ) ?? 0,
                                StackId = DataConverter.Cast<int?>( column[18] ) ?? 0,

                                ElementIdStatus = DataConverter.Cast<int?>( column[19] ) ?? 0,
                                PlannedDeliveryDate = DataConverter.Cast<DateTime?>( column[20] ),
                                DeliveryDate = DataConverter.Cast<DateTime?>( column[21] ),

                                ElementMark = DataConverter.Cast<string>( column[22] ),
                                Building = DataConverter.Cast<string>( column[23] ),
                                FloorId = DataConverter.Cast<int?>( column[24] ) ?? 0,
                                Phase = DataConverter.Cast<string>( column[25] ),

                                ElementType = DataConverter.Cast<string>( column[26] ),
                                Product = DataConverter.Cast<string>( column[27] ),
                                Style = DataConverter.Cast<string>( column[28] ),
                                Mass = DataConverter.Cast<double?>( column[29] ) ?? 0d,
                                ElementLength = DataConverter.Cast<double?>( column[30] ) ?? 0d,
                                ElementWidth = DataConverter.Cast<double?>( column[31] ) ?? 0d,
                                ElementHeight = DataConverter.Cast<double?>( column[32] ) ?? 0d,
                                GrossArea = DataConverter.Cast<double?>( column[33] ) ?? 0d,
                                NetArea = DataConverter.Cast<double?>( column[34] ) ?? 0d,
                            }
                        };
                    } );

                foreach( var item in list )
                {
                    parser.Parse( item.transport, item.veh, item.stack, item.elem );
                }
            }

            return tmList;
        }
        /// <summary>
        /// Swaps elements erection and load sequence
        /// </summary>
        /// <param name="rec1"></param>
        /// <param name="rec2"></param>
        /// <returns></returns>
        public bool SwapElementSequence( RecTMElement rec1, RecTMElement rec2 )
        {
            if( rec1 == null || rec2 == null )
            {
                return false;
            }
            // Create a temp copy
            RecTMElement recTemp = new RecTMElement( rec1 );

            ModelPlanner svc = new ModelPlanner();
            // Update the rec1
            rec1.StackSequenceNo = rec2.StackSequenceNo;
            svc.SaveElementTransport( rec1, false );
            // Update the rec2
            rec2.StackSequenceNo = recTemp.StackSequenceNo;
            svc.SaveElementTransport( rec2, false );

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="vehicle"></param>
        /// <param name="rack"></param>
        /// <param name="elements"></param>
        public int TransportElements( List<RecTMElement> elements )
        {
            // Since all the element in the list are to be transported
            // by the same vehicle stack then use the first element vehicle stack info
            if( elements == null || elements.Count == 0 )
            {
                return 0;
            }
            foreach( RecTMElement elem in elements )
            {
                new TMElement( elem ).Save();
            }
            return elements.Count;
        }

        bool Find( RecTMElement newElem, List<RecTMElement> elements )
        {
            foreach( RecTMElement elem in elements )
            {
                if( newElem.ElementId == elem.ElementId )
                {
                    return true;
                }
            }
            return false;
        }
        RecTransportVehicleStd GetStandardVehicle( RecTMElement rec )
        {
            ProjectManager svc = new ProjectManager();
            RecTransport recTrans = new RecTransport();
            recTrans.Factory = rec.Factory;
            recTrans.Project = rec.Project;
            recTrans.TransportId = rec.TransportId;
            List<RecTransportVehicleStd> vehList = svc.LoadTransportVehiclesEx( recTrans, rec.VehicleId );
            if( vehList != null && vehList.Count > 0 )
            {
                return vehList[0];
            }
            return null;
        }
    }

    /// <summary>
    /// Load of Transport Vehicle Standard items
    /// </summary>
    /// <param name="vehicle"></param>
    /// <returns></returns>
    /// <summary>
    /// A helper parsing class
    /// </summary>
    class TranportParser
    {
        Dictionary<string, RecTMTransport> dic;
        List<RecTMTransport> tmList = null;
        public TranportParser( List<RecTMTransport> lst )
        {
            tmList = lst;
            dic = new Dictionary<string, RecTMTransport>();
        }
        public List<RecTMTransport> GetTransportList()
        {
            return tmList;
        }
        /// <summary>
        /// Parsing method
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="vehicle"></param>
        /// <param name="rack"></param>
        /// <param name="elem"></param>
        public void Parse( RecTransport transport, RecTransportVehicleStd vehicle, RecTransportVehicleStackStd stack, RecTMElement elem )
        {
            string transpKey = transport.Factory + transport.Project + transport.TransportId;
            RecTMVehicle vehObj = null;
            RecTMStack stackObj = null;
            RecTMTransport transpObj = null;
            if( dic.ContainsKey( transpKey ) )
            {
                transpObj = dic[transpKey];
            }
            else
            {
                transpObj = new RecTMTransport();
                dic.Add( transpKey, transpObj );
                tmList.Add( transpObj );
            }
            transpObj.TransportObject = transport;

            bool isVehicleMode = vehicle.VehicleId != 0;
            if( isVehicleMode || transpObj.Vehicles.Count > 0 )
            {
                vehObj = transpObj.FindVehicle( vehicle );
                if( vehObj == null )
                {
                    vehObj = new RecTMVehicle();
                    transpObj.AddVehicle( vehObj );
                }
                vehObj.TransportVehicleStdObject = vehicle;
                if( stack.StackId != 0 )
                {
                    stackObj = vehObj.FindStack( stack );
                    if( stackObj == null )
                    {
                        stackObj = new RecTMStack();
                        vehObj.AddStack( stackObj );
                    }
                    stackObj.TransportVehicleStackStdObject = stack;
                    // To be revised!!!!!!!!!!! rackObj.TransportVehicleStackStdObject = stack;
                }
            }
            // Save the element if we do have an element
            if( elem.ElementId > 0 )
            {
                if( stackObj != null )
                {
                    stackObj.TMElements.Add( elem );
                }
                else if( vehObj != null )
                {
                    vehObj.TMElements.Add( elem );
                }
                else
                {
                    transpObj.TMElements.Add( elem );
                }
            }
        }
    }
}


