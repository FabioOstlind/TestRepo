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
	/// <summary>
	/// Transport Business logic service
	/// </summary>
	public partial class ProjectManager : ITransport
	{
		/// <summary>
		/// Load data, returns transport templates only
		/// </summary>
		/// <param name="transport"></param>
		/// <returns></returns>
		public List<RecTransport> LoadTransport( RecTransport record )
		{
			if( record == null )
			{
				throw new ArgumentNullException( "Transport" );
			}
			string project = "";
			if( record.IsTemplate == 1 )
			{
				// Templates are saved on factory level (factory, factory), 2012-04-23
				project = record.Factory;
			}
			else
			{
				project = record.Project;
			}
			ImpactQuery query = new ImpactQuery()
			{
				Select =
				{
					ImpTransport.TransportId,
					ImpTransport.Description,
					ImpTransport.LoadLocation,
					ImpTransport.LoadDate,
					ImpTransport.DeliveryDate,
					ImpTransport.TimeInterval,
					ImpTransport.LoadDivision,
					ImpTransport.DeliveryDivision,
					ImpTransport.IsTemplate,
					ImpTransport.TemplateId,
					ImpTransport.IsVisible,
					ImpTransport.TransportStatus
				},
				From = { ImpTransport.As( "T1" ) },
				Where = { ImpTransport.Factory.Equal( record.Factory ), 
						  ImpTransport.Project.Equal( project ), // Templates are saved on factory level (factory, factory), 2012-04-23
						  ImpTransport.IsTemplate.Equal( record.IsTemplate )},
				//OrderBy = { 
				//            {ImpTransport.TransportId, OrderBy.Descending} // gives Asc !! to be corrected
				//          }
			};

			string statement = query.ToString();
			statement += " ORDER BY T1.TRANSPORT_ID DESC";

			List<RecTransport> result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.GetAll( statement, column =>
				{
					return new RecTransport()
					{
						Factory = record.Factory,
						Project = record.Project,

						TransportId = DataConverter.Cast<int>( column[0] ),
						Description = DataConverter.Cast<string>( column[1] ),
						LoadLocation = DataConverter.Cast<string>( column[2] ),
						LoadDate = DataConverter.Cast<DateTime>( column[3] ),
						DeliveryDate = DataConverter.Cast<DateTime>( column[4] ),
						TimeInterval = DataConverter.Cast<int>( column[5] ),
						LoadDivision = DataConverter.Cast<string>( column[6] ),
						DeliveryDivision = DataConverter.Cast<string>( column[7] ),
						IsTemplate = DataConverter.Cast<int>( column[8] ),
						TemplateId = DataConverter.Cast<int>( column[9] ),
						IsVisible = DataConverter.Cast<int>( column[10] ),
						TransportStatus = DataConverter.Cast<int>( column[11] ),
					};
				} );
			}

			return result;
		}
		/// <summary>
		/// Bulk Delete of Transport items
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public int BulkDeleteCascadeTransport( List<RecTransport> list )
		{
			if( list == null || list.Count == 0 )
			{
				return 0;
			}
			foreach( RecTransport transport in list )
			{
				this.DeleteCascadeTransport( transport );
			}
			return list.Count;
		}
		/// <summary>
		/// Delete of Transport items
		/// </summary>
		/// <param name="transport"></param>
		/// <returns></returns>
		public int DeleteCascadeTransport( RecTransport transport )
		{
			if( transport == null || transport.TransportId == 0 )
			{
				throw new ArgumentNullException( "RecTransport" );
			}
			string project = "";
			if( transport.IsTemplate == 1 )
			{
				// Templates are saved on factory level (factory, factory), 2012-04-23
				project = transport.Factory;
			}
			else
			{
				project = transport.Project;
			}
			// Delete cascade vehicles
			ProjectManager vehSvc = new ProjectManager();
			List<RecTransportVehicleStd> vehicles = vehSvc.LoadTransportVehicles( transport );
			foreach( RecTransportVehicleStd veh in vehicles )
			{
				this.DeleteCascadeVehicleTransport( transport, veh );
			}

			int ret = 0;
			// Now let's delete the transport 
			using( ImpactDatabase database = new ImpactDatabase() )
			{
				ImpactDelete delete = new ImpactDelete( ImpTransport.Instance )
				{
					Where = { ImpTransport.Factory.Equal( transport.Factory ), 
							  ImpTransport.Project.Equal( project ),// Templates are saved on factory level (factory, factory), 2012-04-23
							  ImpTransport.TransportId.Equal( transport.TransportId )},
				};
				string statement = delete.ToString();
				ret = database.ExecuteNonQuery( statement );
			}
			return ret;
		}

		public int DeleteCascadeVehicleTransport( RecTransport transport, RecTransportVehicleStd veh )
		{
			RecTransportStack rec = new RecTransportStack();
			rec.Factory = transport.Factory;
			rec.Project = transport.Project;
			rec.TransportId = transport.TransportId;
			rec.VehicleId = veh.VehicleId;

			// Delete cascade stacks
			ProjectManager svc = new ProjectManager();
			List<RecTransportStack> stacks = svc.LoadTransportStack( rec );
			foreach( RecTransportStack stack in stacks )
			{
				this.DeleteCascadeStackTransport( transport, veh, stack );
			}

			// Now delete the vehicle
			ProjectManager vehSvc = new ProjectManager();
			List<RecTransportVehicleStd> vehicles = new List<RecTransportVehicleStd>();
			vehicles.Add( veh );
			return vehSvc.RemoveTransportVehicles( transport, vehicles );
		}

        /// <summary>
        /// Delete Cascade Stack
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="veh"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
		public int DeleteCascadeStackTransport( RecTransport transport, RecTransportVehicleStd veh, RecTransportStack stack )
		{
            ProjectManager mgr = new ProjectManager();
            RecElementIdStatusStd std = new RecElementIdStatusStd { Factory = transport.Factory, Project = transport.Project };
            List<RecElementIdStatusStd> settings = mgr.LoadStandardSettings( std );

            // Update Status, Note this can be optimized!
            RecTransport recTransportStatus = new RecTransport( transport );
            recTransportStatus.TransportStatus = (int)TransportStatus.NoStatus;
            recTransportStatus.ElementIdStatus = RecElementIdStatusStd.GetLocalSettingFromGlobalId( settings, recTransportStatus.TransportStatus).StatusId;
            UpdateStatus( transport, recTransportStatus, settings );
            // No reset
			ModelPlanner svc = new ModelPlanner();
			svc.ResetTransport( transport.Factory, transport.Project, 0, transport.TransportId, veh.VehicleId, stack.StackId );

			ProjectManager StackSvc = new ProjectManager();
			return StackSvc.DeleteTransportStack( stack );
		}

		/// <summary>
		/// Bulk Update of Transport items
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public int BulkUpdateTransport( List<RecTransport> list )
		{
			if( list == null || list.Count == 0 )
			{
				return 0;
			}
			foreach( RecTransport transport in list )
			{
				this.UpdateTransport( transport );
			}
			return list.Count;
		}
		/// <summary>
		/// Update of Transport items
		/// </summary>
		/// <param name="transport"></param>
		/// <returns></returns>
		public int UpdateTransport( RecTransport record )
		{
			if( record == null )
			{
				throw new ArgumentNullException( "Transport" );
			}
			string project = "";
			if( record.IsTemplate == 1 )
			{
				// Templates are saved on factory level (factory, factory), 2012-04-23
				project = record.Factory;
			}
			else
			{
				project = record.Project;
			}

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				ImpactUpdate update = new ImpactUpdate( ImpTransport.Instance )
				{
					Columns = 
					{
 						{ ImpTransport.Description, record.Description },
 						{ ImpTransport.LoadDate, record.LoadDate },
 						{ ImpTransport.DeliveryDate, record.DeliveryDate },
 						{ ImpTransport.TimeInterval, record.TimeInterval },
 						{ ImpTransport.IsTemplate, record.IsTemplate },
 						{ ImpTransport.TemplateId, record.TemplateId },
 						{ ImpTransport.IsVisible, record.IsVisible },
					},
					Where =
					{
						ImpTransport.Factory.Equal( record.Factory ),
						ImpTransport.Project.Equal( project ),// Templates are saved on factory level (factory, factory), 2012-04-23
						ImpTransport.TransportId.Equal( record.TransportId ),
					}
				};

				string sql = update.ToString();

				database.ExecuteNonQuery( sql );
			}
			return 0;
		}

        /// <summary>
        /// Update the transport and elements with new status
        /// </summary>
        /// <param name="transports"></param>
        /// <param name="recTransportStatus"></param>
        /// <returns></returns>
        public int UpdateStatusTransport( List<RecTransport> transports, RecTransport recTransportStatus )
        {
            if( null == transports || transports.Count == 0 )
            {
                return 0;
            }

            ProjectManager mgr = new ProjectManager();
            RecElementIdStatusStd std = new RecElementIdStatusStd { Factory = recTransportStatus.Factory, Project = recTransportStatus.Project };
            List<RecElementIdStatusStd> settings = mgr.LoadStandardSettings( std );

            foreach( RecTransport recTransport in transports )
            {
                UpdateStatus( recTransport, recTransportStatus, settings );
            }

            return 0;
        }

        /// <summary>
        /// Update the transport and elements with new status
        /// </summary>
        /// <param name="transports"></param>
        /// <param name="recTransportStatus"></param>
        /// <returns></returns>
        public int UpdateStatus( RecTransport record, RecTransport recTransportStatus, List<RecElementIdStatusStd> settings )
        {
            if( null == record || null == recTransportStatus )
            {
                return 0;
            }

            ModelPlanner plannerService = new ModelPlanner();
            recTransportStatus.Factory = record.Factory;
            recTransportStatus.Project = record.Project;
            recTransportStatus.TransportId = record.TransportId;
            plannerService.SaveElementTransportStatus( recTransportStatus, settings );

            int ret = 0;
            using( ImpactDatabase database = new ImpactDatabase() )
            {
                ImpactUpdate update = new ImpactUpdate( ImpTransport.Instance )
                {
                    Columns = 
					{
 						{ ImpTransport.TransportStatus, recTransportStatus.TransportStatus },
					},
                    Where =
					{
						ImpTransport.Factory.Equal( record.Factory ),
						ImpTransport.Project.Equal( record.Project ),
						ImpTransport.TransportId.Equal( record.TransportId ),
					}
                };
                
                bool b1 = recTransportStatus.TransportStatus == (int)TransportStatus.Planned && recTransportStatus.UpdateDeliveryDate && null != recTransportStatus.DeliveryDate;
                bool b2 = recTransportStatus.TransportStatus == (int)TransportStatus.Delivered && recTransportStatus.UpdateDeliveryDate && null != recTransportStatus.DeliveryDate;

                if( b1 || b2 )
                {
                    update.Columns.Add( ImpTransport.DeliveryDate, recTransportStatus.DeliveryDate );
                }

                string sql = update.ToString();

                ret = database.ExecuteNonQuery( sql );
            }
            return ret;
        }

		/// <summary>
		/// Returns max value of transport_id
		/// </summary>
		/// <param name="transport"></param>
		/// <returns></returns>
		public int GetMaxTransportId( RecTransport record )
		{
			if( record == null )
			{
				throw new ArgumentNullException( "Transport" );
			}
			int ret = 0;
			using( ImpactDatabase database = new ImpactDatabase() )
			{
				ImpactQuery query = new ImpactQuery()
				{
					Select =
					{
						 Aggregate.Max(ImpTransport.TransportId)
					},
					From = { ImpTransport.As( "T1" ) },
					Where = { ImpTransport.Factory.Equal( record.Factory ), 
							  ImpTransport.Project.Equal( record.Project ),
							  ImpTransport.TransportId.GreaterThan( 0 )},
				};
				string statement = query.ToString();
				List<int> result = null;
				try
				{
					result = database.GetAll( statement, column =>
					{
						return DataConverter.Cast<int?>( column[0] ) ?? 0;
					} );
					ret = result[0];
				}
				catch( Exception ) { }//Just eat it please!
			}
			return ret;
		}
		/// <summary>
		/// Insert of Transport items
		/// </summary>
		/// <param name="transport"></param>
		/// <returns></returns>
		public RecTransport InsertTransport( RecTransport record )
		{
			if( record == null )
			{
				throw new ArgumentNullException( "Transport" );
			}
			RecTransport newTransport = new RecTransport( record );
			string project = "";
			if( record.IsTemplate == 1 )
			{
				//Secure and easy solution for unique sequence id's!!!
				// Use negative value to distinguish templates from transports!
				ProjectManager ng = new ProjectManager();
				newTransport.TransportId = -ng.GetNextNumber( record.Factory, record.Project, RecNumberGenerator.NG_TRANSPORT_ID );

				// Templates are saved on factory level (factory, factory), 2012-04-23
				project = record.Factory;
			}
			else
			{
				//Now let's use the slow, unsecure and bad unique id's using max, what a mess :(
				newTransport.TransportId = GetMaxTransportId( record ) + 1;
				project = record.Project;
			}

			var insert = new ImpactInsert( ImpTransport.Instance )
			{
				Columns = 
				{
					{ ImpTransport.Factory, record.Factory },
					{ ImpTransport.Project, project },// Templates are saved on factory level (factory, factory), 2012-04-23
					{ ImpTransport.TransportId, newTransport.TransportId }, // The new Id!
					{ ImpTransport.Description, record.Description },
					{ ImpTransport.LoadDate, record.LoadDate },
					{ ImpTransport.DeliveryDate, record.DeliveryDate },
					{ ImpTransport.IsTemplate, record.IsTemplate },
					{ ImpTransport.IsVisible, record.IsVisible },
					{ ImpTransport.TemplateId, record.TemplateId },
					{ ImpTransport.TimeInterval, record.TimeInterval },
					{ ImpTransport.TransportStatus, record.TransportStatus },
					{ ImpTransport.LoadLocation, record.LoadLocation },
					{ ImpTransport.LoadDivision, record.LoadDivision },
					{ ImpTransport.DeliveryDivision, record.DeliveryDivision },
				}
			};

			string statement = insert.ToString();

			int result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.ExecuteNonQuery( statement );
			}

			return newTransport;
		}
	}
}
