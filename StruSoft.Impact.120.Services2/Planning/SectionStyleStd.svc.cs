using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using StruSoft.Impact.V120.Planning.Common;
using StruSoft.Impact.V120.DB;
using StruSoft.Impact.V120.DB.Query;

namespace StruSoft.Impact.V120.Services
{
	/// <summary>
	/// Used to modify records of type RecSectionStyleStd.
	/// </summary>
	public partial class ProjectManager : ISectionStyleStd
	{
		/// <summary> 
		/// Load all records of the same factory and project as the supplied record.
		/// </summary>
		/// <param name="record">A record with factory and project set.</param>
		/// <returns>A list of all mathcing records.</returns>
		public List<RecSectionStyleStd> LoadSqlServer( RecSectionStyleStd record )
		{
			if( record == null || record.Factory == null )
			{
				return null;
			}
			string company = record.Factory.Substring( 0, 2 ) + "00"; //Is this really OK ??!!

			WhereGroup wgElementType = WhereGroup.Or( ImpSectionStyleStd.ElementType.Equal( "HD/F" ),
													  ImpSectionStyleStd.ElementType.Equal( "D/F" ));

			// Prepare Factory, Project statement
			ImpactQuery projectQuery = new ImpactQuery()
			{
				Select = {
					        ImpSectionStyleStd.Factory,
					        ImpSectionStyleStd.Project,
					        ImpSectionStyleStd.ElementType,
					        ImpSectionStyleStd.Name,
					        ImpSectionStyleStd.SectionType,
					        ImpSectionStyleStd.Description,
				        },
				From = { ImpSectionStyleStd.As( "T1" ) },
				Where = {
                            ImpSectionStyleStd.Factory.Equal( record.Factory ),
					        ImpSectionStyleStd.Project.Equal( record.Project ),
							wgElementType },
			};
			string projectStatement = projectQuery.ToString();

			// Prepare Factory, Factory statement
			ImpactQuery factoryQuery = new ImpactQuery()
			{
				Select = {
					        ImpSectionStyleStd.Factory,
					        ImpSectionStyleStd.Project,
					        ImpSectionStyleStd.ElementType,
					        ImpSectionStyleStd.Name,
					        ImpSectionStyleStd.SectionType,
					        ImpSectionStyleStd.Description,
				        },
				From = { ImpSectionStyleStd.As( "T1" ) },
				Where = {
                            ImpSectionStyleStd.Factory.Equal( record.Factory ),
					        ImpSectionStyleStd.Project.Equal( record.Factory ),
							wgElementType 
                        },
			};
			string factoryStatement = factoryQuery.ToString();
			ImpactQuery projectSubquery = new ImpactQuery {
				Select = { ImpSectionStyleStd.Name },
				From = { ImpSectionStyleStd.As( "T1" ) },
				Where = {
                            ImpSectionStyleStd.Factory.Equal( record.Factory ),
					        ImpSectionStyleStd.Project.Equal( record.Project ),
							wgElementType 
                        },
			};
			factoryStatement += " AND T1.Name Not In (" + projectSubquery.ToString() + ")";

			// Prepare Company, Company statement
			ImpactQuery companyQuery = new ImpactQuery()
			{
				Select = {
					        ImpSectionStyleStd.Factory,
					        ImpSectionStyleStd.Project,
					        ImpSectionStyleStd.ElementType,
					        ImpSectionStyleStd.Name,
					        ImpSectionStyleStd.SectionType,
					        ImpSectionStyleStd.Description,
				        },
				From = { ImpSectionStyleStd.As( "T1" ) },
				Where = {
                            ImpSectionStyleStd.Factory.Equal( company ),
					        ImpSectionStyleStd.Project.Equal( company ),
							wgElementType },
			};
			string companyStatement = companyQuery.ToString();
			ImpactQuery factorySubquery = new ImpactQuery()
			{
				Select = { ImpSectionStyleStd.Name },
				From = { ImpSectionStyleStd.As( "T1" ) },
				Where = {
                            ImpSectionStyleStd.Factory.Equal( record.Factory ),
					        ImpSectionStyleStd.Project.Equal( record.Factory ),
							wgElementType },
			};
			companyStatement += " AND T1.Name Not In (" + projectSubquery.ToString() + ")"
							 + " AND T1.Name Not In (" + factorySubquery.ToString() + ")";
			string statement = projectStatement + " Union " + factoryStatement + " Union " + companyStatement + " Order By T1.ELEMENT_TYPE, T1.Name";

			List<RecSectionStyleStd> result;
			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.GetAll( statement, ParseSectionStyleStd );
			}
			return result;
		}

