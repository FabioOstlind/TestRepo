using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using StruSoft.Impact.V120.Planning.Common;
using StruSoft.Impact.V120.DB;
using StruSoft.Impact.V120.DB.Query;

namespace StruSoft.Impact.V120.Services
{
    using StruSoft.Impact.DataTypes;
    using StruSoft.Impact.V120.Report.Common;

    /// <summary>
    /// Bed Manager Business logic service
    /// </summary>  
    public partial class ProjectManager : IBedManager
    {
        /// <summary>
        /// Positions elements of a set of beds
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="forms">A ser of beds(castUnits)</param>
        /// <param name="elements">A list of elements to be positioned</param>
        /// <returns></returns>
        public BedResult AutoBedManage( BedFilter filter, List<RecProductionFormStd> forms, List<RecTMElement> elements )
        {
            if( null == filter || null == forms || forms.Count == 0 || null == elements || elements.Count == 0 )
            {
                throw new ArgumentNullException( "AutoBedManage, Missing input!" );
            }
            var castResult = new BedResult();
            if( null == castResult.PositionedElements )
            {
                castResult.PositionedElements = new List<RecTMElement>();
            }
            // Let's work on a copy of the given curFilter
            var curFilter = new BedFilter( filter );

            //DateTime curDate = curfilter.StartDateFrom;
            var maxShift = forms.OrderBy(p => p.NbrOfShift).Last().NbrOfShift;
            var firstLoop = true;
            var unpositioned = elements;
            var totPositionedElements = new List<RecTMElement>();

            do
            {
                var firstShift = firstLoop? filter.Shift : 1;

                castResult.FirstShift = Math.Min( castResult.FirstShift, firstShift);
                castResult.LastShift = Math.Max( castResult.LastShift, maxShift);
                castResult.CastDays.Add( curFilter.StartDateFrom );

                var hasDayExistingElements = false;
                var numberOfPositionedElements = 0;

                for( var shift = firstShift; shift <= maxShift; shift++ )
                {
                    curFilter.Shift = shift;
                    var curForms = ( from o in forms where o.NbrOfShift >= curFilter.Shift select o ).ToList();
                    if( curForms.Count <= 0 )
                    {
                        continue;
                    }
                    var tempPositioned = new List<RecTMElement>();
                    bool hasShiftExistingElements;
                    var tempResult = this.ProcessElement( curFilter, curForms, unpositioned, tempPositioned, out hasShiftExistingElements );

                    castResult.PositionedElements.AddRange( tempPositioned );

                    if( hasShiftExistingElements )
                    {
                        hasDayExistingElements = true;
                    }
                    if( tempResult.NumberOfElements > 0 )
                    {
                        castResult.NumberOfCasts += tempResult.NumberOfCasts;
                    }
                    totPositionedElements.AddRange( tempPositioned );

                    unpositioned = this.GetUnpositionedElements( elements, totPositionedElements );

                    castResult.NumberOfElements = totPositionedElements.Count;
                    castResult.Element = tempResult.Element;

                    // For single cast interrupt here, please
                    var done = !curFilter.MultiCast || totPositionedElements.Count == elements.Count;
                    if( done )
                    {
                        return castResult;
                    }

                    numberOfPositionedElements += tempPositioned.Count;
                }

                // If none of the shifts could position then interrupt and return
                if( 0 == numberOfPositionedElements && !hasDayExistingElements )
                {
                    break;
                }

                curFilter.StartDateFrom = curFilter.StartDateFrom.AddDays( 1 );
                firstLoop = false;
            } while( true );

            return castResult;
        }

