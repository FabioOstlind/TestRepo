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
using StruSoft.Impact.V120.DB;
using StruSoft.Impact.V120.DB.Query;

namespace StruSoft.Impact.V120.Services.Planning
{
	public class ExBedProductionContainer
	{
		private BedFilter _filter;
		private List<ExBed> _beds = new List<ExBed>();
		public ExBedProductionContainer() { }

        public int LoadPositionedElementData( List<RecProductionFormStd> forms, BedFilter filter )
        {
            _filter = filter;
            if( forms == null || forms.Count == 0 )
            {
                string msg = string.Format( "No forms defined!" );
                throw new FaultException<BusinessFaultContract>( new BusinessFaultContract() { Argument = msg }, "Cannot manage form!" );
            }

            foreach( RecProductionFormStd f in forms )
            {
                _beds.Add( new ExBed( f, filter ) );
            }

            // Load existing elements
            BedManager mgr = new BedManager();
            CastData castData = mgr.LoadBedPlanning( filter, null );
            List<RecProductionFormStd> existingData = castData.CastList;
            foreach( ExBed bed in _beds )
            {
                bed.AddExistingData( existingData );
            }
            return 0;
        }

		private ExBed GetBed( RecProductionFormStd form)
		{
			// Search for available Bed
			foreach( ExBed curBed in _beds )
			{
				if( curBed.Name.Equals(form.Name) )
				{
					return curBed;
				}
			}
			return null;
		}


		public List<RecTMElement> PositionedElementsOnBed( RecProductionFormStd bed, List<RecTMElement> elements )
		{
            ExBed exBed = GetBed( bed );
            if( null == exBed )
            {
                return null;
            }

            List<RecTMElement> result = exBed.FillBed( elements );
            if( null != result && result.Count > 0 )
            {
                exBed.Save( result );
            }

			return result;
		}

		public int Sort( List<RecTMElement> elements, RecProductionFormStd form, BedFilter filter )
		{
			_filter = filter;
			if( form == null )
			{
				string msg = string.Format( "No forms defined!" );
				throw new FaultException<BusinessFaultContract>( new BusinessFaultContract() { Argument = msg }, "Cannot manage form!" );
			}

            ExBed bed = new ExBed( form, filter );

			List<RecTMElement> result = bed.FillBed( elements );
			return bed.Save( result );
		}
	}

}