	    /// <summary> 
	    /// Load all records of the same factory and project as the supplied record.
	    /// </summary>
	    /// <param name="factory"> </param>
	    /// <param name="project"> </param>
	    /// <returns>A list of all mathcing records.</returns>
	    public List<RecSectionStyleStd> LoadSectionStyleStd(  string factory, string project  )
		{
			// Add, Factory, Factory and Company, Company
			var company = factory.Substring( 0, 2 ) + "00"; //Is this really OK ??!!

			var orElementType = WhereGroup.Or( ImpSectionStyleStd.ElementType.Equal( "HD/F" ),
										       ImpSectionStyleStd.ElementType.Equal( "D/F" ));

            
            var where1 = WhereGroup.And( ImpSectionStyleStd.Factory.Equal( factory ), ImpSectionStyleStd.Project.Equal( project ) );
            var where2 = WhereGroup.And( ImpSectionStyleStd.Factory.Equal( factory ), ImpSectionStyleStd.Project.Equal( factory ) );
            var where3 = WhereGroup.And( ImpSectionStyleStd.Factory.Equal( company ), ImpSectionStyleStd.Project.Equal( company ) );

            var orProjectFactoryCompany = WhereGroup.Or( where1, where2, where3 );

			var query = new ImpactQuery()
			{
				Select =
				{
					ImpSectionStyleStd.Factory,
					ImpSectionStyleStd.Project,
					ImpSectionStyleStd.ElementType,
					ImpSectionStyleStd.Name,
					ImpSectionStyleStd.SectionType,
					ImpSectionStyleStd.Description,
                    ImpSectionStrandptnStd.Name,
                    Aggregate.Count( ImpSectionStrandptnPosStd.StrandPos ),

                    //ImpSectionStyleStd.Width,
                    //ImpSectionStyleStd.WidthTolerance,
                    //ImpSectionStyleStd.DiffBottomTop,
                    //ImpSectionStyleStd.Height,
                    //ImpSectionStyleStd.Endcap,
                    //ImpSectionStyleStd.Strandptn,
                    //ImpSectionStyleStd.RcTemplate,
                    //ImpSectionStyleStd.Material,
                    //ImpSectionStyleStd.ProductionLine,
                    //ImpSectionStyleStd.CutType,
                    //ImpSectionStyleStd.UseCutZone,
                    //ImpSectionStyleStd.LiftMethod,
                    //ImpSectionStyleStd.LiftRotation1,
                    //ImpSectionStyleStd.LiftRotation2,
                    //ImpSectionStyleStd.LiftPlacingLs,
                    //ImpSectionStyleStd.LiftParameterLs,
                    //ImpSectionStyleStd.LiftPlacingSs,
                    //ImpSectionStyleStd.LiftParameterSs,
                    //ImpSectionStyleStd.LiftCores,
                    //ImpSectionStyleStd.LiftCoreLength,
                    //ImpSectionStyleStd.LiftCoreDisplayMode,
                    //ImpSectionStyleStd.LiftDistanceMax,
                    //ImpSectionStyleStd.LiftDistanceMin,
                    //ImpSectionStyleStd.LiftSpacing,
                    //ImpSectionStyleStd.LiftType,
                    //ImpSectionStyleStd.SectionViewDimStrandGrp,
                    //ImpSectionStyleStd.SectionViewTxtStrandGrp,
                    //ImpSectionStyleStd.SectionViewNbrCores,
                    //ImpSectionStyleStd.SectionViewScale,
                    //ImpSectionStyleStd.UseSectionViewSymbol,
                    //ImpSectionStyleStd.SectionViewFilename,
                    //ImpSectionStyleStd.ChamferDistance,
                    //ImpSectionStyleStd.ChamferText,
                    //ImpSectionStyleStd.ChamferVisibility,
                    //ImpSectionStyleStd.RcCoverCut1,
                    //ImpSectionStyleStd.RcCoverCut2,
                    //ImpSectionStyleStd.ElementGrp,
                    //ImpSectionStyleStd.ProductPrefix,
                    //ImpSectionStyleStd.Product,
                    //ImpSectionStyleStd.ElementMarkPrefix,
                    //ImpSectionStyleStd.DrawingNamePrefix,
                    //ImpSectionStyleStd.DrawingType,
                    //ImpSectionStyleStd.DrawingTemplate,
                    //ImpSectionStyleStd.CreatedBy,
                    //ImpSectionStyleStd.CreatedDate,
                    //ImpSectionStyleStd.ChangedBy,
                    //ImpSectionStyleStd.ChangedDate,
                    //ImpSectionStyleStd.LiftHolePosition,

				},
				From = { ImpSectionStyleStd.As( "T1" ) },
                Join =
				{
					Join.Left( ImpSectionStrandptnStd.As( "T2" ),	
						ImpSectionStrandptnStd.Factory.Equal( ImpSectionStyleStd.Factory ),
						ImpSectionStrandptnStd.Project.Equal( ImpSectionStyleStd.Project ),
						ImpSectionStrandptnStd.ElementType.Equal( ImpSectionStyleStd.ElementType ), 
						ImpSectionStrandptnStd.Section.Equal( ImpSectionStyleStd.Name )),

					Join.Left( ImpSectionStrandptnPosStd.As( "T3" ),	
						ImpSectionStrandptnPosStd.Factory.Equal( ImpSectionStrandptnStd.Factory ),
						ImpSectionStrandptnPosStd.Project.Equal( ImpSectionStrandptnStd.Project ),
						ImpSectionStrandptnPosStd.ElementType.Equal( ImpSectionStrandptnStd.ElementType ), 
						ImpSectionStrandptnPosStd.Section.Equal( ImpSectionStrandptnStd.Section ),
                        ImpSectionStrandptnPosStd.Strandptn.Equal( ImpSectionStrandptnStd.Name )),
				},

				Where = { orElementType, orProjectFactoryCompany },

                GroupBy = 
                {
                    ImpSectionStyleStd.Factory,
                    ImpSectionStyleStd.Project,
                    ImpSectionStyleStd.ElementType,
                    ImpSectionStyleStd.Name,
                    ImpSectionStyleStd.SectionType,
                    ImpSectionStyleStd.Description,
                    ImpSectionStrandptnStd.Name,
                }
			};

			var statement = query.ToString();
           
			List<RecSectionStyleStd> result;

			using( var database = new ImpactDatabase() )
			{
				result = database.GetAll( statement, ParseSectionStyleStd );
			}

			if( result == null || result.Count == 0 )
			{
				return result;
			}

            var companyDic = ( from o in result
				            where ( o.Factory == company && o.Project == company )
				            select new RecSectionStyleStd { Factory = o.Factory, Project = o.Factory, ElementType = o.ElementType, 
                                Name = o.Name, Strandptn = o.Strandptn, NumOfStrands = o.NumOfStrands } ).ToDictionary( x => x.Name + x.Strandptn );

            var factoryDic = ( from o in result
				            where ( o.Factory == factory && o.Project == factory )
				            select new RecSectionStyleStd { Factory = o.Factory, Project = o.Factory, ElementType = o.ElementType, 
                                Name = o.Name, Strandptn = o.Strandptn, NumOfStrands = o.NumOfStrands } ).ToDictionary( x => x.Name + x.Strandptn );

            var projectDic = ( from o in result
				            where ( o.Factory == factory && o.Project == project )
				            select new RecSectionStyleStd { Factory = o.Factory, Project = o.Project, ElementType = o.ElementType, 
                                Name = o.Name, Strandptn = o.Strandptn, NumOfStrands = o.NumOfStrands } ).ToDictionary( x => x.Name + x.Strandptn );

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

			return GetStylesFromDictionary(  projectDic);
		}

