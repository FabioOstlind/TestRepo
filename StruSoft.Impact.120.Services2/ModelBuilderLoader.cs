
namespace StruSoft.Impact.V120.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataTypes;
    using DB;
    using DB.Query;
    using V120.ProjectManager.Core.ModelBuilder;
    using SectionParameter = System.Collections.Generic.KeyValuePair<StruSoft.Impact.DataTypes.SectionStyleParameter, double>;

    internal class ModelBuilderLoader
    {
        /// <summary>
        /// The _company.
        /// </summary>
        readonly string _company;

        /// <summary>
        /// The _factory.
        /// </summary>
        readonly string _factory;

        /// <summary>
        /// The _project.
        /// </summary>
        readonly string _project;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBuilderLoader"/> class.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        public ModelBuilderLoader( string factory, string project )
        {
            this._company = factory.Substring( 0, 2 ) + "00";
            this._factory = factory;
            this._project = project.PadLeft( 12 );
        }

        #region Query

        /// <summary>
        /// Get the query for fetching all geometry for the specified project.
        /// </summary>
        /// <param name="factory">
        /// </param>
        /// <param name="project">
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        static string GetElementGeometryQuery( string factory, string project )
        {
            ImpactQuery query = new ImpactQuery( true )
            {
                Select =
				{
					ImpElement.ElementMark, 
					ImpElement.ElementType, 
					ImpElement.Style, 
					ImpElement.ElementLength, 
					ImpElement.ElementWidth, 
					ImpElement.ElementHeight, 
					ImpElement.AngleLeftSide, 
					ImpElement.AngleRightSide, 
					ImpElement.Material, 
				},

                From =
				{
					ImpModelGeometry.As( "T1" ), 
				},

                Join =
				{
					Join.Inner( ImpElement.As( "T2" ), 
						ImpModelGeometry.Factory.Equal( ImpElement.Factory ), 
						ImpModelGeometry.Project.Equal( ImpElement.Project ), 
						ImpModelGeometry.ElementMark.Equal( ImpElement.ElementMark ) ), 
				},

                Where =
				{
					ImpModelGeometry.Factory.Equal( factory ), 
					ImpModelGeometry.Project.Equal( project ), 
					ImpModelGeometry.Deleted.Equal( false ), 
					ImpModelGeometry.DrawInModel.Equal( true ), 
				},
            };

            string statement = query.ToString();

            return statement;
        }

        /// <summary>
        /// Get the query for fetching all geometry instances for the specified project.
        /// </summary>
        /// <param name="factory">
        /// </param>
        /// <param name="project">
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        static string GetElementGeometryInstanceQuery( string factory, string project )
        {
            ImpactQuery query = new ImpactQuery
                                {
                                    Select =
				{
					ImpModelGeometry.ElementMark, 
					ImpModelGeometry.ElementId, 
					ImpModelGeometry.Building, 
					ImpModelGeometry.FloorId, 
					ImpModelGeometry.ModelRotation, 
					ImpModelGeometry.ModelGeometryX, 
					ImpModelGeometry.ModelGeometryY, 
					ImpModelGeometry.ModelGeometryZ, 
					ImpModelGeometry.ModelNormalX, 
					ImpModelGeometry.ModelNormalY, 
					ImpModelGeometry.ModelNormalZ, 
					ImpModelPlanning.ErectionSequenceNo, 
				},

                                    From =
				{
					ImpModelGeometry.As( "T1" ), 
				},

                                    Join =
				{
					Join.Inner( ImpElement.As( "T2" ), 
						ImpModelGeometry.Factory.Equal( ImpElement.Factory ), 
						ImpModelGeometry.Project.Equal( ImpElement.Project ), 
						ImpModelGeometry.ElementMark.Equal( ImpElement.ElementMark ) ), 

					Join.Left( ImpModelPlanning.As( "T3" ), 
						ImpModelGeometry.Factory.Equal( ImpModelPlanning.Factory ), 
						ImpModelGeometry.Project.Equal( ImpModelPlanning.Project ), 
						ImpModelGeometry.ElementId.Equal( ImpModelPlanning.ElementId ) ), 
				},

                                    Where =
				{
					ImpModelGeometry.Factory.Equal( factory ), 
					ImpModelGeometry.Project.Equal( project ), 
					ImpModelGeometry.Deleted.Equal( false ), 
					ImpModelGeometry.DrawInModel.Equal( true ), 
				},
                                };

            string statement = query.ToString();

            return statement;
        }

        /// <summary>
        /// Get the query for fetching all recesses for the specified project.
        /// </summary>
        /// <param name="factory">
        /// </param>
        /// <param name="project">
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        static string GetRecessQuery( string factory, string project )
        {
            ImpactQuery innerQuery = new ImpactQuery
                                     {
                                         Select = {
                ImpModelGeometry.ElementMark.As( "MARK" ), Aggregate.Count().As( "QUANTITY" ) 
             },
                                         From = {
              ImpElement.As( "T1" ) 
           },
                                         Join =
				{
					Join.Inner( ImpModelGeometry.As( "T2" ), 
								ImpElement.Factory.Equal( ImpModelGeometry.Factory ), 
								ImpElement.Project.Equal( ImpModelGeometry.Project ), 
								ImpElement.ElementMark.Equal( ImpModelGeometry.ElementMark ) )
				},
                                         Where =
				{
					ImpModelGeometry.Deleted.Equal( false ), 
					ImpElement.Factory.Equal( factory ), 
					ImpElement.Project.Equal( project )
				},

                                         GroupBy =
				{
					ImpModelGeometry.ElementMark
				},
                                     };

            ImpactQuery query = new ImpactQuery
                                {
                                    From = {
              { innerQuery, "T1" } 
           },

                                    Select =
				{
					ImpRecess.ElementMark, 
					ImpRecess.RecessId, 
					ImpRecess.RecessType, 
					ImpRecess.RecessX1, 
					ImpRecess.RecessY1, 
					ImpRecess.RecessX2, 
					ImpRecess.RecessY2, 
					ImpRecess.RecessRotation, 
					ImpRecess.RecessCountersinking, 
					ImpRecess.Side, 
					ImpRecess.DimensionType, 
					ImpRecess.RecessDiameter, 
				},

                                    Join =
				{
					Join.Inner( ImpRecess.As( "T2" ), 
								ImpactColumnName.Create( "T1", "MARK" ).Equal( ImpRecess.ElementMark ) )
				},

                                    Where = 
				{
					ImpRecess.Factory.Equal( factory ), 
					ImpRecess.Project.Equal( project ), 
					ImpactColumnName.Create( "T1", "QUANTITY" ).GreaterThan( 0 )
				},

                                    OrderBy =
				{
					{ ImpRecess.ElementMark, OrderBy.Ascending }, 
					{ ImpRecess.RecessId, OrderBy.Ascending }, 
				},
                                };

            string statement = query.ToString();
            return statement;
        }

        /// <summary>
        /// Get the query for fetching all recess points for all polygon recesses in the specified project.
        /// The points are ordered by element mark, recess id and point id in ascending order.
        /// </summary>
        /// <param name="factory">
        /// </param>
        /// <param name="project">
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        static string GetRecessPointsQuery( string factory, string project )
        {
            ImpactQuery innerQuery = new ImpactQuery
                                     {
                                         Select = 
				{
					ImpModelGeometry.Factory.As( "FACTORY" ), 
					ImpModelGeometry.Project.As( "PROJECT" ), 
					ImpModelGeometry.ElementMark.As( "MARK" ), 
					Aggregate.Count().As( "QUANTITY" ) 
				},

                                         From = {
              ImpElement.As( "T1" ) 
           },

                                         Join =
				{
					Join.Inner( ImpModelGeometry.As( "T2" ), 
								ImpElement.Factory.Equal( ImpModelGeometry.Factory ), 
								ImpElement.Project.Equal( ImpModelGeometry.Project ), 
								ImpElement.ElementMark.Equal( ImpModelGeometry.ElementMark ) )
				},
                                         Where =
				{
					ImpModelGeometry.Deleted.Equal( false ), 
					ImpElement.Factory.Equal( factory ), 
					ImpElement.Project.Equal( project )
				},

                                         GroupBy =
				{
					ImpModelGeometry.Factory, 
					ImpModelGeometry.Project, 
					ImpModelGeometry.ElementMark
				},
                                     };

            ImpactQuery query = new ImpactQuery
                                {
                                    From = {
              { innerQuery, "T1" } 
           },

                                    Select =
				{
					ImpRecessGeometry.ElementMark, 
					ImpRecessGeometry.RecessId, 
					ImpRecessGeometry.PointId, 
					ImpRecessGeometry.PointX, 
					ImpRecessGeometry.PointY, 
					ImpRecessGeometry.Bulge,	
				},

                                    Join =
				{
					Join.Inner( ImpRecess.As( "T2" ), 
								ImpactColumnName.Create( "T1", "FACTORY" ).Equal( ImpRecess.Factory ), 
								ImpactColumnName.Create( "T1", "PROJECT" ).Equal( ImpRecess.Project ), 
								ImpactColumnName.Create( "T1", "MARK" ).Equal( ImpRecess.ElementMark ) ), 

					Join.Inner( ImpRecessGeometry.As( "T3" ), 
								ImpRecessGeometry.Factory.Equal( ImpRecess.Factory ), 
								ImpRecessGeometry.Project.Equal( ImpRecess.Project ), 
								ImpRecessGeometry.ElementMark.Equal( ImpRecess.ElementMark ), 
								ImpRecessGeometry.RecessId.Equal( ImpRecess.RecessId ) ), 
				},

                                    Where = 
				{
					ImpRecess.Factory.Equal( factory ), 
					ImpRecess.Project.Equal( project ), 
					ImpactColumnName.Create( "T1", "QUANTITY" ).GreaterThan( 0 )
				},

                                    OrderBy =
				{
					{ ImpRecessGeometry.ElementMark, OrderBy.Ascending }, 
					{ ImpRecessGeometry.RecessId, OrderBy.Ascending }, 
					{ ImpRecessGeometry.PointId, OrderBy.Ascending }, 
				},
                                };

            string statement = query.ToString();
            return statement;
        }

        /// <summary>
        /// Get the query for fetching all endcap edges for the specified project.
        /// </summary>
        /// <param name="factory">
        /// </param>
        /// <param name="project">
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        static string GetEndcapEdgeQuery( string factory, string project )
        {
            ImpactQuery innerQuery = new ImpactQuery
                                     {
                                         Select = {
                ImpModelGeometry.ElementMark.As( "MARK" ), Aggregate.Count().As( "QUANTITY" ) 
             },
                                         From = {
              ImpElement.As( "T1" ) 
           },
                                         Join =
				{
					Join.Inner( ImpModelGeometry.As( "T2" ), 
								ImpElement.Factory.Equal( ImpModelGeometry.Factory ), 
								ImpElement.Project.Equal( ImpModelGeometry.Project ), 
								ImpElement.ElementMark.Equal( ImpModelGeometry.ElementMark ) )
				},
                                         Where =
				{
					ImpModelGeometry.Deleted.Equal( false ), 
					ImpElement.Factory.Equal( factory ), 
					ImpElement.Project.Equal( project )
				},

                                         GroupBy =
				{
					ImpModelGeometry.ElementMark
				},
                                     };

            ImpactQuery query = new ImpactQuery
                                {
                                    From = {
              { innerQuery, "T1" } 
           },

                                    Select =
				{
					ImpEndcapEdge.ElementMark, 
					ImpEndcapEdge.Side, 
					ImpEndcapEdge.StartAt, 
					ImpEndcapEdge.Extension, 
					ImpEndcapEdge.FromSide, 
					ImpEndcapEdge.IndentAtStart, 
					ImpEndcapEdge.IndentAtEnd,				
				},

                                    Join =
				{
					Join.Inner( ImpEndcapEdge.As( "T2" ), 
								ImpactColumnName.Create( "T1", "MARK" ).Equal( ImpEndcapEdge.ElementMark ) )
				},

                                    Where = 
				{
					ImpEndcapEdge.Factory.Equal( factory ), 
					ImpEndcapEdge.Project.Equal( project ), 
					ImpactColumnName.Create( "T1", "QUANTITY" ).GreaterThan( 0 )
				},

                                    OrderBy =
				{
					{ ImpEndcapEdge.ElementMark, OrderBy.Ascending }, 
					{ ImpEndcapEdge.EndcapEdgeId, OrderBy.Ascending }, 
				},
                                };

            string statement = query.ToString();
            return statement;
        }

        /// <summary>
        /// Get the query for fetching all opening standards for the specified company, factory and project.
        /// </summary>
        /// <param name="factory">
        /// </param>
        /// <param name="project">
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        static ImpactQuery GetOpeningStandardQuery( string factory, string project )
        {
            ImpactQuery query = new ImpactQuery
                                {
                                    Select =
				{
					ImpOpeningStd.Name, 
					ImpOpeningStd.OpeningWidth, 
					ImpOpeningStd.OpeningHeight, 
				},
                                    From = {
              ImpOpeningStd.As( "T1" ) 
           },
                                    Where =
				{
					ImpOpeningStd.Factory.Like( factory.Substring(0, 2) + "__" ), 
					WhereGroup.Or( ImpOpeningStd.Factory.Equal( ImpOpeningStd.Project, true ), ImpOpeningStd.Project.Equal( project ) )
				},
                                };

            return query;
        }

        /// <summary>
        /// The get opening query.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        static ImpactQuery GetOpeningQuery( string factory, string project )
        {
            ImpactQuery query = new ImpactQuery
                                {
                                    Select =
				{
					ImpOpening.ElementMark, 
					ImpOpening.Opening, 
					ImpOpening.OpeningX, 
					ImpOpening.OpeningY, 
				},

                                    From = {
              ImpOpening.As( "T1" ) 
           },

                                    Where =
				{
					ImpOpening.Factory.Equal( factory ), 
					ImpOpening.Project.Equal( project ), 
				}
                                };

            return query;
        }

        /// <summary>
        /// The get section parameter query.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        static ImpactQuery GetSectionParameterQuery( string factory, string project )
        {
            ImpactQuery query = new ImpactQuery
                                {
                                    Select =
				{
					ImpSectionStyleParameterStd.ElementType, 
					ImpSectionStyleParameterStd.Name, 
					ImpSectionStyleParameterStd.ParameterId, 
					ImpSectionStyleParameterStd.ParameterValue, 
				},

                                    From = {
              ImpSectionStyleParameterStd.As( "T1" ) 
           },

                                    Where =
				{
					ImpSectionStyleParameterStd.Factory.Like( factory.Substring(0, 2) + "__" ), 
					WhereGroup.Or( 
					ImpSectionStyleParameterStd.Factory.Equal( ImpSectionStyleParameterStd.Project, true ), 
					ImpSectionStyleParameterStd.Project.Equal( project ) )
				},
                                };

            return query;
        }

        /// <summary>
        /// The get section geometry query.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        static ImpactQuery GetSectionGeometryQuery( string factory, string project )
        {
            ImpactQuery query = new ImpactQuery
                                {
                                    Select =
				{
					ImpSectionGeometryStd.ElementType, 
					ImpSectionGeometryStd.Section, 
					ImpSectionGeometryStd.PointId, 
					ImpSectionGeometryStd.PointX, 
					ImpSectionGeometryStd.PointY, 
					ImpSectionGeometryStd.PointBulge, 
				},

                                    From = {
              ImpSectionGeometryStd.As( "T1" ) 
           },

                                    Where =
				{
					ImpSectionGeometryStd.Factory.Like( factory.Substring(0, 2) + "__" ), 
					WhereGroup.Or( 
					ImpSectionGeometryStd.Factory.Equal( ImpSectionGeometryStd.Project, true ), 
					ImpSectionGeometryStd.Project.Equal( project ) )
				},
                                };


            return query;
        }

        /// <summary>
        /// The get section core geometry query.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        static ImpactQuery GetSectionCoreGeometryQuery( string factory, string project )
        {
            ImpactQuery query = new ImpactQuery
                                {
                                    Select =
				{
					ImpSectionCoreGeometryStd.ElementType, 
					ImpSectionCoreGeometryStd.Section, 
					ImpSectionCoreGeometryStd.CoreId, 
					ImpSectionCoreGeometryStd.CorePointId, 
					ImpSectionCoreGeometryStd.CorePointX, 
					ImpSectionCoreGeometryStd.CorePointY, 
					ImpSectionCoreGeometryStd.CorePointBulge, 
				},

                                    From = {
              ImpSectionCoreGeometryStd.As( "T1" ) 
           },

                                    Where =
				{
					ImpSectionCoreGeometryStd.Factory.Like( factory.Substring(0, 2) + "__" ), 
					WhereGroup.Or( 
					ImpSectionCoreGeometryStd.Factory.Equal( ImpSectionCoreGeometryStd.Project, true ), 
					ImpSectionCoreGeometryStd.Project.Equal( project ) )
				},
                                };


            return query;
        }

        /// <summary>
        /// The get section style query.
        /// </summary>
        /// <param name="company">
        /// The company.
        /// </param>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        static ImpactQuery GetSectionStyleQuery( string company, string factory, string project )
        {
            ImpactQuery query = new ImpactQuery
                                {
                                    Select =
				{
					ImpSectionStyleStd.ElementType, 
					ImpSectionStyleStd.Name, 
					ImpSectionStyleStd.SectionType, 
					ImpSectionStyleStd.Width, 
					ImpSectionStyleStd.Height, 
					ImpSectionStyleStd.CutType, 
					ImpSectionStyleStd.Material, 
				},

                                    From = {
              ImpSectionStyleStd.As( "T1" ) 
           },

                                    Where =
				{
					ImpSectionStyleStd.Factory.Like( factory.Substring(0, 2) + "__" ), 
					WhereGroup.Or( 
					ImpSectionStyleStd.Factory.Equal( ImpSectionStyleStd.Project, true ), 
					ImpSectionStyleStd.Project.Equal( project ) ), 
				},
                                };

            return query;
        }

        /// <summary>
        /// The get wall material query.
        /// </summary>
        /// <param name="company">
        /// The company.
        /// </param>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        static ImpactQuery GetWallMaterialQuery( string company, string factory, string project )
        {
            ImpactQuery query = new ImpactQuery
                                {
                                    Select =
				{
					ImpWallStyleStd.ElementType, 
					ImpWallStyleStd.Name, 
					ImpWallStyleLayerStd.Material.As( "MATERIAL" ), 
				},

                                    From = {
              ImpWallStyleStd.As( "T1" ) 
           },

                                    Join =
				{
					Join.Inner( ImpWallStyleLayerStd.As( "T2" ), 
						ImpWallStyleStd.Factory.Equal( ImpWallStyleLayerStd.Factory ), 
						ImpWallStyleStd.Project.Equal( ImpWallStyleLayerStd.Project ), 
						ImpWallStyleStd.Name.Equal( ImpWallStyleLayerStd.Style ) ), 
				},

                                    Where =
				{
					ImpWallStyleStd.Factory.Like( factory.Substring(0, 2) + "__" ), 
					WhereGroup.Or( 
					ImpWallStyleStd.Factory.Equal( ImpWallStyleStd.Project, true ), 
					ImpWallStyleStd.Project.Equal( project ) ), 

					// For now we use the material from the first layer in the wall.
					ImpWallStyleLayerStd.LayerId.Equal( 1 ), 
				},
                                };

            return query;
        }

        /// <summary>
        /// The get end beam query.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        static ImpactQuery GetEndBeamQuery( string factory, string project )
        {
            ImpactQuery query = new ImpactQuery
                                {
                                    Select = {
                ImpEndBeam.ElementMark, ImpEndBeam.Side, ImpEndBeam.VerticalAngle 
             },
                                    From = {
              ImpEndBeam.As( "T1" ) 
           },
                                    Where = 
				{ 
					ImpEndBeam.Factory.Equal( factory ), 
					ImpEndBeam.Project.Equal( project ), 
					WhereGroup.Or( ImpEndBeam.Side.Equal( 3 ), ImpEndBeam.Side.Equal( 4 ) ), 
				},
                                };

            return query;
        }

        /// <summary>
        /// The get material query.
        /// </summary>
        /// <param name="company">
        /// The company.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        static ImpactQuery GetMaterialQuery( string company )
        {
            ImpactQuery query = new ImpactQuery
                                {
                                    From = {
              ImpMaterialStd.As( "T1" ) 
           },
                                    Select = 
				{ 
					ImpMaterialStd.Name, 
					ImpMaterialStd.MaterialType, 
					ImpMaterialStd.ColorRed, 
					ImpMaterialStd.ColorGreen, 
					ImpMaterialStd.ColorBlue, 
				},
                                    Where =
				{
					ImpMaterialStd.Factory.Equal( company ), 
					ImpMaterialStd.Project.Equal( company ), 
				},
                                };

            return query;
        }

        /// <summary>
        /// The get wall material query.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The StruSoft.Impact.V120.DB.Query.ImpactQuery.
        /// </returns>
        static ImpactQuery GetWallMaterialQuery( string factory, string project )
        {
            ImpactQuery query = new ImpactQuery
                                {
                                    From = {
              ImpElementGeometryLayer.As( "T1" ) 
           },
                                    Select = 
				{ 
					ImpElementGeometryLayer.ElementMark, 
					ImpElementGeometryLayer.Material, 					
				},
                                    Where =
				{
					ImpElementGeometryLayer.Factory.Equal( factory ), 
					ImpElementGeometryLayer.Project.Equal( project ), 
					ImpElementGeometryLayer.LayerId.Equal( 1 ), 
				},
                                };

            return query;
        }

        #endregion Query


        /// <summary>
        /// Adds wall data ( Wall, Insulated, Sandwich and Double Wall ) to the ModelBuilderData instance.
        /// </summary>
        /// <param name="database">
        /// </param>
        /// <param name="modelBuilderData">
        /// </param>
        /// <param name="elementDictionary">
        /// </param>
        void AddWallData( ImpactDatabase database, ref ModelBuilderData modelBuilderData, ref Dictionary<ElementType.KnownValue, List<Element>> elementDictionary )
        {
            if( null == modelBuilderData )
                throw new ArgumentNullException( "data" );

            if( null == elementDictionary )
                throw new ArgumentNullException( "elementDictionary" );

            // No need to continue if the list is empty.
            if( 0 == elementDictionary.Count )
                return;

            List<Element> tempList = null;
            List<Element> allList = new List<Element>( 500 );

            if( elementDictionary.TryGetValue( ElementType.KnownValue.Wall, out tempList ) )
            {
                allList.AddRange( tempList );
                elementDictionary.Remove( ElementType.KnownValue.Wall );
            }

            if( elementDictionary.TryGetValue( ElementType.KnownValue.InsulatedWall, out tempList ) )
            {
                allList.AddRange( tempList );
                elementDictionary.Remove( ElementType.KnownValue.InsulatedWall );
            }

            if( elementDictionary.TryGetValue( ElementType.KnownValue.Sandwich, out tempList ) )
            {
                allList.AddRange( tempList );
                elementDictionary.Remove( ElementType.KnownValue.Sandwich );
            }

            if( elementDictionary.TryGetValue( ElementType.KnownValue.DoubleWall, out tempList ) )
            {
                allList.AddRange( tempList );
                elementDictionary.Remove( ElementType.KnownValue.DoubleWall );
            }

            // No walls in the project, no need to query the database for more information.
            if( allList.Count == 0 )
                return;



            // var wallMaterialArgs = new ImpactDatabase.ProfacomArgs<ImpWallStyleStd>( _factory, _project, GetWallMaterialQuery( _company, _factory, _project ),
            // ImpWallStyleStd.ElementType,
            // ImpWallStyleStd.Name );

            // var wallMaterialList = database.Profacom( wallMaterialArgs, column => new
            // {
            // ElementType = ( (ElementType)DataConverter.Cast<string>( column[0] ) ).EnumValue,
            // Name = DataConverter.Cast<string>( column[1] ),
            // MaterialName = DataConverter.Cast<string>( column[2] ),
            // } );
            var wallMaterialList = database.GetAll( GetWallMaterialQuery( this._factory, this._project ).ToString(), column => new
            {
                ElementMark = column[0].Cast<string>().Trim(),
                MaterialName = column[1].Cast<string>(),
            } );



            #region Endcap Edges

            // Get all endcap edges for walls.
            var endcapEdgeList = database.GetAll( GetEndcapEdgeQuery( this._factory, this._project ), column => new
            {
                ElementMark = column[0].Cast<string>().Trim(),
                EndcapEdge = new EndcapEdge
                             {
                                 Side = ( (Side)column[1].Cast<string>() ).EnumValue,
                                 StartAt = column[2].Cast<double>(),
                                 Length = column[3].Cast<double>(),
                                 FromSide = ( (Side)column[4].Cast<string>() ).EnumValue,
                                 IndentAtStart = column[5].Cast<double>(),
                                 IndentAtEnd = column[6].Cast<double>(),
                             }
            } );

            #endregion Endcap Edges

            #region Openings

            // Get all opening standards.
            ImpactDatabase.ProfacomArgs<ImpOpeningStd> openingArgs =
                new ImpactDatabase.ProfacomArgs<ImpOpeningStd>( this._factory, this._project, GetOpeningStandardQuery( this._factory, this._project ), ImpOpeningStd.Name );

            var openingStandardDictionary = database.Profacom( openingArgs, column => new
            {
                Name = column[0].Cast<string>(),
                Width = column[1].Cast<double>(),
                Height = column[2].Cast<double>(),
            } ).ToDictionary( openingStd => openingStd.Name );

            var openingStandard = new { Name = string.Empty, Width = 0d, Height = 0d };

            // Get all the openings in the project.
            var openingList = database.GetAll( GetOpeningQuery( this._factory, this._project ).ToString(), column => new
                        {
                            ElementMark = column[0].Cast<string>().Trim(),
                            Name = column[1].Cast<string>(),
                            X = column[2].Cast<double>(),
                            Y = column[3].Cast<double>(),
                        } );

            var openingGroupList = ( from n in openingList
                                     let isFound = openingStandardDictionary.TryGetValue( n.Name, out openingStandard )
                                     where isFound
                                     select new
                                     {
                                         n.ElementMark,
                                         Opening = new Opening
                                                   {
                                                       X = n.X,
                                                       Y = n.Y,
                                                       Width = openingStandard.Width,
                                                       Height = openingStandard.Height
                                                   }
                                     } ).GroupBy( x => x.ElementMark, x => x.Opening ).ToList();

            #endregion Openings

            List<WallGeometry> wallList = new List<WallGeometry>( allList.Count );

            foreach( var element in allList )
            {
                WallGeometry wall = new WallGeometry( element.Geometry );

                // Endcap Edges.
                wall.EndcapEdges = ( from endcap in endcapEdgeList
                                     where element.Geometry.ElementMark == endcap.ElementMark
                                     select endcap.EndcapEdge ).ToList();

                // Windows and Doors.
                var openingGroup = openingGroupList.Find( group => group.Key == element.Geometry.ElementMark );
                if( null != openingGroup )
                {
                    wall.Openings = openingGroup.ToList();
                }

                // Find the material for the wall.
                var wallMaterial = wallMaterialList.Find( item => item.ElementMark == wall.ElementMark );

                if( null != wallMaterial )
                {
                    wall.MaterialName = wallMaterial.MaterialName;
                }

                wallList.Add( wall );
            }

            // Add the walls to the model builder data.
            modelBuilderData.Walls = wallList;
        }

        /// <summary>
        /// Adds slab data ( Slab, Form Slab, and Prestressed Form Slab ) to the ModelBuilderData instance.
        /// </summary>
        /// <param name="database">
        /// </param>
        /// <param name="modelBuilderData">
        /// </param>
        /// <param name="elementDictionary">
        /// </param>
        void AddSlabData( ImpactDatabase database, ref ModelBuilderData modelBuilderData, ref Dictionary<ElementType.KnownValue, List<Element>> elementDictionary )
        {
            if( null == modelBuilderData )
                throw new ArgumentNullException( "data" );

            if( null == elementDictionary )
                throw new ArgumentNullException( "elementDictionary" );

            // No need to continue if the list is empty.
            if( 0 == elementDictionary.Count )
                return;

            List<Element> tempList = null;
            List<Element> allList = new List<Element>( 500 );

            if( elementDictionary.TryGetValue( ElementType.KnownValue.Slab, out tempList ) )
            {
                allList.AddRange( tempList );
                elementDictionary.Remove( ElementType.KnownValue.Slab );
            }

            if( elementDictionary.TryGetValue( ElementType.KnownValue.FormSlab, out tempList ) )
            {
                allList.AddRange( tempList );
                elementDictionary.Remove( ElementType.KnownValue.FormSlab );
            }

            if( elementDictionary.TryGetValue( ElementType.KnownValue.PrestressedFormSlab, out tempList ) )
            {
                allList.AddRange( tempList );
                elementDictionary.Remove( ElementType.KnownValue.PrestressedFormSlab );
            }

            if( allList.Count == 0 )
                return;

            List<SlabGeometry> slabList = new List<SlabGeometry>( allList.Count );

            foreach( var element in allList )
            {
                SlabGeometry slab = new SlabGeometry( element.Geometry );

                slabList.Add( slab );
            }

            // Add the walls to the model builder data.
            modelBuilderData.Slabs = slabList;
        }

        /// <summary>
        /// Adds section data ( Beam, Prestressed Beam, Column, Hollow Core and Prestressed Slab ) to the ModelBuilderData instance.
        /// </summary>
        /// <param name="database">
        /// </param>
        /// <param name="modelBuilderData">
        /// </param>
        /// <param name="elementDictionary">
        /// </param>
        void AddSectionData( ImpactDatabase database, ref ModelBuilderData modelBuilderData, ref Dictionary<ElementType.KnownValue, List<Element>> elementDictionary )
        {
            if( null == modelBuilderData )
            {
                throw new ArgumentNullException( "data" );
            }

            if( null == elementDictionary )
            {
                throw new ArgumentNullException( "elementDictionary" );
            }

            // No need to continue if the list is empty.
            if( 0 == elementDictionary.Count )
            {
                return;
            }

            List<Element> tempList = null;
            List<Element> beamList = new List<Element>( 500 );
            List<Element> columnList = new List<Element>( 500 );
            List<Element> hollowCoreList = new List<Element>( 500 );
            List<Element> slabList = new List<Element>( 500 );

            List<Element> allList = new List<Element>( 1000 );

            bool proceed = false;

            if( elementDictionary.TryGetValue( ElementType.KnownValue.Beam, out tempList ) )
            {
                beamList.AddRange( tempList );
                allList.AddRange( tempList );
                elementDictionary.Remove( ElementType.KnownValue.Beam );

                proceed = true;
            }

            if( elementDictionary.TryGetValue( ElementType.KnownValue.PrestressedBeam, out tempList ) )
            {
                beamList.AddRange( tempList );
                allList.AddRange( tempList );
                elementDictionary.Remove( ElementType.KnownValue.PrestressedBeam );

                proceed = true;
            }

            if( elementDictionary.TryGetValue( ElementType.KnownValue.Column, out tempList ) )
            {
                columnList.AddRange( tempList );
                allList.AddRange( tempList );
                elementDictionary.Remove( ElementType.KnownValue.Column );

                proceed = true;
            }

            if( elementDictionary.TryGetValue( ElementType.KnownValue.HollowCore, out tempList ) )
            {
                hollowCoreList.AddRange( tempList );
                allList.AddRange( tempList );
                elementDictionary.Remove( ElementType.KnownValue.HollowCore );

                proceed = true;
            }

            if( elementDictionary.TryGetValue( ElementType.KnownValue.PrestressedSlab, out tempList ) )
            {
                slabList.AddRange( tempList );
                allList.AddRange( tempList );
                elementDictionary.Remove( ElementType.KnownValue.PrestressedSlab );

                proceed = true;
            }

            // No need to query the database if there are no elements.
            if( false == proceed )
            {
                return;
            }


            // Section Parameters.
            var sectionParameterArgs = new ImpactDatabase.ProfacomArgs<ImpSectionStyleParameterStd>(
                this._factory, this._project,
                GetSectionParameterQuery( this._factory, this._project ),
                ImpSectionStyleParameterStd.ElementType,
                ImpSectionStyleParameterStd.Name,
                ImpSectionStyleParameterStd.ParameterId );

            var sectionParameterList = database.Profacom( sectionParameterArgs, column => new
                                                                                          {
                                                                                              ElementType =
                                                                                              ( (ElementType)
                                                                                                column[0].Cast<string>() ).
                                                                                              EnumValue,
                                                                                              Name = column[1].Cast<string>(),
                                                                                              Parameter =
                                                                                              new SectionParameter(
                                                                                              DataConverter.Cast
                                                                                                  <SectionStyleParameter>(
                                                                                                      column[2] ),
                                                                                              column[3].Cast<double>() )
                                                                                          } );


            // Section Geometry.
            var sectionGeometryArgs = new ImpactDatabase.ProfacomArgs<ImpSectionGeometryStd>(
                this._factory, this._project,
                GetSectionGeometryQuery( this._factory, this._project ),
                ImpSectionGeometryStd.ElementType,
                ImpSectionGeometryStd.Section,
                ImpSectionGeometryStd.PointId );

            var sectionGeometryList = database.Profacom( sectionGeometryArgs, column => new
                                                                                        {
                                                                                            ElementType =
                                                                                            ( (ElementType)
                                                                                              column[0].Cast<string>() ).
                                                                                            EnumValue,
                                                                                            Name = column[1].Cast<string>(),
                                                                                            Id = column[2].Cast<int>(),
                                                                                            Point = new GeometryPoint
                                                                                                    {
                                                                                                        X =
                                                                                                            column[3].Cast
                                                                                                            <double>(),
                                                                                                        Y =
                                                                                                            column[4].Cast
                                                                                                            <double>(),
                                                                                                        Bulge =
                                                                                                            column[5].Cast
                                                                                                            <double>()
                                                                                                    }
                                                                                        } );

            // Section Core Geometry.
            var sectionCoreGeometryArgs = new ImpactDatabase.ProfacomArgs<ImpSectionCoreGeometryStd>(
                this._factory, this._project,
                GetSectionCoreGeometryQuery( this._factory, this._project ),
                ImpSectionCoreGeometryStd.ElementType,
                ImpSectionCoreGeometryStd.Section,
                ImpSectionCoreGeometryStd.CoreId,
                ImpSectionCoreGeometryStd.CorePointId );

            var sectionCoreGeometryList = database.Profacom( sectionCoreGeometryArgs, column => new
                                                                                                {
                                                                                                    ElementType =
                                                                                                    ( (ElementType)
                                                                                                      column[0].Cast<string>() )
                                                                                                    .EnumValue,
                                                                                                    Name =
                                                                                                    column[1].Cast<string>(),
                                                                                                    Id = column[2].Cast<int>(),
                                                                                                    PointId =
                                                                                                    column[3].Cast<int>(),
                                                                                                    Point = new GeometryPoint
                                                                                                            {
                                                                                                                X =
                                                                                                                    column[4].
                                                                                                                    Cast
                                                                                                                    <double>(),
                                                                                                                Y =
                                                                                                                    column[5].
                                                                                                                    Cast
                                                                                                                    <double>(),
                                                                                                                Bulge =
                                                                                                                    column[6].
                                                                                                                    Cast
                                                                                                                    <double>()
                                                                                                            }
                                                                                                } ).GroupBy(
                                                                                                    item =>
                                                                                                    new
                                                                                                    {
                                                                                                        item.ElementType,
                                                                                                        item.Name,
                                                                                                        item.Id
                                                                                                    } );


            // Section style and material.
            var sectionArgs = new ImpactDatabase.ProfacomArgs<ImpSectionStyleStd>(
                this._factory, this._project,
                GetSectionStyleQuery( this._company, this._factory, this._project ),
                ImpSectionStyleStd.ElementType,
                ImpSectionStyleStd.Name );

            var sectionStyleList = database.Profacom( sectionArgs, column => new
                                                                             {
                                                                                 ElementType =
                                                                                 ( (ElementType)column[0].Cast<string>() ).
                                                                                 EnumValue,
                                                                                 Name = column[1].Cast<string>(),
                                                                                 SectionType = column[2].Cast<string>(),
                                                                                 Width = column[3].Cast<double>(),
                                                                                 Height = column[4].Cast<double>(),
                                                                                 CutType = column[5].Cast<CutType>(),

                                                                                 // It is possible that no material was found, in that case
                                                                                 // the resulting columns will contain 'null' values.
                                                                                 // If the material is missing, we'll set the RGB value
                                                                                 // to black ( 0, 0, 0 ).
                                                                                 Material = column[6].Cast<string>(),
                                                                             } );

            // End Beam angles.
            var endBeamList = database.GetAll( GetEndBeamQuery( this._factory, this._project ).ToString(), column => new
                                                                                                                     {
                                                                                                                         ElementMark
                                                                                                                         =
                                                                                                                         column
                                                                                                                         [0].
                                                                                                                         Cast
                                                                                                                         <
                                                                                                                         string
                                                                                                                         >().
                                                                                                                         Trim(),
                                                                                                                         Side
                                                                                                                         =
                                                                                                                         column
                                                                                                                             [
                                                                                                                                 1
                                                                                                                             ]
                                                                                                                             .
                                                                                                                             Cast
                                                                                                                             <
                                                                                                                             int
                                                                                                                             >
                                                                                                                             () ==
                                                                                                                         3
                                                                                                                             ? Side
                                                                                                                                   .
                                                                                                                                   KnownValue
                                                                                                                                   .
                                                                                                                                   Left
                                                                                                                             : Side
                                                                                                                                   .
                                                                                                                                   KnownValue
                                                                                                                                   .
                                                                                                                                   Right,
                                                                                                                         Angle
                                                                                                                         =
                                                                                                                         column
                                                                                                                         [2].
                                                                                                                         Cast
                                                                                                                         <
                                                                                                                         double
                                                                                                                         >(),
                                                                                                                     } );


            var sectionList = new List<SectionGeometry>();

            foreach( var element in allList )
            {
                var style =
                    sectionStyleList.Find( x => x.ElementType == element.Geometry.ElementType && x.Name == element.Style );
                if( null != style )
                {
                    var section = new SectionGeometry( element.Geometry )
                                  {
                                      SectionType = ( (SectionType)style.SectionType ).EnumValue,
                                      CutType = style.CutType,
                                      SectionWidth = style.Width,
                                      SectionHeight = style.Height,
                                      MaterialName = element.Geometry.MaterialName,
                                      Parameters = ( from item in sectionParameterList
                                                     where item.ElementType == style.ElementType
                                                           && item.Name == style.Name
                                                     select item.Parameter ).ToDictionary( param => param.Key,
                                                                                           param => param.Value ),
                                      SectionPoints = ( from item in sectionGeometryList
                                                        where item.ElementType == style.ElementType
                                                              && item.Name == style.Name
                                                        orderby item.Id ascending
                                                        select item.Point ).ToList(),
                                      Cores = ( from item in sectionCoreGeometryList
                                                where item.Key.ElementType == style.ElementType
                                                      && item.Key.Name == style.Name
                                                let points = from n in item
                                                             orderby n.PointId ascending
                                                             select n.Point
                                                orderby item.Key.Id ascending
                                                select new CoreGeometry( points.ToList() ) ).ToList()
                                  };
                    sectionList.Add( section );
                }
                else
                {
                    // If we don't have a style, then we have to create the section as a bounding box.
                    var section = new SectionGeometry( element.Geometry )
                                  {
                                      SectionType = SectionType.KnownValue.Invalid,
                                      CutType = CutType.Symmetrical,
                                      SectionWidth = element.Geometry.Width,
                                      SectionHeight = element.Geometry.Height,
                                      MaterialName = element.Geometry.MaterialName,
                                  };
                    sectionList.Add( section );
                }
            }

            // var sectionList = ( from element in allList
            // let style = sectionStyleList.Find( x => x.ElementType == element.Geometry.ElementType && x.Name == element.Style )
            // where null != style
            // let section = new SectionGeometry( element.Geometry )
            // {
            // SectionType = ( (SectionType)style.SectionType ).EnumValue,
            // CutType = style.CutType,
            // SectionWidth = style.Width,
            // SectionHeight = style.Height,
            // MaterialName = style.Material,

            // Parameters = ( from item in sectionParameterList
            // where item.ElementType == style.ElementType
            // && item.Name == style.Name
            // select item.Parameter ).ToDictionary( param => param.Key, param => param.Value ),

            // SectionPoints = ( from item in sectionGeometryList
            // where item.ElementType == style.ElementType
            // && item.Name == style.Name
            // orderby item.Id ascending
            // select item.Point ).ToList(),

            // Cores = ( from item in sectionCoreGeometryList
            // where item.Key.ElementType == style.ElementType
            // && item.Key.Name == style.Name
            // let points = ( from n in item orderby n.PointId ascending select n.Point )
            // orderby item.Key.Id ascending
            // select new CoreGeometry( points.ToList() ) ).ToList()
            // }
            // select section ).ToList();

            foreach( var item in sectionList )
            {
                switch( item.ElementType )
                {
                    case ElementType.KnownValue.Beam:
                    case ElementType.KnownValue.PrestressedBeam:
                    {
                        // Because the height parameter might be missing in the database,
                        // we add it explicitly here.
                        item.Parameters[SectionStyleParameter.H] = item.Height;

                        var beam = new BeamGeometry( item );

                        // Check if the beam has an left vertical angle.
                        var endBeamItem =
                            endBeamList.Find(
                                endBeam => endBeam.ElementMark == beam.ElementMark && endBeam.Side == Side.KnownValue.Left );

                        // Set the left vertical angle.
                        if( null != endBeamItem )
                        {
                            beam.VerticalAngleLeft = endBeamItem.Angle;
                        }

                        // Check if the beam has an right vertical angle.
                        endBeamItem =
                            endBeamList.Find(
                                endBeam => endBeam.ElementMark == beam.ElementMark && endBeam.Side == Side.KnownValue.Right );

                        // Set the right vertical angle.
                        if( null != endBeamItem )
                        {
                            beam.VerticalAngleRight = endBeamItem.Angle;
                        }

                        modelBuilderData.Beams.Add( beam );
                        break;
                    }

                    case ElementType.KnownValue.Column:
                    {
                        // Because the height parameter might be missing in the database,
                        // we add it explicitly here.
                        item.Parameters[SectionStyleParameter.H] = item.Height;
                        modelBuilderData.Columns.Add( item );
                        break;
                    }

                    case ElementType.KnownValue.HollowCore:
                    {
                        modelBuilderData.HollowCores.Add( item );
                        break;
                    }

                    case ElementType.KnownValue.PrestressedSlab:
                    {
                        modelBuilderData.PrestressedSlabs.Add( item );
                        break;
                    }

                    default:
                    break;
                }
            }
        }

        /// <summary>
        /// The add linked data.
        /// </summary>
        /// <param name="database">
        /// The database.
        /// </param>
        /// <param name="modelBuilderData">
        /// The model builder data.
        /// </param>
        /// <param name="elementDictionary">
        /// The element dictionary.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        void AddLinkedData( ImpactDatabase database, ref ModelBuilderData modelBuilderData, ref Dictionary<ElementType.KnownValue, List<Element>> elementDictionary )
        {
            if( null == modelBuilderData )
                throw new ArgumentNullException( "modelBuilderData" );

            if( null == elementDictionary )
                throw new ArgumentNullException( "elementDictionary" );

            // No need to continue if the list is empty.
            if( 0 == elementDictionary.Count )
                return;

            List<Element> tempList = null;
            List<LinkedGeometry> linkedList = new List<LinkedGeometry>( 50 );

            if( elementDictionary.TryGetValue( ElementType.KnownValue.Linked, out tempList ) )
            {
                linkedList.AddRange( from item in tempList select new LinkedGeometry( item.Geometry ) );
                elementDictionary.Remove( ElementType.KnownValue.Linked );
            }

            if( elementDictionary.TryGetValue( ElementType.KnownValue.PrestressedLinked, out tempList ) )
            {
                linkedList.AddRange( from item in tempList select new LinkedGeometry( item.Geometry ) );
                elementDictionary.Remove( ElementType.KnownValue.PrestressedLinked );
            }

            modelBuilderData.Linked = linkedList;
        }

        /// <summary>
        /// The load geometry data.
        /// </summary>
        /// <returns>
        /// The StruSoft.Impact.V120.ProjectManager.Core.ModelBuilder.ModelBuilderData.
        /// </returns>
        public ModelBuilderData LoadGeometryData()
        {

            ModelBuilderData modelBuilderData = new ModelBuilderData();

            using( ImpactDatabase database = new ImpactDatabase() )
            {
                // Get all materials.
                var materialList = database.GetAll( GetMaterialQuery( this._company ).ToString(), column =>
                    new Material
                    {
                        Name = column[0].Cast<string>(),
                        MaterialType = column[1].Cast<MaterialType>(),
                        R = column[2].Cast<int>(),
                        G = column[3].Cast<int>(),
                        B = column[4].Cast<int>(),
                    } );

                modelBuilderData.Materials = materialList.ToDictionary( material => material.Name );




                // Get the geometry for each element.
                var elementGeometryList = database.GetAll( GetElementGeometryQuery( this._factory, this._project ), column =>
                    new Element
                    {
                        Style = column[2].Cast<string>(),
                        Geometry = new ElementGeometry
                                   {
                                       ElementMark = column[0].Cast<string>().Trim(),
                                       ElementType = ( (ElementType)column[1].Cast<string>() ).EnumValue,

                                       Length = column[3].Cast<double>(),
                                       Width = column[4].Cast<double>(),
                                       Height = column[5].Cast<double>(),
                                       AngleLeft = column[6].Cast<double>(),
                                       AngleRight = column[7].Cast<double>(),
                                       MaterialName = column[8].Cast<string>(),
                                   }
                    } );



                #region Model Geometry

                // Get all the instances of each element.
                var elementGeometryInstanceList = database.GetAll( GetElementGeometryInstanceQuery( this._factory, this._project ), column => new
                {
                    ElementMark = column[0].Cast<string>().Trim(),
                    Instance = new ElementTransformation
                               {
                                   Id = column[1].Cast<int>(),
                                   Building = column[2].Cast<string>().Trim(),
                                   Level = column[3].Cast<int>(),
                                   Rotation = column[4].Cast<double>(),
                                   PositionX = column[5].Cast<double>(),
                                   PositionY = column[6].Cast<double>(),
                                   PositionZ = column[7].Cast<double>(),
                                   NormalX = column[8].Cast<double>(),
                                   NormalY = column[9].Cast<double>(),
                                   NormalZ = column[10].Cast<double>(),
                                   ErectionSequenceNo = column[11].Cast<int?>() ?? 0,
                               }
                } );

                #endregion Model Geometry

                #region Recesses

                // Get all recesses used in the project.
                var recessList = database.GetAll( GetRecessQuery( this._factory, this._project ), column => new
                {
                    ElementMark = column[0].Cast<string>().Trim(),
                    Recess = new Recess
                             {
                                 Id = column[1].Cast<int>(),
                                 Type = ( (RecessType)column[2].Cast<string>() ).EnumValue,
                                 X1 = column[3].Cast<double>(),
                                 Y1 = column[4].Cast<double>(),
                                 X2 = column[5].Cast<double>(),
                                 Y2 = column[6].Cast<double>(),
                                 Rotation = column[7].Cast<double>(),
                                 Countersinking = column[8].Cast<double>(),
                                 Side = ( (Side)column[9].Cast<string>() ).EnumValue,
                                 DimensionType = ( (DimensionType)column[10].Cast<string>() ).EnumValue,
                                 Diameter = column[11].Cast<double>(),
                             }
                } );

                // Get all recess points for each polygon recess in the project.
                var recessPointList = database.GetAll( GetRecessPointsQuery( this._factory, this._project ), column => new
                {
                    ElementMark = column[0].Cast<string>().Trim(),
                    RecessId = column[1].Cast<int>(),
                    Id = column[2].Cast<int>(),
                    Point = new GeometryPoint
                            {
                                X = column[3].Cast<double>(),
                                Y = column[4].Cast<double>(),
                                Bulge = column[5].Cast<double>()
                            }

                } );

                #endregion Recesses

                // Add information that is common for all element types.
                foreach( var element in elementGeometryList )
                {
                    // Element instances.
                    element.Geometry.Instances = ( from instance in elementGeometryInstanceList
                                                   where element.Geometry.ElementMark == instance.ElementMark
                                                   select instance.Instance ).ToList();

                    // Recess instances.
                    element.Geometry.Recesses = ( from recess in recessList
                                                  where element.Geometry.ElementMark == recess.ElementMark

                                                  // Recess points.
                                                  let points = recess.Recess.Points =
                                                  ( from point in recessPointList
                                                    where point.ElementMark == element.Geometry.ElementMark &&
                                                    point.RecessId == recess.Recess.Id
                                                    select point.Point ).ToList()

                                                  select recess.Recess ).ToList();
                }

                var groupedElements = elementGeometryList.GroupBy( element => element.Geometry.ElementType ).ToDictionary( x => x.Key, x => x.ToList() );

                this.AddWallData( database, ref modelBuilderData, ref groupedElements );

                this.AddSlabData( database, ref modelBuilderData, ref groupedElements );

                this.AddSectionData( database, ref modelBuilderData, ref groupedElements );

                this.AddLinkedData( database, ref modelBuilderData, ref groupedElements );
            }

            return modelBuilderData;
        }


        /// <summary>
        /// The element.
        /// </summary>
        class Element
        {
            /// <summary>
            /// Gets or sets the style.
            /// </summary>
            public string Style
            { get; set; }

            /// <summary>
            /// Gets or sets the geometry.
            /// </summary>
            public ElementGeometry Geometry
            { get; set; }
        }
    }
}