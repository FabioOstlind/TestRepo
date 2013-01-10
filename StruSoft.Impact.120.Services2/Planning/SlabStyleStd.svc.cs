using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using StruSoft.Impact.V120.Planning.Common;
using StruSoft.Impact.V120.DB;
using StruSoft.Impact.V120.DB.Query;

namespace StruSoft.Impact.V120.Services
{
	/// <summary>
	/// Used to modify records of type RecSlabStyleStd.
	/// </summary>
	public partial class ProjectManager : ISlabStyleStd
	{
	    /// <summary> 
	    /// Load all records of the same factory and project as the supplied record.
	    /// </summary>
	    /// <param name="factory"> </param>
	    /// <param name="project"> </param>
	    /// <returns>A list of all mathcing records.</returns>
	    public List<RecSlabStyleStd> LoadSlabStyleStd( string factory, string project )
		{
			var wgElementType = WhereGroup.Or(	ImpSlabStyleStd.ElementType.Equal( "PB/F" ), 
												ImpSlabStyleStd.ElementType.Equal( "PB" ),
												ImpSlabStyleStd.ElementType.Equal( "D" ));

			var query = new ImpactQuery()
			{
				Select =
				{
					ImpSlabStyleStd.Factory,
					ImpSlabStyleStd.Project,
					ImpSlabStyleStd.ElementType,
					ImpSlabStyleStd.Name,
					ImpSlabStyleStd.Description,
                    ImpSlabStyleStd.StrandSpacing,
                    ImpSlabStyleStd.StrandEdgeDistance,
                    ImpSlabStyleStd.DirectionArrowJustify,
				},
				From = { ImpSlabStyleStd.As( "T1" ) },
				Where = 
				{ 
					wgElementType 
				}
			};

			var statement = query.ToString();

			// To be rewritten in a better way!
			// Add, Factory, Factory and Company, Company
			var company = factory.Substring( 0, 2 ) + "00"; //Is this really OK ??!!


			var projectLevel = string.Format( "( T1.FACTORY = '{0}' AND T1.PROJECT = '{1}')", factory, project );
			var factoryLevel = string.Format( "( T1.FACTORY = '{0}' AND T1.PROJECT = '{1}')", factory, factory );
			var companyLevel = string.Format( "( T1.FACTORY = '{0}' AND T1.PROJECT = '{1}')", company, company );
			statement += " AND (" + projectLevel + " OR " + factoryLevel + " OR " + companyLevel + ")";

			List<RecSlabStyleStd> result;

			using( var database = new ImpactDatabase() )
			{
				result = database.GetAll( statement, ParseSlabStyleStd );
			}

			if( result == null || result.Count == 0 )
			{
				return result;
			}

			var companyDic = ( from o in result
							    where ( o.Factory == company && o.Project == company ) 
							    select new RecSlabStyleStd { Factory = o.Factory, Project = o.Factory, ElementType = o.ElementType, Name = o.Name,
                                StrandSpacing = o.StrandSpacing, StrandEdgeDistance = o.StrandEdgeDistance, DirectionArrowJustify = o.DirectionArrowJustify } ).ToDictionary( x => x.Name );

			var factoryDic = ( from o in result
								where ( o.Factory == factory && o.Project == factory )
								select new RecSlabStyleStd { Factory = o.Factory, Project = o.Factory, ElementType = o.ElementType, Name = o.Name,
                                StrandSpacing = o.StrandSpacing, StrandEdgeDistance = o.StrandEdgeDistance, DirectionArrowJustify = o.DirectionArrowJustify } ).ToDictionary( x => x.Name );

			var projectDic = ( from o in result
								where ( o.Factory == factory && o.Project == project )
								select new RecSlabStyleStd { Factory = o.Factory, Project = o.Project, ElementType = o.ElementType, Name = o.Name,
                                StrandSpacing = o.StrandSpacing, StrandEdgeDistance = o.StrandEdgeDistance, DirectionArrowJustify = o.DirectionArrowJustify } ).ToDictionary( x => x.Name );

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

			var list = projectDic.Values.ToList();
			list.OrderBy( p => p.Name );
			return list;
		}