	    /// <summary> 
	    /// Load the strands of the given pattern.
	    /// </summary>
	    /// <param name="factory"> </param>
	    /// <param name="project"> </param>
	    /// <param name="elementType"> </param>
	    /// <param name="style"> </param>
	    /// <param name="strandpattern"> </param>
	    /// <returns>A list of all mathcing records.</returns>
	    public List<RecProductionCastStrand> LoadStrands(  string factory, string project, string elementType, string style, string strandpattern )
		{
			// Add, Factory, Factory and Company, Company
			var company = factory.Substring( 0, 2 ) + "00"; //Is this really OK ??!!
            
            var where1 = WhereGroup.And( ImpSectionStyleStd.Factory.Equal( factory ), ImpSectionStyleStd.Project.Equal( project ) );
            var where2 = WhereGroup.And( ImpSectionStyleStd.Factory.Equal( factory ), ImpSectionStyleStd.Project.Equal( factory ) );
            var where3 = WhereGroup.And( ImpSectionStyleStd.Factory.Equal( company ), ImpSectionStyleStd.Project.Equal( company ) );

            var orProjectFactoryCompany = WhereGroup.Or( where1, where2, where3 );

			var query = new ImpactQuery()
			{
				Select =
				{
					ImpSectionStyleStd.Factory,
					ImpSectionStyleStd.Project,
                    ImpSectionStyleStd.Name,
                    ImpSectionStrandptnStd.Name,
					ImpSectionStrandPosStd.StrandPos,
					ImpSectionStrandPosStd.StrandX,
					ImpSectionStrandPosStd.StrandY,
					ImpSectionStrandptnGrpStd.StrandDimension,
                    ImpSectionStrandptnGrpStd.StrandPrestressing,
                    ImpSectionStrandptnGrpStd.StrandQuality,
				},
				From = { ImpSectionStyleStd.As( "T1" ) },
                Join =
				{
					Join.Left( ImpSectionStrandptnStd.As( "T2" ),	
						ImpSectionStrandptnStd.Factory.Equal( ImpSectionStyleStd.Factory ),
						ImpSectionStrandptnStd.Project.Equal( ImpSectionStyleStd.Project ),
						ImpSectionStrandptnStd.ElementType.Equal( ImpSectionStyleStd.ElementType ), 
						ImpSectionStrandptnStd.Section.Equal( ImpSectionStyleStd.Name )),

					Join.Left( ImpSectionStrandptnPosStd.As( "T3" ),	
						ImpSectionStrandptnPosStd.Factory.Equal( ImpSectionStrandptnStd.Factory ),
						ImpSectionStrandptnPosStd.Project.Equal( ImpSectionStrandptnStd.Project ),
						ImpSectionStrandptnPosStd.ElementType.Equal( ImpSectionStrandptnStd.ElementType ), 
						ImpSectionStrandptnPosStd.Section.Equal( ImpSectionStrandptnStd.Section ),
                        ImpSectionStrandptnPosStd.Strandptn.Equal( ImpSectionStrandptnStd.Name )),

					Join.Left( ImpSectionStrandPosStd.As( "T4" ),	
						ImpSectionStrandPosStd.Factory.Equal( ImpSectionStrandptnPosStd.Factory ),
						ImpSectionStrandPosStd.Project.Equal( ImpSectionStrandptnPosStd.Project ),
						ImpSectionStrandPosStd.ElementType.Equal( ImpSectionStrandptnPosStd.ElementType ), 
						ImpSectionStrandPosStd.Section.Equal( ImpSectionStrandptnPosStd.Section ),
                        ImpSectionStrandPosStd.StrandPos.Equal( ImpSectionStrandptnPosStd.StrandPos )),

					Join.Left( ImpSectionStrandptnGrpStd.As( "T5" ),	
						ImpSectionStrandptnGrpStd.Factory.Equal( ImpSectionStrandPosStd.Factory ),
						ImpSectionStrandptnGrpStd.Project.Equal( ImpSectionStrandPosStd.Project ),
						ImpSectionStrandptnGrpStd.ElementType.Equal( ImpSectionStrandPosStd.ElementType ), 
                        ImpSectionStrandptnGrpStd.Section.Equal( ImpSectionStrandPosStd.Section ),
						ImpSectionStrandptnGrpStd.Strandptn.Equal( ImpSectionStrandptnPosStd.Strandptn ),
                        ImpSectionStrandptnGrpStd.StrandGrp.Equal( ImpSectionStrandPosStd.StrandGrp )),
				},

				Where =
				    {
				        ImpSectionStyleStd.ElementType.Equal( elementType ), 
                        ImpSectionStyleStd.Name.Equal( style ), 
                        ImpSectionStrandptnPosStd.Strandptn.Equal( strandpattern ),
                        orProjectFactoryCompany
				    },
			};

			var statement = query.ToString();
           
			List<RecProductionCastStrand> result;

			using( var database = new ImpactDatabase() )
			{
				result = database.GetAll( statement, column => new RecProductionCastStrand()
				                {
				                    Factory = column[0].Cast<string>(),
				                    Project = column[1].Cast<string>(),
				                    Style = column[2].Cast<string>(),
				                    Strandpattern = column[3].Cast<string>(),
                                    StrandPos = column[4].Cast<int>(),
                                    StrandX = column[5].Cast<double>(),
                                    StrandY = column[6].Cast<double>(),
                                    StrandDimension = column[7].Cast<double>(),
                                    StrandPrestressing = column[8].Cast<double>(),
                                    StrandQuality = column[9].Cast<string>(),

				                } );
			}

			if( result == null || result.Count == 0 )
			{
				return result;
			}

	        var strands = ( from o in result where ( o.Factory == factory && o.Project == project ) select o ).ToList();
            if( strands.Count > 0 )
            {
                return strands;
            }

            strands = ( from o in result where ( o.Factory == factory && o.Project == factory ) select o ).ToList();
            if( strands.Count > 0 )
            {
                return strands;
            }

            return ( from o in result where ( o.Factory == company && o.Project == company ) select o ).ToList();
		}

