using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StruSoft.Impact.V120.Planning.Common;
using System.Data.Common;

namespace StruSoft.Impact.V120.Services
{
	public class TMElement : RecTMElement
	{
		public TMElement( RecTMElement element )
			: base( element )
		{
		}
		/// <summary>
		/// Update the element with transport information
		/// Create a new record for the element if it is missing
		/// </summary>
		public void Save()
		{
			ModelPlanner svc = new ModelPlanner();
			svc.SaveElementTransport( this, true );
		}
	}
}