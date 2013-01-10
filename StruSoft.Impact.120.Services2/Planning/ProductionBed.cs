using System;
using System.Collections.Generic;
using System.Linq;
using StruSoft.Impact.V120.Planning.Common;
using StruSoft.Impact.V120.DB.Query;

namespace StruSoft.Impact.V120.Services
{
    using System.Data.Common;

    using StruSoft.Impact.V120.DB;

    public class ProductionBed : RecProductionFormStd
    {
        private ConvexPolygon lastExistingPolygon = null;
        public bool IsFull { set; get; }

        readonly BedFilter bedFilter;

        public ProductionBed( RecProductionFormStd form, BedFilter filter )
            : base( form )
        {
            this.bedFilter = filter;
            IsFull = false;
            if( null != form.ProductionCast && form.ProductionCast.CastId > 0 )
            {
                // Well this is never used so far
                this.ProductionCast = form.ProductionCast;
            }
            else
            {
                // Create a cast object
                var cast = new RecProductionCast( form )
                               {
                                   Factory = filter.Factory,
                                   Project = filter.Project,
                                   StartDate = filter.StartDateFrom,
                                   EndDate = filter.EndDateFrom,
                                   Shift = filter.Shift
                               };

                if( form.FormType == V120.Planning.Common.FormType.Bed && form.StrandType == V120.Planning.Common.StrandType.Bed )
                {
                    var strand = new RecProductionFormStrandStd()
                                     { Factory = filter.Factory, Project = filter.Project, Name = form.Name };
                    cast.Strands = LoadStandardStrands( strand );
                }

                this.ProductionCast = cast;
            }
        }

        /// <summary>
        /// Load standard strands into strand list
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public List<RecProductionCastStrand> LoadStandardStrands( RecProductionFormStrandStd record )
        {
            var strands = new List<RecProductionCastStrand>();
            var svc = new ProjectManager();
            var list = svc.LoadProductionFormStrandStd( record );
            list = ( from o in list where o.IsUsed select o ).ToList();
            foreach( var std in list )
            {
                var strand = new RecProductionCastStrand()
                    {
                        Factory = record.Factory,
                        Project = record.Project,
                        StrandPos = std.StrandPos,
                        CastId = 0, // Since it is not created yet
                    };
                strands.Add( strand );
            }
            return strands;
        }
        
        public List<RecProductionCastStrand> LoadCastStrands( RecProductionCastStrand record )
        {
            var svc = new ProjectManager();
            return svc.LoadProductionCastStrand( record );
        }

        public void AssignProductionCast( List<RecProductionFormStd> casts )
        {
            foreach( RecProductionFormStd formCast in casts )
            {
                if( null != formCast.ProductionCast && !string.IsNullOrWhiteSpace( formCast.Name ) && formCast.Name.Equals( this.Name ) )
                {
                    // Assign the ProductionCast
                    this.ProductionCast = formCast.ProductionCast;

                    // Now handle special case
                    if( this.FormType == (int)V120.Planning.Common.FormType.Bed && this.StrandType == V120.Planning.Common.StrandType.Bed )
                    {
                        var strand = new RecProductionCastStrand()
                                         { Factory = this.bedFilter.Factory, Project = this.bedFilter.Project, CastId = formCast.ProductionCast.CastId };
                        this.ProductionCast.Strands = LoadCastStrands( strand );
                    }
                }
            }
        }

        public void PrepareLastElement()
        {
            if( null == this.ProductionCast || null == this.ProductionCast.Elements )
            {
                this.lastExistingPolygon = null;
                return;
            }
            this.lastExistingPolygon = this.ProductionCast.LastElement;
        }

        /// <summary>
        /// Has Existing Elements
        /// </summary>
        /// <returns></returns>
        public bool HasExistingElements()
        {
            return this.ProductionCast != null && this.ProductionCast.Elements != null &&
            this.ProductionCast.Elements.Count > 0;
        }

        /// <summary>
        /// Returns forms Production Cast if any otherwise null is returned
        /// </summary>
        /// <returns></returns>
        public RecProductionCast GetProductionCast()
        {
            return this.ProductionCast;
        }

        /// <summary>
        /// Return Existing Elements
        /// </summary>
        /// <returns></returns>
        private List<RecTMElement> GetExistingElements()
        {
            return this.ProductionCast != null ? this.ProductionCast.Elements : null;
        }

        /// <summary>
        /// Loads element details
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static RecTMElement LoadElement( RecTMElement element )
        {
            if( element == null || string.IsNullOrEmpty( element.Factory ) || string.IsNullOrEmpty( element.Project ) )
            {
                return null;
            }

            List<RecTMElement> result;
            var query = new ImpactQuery()
            {
                Select =
				{
					ImpElement.Style,
					ImpElement.Strandptn,
					ImpElement.NbrOfStrands,
				},

                From = { ImpElement.As( "ELM" ) },
                Where = {
							ImpElement.Factory.Equal( element.Factory ), 
							ImpElement.Project.Equal( element.Project ), 
							ImpElement.ElementMark.Equal( element.ElementMark ), 
						},
            };
            var statement = query.ToString();
            using( var database = new ImpactDatabase() )
            {
                result = database.GetAll( statement, ElementParse );
            }

            return (result != null && result.Count > 0)? result[0] : null;
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
                                 Style = column[0].Cast<string>(),
                                 Strandptn = column[1].Cast<string>(),
                                 NbrOfStrands = column[2].Cast<int>(),
                             };

