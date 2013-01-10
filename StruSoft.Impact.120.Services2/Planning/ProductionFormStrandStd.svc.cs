using System;
using System.Collections.Generic;
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
	/// Used to modify records of type RecProductionFormStrandStd.
	/// </summary>
	public partial class ProjectManager : IProductionFormStrandStd
	{
        /// <summary> 
		/// Load all records of the same factory and project as the supplied record.
		/// </summary>
		/// <param name="record">A record with factory and project set.</param>
		/// <returns>A list of all mathcing records.</returns>
		public List<RecProductionFormStrandStd> LoadProductionFormStrandStd( RecProductionFormStrandStd record )
		{
			ImpactQuery query = new ImpactQuery()
			{
				Select = 
				{
					ImpProductionFormStrandStd.Factory,
					ImpProductionFormStrandStd.Project,
					ImpProductionFormStrandStd.Name,
					ImpProductionFormStrandStd.StrandPos,
					ImpProductionFormStrandStd.StrandX,
					ImpProductionFormStrandStd.StrandY,
					ImpProductionFormStrandStd.StrandQuality,
					ImpProductionFormStrandStd.StrandDimension,
					ImpProductionFormStrandStd.StrandPrestressing,
                    ImpProductionFormStrand.Form,

				},
				From = { ImpProductionFormStrandStd.As( "T1" ) },
                Join =
				{
					Join.Left( ImpProductionFormStrand.As( "T2" ),	
						ImpProductionFormStrand.Factory.Equal( ImpProductionFormStrandStd.Factory ),
						ImpProductionFormStrand.Project.Equal( ImpProductionFormStrandStd.Project ),//Factory, Factory
						ImpProductionFormStrand.Form.Equal( ImpProductionFormStrandStd.Name ),
                        ImpProductionFormStrand.Strand.Equal( ImpProductionFormStrandStd.StrandPos )),
                },

				Where = { ImpProductionFormStrandStd.Factory.Equal( record.Factory ), 
						  ImpProductionFormStrandStd.Project.Equal( record.Factory ), 
						  ImpProductionFormStrandStd.Name.Equal( record.Name )} //Form name
			};

			string statement = query.ToString();

			List<RecProductionFormStrandStd> result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.GetAll( statement, ParseProductionFormStrandStd );
			}

			return result;
		}

		/// <summary>
		/// Parses one row in <see cref="System.Data.Common.DbDataReader"/> into
		/// a new instance of <see cref="Paths.Common.Records.RecProductionFormStrandStd"/>.
		/// </summary>
		/// <param name="dataReader">The data reader.</param>
		/// <returns>A new instance of <see cref="Paths.Common.Records.RecProductionFormStrandStd"/>.</returns>
		public static RecProductionFormStrandStd ParseProductionFormStrandStd( DbDataReader dataReader )
		{
			var record = new RecProductionFormStrandStd();
			record.Factory = dataReader[0].Cast<string>();
			record.Project = dataReader[1].Cast<string>();
			record.Name = dataReader[2].Cast<string>();
			record.StrandPos = dataReader[3].Cast<int>();
			record.StrandX = dataReader[4].Cast<double>();
			record.StrandY = dataReader[5].Cast<double>();
			record.StrandQuality = dataReader[6].Cast<string>();
			record.StrandDimension = dataReader[7].Cast<double>();
			record.StrandPrestressing = dataReader[8].Cast<double>();
            string form = dataReader[9].Cast<string>();
		    record.IsUsed = null != form;
			return record;
		}
		/// <summary>
		/// Insert the specified record into the database.
		/// </summary>
		/// <param name="record">The record to insert into the database.</param>
		/// <returns>The number of affected records.</returns>
		public int InsertProductionFormStrandStd( RecProductionFormStrandStd record )
		{
			var insert = new ImpactInsert( ImpProductionFormStrandStd.Instance )
			{
				Columns = 
				{
					{ ImpProductionFormStrandStd.Factory, record.Factory },
					{ ImpProductionFormStrandStd.Project, record.Factory },
					{ ImpProductionFormStrandStd.Name, record.Name }, //Form name ??
					{ ImpProductionFormStrandStd.StrandPos, record.StrandPos },
					{ ImpProductionFormStrandStd.StrandX, record.StrandX },
					{ ImpProductionFormStrandStd.StrandY, record.StrandY },
					{ ImpProductionFormStrandStd.StrandQuality, record.StrandQuality },
					{ ImpProductionFormStrandStd.StrandDimension, record.StrandDimension },
					{ ImpProductionFormStrandStd.StrandPrestressing, record.StrandPrestressing },
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
		public int DeleteProductionFormStrandStd( RecProductionFormStrandStd record )
		{
            // Delete relation first
		    DeleteProductionFormStrand( record );

            // Now delete the record
			var delete = new ImpactDelete( ImpProductionFormStrandStd.Instance )
			{
				Where = 
				{
					{ ImpProductionFormStrandStd.Factory.Equal( record.Factory )},
					{ ImpProductionFormStrandStd.Project.Equal( record.Factory )},
					{ ImpProductionFormStrandStd.Name.Equal( record.Name )}, //Form name
				}
			};
			if( record.StrandPos > 0 )
			{
				delete.Where.Add( ImpProductionFormStrandStd.StrandPos.Equal( record.StrandPos ) );
			}
			string statement = delete.ToString();

			int result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.ExecuteNonQuery( statement );
			}

			return result;
		}
		//public int DeleteProductionFormStrand( RecProductionFormStd form, RecProductionFormStrandStd strand )
		//{
		//    //// Delete child (form_strand)
		//    //var delete = new ImpactDelete<ImpProductionFormStrand>()
		//    //{
		//    //    Where = 
		//    //    {
		//    //        { ImpProductionFormStrand.Factory.Equal( form.Factory )},
		//    //        { ImpProductionFormStrand.Project.Equal( form.Factory )},
		//    //        { ImpProductionFormStrand.Form.Equal( form.Name )},
		//    //        { ImpProductionFormStrand.Strand.Equal( strand.Name )},
		//    //    }
		//    //};

		//    //string statement = delete.ToString();

		//    //int result;

		//    //using( ImpactDatabase database = new ImpactDatabase() )
		//    //{
		//    //    result = database.ExecuteNonQuery( statement );
		//    //}

		//    // Delete parant strand 
		//    Delete( strand );

		//    return result;
		//}
		/// <summary>
		/// Delete the specified list of records from the database.
		/// </summary>
		/// <param name="list">The list of records to delete from the database.</param>
		/// <returns>The number of affected  records.</returns>
		public int BulkDeleteProductionFormStrandStd( List<RecProductionFormStrandStd> list )
		{
			int result = 0;
			// Note that we have a one-to-one replationship between ProductionFormStrand and ProductionFormStrandStd
			foreach( var record in list )
			{
				this.DeleteProductionFormStrandStd( record );
			}

			return result;
		}
		/// <summary>
		/// Update the specified record in the database.
		/// </summary>
		/// <param name="record">The record to update.</param>
		/// <returns></returns>
		public int UpdateProductionFormStrandStd( RecProductionFormStrandStd record )
		{
            // Update IMP_PRODUCTION_FORM_STRAND check the flag IsUsed
            // Update IMP_PRODUCTION_FORM_STRAND_STD
		    UpdateProductionFormStrand( record );

			var update = new ImpactUpdate( ImpProductionFormStrandStd.Instance )
			{
				Columns = 
				{
					{ ImpProductionFormStrandStd.StrandX, record.StrandX },
					{ ImpProductionFormStrandStd.StrandY, record.StrandY },
					{ ImpProductionFormStrandStd.StrandQuality, record.StrandQuality },
					{ ImpProductionFormStrandStd.StrandDimension, record.StrandDimension },
					{ ImpProductionFormStrandStd.StrandPrestressing, record.StrandPrestressing },
				},
				Where = 
				{
					ImpProductionFormStrandStd.Factory.Equal( record.Factory ),
					ImpProductionFormStrandStd.Project.Equal( record.Factory ), // Factory, Factory Level
					ImpProductionFormStrandStd.Name.Equal( record.Name ),
					ImpProductionFormStrandStd.StrandPos.Equal( record.StrandPos ),
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

		public int BulkUpdateProductionFormStrandStd( List<RecProductionFormStrandStd> list )
		{
			int result = 0;

			foreach( var record in list )
			{
				result += this.UpdateProductionFormStrandStd( record );
			}

			return result;
		}

		//public List<RecProductionFormStrandStd> LoadFormStrandStd( RecProductionFormStd rec )
		//{
		//    ImpactQuery query = new ImpactQuery()
		//    {
		//        Select =
		//        {
		//            //ImpProductionFormStrandStd.Name,
		//            ImpProductionFormStrandStd.StrandPos,
		//            ImpProductionFormStrandStd.StrandX,
		//            ImpProductionFormStrandStd.StrandY,
		//            ImpProductionFormStrandStd.StrandDimension, 
		//            ImpProductionFormStrandStd.StrandQuality, 
		//            ImpProductionFormStrandStd.StrandPrestressing, 
		//        },
		//        From = { ImpProductionFormStrandStd.As( "STD" ) },

		//        Where =
		//        {
		//            ImpProductionFormStrandStd.Factory.Equal( rec.Factory ),
		//            ImpProductionFormStrandStd.Project.Equal( rec.Factory ),
		//            ImpProductionFormStrandStd.Name.Equal( rec.Name ), //Form name
		//        },
		//        OrderBy = 
		//        { 
		//            { ImpProductionFormStrandStd.StrandPos },
		//        },
		//    };
		//    string statement = query.ToString();
		//    List<RecProductionFormStrandStd> result;

		//    using( ImpactDatabase database = new ImpactDatabase() )
		//    {
		//        result = database.GetAll( statement, FormStrandParse );
		//    }

		//    return result;
		//}

		//public static RecProductionFormStrandStd FormStrandParse( DbDataReader column )
		//{
		//    var record = new RecProductionFormStrandStd();
		//    //record.Name = DataConverter.Cast<string>( column[0] );
		//    record.StrandPos = DataConverter.Cast<int>( column[0] );
		//    record.StrandX = DataConverter.Cast<double>( column[1] );
		//    record.StrandY = DataConverter.Cast<double>( column[2] );
		//    record.StrandDimension = DataConverter.Cast<double>( column[3] );
		//    record.StrandQuality = DataConverter.Cast<string>( column[4] );
		//    record.StrandPrestressing = DataConverter.Cast<double>( column[5] );
		//    return record;
		//}

		//public int DeleteFormStrandStd( RecProductionFormStd recProductionFormStd, RecProductionFormStrandStd recProductionFormStrandStd )
		//{
		//    return 0;
		//}

        private bool FindProductionFormStrand( RecProductionFormStrandStd record )
        {
			using( ImpactDatabase database = new ImpactDatabase() )
			{
				var query = new ImpactQuery()
				{
					From = { ImpProductionFormStrand.As( "T1" ) },
					Where =
					{
						ImpProductionFormStrand.Factory.Equal( record.Factory ),
						ImpProductionFormStrand.Project.Equal( record.Factory ),
						ImpProductionFormStrand.Form.Equal( record.Name ),
						ImpProductionFormStrand.Strand.Equal( record.StrandPos.ToString() ),
					},
				};

				var statement = query.ToString();
				record = database.GetFirst( statement, column => new RecProductionFormStrandStd() );
			}

			return record != null;
        }

        private int InsertProductionFormStrand ( RecProductionFormStrandStd record )
        {
			var insert = new ImpactInsert( ImpProductionFormStrand.Instance )
			{
				Columns = 
				{
					{ ImpProductionFormStrand.Factory, record.Factory },
					{ ImpProductionFormStrand.Project, record.Factory },// (factory, factory)
					{ ImpProductionFormStrand.Form, record.Name }, 
					{ ImpProductionFormStrand.Strand, record.StrandPos.ToString() },
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

        private int DeleteProductionFormStrand  ( RecProductionFormStrandStd record )
        {
			int ret = 0;
			// Now let's delete the transport 
			using( ImpactDatabase database = new ImpactDatabase() )
			{
				ImpactDelete delete = new ImpactDelete( ImpProductionFormStrand.Instance )
				{
					Where = { ImpProductionFormStrand.Factory.Equal( record.Factory ), 
							  ImpProductionFormStrand.Project.Equal( record.Factory ), // (factory, factory)
							  ImpProductionFormStrand.Form.Equal( record.Name ),
                              ImpProductionFormStrand.Strand.Equal( record.StrandPos.ToString() )},
				};
				var statement = delete.ToString();
				ret = database.ExecuteNonQuery( statement );
			}
			return ret;
        }

	    private int UpdateProductionFormStrand( RecProductionFormStrandStd record )
        {
            if( null == record || string.IsNullOrEmpty( record.Factory ) || string.IsNullOrEmpty( record.Name ) || record.StrandPos < 0 )
            {
                return 0;
            }

	        var found = FindProductionFormStrand( record );
            if( record.IsUsed )
            {
                if( !found )
                {
                    InsertProductionFormStrand( record );
                }
            }
            else
            {
                if( found )
                {
                    DeleteProductionFormStrand( record );
                }
            }
            return 0;
        }
	}
}