        /// <summary>
        /// Parser
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
		public static RecSlabStyleStd ParseSlabStyleStd( DbDataReader dataReader )
		{
			var record = new RecSlabStyleStd
			                 {
			                     Factory = dataReader[0].Cast<string>(),
			                     Project = dataReader[1].Cast<string>(),
			                     ElementType = dataReader[2].Cast<string>(),
			                     Name = dataReader[3].Cast<string>(),
			                     Description = dataReader[4].Cast<string>(),
			                     StrandSpacing = dataReader[5].Cast<double>(),
			                     StrandEdgeDistance = dataReader[6].Cast<double>(),
                                 DirectionArrowJustify = dataReader[7].Cast<int>(),
			                 };
		    //record.Width = DataConverter.Cast<double>( dataReader[5] );
			//record.Tolerance = DataConverter.Cast<double>( dataReader[6] );
			//record.ElementThickness = DataConverter.Cast<double>( dataReader[7] );
			//record.Thickness = DataConverter.Cast<double>( dataReader[8] );
			//record.Material = DataConverter.Cast<string>( dataReader[9] );
			//record.EndStopCim = DataConverter.Cast<string>( dataReader[10] );
			//record.DirectionArrowCim = DataConverter.Cast<string>( dataReader[11] );
			//record.DirectionArrowJustify = DataConverter.Cast<int>( dataReader[12] );
			//record.ProductionLine = DataConverter.Cast<string>( dataReader[13] );
			//record.RcPrimary = DataConverter.Cast<int>( dataReader[14] );
			//record.RcSecondary = DataConverter.Cast<int>( dataReader[15] );
			//record.RcBentEnds = DataConverter.Cast<int>( dataReader[16] );
			//record.CoverBottom = DataConverter.Cast<double>( dataReader[17] );
			//record.PlacementLower = DataConverter.Cast<int>( dataReader[18] );
			//record.RcMeshAddPrimary = DataConverter.Cast<int>( dataReader[19] );
			//record.RcMeshAddSecondary = DataConverter.Cast<int>( dataReader[20] );
			//record.PrimaryRcDistribution = DataConverter.Cast<int>( dataReader[21] );
			//record.PrimaryRcDimension = DataConverter.Cast<double>( dataReader[22] );
			//record.PrimaryRcQuality = DataConverter.Cast<string>( dataReader[23] );
			//record.PrimaryRcSpacing = DataConverter.Cast<double>( dataReader[24] );
			//record.PrimaryRcEdgeDistance = DataConverter.Cast<double>( dataReader[25] );
			//record.PrimaryRcCoverCut = DataConverter.Cast<double>( dataReader[26] );
			//record.PrimaryLength1Left = DataConverter.Cast<double>( dataReader[27] );
			//record.PrimaryLength2Left = DataConverter.Cast<double>( dataReader[28] );
			//record.PrimaryLength1Right = DataConverter.Cast<double>( dataReader[29] );
			//record.PrimaryLength2Right = DataConverter.Cast<double>( dataReader[30] );
			//record.PrimaryAngle1Left = DataConverter.Cast<double>( dataReader[31] );
			//record.PrimaryAngle2Left = DataConverter.Cast<double>( dataReader[32] );
			//record.PrimaryAngle1Right = DataConverter.Cast<double>( dataReader[33] );
			//record.PrimaryAngle2Right = DataConverter.Cast<double>( dataReader[34] );
			//record.SecondaryRcDistribution = DataConverter.Cast<int>( dataReader[35] );
			//record.SecondaryRcDimension = DataConverter.Cast<double>( dataReader[36] );
			//record.SecondaryRcQuality = DataConverter.Cast<string>( dataReader[37] );
			//record.SecondaryRcSpacing = DataConverter.Cast<double>( dataReader[38] );
			//record.SecondaryRcEdgeDistance = DataConverter.Cast<double>( dataReader[39] );
			//record.SecondaryRcCoverCut = DataConverter.Cast<double>( dataReader[40] );
			//record.LatticeGirderCim = DataConverter.Cast<string>( dataReader[41] );
			//record.LatticeGirderEdgeDistance = DataConverter.Cast<double>( dataReader[42] );
			//record.LatticeGirderNumberOf = DataConverter.Cast<int>( dataReader[43] );
			//record.LatticeGirderCover = DataConverter.Cast<double>( dataReader[44] );
			//record.LatticeGirderEdgeMax = DataConverter.Cast<double>( dataReader[45] );
			//record.LatticeGirderEdgeMin = DataConverter.Cast<double>( dataReader[46] );
			//record.LatticeGirderAddPrimary = DataConverter.Cast<int>( dataReader[47] );
			//record.StrandDistribution = DataConverter.Cast<int>( dataReader[48] );
			//record.StrandDimension = DataConverter.Cast<double>( dataReader[49] );
			//record.StrandQuality = DataConverter.Cast<string>( dataReader[50] );
			//record.StrandPrestressing = DataConverter.Cast<double>( dataReader[53] );
			//record.StrandZ = DataConverter.Cast<double>( dataReader[54] );
			//record.LiftRotation = DataConverter.Cast<double>( dataReader[55] );
			//record.LiftCim = DataConverter.Cast<string>( dataReader[56] );
			//record.LiftPlacingSs = DataConverter.Cast<int>( dataReader[57] );
			//record.LiftPlacingParameterSs = DataConverter.Cast<double>( dataReader[58] );
			//record.LiftPlacingLs = DataConverter.Cast<int>( dataReader[59] );
			//record.LiftPlacingParameterLs = DataConverter.Cast<double>( dataReader[60] );
			//record.LiftLargeElements = DataConverter.Cast<int>( dataReader[61] );
			//record.LiftMassLimit = DataConverter.Cast<double>( dataReader[62] );
			//record.LiftWidthLimit = DataConverter.Cast<double>( dataReader[63] );
			//record.LiftLengthLimit = DataConverter.Cast<double>( dataReader[64] );
			//record.LiftSpacing = DataConverter.Cast<double>( dataReader[65] );
			//record.LiftType = DataConverter.Cast<string>( dataReader[66] );
			//record.ElementMarkPrefix = DataConverter.Cast<string>( dataReader[67] );
			//record.ElementGrp = DataConverter.Cast<string>( dataReader[68] );
			//record.ProductPrefix = DataConverter.Cast<string>( dataReader[69] );
			//record.Product = DataConverter.Cast<string>( dataReader[70] );
			//record.DrawingNamePrefix = DataConverter.Cast<string>( dataReader[71] );
			//record.DrawingType = DataConverter.Cast<string>( dataReader[72] );
			//record.DrawingTemplate = DataConverter.Cast<string>( dataReader[73] );
			//record.EdgeStandardBetweenElements = DataConverter.Cast<string>( dataReader[74] );
			//record.EdgeStandardOther = DataConverter.Cast<string>( dataReader[75] );
			//record.CreatedBy = DataConverter.Cast<string>( dataReader[76] );
			//record.CreatedDate = DataConverter.Cast<System.DateTime?>( dataReader[77] );
			//record.ChangedBy = DataConverter.Cast<string>( dataReader[78] );
			//record.ChangedDate = DataConverter.Cast<System.DateTime?>( dataReader[79] );
			return record;
		}

