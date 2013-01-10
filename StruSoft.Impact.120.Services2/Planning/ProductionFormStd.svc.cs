using System.Collections.Generic;
using System.Text;
using StruSoft.Impact.V120.DB;
using StruSoft.Impact.V120.DB.Query;
using StruSoft.Impact.V120.Planning.Common;
using System.Data.Common;

namespace StruSoft.Impact.V120.Services
{
    using StruSoft.Impact.DataTypes;

    /// <summary>
	/// Used to modify records of type RecProductionFormStd.
	/// </summary>
	public partial class ProjectManager : IProductionFormStd
	{
        /// <summary>
        /// Load Slab Style
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="project"></param>
        /// <returns></returns>
		private IEnumerable<RecSectionStyleStd> GetSlabStyles(  string factory, string project  )
		{
			// Load Slab Style
			var slabStyleSvc = new ProjectManager();
			var slabStyles = slabStyleSvc.LoadSlabStyleStd( factory, project );
			var styles = new List<RecSectionStyleStd>();
			foreach( var ss in slabStyles )
			{
 			    var rec = new RecSectionStyleStd
				    {
				        Factory = ss.Factory,
				        Project = ss.Project,
				        ElementType = ss.ElementType,
				        Name = ss.Name,
                        DirectionArrowJustify = ss.DirectionArrowJustify,
				        SectionType = "",
				        Description = ss.Description,
				        StrandSpacing = ss.StrandSpacing,
				        StrandEdgeDistance = ss.StrandEdgeDistance,
				        Strandpatterns = new List<Strandpattern>(),
				    };

			    //int numOfStrands = 0
                //if( rec
                //rec.Strandpatterns.Add( new Strandpattern( "", rec.NumOfStrands ) );
				styles.Add( rec );

			}
			return styles;
		}

        /// <summary>
        /// Load Wall Style
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="project"></param>
        /// <returns></returns>
		private IEnumerable<RecSectionStyleStd> GetWallStyles( string factory, string project )
		{
			// Load Wall Style
            var wallStyleSvc = new ProjectManager();
			var wallStyles = wallStyleSvc.LoadWallStyleStd( factory, project );
			var styles = new List<RecSectionStyleStd>();
			foreach( var ss in wallStyles )
			{
				var rec = new RecSectionStyleStd
				{
 					Factory = ss.Factory,
					Project = ss.Project,

 					ElementType = ss.ElementType,
					Name = ss.Name,

 					SectionType = "",
					Description = ss.Description,
				};
				styles.Add( rec );

			}
			return styles;
		}

        /// <summary>
        /// Load Section Styles
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public List<RecSectionStyleStd> LoadSectionStyles( string factory, string project )
        {
            // Hollowcore (HD/F), Prestressed Slab (D/F)
            // (1) Load Style from IMP_SECTION_STYLE_STD
            // (2) Load Sttrandpattern and No of strands from
            // IMP_SECTION_STRANDPTN_STD.NAME, IMP_SECTION_STRANDPTN_POS_STD->Count ([STRAND_POS])
            var styleSvc = new ProjectManager();
			var styles = styleSvc.LoadSectionStyleStd( factory, project );

			// Load Slab Style
            // 3 kinds of slabs
			styles.AddRange( GetSlabStyles( factory, project ) );
			// Load Wall Style
            // 1 kind
			styles.AddRange( GetWallStyles( factory, project ) );

            return styles;
        }

