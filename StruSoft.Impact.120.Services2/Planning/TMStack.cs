using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StruSoft.Impact.V120.Planning.Common;
using System.ServiceModel;

namespace StruSoft.Impact.V120.Services
{
	public class TMStack : RecTMStack
	{
		bool IsFull { get; set; }
		public TMStack( RecTransportVehicleStackStd stack )
		{
			TransportVehicleStackStdObject = stack;
		}
		public bool Load( RecTMElement elem, RecTransport recTransport, RecTransportVehicleStd recVeh )
		{
			// *** Force erection sequence since it is paramount ***
			// Once a vehicle is set till full, never try to put other lighter elements even though
			// the total weight still allowed
			if( IsFull )
			{
				return false;
			}
			// It is not allowed to use a vehicle not capable to carry a single element!
			if( elem.Mass > TransportVehicleStackStdObject.MaxMass )
			{
				string elemMass = elem.Mass.ToString( "#0.000" );
				string stackMaxMass = TransportVehicleStackStdObject.MaxMass.ToString( "#0.000" );
				string msg = string.Format( "The stack \"{0}\" max load capacity {1} < element mass {2} for element \"{3}\", Standard vehicle \"{4}\" !",
					TransportVehicleStackStdObject.Description,
					stackMaxMass,
					elemMass,
					elem.ElementId,
					TransportVehicleStackStdObject.Name );
				throw new FaultException<BusinessFaultContract>( new BusinessFaultContract() { Argument = msg }, "Stack load capacity < Element mass!" );
			}
			double tot = GetTotElementMass() + elem.Mass;
			if( tot > TransportVehicleStackStdObject.MaxMass )
			{
				IsFull = true;
				return false;
			}
			else
			{
				TMElements.Add( new TMElement( elem ) );
			}
			return true;
		}
		public int GetNumOfElement()
		{
			return TMElements.Count;
		}

		/// <summary>
		/// Saves the transport into the database
		/// </summary>
		private void SaveTransportVehicleStack( RecTransport transport, RecTransportVehicleStd recVeh, int seq )
		{
			// Prepare data
			TransportVehicleStackStdObject.Factory = transport.Factory;
			TransportVehicleStackStdObject.Project = transport.Project;
			TransportVehicleStackStdObject.TransportId = transport.TransportId;
			TransportVehicleStackStdObject.VehicleId = recVeh.VehicleId;

			// Save Stack object
			ProjectManager svc = new ProjectManager();
			// Use the new stack id
			TransportVehicleStackStdObject.StackId = svc.InsertTransportVehicleStack( TransportVehicleStackStdObject );
		}
		void SimpleSave( RecTransport transport, RecTransportVehicleStd recVeh )
		{
			if( TMElements == null || TMElements.Count == 0 )
			{
				return;
			}
			// Update the elements with transport info;
			int loadSeq = TMElements.Count;
			foreach( TMElement tmElement in TMElements )
			{
				tmElement.StackSequenceNo = loadSeq;
				tmElement.StackId = TransportVehicleStackStdObject.StackId;
				tmElement.VehicleId = recVeh.VehicleId;

				tmElement.Factory = transport.Factory;
				tmElement.Project = transport.Project;
				tmElement.TransportId = transport.TransportId;

				tmElement.Save();
				loadSeq--;
			}
		}

		public void Save( RecTransport transport, RecTransportVehicleStd recVeh, int seq )
		{
			//Rack transport Mode, saves vehicle objects and elements on them
			//TransportVehicleStackStdObject.Save(_transport);


			//// Save the stack object;
			SaveTransportVehicleStack( transport, recVeh, seq );

			// Save elements
			SimpleSave( transport, recVeh );

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
	}
}