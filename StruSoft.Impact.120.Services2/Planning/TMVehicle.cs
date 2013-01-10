using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using StruSoft.Impact.V120.Planning.Common;
namespace StruSoft.Impact.V120.Services
{
	public class TMVehicle : RecTMVehicle
	{
		bool IsFull { get; set; }
		public TMVehicle( RecTransportVehicleStd recTransportVehicleStd )
		{
			TransportVehicleStdObject = recTransportVehicleStd;
		}
		public void CreateFromTemplate( RecTransportVehicleStd recVehTpl )
		{
			TransportVehicleStdObject = new RecTransportVehicleStd( recVehTpl );
			foreach( RecTransportVehicleStackStd rec in recVehTpl.Stacks )
			{
				TMStack tmStack = new TMStack( rec );

				Stacks.Add( tmStack );
			}
		}

		public bool Load( RecTMElement elem, RecTransport recTransport )
		{
			foreach( TMStack tmStack in Stacks )
			{
				// Try to load this element on the current vehicle
				if( tmStack.Load( elem, recTransport, TransportVehicleStdObject ) )
				{
					return true;
				}
			}
			// OK, All the vehicles are already fully loaded
			IsFull = true;
			// return false, to indicate that.
			return false;
		}
		public int GetNumOfElement()
		{
			int count = 0;
			if( IsSimpleMode() )
			{
				return TMElements.Count;
			}
			else
			{
				foreach( TMStack tmStack in Stacks )
				{
					count += tmStack.GetNumOfElement();
				}
			}
			return count;
		}

		private bool IsSimpleMode()
		{
			return ( Stacks.Count == 0 );
		}
		/// <summary>
		/// Saves the transport into the database
		/// </summary>
		private void SaveTransportVehicle( RecTransport transport, int seq )
		{
			ProjectManager svc = new ProjectManager();
			RecTransportVehicleStd newRec = svc.InsertTransportVehicle( transport, TransportVehicleStdObject, seq );
			// Use the new veh id
			TransportVehicleStdObject.VehicleId = newRec.VehicleId;
		}
		void SimpleSave( RecTransport transport )
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
				tmElement.VehicleId = TransportVehicleStdObject.VehicleId;

				tmElement.Factory = transport.Factory;
				tmElement.Project = transport.Project;
				tmElement.TransportId = transport.TransportId;

				tmElement.Save();
				loadSeq--;
			}
		}

		public void Save( RecTransport transport, int seq )
		{
			// Save the transport object;
			SaveTransportVehicle( transport, seq );

			// Vehicle transport Mode, saves vehicle objects and elements on them
			foreach( TMStack stack in Stacks )
			{
				stack.Save( transport, TransportVehicleStdObject, seq );
				seq++;
			}
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