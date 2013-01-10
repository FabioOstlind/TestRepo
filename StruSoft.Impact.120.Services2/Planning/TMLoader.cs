using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data.Common;
using System.ServiceModel.Activation;
using StruSoft.Impact.V120.Planning.Common;
using System.Diagnostics;

namespace StruSoft.Impact.V120.Services
{
	public class TMLoader
	{
		private PlanningFilter _filter;
		private int templateUsageCount = 0;
		private List<TMTransport> _tmTransports = new List<TMTransport>();
		public TMLoader() { }
		public int Load( List<RecTMElement> elements, List<RecTransport> templates, PlanningFilter filter, bool generateErectionSequence )
		{
			_filter = filter;
			if( templates == null || templates.Count == 0 )
			{
				string msg = string.Format( "No transport templates defined!" );
				throw new FaultException<BusinessFaultContract>( new BusinessFaultContract() { Argument = msg }, "Cannot manage transport!" );
			}

			// Put elements on the transportation devices
			int erectionSequence = 1;
			foreach( RecTMElement elem in elements )
			{
				if( generateErectionSequence )
				{
					elem.ErectionSequenceNo = erectionSequence;
					erectionSequence++;
				}
				if( !ProcessLoad( elem, templates ) )
				{
					break;// stop loading elements if ProcessLoad returns false which occurs when filter.MultipleTemplateUsage = false
				}
			}

			// Finally save the data to the database
			return SaveToDb( _tmTransports, false );
		}
		/// <summary>
		/// Create a chunk of transport as a copy of the chunk of transport templates
		/// only in memory.
		/// </summary>
		/// <param name="templates"></param>
		public void CreateTransportFromTemplate( PlanningFilter filter, List<RecTransport> templates, int templateUsageCount, List<TMTransport> transportList )
		{
			foreach( RecTransport transport in templates )
			{
				TMTransport tmTransport = new TMTransport();
				tmTransport.CreateFromTemplate( transport, templateUsageCount, filter );
				transportList.Add( tmTransport );
			}
		}

		/// <summary>
		/// Saves data into databse
		/// </summary>
		/// <param name="createEmptyTransport"></param>
		public int SaveToDb( List<TMTransport> transportList, bool createEmptyTransport )
		{
			int elementCount = 0;
			foreach( TMTransport tmTransport in transportList )
			{
				tmTransport.Save( createEmptyTransport );
				elementCount += tmTransport.GetNumOfElement();
				//Console.WriteLine("TransportId: " + tmTransport.TransportObject.TransportId);
				//Trace.WriteLine("TransportId: " + tmTransport.TransportObject.TransportId);
			}
			return elementCount; 
		}

		private bool ProcessLoad( RecTMElement elem, List<RecTransport> templates )
		{
			TMTransport tmTransport = GetTMTransport( templates );
			if( tmTransport == null )
			{
				return false;
			}
			if( !tmTransport.Load( elem ) )
			{
				// OK that transport device was full, let's try the next transport
				// Recursive call!
				if( !ProcessLoad( elem, templates ) )
				{
					return false;
				}
			}
			return true;
		}

		private TMTransport GetTMTransport( List<RecTransport> templates )
		{
			// Search for available TMTransport
			foreach( TMTransport tmTransport in _tmTransports )
			{
				if( !tmTransport.IsFull )
				{
					return tmTransport;
				}
			}
			// Stop creating transports from templates after
			// the first round if _filter.MultipleTemplateUsage = false
			if( !_filter.MultipleTemplateUsage && templateUsageCount >= 1 )
			{
				return null;
			}
			// OK, then we have no available transport
			// Let's create a chunk of transports matching our templates
			CreateTransportFromTemplate( _filter, templates, templateUsageCount, _tmTransports );
			templateUsageCount++;
			//Try again using Recursive call!
			return GetTMTransport( templates );
		}
	}
}