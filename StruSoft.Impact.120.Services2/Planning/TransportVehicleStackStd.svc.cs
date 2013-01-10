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
using StruSoft.Impact.V120.Services;

namespace StruSoft.Impact.V120.Services
{
	/// <summary>
	/// Used to modify records of type RecTransportVehicleStackStd.
	/// </summary>
	public partial class ProjectManager : ITransportVehicleStackStd
	{
		/// <summary> 
		/// Load all records of the same factory and project as the supplied record.
		/// </summary>
		/// <param name="record">A record with factory and project set.</param>
		/// <returns>A list of all mathcing records.</returns>
		public List<RecTransportVehicleStackStd> LoadTransportVehicleStackStd( RecTransportVehicleStackStd record )
		{
			ImpactQuery query = new ImpactQuery()
			{
				Select =
				{
					ImpTransportVehicleStackStd.Name,
					ImpTransportVehicleStackStd.StackId,
					ImpTransportVehicleStackStd.Rack,
					ImpTransportVehicleStackStd.StackPosition,
					ImpTransportVehicleStackStd.StackX,
					ImpTransportVehicleStackStd.StackY,
					ImpTransportVehicleStackStd.StackZ,
					ImpTransportVehicleStackStd.StackRotation,
					ImpTransportVehicleStackStd.Description,
					ImpTransportVehicleStackStd.StackType,
					ImpTransportVehicleStackStd.MaxLength,
					ImpTransportVehicleStackStd.MaxWidth,
					ImpTransportVehicleStackStd.MaxHeight,
					ImpTransportVehicleStackStd.MaxMass
				},
				From = { ImpTransportVehicleStackStd.As( "T1" ) },
				Where = { ImpTransportVehicleStackStd.Factory.Equal( record.Factory ), 
						  ImpTransportVehicleStackStd.Project.Equal( record.Factory ),//for Std use Factory, Factory
						  ImpTransportVehicleStackStd.Name.Equal( record.Name )}
			};

			string statement = query.ToString();

			List<RecTransportVehicleStackStd> result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.GetAll( statement, ParseTransportVehicleStackStd );
			}

			return result;
		}

		/// <summary>
		/// Parses one row in <see cref="System.Data.Common.DbDataReader"/> into
		/// a new instance of <see cref="StruSoft.Impact.DB.RawData.RecTransportVehicleStackStd"/>.
		/// </summary>
		/// <param name="dataReader">The data reader.</param>
		/// <param name="record">A new instance of <see cref="StruSoft.Impact.DB.RawData.RecTransportVehicleStackStd"/>.</param>
		public static RecTransportVehicleStackStd ParseTransportVehicleStackStd( DbDataReader dataReader )
		{
			var record = new RecTransportVehicleStackStd();
			record.Factory = record.Factory;
			record.Project = record.Project;
			record.Name = DataConverter.Cast<string>( dataReader[0] );
			record.StackId = DataConverter.Cast<int>( dataReader[1] );
			record.Rack = DataConverter.Cast<string>( dataReader[2] );
			record.StackPosition = DataConverter.Cast<int>( dataReader[3] );
			record.StackX = DataConverter.Cast<double>( dataReader[4] );
			record.StackY = DataConverter.Cast<double>( dataReader[5] );
			record.StackZ = DataConverter.Cast<double>( dataReader[6] );
			record.StackRotation = DataConverter.Cast<double>( dataReader[7] );
			record.Description = DataConverter.Cast<string>( dataReader[8] );
			record.StackType = DataConverter.Cast<int>( dataReader[9] );
			record.MaxLength = DataConverter.Cast<double>( dataReader[10] );
			record.MaxWidth = DataConverter.Cast<double>( dataReader[11] );
			record.MaxHeight = DataConverter.Cast<double>( dataReader[12] );
			record.MaxMass = DataConverter.Cast<double>( dataReader[13] );
			return record;
		}

