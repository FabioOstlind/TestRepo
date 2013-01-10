using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using StruSoft.Impact.V120.Planning.Common;

namespace StruSoft.Impact.V120.Services
{
	public class TMTransport : RecTMTransport
	{
		public bool IsFull { set; get; }

		public TMTransport() { }
		public TMTransport( RecTransport transport )
		{
			IsFull = false;
			TransportObject = transport;
		}
		private DateTime GetNextDate( DateTime dt, int interval, int count )
		{
			int hours = 0;
			int minutes = interval * count;
			int seconds = 0;
			return dt + new TimeSpan( hours, minutes, seconds );
		}
		public void CreateFromTemplate( RecTransport transportTpl, int templateUsageCount, PlanningFilter filter )
		{
			TransportObject = new RecTransport( transportTpl );
			// Calculate the load date and delivery date based on
			// the template and the template reuse loop(count)
			// The interval is minutes
			TransportObject.LoadDate = GetNextDate( filter.LoadDateFrom, (int)filter.TimeInterval, templateUsageCount );
			TransportObject.DeliveryDate = GetNextDate( filter.DeliveryDateFrom, (int)filter.TimeInterval, templateUsageCount );
			TransportObject.IsTemplate = 0;
			foreach( RecTransportVehicleStd rec in transportTpl.Vehicles )
			{
				TMVehicle tmVehicle = new TMVehicle( rec );
				//Create Stacks from template
				tmVehicle.CreateFromTemplate( rec );

				Vehicles.Add( tmVehicle );
			}
		}
		private bool IsSimpleMode()
		{
			return ( Vehicles.Count == 0 );
		}
		//private bool SimpleLoad(RecTMElement elem)
		//{
		//  if (Vehicles.Count == 0 && elem.Mass > TransportObject.MaxMass)
		//  {
		//    string msg = string.Format("Transport {0} max load capacity {1} < element mass {2} for element{3}!", TransportObject.Name, TransportObject.MaxMass, elem.Mass, elem.Element_id);
		//    throw new FaultException<BusinessFaultContract>(new BusinessFaultContract() { Argument = msg }, "Transport load capacity < Element mass!");
		//  }
		//  double tot = GetTotElementMass() + elem.Mass;
		//  if (tot > TransportObject.MaxMass)
		//  {
		//    IsFull = true;
		//    return false;
		//  }
		//  else
		//  {
		//    TMElements.Add(new TMElement(elem));
		//  }
		//  return true;
		//}
		public bool Load( RecTMElement elem )
		{
			// Vehicle transport Mode
			foreach( TMVehicle tmVehicle in Vehicles )
			{
				// Try to load this element on the current vehicle
				if( tmVehicle.Load( elem, TransportObject ) )
				{
					return true;
				}
			}
			// OK, All the vehicles are already fully loaded
			IsFull = true;
			// return false, to indicate that.
			return false;
		}
		private double GetTotElementMass()
		{
			double tot = 0;
			foreach( TMElement tmElement in TMElements )
			{
				tot += tmElement.Mass;
			}
			return tot;
		}
		public int GetNumOfElement()
		{
			int noOfElem = 0;
			if( IsSimpleMode() )
			{
				return TMElements.Count;
			}
			// Vehicle transport Mode
			foreach( TMVehicle tmVehicle in Vehicles )
			{
				noOfElem += tmVehicle.GetNumOfElement();
			}
			return noOfElem;
		}

		/// <summary>
		/// Saves the transport into the database
		/// </summary>
		private bool SaveTransport( bool createEmptyTransport )
		{
			// If there no elements then dont save the object to the db
			if( !createEmptyTransport && GetNumOfElement() == 0 )
			{
				return false;
			}

			ProjectManager svc = new ProjectManager();
			RecTransport newRecTransport = svc.InsertTransport( TransportObject );
			// Use the new transport id
			TransportObject.TransportId = newRecTransport.TransportId;
			return true;
		}
		void SimpleSave()
		{
			// Update the elements with transport info;
			int loadSeq = 1;
			foreach( TMElement tmElement in TMElements )
			{
				tmElement.StackSequenceNo = loadSeq;
				tmElement.VehicleId = 0; // So far!!!!

				tmElement.Factory = TransportObject.Factory;
				tmElement.Project = TransportObject.Project;
				tmElement.TransportId = TransportObject.TransportId;

				tmElement.Save();
				loadSeq++;
			}
		}
		/// <summary>
		/// Saves the transport and updates the element in the database
		/// </summary>
		public void Save( bool createEmptyTransport )
		{
			// Save the transport object;
			if( SaveTransport( createEmptyTransport ) )
			{
				//So these seem to elements on the trasport and the transport
				//is already saved so let's save all the related vehicle and elements

				// Now it is time to update the element planning info
				if( IsSimpleMode() )
				{
					// Simple transport Mode, elements are found in the currtent transp obj
					SimpleSave();
				}
				else
				{
					// Vehicle transport Mode, saves vehicle objects and elements on them
					int seq = 1;
					foreach( TMVehicle vehicle in Vehicles )
					{
						vehicle.Save( TransportObject, seq );
						seq++;
					}
				}
			}
		}
	}
}