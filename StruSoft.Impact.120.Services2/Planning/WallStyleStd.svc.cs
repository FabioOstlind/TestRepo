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
	/// Used to modify records of type RecWallStyleStd.
	/// </summary>
	public partial class ProjectManager : IWallStyleStd
	{
		/// <summary> 
		/// Load all records of the same factory and project as the supplied record.
		/// </summary>
		/// <param name="record">A record with factory and project set.</param>
		/// <returns>A list of all mathcing records.</returns>
		public List<RecWallStyleStd> LoadWallStyleStd( string factory, string project )
		{
			WhereGroup wgElementType = WhereGroup.Or( ImpWallStyleStd.ElementType.Equal( "V" ) );

			ImpactQuery query = new ImpactQuery()
			{
				Select =
				{
					ImpWallStyleStd.Factory,
					ImpWallStyleStd.Project,
					ImpWallStyleStd.ElementType,
					ImpWallStyleStd.Name,
					ImpWallStyleStd.Description,
				},
				From = { ImpWallStyleStd.As( "T1" ) },
				Where = 
				{ 
					{ wgElementType }
				}
			};

			string statement = query.ToString();

			// To be rewritten in a better way!
			// Add, Factory, Factory and Company, Company
			string company = factory.Substring( 0, 2 ) + "00"; //Is this really OK ??!!


			string projectLevel = string.Format( "( T1.FACTORY = '{0}' AND T1.PROJECT = '{1}')", factory, project );
			string factoryLevel = string.Format( "( T1.FACTORY = '{0}' AND T1.PROJECT = '{1}')", factory, factory );
			string companyLevel = string.Format( "( T1.FACTORY = '{0}' AND T1.PROJECT = '{1}')", company, company );
			statement += " AND (" + projectLevel + " OR " + factoryLevel + " OR " + companyLevel + ")";

			List<RecWallStyleStd> result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.GetAll( statement, ParseWallStyleStd );
			}

			if( result == null || result.Count == 0 )
			{
				return result;
			}

			Dictionary<string, RecWallStyleStd> companyDic = ( from o in result
																  where ( o.Factory == company && o.Project == company )
																  select new RecWallStyleStd { Factory = o.Factory, Project = o.Factory, ElementType = o.ElementType, Name = o.Name } ).ToDictionary( x => x.Name );

			Dictionary<string, RecWallStyleStd> factoryDic = ( from o in result
																  where ( o.Factory == factory && o.Project == factory )
																  select new RecWallStyleStd { Factory = o.Factory, Project = o.Factory, ElementType = o.ElementType, Name = o.Name } ).ToDictionary( x => x.Name );

			Dictionary<string, RecWallStyleStd> projectDic = ( from o in result
																  where ( o.Factory == factory && o.Project == project )
																  select new RecWallStyleStd { Factory = o.Factory, Project = o.Project, ElementType = o.ElementType, Name = o.Name } ).ToDictionary( x => x.Name );

			foreach( var pair in factoryDic )
			{
				if( !projectDic.ContainsKey( pair.Key ) )
				{
					projectDic.Add( pair.Key, pair.Value );
				}
			}
			foreach( var pair in companyDic )
			{
				if( !projectDic.ContainsKey( pair.Key ) )
				{
					projectDic.Add( pair.Key, pair.Value );
				}
			}

			List<RecWallStyleStd> list = projectDic.Values.ToList();
			list.OrderBy( p => p.Name );
			return list;
		}

		/// <summary>
		/// Load all records of the same factory and project as the supplied record.
		/// </summary>
		/// <param name="record">A record with factory and project set.</param>
		/// <returns>A list of all mathcing records.</returns>
		public List<RecWallStyleStd> Load_Dummy( RecWallStyleStd record )
		{
			ImpactQuery query = new ImpactQuery()
			{
				Select =
				{
					ImpWallStyleStd.Factory,
					ImpWallStyleStd.Project,
					ImpWallStyleStd.ElementType,
					ImpWallStyleStd.Name,
					ImpWallStyleStd.Description,
					ImpWallStyleStd.CastType,
					ImpWallStyleStd.RcLayout,
					ImpWallStyleStd.RcTemplate,
					ImpWallStyleStd.ProductionLine,
					ImpWallStyleStd.LiftType,
					ImpWallStyleStd.LiftDistanceMax,
					ImpWallStyleStd.LiftDistanceMin,
					ImpWallStyleStd.LiftSpacing,
					ImpWallStyleStd.BracingCim,
					ImpWallStyleStd.BracingSide,
					ImpWallStyleStd.BracingPlacingLs,
					ImpWallStyleStd.BracingPlacingParameterLs,
					ImpWallStyleStd.BracingPlacingEl,
					ImpWallStyleStd.BracingPlacingParameterEl,
					ImpWallStyleStd.ElementGrp,
					ImpWallStyleStd.ProductPrefix,
					ImpWallStyleStd.Product,
					ImpWallStyleStd.ElementMarkPrefix,
					ImpWallStyleStd.DrawingNamePrefix,
					ImpWallStyleStd.DrawingType,
					ImpWallStyleStd.DrawingTemplate,
					ImpWallStyleStd.CreatedBy,
					ImpWallStyleStd.CreatedDate,
					ImpWallStyleStd.ChangedBy,
					ImpWallStyleStd.ChangedDate,
					ImpWallStyleStd.LiftOtherType,

				},
				From  = { ImpWallStyleStd.As( "T1" ) },
				Where = { ImpWallStyleStd.Factory.Equal( record.Factory ), ImpWallStyleStd.Project.Equal( record.Factory ) }
			};

			string statement = query.ToString();

			List<RecWallStyleStd> result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.GetAll( statement, ParseWallStyleStd );
			}

			return result;
		}

		/// <summary>
		/// Parses one row in <see cref="System.Data.Common.DbDataReader"/> into
		/// a new instance of <see cref="StruSoft.Impact.V120.Common.Records.RecWallStyleStd"/>.
		/// </summary>
		/// <param name="dataReader">The data reader.</param>
		/// <returns>A new instance of <see cref="StruSoft.Impact.V120.Common.Records.RecWallStyleStd"/>.</returns>
		public static RecWallStyleStd ParseWallStyleStd( DbDataReader dataReader )
		{
			var record = new RecWallStyleStd();
			record.Factory = DataConverter.Cast<string>( dataReader[0] );
			record.Project = DataConverter.Cast<string>( dataReader[1] );
			record.ElementType = DataConverter.Cast<string>( dataReader[2] );
			record.Name = DataConverter.Cast<string>( dataReader[3] );
			record.Description = DataConverter.Cast<string>( dataReader[4] );
			//record.CastType = DataConverter.Cast<string>( dataReader[5] );
			//record.RcLayout = DataConverter.Cast<int>( dataReader[6] );
			//record.RcTemplate = DataConverter.Cast<string>( dataReader[7] );
			//record.ProductionLine = DataConverter.Cast<string>( dataReader[8] );
			//record.LiftType = DataConverter.Cast<string>( dataReader[9] );
			//record.LiftDistanceMax = DataConverter.Cast<double>( dataReader[10] );
			//record.LiftDistanceMin = DataConverter.Cast<double>( dataReader[11] );
			//record.LiftSpacing = DataConverter.Cast<double>( dataReader[12] );
			//record.BracingCim = DataConverter.Cast<string>( dataReader[13] );
			//record.BracingSide = DataConverter.Cast<string>( dataReader[14] );
			//record.BracingPlacingLs = DataConverter.Cast<int>( dataReader[15] );
			//record.BracingPlacingParameterLs = DataConverter.Cast<double>( dataReader[16] );
			//record.BracingPlacingEl = DataConverter.Cast<int>( dataReader[17] );
			//record.BracingPlacingParameterEl = DataConverter.Cast<double>( dataReader[18] );
			//record.ElementGrp = DataConverter.Cast<string>( dataReader[19] );
			//record.ProductPrefix = DataConverter.Cast<string>( dataReader[20] );
			//record.Product = DataConverter.Cast<string>( dataReader[21] );
			//record.ElementMarkPrefix = DataConverter.Cast<string>( dataReader[22] );
			//record.DrawingNamePrefix = DataConverter.Cast<string>( dataReader[23] );
			//record.DrawingType = DataConverter.Cast<string>( dataReader[24] );
			//record.DrawingTemplate = DataConverter.Cast<string>( dataReader[25] );
			//record.CreatedBy = DataConverter.Cast<string>( dataReader[26] );
			//record.CreatedDate = DataConverter.Cast<System.DateTime?>( dataReader[27] );
			//record.ChangedBy = DataConverter.Cast<string>( dataReader[28] );
			//record.ChangedDate = DataConverter.Cast<System.DateTime?>( dataReader[29] );
			//record.LiftOtherType = DataConverter.Cast<string>( dataReader[30] );
			return record;
		}

		/// <summary>
		/// Insert the specified record into the database.
		/// </summary>
		/// <param name="record">The record to insert into the database.</param>
		/// <returns>The number of affected records.</returns>
		public int InsertWallStyleStd( RecWallStyleStd record )
		{
			var insert = new ImpactInsert(ImpWallStyleStd.Instance)
			{
				Columns = 
				{
					{ ImpWallStyleStd.Factory, record.Factory },
					{ ImpWallStyleStd.Project, record.Project },
					{ ImpWallStyleStd.ElementType, record.ElementType },
					{ ImpWallStyleStd.Name, record.Name },
					{ ImpWallStyleStd.Description, record.Description },
					{ ImpWallStyleStd.CastType, record.CastType },
					{ ImpWallStyleStd.RcLayout, record.RcLayout },
					{ ImpWallStyleStd.RcTemplate, record.RcTemplate },
					{ ImpWallStyleStd.ProductionLine, record.ProductionLine },
					{ ImpWallStyleStd.LiftType, record.LiftType },
					{ ImpWallStyleStd.LiftDistanceMax, record.LiftDistanceMax },
					{ ImpWallStyleStd.LiftDistanceMin, record.LiftDistanceMin },
					{ ImpWallStyleStd.LiftSpacing, record.LiftSpacing },
					{ ImpWallStyleStd.BracingCim, record.BracingCim },
					{ ImpWallStyleStd.BracingSide, record.BracingSide },
					{ ImpWallStyleStd.BracingPlacingLs, record.BracingPlacingLs },
					{ ImpWallStyleStd.BracingPlacingParameterLs, record.BracingPlacingParameterLs },
					{ ImpWallStyleStd.BracingPlacingEl, record.BracingPlacingEl },
					{ ImpWallStyleStd.BracingPlacingParameterEl, record.BracingPlacingParameterEl },
					{ ImpWallStyleStd.ElementGrp, record.ElementGrp },
					{ ImpWallStyleStd.ProductPrefix, record.ProductPrefix },
					{ ImpWallStyleStd.Product, record.Product },
					{ ImpWallStyleStd.ElementMarkPrefix, record.ElementMarkPrefix },
					{ ImpWallStyleStd.DrawingNamePrefix, record.DrawingNamePrefix },
					{ ImpWallStyleStd.DrawingType, record.DrawingType },
					{ ImpWallStyleStd.DrawingTemplate, record.DrawingTemplate },
					{ ImpWallStyleStd.CreatedBy, record.CreatedBy },
					{ ImpWallStyleStd.CreatedDate, record.CreatedDate },
					{ ImpWallStyleStd.ChangedBy, record.ChangedBy },
					{ ImpWallStyleStd.ChangedDate, record.ChangedDate },
					{ ImpWallStyleStd.LiftOtherType, record.LiftOtherType },
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
		public int DeleteWallStyleStd( RecWallStyleStd record )
		{
			var delete = new ImpactDelete( ImpWallStyleStd.Instance )
			{
				Where = 
				{
					{ ImpWallStyleStd.Factory.Equal( record.Factory )},
					{ ImpWallStyleStd.Project.Equal( record.Project )},
					{ ImpWallStyleStd.ElementType.Equal( record.ElementType )},
					{ ImpWallStyleStd.Name.Equal( record.Name )},
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
		public int BulkDeleteWallStyleStd( List<RecWallStyleStd> list )
		{
			int result = 0;

			foreach( var record in list )
			{
				result += this.DeleteWallStyleStd( record );
			}

			return result;
		}
		/// <summary>
		/// Update the specified record in the database.
		/// </summary>
		/// <param name="record">The record to update.</param>
		/// <returns></returns>
		public int UpdateWallStyleStd( RecWallStyleStd record )
		{

			var update = new ImpactUpdate( ImpWallStyleStd.Instance )
			{
				Columns = 
				{
					{ ImpWallStyleStd.Description, record.Description },
					{ ImpWallStyleStd.CastType, record.CastType },
					{ ImpWallStyleStd.RcLayout, record.RcLayout },
					{ ImpWallStyleStd.RcTemplate, record.RcTemplate },
					{ ImpWallStyleStd.ProductionLine, record.ProductionLine },
					{ ImpWallStyleStd.LiftType, record.LiftType },
					{ ImpWallStyleStd.LiftDistanceMax, record.LiftDistanceMax },
					{ ImpWallStyleStd.LiftDistanceMin, record.LiftDistanceMin },
					{ ImpWallStyleStd.LiftSpacing, record.LiftSpacing },
					{ ImpWallStyleStd.BracingCim, record.BracingCim },
					{ ImpWallStyleStd.BracingSide, record.BracingSide },
					{ ImpWallStyleStd.BracingPlacingLs, record.BracingPlacingLs },
					{ ImpWallStyleStd.BracingPlacingParameterLs, record.BracingPlacingParameterLs },
					{ ImpWallStyleStd.BracingPlacingEl, record.BracingPlacingEl },
					{ ImpWallStyleStd.BracingPlacingParameterEl, record.BracingPlacingParameterEl },
					{ ImpWallStyleStd.ElementGrp, record.ElementGrp },
					{ ImpWallStyleStd.ProductPrefix, record.ProductPrefix },
					{ ImpWallStyleStd.Product, record.Product },
					{ ImpWallStyleStd.ElementMarkPrefix, record.ElementMarkPrefix },
					{ ImpWallStyleStd.DrawingNamePrefix, record.DrawingNamePrefix },
					{ ImpWallStyleStd.DrawingType, record.DrawingType },
					{ ImpWallStyleStd.DrawingTemplate, record.DrawingTemplate },
					{ ImpWallStyleStd.CreatedBy, record.CreatedBy },
					{ ImpWallStyleStd.CreatedDate, record.CreatedDate },
					{ ImpWallStyleStd.ChangedBy, record.ChangedBy },
					{ ImpWallStyleStd.ChangedDate, record.ChangedDate },
					{ ImpWallStyleStd.LiftOtherType, record.LiftOtherType },
				},
				Where = 
				{
					{ ImpWallStyleStd.Factory.Equal( record.Factory ) },
					{ ImpWallStyleStd.Project.Equal( record.Project ) },
					{ ImpWallStyleStd.ElementType.Equal( record.ElementType ) },
					{ ImpWallStyleStd.Name.Equal( record.Name ) },
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

		public int BulkUpdateWallStyleStd( List<RecWallStyleStd> list )
		{
			int result = 0;

			foreach( var record in list )
			{
				result += this.UpdateWallStyleStd( record );
			}

			return result;
		}
	}
}