		/// <summary>
		/// Load all records of the same factory and project as the supplied record.
		/// </summary>
		/// <param name="record">A record with factory and project set.</param>
		/// <returns>A list of all mathcing records.</returns>
		public List<RecSlabStyleStd> Load_dummy( RecSlabStyleStd record )
		{
			ImpactQuery query = new ImpactQuery()
			{
				Select =
				{
					ImpSlabStyleStd.Factory,
					ImpSlabStyleStd.Project,
					ImpSlabStyleStd.ElementType,
					ImpSlabStyleStd.Name,
					ImpSlabStyleStd.Description,
					ImpSlabStyleStd.Width,
					ImpSlabStyleStd.Tolerance,
					ImpSlabStyleStd.ElementThickness,
					ImpSlabStyleStd.Thickness,
					ImpSlabStyleStd.Material,
					ImpSlabStyleStd.EndStopCim,
					ImpSlabStyleStd.DirectionArrowCim,
					ImpSlabStyleStd.DirectionArrowJustify,
					ImpSlabStyleStd.ProductionLine,
					ImpSlabStyleStd.RcPrimary,
					ImpSlabStyleStd.RcSecondary,
					ImpSlabStyleStd.RcBentEnds,
					ImpSlabStyleStd.CoverBottom,
					ImpSlabStyleStd.PlacementLower,
					ImpSlabStyleStd.RcMeshAddPrimary,
					ImpSlabStyleStd.RcMeshAddSecondary,
					ImpSlabStyleStd.PrimaryRcDistribution,
					ImpSlabStyleStd.PrimaryRcDimension,
					ImpSlabStyleStd.PrimaryRcQuality,
					ImpSlabStyleStd.PrimaryRcSpacing,
					ImpSlabStyleStd.PrimaryRcEdgeDistance,
					ImpSlabStyleStd.PrimaryRcCoverCut,
					ImpSlabStyleStd.PrimaryLength1Left,
					ImpSlabStyleStd.PrimaryLength2Left,
					ImpSlabStyleStd.PrimaryLength1Right,
					ImpSlabStyleStd.PrimaryLength2Right,
					ImpSlabStyleStd.PrimaryAngle1Left,
					ImpSlabStyleStd.PrimaryAngle2Left,
					ImpSlabStyleStd.PrimaryAngle1Right,
					ImpSlabStyleStd.PrimaryAngle2Right,
					ImpSlabStyleStd.SecondaryRcDistribution,
					ImpSlabStyleStd.SecondaryRcDimension,
					ImpSlabStyleStd.SecondaryRcQuality,
					ImpSlabStyleStd.SecondaryRcSpacing,
					ImpSlabStyleStd.SecondaryRcEdgeDistance,
					ImpSlabStyleStd.SecondaryRcCoverCut,
					ImpSlabStyleStd.LatticeGirderCim,
					ImpSlabStyleStd.LatticeGirderEdgeDistance,
					ImpSlabStyleStd.LatticeGirderNumberOf,
					ImpSlabStyleStd.LatticeGirderCover,
					ImpSlabStyleStd.LatticeGirderEdgeMax,
					ImpSlabStyleStd.LatticeGirderEdgeMin,
					ImpSlabStyleStd.LatticeGirderAddPrimary,
					ImpSlabStyleStd.StrandDistribution,
					ImpSlabStyleStd.StrandDimension,
					ImpSlabStyleStd.StrandQuality,
					ImpSlabStyleStd.StrandSpacing,
					ImpSlabStyleStd.StrandEdgeDistance,
					ImpSlabStyleStd.StrandPrestressing,
					ImpSlabStyleStd.StrandZ,
					ImpSlabStyleStd.LiftRotation,
					ImpSlabStyleStd.LiftCim,
					ImpSlabStyleStd.LiftPlacingSs,
					ImpSlabStyleStd.LiftPlacingParameterSs,
					ImpSlabStyleStd.LiftPlacingLs,
					ImpSlabStyleStd.LiftPlacingParameterLs,
					ImpSlabStyleStd.LiftLargeElements,
					ImpSlabStyleStd.LiftMassLimit,
					ImpSlabStyleStd.LiftWidthLimit,
					ImpSlabStyleStd.LiftLengthLimit,
					ImpSlabStyleStd.LiftSpacing,
					ImpSlabStyleStd.LiftType,
					ImpSlabStyleStd.ElementMarkPrefix,
					ImpSlabStyleStd.ElementGrp,
					ImpSlabStyleStd.ProductPrefix,
					ImpSlabStyleStd.Product,
					ImpSlabStyleStd.DrawingNamePrefix,
					ImpSlabStyleStd.DrawingType,
					ImpSlabStyleStd.DrawingTemplate,
					ImpSlabStyleStd.EdgeStandardBetweenElements,
					ImpSlabStyleStd.EdgeStandardOther,
					ImpSlabStyleStd.CreatedBy,
					ImpSlabStyleStd.CreatedDate,
					ImpSlabStyleStd.ChangedBy,
					ImpSlabStyleStd.ChangedDate,

				},
				From  = { ImpSlabStyleStd.As( "T1" ) },
				Where = { ImpSlabStyleStd.Factory.Equal( record.Factory ), ImpSlabStyleStd.Project.Equal( record.Factory ) }
			};

			string statement = query.ToString();

			List<RecSlabStyleStd> result;

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.GetAll( statement, ParseSlabStyleStd );
			}