            return record;
        }

        /// <summary>
        /// Create Production Cast
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public RecProductionCast AdjustProductionCastToElement( RecTMElement element )
        {
            var result = LoadElement( element );
            this.ProductionCast.Style = result.Style;
            this.ProductionCast.Strandptn = result.Strandptn;
            this.ProductionCast.NumOfStrands = result.NbrOfStrands;

            return this.ProductionCast;
        }

        /// <summary>
        /// Saves the cast into the database
        /// </summary>
        public RecProductionCast GetCast()
        {
            // If there is a cast object with the same date (Year, Month, Day), shift then use it
            // otherwise create a new one
            var cast = this.FindCast( -1 );
            var svc = new ProjectManager();
            var pcd = svc.LoadProductionCast( this.bedFilter, cast );
            RecProductionCast newCast = null;
            if( pcd != null && pcd.Count > 0 )
            {
                newCast = pcd[0]; // Use the first one
            }
            if( newCast == null )
            {
                newCast = svc.InsertProductionCast( cast );
            }
            if( newCast == null )
            {
                throw new Exception( "Cannot create new cast object!" );
            }
            cast.CastId = newCast.CastId;

            return cast;
        }

        /// <summary>
        /// Saves elements into the database
        /// </summary>
        public int Save( List<RecTMElement> elements )
        {
            var cast = GetCast();
            var bedSequenceNo = 1;
            foreach( var elem in elements )
            {
                elem.CastId = cast.CastId;
                elem.BedSequenceNo = bedSequenceNo;
                //elem.BedZ = 0;

                var svc = new ModelPlanner();
                svc.SaveElementProduction( elem, true );

                bedSequenceNo++;
            }

            return elements.Count;
        }

        /// <summary>
        /// GetXStart
        /// </summary>
        /// <returns></returns>
        double GetXStart()
        {
            double xOffset = 0;
            if( null != this.lastExistingPolygon )
            {
                xOffset = this.lastExistingPolygon.MaxX + GetToleranceX();
            }

            return xOffset;
        }

        /// <summary>
        /// Returns the current cast ToleranceX if any otherwise
        /// returns the forms ToleranceX
        /// </summary>
        /// <returns></returns>
        private double GetToleranceX()
        {
            if( null != this.ProductionCast )
            {
                return this.ProductionCast.Tolerance;
            }

            return this.Tolerance;
        }

        /// <summary>
        /// Return the current filter ToleranceY
        /// </summary>
        /// <returns></returns>
        private double GetToleranceY()
        {
            return this.bedFilter.ToleranceY;
        }

        /// <summary>
        /// Two dimensional bin packing algorithm (2BP) 
        /// Fill in by columns in a single row.
        /// </summary>
        /// <param name="rects"></param>
        public List<RecTMElement> FillBed( List<RecTMElement> rects )
        {
            //rects = GetAllowedElements(rects);
            double bedXStart = GetXStart();
            if( rects == null )
            {
                // If new elements added then just rearrange existing elements
                rects = GetExistingElements();
                bedXStart = 0;
            }

            // Make lists of positioned and not positioned rectangles.
            List<RecTMElement> notPositioned = new List<RecTMElement>();
            List<RecTMElement> positioned = new List<RecTMElement>();
            for( int i = 0; i <= rects.Count - 1; i++ )
            {
                notPositioned.Add( rects[i] );
            }

            // Arrange the rectangles.
            double y = 0;
            double x = bedXStart;
            double rowLength = 0;
            while( notPositioned.Count > 0 )
            {
                // Find the next rectangle that will fit on this column.
                int nextRect = -1;
                for( int i = 0; i <= notPositioned.Count - 1; i++ )
                {
                    if( y + notPositioned[i].ElementWidthOnBed <= MaxWidth )
                    {
                        nextRect = i;
                        break;
                    }
                }

                // If we didn't find a rectangle that fits, start a new column.
                if( nextRect < 0 )
                {
                    x += rowLength + GetToleranceX();
                    y = 0;
                    rowLength = 0;
                    nextRect = 0;
                }

                // Position the selected rectangle.
                RecTMElement rect = notPositioned[nextRect];

                // Now break the loop if there is no enough space on x direction for the next element
                if( x + rect.ElementLengthOnBed > MaxLength )
                {
                    this.IsFull = true;
                    break;
                }

                rect.BedY = y;
                rect.BedX = x;

                // Take care of rotation (we assume 180 degrees)
                if( rect.BedRotation > 0 )
                {
                    rect.BedRotation = 0;
                    RecTMElement temp = RecTMElement.RotateElement( rect );
                    rect.BedX = temp.BedX;
                    rect.BedY = temp.BedY;
                    rect.BedRotation = temp.BedRotation;
                }

                y += rect.ElementWidthOnBed + GetToleranceY();

                if( rowLength < rect.ElementLengthOnBed )
                {
                    rowLength = rect.ElementLengthOnBed;
                }

                // Move the rectangle into the positioned list.
                positioned.Add( rect );
                notPositioned.RemoveAt( nextRect );

                if( this.bedFilter.SingleElementCastUnit && positioned.Count > 0 )
                {
                    break;
                }
            }

            List<RecTMElement> result = new List<RecTMElement>();
            // Prepare the results.
            for( int i = 0; i <= positioned.Count - 1; i++ )
            {
                result.Add( positioned[i] );
            }
            return result;
        }

    }
}