	    /// <summary>
	    /// Load all records of the same factory and project as the supplied record.
	    /// </summary>
	    /// <param name="filter"> </param>
	    /// <returns>A list of all mathcing records.</returns>
	    public ProductionFormStdData LoadProductionFormStd( BedFilter filter )
		{
            var formData = new ProductionFormStdData
                               { SectionStyles = this.LoadSectionStyles( filter.Factory, filter.Project ) };
            //Load SectionStyleStd
	        //Load forms
            var query = new ImpactQuery()
			{
				Select =
				{
					ImpProductionFormStd.Factory,
					ImpProductionFormStd.Project,
					ImpProductionFormStd.Name,
					ImpProductionFormStd.FormType,
					ImpProductionFormStd.Description,
					ImpProductionFormStd.Location,
					ImpProductionFormStd.NbrOfShift,
					ImpProductionFormStd.Length,
					ImpProductionFormStd.Width,
					ImpProductionFormStd.Height,
					ImpProductionFormStd.Tolerance,
					ImpProductionFormStd.MaxLength,
					ImpProductionFormStd.MaxWidth,
					ImpProductionFormStd.MaxHeight,
					ImpProductionFormStd.MaxMass,
					ImpProductionFormStd.ElementType,
					ImpProductionFormStd.Style,
					ImpProductionFormStd.StrandType,
					ImpProductionFormStd.Strandptn,
					ImpProductionFormStd.Division,
					ImpProductionFormStd.CreatedBy,
					ImpProductionFormStd.CreatedDate,
					ImpProductionFormStd.ChangedBy,
					ImpProductionFormStd.ChangedDate,

                    Aggregate.Count( ImpProductionFormStrand.Strand ),

				},
				From  = { ImpProductionFormStd.As( "T1" ) },
                Join =
				{
					Join.Left( ImpProductionFormStrand.As( "T2" ),	
					ImpProductionFormStrand.Factory.Equal( ImpProductionFormStd.Factory ),
					ImpProductionFormStrand.Project.Equal( ImpProductionFormStd.Project ),
					ImpProductionFormStrand.Form.Equal( ImpProductionFormStd.Name ) ),

					Join.Left( ImpProductionFormStrandStd.As( "T3" ),	
					ImpProductionFormStrandStd.Factory.Equal( ImpProductionFormStrand.Factory ),
					ImpProductionFormStrandStd.Project.Equal( ImpProductionFormStrand.Project ),
					ImpProductionFormStrandStd.Name.Equal( ImpProductionFormStrand.Form ),
					ImpProductionFormStrandStd.StrandPos.Equal( ImpProductionFormStrand.Strand ) ),
                },
				Where = { ImpProductionFormStd.Factory.Equal( filter.Factory ), ImpProductionFormStd.Project.Equal( filter.Factory ) },//Factory, Factory
                GroupBy = 
                {
					ImpProductionFormStd.Factory,
					ImpProductionFormStd.Project,
					ImpProductionFormStd.Name,
					ImpProductionFormStd.FormType,
					ImpProductionFormStd.Description,
					ImpProductionFormStd.Location,
					ImpProductionFormStd.NbrOfShift,
					ImpProductionFormStd.Length,
					ImpProductionFormStd.Width,
					ImpProductionFormStd.Height,
					ImpProductionFormStd.Tolerance,
					ImpProductionFormStd.MaxLength,
					ImpProductionFormStd.MaxWidth,
					ImpProductionFormStd.MaxHeight,
					ImpProductionFormStd.MaxMass,
					ImpProductionFormStd.ElementType,
					ImpProductionFormStd.Style,
					ImpProductionFormStd.StrandType,
					ImpProductionFormStd.Strandptn,
					ImpProductionFormStd.Division,
					ImpProductionFormStd.CreatedBy,
					ImpProductionFormStd.CreatedDate,
					ImpProductionFormStd.ChangedBy,
					ImpProductionFormStd.ChangedDate,
                }

			};

            if (null != filter && !string.IsNullOrWhiteSpace(filter.Location) && !filter.Location.Equals(Filter.All))
            {
                query.Where.Add(ImpProductionFormStd.Location.Equal(filter.Location));
            }

			string statement = query.ToString();

			List<RecProductionFormStd> result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.GetAll( statement, ParseProductionFormStd );
			}
            formData.Forms = result;

			return formData;
		}