			return result;
		}

		/// <summary>
		/// Insert the specified record into the database.
		/// </summary>
		/// <param name="record">The record to insert into the database.</param>
		/// <returns>The number of affected records.</returns>
		public int InsertSlabStyleStd( RecSlabStyleStd record )
		{
			var insert = new ImpactInsert( ImpSlabStyleStd.Instance )
			{
				Columns = 
				{
					{ ImpSlabStyleStd.Factory, record.Factory },
					{ ImpSlabStyleStd.Project, record.Project },
					{ ImpSlabStyleStd.ElementType, record.ElementType },
					{ ImpSlabStyleStd.Name, record.Name },
					{ ImpSlabStyleStd.Description, record.Description },
					{ ImpSlabStyleStd.Width, record.Width },
					{ ImpSlabStyleStd.Tolerance, record.Tolerance },
					{ ImpSlabStyleStd.ElementThickness, record.ElementThickness },
					{ ImpSlabStyleStd.Thickness, record.Thickness },
					{ ImpSlabStyleStd.Material, record.Material },
					{ ImpSlabStyleStd.EndStopCim, record.EndStopCim },
					{ ImpSlabStyleStd.DirectionArrowCim, record.DirectionArrowCim },
					{ ImpSlabStyleStd.DirectionArrowJustify, record.DirectionArrowJustify },
					{ ImpSlabStyleStd.ProductionLine, record.ProductionLine },
					{ ImpSlabStyleStd.RcPrimary, record.RcPrimary },
					{ ImpSlabStyleStd.RcSecondary, record.RcSecondary },
					{ ImpSlabStyleStd.RcBentEnds, record.RcBentEnds },
					{ ImpSlabStyleStd.CoverBottom, record.CoverBottom },
					{ ImpSlabStyleStd.PlacementLower, record.PlacementLower },
					{ ImpSlabStyleStd.RcMeshAddPrimary, record.RcMeshAddPrimary },
					{ ImpSlabStyleStd.RcMeshAddSecondary, record.RcMeshAddSecondary },
					{ ImpSlabStyleStd.PrimaryRcDistribution, record.PrimaryRcDistribution },
					{ ImpSlabStyleStd.PrimaryRcDimension, record.PrimaryRcDimension },
					{ ImpSlabStyleStd.PrimaryRcQuality, record.PrimaryRcQuality },
					{ ImpSlabStyleStd.PrimaryRcSpacing, record.PrimaryRcSpacing },
					{ ImpSlabStyleStd.PrimaryRcEdgeDistance, record.PrimaryRcEdgeDistance },
					{ ImpSlabStyleStd.PrimaryRcCoverCut, record.PrimaryRcCoverCut },
					{ ImpSlabStyleStd.PrimaryLength1Left, record.PrimaryLength1Left },
					{ ImpSlabStyleStd.PrimaryLength2Left, record.PrimaryLength2Left },
					{ ImpSlabStyleStd.PrimaryLength1Right, record.PrimaryLength1Right },
					{ ImpSlabStyleStd.PrimaryLength2Right, record.PrimaryLength2Right },
					{ ImpSlabStyleStd.PrimaryAngle1Left, record.PrimaryAngle1Left },
					{ ImpSlabStyleStd.PrimaryAngle2Left, record.PrimaryAngle2Left },
					{ ImpSlabStyleStd.PrimaryAngle1Right, record.PrimaryAngle1Right },
					{ ImpSlabStyleStd.PrimaryAngle2Right, record.PrimaryAngle2Right },
					{ ImpSlabStyleStd.SecondaryRcDistribution, record.SecondaryRcDistribution },
					{ ImpSlabStyleStd.SecondaryRcDimension, record.SecondaryRcDimension },
					{ ImpSlabStyleStd.SecondaryRcQuality, record.SecondaryRcQuality },
					{ ImpSlabStyleStd.SecondaryRcSpacing, record.SecondaryRcSpacing },
					{ ImpSlabStyleStd.SecondaryRcEdgeDistance, record.SecondaryRcEdgeDistance },
					{ ImpSlabStyleStd.SecondaryRcCoverCut, record.SecondaryRcCoverCut },
					{ ImpSlabStyleStd.LatticeGirderCim, record.LatticeGirderCim },
					{ ImpSlabStyleStd.LatticeGirderEdgeDistance, record.LatticeGirderEdgeDistance },
					{ ImpSlabStyleStd.LatticeGirderNumberOf, record.LatticeGirderNumberOf },
					{ ImpSlabStyleStd.LatticeGirderCover, record.LatticeGirderCover },
					{ ImpSlabStyleStd.LatticeGirderEdgeMax, record.LatticeGirderEdgeMax },
					{ ImpSlabStyleStd.LatticeGirderEdgeMin, record.LatticeGirderEdgeMin },
					{ ImpSlabStyleStd.LatticeGirderAddPrimary, record.LatticeGirderAddPrimary },
					{ ImpSlabStyleStd.StrandDistribution, record.StrandDistribution },
					{ ImpSlabStyleStd.StrandDimension, record.StrandDimension },
					{ ImpSlabStyleStd.StrandQuality, record.StrandQuality },
					{ ImpSlabStyleStd.StrandSpacing, record.StrandSpacing },
					{ ImpSlabStyleStd.StrandEdgeDistance, record.StrandEdgeDistance },
					{ ImpSlabStyleStd.StrandPrestressing, record.StrandPrestressing },
					{ ImpSlabStyleStd.StrandZ, record.StrandZ },
					{ ImpSlabStyleStd.LiftRotation, record.LiftRotation },
					{ ImpSlabStyleStd.LiftCim, record.LiftCim },
					{ ImpSlabStyleStd.LiftPlacingSs, record.LiftPlacingSs },
					{ ImpSlabStyleStd.LiftPlacingParameterSs, record.LiftPlacingParameterSs },
					{ ImpSlabStyleStd.LiftPlacingLs, record.LiftPlacingLs },
					{ ImpSlabStyleStd.LiftPlacingParameterLs, record.LiftPlacingParameterLs },
					{ ImpSlabStyleStd.LiftLargeElements, record.LiftLargeElements },
					{ ImpSlabStyleStd.LiftMassLimit, record.LiftMassLimit },
					{ ImpSlabStyleStd.LiftWidthLimit, record.LiftWidthLimit },
					{ ImpSlabStyleStd.LiftLengthLimit, record.LiftLengthLimit },
					{ ImpSlabStyleStd.LiftSpacing, record.LiftSpacing },
					{ ImpSlabStyleStd.LiftType, record.LiftType },
					{ ImpSlabStyleStd.ElementMarkPrefix, record.ElementMarkPrefix },
					{ ImpSlabStyleStd.ElementGrp, record.ElementGrp },
					{ ImpSlabStyleStd.ProductPrefix, record.ProductPrefix },
					{ ImpSlabStyleStd.Product, record.Product },
					{ ImpSlabStyleStd.DrawingNamePrefix, record.DrawingNamePrefix },
					{ ImpSlabStyleStd.DrawingType, record.DrawingType },
					{ ImpSlabStyleStd.DrawingTemplate, record.DrawingTemplate },
					{ ImpSlabStyleStd.EdgeStandardBetweenElements, record.EdgeStandardBetweenElements },
					{ ImpSlabStyleStd.EdgeStandardOther, record.EdgeStandardOther },
					{ ImpSlabStyleStd.CreatedBy, record.CreatedBy },
					{ ImpSlabStyleStd.CreatedDate, record.CreatedDate },
					{ ImpSlabStyleStd.ChangedBy, record.ChangedBy },
					{ ImpSlabStyleStd.ChangedDate, record.ChangedDate },
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
		public int DeleteSlabStyleStd( RecSlabStyleStd record )
		{
			var delete = new ImpactDelete( ImpSlabStyleStd.Instance )
			{
				Where = 
				{
					{ ImpSlabStyleStd.Factory.Equal( record.Factory )},
					{ ImpSlabStyleStd.Project.Equal( record.Project )},
					{ ImpSlabStyleStd.ElementType.Equal( record.ElementType )},
					{ ImpSlabStyleStd.Name.Equal( record.Name )},
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
		public int BulkDeleteSlabStyleStd( List<RecSlabStyleStd> list )
		{
			int result = 0;

			foreach( var record in list )
			{
				result += this.DeleteSlabStyleStd( record );
			}

			return result;
		}
		/// <summary>
		/// Update the specified record in the database.
		/// </summary>
		/// <param name="record">The record to update.</param>
		/// <returns></returns>
		public int UpdateSlabStyleStd( RecSlabStyleStd record )
		{

			var update = new ImpactUpdate( ImpSlabStyleStd.Instance )
			{
				Columns = 
				{
					{ ImpSlabStyleStd.Description, record.Description },
					{ ImpSlabStyleStd.Width, record.Width },
					{ ImpSlabStyleStd.Tolerance, record.Tolerance },
					{ ImpSlabStyleStd.ElementThickness, record.ElementThickness },
					{ ImpSlabStyleStd.Thickness, record.Thickness },
					{ ImpSlabStyleStd.Material, record.Material },
					{ ImpSlabStyleStd.EndStopCim, record.EndStopCim },
					{ ImpSlabStyleStd.DirectionArrowCim, record.DirectionArrowCim },
					{ ImpSlabStyleStd.DirectionArrowJustify, record.DirectionArrowJustify },
					{ ImpSlabStyleStd.ProductionLine, record.ProductionLine },
					{ ImpSlabStyleStd.RcPrimary, record.RcPrimary },
					{ ImpSlabStyleStd.RcSecondary, record.RcSecondary },
					{ ImpSlabStyleStd.RcBentEnds, record.RcBentEnds },
					{ ImpSlabStyleStd.CoverBottom, record.CoverBottom },
					{ ImpSlabStyleStd.PlacementLower, record.PlacementLower },
					{ ImpSlabStyleStd.RcMeshAddPrimary, record.RcMeshAddPrimary },
					{ ImpSlabStyleStd.RcMeshAddSecondary, record.RcMeshAddSecondary },
					{ ImpSlabStyleStd.PrimaryRcDistribution, record.PrimaryRcDistribution },
					{ ImpSlabStyleStd.PrimaryRcDimension, record.PrimaryRcDimension },
					{ ImpSlabStyleStd.PrimaryRcQuality, record.PrimaryRcQuality },
					{ ImpSlabStyleStd.PrimaryRcSpacing, record.PrimaryRcSpacing },
					{ ImpSlabStyleStd.PrimaryRcEdgeDistance, record.PrimaryRcEdgeDistance },
					{ ImpSlabStyleStd.PrimaryRcCoverCut, record.PrimaryRcCoverCut },
					{ ImpSlabStyleStd.PrimaryLength1Left, record.PrimaryLength1Left },
					{ ImpSlabStyleStd.PrimaryLength2Left, record.PrimaryLength2Left },
					{ ImpSlabStyleStd.PrimaryLength1Right, record.PrimaryLength1Right },
					{ ImpSlabStyleStd.PrimaryLength2Right, record.PrimaryLength2Right },
					{ ImpSlabStyleStd.PrimaryAngle1Left, record.PrimaryAngle1Left },
					{ ImpSlabStyleStd.PrimaryAngle2Left, record.PrimaryAngle2Left },
					{ ImpSlabStyleStd.PrimaryAngle1Right, record.PrimaryAngle1Right },
					{ ImpSlabStyleStd.PrimaryAngle2Right, record.PrimaryAngle2Right },
					{ ImpSlabStyleStd.SecondaryRcDistribution, record.SecondaryRcDistribution },
					{ ImpSlabStyleStd.SecondaryRcDimension, record.SecondaryRcDimension },
					{ ImpSlabStyleStd.SecondaryRcQuality, record.SecondaryRcQuality },
					{ ImpSlabStyleStd.SecondaryRcSpacing, record.SecondaryRcSpacing },
					{ ImpSlabStyleStd.SecondaryRcEdgeDistance, record.SecondaryRcEdgeDistance },
					{ ImpSlabStyleStd.SecondaryRcCoverCut, record.SecondaryRcCoverCut },
					{ ImpSlabStyleStd.LatticeGirderCim, record.LatticeGirderCim },
					{ ImpSlabStyleStd.LatticeGirderEdgeDistance, record.LatticeGirderEdgeDistance },
					{ ImpSlabStyleStd.LatticeGirderNumberOf, record.LatticeGirderNumberOf },
					{ ImpSlabStyleStd.LatticeGirderCover, record.LatticeGirderCover },
					{ ImpSlabStyleStd.LatticeGirderEdgeMax, record.LatticeGirderEdgeMax },
					{ ImpSlabStyleStd.LatticeGirderEdgeMin, record.LatticeGirderEdgeMin },
					{ ImpSlabStyleStd.LatticeGirderAddPrimary, record.LatticeGirderAddPrimary },
					{ ImpSlabStyleStd.StrandDistribution, record.StrandDistribution },
					{ ImpSlabStyleStd.StrandDimension, record.StrandDimension },
					{ ImpSlabStyleStd.StrandQuality, record.StrandQuality },
					{ ImpSlabStyleStd.StrandSpacing, record.StrandSpacing },
					{ ImpSlabStyleStd.StrandEdgeDistance, record.StrandEdgeDistance },
					{ ImpSlabStyleStd.StrandPrestressing, record.StrandPrestressing },
					{ ImpSlabStyleStd.StrandZ, record.StrandZ },
					{ ImpSlabStyleStd.LiftRotation, record.LiftRotation },
					{ ImpSlabStyleStd.LiftCim, record.LiftCim },
					{ ImpSlabStyleStd.LiftPlacingSs, record.LiftPlacingSs },
					{ ImpSlabStyleStd.LiftPlacingParameterSs, record.LiftPlacingParameterSs },
					{ ImpSlabStyleStd.LiftPlacingLs, record.LiftPlacingLs },
					{ ImpSlabStyleStd.LiftPlacingParameterLs, record.LiftPlacingParameterLs },
					{ ImpSlabStyleStd.LiftLargeElements, record.LiftLargeElements },
					{ ImpSlabStyleStd.LiftMassLimit, record.LiftMassLimit },
					{ ImpSlabStyleStd.LiftWidthLimit, record.LiftWidthLimit },
					{ ImpSlabStyleStd.LiftLengthLimit, record.LiftLengthLimit },
					{ ImpSlabStyleStd.LiftSpacing, record.LiftSpacing },
					{ ImpSlabStyleStd.LiftType, record.LiftType },
					{ ImpSlabStyleStd.ElementMarkPrefix, record.ElementMarkPrefix },
					{ ImpSlabStyleStd.ElementGrp, record.ElementGrp },
					{ ImpSlabStyleStd.ProductPrefix, record.ProductPrefix },
					{ ImpSlabStyleStd.Product, record.Product },
					{ ImpSlabStyleStd.DrawingNamePrefix, record.DrawingNamePrefix },
					{ ImpSlabStyleStd.DrawingType, record.DrawingType },
					{ ImpSlabStyleStd.DrawingTemplate, record.DrawingTemplate },
					{ ImpSlabStyleStd.EdgeStandardBetweenElements, record.EdgeStandardBetweenElements },
					{ ImpSlabStyleStd.EdgeStandardOther, record.EdgeStandardOther },
					{ ImpSlabStyleStd.CreatedBy, record.CreatedBy },
					{ ImpSlabStyleStd.CreatedDate, record.CreatedDate },
					{ ImpSlabStyleStd.ChangedBy, record.ChangedBy },
					{ ImpSlabStyleStd.ChangedDate, record.ChangedDate },
				},
				Where = 
				{
					{ ImpSlabStyleStd.Factory.Equal( record.Factory ) },
					{ ImpSlabStyleStd.Project.Equal( record.Project ) },
					{ ImpSlabStyleStd.ElementType.Equal( record.ElementType ) },
					{ ImpSlabStyleStd.Name.Equal( record.Name ) },
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

		public int BulkUpdateSlabStyleStd( List<RecSlabStyleStd> list )
		{
			int result = 0;

			foreach( var record in list )
			{
				result += this.UpdateSlabStyleStd( record );
			}

			return result;
		}
	}

}
