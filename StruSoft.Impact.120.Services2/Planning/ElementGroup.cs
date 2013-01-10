using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StruSoft.Impact.V120.Planning.Common;

namespace StruSoft.Impact.V120.Services.Planning
{
	public class ElementGroup
	{
		public const double TolenranceY = 100;//mm
		private List<RecTMElement> Elements { get; set; }
		public ElementGroup()
		{
			Elements = new List<RecTMElement>();
		}
		public int Count
		{
			get
			{
				return Elements.Count;
			}
		}

		public double GetWidth()
		{
			double width = 0;
			foreach( RecTMElement elem in Elements )
			{
				width += elem.ElementWidthOnBed;
			}

			width += ( Elements.Count - 1 ) * TolenranceY;

			return width;
		}

		public double GetLength()
		{
			double length = 0;
			foreach( RecTMElement elem in Elements )
			{
				length = Math.Max( length, elem.ElementLength );
			}

			return length;
		}

		public bool CanAdd( ExBed bed, RecTMElement element)
		{
			double maxWidth = bed.MaxWidth;
			double currWidth = GetWidth() + TolenranceY + element.ElementWidthOnBed;
			if( maxWidth >= currWidth )
			{
				return true;
			}

			return false;
		}

		public bool Add( ExBed bed, RecTMElement element)
		{
			double maxWidth = bed.MaxWidth;
			double currWidth = GetWidth() + TolenranceY + element.ElementWidthOnBed;
			if( maxWidth >= currWidth )
			{
				Elements.Add( element );
				return true;
			}

			return false;
		}
	}
}