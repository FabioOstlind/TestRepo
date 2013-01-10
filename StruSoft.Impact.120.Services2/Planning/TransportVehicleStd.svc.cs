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
	/// Transport Vehicle Standard Business logic service
	/// </summary>
	public partial class ProjectManager : ITransportVehicleStd
	{
		/// <summary>
		/// Load of Transport Vehicle Standard items 
		/// </summary>
		/// <param name="vehicle"></param>
		/// <returns></returns>
		public List<RecTransportVehicleStd> LoadTransportVehicleStd( RecTransportVehicleStd record )
		{
			if( record  == null )
			{
				throw new ArgumentNullException( "TransportVehicleStd" );
			}
			ImpactQuery query = new ImpactQuery()
			{
				Select =
				{
					ImpTransportVehicleStd.Factory,
					ImpTransportVehicleStd.Project, 
					ImpTransportVehicleStd.Name, 

					ImpTransportVehicleStd.Division, 
					ImpTransportVehicleStd.Description, 
					ImpTransportVehicleStd.VehicleType, 
					ImpTransportVehicleStd.TransportType, 

					ImpTransportVehicleStd.Length, 
					ImpTransportVehicleStd.Width, 
					ImpTransportVehicleStd.Height, 
					ImpTransportVehicleStd.Mass,

					ImpTransportVehicleStd.MaxLength, 
					ImpTransportVehicleStd.MaxWidth, 
					ImpTransportVehicleStd.MaxHeight, 
					ImpTransportVehicleStd.MaxMass, 

					ImpTransportVehicleStd.CreatedBy, 
					ImpTransportVehicleStd.CreatedDate, 
					ImpTransportVehicleStd.ChangedBy, 
					ImpTransportVehicleStd.ChangedDate, 
				},
				From = { ImpTransportVehicleStd.As( "T1" ) },
				Where = { ImpTransportVehicleStd.Factory.Equal( record.Factory ), 
						  ImpTransportVehicleStd.Project.Equal( record.Factory ),//for Std use Factory, Factory
						  },
				OrderBy = {ImpTransportVehicleStd.Name}
			};

			string statement = query.ToString();

			List<RecTransportVehicleStd> result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.GetAll( statement, ParseTransportVehicleStd );
			}

			return result;
		}
		public static RecTransportVehicleStd ParseTransportVehicleStd( DbDataReader dataReader )
		{
			var record = new RecTransportVehicleStd();
			record.Factory = DataConverter.Cast<string>( dataReader[0] );
			record.Project = DataConverter.Cast<string>( dataReader[1] ); 
			record.Name = DataConverter.Cast<string>( dataReader[2] ); 

			record.Division = DataConverter.Cast<string>( dataReader[3] ); 
			record.Description = DataConverter.Cast<string>( dataReader[4] ); 
			record.VehicleType = DataConverter.Cast<int>( dataReader[5] ); 
			record.TransportType = DataConverter.Cast<int>( dataReader[6] ); 

			record.Length = DataConverter.Cast<double>( dataReader[7] ); 
			record.Width = DataConverter.Cast<double>( dataReader[8] ); 
			record.Height = DataConverter.Cast<double>( dataReader[9] ); 
			record.Mass = DataConverter.Cast<double>( dataReader[10] );

			record.MaxLength = DataConverter.Cast<double>( dataReader[11] ); 
			record.MaxWidth = DataConverter.Cast<double>( dataReader[12] ); 
			record.MaxHeight = DataConverter.Cast<double>( dataReader[13] ); 
			record.MaxMass = DataConverter.Cast<double>( dataReader[14] ); 

			record.Created_By = DataConverter.Cast<string>( dataReader[15] ); 
			record.Created_Date = DataConverter.Cast<DateTime?>( dataReader[16] ); 
			record.Changed_By = DataConverter.Cast<string>( dataReader[17] ); 
			record.Changed_Date = DataConverter.Cast<DateTime?>( dataReader[18] ); 
			return record;
		}
		/// <summary>
		/// Bulk Delete of Transport Vehicle Standard items
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public int BulkDeleteTransportVehicleStd( List<RecTransportVehicleStd> list )
		{
			if( list == null || list.Count == 0 )
			{
				return 0;
			}
			foreach( RecTransportVehicleStd vehicle in list )
			{
				this.DeleteTransportVehicleStd( vehicle );
			}
			return list.Count;
		}
		/// <summary>
		/// Delete of Transport Vehicle Standard items
		/// </summary>
		/// <param name="vehicle"></param>
		/// <returns></returns>
		public int DeleteTransportVehicleStd( RecTransportVehicleStd record )
		{
			if( record == null )
			{
				throw new ArgumentNullException( "TransportVehicleStd" );
			}
			// Delete underlying std stacks
			ProjectManager svc = new ProjectManager();
			RecTransportVehicleStackStd stack = new RecTransportVehicleStackStd()
			{ 
				Factory = record.Factory,
				Project = record.Project,
				Name = record.Name, 
			};
			svc.DeleteTransportVehicleStackStd( stack );
 
			// Now delete the std veh
			var delete = new ImpactDelete( ImpTransportVehicleStd.Instance )
			{
				Where = 
				{
					{ ImpTransportVehicleStd.Factory.Equal( record.Factory )},
					{ ImpTransportVehicleStd.Project.Equal( record.Factory )},//for Std use Factory, Factory
					{ ImpTransportVehicleStd.Name.Equal( record.Name )},
				}
			};

			string statement = delete.ToString();

			int result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.ExecuteNonQuery( statement );
			}

			return result;
		}
		/// <summary>
		/// Bulk Update of Transport Vehicle Standard items
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public int BulkUpdateTransportVehicleStd( List<RecTransportVehicleStd> list )
		{
			if( list == null || list.Count == 0 )
			{
				return 0;
			}
			foreach( RecTransportVehicleStd vehicle in list )
			{
				this.UpdateTransportVehicleStd( vehicle );
			}
			return list.Count;
		}
		/// <summary>
		/// Update of Transport Vehicle Standard items
		/// </summary>
		/// <param name="vehicle"></param>
		/// <returns></returns>
		public int UpdateTransportVehicleStd( RecTransportVehicleStd record )
		{
			if( record == null )
			{
				throw new ArgumentNullException( "TransportVehicleStd" );
			}
			var update = new ImpactUpdate( ImpTransportVehicleStd.Instance )
			{
				Columns = 
				{
					{ ImpTransportVehicleStd.Description, record.Description },
					{ ImpTransportVehicleStd.VehicleType, record.VehicleType },
					{ ImpTransportVehicleStd.TransportType, record.TransportType },
					{ ImpTransportVehicleStd.Length, record.Length },
					{ ImpTransportVehicleStd.Width, record.Width },
					{ ImpTransportVehicleStd.Height, record.Height },
					{ ImpTransportVehicleStd.Mass, record.Mass },
					{ ImpTransportVehicleStd.MaxLength, record.MaxLength },
					{ ImpTransportVehicleStd.MaxWidth, record.MaxWidth },
					{ ImpTransportVehicleStd.MaxHeight, record.MaxHeight },
					{ ImpTransportVehicleStd.MaxMass, record.MaxMass },
					{ ImpTransportVehicleStd.Division, record.Division },
					{ ImpTransportVehicleStd.ChangedBy, record.Changed_By },
					{ ImpTransportVehicleStd.ChangedDate, record.Changed_Date },
				},
				Where = 
				{
					{ ImpTransportVehicleStd.Factory.Equal( record.Factory ) },
					{ ImpTransportVehicleStd.Project.Equal( record.Factory ) },//for Std use Factory, Factory
					{ ImpTransportVehicleStd.Name.Equal( record.Name ) },
				},
			};

			string statement = update.ToString();

			int result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.ExecuteNonQuery( statement );
			}

			return result;
		}
		/// <summary>
		///  Insert of Transport Vehicle Standard items
		/// </summary>
		/// <param name="vehicle"></param>
		/// <returns></returns>
		public int InsertTransportVehicleStd( RecTransportVehicleStd record )
		{
			if( record == null )
			{
				throw new ArgumentNullException( "TransportVehicleStd" );
			}
			var insert = new ImpactInsert( ImpTransportVehicleStd.Instance )
			{
				Columns = 
				{
					{ ImpTransportVehicleStd.Factory, record.Factory },
					{ ImpTransportVehicleStd.Project, record.Factory }, //Std use, Factory, Factory
					{ ImpTransportVehicleStd.Name, record.Name },

					{ ImpTransportVehicleStd.Division, record.Division },
					{ ImpTransportVehicleStd.Description, record.Description },
					{ ImpTransportVehicleStd.VehicleType, record.VehicleType },
					{ ImpTransportVehicleStd.TransportType, record.TransportType },

					{ ImpTransportVehicleStd.Length, record.Length },
					{ ImpTransportVehicleStd.Width, record.Width },
					{ ImpTransportVehicleStd.Height, record.Height },
					{ ImpTransportVehicleStd.Mass, record.Mass },

					{ ImpTransportVehicleStd.MaxLength, record.MaxLength },
					{ ImpTransportVehicleStd.MaxWidth, record.MaxWidth },
					{ ImpTransportVehicleStd.MaxHeight, record.MaxHeight },
					{ ImpTransportVehicleStd.MaxMass, record.MaxMass },

					{ ImpTransportVehicleStd.CreatedBy, record.Created_By },
					{ ImpTransportVehicleStd.CreatedDate, record.Created_Date },
					//{ ImpTransportVehicleStd.ChangedBy, record.Created_By },
					//{ ImpTransportVehicleStd.ChangedDate, record.Created_Date },
				}
			};
			string statement = insert.ToString();
			int result;
			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.ExecuteNonQuery( statement );
			}

			return result;
		}
		/// <summary>
		///  Insert of Transport Vehicle Standard items
		/// </summary>
		/// <param name="rec"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public List<RecTransportVehicleStd> InsertTransportVehicles( RecTransport rec, List<RecTransportVehicleStd> list )
		{
			if( rec == null || list == null || list.Count == 0 )
			{
				return null;
			}
			List<RecTransportVehicleStd> res = new List<RecTransportVehicleStd>();
			int seq = 1;
			foreach( RecTransportVehicleStd vehicle in list )
			{
				res.Add( InsertTransportVehicle( rec, vehicle, seq ) );
				seq++;
			}

			return res;
		}

		/// <summary>
		/// Insert of Transport Vehicle Standard items
		/// </summary>
		/// <param name="rec"></param>
		/// <param name="vehicle"></param>
		/// <param name="seq"></param>
		/// <returns></returns>
		public RecTransportVehicleStd InsertTransportVehicle( RecTransport rec, RecTransportVehicleStd vehicle, int seq )
		{
			if( rec == null || rec.Factory == null || rec.Project == null )
			{
				throw new ArgumentNullException( "Transport" );
			}
			string project = "";
			if( rec.IsTemplate == 1 )
			{
				// Templates are saved on factory level (factory, factory), 2012-04-23
				project = rec.Factory;
			}
			else
			{
				project = rec.Project;
			}
			RecTransportVehicleStd res = new RecTransportVehicleStd( vehicle );
			using( ImpactDatabase database = new ImpactDatabase() )
			{
				// 1. Instantiate a new command with a query and connection
				ProjectManager ng = new ProjectManager();
				int vehicleId = ng.GetNextNumber( rec.Factory, rec.Project, RecNumberGenerator.NG_VEHICLE_ID );

				string sql = "Insert into IMP_TRANSPORT_VEHICLE(FACTORY,PROJECT,TRANSPORT_ID,VEHICLE_ID,VEHICLE,VEHICLE_SEQUENCE_NO,TRANSPORT_TYPE)values("
										+ Conv.Sql( rec.Factory ) + ","
										+ Conv.Sql( project ) + "," //Templates are saved on factory level (factory, factory), 2012-04-23
										+ Conv.Sql( rec.TransportId ) + ","
										+ Conv.Sql( vehicleId ) + ","
										+ Conv.Sql( vehicle.Name ) + ","
										+ Conv.Sql( seq ) + ","
										+ Conv.Sql( vehicle.TransportType )
										+ ")";

				database.ExecuteNonQuery( sql );

				res.VehicleId = vehicleId;
			}
			return res;
		}
		/// <summary>
		/// Loading Transport Vehicle Standard items
		/// </summary>
		/// <param name="rec"></param>
		/// <returns></returns>
		public List<RecTransportVehicleStd> LoadTransportVehicles( RecTransport rec )
		{
			return LoadTransportVehiclesEx( rec, 0 );
		}
		/// <summary>
		/// Loading Transport Vehicle Standard items
		/// </summary>
		/// <param name="rec"></param>
		/// <returns></returns>
		public List<RecTransportVehicleStd> LoadTransportVehiclesEx( RecTransport rec, int vehicleId )
		{
			if( rec == null || rec.Factory == null || rec.Project == null )
			{
				return null;
			}
			string project = "";
			if( rec.IsTemplate == 1 )
			{
				// Templates are saved on factory level (factory, factory), 2012-04-23
				project = rec.Factory;
			}
			else
			{
				project = rec.Project;
			}
			List<RecTransportVehicleStd> lst = new List<RecTransportVehicleStd>();
			using( ImpactDatabase database = new ImpactDatabase() )
			{
				// 1. Instantiate a new command with a query and connection
				string wcVehicle = "";
				if( vehicleId > 0 )
				{
					wcVehicle = " AND TSP.VEHICLE_ID=" + Conv.Sql( vehicleId );
				}
				string sql = "SELECT TSP.TRANSPORT_TYPE AS TSP_TRANSPORT_TYPE, TSP.VEHICLE_ID AS TSP_VEHICLE_ID, "
							+ "VEH.VEHICLE_TYPE, VEH.NAME, VEH.Description, VEH.Max_Length, VEH.Max_Width, VEH.Max_Height, VEH.Max_Mass"
							+ " FROM "
							+ "IMP_TRANSPORT_VEHICLE_STD VEH "
							+ "join IMP_TRANSPORT_VEHICLE TSP ON "
							+ "VEH.FACTORY = TSP.FACTORY "
							+ "AND VEH.PROJECT = TSP.FACTORY "// Factory, Factory only so far!!!
							+ "AND VEH.NAME = TSP.VEHICLE "
							+ "Where "
							+ "TSP.FACTORY=" + Conv.Sql( rec.Factory ) + " AND "
							+ "TSP.PROJECT=" + Conv.Sql( project ) + " AND " // Templates are saved on factory level (factory, factory), 2012-04-23
							+ "TSP.TRANSPORT_ID=" + Conv.Sql( rec.TransportId )
							+ wcVehicle
							+ " Order By TSP.VEHICLE_SEQUENCE_NO";

				lst = database.GetAll( sql, column => new RecTransportVehicleStd()
				{
					TransportType = DataConverter.Cast<int>( column[0] ),
					VehicleId = DataConverter.Cast<int>( column[1] ),
					VehicleType = DataConverter.Cast<int>( column[2] ),


					Name = DataConverter.Cast<string>( column[3] ),
					Description = DataConverter.Cast<string>( column[4] ),
					MaxLength = DataConverter.Cast<double>( column[5] ),
					MaxWidth = DataConverter.Cast<double>( column[6] ),
					MaxHeight = DataConverter.Cast<double>( column[7] ),
					MaxMass = DataConverter.Cast<double>( column[8] ),

					// Make life easier for the client
					TransportId = rec.TransportId,
					Factory = rec.Factory,
					Project = rec.Project,
				} );
			}
			return lst;
		}
		private void LoadVehicleStdDetails_( RecTransport veh )
		{
			// Try to find the vehicle in factory, factory otherwise
			// try to find it in company, compnay
		}
		private RecTransportVehicleStd LoadVehicleStdDetails( RecTransportVehicleStd record )
		{
			if( record == null )
			{
				throw new ArgumentNullException( "VehicleStd" );
			}

			ImpactQuery query = new ImpactQuery()
			{
				Select =
				{
					ImpTransportVehicleStd.Name,
					ImpTransportVehicleStd.Description,
					ImpTransportVehicleStd.MaxLength,
					ImpTransportVehicleStd.MaxWidth,
					ImpTransportVehicleStd.MaxHeight,
					ImpTransportVehicleStd.MaxMass,
					ImpTransportVehicleStd.VehicleType,
					ImpTransportVehicleStd.TransportType,
				},
				From = { ImpTransportVehicleStd.As( "T1" ) },
				Where = { ImpTransportVehicleStd.Factory.Equal( record.Factory ), 
						  ImpTransportVehicleStd.Project.Equal( record.Factory ),  
						  ImpTransportVehicleStd.Name.Equal( record.Name)}
			};

			string statement = query.ToString();

			RecTransportVehicleStd result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.GetFirst<RecTransportVehicleStd>( statement, column =>
				{
					return new RecTransportVehicleStd()
					{
						Factory = record.Factory,
						Project = record.Project,

						Name = DataConverter.Cast<string>( column[0] ),
						Description = DataConverter.Cast<string>( column[1] ),
						MaxLength = DataConverter.Cast<double>( column[2] ),
						MaxWidth = DataConverter.Cast<double>( column[3] ),
						MaxHeight = DataConverter.Cast<double>( column[4] ),
						MaxMass = DataConverter.Cast<double>( column[5] ),
						VehicleType = DataConverter.Cast<int>( column[6] ),
						TransportType = DataConverter.Cast<int>( column[7] ),
					};
				} );
			}

			if( result != null )
			{
				result.Factory = record.Factory;
				result.Project = record.Project;
				result.TransportType = record.TransportType; // Hmm which one should be used?
				result.VehicleId = record.VehicleId;
				result.TransportId = record.TransportId;
			}
			return result;
		}

		/// <summary>
		///  Remove Transport Vehicles Standard items from transport
		/// </summary>
		/// <param name="rec"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public int RemoveTransportVehicles( RecTransport rec, List<RecTransportVehicleStd> list )
		{
			if( rec == null || list == null || list.Count == 0 )
			{
				return 0;
			}
			string project = "";
			if( rec.IsTemplate == 1 )
			{
				// Templates are saved on factory level (factory, factory), 2012-04-23
				project = rec.Factory;
			}
			else
			{
				project = rec.Project;
			}
			string ids = "";
			foreach( RecTransportVehicleStd vehicle in list )
			{
				ids += Conv.Sql( vehicle.VehicleId ) + ",";
			}
			// remove last comma(,)
			if( ids.Length > 0 )
			{
				ids = ids.Substring( 0, ids.Length - 1 );
			}

			string sql = "Delete from IMP_TRANSPORT_VEHICLE Where "
									+ "FACTORY = " + Conv.Sql( rec.Factory ) + " AND "
									+ "PROJECT = " + Conv.Sql( project ) + " AND "// Templates are saved on factory level (factory, factory), 2012-04-23
									+ "TRANSPORT_ID = " + Conv.Sql( rec.TransportId ) + " AND "
									+ "VEHICLE_ID IN (" + ids + ")";

			int result = 0;
			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.ExecuteNonQuery( sql );
			}
			return result;
		}
	}
}