        /// <summary>
        /// MoveElement
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="form"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public BedResult MoveElementBedManager( BedFilter filter, RecProductionFormStd form, List<RecTMElement> elements )
        {
            if( null == filter || null == form || null == elements || elements.Count == 0 )
            {
                throw new ArgumentNullException( "AutoBedManage, Missing input!" );
            }
            var castUnits = new List<RecProductionFormStd> { form };
            var positionedElements = new List<RecTMElement>();
            bool hasShiftExistingElements;
            return ProcessElement( filter, castUnits, elements, positionedElements, out hasShiftExistingElements, true );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="castUnits"></param>
        /// <param name="elements"></param>
        /// <param name="positionedElements"> </param>
        /// <param name="hasShiftExistingElements"> </param>
        /// <param name="isUserPositionedElement"> </param>
        /// <returns></returns>
        private BedResult ProcessElement( BedFilter filter, List<RecProductionFormStd> castUnits, 
            IList<RecTMElement> elements, List<RecTMElement> positionedElements, out bool hasShiftExistingElements,
            bool isUserPositionedElement = false )
        {
            if( positionedElements == null )
            {
                throw new ArgumentNullException( "positionedElements" );
            }
            var bedResult = new BedResult();
            if( null == filter || null == castUnits || castUnits.Count == 0 || null == elements || elements.Count == 0 )
            {
                throw new ArgumentNullException( "Missing curFilter, castUnits or positionedElements!" );
            }
            //
            var bpc = new ProductionBedContainer();
            bpc.LoadPositionedElementData( castUnits, filter, isUserPositionedElement );

            hasShiftExistingElements = false;
            foreach( var castUnit in castUnits )
            {
                //var controlfilter = new BedFilter( filter );
                var hasElements = bpc.HasExistingElements( castUnit );
                // If there are no elements on the current cast unit of type bed
                // then adjust the cast unit data (element type, style,strands) to the first element
                var recProductionCast = bpc.GetProductionCast( castUnit );
                if( castUnit.FormType == FormType.Bed && !hasElements )
                {
                    //controlfilter.ForceStrandControl = false;
                    //controlfilter.ForceStyleControl = false;
                    recProductionCast = bpc.AdjustProductionCastToElement( castUnit, elements[0] );
                }

                // Register element existing flag
                if( hasElements )
                {
                    hasShiftExistingElements = true;
                }
                // interrupt for single element cast unit if there are existing elements
                if( filter.SingleElementCastUnit && hasElements )
                {
                    continue;
                }

                var unpositioned = GetUnpositionedElements( elements, positionedElements );
                var allowedElements = LoadAllowedElements( filter, castUnit, recProductionCast, unpositioned, true );
                allowedElements = ( from o in allowedElements where o.ElementWidthOnBed < castUnit.MaxWidth || DoubleHelper.IsEqual( o.ElementWidthOnBed, castUnit.MaxWidth )  select o ).ToList();
                if( allowedElements.Count > 0 )
                {
                    List<RecTMElement> elementsToSave;
                    if( isUserPositionedElement )
                    {
                        elementsToSave = new List<RecTMElement>();
                        foreach( var element in elements  )
                        {
                            var found = (from o in allowedElements
                                          where
                                              o.ElementId == element.ElementId
                                              && o.Project.Equals( element.Project ) select o).Any();
                            if( found )
                            {
                                elementsToSave.Add( element );
                            }
                        }
                    }
                    else
                    {
                        // Sort elements by dim
                        var sortedElements =
                            allowedElements.OrderBy( x => x.ElementWidthOnBed ).ThenBy( x => x.ElementLengthOnBed ).
                                ToList();
                        //Debug( sortedElements );
                        elementsToSave = bpc.PositionElementsOnBed( castUnit, sortedElements );
                    }
                    bpc.Save( castUnit, elementsToSave );
                    if( null != elementsToSave )
                    {
                        bedResult.NumberOfCasts++;
                        positionedElements.AddRange( elementsToSave );
                    }
                }
                if( positionedElements.Count == elements.Count )
                {
                    //Then we are done
                    break;
                }
            }
            bedResult.NumberOfElements = positionedElements.Count;

            if( positionedElements.Count == 0 )
            {
                // Let's prepare some hint for the user
                var tempList = new List<RecTMElement> { elements[0] };
                var elemInfo = LoadAllowedElements( filter, castUnits[0], null, tempList, false );
                if( null != elemInfo && elemInfo.Count > 0 )
                {
                    bedResult.Element = elemInfo[0];
                }
            }

            return bedResult;
        }

        /// <summary>
        /// Returns a list with unpositioned elements
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="totPositioned"></param>
        /// <returns></returns>
        private List<RecTMElement> GetUnpositionedElements( IEnumerable<RecTMElement> elements, List<RecTMElement> totPositioned )
        {
            if( elements == null )
            {
                throw new ArgumentNullException( "elements" );
            }
            return elements.Where( elem => !this.Find( totPositioned, elem ) ).ToList();
        }

        /// <summary>
        /// Looks for an element
        /// </summary>
        /// <param name="totPositioned"></param>
        /// <param name="elem"></param>
        /// <returns></returns>
        public bool Find( List<RecTMElement> totPositioned, RecTMElement elem )
        {
            return totPositioned.Any( positioned => positioned.GetKey().Equals( elem.GetKey() ) );
        }

        /// <summary>
        /// Returns no of strands
        /// </summary>
        /// <param name="castunit"></param>
        /// <returns></returns>
        private int GetNumberOfStrands( RecProductionFormStd castunit )
        {
            var nbrOfStrands = 0;
            if( castunit == null || string.IsNullOrEmpty( castunit.Factory ) || string.IsNullOrEmpty( castunit.Project ) )
            {
                return nbrOfStrands;
            }
            var svc = new ProjectManager();
            var recStrand = new RecProductionFormStrandStd { Factory = castunit.Factory, Project = castunit.Project, Name = castunit.Name };
            var strands = svc.LoadProductionFormStrandStd( recStrand );
            if( null != strands )
            {
                nbrOfStrands = (from o in strands where o.IsUsed select o).Count();
            }
            return nbrOfStrands;
        }

        /// <summary>
        /// Returns no of strands
        /// </summary>
        /// <param name="cast"></param>
        /// <returns></returns>
        private int GetNumberOfStrands( RecProductionCast cast )
        {
            if( cast == null || null == cast.Strands )
            {
                return 0;
            }
            return cast.Strands.Count;
            //var nbrOfStrands = 0;
            //if( cast == null || string.IsNullOrEmpty( cast.Factory ) || string.IsNullOrEmpty( cast.Project ) || 0 == cast.CastId)
            //{
            //    return nbrOfStrands;
            //}
            //var svc = new ProductionCastStrand();
            //var recStrand = new RecProductionCastStrand { Factory = cast.Factory, Project = cast.Project, CastId = cast.CastId };
            //var strands = svc.Load( recStrand );
            //if( null != strands )
            //{
            //    nbrOfStrands = strands.Count;
            //}
            //return nbrOfStrands;
        }

        /// <summary>
        /// Returns a list of elements that are allowed to bed positioned of the given castunit(bed)
        /// </summary>
        /// <param name="filter">curFilter</param>
        /// <param name="form">castunit</param>
        /// <param name="recProductionCast"> </param>
        /// <param name="elements">A list of elements to be positioned</param>
        /// <param name="strict">Strict flag</param>
        /// <returns></returns>
        private List<RecTMElement> LoadAllowedElements( BedFilter filter, RecProductionFormStd form, RecProductionCast recProductionCast, List<RecTMElement> elements, bool strict )
        {
            if( elements == null || elements.Count == 0 || form == null &&
                filter == null || string.IsNullOrEmpty( filter.Factory ) || string.IsNullOrEmpty( filter.Project ) )
            {
                return null;
            }

            var inVect = ( from o in elements select o.ElementId ).ToArray();

            List<RecTMElement> result;
            var query = new ImpactQuery()
            {
                Select =
				{
                    ImpElement.ElementMark,

                    ImpElement.ElementLength,
					ImpElement.ElementWidth,
					ImpElement.ElementHeight,

                    ImpElement.ElementType,
                    ImpElement.AngleLeftSide,
					ImpElement.AngleRightSide,

					ImpElement.Style,
					ImpElement.Strandptn,
					ImpElement.NbrOfStrands,

                    ImpModelGeometry.Factory,
                    ImpModelGeometry.Project,
                    ImpModelGeometry.ElementId,
				},

                From = { ImpElement.As( "ELM" ) },

                Join =
				{
					Join.Left( ImpModelGeometry.As( "GEO" ),	
						ImpModelGeometry.Factory.Equal( ImpElement.Factory ),
						ImpModelGeometry.Project.Equal( ImpElement.Project ),
						ImpModelGeometry.ElementMark.Equal( ImpElement.ElementMark ) ),
				},

                Where = {
							ImpElement.Factory.Equal( filter.Factory ), 
							ImpElement.Project.Equal( filter.Project ), 
							ImpModelGeometry.ElementId.In( inVect ), 
						},
            };

            if( form != null && ( strict && form.FormType == FormType.Bed ) )
            {
                query.Where.Add( ImpElement.ElementType.Equal( form.ElementType ) );

                var hasStrands = form.ElementType.Equals( ImpactElementType.HollowCore ) ||
                                  form.ElementType.Equals( ImpactElementType.PrestressedSlab ) ||
                                  form.ElementType.Equals( ImpactElementType.PrestressedFormSlab );

                if( hasStrands )
                {
                    if( filter.ForceStyleControl )
                    {
                        var style = (null != recProductionCast)? recProductionCast.Style : form.Style;
                        var strandptn = (null != recProductionCast)? recProductionCast.Strandptn : form.Strandptn;
                        var nbrOfStrands = (null != recProductionCast)? GetNumberOfStrands( recProductionCast ) : GetNumberOfStrands( form );

                        query.Where.Add( ImpElement.Style.Equal( style ) );
                        query.Where.Add( ImpElement.Strandptn.Equal( strandptn ) );
                        query.Where.Add( ImpElement.NbrOfStrands.LessThanOrEqual( nbrOfStrands ) );
                    }
                }
            }

            var statement = query.ToString();
            using( var database = new ImpactDatabase() )  
            {
                result = database.GetAll( statement, ElementParse );
            }

            return result;
        }

        /// <summary>
        /// Parsing helper
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        private static RecTMElement ElementParse( DbDataReader column )
        {
            var record = new RecTMElement
                             {
                                 ElementMark = column[0].Cast<string>(),
                                 ElementLength = column[1].Cast<double?>() ?? 0d,
                                 ElementWidth = column[2].Cast<double?>() ?? 0d,
                                 ElementHeight = column[3].Cast<double?>() ?? 0d,
                                 ElementType = column[4].Cast<string>(),
                                 AngleLeft = column[5].Cast<double?>() ?? 0d,
                                 AngleRight = column[6].Cast<double?>() ?? 0d,
                                 Style = column[7].Cast<string>(),
                                 Strandptn = column[8].Cast<string>(),
                                 NbrOfStrands = column[9].Cast<int>(),
                                 Factory = column[10].Cast<string>(),
                                 Project = column[11].Cast<string>(),
                                 ElementId = column[12].Cast<int?>() ?? 0
                             };

            return record;
        }

        /// <summary>
        /// Debug helper
        /// </summary>
        /// <param name="elements"></param>
        private void Debug( IEnumerable<RecTMElement> elements )
        {
            foreach( RecTMElement elem in elements )
            {
                Console.WriteLine( "Transport Id={0}, Element Id={1} ", elem.TransportId, elem.ElementId );
            }
        }

        /// <summary>
        /// Sorts element on bed
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="form"></param>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public bool SortBedElements( BedFilter filter, RecProductionFormStd form, SortType sortType )
        {
            // Let's work on a copy of the given curFilter
            // Make sure that SingleElementCastUnit is set to false
            var curFilter = new BedFilter( filter ) { SingleElementCastUnit = false };

            var castData = LoadBedPlanning( curFilter, form );
            var forms = castData.CastList;
            if( forms != null )
            {
                foreach( var bed in forms )
                {
                    if( bed.ProductionCast == null )
                    {
                        continue;
                    }
                    if( bed.ProductionCast.Elements == null || bed.ProductionCast.Elements.Count <= 0 )
                    {
                        continue;
                    }
                    List<RecTMElement> sortedElements;
                    if( sortType == SortType.DimensionAscending )
                    {
                        sortedElements = bed.ProductionCast.Elements.OrderBy( x => x.ElementWidth ).ThenBy( x => x.ElementLengthOnBed ).ToList();
                    }
                    else if( sortType == SortType.DimensionDescending )
                    {
                        sortedElements = bed.ProductionCast.Elements.OrderByDescending( x => x.ElementWidth ).ThenByDescending( x => x.ElementLengthOnBed ).ToList();
                    }
                    else if( sortType == SortType.ErectionSequenceAscending )
                    {
                        sortedElements = bed.ProductionCast.Elements.OrderBy( x => x.ErectionSequenceNo ).ToList();
                    }
                    else //( sortType == SortType.ErectionSequenceDescending )
                    {
                        sortedElements = bed.ProductionCast.Elements.OrderByDescending( x => x.ErectionSequenceNo ).ToList();
                    }

                    var bpc = new ProductionBedContainer();
                    bpc.Sort( sortedElements, bed, curFilter );
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a list of projects that are involved in a certain cast
        /// </summary>
        /// <param name="filter">curFilter</param>
        /// <param name="castId">castId</param>
        /// <returns></returns>
        public List<string> LoadCastProjects( BedFilter filter, int castId )
        {
            if( null == filter || string.IsNullOrWhiteSpace( filter.Factory ) )
            {
                return null;
            }

            var query = new ImpactQuery( true )
            {
                Select =
				{
					ImpModelPlanning.Project,
				},

                From = { ImpProductionCast.As( "CST" ) },

                Join =
				{
					Join.Left( ImpModelPlanning.As( "PLN" ),	
						ImpModelPlanning.Factory.Equal( ImpProductionCast.Factory ),
						ImpModelPlanning.CastId.Equal( ImpProductionCast.CastId ) ),
				},

                Where = {
							ImpModelPlanning.CastId.GreaterThan( 0 ), 
							ImpProductionCast.Factory.Equal( filter.Factory ), 
						},
            };

            if( castId > 0 )
            {
                query.Where.Add( ImpProductionCast.CastId.Equal( castId ) );
            }
            else
            {
                query.Where.Add( ImpProductionCast.StartDate.Equal( filter.StartDateFrom ) );
                query.Where.Add( ImpProductionCast.Shift.Equal( filter.Shift ) );
            }

            string statement = query.ToString();

            List<string> result;
            using( ImpactDatabase database = new ImpactDatabase() )
            {
                result = database.GetAll( statement, Parse );
            }

            return result;
        }

        /// <summary>
        /// Parsing helper
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        private static string Parse( DbDataReader dataReader )
        {
            return dataReader[0].Cast<string>();
        }

        /// <summary>
        /// Returns a set of casts with element information 
        /// </summary>
        /// <param name="filter">curFilter</param>
        /// <param name="reportFilter"> </param>
        /// <returns></returns>
        public CastScheduleData LoadCastSchedule( BedFilter filter, ReportFilter reportFilter )
        {
            if( filter == null || string.IsNullOrEmpty( filter.Factory ) || string.IsNullOrEmpty( filter.Project ) )
            {
                return null;
            }

            var query = new ImpactQuery()
            {
                Select =
				{
					ImpProductionCast.CastId,
					ImpProductionCast.Description,
					ImpProductionCast.StartDate,
					ImpProductionCast.EndDate,
					ImpProductionCast.Tolerance,
					ImpProductionCast.ElementType,
					ImpProductionCast.Style,
					ImpProductionCast.Strandptn,
					ImpProductionCast.CastStatus,
                    ImpProductionCast.CastType,
					ImpProductionCast.Form,
					ImpProductionCast.Shift,

                    Aggregate.Count( ImpProductionCastStrand.StrandPos ),

					ImpProductionFormStd.Location,
                    ImpProductionFormStd.FormType,
					ImpProductionFormStd.MaxLength,
					ImpProductionFormStd.MaxWidth,
					ImpProductionFormStd.MaxHeight,
                    ImpProductionFormStd.ElementType,

					ImpModelPlanning.ElementId,
					ImpModelGeometry.ElementMark,

                    ImpModelGeometry.Building,
                    ImpModelGeometry.FloorId,
                    ImpModelGeometry.Phase,

					ImpModelPlanning.ErectionSequenceNo,
					ImpModelPlanning.TransportId,

					ImpModelPlanning.BedSequenceNo,
					ImpModelPlanning.BedX,
					ImpModelPlanning.BedY,
					ImpModelPlanning.BedZ,
					ImpModelPlanning.BedRotation,
                    ImpModelPlanning.ElementIdStatus,

                    ImpModelPlanning.ProductionDate,
                    ImpModelPlanning.PlannedProductionDate,

                    ImpElement.ElementLength,
					ImpElement.ElementWidth,
					ImpElement.ElementHeight,
					ImpElement.ElementType,
					ImpElement.GrossArea,
					ImpElement.NetArea,
					ImpElement.Volume,
					ImpElement.Product,
					ImpElement.Project,
					ImpElement.AngleLeftSide,
					ImpElement.AngleRightSide,
                    ImpElement.Mass,

					ImpElement.Style,
					ImpElement.Strandptn,
					ImpElement.NbrOfStrands,
				},

                From = { ImpProductionCast.As( "CST" ) },

                Join =
				{
					Join.Left( ImpProductionCastStrand.As( "STA" ),	
						ImpProductionCast.Factory.Equal( ImpProductionCastStrand.Factory ),
						ImpProductionCast.Project.Equal( ImpProductionCastStrand.Project ),//Factory, Factory
						ImpProductionCast.CastId.Equal( ImpProductionCastStrand.CastId ) ),

					Join.Left( ImpProductionFormStd.As( "FRM" ),	
						ImpProductionCast.Factory.Equal( ImpProductionFormStd.Factory ),
						ImpProductionCast.Project.Equal( ImpProductionFormStd.Project ),// Factory, Factory for productionCast & ProductionCastStrand
						ImpProductionCast.Form.Equal( ImpProductionFormStd.Name ) ),

					Join.Left( ImpModelPlanning.As( "MPL" ),	
						ImpProductionCast.Factory.Equal( ImpModelPlanning.Factory ),
						ImpProductionCast.Project.Equal( ImpModelPlanning.Factory ),// Factory, Factory for productionCast & ProductionCastStrand
						ImpProductionCast.CastId.Equal( ImpModelPlanning.CastId ) ),


					Join.Left( ImpModelGeometry.As( "MGO" ),	
						ImpModelPlanning.Factory.Equal( ImpModelGeometry.Factory ),
						ImpModelPlanning.Project.Equal( ImpModelGeometry.Project ),
						ImpModelPlanning.ElementId.Equal( ImpModelGeometry.ElementId ) ),

					Join.Left( ImpElement.As( "ELM" ),	
						ImpModelGeometry.Factory.Equal( ImpElement.Factory ),
						ImpModelGeometry.Project.Equal( ImpElement.Project ),
						ImpModelGeometry.ElementMark.Equal( ImpElement.ElementMark ) ),
				},

                Where = {
							ImpProductionCast.Factory.Equal( filter.Factory ), 
							ImpProductionCast.Project.Equal( filter.Factory ), // Factory, Factory for productionCast & ProductionCastStrand
                            //ImpModelPlanning.CastId.GreaterThan( 0 ), 
						},
                GroupBy = 
                {
					ImpProductionCast.CastId,
					ImpProductionCast.Description,
					ImpProductionCast.StartDate,
					ImpProductionCast.EndDate,
					ImpProductionCast.Tolerance,
					ImpProductionCast.ElementType,
					ImpProductionCast.Style,
					ImpProductionCast.Strandptn,
					ImpProductionCast.CastStatus,
                    ImpProductionCast.CastType,
					ImpProductionCast.Form,
					ImpProductionCast.Shift,

					ImpProductionFormStd.Location,
                    ImpProductionFormStd.FormType,
					ImpProductionFormStd.MaxLength,
					ImpProductionFormStd.MaxWidth,
					ImpProductionFormStd.MaxHeight,
                    ImpProductionFormStd.ElementType,

					ImpModelPlanning.ElementId,
					ImpModelGeometry.ElementMark,

                    ImpModelGeometry.Building,
                    ImpModelGeometry.FloorId,
                    ImpModelGeometry.Phase,

					ImpModelPlanning.ErectionSequenceNo,
					ImpModelPlanning.TransportId,

					ImpModelPlanning.BedSequenceNo,
					ImpModelPlanning.BedX,
					ImpModelPlanning.BedY,
					ImpModelPlanning.BedZ,
					ImpModelPlanning.BedRotation,
                    ImpModelPlanning.ElementIdStatus,

                    ImpModelPlanning.ProductionDate,
                    ImpModelPlanning.PlannedProductionDate,

					ImpElement.ElementLength,
					ImpElement.ElementWidth,
					ImpElement.ElementHeight,
					ImpElement.ElementType,
					ImpElement.GrossArea,
					ImpElement.NetArea,
					ImpElement.Volume,
					ImpElement.Product,
					ImpElement.Project,
					ImpElement.AngleLeftSide,
					ImpElement.AngleRightSide,
                    ImpElement.Mass,

					ImpElement.Style,
					ImpElement.Strandptn,
					ImpElement.NbrOfStrands,
                },

                OrderBy = 
				{ 
					{ ImpElement.Project },
					{ ImpProductionCast.StartDate, OrderBy.Descending },
					{ ImpProductionCast.CastId },
				},
            };

            var whereStatus = WhereGroup.Or( new Where[] { } );

            if( filter.NoStatus )
            {
                whereStatus.Add( ImpProductionCast.CastStatus.Equal( CastStatus.NoStatus ) );
            }
            if( filter.Planned )
            {
                whereStatus.Add( ImpProductionCast.CastStatus.Equal( CastStatus.Planned ) );
            }
            if( filter.Produced )
            {
                whereStatus.Add( ImpProductionCast.CastStatus.Equal( CastStatus.Produced ) );
            }

            if( whereStatus.Count > 0 )
            {
               query.Where.Add( whereStatus );
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

            if( filter.Shift != 0 )
            {
                query.Where.Add( ImpProductionCast.Shift.Equal( filter.Shift ) );
            }
            if( !string.IsNullOrEmpty(filter.Location) && !filter.Location.Equals( Filter.All ) )
            {
                query.Where.Add( ImpProductionFormStd.Location.Equal( filter.Location ) );
            }
            if( !filter.Bed.Equals( Filter.All ) )
            {
                query.Where.Add( ImpProductionCast.Form.Equal( filter.Bed ) );
            }
            if( !string.IsNullOrEmpty( filter.CastId ) )
            {
                query.Where.Add( ImpProductionCast.CastId.Like( filter.CastId ) );
            }

            if( reportFilter != null && reportFilter.Ranges.Count > 0 )
            {
                var list = new List<Where>();
                foreach( var range in reportFilter.Ranges )
                {
                    if( !string.IsNullOrEmpty( range.From ) && !string.IsNullOrEmpty( range.To ) )
                    {
                        list.Add( ImpProductionCast.CastId.Between( range.From, range.To ) );
                    }
                }

                if( list.Count > 0 )
                {
                    query.Where.Add( WhereGroup.Or( list.ToArray() ) );
                }
            }

            var statement = query.ToString();

            var tmList = new List<RecProductionCast>();
            var parser = new CastParser( tmList );

            using( var database = new ImpactDatabase() )
            {
                var list = database.GetAll( statement, ParseSchedule );

                foreach( var item in list )
                {
                    item.Form.Factory = filter.Factory;
                    item.Cast.Factory = filter.Factory;

                    item.Form.Project = filter.Project;
                    item.Cast.Project = filter.Project;

                    if( null != item.Element )
                    {
                        item.Element.Factory = filter.Factory;
                        // Never set element.project to curFilter.project
                        // since we retrieve elmenets from different projects!                    
                    }

                    parser.Parse( item.Form, item.Cast, item.Element );
                }
            }

            // Now load all the existing forms
            var formSvc = new ProjectManager();
            var productionFormStdData = formSvc.LoadProductionFormStd( new BedFilter( filter ) { Location = Filter.All } );
            return new CastScheduleData( tmList, productionFormStdData );
        }

        /// <summary>
        /// Parsing helper
        /// </summary>
        /// <param name="column">column</param>
        /// <returns></returns>
        private static DataHolder ParseSchedule( DbDataReader column )
        {
            RecProductionCast cast = new RecProductionCast()
            {
                CastId = DataConverter.Cast<int>( column[0] ),

                Description = DataConverter.Cast<string>( column[1] ),
                StartDate = DataConverter.Cast<DateTime>( column[2] ),
                EndDate = DataConverter.Cast<DateTime?>( column[3] ),
                Tolerance = DataConverter.Cast<double>( column[4] ),
                ElementType = DataConverter.Cast<string>( column[5] ),
                Style = DataConverter.Cast<string>( column[6] ),
                Strandptn = DataConverter.Cast<string>( column[7] ),
                CastStatus = DataConverter.Cast<int>( column[8] ),
                CastType = DataConverter.Cast<int>( column[9] ),

                Form = DataConverter.Cast<string>( column[10] ),
                Shift = DataConverter.Cast<int>( column[11] ),
                NumOfStrands = DataConverter.Cast<int>( column[12] ),
                Location = DataConverter.Cast<string>( column[13] ),
            };
            RecProductionFormStd form = new RecProductionFormStd()
            {
                Location = DataConverter.Cast<string>( column[13] ), // Location repeated here!!!
                FormType = DataConverter.Cast<int>( column[14] ),
                MaxLength = DataConverter.Cast<double?>( column[15] ) ?? 0d,
                MaxWidth = DataConverter.Cast<double?>( column[16] ) ?? 0d,
                MaxHeight = DataConverter.Cast<double?>( column[17] ) ?? 0d,
                ElementType = DataConverter.Cast<string>( column[18] ),
            };

            RecTMElement elem = null;
            int elementId = DataConverter.Cast<int?>( column[19] ) ?? 0;
            if( 0 != elementId )
            {
                elem = new RecTMElement()
                {
                    ElementId = elementId,
                    ElementMark = DataConverter.Cast<string>( column[20] ),

                    Building = DataConverter.Cast<string>( column[21] ),
                    FloorId = DataConverter.Cast<int?>( column[22] ) ?? 0,
                    Phase = DataConverter.Cast<string>( column[23] ),

                    ErectionSequenceNo = DataConverter.Cast<int?>( column[24] ) ?? 0,
                    TransportId = DataConverter.Cast<int?>( column[25] ) ?? 0,

                    BedSequenceNo = DataConverter.Cast<int>( column[26] ),
                    BedX = DataConverter.Cast<double>( column[27] ),
                    BedY = DataConverter.Cast<double>( column[28] ),
                    BedZ = DataConverter.Cast<double>( column[29] ),
                    BedRotation = DataConverter.Cast<double>( column[30] ),
                    ElementIdStatus = DataConverter.Cast<int>( column[31] ),

                    ProductionDate = column[32].Cast<DateTime?>(),
                    PlannedProductionDate = column[33].Cast<DateTime?>(),

                    ElementLength = DataConverter.Cast<double?>( column[34] ) ?? 0d,
                    ElementWidth = DataConverter.Cast<double?>( column[35] ) ?? 0d,
                    ElementHeight = DataConverter.Cast<double?>( column[36] ) ?? 0d,
                    ElementType = DataConverter.Cast<string>( column[37] ),
                    GrossArea = DataConverter.Cast<double?>( column[38] ) ?? 0d,
                    NetArea = DataConverter.Cast<double?>( column[39] ) ?? 0d,
                    Volume = DataConverter.Cast<double?>( column[40] ) ?? 0d,
                    Product = DataConverter.Cast<string>( column[41] ),
                    Project = DataConverter.Cast<string>( column[42] ),
                    AngleLeft = DataConverter.Cast<double?>( column[43] ) ?? 0d,
                    AngleRight = DataConverter.Cast<double?>( column[44] ) ?? 0d,
                    Mass = DataConverter.Cast<double?>( column[45] ) ?? 0d,

                    Style = DataConverter.Cast<string>( column[46] ),
                    Strandptn = DataConverter.Cast<string>( column[47] ),
                    NbrOfStrands = DataConverter.Cast<int>( column[48] ),
                };
            }

            return new DataHolder( cast, form, elem );
        }

        /// <summary>
        /// Returns CastData which contains the following info
        /// (1) List<RecProductionFormStd>: a set of beds, each with one production cast ONLY.
        /// The production cast object can contain elements.
        /// (2) ProductionFormStdData: Production Forms Standard Data
        /// </summary>
        /// <param name="filter">BedFilter</param>
        /// <param name="form">RecProductionFormStd</param>
        /// <returns></returns>
        public CastData LoadBedPlanning( BedFilter filter, RecProductionFormStd form )
        {
            if( null == filter || 
                string.IsNullOrEmpty( filter.Factory ) || 
                string.IsNullOrEmpty( filter.Project ) )
            {
                return null;
            }

            ImpactQuery query = new ImpactQuery()
            {
                Select =
				{
					ImpProductionCast.CastId,
					ImpProductionCast.Description,
					ImpProductionCast.StartDate,
					ImpProductionCast.EndDate,
					ImpProductionCast.Tolerance,
					ImpProductionCast.ElementType,
                    ImpProductionCast.Style,
					ImpProductionCast.Strandptn,
                    ImpProductionCast.CastType,
					ImpProductionCast.CastStatus,
                    ImpProductionCast.Form,
					ImpProductionCast.Shift,
					Aggregate.Count( ImpProductionCastStrand.StrandPos ),

					ImpProductionFormStd.Name,
					ImpProductionFormStd.MaxLength,
					ImpProductionFormStd.MaxWidth,
					ImpProductionFormStd.MaxHeight,
                    ImpProductionFormStd.ElementType,
                    ImpProductionFormStd.Style,
					ImpProductionFormStd.Strandptn,
                    ImpProductionFormStd.StrandType,
                    ImpProductionFormStd.FormType,

					ImpModelPlanning.ElementId,
					ImpModelGeometry.ElementMark,
					ImpModelPlanning.ErectionSequenceNo,
					ImpModelPlanning.TransportId,

					ImpModelPlanning.BedSequenceNo,
					ImpModelPlanning.BedX,
					ImpModelPlanning.BedY,
					ImpModelPlanning.BedZ,
					ImpModelPlanning.BedRotation,
                    ImpModelPlanning.ElementIdStatus,
                    ImpModelPlanning.ProductionDate,
                    ImpModelPlanning.PlannedProductionDate,

					ImpElement.ElementLength,
					ImpElement.ElementWidth,
					ImpElement.ElementHeight,
					ImpElement.ElementType,
					ImpElement.GrossArea,
					ImpElement.Product,
					ImpElement.Project,
					ImpElement.AngleLeftSide,
					ImpElement.AngleRightSide,

					ImpElement.Style,
					ImpElement.Strandptn,
					ImpElement.NbrOfStrands,
                    ImpElement.Volume,
                },

                From = { ImpProductionFormStd.As( "FRM" ) },

                Join =
				{
					Join.Left( ImpProductionCast.As( "CST" ),	
						ImpProductionCast.Factory.Equal( ImpProductionFormStd.Factory ),
						ImpProductionCast.Project.Equal( ImpProductionFormStd.Project ),//Factory, Factory
						ImpProductionCast.Form.Equal( ImpProductionFormStd.Name ) ),

					Join.Left( ImpProductionCastStrand.As( "STA" ),	
						ImpProductionCast.Factory.Equal( ImpProductionCastStrand.Factory ),
						ImpProductionCast.Project.Equal( ImpProductionCastStrand.Project ),// Factory, Factory for productionCast & ProductionCastStrand
						ImpProductionCast.CastId.Equal( ImpProductionCastStrand.CastId ) ),


					Join.Left( ImpModelPlanning.As( "MPL" ),	
						ImpProductionCast.Factory.Equal( ImpModelPlanning.Factory ),
						ImpProductionCast.Project.Equal( ImpModelPlanning.Factory ),// Factory, Factory for productionCast & ProductionCastStrand
						ImpProductionCast.CastId.Equal( ImpModelPlanning.CastId ) ),


					Join.Left( ImpModelGeometry.As( "MGO" ),	
						ImpModelPlanning.Factory.Equal( ImpModelGeometry.Factory ),
						ImpModelPlanning.Project.Equal( ImpModelGeometry.Project ),
						ImpModelPlanning.ElementId.Equal( ImpModelGeometry.ElementId ) ),

					Join.Left( ImpElement.As( "ELM" ),	
						ImpModelGeometry.Factory.Equal( ImpElement.Factory ),
						ImpModelGeometry.Project.Equal( ImpElement.Project ),
						ImpModelGeometry.ElementMark.Equal( ImpElement.ElementMark ) ),
				},

                Where = {
							ImpProductionCast.Factory.Equal( filter.Factory ), 
							ImpProductionCast.Project.Equal( filter.Factory ), // Factory, Factory for productionCast & ProductionCastStrand
							ImpProductionCast.StartDate.Equal( filter.StartDateFrom ), 
                            ImpProductionCast.Shift.Equal( filter.Shift ), 
						   // ImpModelPlanning.CastId.GreaterThan( 0 ),      // Get even the casts without elements!
						},

                GroupBy = 
                {
					ImpProductionCast.CastId,
					ImpProductionCast.Description,
					ImpProductionCast.StartDate,
					ImpProductionCast.EndDate,
					ImpProductionCast.Tolerance,
					ImpProductionCast.ElementType,
                    ImpProductionCast.Style,
					ImpProductionCast.Strandptn,
                    ImpProductionCast.CastType,
					ImpProductionCast.CastStatus,
					ImpProductionCast.Form,
					ImpProductionCast.Shift,

					ImpProductionFormStd.Name,
					ImpProductionFormStd.MaxLength,
					ImpProductionFormStd.MaxWidth,
					ImpProductionFormStd.MaxHeight,
                    ImpProductionFormStd.ElementType,
                    ImpProductionFormStd.Style,
					ImpProductionFormStd.Strandptn,
                    ImpProductionFormStd.StrandType,
                    ImpProductionFormStd.FormType,

					ImpModelPlanning.ElementId,
					ImpModelGeometry.ElementMark,
					ImpModelPlanning.ErectionSequenceNo,
					ImpModelPlanning.TransportId,

					ImpModelPlanning.BedSequenceNo,
					ImpModelPlanning.BedX,
					ImpModelPlanning.BedY,
					ImpModelPlanning.BedZ,
					ImpModelPlanning.BedRotation,
                    ImpModelPlanning.ElementIdStatus,
                    ImpModelPlanning.ProductionDate,
                    ImpModelPlanning.PlannedProductionDate,

					ImpElement.ElementLength,
					ImpElement.ElementWidth,
					ImpElement.ElementHeight,
					ImpElement.ElementType,
					ImpElement.GrossArea,
					ImpElement.Product,
					ImpElement.Project,
					ImpElement.AngleLeftSide,
					ImpElement.AngleRightSide,

					ImpElement.Style,
					ImpElement.Strandptn,
					ImpElement.NbrOfStrands,
                    ImpElement.Volume,
                },

                OrderBy = 
				{ 
					{ ImpElement.Project },
					{ ImpProductionFormStd.Name },
					{ ImpProductionCast.CastId },
					{ ImpModelPlanning.BedSequenceNo, OrderBy.Ascending }
				},
            };

            if( filter.FormType != FormType.All )
            {
                query.Where.Add( ImpProductionCast.CastType.Equal( filter.FormType ) );
            }

            if( !string.IsNullOrWhiteSpace( filter.Location ) && !filter.Location.Equals( Filter.All ) )
            {
                query.Where.Add( ImpProductionFormStd.Location.Equal( filter.Location ) );
            }
            if( !filter.Bed.Equals( Filter.All ) )
            {
                query.Where.Add( ImpProductionCast.Form.Equal( filter.Bed ) );
            }

            // used when sorting elements on bed
            if( form != null && !string.IsNullOrWhiteSpace( form.Name ) )
            {
                query.Where.Add( ImpProductionFormStd.Name.Equal( form.Name ) );
            }

            var statement = query.ToString();

            var tmList = new List<RecProductionFormStd>();
            var parser = new FormParser( tmList );

            using( var database = new ImpactDatabase() )
            {
                var list = database.GetAll( statement, ParseCast );

                foreach( var item in list )
                {
                    item.Form.Factory = filter.Factory;
                    item.Cast.Factory = filter.Factory;

                    item.Form.Project = filter.Project; // What does that really mean!
                    item.Cast.Project = filter.Project; // What does that really mean!

                    if( null != item.Element )
                    {
                        item.Element.Factory = filter.Factory;
                        // Never set element.project to curFilter.project
                        // since we retrieve elmenets from different projects!
                    }

                    parser.Parse( item.Form, item.Cast, item.Element );
                }
            }

            var formSvc = new ProjectManager();
            var filter2 = new BedFilter( filter ) { Location = "" };
            var productionFormStdData = formSvc.LoadProductionFormStd( filter2 );

            // Load strands
            LoadCastStrands( tmList, filter );
            LoadFormStrands( productionFormStdData.Forms, filter );

            return new CastData( tmList, productionFormStdData );
        }

        /// <summary>
        /// LoadFormStrands
        /// </summary>
        /// <param name="forms"></param>
        /// <param name="filter"></param>
        private void LoadFormStrands( IEnumerable<RecProductionFormStd> forms, Filter filter )
        {
            if( null == forms )
            {
                return;
            }
            foreach( var recProductionFormStd in forms )
            {
                if( recProductionFormStd.FormType == FormType.Bed && recProductionFormStd.StrandType == StrandType.Bed )
                {
                    // Load for strands
                    var stdSvc = new ProjectManager();
                    var recStrand = new RecProductionFormStrandStd { Factory = filter.Factory, Project = filter.Project, Name = recProductionFormStd.Name };
                    var strands = stdSvc.LoadProductionFormStrandStd( recStrand );
                    if( null != strands )
                    {
                        strands = (from o in strands where o.IsUsed select o).ToList();
                    }
                    recProductionFormStd.Strands = strands;
                }
                else if( recProductionFormStd.FormType == FormType.Bed && ( recProductionFormStd.ElementType.Equals( "HD/F" ) || recProductionFormStd.ElementType.Equals( "D/F" ) ) )
                {
                    recProductionFormStd.Strands = LoadHollowcoreStrands( recProductionFormStd, filter);
                }
            }

        }

        /// <summary>
        /// LoadHollowcoreStrands
        /// </summary>
        /// <param name="form"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private List<RecProductionFormStrandStd> LoadHollowcoreStrands( RecProductionFormStd form, Filter filter )
        {

            if( string.IsNullOrEmpty( form.Style ) || string.IsNullOrEmpty( form.Strandptn ) )
            {
                return null;
            }
            var strandSvc = new ProjectManager();
            var strands = strandSvc.LoadStrands(
                            filter.Factory,
                            filter.Project,
                            form.ElementType,
                            form.Style,
                            form.Strandptn );
            if( null != strands && strands.Count > 0 )
            {
                var list = new List<RecProductionFormStrandStd>();
                foreach( var strand in strands)
                {
                    var strandStd = new RecProductionFormStrandStd 
                    { 
                        Factory = strand.Factory, 
                        Project = strand.Project, 
                        StrandPos = strand.StrandPos, 
                        StrandX = strand.StrandX, 
                        StrandY = strand.StrandY, 
                        StrandQuality = strand.StrandQuality,
                        StrandDimension = strand.StrandDimension,
                        StrandPrestressing = strand.StrandPrestressing,
                    };

                    list.Add( strandStd );
                }

                return list;
            }

            return null;
        }

        /// <summary>
        /// LoadStrands
        /// </summary>
        /// <param name="forms"></param>
        /// <param name="filter"></param>
        private void LoadCastStrands( IEnumerable<RecProductionFormStd> forms, Filter filter )
        {
            foreach( var recProductionFormStd in forms )
            {
                var rec = new RecProductionCastStrand()
                              {
                                  Factory = filter.Factory,
                                  Project = filter.Project,
                                  CastId = recProductionFormStd.ProductionCast.CastId
                              };
                if( null != recProductionFormStd.ProductionCast )
                {
                    if( recProductionFormStd.FormType == FormType.Bed && recProductionFormStd.StrandType == StrandType.Bed )
                    {
                        // Load cast strands
                        var svc = new ProjectManager();
                        recProductionFormStd.ProductionCast.Strands = svc.LoadProductionCastStrand( rec );
                    }
                    var et = recProductionFormStd.ProductionCast.ElementType;
                    var hasStrands = et.Equals( "HD" ) || et.Equals( "HD/F" );

                    if( hasStrands )
                    {
                        var strandSvc = new ProjectManager();
                        recProductionFormStd.ProductionCast.Strands = strandSvc.LoadStrands(
                            filter.Factory,
                            filter.Project,
                            recProductionFormStd.ProductionCast.ElementType,
                            recProductionFormStd.ProductionCast.Style,
                            recProductionFormStd.ProductionCast.Strandptn );
                    }
                }
            }
        }

        /// <summary>
        /// Parsing helper
        /// </summary>
        /// <param name="column">column</param>
        /// <returns></returns>
        private static DataHolder ParseCast( DbDataReader column )
        {
            var cast = new RecProductionCast()
            {
                CastId = DataConverter.Cast<int>( column[0] ),

                Description = DataConverter.Cast<string>( column[1] ),
                StartDate = DataConverter.Cast<DateTime>( column[2] ),
                EndDate = DataConverter.Cast<DateTime?>( column[3] ),
                Tolerance = DataConverter.Cast<double>( column[4] ),
                ElementType = DataConverter.Cast<string>( column[5] ),
                Style = DataConverter.Cast<string>( column[6] ),
                Strandptn = DataConverter.Cast<string>( column[7] ),
                CastType = DataConverter.Cast<int>( column[8] ),

                CastStatus = DataConverter.Cast<int>( column[9] ),

                Form = DataConverter.Cast<string>( column[10] ),
                Shift = DataConverter.Cast<int>( column[11] ),

                NumOfStrands = DataConverter.Cast<int>( column[12] ),
            };
            var form = new RecProductionFormStd()
            {
                Name = DataConverter.Cast<string>( column[13] ),
                MaxLength = DataConverter.Cast<double?>( column[14] ) ?? 0d,
                MaxWidth = DataConverter.Cast<double?>( column[15] ) ?? 0d,
                MaxHeight = DataConverter.Cast<double?>( column[16] ) ?? 0d,
                ElementType = DataConverter.Cast<string>( column[17] ),
                Style = DataConverter.Cast<string>( column[18] ),
                Strandptn = DataConverter.Cast<string>( column[19] ),
                StrandType = DataConverter.Cast<int>( column[20] ),
                FormType = DataConverter.Cast<int>( column[21] ), 
            };
            var elementId = DataConverter.Cast<int?>( column[22] ) ?? 0;
            RecTMElement elem = null;
            if( 0 != elementId )
            {
                elem = new RecTMElement()
                {
                    ElementId = elementId,
                    ElementMark = DataConverter.Cast<string>( column[23] ),
                    ErectionSequenceNo = DataConverter.Cast<int?>( column[24] ) ?? 0,
                    TransportId = DataConverter.Cast<int?>( column[25] ) ?? 0,

                    BedSequenceNo = DataConverter.Cast<int>( column[26] ),
                    BedX = DataConverter.Cast<double>( column[27] ),
                    BedY = DataConverter.Cast<double>( column[28] ),
                    BedZ = DataConverter.Cast<double>( column[29] ),
                    BedRotation = DataConverter.Cast<double>( column[30] ),
                    ElementIdStatus = DataConverter.Cast<int>( column[31] ),
                    ProductionDate = column[32].Cast<DateTime?>(),
                    PlannedProductionDate = column[33].Cast<DateTime?>(),

                    ElementLength = DataConverter.Cast<double?>( column[34] ) ?? 0d,
                    ElementWidth = DataConverter.Cast<double?>( column[35] ) ?? 0d,
                    ElementHeight = DataConverter.Cast<double?>( column[36] ) ?? 0d,
                    ElementType = DataConverter.Cast<string>( column[37] ),
                    GrossArea = DataConverter.Cast<double?>( column[38] ) ?? 0d,
                    Product = DataConverter.Cast<string>( column[39] ),
                    Project = DataConverter.Cast<string>( column[40] ),
                    AngleLeft = DataConverter.Cast<double?>( column[41] ) ?? 0d,
                    AngleRight = DataConverter.Cast<double?>( column[42] ) ?? 0d,

                    Style = DataConverter.Cast<string>( column[43] ),
                    Strandptn = DataConverter.Cast<string>( column[44] ),
                    NbrOfStrands = DataConverter.Cast<int>( column[45] ),
                    Volume = DataConverter.Cast<double?>( column[46] ) ?? 0d,
                };
            }
            return new DataHolder( cast, form, elem );
        }

        /// <summary>
        /// Form parser
        /// </summary>
        class FormParser
        {
            Dictionary<string, RecProductionFormStd> dic;

            private List<RecProductionFormStd> tmList = null;
            public FormParser( List<RecProductionFormStd> lst )
            {
                tmList = lst;
                dic = new Dictionary<string, RecProductionFormStd>();
            }

            public void Parse( RecProductionFormStd form, RecProductionCast cast, RecTMElement elem )
            {
                string formKey = form.Factory + form.Project + form.Name;
                RecProductionFormStd formObj = null;
                if( dic.ContainsKey( formKey ) )
                {
                    formObj = dic[formKey];
                }
                else
                {
                    formObj = new RecProductionFormStd( form );
                    dic.Add( formKey, formObj );
                    tmList.Add( formObj );
                }

                RecProductionCast castObj = null;
                if( cast != null )
                {
                    castObj = formObj.FindCast( cast );
                    if( castObj == null )
                    {
                        castObj = new RecProductionCast( cast ) { Form = form.Name };
                        formObj.ProductionCast = castObj;
                    }
                }
                // Save the element if we do have an element
                if( null == elem || elem.ElementId <= 0 )
                {
                    return;
                }
                if( castObj == null || castObj.CastId <= 0 )
                {
                    return;
                }
                elem.CastId = castObj.CastId;
                castObj.Project = elem.Project;
                castObj.AddElement( elem );
            }
        }

        /// <summary>
        /// Cast parser
        /// </summary>
        class CastParser
        {
            Dictionary<string, RecProductionCast> dic;
            List<RecProductionCast> tmList = null;
            public CastParser( List<RecProductionCast> lst )
            {
                tmList = lst;
                dic = new Dictionary<string, RecProductionCast>();
            }
            public List<RecProductionCast> GetTransportList()
            {
                return tmList;
            }
            public void Parse( RecProductionFormStd form, RecProductionCast cast, RecTMElement elem )
            {
                string castKey = cast.Factory + cast.Project + cast.CastId;
                RecProductionCast castObj = null;
                if( dic.ContainsKey( castKey ) )
                {
                    castObj = dic[castKey];
                }
                else
                {
                    castObj = new RecProductionCast( cast );
                    dic.Add( castKey, castObj );
                    tmList.Add( castObj );
                }

                // Save the element if we do have an element
                if( elem != null && elem.ElementId > 0 )
                {
                    if( castObj != null && castObj.CastId > 0 )
                    {
                        elem.CastId = castObj.CastId;
                        castObj.Project = elem.Project;
                        castObj.AddElement( elem );
                    }
                }
            }
        }
        /// <summary>
        /// Local Data holder
        /// </summary>
        public class DataHolder
        {
            public RecProductionCast Cast;
            public RecProductionFormStd Form;
            public RecTMElement Element;
            public DataHolder() { }
            public DataHolder( RecProductionCast cast, RecProductionFormStd form, RecTMElement element )
            {
                this.Cast = cast;
                this.Form = form;
                this.Element = element;
            }
        }

    }
}