        // How is it about strand diameter/Quality? 2012-11-05
        //// Initial (default) values
        //LEFT JOIN IMP_SECTION_STRANDPTN_GRP_STD AS T5 ON 
        //T5.FACTORY = T4.FACTORY 
        //AND T5.PROJECT = T4.PROJECT 
        //AND T5.ELEMENT_TYPE = T4.ELEMENT_TYPE 
        //AND T5.SECTION = T4.SECTION 
        //AND T5.STRAND_GRP = T4.STRAND_GR

        //// Current values
        //IMP_STRANDPTN_GRP

        // 2012-11-21 TS, MN
        // Here the select that reads the strand PosX,Y,Diameter for standard settings,
        // Drawn elements may contain different info !!!

        //SELECT T1.FACTORY, T1.PROJECT, T1.ELEMENT_TYPE, T1.NAME, T1.SECTION_TYPE, T1.DESCRIPTION, T2.NAME--, COUNT( T3.STRAND_POS )
        //,T4.STRAND_GRP, T4.STRAND_POS, T4.STRAND_X, T4.STRAND_Y 
        //,T5.STRAND_DIMENSION,T5.STRAND_PRESTRESSING,T5.STRAND_QUALITY

        //FROM IMP_SECTION_STYLE_STD AS T1
        //LEFT JOIN IMP_SECTION_STRANDPTN_STD AS T2 ON 
        //T2.FACTORY = T1.FACTORY 
        //AND T2.PROJECT = T1.PROJECT 
        //AND T2.ELEMENT_TYPE = T1.ELEMENT_TYPE 
        //AND T2.SECTION = T1.NAME 

