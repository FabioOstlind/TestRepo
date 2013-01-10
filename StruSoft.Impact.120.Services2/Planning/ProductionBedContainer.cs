namespace StruSoft.Impact.V120.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using StruSoft.Impact.V120.Planning.Common;

	public class ProductionBedContainer
	{
		private BedFilter filter;
		private List<ProductionBed> CastUnits = new List<ProductionBed>();

	    /// <summary>
	    /// Load Positioned Element Data
	    /// </summary>
	    /// <param name="forms"></param>
	    /// <param name="bedfilter"></param>
	    /// <param name="isUserPositionedElement"> </param>
	    /// <returns></returns>
	    public int LoadPositionedElementData( List<RecProductionFormStd> forms, BedFilter bedfilter, bool isUserPositionedElement = false )
        {
            this.filter = bedfilter;
            if( forms == null || forms.Count == 0 )
            {
                string msg = string.Format( "No forms defined!" );
                throw new FaultException<BusinessFaultContract>( new BusinessFaultContract() { Argument = msg }, "Cannot manage form!" );
            }

            foreach( var form in forms )
            {
                this.CastUnits.Add( new ProductionBed( form, bedfilter ) );
            }

            // Load existing elements
            var mgr = new ProjectManager();
            var castData = mgr.LoadBedPlanning( bedfilter, isUserPositionedElement? forms[0] : null );
            var casts = castData.CastList;
            foreach( var bed in this.CastUnits )
            {
                bed.AssignProductionCast( casts );
            }
            return 0;
        }

        /// <summary>
        /// CastUnit
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
		private ProductionBed GetCastUnit( RecProductionFormStd form)
		{
		    // Search for available Bed
		    return this.CastUnits.FirstOrDefault( curBed => curBed.Name.Equals( form.Name ) );
		}

        /// <summary>
        /// Returns true if it Has Existing Elements
        /// </summary>
        /// <param name="castUnit"></param>
        /// <returns></returns>
        public bool HasExistingElements( RecProductionFormStd castUnit )
        {
            var productionBed = this.GetCastUnit( castUnit );
            if( null == productionBed )
            {
                return false;
            }
            return productionBed.HasExistingElements();
        }

        /// <summary>
        /// Returns forms Production Cast if any otherwise null is returned
        /// </summary>
        /// <param name="castUnit"></param>
        /// <returns></returns>
        public RecProductionCast GetProductionCast( RecProductionFormStd castUnit )
        {
            var productionBed = this.GetCastUnit( castUnit );
            return null == productionBed ? null : productionBed.GetProductionCast();
        }

        /// <summary>
        /// Create Production Cast
        /// </summary>
        /// <param name="castUnit"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public RecProductionCast AdjustProductionCastToElement( RecProductionFormStd castUnit, RecTMElement element )
        {
            var productionBed = this.GetCastUnit( castUnit ); 

            return null == productionBed ? null : productionBed.AdjustProductionCastToElement( element );
        }

	    /// <summary>
        /// Position Elements OnBed
        /// </summary>
        /// <param name="bed"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
		public List<RecTMElement> PositionElementsOnBed( RecProductionFormStd bed, List<RecTMElement> elements )
		{
            var productionBed = this.GetCastUnit( bed );
            if( null == productionBed )
            {
                return null;
            }

            productionBed.PrepareLastElement();
            var result = productionBed.FillBed( elements );
			return result;
		}

        /// <summary>
        /// Saves elements into db
        /// </summary>
        /// <param name="bed"></param>
        /// <param name="elements"></param>
        public void Save( RecProductionFormStd bed, List<RecTMElement> elements )
        {
            var productionBed = this.GetCastUnit( bed );
            if( null == productionBed )
            {
                return;
            }
          
            if( null != elements && elements.Count > 0 )
            {
                productionBed.Save( elements );
            }
        }

        /// <summary>
        /// Sort existin elements
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="form"></param>
        /// <param name="curFilter"></param>
        /// <returns></returns>
		public int Sort( List<RecTMElement> elements, RecProductionFormStd form, BedFilter curFilter )
		{
			if( null == form || null == elements || elements.Count == 0 )
			{
				var msg = string.Format( "Missing sorting input!" );
				throw new FaultException<BusinessFaultContract>( new BusinessFaultContract() { Argument = msg }, "Cannot sort form!" );
			}

			this.filter = curFilter;
            var bed = new ProductionBed( form, curFilter );

			var positionedElements = bed.FillBed( elements );
			var savedElem = bed.Save( positionedElements );

            if( savedElem < elements.Count )
            {
                // remove the delta
                var mgr = new ProjectManager();
                var planner = new ModelPlanner();
                foreach( RecTMElement elem in elements )
                {
                    if( !mgr.Find( positionedElements, elem ) )
                    {
                        planner.ResetElementProduction( elem.Factory, elem.Project, 0, elem.ElementId, false );
                    }
                }
            }

            return 0;
		}
	}

}