        /// <summary>
        /// Parses one row in <see cref="System.Data.Common.DbDataReader"/> into
        /// a new instance of <see cref="Paths.Common.Records.RecProductionFormStd"></see>.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public static RecProductionFormStd ParseProductionFormStd( DbDataReader dataReader )
		{
			var record = new RecProductionFormStd();
			record.Factory = DataConverter.Cast<string>( dataReader[0] );
			record.Project = DataConverter.Cast<string>( dataReader[1] );
			record.Name = DataConverter.Cast<string>( dataReader[2] );
			record.FormType = DataConverter.Cast<int>( dataReader[3] );
			record.Description = DataConverter.Cast<string>( dataReader[4] );
			record.Location = DataConverter.Cast<string>( dataReader[5] );
			record.NbrOfShift = DataConverter.Cast<int>( dataReader[6] );
			record.Length = DataConverter.Cast<double>( dataReader[7] );
			record.Width = DataConverter.Cast<double>( dataReader[8] );
			record.Height = DataConverter.Cast<double>( dataReader[9] );
			record.Tolerance = DataConverter.Cast<double>( dataReader[10] );
			record.MaxLength = DataConverter.Cast<double>( dataReader[11] );
			record.MaxWidth = DataConverter.Cast<double>( dataReader[12] );
			record.MaxHeight = DataConverter.Cast<double>( dataReader[13] );
			record.MaxMass = DataConverter.Cast<double>( dataReader[14] );
			record.ElementType = DataConverter.Cast<string>( dataReader[15] );
			record.Style = DataConverter.Cast<string>( dataReader[16] );
			record.StrandType = DataConverter.Cast<int>( dataReader[17] );
			record.Strandptn = DataConverter.Cast<string>( dataReader[18] );
			record.Division = DataConverter.Cast<string>( dataReader[19] );
			record.CreatedBy = DataConverter.Cast<string>( dataReader[20] );
			record.CreatedDate = DataConverter.Cast<System.DateTime?>( dataReader[21] );
			record.ChangedBy = DataConverter.Cast<string>( dataReader[22] );
			record.ChangedDate = DataConverter.Cast<System.DateTime?>( dataReader[23] );

            record.NumOfStrands = DataConverter.Cast<int>( dataReader[24] );
			return record;
		}