        //LEFT JOIN IMP_SECTION_STRANDPTN_POS_STD AS T3 ON 
        //T3.FACTORY = T2.FACTORY 
        //AND T3.PROJECT = T2.PROJECT 
        //AND T3.ELEMENT_TYPE = T2.ELEMENT_TYPE 
        //AND T3.SECTION = T2.SECTION 
        //AND T3.STRANDPTN = T2.NAME 

        //LEFT JOIN IMP_SECTION_STRAND_POS_STD AS T4 ON 
        //T4.FACTORY = T3.FACTORY 
        //AND T4.PROJECT = T3.PROJECT 
        //AND T4.ELEMENT_TYPE = T3.ELEMENT_TYPE 
        //AND T4.SECTION = T3.SECTION 
        //AND T4.STRAND_POS = T3.STRAND_POS

        //LEFT JOIN IMP_SECTION_STRANDPTN_GRP_STD AS T5 ON 
        //T5.FACTORY = T4.FACTORY 
        //AND T5.PROJECT = T4.PROJECT 
        //AND T5.ELEMENT_TYPE = T4.ELEMENT_TYPE 
        //AND T5.SECTION = T4.SECTION 
        //AND T5.STRANDPTN = T3.STRANDPTN 
        //AND T5.STRAND_GRP = T4.STRAND_GRP
        //WHERE
        // (  ( T1.ELEMENT_TYPE = 'HD/F'
        //OR T1.ELEMENT_TYPE = 'D/F'
        // ) 
        //AND  (  ( T1.FACTORY = '1101'
        //AND T1.PROJECT = '      IMPACT'
        // ) 
        //OR  ( T1.FACTORY = '1101'
        //AND T1.PROJECT = '1101'
        // ) 

        //OR  ( T1.FACTORY = '1100'
        //AND T1.PROJECT = '1100'
        // ) 
        // ) 
        //) 

        /// <summary>
        /// Get Styles From Dictionary
        /// </summary>
        /// <param name="projectDic"></param>
        /// <returns></returns>
        private List<RecSectionStyleStd> GetStylesFromDictionary(Dictionary<string, RecSectionStyleStd> projectDic)
        {
            var resultList = new List<RecSectionStyleStd>();
            var dic = new Dictionary<string, RecSectionStyleStd>(); 
			foreach( var pair in projectDic )
			{
                var style = pair.Value;
                var localKey = style.ElementType + style.Name;
                Strandpattern strandpattern = null;
                if( !string.IsNullOrEmpty( style.Name ) )
                {
                    strandpattern = new Strandpattern(style.Strandptn, style.NumOfStrands);
                }
                if( dic.ContainsKey( localKey ) )
                {
                    style = dic[localKey];
                }
                else
                {
                    dic.Add( localKey, style );
                    resultList.Add( style );
                }
                if( null != strandpattern )
                {
                    style.AddStrandpattern(strandpattern);
                }
			}
            // Sort the strandpatterns
            foreach( var rec in resultList )
            {
                if( null != rec.Strandpatterns )
                {
                    rec.Strandpatterns.Sort( ( p1, p2 ) => System.String.Compare(p1.Name, p2.Name, System.StringComparison.Ordinal) );
                }
            }
            return resultList;
        }