		/// <summary>
		/// Returns max value of transport_id
		/// </summary>
		/// <param name="transport"></param>
		/// <returns></returns>
		public int GetMaxStacktId( RecTransportVehicleStackStd record )
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
						 Aggregate.Max(ImpTransportStack.StackId)
					},
					From = { ImpTransportStack.As( "T1" ) },
					Where = { ImpTransportStack.Factory.Equal( record.Factory ), // Per factory, project only!
							  ImpTransportStack.Project.Equal( record.Project ),
							  ImpTransportStack.StackId.GreaterThan( 0 )},
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
		/// Insert the specified record into the database.
		/// </summary>
		/// <param name="record">The record to insert into the database.</param>
		/// <returns>The number of affected records.</returns>
		public int InsertTransportVehicleStackStd( RecTransportVehicleStackStd record )
		{
			ProjectManager ng = new ProjectManager();
			string company = ProjectManager.GetCompany( record.Factory );
			record.StackId = ng.GetNextNumber( company, company, RecNumberGenerator.CMP_NG_STACK_ID );

			var insert = new ImpactInsert( ImpTransportVehicleStackStd.Instance )
			{
				Columns = 
				{
					{ ImpTransportVehicleStackStd.Factory, record.Factory },
					{ ImpTransportVehicleStackStd.Project, record.Factory }, //Std use, Factory, Factory
					{ ImpTransportVehicleStackStd.Name, record.Name },
					{ ImpTransportVehicleStackStd.StackId, record.StackId },
					{ ImpTransportVehicleStackStd.Rack, record.Rack },
					{ ImpTransportVehicleStackStd.StackPosition, record.StackPosition },
					{ ImpTransportVehicleStackStd.StackX, record.StackX },
					{ ImpTransportVehicleStackStd.StackY, record.StackY },
					{ ImpTransportVehicleStackStd.StackZ, record.StackZ },
					{ ImpTransportVehicleStackStd.StackRotation, record.StackRotation },
					{ ImpTransportVehicleStackStd.Description, record.Description },
					{ ImpTransportVehicleStackStd.StackType, record.StackType },
					{ ImpTransportVehicleStackStd.MaxLength, record.MaxLength },
					{ ImpTransportVehicleStackStd.MaxWidth, record.MaxWidth },
					{ ImpTransportVehicleStackStd.MaxHeight, record.MaxHeight },
					{ ImpTransportVehicleStackStd.MaxMass, record.MaxMass },
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
		/// Delete the specified record from the database.
		/// </summary>
		/// <param name="record">The record to delete from the database.</param>
		/// <returns>The number of affected records.</returns>
		public int DeleteTransportVehicleStackStd( RecTransportVehicleStackStd record )
		{
			ImpactDelete delete = null;
			if( record.StackId > 0 )
			{
				delete = new ImpactDelete( ImpTransportVehicleStackStd.Instance )
				{
					Where = 
					{
						{ ImpTransportVehicleStackStd.Factory.Equal( record.Factory )},
						{ ImpTransportVehicleStackStd.Project.Equal( record.Factory )},//for Std use Factory, Factory
						{ ImpTransportVehicleStackStd.Name.Equal( record.Name )},
						{ ImpTransportVehicleStackStd.StackId.Equal( record.StackId )},
					}
				};
			}
			else
			{
				delete = new ImpactDelete( ImpTransportVehicleStackStd.Instance )
				{
					Where = 
					{
						{ ImpTransportVehicleStackStd.Factory.Equal( record.Factory )},
						{ ImpTransportVehicleStackStd.Project.Equal( record.Factory )},//for Std use Factory, Factory
						{ ImpTransportVehicleStackStd.Name.Equal( record.Name )},
					}
				};
			}

			string statement = delete.ToString();

			int result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.ExecuteNonQuery( statement );
			}

			return result;
		}
		/// <summary>
		/// Delete the specified list of records from the database.
		/// </summary>
		/// <param name="list">The list of records to delete from the database.</param>
		/// <returns>The number of affected  records.</returns>
		public int BulkDeleteTransportVehicleStackStd( List<RecTransportVehicleStackStd> list )
		{
			int result = 0;

			foreach( var record in list )
			{
				result += this.DeleteTransportVehicleStackStd( record );
			}

			return result;
		}
		/// <summary>
		/// Update the specified record in the database.
		/// </summary>
		/// <param name="record">The record to update.</param>
		/// <returns></returns>
		public int UpdateTransportVehicleStackStd( RecTransportVehicleStackStd record )
		{

			var update = new ImpactUpdate( ImpTransportVehicleStackStd.Instance )
			{
				Columns = 
				{
					{ ImpTransportVehicleStackStd.Rack, record.Rack },
					{ ImpTransportVehicleStackStd.StackPosition, record.StackPosition },
					{ ImpTransportVehicleStackStd.StackX, record.StackX },
					{ ImpTransportVehicleStackStd.StackY, record.StackY },
					{ ImpTransportVehicleStackStd.StackZ, record.StackZ },
					{ ImpTransportVehicleStackStd.StackRotation, record.StackRotation },
					{ ImpTransportVehicleStackStd.Description, record.Description },
					{ ImpTransportVehicleStackStd.StackType, record.StackType },
					{ ImpTransportVehicleStackStd.MaxLength, record.MaxLength },
					{ ImpTransportVehicleStackStd.MaxWidth, record.MaxWidth },
					{ ImpTransportVehicleStackStd.MaxHeight, record.MaxHeight },
					{ ImpTransportVehicleStackStd.MaxMass, record.MaxMass },
				},
				Where = 
				{
					{ ImpTransportVehicleStackStd.Factory.Equal( record.Factory ) },
					{ ImpTransportVehicleStackStd.Project.Equal( record.Factory ) },//for Std use Factory, Factory
					{ ImpTransportVehicleStackStd.Name.Equal( record.Name ) },
					{ ImpTransportVehicleStackStd.StackId.Equal( record.StackId ) },
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

		public int BulkUpdateTransportVehicleStackStd( List<RecTransportVehicleStackStd> list )
		{
			int result = 0;

			foreach( var record in list )
			{
				result += this.UpdateTransportVehicleStackStd( record );
			}

			return result;
		}

		/// <summary>
		/// Insert the specified record into the database.
		/// Actually we copy std stack info into a new db-record
		/// </summary>
		/// <param name="record">The record to insert into the database.</param>
		/// <returns>The number of affected records.</returns>
		public int InsertTransportVehicleStack( RecTransportVehicleStackStd record )
		{
			// !!! Sorry, NumberGenerator is not used for this Id !!! 
			//NumberGenerator ng = new NumberGenerator();
			//string company = NumberGenerator.GetCompany( record.Factory );
			//record.StackId = ng.GetNextNumber( company, company, RecNumberGenerator.CMP_NG_STACK_ID );

			//Now let's use the slow, unsecure and bad unique id's using max, what a mess :(
			record.StackId = GetMaxStacktId( record ) + 1;
			var insert = new ImpactInsert( ImpTransportStack.Instance )
			{
				Columns = 
				{
					{ ImpTransportStack.Factory, record.Factory },
					{ ImpTransportStack.Project, record.Project },
					{ ImpTransportStack.TransportId, record.TransportId },
					{ ImpTransportStack.VehicleId, record.VehicleId },
					{ ImpTransportStack.StackId, record.StackId },
					{ ImpTransportStack.Rack, record.Rack },
					{ ImpTransportStack.StackX, record.StackX },
					{ ImpTransportStack.StackY, record.StackY },
					{ ImpTransportStack.StackZ, record.StackZ },
					{ ImpTransportStack.StackRotation, record.StackRotation },
					{ ImpTransportStack.Description, record.Description },
					{ ImpTransportStack.StackType, record.StackType },
					{ ImpTransportStack.StackPosition, record.StackPosition },
					{ ImpTransportStack.MaxLength, record.MaxLength },
					{ ImpTransportStack.MaxWidth, record.MaxWidth },
					{ ImpTransportStack.MaxHeight, record.MaxHeight },
					{ ImpTransportStack.MaxMass, record.MaxMass },
				}
			};

			string statement = insert.ToString();

			int result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.ExecuteNonQuery( statement );
			}

			return record.StackId;
		}

		/// <summary>
		/// Delete the specified record from the database.
		/// </summary>
		/// <param name="record">The record to delete from the database.</param>
		/// <returns>The number of affected records.</returns>
		public int DeleteTransportVehicleStack( RecTransportVehicleStackStd record )
		{
			var delete = new ImpactDelete( ImpTransportStack.Instance )
			{
				Where = 
				{
					{ ImpTransportStack.Factory.Equal( record.Factory )},
					{ ImpTransportStack.Project.Equal( record.Project )},
					{ ImpTransportStack.TransportId.Equal( record.TransportId )},
					{ ImpTransportStack.VehicleId.Equal( record.VehicleId )},
					{ ImpTransportStack.StackId.Equal( record.StackId )},
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
	}
}