		/// <summary>
		/// Insert the specified record into the database.
		/// </summary>
		/// <param name="record">The record to insert into the database.</param>
		/// <returns>The number of affected records.</returns>
		public int InsertProductionFormStd( RecProductionFormStd record )
		{
			var insert = new ImpactInsert( ImpProductionFormStd.Instance )
			{
				Columns = 
				{
					{ ImpProductionFormStd.Factory, record.Factory },
					{ ImpProductionFormStd.Project, record.Factory }, // Factory level ie(Factory, Factory)!!!
					{ ImpProductionFormStd.Name, record.Name },
					{ ImpProductionFormStd.FormType, record.FormType },
					{ ImpProductionFormStd.Description, record.Description },
					{ ImpProductionFormStd.Location, record.Location },
					{ ImpProductionFormStd.NbrOfShift, record.NbrOfShift },
					{ ImpProductionFormStd.Length, record.Length },
					{ ImpProductionFormStd.Width, record.Width },
					{ ImpProductionFormStd.Height, record.Height },
					{ ImpProductionFormStd.Tolerance, record.Tolerance },
					{ ImpProductionFormStd.MaxLength, record.MaxLength },
					{ ImpProductionFormStd.MaxWidth, record.MaxWidth },
					{ ImpProductionFormStd.MaxHeight, record.MaxHeight },
					{ ImpProductionFormStd.MaxMass, record.MaxMass },
					{ ImpProductionFormStd.ElementType, record.ElementType },
					{ ImpProductionFormStd.Style, record.Style },
					{ ImpProductionFormStd.StrandType, record.StrandType },
					{ ImpProductionFormStd.Strandptn, record.Strandptn },
					{ ImpProductionFormStd.Division, record.Division },
					{ ImpProductionFormStd.CreatedBy, record.CreatedBy },
					{ ImpProductionFormStd.CreatedDate, record.CreatedDate },
					{ ImpProductionFormStd.ChangedBy, record.ChangedBy },
					{ ImpProductionFormStd.ChangedDate, record.ChangedDate },
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
		/// This method should be optimized
		/// </summary>
		/// <param name="record">The record to delete from the database.</param>
		/// <returns>The number of affected records.</returns>
		public int DeleteProductionFormStd( RecProductionFormStd record )
		{
			ProjectManager castSvc = new ProjectManager( );
			int count = castSvc.GetCastCount( record );
			if( count > 0 )
			{
				return 0;
			}

			// Delete form std strands
			RecProductionFormStrandStd strand = new RecProductionFormStrandStd( );
			strand.Factory = record.Factory;
			strand.Project = record.Project;
			strand.Name = record.Name;
			strand.StrandPos = 0;//Means delete all strands related to this form
			ProjectManager svc = new ProjectManager();
			svc.DeleteProductionFormStrandStd( strand );

			// (2) Now delete form
			var delete = new ImpactDelete( ImpProductionFormStd.Instance )
			{
				Where = 
				{
					{ ImpProductionFormStd.Factory.Equal( record.Factory )},
					{ ImpProductionFormStd.Project.Equal( record.Factory )}, //Factory Level, ie(Factory, Factory)
					{ ImpProductionFormStd.Name.Equal( record.Name )},
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
		public int BulkDeleteProductionFormStd1( List<RecProductionFormStd> list )
		{
			int result = 0;

			foreach( var record in list )
			{
				result += this.DeleteProductionFormStd( record );
			}

			return result;
		}
		/// <summary>
		/// Update the specified record in the database.
		/// </summary>
		/// <param name="record">The record to update.</param>
		/// <returns></returns>
		public int UpdateProductionFormStd( RecProductionFormStd record )
		{
			var update = new ImpactUpdate( ImpProductionFormStd.Instance )
			{
				Columns = 
				{
					{ ImpProductionFormStd.FormType, record.FormType },
					{ ImpProductionFormStd.Description, record.Description },
					{ ImpProductionFormStd.Location, record.Location },
					{ ImpProductionFormStd.NbrOfShift, record.NbrOfShift },
					{ ImpProductionFormStd.Length, record.Length },
					{ ImpProductionFormStd.Width, record.Width },
					{ ImpProductionFormStd.Height, record.Height },
					{ ImpProductionFormStd.Tolerance, record.Tolerance },
					{ ImpProductionFormStd.MaxMass, record.MaxMass },
					{ ImpProductionFormStd.ElementType, record.ElementType },
					{ ImpProductionFormStd.Style, record.Style },
					{ ImpProductionFormStd.StrandType, record.StrandType },
					{ ImpProductionFormStd.Strandptn, record.Strandptn },
					{ ImpProductionFormStd.Division, record.Division },
					{ ImpProductionFormStd.CreatedBy, record.CreatedBy },
					{ ImpProductionFormStd.CreatedDate, record.CreatedDate },
					{ ImpProductionFormStd.ChangedBy, record.ChangedBy },
					{ ImpProductionFormStd.ChangedDate, record.ChangedDate },
				},
				Where = 
				{
					{ ImpProductionFormStd.Factory.Equal( record.Factory ) },
					{ ImpProductionFormStd.Project.Equal( record.Factory ) },//Factory Leveö
					{ ImpProductionFormStd.Name.Equal( record.Name ) },
				},
			};

            // Never change bed dimensions if it is used in some casts!
			ProjectManager castSvc = new ProjectManager( );
			int numOfExistingCasts = castSvc.GetCastCount( record );
            if( 0 == numOfExistingCasts )
			{
                update.Columns.Add(ImpProductionFormStd.MaxLength, record.MaxLength);
                update.Columns.Add(ImpProductionFormStd.MaxWidth, record.MaxWidth);
                update.Columns.Add(ImpProductionFormStd.MaxHeight, record.MaxHeight);
			}

			string statement = update.ToString();

			int result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.ExecuteNonQuery( statement );
			}

			return result;
		}

		public int BulkUpdateProductionFormStd( List<RecProductionFormStd> list )
		{
			int result = 0;

			foreach( var record in list )
			{
				result += this.UpdateProductionFormStd( record );
			}

			return result;
		}
	}
}