		/// <summary>
		/// Parses one row in <see cref="System.Data.Common.DbDataReader"/> into
		/// a new instance of <see cref="Paths.Common.Records.RecSectionStyleStd"/>.
		/// </summary>
		/// <param name="dataReader">The data reader.</param>
		/// <returns>A new instance of <see cref="Paths.Common.Records.RecSectionStyleStd"/>.</returns>
		public static RecSectionStyleStd ParseSectionStyleStd( DbDataReader dataReader )
		{
			var record = new RecSectionStyleStd();
			record.Factory = DataConverter.Cast<string>( dataReader[0] );
			record.Project = DataConverter.Cast<string>( dataReader[1] );
			record.ElementType = DataConverter.Cast<string>( dataReader[2] );
			record.Name = DataConverter.Cast<string>( dataReader[3] );
			record.SectionType = DataConverter.Cast<string>( dataReader[4] );
			record.Description = DataConverter.Cast<string>( dataReader[5] );
            record.Strandptn = DataConverter.Cast<string>( dataReader[6] );
            record.NumOfStrands = DataConverter.Cast<int>( dataReader[7] );

			//record.Width = DataConverter.Cast<double>(dataReader[6]);
			//record.WidthTolerance = DataConverter.Cast<double>(dataReader[7]);
			//record.DiffBottomTop = DataConverter.Cast<double>(dataReader[8]);
			//record.Height = DataConverter.Cast<double>(dataReader[9]);
			//record.Endcap = DataConverter.Cast<string>(dataReader[10]);
			//record.Strandptn = DataConverter.Cast<string>(dataReader[11]);
			//record.RcTemplate = DataConverter.Cast<string>(dataReader[12]);
			//record.Material = DataConverter.Cast<string>(dataReader[13]);
			//record.ProductionLine = DataConverter.Cast<string>(dataReader[14]);
			//record.CutType = DataConverter.Cast<int>(dataReader[15]);
			//record.UseCutZone = DataConverter.Cast<int>(dataReader[16]);
			//record.LiftMethod = DataConverter.Cast<int>(dataReader[17]);
			//record.LiftRotation1 = DataConverter.Cast<double>(dataReader[18]);
			//record.LiftRotation2 = DataConverter.Cast<double>(dataReader[19]);
			//record.LiftPlacingLs = DataConverter.Cast<int>(dataReader[20]);
			//record.LiftParameterLs = DataConverter.Cast<double>(dataReader[21]);
			//record.LiftPlacingSs = DataConverter.Cast<int>(dataReader[22]);
			//record.LiftParameterSs = DataConverter.Cast<double>(dataReader[23]);
			//record.LiftCores = DataConverter.Cast<string>(dataReader[24]);
			//record.LiftCoreLength = DataConverter.Cast<double>(dataReader[25]);
			//record.LiftCoreDisplayMode = DataConverter.Cast<int>(dataReader[26]);
			//record.LiftDistanceMax = DataConverter.Cast<double>(dataReader[27]);
			//record.LiftDistanceMin = DataConverter.Cast<double>(dataReader[28]);
			//record.LiftSpacing = DataConverter.Cast<double>(dataReader[29]);
			//record.LiftType = DataConverter.Cast<string>(dataReader[30]);
			//record.SectionViewDimStrandGrp = DataConverter.Cast<int>(dataReader[31]);
			//record.SectionViewTxtStrandGrp = DataConverter.Cast<int>(dataReader[32]);
			//record.SectionViewNbrCores = DataConverter.Cast<int>(dataReader[33]);
			//record.SectionViewScale = DataConverter.Cast<double>(dataReader[34]);
			//record.UseSectionViewSymbol = DataConverter.Cast<int>(dataReader[35]);
			//record.SectionViewFilename = DataConverter.Cast<string>(dataReader[36]);
			//record.ChamferDistance = DataConverter.Cast<double>(dataReader[37]);
			//record.ChamferText = DataConverter.Cast<string>(dataReader[38]);
			//record.ChamferVisibility = DataConverter.Cast<int>(dataReader[39]);
			//record.RcCoverCut1 = DataConverter.Cast<double>(dataReader[40]);
			//record.RcCoverCut2 = DataConverter.Cast<double>(dataReader[41]);
			//record.ElementGrp = DataConverter.Cast<string>(dataReader[42]);
			//record.ProductPrefix = DataConverter.Cast<string>(dataReader[43]);
			//record.Product = DataConverter.Cast<string>(dataReader[44]);
			//record.ElementMarkPrefix = DataConverter.Cast<string>(dataReader[45]);
			//record.DrawingNamePrefix = DataConverter.Cast<string>(dataReader[46]);
			//record.DrawingType = DataConverter.Cast<string>(dataReader[47]);
			//record.DrawingTemplate = DataConverter.Cast<string>(dataReader[48]);
			//record.CreatedBy = DataConverter.Cast<string>(dataReader[49]);
			//record.CreatedDate = DataConverter.Cast<System.DateTime?>(dataReader[50]);
			//record.ChangedBy = DataConverter.Cast<string>(dataReader[51]);
			//record.ChangedDate = DataConverter.Cast<System.DateTime?>(dataReader[52]);
			//record.LiftHolePosition = DataConverter.Cast<double>(dataReader[53]);
			return record;
		}

