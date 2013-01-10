using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StruSoft.Impact.V120.Planning.Common;
using System.Drawing;

namespace StruSoft.Impact.V120.Services.Planning
{
	public class ExBed : RecProductionFormStd
	{
		public List<ElementGroup> ElementGroups { get; set; }

		private ConvexPolygon LastExistingPolygon = null;
		//public double YTolerance = 100; // 10 com
		public bool IsFull { set; get; }

		BedFilter _bedFilter;

		private ExBed() { }

		public ExBed( RecProductionFormStd form, BedFilter filter )
			: base( form )
		{
			_bedFilter = filter;
			IsFull = false;
			// Create a cast object
			RecProductionCast cast = new RecProductionCast( form );
			cast.Factory = filter.Factory;
			cast.Project = filter.Project;

			cast.StartDate = filter.StartDateFrom;
			cast.EndDate = filter.EndDateFrom;

			cast.Shift = filter.Shift;
			AddCast( cast );
		}

		private List<RecTMElement> GetElements( List<RecProductionFormStd> existingData )
		{
			foreach( RecProductionFormStd std in existingData )
			{
				if( !string.IsNullOrWhiteSpace( std.Name ) && std.Name.Equals( this.Name ) )
				{
					if( std.ProductionCast != null )
					{
						List<RecTMElement> elements = std.ProductionCast.Elements;
						if( elements != null && elements.Count > 0 )
						{
							return elements;
						}
					}
				}
			}

			return null;
		}

		public void AddExistingData( List<RecProductionFormStd> existingData )
		{
			if( existingData == null || existingData.Count == 0 )
			{
				return;
			}
			// We assume that we have cast ONLY on a singe bed on a certain day & shiff
			List<RecTMElement> elements = GetElements( existingData );
			if( elements != null && elements.Count > 0 )
			{
				ConvexPolygonList polygons = new ConvexPolygonList( elements );
				polygons.Sort(delegate(ConvexPolygon p1, ConvexPolygon p2) { return p1.MaxX.CompareTo(p2.MaxX); });
				LastExistingPolygon = polygons[polygons.Count - 1];
			}
		}

		private List<RecTMElement> GetExistingElements()
		{
			if( this.ProductionCast != null )
			{
			    // We assume that there is one cast  only
			    return ProductionCast.Elements;
			}
			return null;
		}

		/// <summary>
		/// Saves the cast into the database
		/// </summary>
		public RecProductionCast GetCast()
		{
			// If there is a cast object with the same date (Year, Month, Day), shift then use it
			// otherwise create a new one
			RecProductionCast cast = this.FindCast( -1 );
			ProductionCast svc = new ProductionCast();
			List<RecProductionCast> pcd = svc.Load( _bedFilter, cast );
			RecProductionCast newCast = null;
			if( pcd != null && pcd.Count > 0 )
			{
				newCast = pcd[0]; // Use the first one
			}
			if( newCast == null )
			{
				newCast = svc.Insert( cast );
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
			RecProductionCast cast = GetCast();
			int bedSequenceNo = 1;
			foreach( RecTMElement elem in elements )
			{
				elem.CastId = cast.CastId;
				elem.BedSequenceNo = bedSequenceNo;
				//elem.BedZ = 0;

				ModelPlanner svc = new ModelPlanner();
				svc.SaveElementProduction( elem, true );

				bedSequenceNo++;
			}

			return elements.Count;
		}

		double GetXStart()
		{
		    double xOffset = 0;
		    if( null != LastExistingPolygon )
		    {
		        xOffset = LastExistingPolygon.MaxX + Tolerance;
		    }

		    return xOffset;
		}

        //List<RecTMElement> GetAllowedElements(List<RecTMElement> elements)
        //{
        //    List<RecTMElement> allowedElements = ( from o in elements
        //                                            where ( o.ElementType == this.ElementType )
        //                                            select o ).ToList();
        //    return allowedElements;
        //}
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
					x += rowLength + Tolerance;
					y = 0;
					rowLength = 0;
					nextRect = 0;
				}

				// Position the selected rectangle.
				RecTMElement rect = notPositioned[nextRect];

				// Now break the loop if there is no enough space on x direction for the next element
				if( x + rect.ElementLength > MaxLength )
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

				y += rect.ElementWidthOnBed + _bedFilter.ToleranceY;

				if( rowLength < rect.ElementLength )
				{
					rowLength = rect.ElementLength;
				}

				// Move the rectangle into the positioned list.
				positioned.Add( rect );
				notPositioned.RemoveAt( nextRect );
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