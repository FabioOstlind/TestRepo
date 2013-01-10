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
	/// Used to modify records of type RecProductionCast. 
	/// </summary>
	public partial class ProjectManager : IProductionCast
	{
		/// <summary>  
		/// Load all records of the same factory and project as the supplied record.
		/// </summary>
		/// <param name="record">A record with factory and project set.</param>
		/// <returns>A list of all mathcing records.</returns>
		public List<RecProductionCast> LoadProductionCast( BedFilter filter, RecProductionCast record )
		{
            List<RecProductionCast> castData = new List<RecProductionCast>( );
			ImpactQuery query = new ImpactQuery()
			{
				Select =
				{
					ImpProductionCast.Factory,
					ImpProductionCast.Project,
					ImpProductionCast.CastId,
					ImpProductionCast.CastType,
					ImpProductionCast.Description,
					ImpProductionCast.Shift,
					ImpProductionCast.StartDate,
					ImpProductionCast.EndDate,
					ImpProductionCast.Form,
					ImpProductionCast.Tolerance,
					ImpProductionCast.ElementType,
					ImpProductionCast.Style,
					ImpProductionCast.Strandptn,
					ImpProductionCast.CastStatus,
					ImpProductionCast.CastDivision,

					ImpProductionFormStd.Location,
				},
				From  = { ImpProductionCast.As( "T1" ) },

				Join =
				{
					Join.Left( ImpProductionFormStd.As( "FRM" ),	
						ImpProductionCast.Factory.Equal( ImpProductionFormStd.Factory ),
						ImpProductionCast.Project.Equal( ImpProductionFormStd.Project ),//Factory, Factory
						ImpProductionCast.Form.Equal( ImpProductionFormStd.Name ) ),
				},

				Where = { ImpProductionCast.Factory.Equal( record.Factory ) , 
                          ImpProductionCast.Project.Equal( record.Factory ) }, // Factory, Factory for productionCast & ProductionCastStrand
			};

			query.Where.Add( ImpProductionCast.StartDate.Equal( record.StartDate ) );
			query.Where.Add( ImpProductionCast.Form.Equal( record.Form ) );

            if( filter != null )
            {
                if( filter.Shift != 0 )
                {
                    query.Where.Add( ImpProductionCast.Shift.Equal( filter.Shift) );
                }
                if( !filter.Location.Equals( Filter.All) && !string.IsNullOrEmpty( filter.Location ) )
                {
                    query.Where.Add( ImpProductionFormStd.Location.Equal( filter.Location) );
                }
                if( !filter.Bed.Equals( Filter.All) && !string.IsNullOrEmpty( filter.Bed ) )
                {
                    query.Where.Add( ImpProductionCast.Form.Equal( filter.Bed ) );
                }
                if( filter.UseStartDateFrom )
                {
                    query.Where.Add( ImpProductionCast.StartDate.GreaterThanOrEqual( filter.StartDateFrom ) );
                }			
                if( filter.UseStartDateTo )
                {
                    query.Where.Add( ImpProductionCast.StartDate.LessThanOrEqual( filter.StartDateTo ) );
                }			
                if( filter.UseEndDateFrom )
                {
                    query.Where.Add( ImpProductionCast.EndDate.GreaterThanOrEqual( filter.EndDateFrom ) );
                }			
                if( filter.UseEndDateTo )
                {
                    query.Where.Add( ImpProductionCast.EndDate.LessThanOrEqual( filter.EndDateTo ) );
                }			
            }
			string statement = query.ToString();

			List<RecProductionCast> result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.GetAll( statement, ParseProductionCast );
			}

            return result;
		}
				/// <summary>
		/// Parses one row in <see cref="System.Data.Common.DbDataReader"/> into
		/// a new instance of <see cref="StruSoft.Impact.V120.Common.Records.RecProductionCast"/>.
		/// </summary>
		/// <param name="dataReader">The data reader.</param>
		/// <returns>A new instance of <see cref="StruSoft.Impact.V120.Common.Records.RecProductionCast"/>.</returns>
		public static RecProductionCast ParseProductionCast( DbDataReader dataReader )
		{
			var record = new RecProductionCast();
			record.Factory = DataConverter.Cast<string>( dataReader[0] );
			record.Project = DataConverter.Cast<string>( dataReader[1] );
			record.CastId = DataConverter.Cast<int>( dataReader[2] );
			record.CastType = DataConverter.Cast<int>( dataReader[3] );
			record.Description = DataConverter.Cast<string>( dataReader[4] );
			record.Shift = DataConverter.Cast<int>( dataReader[5] );
			record.StartDate = DataConverter.Cast<System.DateTime?>( dataReader[6] );
			record.EndDate = DataConverter.Cast<System.DateTime?>( dataReader[7] );
			record.Form = DataConverter.Cast<string>( dataReader[8] );
			record.Tolerance = DataConverter.Cast<double>( dataReader[9] );
			record.ElementType = DataConverter.Cast<string>( dataReader[10] );
			record.Style = DataConverter.Cast<string>( dataReader[11] );
			record.Strandptn = DataConverter.Cast<string>( dataReader[12] );
			record.CastStatus = DataConverter.Cast<int>( dataReader[13] );
			record.CastDivision = DataConverter.Cast<string>( dataReader[14] );

			record.Location = DataConverter.Cast<string>( dataReader[15] );
			return record;
		}
		/// <summary>
		/// Insert the specified record into the database.
		/// </summary>
		/// <param name="record">The record to insert into the database.</param>
		/// <returns>The number of affected records.</returns>
		public RecProductionCast InsertProductionCast( RecProductionCast record )
		{
            // Get new sequence
			ProjectManager ng = new ProjectManager();
			string company = ProjectManager.GetCompany( record.Factory );
			int castId = ng.GetNextNumber( company, company, RecNumberGenerator.CMP_NG_CAST_ID );

			var insert = new ImpactInsert( ImpProductionCast.Instance )
			{
				Columns = 
				{
					{ ImpProductionCast.Factory, record.Factory },
					{ ImpProductionCast.Project, record.Factory }, // Factory, Factory for productionCast
					{ ImpProductionCast.CastId, castId }, //Sequence
					{ ImpProductionCast.CastType, record.CastType },
					{ ImpProductionCast.Description, record.Description },
					{ ImpProductionCast.Shift, record.Shift },
					{ ImpProductionCast.StartDate, record.StartDate },
					{ ImpProductionCast.EndDate, record.EndDate },
					{ ImpProductionCast.Form, record.Form },
					{ ImpProductionCast.Tolerance, record.Tolerance },
					{ ImpProductionCast.ElementType, record.ElementType },
					{ ImpProductionCast.Style, record.Style },
					{ ImpProductionCast.Strandptn, record.Strandptn },
					{ ImpProductionCast.CastStatus, record.CastStatus },
					{ ImpProductionCast.CastDivision, record.CastDivision },
				}
			};

			string statement = insert.ToString();

			int result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.ExecuteNonQuery( statement );
			}

            // Copy strands from template (form)
            if (!string.IsNullOrWhiteSpace(record.Form))
            {
                record.CastId = castId;
                ProjectManager svc = new ProjectManager( );
                svc.CopyStrandsFromTemplate( record );
            }

			return record;
		}

		/// <summary>
		/// Returns a list of the reset element ids.
		/// </summary>
		/// <param name="list">The list of records to delete from the database.</param>
		/// <returns>The number of affected  records.</returns>
		public List<CastScheduleResult> BulkDeleteProductionCast( List<RecProductionCast> list )
		{
            List<CastScheduleResult> totalResult = new List<CastScheduleResult>();
			foreach( var record in list )
			{
				CastScheduleResult result = this.DeleteProductionCast( record );
                if( null != result )
                {
                    totalResult.Add( result );
                }
			}

			return totalResult;
		}

		/// <summary>
		/// Returns a list of the reset element ids.
		/// </summary>
		/// <param name="record">The record to delete from the database.</param>
		/// <returns>The number of affected records.</returns>
		public CastScheduleResult DeleteProductionCast( RecProductionCast record )
		{
            ProjectManager projectManagerService = new ProjectManager();   
            BedFilter filter = new BedFilter();
            filter.Factory = record.Factory;
            List<string> projectsInvolved = projectManagerService.LoadCastProjects( filter, record.CastId );
            if( null == projectsInvolved )
            {
                return null;
            }

            ProjectManager mgr = new ProjectManager();
            RecElementIdStatusStd std = new RecElementIdStatusStd { Factory = record.Factory, Project = record.Project };
            List<RecElementIdStatusStd> settings = mgr.LoadStandardSettings( std );

			ModelPlanner svc = new ModelPlanner( );
            List<int> deletedIds = svc.LoadElementIds( record.Factory, record.Project, record.CastId );

            // (1) Reset elements
            // Update Status, Note this can be optimized!
            RecProductionCast recProductionCastStatus = new RecProductionCast( record );
            recProductionCastStatus.CastStatus = (int)CastStatus.NoStatus;
            recProductionCastStatus.ElementIdStatus = RecElementIdStatusStd.GetLocalSettingFromGlobalId( settings, recProductionCastStatus.CastStatus).StatusId;
            UpdateStatus( record, recProductionCastStatus, settings );
            // Now reset
			svc.ResetElementProduction( record.Factory, record.Project, record.CastId, 0, false );

            // (2) Delete strands
            RecProductionCastStrand recStrand = new RecProductionCastStrand();
            recStrand.Factory = record.Factory;
            recStrand.Project = record.Factory; // Factory, Factory for productionCast & ProductionCastStrand
            recStrand.CastId = record.CastId;
            recStrand.StrandPos = 0;
            ProjectManager strand = new ProjectManager();
            strand.DeleteProductionCastStrand(recStrand);
            // (3) Now delete the cast Object
            int result = 0;
            // The cast object can be deleted if no elements belong to other projects other that the current one
            if( projectsInvolved.Count == 0 || (projectsInvolved.Count == 1 && projectsInvolved[0].Equals( record.Project ) )  )
            {
                var delete = new ImpactDelete( ImpProductionCast.Instance )
                {
                    Where = 
				{
					{ ImpProductionCast.Factory.Equal( record.Factory )},
					{ ImpProductionCast.Project.Equal( record.Factory )},// Factory, Factory for productionCast & ProductionCastStrand
					{ ImpProductionCast.CastId.Equal( record.CastId )},
				}
                };

                string statement = delete.ToString();

                using( ImpactDatabase database = new ImpactDatabase() )
                {
                    result = database.ExecuteNonQuery( statement );
                }
            }

			return new CastScheduleResult(deletedIds, projectsInvolved);
		}

        /// <summary>
        /// Update the cast and elements with new status
        /// </summary>
        /// <param name="transports"></param>
        /// <param name="recTransportStatus"></param>
        /// <returns></returns>
        private int UpdateStatus( RecProductionCast cast, RecProductionCast recProductionCastStatus, List<RecElementIdStatusStd> settings )
        {
            if( null == cast || null == recProductionCastStatus )
            {
                return 0;
            }

            ModelPlanner plannerService = new ModelPlanner();
            
            plannerService.SaveElementProductionStatus( cast, recProductionCastStatus, settings );

            int ret = 0;
            using( ImpactDatabase database = new ImpactDatabase() )
            {
                ImpactUpdate update = new ImpactUpdate( ImpProductionCast.Instance )
                {
                    Columns = 
					{
 						{ ImpProductionCast.CastStatus, recProductionCastStatus.CastStatus },
					},
                    Where =
					{
						ImpProductionCast.Factory.Equal( cast.Factory ),
						ImpProductionCast.Project.Equal( cast.Factory ), //Factory, Factory
						ImpProductionCast.CastId.Equal( cast.CastId ),
					}
                };
                
                string sql = update.ToString();

                ret = database.ExecuteNonQuery( sql );
            }
            return ret;
        }

        /// <summary>
        /// Update the transport and elements with new status
        /// </summary>
        /// <param name="transports"></param>
        /// <param name="recTransportStatus"></param>
        /// <returns></returns>
        public int UpdateStatusProductionCast( List<RecProductionCast> casts, RecProductionCast recProductionCastStatus )
        {
            if( null == casts || casts.Count == 0 )
            {
                return 0;
            }

            ProjectManager mgr = new ProjectManager();
            RecElementIdStatusStd std = new RecElementIdStatusStd { Factory = recProductionCastStatus.Factory, Project = recProductionCastStatus.Project };
            List<RecElementIdStatusStd> settings = mgr.LoadStandardSettings( std );

            int ret = 0;
            foreach( RecProductionCast rec in casts )
            {
                ret += UpdateStatus( rec, recProductionCastStatus, settings );
            }

            return ret;
        }

		/// <summary>
		/// Update the specified record in the database.
		/// </summary>
		/// <param name="record">The record to update.</param>
		/// <returns></returns>
		public CastResult UpdateProductionCast( RecProductionCast record )
		{
            bool hasScheduleCollision = false;
            BedFilter filter = new BedFilter();
            filter.Factory = record.Factory;
            filter.Project = record.Project;
            filter.Shift = record.Shift;
            List<RecProductionCast> casts = this.LoadProductionCast( filter, record );
            if( null != casts && casts.Count > 0 )
            {
                List<RecProductionCast> otherCasts = (from o in casts where o.CastId != record.CastId select o).ToList();
                if( null != otherCasts && otherCasts.Count > 0 )
                {
                    hasScheduleCollision = true;
                }
            }

			var update = new ImpactUpdate( ImpProductionCast.Instance )
			{
				Columns = 
				{
					{ ImpProductionCast.CastType, record.CastType },
					{ ImpProductionCast.Description, record.Description },

					{ ImpProductionCast.Form, record.Form },
					{ ImpProductionCast.Tolerance, record.Tolerance },
					{ ImpProductionCast.ElementType, record.ElementType },
					{ ImpProductionCast.Style, record.Style },
					{ ImpProductionCast.Strandptn, record.Strandptn },
					{ ImpProductionCast.CastStatus, record.CastStatus },
					{ ImpProductionCast.CastDivision, record.CastDivision },
				},
				Where = 
				{
					{ ImpProductionCast.Factory.Equal( record.Factory ) },
					{ ImpProductionCast.Project.Equal( record.Factory ) },// Factory, Factory for productionCast & ProductionCastStrand
					{ ImpProductionCast.CastId.Equal( record.CastId ) },
				},
			};

            if( !hasScheduleCollision )
            {
                update.Columns.Add( ImpProductionCast.Shift, record.Shift );
                update.Columns.Add( ImpProductionCast.StartDate, record.StartDate );
                update.Columns.Add( ImpProductionCast.EndDate, record.EndDate );
            }

			string statement = update.ToString();

			int result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.ExecuteNonQuery( statement );
			}

			return new CastResult { HasScheduleCollision = hasScheduleCollision, EffectedRows = result };
		}

        /// <summary>
        /// Bulk Update
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
		public List<CastResult> BulkUpdateProductionCast( List<RecProductionCast> list )
		{
			List<CastResult> result = new List<CastResult>();

			foreach( var record in list )
			{
				result.Add( this.UpdateProductionCast( record ) );
			}

			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="record"></param>
		/// <returns></returns>
		public int GetCastCount(  RecProductionFormStd record )
		{
			if( record == null )
			{
				throw new ArgumentNullException( "Production Cast" );
			}
			int ret = 0;
			using( ImpactDatabase database = new ImpactDatabase() )
			{
                ImpactQuery query = new ImpactQuery()
                {
                    Select =
					{
						 Aggregate.Count( )
					},
                    From = { ImpProductionCast.As( "T1" ) },

                    Join =
                    {
					    Join.Left( ImpModelPlanning.As( "T2" ),	
						    ImpProductionCast.Factory.Equal( ImpModelPlanning.Factory ),
						    ImpProductionCast.Project.Equal( ImpModelPlanning.Factory ),// Factory, Factory for productionCast & ProductionCastStrand
						    ImpProductionCast.CastId.Equal( ImpModelPlanning.CastId ) ),
                    },

                     Where = 
                     { 
                         ImpProductionCast.Factory.Equal( record.Factory ), 
						 ImpProductionCast.Form.Equal( record.Name ),
						 ImpModelPlanning.CastId.GreaterThan( 0 )
                     },
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

	}
}
