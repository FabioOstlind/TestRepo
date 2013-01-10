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
	/// Used to modify records of type RecTransportStack.
	/// </summary>
	public partial class ProjectManager : ITransportStack        
	{
		/// <summary>
		/// Load all records of the same factory and project as the supplied record.
		/// </summary>
		/// <param name="record">A record with factory and project set.</param>
		/// <returns>A list of all mathcing records.</returns>
		public List<RecTransportStack> LoadTransportStack( RecTransportStack record )
		{
			ImpactQuery query = new ImpactQuery()
			{
				Select =
				{
					ImpTransportStack.Factory,
					ImpTransportStack.Project,
					ImpTransportStack.TransportId,
					ImpTransportStack.VehicleId,
					ImpTransportStack.StackId,
					ImpTransportStack.Rack,
					ImpTransportStack.StackX,
					ImpTransportStack.StackY,
					ImpTransportStack.StackZ,
					ImpTransportStack.StackRotation,
					ImpTransportStack.Description,
					ImpTransportStack.StackType,
					ImpTransportStack.StackPosition,
					ImpTransportStack.MaxLength,
					ImpTransportStack.MaxWidth,
					ImpTransportStack.MaxHeight,
					ImpTransportStack.MaxMass,

				},
				From = { ImpTransportStack.As( "T1" ) },
				Where = { ImpTransportStack.Factory.Equal( record.Factory ), 
						  ImpTransportStack.Project.Equal( record.Project ), 
						  ImpTransportStack.TransportId.Equal( record.TransportId ), 
						  ImpTransportStack.VehicleId.Equal( record.VehicleId ) 
						}
			};
			
			if( record.StackId > 0 )
			{
				query.Where.Add( ImpTransportStack.StackId.Equal( record.StackId ) );
			}

			string statement = query.ToString();

			List<RecTransportStack> result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.GetAll( statement, ParseTransportStack );
			}

			return result;
		}

		/// <summary>
		/// Parses one row in <see cref="System.Data.Common.DbDataReader"/> into
		/// a new instance of <see cref="StruSoft.Impact.V120.Common.Records.RecTransportStack"/>.
		/// </summary>
		/// <param name="dataReader">The data reader.</param>
		/// <returns>A new instance of <see cref="StruSoft.Impact.V120.Common.Records.RecTransportStack"/>.</returns>
		public static RecTransportStack ParseTransportStack( DbDataReader dataReader )
		{
			var record = new RecTransportStack();
			record.Factory = DataConverter.Cast<string>( dataReader[0] );
			record.Project = DataConverter.Cast<string>( dataReader[1] );
			record.TransportId = DataConverter.Cast<int>( dataReader[2] );
			record.VehicleId = DataConverter.Cast<int>( dataReader[3] );
			record.StackId = DataConverter.Cast<int>( dataReader[4] );
			record.Rack = DataConverter.Cast<string>( dataReader[5] );
			record.StackX = DataConverter.Cast<double>( dataReader[6] );
			record.StackY = DataConverter.Cast<double>( dataReader[7] );
			record.StackZ = DataConverter.Cast<double>( dataReader[8] );
			record.StackRotation = DataConverter.Cast<double>( dataReader[9] );
			record.Description = DataConverter.Cast<string>( dataReader[10] );
			record.StackType = DataConverter.Cast<int>( dataReader[11] );
			record.StackPosition = DataConverter.Cast<int>( dataReader[12] );
			record.MaxLength = DataConverter.Cast<double>( dataReader[13] );
			record.MaxWidth = DataConverter.Cast<double>( dataReader[14] );
			record.MaxHeight = DataConverter.Cast<double>( dataReader[15] );
			record.MaxMass = DataConverter.Cast<double>( dataReader[16] );
			return record;
		}
		/// <summary>
		/// Insert the specified record into the database.
		/// </summary>
		/// <param name="record">The record to insert into the database.</param>
		/// <returns>The number of affected records.</returns>
		public int InsertTransportStack( RecTransportStack record )
		{
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

			return result;
		}
		/// <summary>
		/// Delete the specified record from the database.
		/// </summary>
		/// <param name="record">The record to delete from the database.</param>
		/// <returns>The number of affected records.</returns>
		public int DeleteTransportStack( RecTransportStack record )
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
		/// <summary>
		/// Delete the specified list of records from the database.
		/// </summary>
		/// <param name="list">The list of records to delete from the database.</param>
		/// <returns>The number of affected  records.</returns>
		public int BulkDeleteTransportStack( List<RecTransportStack> list )
		{
			int result = 0;

			foreach( var record in list )
			{
				result += this.DeleteTransportStack( record );
			}

			return result;
		}
		/// <summary>
		/// Update the specified record in the database.
		/// </summary>
		/// <param name="record">The record to update.</param>
		/// <returns></returns>
		public int UpdateTransportStack( RecTransportStack record )
		{

			var update = new ImpactUpdate( ImpTransportStack.Instance )
			{
				Columns = 
				{
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
				},
				Where = 
				{
					{ ImpTransportStack.Factory.Equal( record.Factory ) },
					{ ImpTransportStack.Project.Equal( record.Project ) },
					{ ImpTransportStack.TransportId.Equal( record.TransportId ) },
					{ ImpTransportStack.VehicleId.Equal( record.VehicleId ) },
					{ ImpTransportStack.StackId.Equal( record.StackId ) },
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

		public int BulkUpdateTransportStack( List<RecTransportStack> list )
		{
			int result = 0;

			foreach( var record in list )
			{
				result += this.UpdateTransportStack( record );
			}

			return result;
		}
	}
}
