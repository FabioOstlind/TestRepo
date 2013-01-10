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
	/// Used to modify records of type RecProductionCastStrand.
	/// </summary>
	public partial class ProjectManager : IProductionCastStrand
	{
        /// <summary> 
		/// Load all records of the same factory and project as the supplied record. 
		/// </summary>
		/// <param name="record">A record with factory and project set.</param>
		/// <returns>A list of all mathcing records.</returns>
		public List<RecProductionCastStrand> LoadProductionCastStrand( RecProductionCastStrand record )
		{
			ImpactQuery query = new ImpactQuery()
			{
				Select =
				{
					ImpProductionCastStrand.Factory,
					ImpProductionCastStrand.Project,
					ImpProductionCastStrand.CastId,
					ImpProductionCastStrand.StrandPos,
					ImpProductionCastStrand.StrandX,
					ImpProductionCastStrand.StrandY,
					ImpProductionCastStrand.StrandQuality,
					ImpProductionCastStrand.StrandDimension,
					ImpProductionCastStrand.StrandPrestressing,

				},
				From  = { ImpProductionCastStrand.As( "T1" ) },
				Where = { ImpProductionCastStrand.Factory.Equal( record.Factory ), 
                          ImpProductionCastStrand.Project.Equal( record.Factory ),// Factory, Factory for productionCast & ProductionCastStrand
                          ImpProductionCastStrand.CastId.Equal( record.CastId )}
			};

			string statement = query.ToString();

			List<RecProductionCastStrand> result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.GetAll( statement, ParseProductionCastStrand );
			}

			return result;
		}

        public int CopyStrandsFromTemplate( RecProductionCast record )
        {
            var rec = new RecProductionFormStrandStd
                          { Factory = record.Factory, Project = record.Project, Name = record.Form };
            var svc = new ProjectManager();
            var list = svc.LoadProductionFormStrandStd( rec );
            if (list == null || list.Count == 0)
            {
                return 0;
            }
            list = (from o in list where o.IsUsed select o).ToList();

            foreach (var std in list)
            {
                var strand = new RecProductionCastStrand
                                 {
                                     Factory = record.Factory,
                                     Project = record.Project,
                                     CastId = record.CastId,
                                     StrandPos = std.StrandPos,
                                     StrandX = std.StrandX,
                                     StrandY = std.StrandY,
                                     StrandQuality = std.StrandQuality,
                                     StrandDimension = std.StrandDimension,
                                     StrandPrestressing = std.StrandPrestressing
                                 };

                this.InsertProductionCastStrand( strand, null, null );
            }

            return list.Count;
        }

	    /// <summary>
	    /// Parses one row in <see cref="System.Data.Common.DbDataReader"/> into
	    /// a new instance of <see>
	    ///                     <cref>Paths.Common.Records.RecProductionCastStrand</cref>
	    ///                   </see> .
	    /// </summary>
	    /// <param name="dataReader">The data reader.</param>
	    /// <returns>A new instance of <see>
	    ///                              <cref>Paths.Common.Records.RecProductionCastStrand</cref>
	    ///                            </see> .</returns>
	    public static RecProductionCastStrand ParseProductionCastStrand( DbDataReader dataReader )
		{
			var record = new RecProductionCastStrand();
			record.Factory = DataConverter.Cast<string>( dataReader[0] );
			record.Project = DataConverter.Cast<string>( dataReader[1] );
			record.CastId = DataConverter.Cast<int>( dataReader[2] );
			record.StrandPos = DataConverter.Cast<int>( dataReader[3] );
			record.StrandX = DataConverter.Cast<double>( dataReader[4] );
			record.StrandY = DataConverter.Cast<double>( dataReader[5] );
			record.StrandQuality = DataConverter.Cast<string>( dataReader[6] );
			record.StrandDimension = DataConverter.Cast<double>( dataReader[7] );
			record.StrandPrestressing = DataConverter.Cast<double>( dataReader[8] );
			return record;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="records"></param>
        /// <param name="form"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public int BulkInsertProductionCastStrand( List<RecProductionCastStrand> records, RecProductionFormStd form, BedFilter filter )
        {
            if( null == records || records.Count == 0 )
            {
                return -1;
            }

            foreach( var recProductionCastStrand in records )
            {
                this.InsertProductionCastStrand( recProductionCastStrand, form, filter );
            }

            return 0;
        }

	    /// <summary>
		/// Insert the specified record into the database.
		/// </summary>
		/// <param name="record">The record to insert into the database.</param>
		/// <returns>The number of affected records.</returns>
        public int InsertProductionCastStrand(RecProductionCastStrand record, RecProductionFormStd form, BedFilter filter)
		{
            if (null == record)
            {
                return -1;
            }
            int castId = record.CastId;
            if (castId <= 0)
            {
                if (null == form || null == filter)
                {
                    return -1;
                }
                var cast = new RecProductionCast
                    {
                        Factory = record.Factory,
                        Project = record.Project,
                        CastType = form.FormType,
                        Description = "",
                        Shift = filter.Shift,
                        StartDate = filter.StartDateFrom,
                        EndDate = filter.StartDateFrom,
                        Form = form.Name,
                        Tolerance = form.Tolerance,
                        ElementType = form.ElementType,
                        Style = form.Style,
                        Strandptn = form.Strandptn,
                        CastStatus = 0,
                        CastDivision = ""
                    };

                var svc = new ProjectManager();
                var newCast = svc.InsertProductionCast(cast);
                castId = newCast.CastId;
            }

			var insert = new ImpactInsert( ImpProductionCastStrand.Instance )
			{
				Columns = 
				{
					{ ImpProductionCastStrand.Factory, record.Factory },
					{ ImpProductionCastStrand.Project, record.Factory },// Factory, Factory for productionCast & ProductionCastStrand
					{ ImpProductionCastStrand.CastId, castId },
					{ ImpProductionCastStrand.StrandPos, record.StrandPos },
					{ ImpProductionCastStrand.StrandX, record.StrandX },
					{ ImpProductionCastStrand.StrandY, record.StrandY },
					{ ImpProductionCastStrand.StrandQuality, record.StrandQuality },
					{ ImpProductionCastStrand.StrandDimension, record.StrandDimension },
					{ ImpProductionCastStrand.StrandPrestressing, record.StrandPrestressing },
				}
			};

			string statement = insert.ToString();

			int result;

			using( var database = new ImpactDatabase() )
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
		public int DeleteProductionCastStrand( RecProductionCastStrand record )
		{
			var delete = new ImpactDelete( ImpProductionCastStrand.Instance )
			{
				Where = 
				{
					ImpProductionCastStrand.Factory.Equal( record.Factory ),
					ImpProductionCastStrand.Project.Equal( record.Factory ),// Factory, Factory for productionCast & ProductionCastStrand
					ImpProductionCastStrand.CastId.Equal( record.CastId ),
				}
			};

            if (record.StrandPos > 0)
            {
                delete.Where.Add( ImpProductionCastStrand.StrandPos.Equal(record.StrandPos) );
            }

			var statement = delete.ToString();

			int result;

			using( var database = new ImpactDatabase() )
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
		public int BulkDeleteProductionCastStrand( List<RecProductionCastStrand> list )
		{
			var result = 0;

			foreach( var record in list )
			{
				result += this.DeleteProductionCastStrand( record );
			}

			return result;
		}
		/// <summary>
		/// Update the specified record in the database.
		/// </summary>
		/// <param name="record">The record to update.</param>
		/// <returns></returns>
		public int UpdateProductionCastStrand( RecProductionCastStrand record )
		{

			var update = new ImpactUpdate( ImpProductionCastStrand.Instance )
			{
				Columns = 
				{
					{ ImpProductionCastStrand.StrandX, record.StrandX },
					{ ImpProductionCastStrand.StrandY, record.StrandY },
					{ ImpProductionCastStrand.StrandQuality, record.StrandQuality },
					{ ImpProductionCastStrand.StrandDimension, record.StrandDimension },
					{ ImpProductionCastStrand.StrandPrestressing, record.StrandPrestressing },
				},
				Where = 
				{
					{ ImpProductionCastStrand.Factory.Equal( record.Factory ) },
					{ ImpProductionCastStrand.Project.Equal( record.Factory ) },// Factory, Factory for productionCast & ProductionCastStrand
					{ ImpProductionCastStrand.CastId.Equal( record.CastId ) },
					{ ImpProductionCastStrand.StrandPos.Equal( record.StrandPos ) },
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

		public int BulkUpdateProductionCastStrand( List<RecProductionCastStrand> list )
		{
			int result = 0;

			foreach( var record in list )
			{
				result += this.UpdateProductionCastStrand( record );
			}

			return result;
		}
	}
}