		/// <summary>
		/// Insert the specified record into the database.
		/// </summary>
		/// <param name="record">The record to insert into the database.</param>
		/// <returns>The number of affected records.</returns>
		public int InsertSectionStyleStd( RecSectionStyleStd record )
		{
			var insert = new ImpactInsert( ImpSectionStyleStd.Instance )
			{
				Columns = 
				{
					{ ImpSectionStyleStd.Factory, record.Factory },
					{ ImpSectionStyleStd.Project, record.Project },
					{ ImpSectionStyleStd.ElementType, record.ElementType },
					{ ImpSectionStyleStd.Name, record.Name },
					{ ImpSectionStyleStd.SectionType, record.SectionType },
					{ ImpSectionStyleStd.Description, record.Description },
					{ ImpSectionStyleStd.Width, record.Width },
					{ ImpSectionStyleStd.WidthTolerance, record.WidthTolerance },
					{ ImpSectionStyleStd.DiffBottomTop, record.DiffBottomTop },
					{ ImpSectionStyleStd.Height, record.Height },
					{ ImpSectionStyleStd.Endcap, record.Endcap },
					{ ImpSectionStyleStd.Strandptn, record.Strandptn },
					{ ImpSectionStyleStd.RcTemplate, record.RcTemplate },
					{ ImpSectionStyleStd.Material, record.Material },
					{ ImpSectionStyleStd.ProductionLine, record.ProductionLine },
					{ ImpSectionStyleStd.CutType, record.CutType },
					{ ImpSectionStyleStd.UseCutZone, record.UseCutZone },
					{ ImpSectionStyleStd.LiftMethod, record.LiftMethod },
					{ ImpSectionStyleStd.LiftRotation1, record.LiftRotation1 },
					{ ImpSectionStyleStd.LiftRotation2, record.LiftRotation2 },
					{ ImpSectionStyleStd.LiftPlacingLs, record.LiftPlacingLs },
					{ ImpSectionStyleStd.LiftParameterLs, record.LiftParameterLs },
					{ ImpSectionStyleStd.LiftPlacingSs, record.LiftPlacingSs },
					{ ImpSectionStyleStd.LiftParameterSs, record.LiftParameterSs },
					{ ImpSectionStyleStd.LiftCores, record.LiftCores },
					{ ImpSectionStyleStd.LiftCoreLength, record.LiftCoreLength },
					{ ImpSectionStyleStd.LiftCoreDisplayMode, record.LiftCoreDisplayMode },
					{ ImpSectionStyleStd.LiftDistanceMax, record.LiftDistanceMax },
					{ ImpSectionStyleStd.LiftDistanceMin, record.LiftDistanceMin },
					{ ImpSectionStyleStd.LiftSpacing, record.LiftSpacing },
					{ ImpSectionStyleStd.LiftType, record.LiftType },
					{ ImpSectionStyleStd.SectionViewDimStrandGrp, record.SectionViewDimStrandGrp },
					{ ImpSectionStyleStd.SectionViewTxtStrandGrp, record.SectionViewTxtStrandGrp },
					{ ImpSectionStyleStd.SectionViewNbrCores, record.SectionViewNbrCores },
					{ ImpSectionStyleStd.SectionViewScale, record.SectionViewScale },
					{ ImpSectionStyleStd.UseSectionViewSymbol, record.UseSectionViewSymbol },
					{ ImpSectionStyleStd.SectionViewFilename, record.SectionViewFilename },
					{ ImpSectionStyleStd.ChamferDistance, record.ChamferDistance },
					{ ImpSectionStyleStd.ChamferText, record.ChamferText },
					{ ImpSectionStyleStd.ChamferVisibility, record.ChamferVisibility },
					{ ImpSectionStyleStd.RcCoverCut1, record.RcCoverCut1 },
					{ ImpSectionStyleStd.RcCoverCut2, record.RcCoverCut2 },
					{ ImpSectionStyleStd.ElementGrp, record.ElementGrp },
					{ ImpSectionStyleStd.ProductPrefix, record.ProductPrefix },
					{ ImpSectionStyleStd.Product, record.Product },
					{ ImpSectionStyleStd.ElementMarkPrefix, record.ElementMarkPrefix },
					{ ImpSectionStyleStd.DrawingNamePrefix, record.DrawingNamePrefix },
					{ ImpSectionStyleStd.DrawingType, record.DrawingType },
					{ ImpSectionStyleStd.DrawingTemplate, record.DrawingTemplate },
					{ ImpSectionStyleStd.CreatedBy, record.CreatedBy },
					{ ImpSectionStyleStd.CreatedDate, record.CreatedDate },
					{ ImpSectionStyleStd.ChangedBy, record.ChangedBy },
					{ ImpSectionStyleStd.ChangedDate, record.ChangedDate },
					{ ImpSectionStyleStd.LiftHolePosition, record.LiftHolePosition },
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
		public int DeleteSectionStyleStd( RecSectionStyleStd record )
		{
			var delete = new ImpactDelete( ImpSectionStyleStd.Instance )
			{
				Where = 
				{
					{ ImpSectionStyleStd.Factory.Equal( record.Factory )},
					{ ImpSectionStyleStd.Project.Equal( record.Project )},
					{ ImpSectionStyleStd.ElementType.Equal( record.ElementType )},
					{ ImpSectionStyleStd.Name.Equal( record.Name )},
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
		public int BulkDeleteSectionStyleStd( List<RecSectionStyleStd> list )
		{
			int result = 0;

			foreach( var record in list )
			{
				result += this.DeleteSectionStyleStd( record );
			}

			return result;
		}
		/// <summary>
		/// Update the specified record in the database.
		/// </summary>
		/// <param name="record">The record to update.</param>
		/// <returns></returns>
		public int UpdateSectionStyleStd( RecSectionStyleStd record )
		{

			var update = new ImpactUpdate( ImpSectionStyleStd.Instance )
			{
				Columns = 
				{
					{ ImpSectionStyleStd.SectionType, record.SectionType },
					{ ImpSectionStyleStd.Description, record.Description },
					{ ImpSectionStyleStd.Width, record.Width },
					{ ImpSectionStyleStd.WidthTolerance, record.WidthTolerance },
					{ ImpSectionStyleStd.DiffBottomTop, record.DiffBottomTop },
					{ ImpSectionStyleStd.Height, record.Height },
					{ ImpSectionStyleStd.Endcap, record.Endcap },
					{ ImpSectionStyleStd.Strandptn, record.Strandptn },
					{ ImpSectionStyleStd.RcTemplate, record.RcTemplate },
					{ ImpSectionStyleStd.Material, record.Material },
					{ ImpSectionStyleStd.ProductionLine, record.ProductionLine },
					{ ImpSectionStyleStd.CutType, record.CutType },
					{ ImpSectionStyleStd.UseCutZone, record.UseCutZone },
					{ ImpSectionStyleStd.LiftMethod, record.LiftMethod },
					{ ImpSectionStyleStd.LiftRotation1, record.LiftRotation1 },
					{ ImpSectionStyleStd.LiftRotation2, record.LiftRotation2 },
					{ ImpSectionStyleStd.LiftPlacingLs, record.LiftPlacingLs },
					{ ImpSectionStyleStd.LiftParameterLs, record.LiftParameterLs },
					{ ImpSectionStyleStd.LiftPlacingSs, record.LiftPlacingSs },
					{ ImpSectionStyleStd.LiftParameterSs, record.LiftParameterSs },
					{ ImpSectionStyleStd.LiftCores, record.LiftCores },
					{ ImpSectionStyleStd.LiftCoreLength, record.LiftCoreLength },
					{ ImpSectionStyleStd.LiftCoreDisplayMode, record.LiftCoreDisplayMode },
					{ ImpSectionStyleStd.LiftDistanceMax, record.LiftDistanceMax },
					{ ImpSectionStyleStd.LiftDistanceMin, record.LiftDistanceMin },
					{ ImpSectionStyleStd.LiftSpacing, record.LiftSpacing },
					{ ImpSectionStyleStd.LiftType, record.LiftType },
					{ ImpSectionStyleStd.SectionViewDimStrandGrp, record.SectionViewDimStrandGrp },
					{ ImpSectionStyleStd.SectionViewTxtStrandGrp, record.SectionViewTxtStrandGrp },
					{ ImpSectionStyleStd.SectionViewNbrCores, record.SectionViewNbrCores },
					{ ImpSectionStyleStd.SectionViewScale, record.SectionViewScale },
					{ ImpSectionStyleStd.UseSectionViewSymbol, record.UseSectionViewSymbol },
					{ ImpSectionStyleStd.SectionViewFilename, record.SectionViewFilename },
					{ ImpSectionStyleStd.ChamferDistance, record.ChamferDistance },
					{ ImpSectionStyleStd.ChamferText, record.ChamferText },
					{ ImpSectionStyleStd.ChamferVisibility, record.ChamferVisibility },
					{ ImpSectionStyleStd.RcCoverCut1, record.RcCoverCut1 },
					{ ImpSectionStyleStd.RcCoverCut2, record.RcCoverCut2 },
					{ ImpSectionStyleStd.ElementGrp, record.ElementGrp },
					{ ImpSectionStyleStd.ProductPrefix, record.ProductPrefix },
					{ ImpSectionStyleStd.Product, record.Product },
					{ ImpSectionStyleStd.ElementMarkPrefix, record.ElementMarkPrefix },
					{ ImpSectionStyleStd.DrawingNamePrefix, record.DrawingNamePrefix },
					{ ImpSectionStyleStd.DrawingType, record.DrawingType },
					{ ImpSectionStyleStd.DrawingTemplate, record.DrawingTemplate },
					{ ImpSectionStyleStd.CreatedBy, record.CreatedBy },
					{ ImpSectionStyleStd.CreatedDate, record.CreatedDate },
					{ ImpSectionStyleStd.ChangedBy, record.ChangedBy },
					{ ImpSectionStyleStd.ChangedDate, record.ChangedDate },
					{ ImpSectionStyleStd.LiftHolePosition, record.LiftHolePosition },
				},
				Where = 
				{
					{ ImpSectionStyleStd.Factory.Equal( record.Factory ) },
					{ ImpSectionStyleStd.Project.Equal( record.Project ) },
					{ ImpSectionStyleStd.ElementType.Equal( record.ElementType ) },
					{ ImpSectionStyleStd.Name.Equal( record.Name ) },
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

		public int BulkUpdateSectionStyleStd( List<RecSectionStyleStd> list )
		{
			int result = 0;

			foreach( var record in list )
			{
				result += this.UpdateSectionStyleStd( record );
			}

			return result;
		}
	}
}
