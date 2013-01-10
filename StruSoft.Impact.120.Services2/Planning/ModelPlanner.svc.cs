using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data.Common;
using StruSoft.Impact.V120.Planning.Common;
using StruSoft.Impact.V120.DB;
using StruSoft.Impact.V120.DB.Query;
using StruSoft.Impact.Settings;

namespace StruSoft.Impact.V120.Services
{
    public class ModelPlanner : IModelPlanner
    {
        /// <summary>
        /// Bulk Update Element
        /// </summary>
        /// <param name="list"></param>
        public void BulkUpdateElement( List<RecTMElement> list )
        {
            foreach( RecTMElement rec in list )
            {
                SaveElementTransport( rec, false );
            }
        }

        /// <summary> 
        /// Creates a new record if the desired record is not yet created 
        /// and updates the desired record.
        /// </summary>
        /// <param name="rec">RecTransportPlanner</param>
        public void SaveElementTransport( RecTMElement rec, bool verifyExist )
        {
            if( rec == null || rec.Factory == null || rec.Project == null || rec.ElementId == 0 )
            {
                return;
            }

            // Create a new record if it doesnt exist
            if( verifyExist )
            {
                Verify( rec.Factory, rec.Project, rec.ElementId );
            }

            SaveElementTransport( rec );
        }

        /// <summary> 
        /// Creates a new record if the desired record is not yet created 
        /// and updates the desired record.
        /// </summary>
        /// <param name="rec">RecTransportPlanner</param>
        private void SaveElementTransport( RecTMElement rec )
        {
            if( rec == null || rec.Factory == null || rec.Project == null )
            {
                return;
            }
            // Now update the element with transport information
            ImpactUpdate update = new ImpactUpdate( ImpModelPlanning.Instance )
            {
                Columns =
                {
                    //{ ImpTransport.TransportStatus, record.TransportStatus },
                },
                Where =
				{
					ImpModelPlanning.Factory.Equal( rec.Factory ),
					ImpModelPlanning.Project.Equal( rec.Project ),
				}
            };
            // Extra Where Clause
            if( rec.ElementId > 0 )
            {
                update.Where.Add( ImpModelPlanning.ElementId.Equal( rec.ElementId ) );
            }
            else
            {
                update.Where.Add( ImpModelPlanning.TransportId.Equal( rec.TransportId ) );
            }

            // Extra Update fields
            if( rec.TransportId > 0 )
            {
                update.Columns.Add( ImpModelPlanning.TransportId, rec.TransportId );
            }

            if( rec.VehicleId > 0 )
            {
                update.Columns.Add( ImpModelPlanning.VehicleId, rec.VehicleId );
            }

            if( rec.StackId > 0 )
            {
                update.Columns.Add( ImpModelPlanning.StackId, rec.StackId );
            }
            if( rec.StackSequenceNo > 0 )
            {
                update.Columns.Add( ImpModelPlanning.StackSequenceNo, rec.StackSequenceNo );
            }

            string sql = update.ToString();

            int result;

            using( ImpactDatabase database = new ImpactDatabase() )
            {
                result = database.ExecuteNonQuery( sql );
            }
        }

        /// <summary>
        /// Saves element production data
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="verifyExist"></param>
        /// <returns></returns>
        public int SaveElementProduction( RecTMElement rec, bool verifyExist )
        {
            if( rec == null || string.IsNullOrWhiteSpace( rec.Factory ) || string.IsNullOrWhiteSpace( rec.Project ) || rec.ElementId == 0 )
            {
                return -1;
            }
            // Create a new record if it doesnt exist
            if( verifyExist )
            {
                Verify( rec.Factory, rec.Project, rec.ElementId );
            }

            var update = new ImpactUpdate( ImpModelPlanning.Instance )
            {
                Columns = 
				{
					{ ImpModelPlanning.CastId, rec.CastId },
					{ ImpModelPlanning.BedSequenceNo, rec.BedSequenceNo },
					{ ImpModelPlanning.BedX, rec.BedX },
					{ ImpModelPlanning.BedY, rec.BedY },
					{ ImpModelPlanning.BedZ, rec.BedZ },
					{ ImpModelPlanning.BedRotation, rec.BedRotation },
				},
                Where = 
				{
					{ ImpModelPlanning.Factory.Equal( rec.Factory ) },
					{ ImpModelPlanning.Project.Equal( rec.Project ) },
					{ ImpModelPlanning.ElementId.Equal( rec.ElementId ) },
				},
            };

            string statement = update.ToString();

            int result;

            using( ImpactDatabase database = new ImpactDatabase() )
            {
                result = database.ExecuteNonQuery( statement );
            }
            return result;
        }

        #region Element Status
        /// <summary>
        /// Data holder
        /// </summary>
        internal class StatusDataHolder
        {
            public string Factory { get; set; }
            public string Project { get; set; } // Required since Cast Planning works with many projects at a time
            public int ElementId { get; set; }
            public int CastId { get; set; }
            public DateTime? ProductionDate { get; set; }
            public int ReadyForProduction { get; set; }

            public StatusDataHolder()
            {
            }
        }

        /// <summary> 
        /// Creates a new record if the desired record is not yet created 
        /// and updates the desired record.
        /// </summary>
        /// <param name="rec">RecTransportPlanner</param>
        public int SaveElementTransportStatus( RecTransport rec, List<RecElementIdStatusStd> settings )
        {
            if( rec == null || rec.Factory == null || rec.Project == null || rec.TransportId == 0 )
            {
                return 0;
            }
            bool isValidElementIdStatus = rec.ElementIdStatus != -1;
            bool noStatus = rec.TransportStatus == (int)TransportStatus.NoStatus;
            int result = 0;
            if( noStatus )
            {
                result = SaveElementTransportNoStatus( rec, settings );
            }

            // Now update the element with transport information
            ImpactUpdate update = new ImpactUpdate( ImpModelPlanning.Instance )
            {
                Columns =
                {
                    //{ ImpTransport.TransportStatus, record.TransportStatus },
                },
                Where =
				{
					ImpModelPlanning.Factory.Equal( rec.Factory ),
					ImpModelPlanning.Project.Equal( rec.Project ),
					ImpModelPlanning.TransportId.Equal( rec.TransportId ),
				}
            };

            if( noStatus )
            {
                if( rec.ClearDeliveryDate )
                {
                    update.Columns.Add( ImpModelPlanning.DeliveryDate, null );
                }
                if( rec.ClearPlannedDeliveryDate )
                {
                    update.Columns.Add( ImpModelPlanning.PlannedDeliveryDate, null );
                }
            }
            else if( rec.TransportStatus == (int)TransportStatus.Planned )
            {
                if( rec.UpdateElementStatus && isValidElementIdStatus )
                {
                    update.Columns.Add( ImpModelPlanning.ElementIdStatus, rec.ElementIdStatus );
                }
                if( rec.UpdateDeliveryDate && rec.DeliveryDate != null )
                {
                    //update.Columns.Add( ImpModelPlanning.DeliveryDate, rec.DeliveryDate );
                    update.Columns.Add( ImpModelPlanning.PlannedDeliveryDate, rec.DeliveryDate );
                }
            }
            else if( rec.TransportStatus == (int)TransportStatus.CallOff )
            {
                if( rec.UpdateElementStatus && isValidElementIdStatus )
                {
                    update.Columns.Add( ImpModelPlanning.ElementIdStatus, rec.ElementIdStatus );
                }
            }
            else if( rec.TransportStatus == (int)TransportStatus.Delivered )
            {
                if( rec.UpdateElementStatus && isValidElementIdStatus )
                {
                    update.Columns.Add( ImpModelPlanning.ElementIdStatus, rec.ElementIdStatus );
                }
                if( rec.UpdateDeliveryDate && rec.DeliveryDate != null )
                {
                    update.Columns.Add( ImpModelPlanning.DeliveryDate, rec.DeliveryDate );
                }
            }

            if( update.Columns.Count == 0 )
            {
                return 0; // Nothing to update
            }

            string sql = update.ToString();

            using( ImpactDatabase database = new ImpactDatabase() )
            {
                result += database.ExecuteNonQuery( sql );
            }
            return result;
        }

        /// <summary>
        /// Sets element no status
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        private int SaveElementTransportNoStatus( RecTransport rec, List<RecElementIdStatusStd> settings )
        {
            int produced = RecElementIdStatusStd.GetLocalSettingFromGlobalId( settings, 4 ).StatusId;
            int notProduced = RecElementIdStatusStd.GetLocalSettingFromGlobalId( settings, 3 ).StatusId;
            int readyForProduction = RecElementIdStatusStd.GetLocalSettingFromGlobalId( settings, 2 ).StatusId;
            int notReadyForProduction = RecElementIdStatusStd.GetLocalSettingFromGlobalId( settings, 1 ).StatusId;

            switch( ImpactDatabase.DataSource )
            {
                case DataSource.SqlServer:
                case DataSource.SqlServerExpress:
                {
                    string statement = string.Format( @"
                        UPDATE PLN
                        SET
                        PLN.ELEMENT_ID_STATUS =
                        ( CASE 
                          WHEN (PLN.CAST_ID > 0 AND PLN.PRODUCTION_DATE is  not null ) THEN {0}
                          WHEN (PLN.CAST_ID > 0) THEN {1}
                          WHEN (GEO.READY_FOR_PRODUCTION > 0) THEN {2}
                          ELSE {3}
                          END
                        )
                        FROM IMP_MODEL_PLANNING PLN
                        INNER JOIN IMP_MODEL_GEOMETRY GEO ON
                           PLN.FACTORY = GEO.FACTORY AND
                           PLN.PROJECT = GEO.PROJECT AND
                           PLN.ELEMENT_ID = GEO.ELEMENT_ID
                        WHERE 
                        PLN.TRANSPORT_ID =  {4} AND
                        PLN.FACTORY = '{5}' AND
                        PLN.PROJECT = '{6}' 
                        ", produced, notProduced, readyForProduction, notReadyForProduction, rec.TransportId, rec.Factory, rec.Project );

                    int result;

                    using( ImpactDatabase database = new ImpactDatabase() )
                    {
                        result = database.ExecuteNonQuery( statement );
                    }
                    return result;
                }
                case DataSource.Ingres92:
                case DataSource.Ingres100:
                {
                    int result = 0;
                    List<StatusDataHolder> list = LoadElementStatusData( rec.Factory, rec.Project, 0, rec.TransportId, true );
                    if( null == list || list.Count == 0 )
                    {
                        return 0;
                    }
                    foreach( StatusDataHolder data in list )
                    {
                        int idStatus = 0;
                        if( data.CastId > 0 && data.ProductionDate != null )
                        {
                            idStatus = produced;
                        }
                        else if( data.CastId > 0 )
                        {
                            idStatus = notProduced;
                        }
                        else if( 1 == data.ReadyForProduction )
                        {
                            idStatus = readyForProduction;
                        }
                        else
                        {
                            idStatus = notReadyForProduction;
                        }

                        result = UpdateElementStatus( data.Factory, data.Project, data.ElementId, idStatus );
                    }
                    return result;
                }
            }
            return 0;
        }

        /// <summary>
        /// Update Element IdStatus
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="project"></param>
        /// <param name="elementId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private int UpdateElementStatus( string factory, string project, int elementId, int idStatus )
        {
            int result = 0;
			var update = new ImpactUpdate( ImpModelPlanning.Instance )
			{
				Columns = 
				{
					{ ImpModelPlanning.ElementIdStatus, idStatus },
				},
				Where = 
				{
					{ ImpModelPlanning.Factory.Equal( factory ) },
					{ ImpModelPlanning.Project.Equal( project ) },
					{ ImpModelPlanning.ElementId.Equal( elementId ) },
				},
			};

			string statement = update.ToString();
			using( ImpactDatabase database = new ImpactDatabase() )
			{
				result = database.ExecuteNonQuery( statement );
			}
            return result;
        }

        /// <summary>
        /// Returns element status info
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="project"></param>
        /// <param name="castId"></param>
        /// <param name="transportId"></param>
        /// <param name="isTransport"> </param>
        /// <returns></returns>
        private List<StatusDataHolder> LoadElementStatusData( string factory, string project, int castId, int transportId, bool isTransport )
        {
            if( null == factory || null == project || ( 0 == castId && 0 == transportId ) )
            {
                return null;
            }
            ImpactQuery query = new ImpactQuery()
           {
               Select =
				{
                    ImpModelPlanning.Factory,
                    ImpModelPlanning.Project, // Project is required since Cast Planning works with many projects at a time
                    ImpModelPlanning.ElementId,
					ImpModelPlanning.CastId,
					ImpModelPlanning.ProductionDate,
                    ImpModelGeometry.ReadyForProduction,
                },

               From = { ImpModelPlanning.As( "MPL" ) },

               Join =
				{
					Join.Left( ImpModelGeometry.As( "MGO" ),	
						       ImpModelPlanning.Factory.Equal( ImpModelGeometry.Factory ),
						       ImpModelPlanning.Project.Equal( ImpModelGeometry.Project ),
						       ImpModelPlanning.ElementId.Equal( ImpModelGeometry.ElementId ) ),
                },

               Where = {
							ImpModelPlanning.Factory.Equal( factory ), 
							//ImpModelPlanning.Project.Equal( project ), 
						},
           };

            if( isTransport )
            {
                query.Where.Add( ImpModelPlanning.Project.Equal( project ) );
                query.Where.Add( ImpModelPlanning.TransportId.Equal( transportId ) );
            }
            else
            {
                query.Where.Add( ImpModelPlanning.CastId.Equal( castId ) );
            }

            var statement = query.ToString();
            using( ImpactDatabase database = new ImpactDatabase() )
            {
                return database.GetAll( statement, ParseStatus ).ToList();
            }
        }

        /// <summary>
        /// Status Parser
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        private static StatusDataHolder ParseStatus( DbDataReader column )
        {
            return new StatusDataHolder()
             {
                 // Project is required since Cast Planning works with many projects at a time
                 Factory = column[0].Cast<string>(),
                 Project = column[1].Cast<string>(),
                 ElementId = column[2].Cast<int>(),
                 CastId = column[3].Cast<int?>() ?? 0,
                 ProductionDate = column[4].Cast<DateTime?>(),
                 ReadyForProduction = column[5].Cast<int?>() ?? 0,
             };
        }

        /// <summary>
        /// Sets the cast status
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        public int SaveElementProductionStatus( RecProductionCast rec, RecProductionCast productionCastStatus, List<RecElementIdStatusStd> settings )
        {
            if( null == rec || string.IsNullOrWhiteSpace( rec.Factory ) || string.IsNullOrWhiteSpace( rec.Project ) || 0 == rec.CastId )
            {
                return 0;
            }

            bool isValidElementIdStatus = productionCastStatus.ElementIdStatus != -1;
            bool noStatus = productionCastStatus.CastStatus == (int)CastStatus.NoStatus;
            int result = 0;
            if( noStatus )
            {
                SaveElementProductionNoStatus( rec, settings );
            }

            var update = new ImpactUpdate( ImpModelPlanning.Instance )
            {
                Columns = 
				{
					//{ ImpModelPlanning.ElementIdStatus, rec.ElementIdStatus },
				},
                Where = 
				{
					{ ImpModelPlanning.Factory.Equal( rec.Factory ) },
					//{ ImpModelPlanning.Project.Equal( rec.Project ) },
					{ ImpModelPlanning.CastId.Equal( rec.CastId ) },
				},
            };

            if( noStatus )
            {
                if( productionCastStatus.ClearProductionDate )
                {
                    update.Columns.Add( ImpModelPlanning.ProductionDate, null );
                }
                if( productionCastStatus.ClearPlannedProductionDate )
                {
                    update.Columns.Add( ImpModelPlanning.PlannedProductionDate, null );
                }
            }
            else if( productionCastStatus.CastStatus == (int)CastStatus.Planned )
            {
                if( productionCastStatus.UpdateElementStatus && isValidElementIdStatus )
                {
                    update.Columns.Add( ImpModelPlanning.ElementIdStatus, productionCastStatus.ElementIdStatus );
                }
                if( productionCastStatus.UpdateProductionDate && productionCastStatus.StartDate != null )
                {
                    update.Columns.Add( ImpModelPlanning.PlannedProductionDate, productionCastStatus.StartDate );
                }
            }
            else if( productionCastStatus.CastStatus == (int)CastStatus.Produced )
            {
                if( productionCastStatus.UpdateElementStatus && isValidElementIdStatus )
                {
                    update.Columns.Add( ImpModelPlanning.ElementIdStatus, productionCastStatus.ElementIdStatus );
                }
                if( productionCastStatus.UpdateProductionDate && productionCastStatus.StartDate != null )
                {
                    update.Columns.Add( ImpModelPlanning.ProductionDate, productionCastStatus.StartDate );
                }
            }

            if( update.Columns.Count == 0 )
            {
                return 0; // Nothing to update
            }

            string sql = update.ToString();

            using( ImpactDatabase database = new ImpactDatabase() )
            {
                result += database.ExecuteNonQuery( sql );
            }
            return result;
        }

        /// <summary>
        /// Resets status
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="settings"> </param>
        /// <returns></returns>
        private int SaveElementProductionNoStatus( RecProductionCast rec, List<RecElementIdStatusStd> settings )
        {
            var readyForProduction = RecElementIdStatusStd.GetLocalSettingFromGlobalId( settings, 2 ).StatusId;
            var notReadyForProduction = RecElementIdStatusStd.GetLocalSettingFromGlobalId( settings, 1 ).StatusId;

            switch( ImpactDatabase.DataSource )
            {
                case DataSource.SqlServer:
                case DataSource.SqlServerExpress:
                {
                    string statement = string.Format( @"
                        UPDATE PLN
                        SET
                        PLN.ELEMENT_ID_STATUS =
                        ( CASE 
                            WHEN (GEO.READY_FOR_PRODUCTION  > 0 ) THEN {0}
                            ELSE {1}
                            END
                        )
                        FROM IMP_MODEL_PLANNING PLN
                        INNER JOIN IMP_MODEL_GEOMETRY GEO ON
                            PLN.FACTORY = GEO.FACTORY AND
                            PLN.PROJECT = GEO.PROJECT AND
                            PLN.ELEMENT_ID = GEO.ELEMENT_ID
                        WHERE 
                        PLN.CAST_ID =  {2} AND
                        PLN.FACTORY = '{3}' 
                        ", readyForProduction, notReadyForProduction, rec.CastId, rec.Factory ); //, rec.Project  AND PLN.PROJECT = '{4}' 

                    int result;

                    using( ImpactDatabase database = new ImpactDatabase() )
                    {
                        result = database.ExecuteNonQuery( statement );
                    }
                    return result;
                }
                case DataSource.Ingres92:
                case DataSource.Ingres100:
                {
                    int result = 0;
                    List<StatusDataHolder> list = LoadElementStatusData( rec.Factory, rec.Project, rec.CastId, 0, false );
                    if( null == list || list.Count == 0 )
                    {
                        return 0;
                    }
                    foreach( var data in list )
                    {
                        var idStatus = 0;
                        if( 1 == data.ReadyForProduction )
                        {
                            idStatus = readyForProduction;
                        }
                        else
                        {
                            idStatus = notReadyForProduction;
                        }

                        UpdateElementStatus( data.Factory, data.Project, data.ElementId, idStatus );
                    }
                    return result;
                }
            }
            return 0;
        }
        #endregion Element Status

        /// <summary>
        /// Saves elemen cast information
        /// </summary>
        /// <param name="list"></param>
        public void BulkUpdateCastElement( List<RecTMElement> list )
        {
            if( null == list )
            {
                return;
            }
            foreach( RecTMElement rec in list )
            {
                SaveElementProduction( rec, false );
            }
        }

        /// <summary>
        /// Load cast Element Ids
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="project"></param>
        /// <param name="castId"></param>
        /// <returns></returns>
        public List<int> LoadElementIds( string factory, string project, int castId )
        {
            ImpactQuery query = new ImpactQuery()
            {
                Select =
				{
					ImpModelPlanning.ElementId,
				},
                From = { ImpModelPlanning.As( "T1" ) },

                Where = 
                { 
					{ ImpModelPlanning.Factory.Equal( factory ) },
					{ ImpModelPlanning.Project.Equal( project ) },
					{ ImpModelPlanning.CastId.Equal( castId ) },
                }

            };

            string statement = query.ToString();

            List<int> result;

            using( ImpactDatabase database = new ImpactDatabase() )
            {
                result = database.GetAll( statement, ElementParse );
            }
            return result;
        }

        /// <summary>
        /// Parser
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public static int ElementParse( DbDataReader dataReader )
        {
            return DataConverter.Cast<int>( dataReader[0] );
        }

        /// <summary>
        /// Resets the cast info
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="project"></param>
        /// <param name="castId"></param>
        /// <param name="elementId"></param>
        /// <param name="verifyExist"></param>
        /// <returns></returns>
        public int ResetElementProduction( string factory, string project, int castId, int elementId, bool verifyExist )
        {
            if( string.IsNullOrWhiteSpace( factory ) || string.IsNullOrWhiteSpace( project ) || ( castId == 0 && elementId == 0 ) )
            {
                return -1;
            }
            // Create a new record if it doesnt exist
            if( verifyExist && elementId > 0 )
            {
                Verify( factory, project, elementId );
            }

            var update = new ImpactUpdate( ImpModelPlanning.Instance )
            {
                Columns = 
				{
					{ ImpModelPlanning.CastId, 0 },
					{ ImpModelPlanning.BedSequenceNo, 0 },
					{ ImpModelPlanning.BedX, 0 },
					{ ImpModelPlanning.BedY, 0 },
					{ ImpModelPlanning.BedZ, 0 },
					{ ImpModelPlanning.BedRotation, 0 },
				},
                Where = 
				{
					{ ImpModelPlanning.Factory.Equal( factory ) },
					{ ImpModelPlanning.Project.Equal( project ) },
				},
            };

            if( castId > 0 )
            {
                update.Where.Add( ImpModelPlanning.CastId.Equal( castId ) );
            }

            if( elementId > 0 )
            {
                update.Where.Add( ImpModelPlanning.ElementId.Equal( elementId ) );
            }

            string statement = update.ToString();

            int result;

            using( ImpactDatabase database = new ImpactDatabase() )
            {
                result = database.ExecuteNonQuery( statement );
            }
            return result;
        }

        private string ConcatWC( string wc, string tableCol, int val )
        {
            if( val > 0 )
            {
                if( wc.Length > 0 )
                {
                    wc += " AND ";
                }
                wc += tableCol + "=" + Conv.Sql( val );
            }
            return wc;
        }

        /// <summary>
        /// Reset element transport data and corrects the stack order number on the stack
        /// </summary>
        /// <param name="rec">RecTransportPlanner</param>
        public void RemoveElement( RecTMElement rec )
        {
            if( rec.Factory == null || rec.Project == null )
            {
                return;
            }
            if( rec.ElementId <= 0 || rec.TransportId <= 0 || rec.VehicleId <= 0 || rec.StackId <= 0 )
            {
                return;
            }
            ResetTransport( rec.Factory, rec.Project, rec.ElementId, rec.TransportId, rec.VehicleId, rec.StackId );
            ShrinkStackOrder( rec );
        }
        /// <summary>
        /// Shrinks stack order no for elements in a given stack
        /// with higher stack order if there are no elements on the same level
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        private int ShrinkStackOrder( RecTMElement rec )
        {
            if( rec.Factory == null || rec.Project == null )
            {
                return -1;
            }
            if( rec.ElementId <= 0 || rec.TransportId <= 0 || rec.VehicleId <= 0 || rec.StackId <= 0 )
            {
                return -1;
            }
            // Find elements on the same level
            ImpactQuery selectQuery = new ImpactQuery()
            {
                From = { ImpModelPlanning.As( "T1" ) },
                Where =
				{
					ImpModelPlanning.Factory.Equal( rec.Factory ),
					ImpModelPlanning.Project.Equal( rec.Project ),
					ImpModelPlanning.TransportId.Equal( rec.TransportId ),
					ImpModelPlanning.VehicleId.Equal( rec.VehicleId ),
					ImpModelPlanning.StackId.Equal( rec.StackId ),
					ImpModelPlanning.StackSequenceNo.Equal( rec.StackSequenceNo ),
				}
            };

            // Conditional update
            int result = 0;
            using( ImpactDatabase database = new ImpactDatabase() )
            {

                string wc = "";
                wc = ConcatWC( wc, "TRANSPORT_ID", rec.TransportId );
                wc = ConcatWC( wc, "VEHICLE_ID", rec.VehicleId );
                wc = ConcatWC( wc, "STACK_ID", rec.StackId );
                string sql = "Update IMP_MODEL_PLANNING set "
                                        + "Stack_sequence_no=Stack_sequence_no - " + Conv.Sql( 1 )
                                        + " Where "
                                        + "Factory =" + Conv.Sql( rec.Factory ) + " AND "
                                        + "Project=" + Conv.Sql( rec.Project ) + " AND "
                                        + "Stack_sequence_no > " + Conv.Sql( rec.StackSequenceNo ) + " AND "
                                        + wc;

                string statement = string.Format( "IF NOT EXISTS ( {0} ) {1}", selectQuery.ToString(), sql );

                result = database.ExecuteNonQuery( statement );
            }
            return result;
        }
        /// <summary>
        /// Creates a new record if the desired record is not yet created
        /// and updates the desired record.
        /// </summary>
        /// <param name="rec">RecTransportPlanner</param>
        public void ResetTransport( string factory, string project, int elementId, int transportId, int vehicleId, int stackId )
        {
            if( factory == null || project == null )
            {
                return;
            }
            if( elementId == 0 && transportId == 0 && vehicleId == 0 && stackId == 0 )
            {
                return;
            }

            var where = WhereGroup.And( ImpModelPlanning.Factory.Equal( factory ) );
            where.Add( ImpModelPlanning.Project.Equal( project ) );
            if( elementId > 0 )
            {
                where.Add( ImpModelPlanning.ElementId.Equal( elementId ) );
            }
            if( transportId > 0 )
            {
                where.Add( ImpModelPlanning.TransportId.Equal( transportId ) );
            }
            if( vehicleId > 0 )
            {
                where.Add( ImpModelPlanning.VehicleId.Equal( vehicleId ) );
            }
            if( stackId > 0 )
            {
                where.Add( ImpModelPlanning.StackId.Equal( stackId ) );
            }

            using( ImpactDatabase database = new ImpactDatabase() )
            {
                ImpactUpdate update = new ImpactUpdate( ImpModelPlanning.Instance )
                {
                    Columns = 
					{
 						{ ImpModelPlanning.TransportId, 0 },
 						{ ImpModelPlanning.VehicleId, 0 },
 						{ ImpModelPlanning.StackId, 0 },

 						{ ImpModelPlanning.RackSide, 0 },
 						{ ImpModelPlanning.StackSequenceNo, 0 },
 						{ ImpModelPlanning.StackX, 0 },
 						{ ImpModelPlanning.StackY, 0 },
 						{ ImpModelPlanning.StackZ, 0 },
 						{ ImpModelPlanning.StackRotation, 0 },
 						{ ImpModelPlanning.DeliveryDate, null },
					},
                    Where =
					{
						where
					}
                };

                string sql = update.ToString();

                database.ExecuteNonQuery( sql );
            }
        }

        /// <summary>
        /// Creates a new record if the desired record is not yet created.
        /// </summary>
        /// <param name="rec">RecTransportPlanner</param>
        /// <param name="factory"> </param>
        /// <param name="project"> </param>
        /// <param name="elementId"> </param>
        public void Verify( string factory, string project, int elementId )
        {
            if( string.IsNullOrEmpty(factory) || string.IsNullOrEmpty(project) || elementId <= 0 )
            {
                return;
            }
		    var selectQuery = new ImpactQuery()
		        {
		            From = { ImpModelPlanning.As( "T1" ) },
		            Where =
		                {
		                    ImpModelPlanning.Factory.Equal( factory ),
		                    ImpModelPlanning.Project.Equal( project ),
		                    ImpModelPlanning.ElementId.Equal( elementId ),
		                }
		        };
		    var insert = new ImpactInsert( ImpModelPlanning.Instance )
		        {
		            Columns =
		                {
		                    { ImpModelPlanning.Factory, factory },
		                    { ImpModelPlanning.Project, project },
		                    { ImpModelPlanning.ElementId, elementId },
		                }
		        };
		    switch( ImpactDatabase.DataSource )
		    {
		        case DataSource.SqlServer:
		        case DataSource.SqlServerExpress:
		        {
                    using( var database = new ImpactDatabase() )
                    {
                        var statement = string.Format( "IF NOT EXISTS ( {0} ) {1}", selectQuery.ToString(), insert.ToString() );

                        database.ExecuteNonQuery( statement );
                    }
                    break;
                }
                case DataSource.Ingres92:
                case DataSource.Ingres100:
                {
		            List<RecTMElement> result;
		            using( var database = new ImpactDatabase() )
		            {
		                result = database.GetAll( selectQuery.ToString(), ParseModelPlanning ).ToList();
		            }
                    if( result.Count == 0 )
                    {
		                using( var database = new ImpactDatabase() )
		                {
                            database.ExecuteNonQuery( insert.ToString() );
                        }
                    }

		            break;
		        }
    	    }
        }

        private static RecTMElement ParseModelPlanning( DbDataReader column )
        {
            return new RecTMElement()
             {
                 // Project is required since Cast Planning works with many projects at a time
                 Factory = column[0].Cast<string>(),
                 Project = column[1].Cast<string>(),
                 ElementId = column[2].Cast<int>()
             };
        }


        private double GetGap( RecTMElement element1, RecTMElement element2 )
        {
            // Gap Between Elements
            double gap = 0;
            if( element1.BedRotation > 0 )
            {
                gap = element2.BedX - element1.BedX - element2.ElementLengthOnBed;
            }
            else
            {
                gap = element2.BedX - element1.BedX - element1.ElementLengthOnBed;
            }

            return gap;
        }

        private bool SwapElements( RecTMElement element1, RecTMElement element2 )
        {
            if( element1 == null || element2 == null )
            {
                return false;
            }
            Boolean rotated = false;
            RecTMElement temp = new RecTMElement( element1 );
            if( element1.BedRotation != element2.BedRotation )
            {
                // Rotate the first
                RecTMElement recRotated = RecTMElement.RotateElement( element1 );
                element1.BedX = recRotated.BedX;
                element1.BedY = recRotated.BedY;
                element1.BedRotation = recRotated.BedRotation;
                rotated = true;
            }
            // Gap Between Elements
            double gap = GetGap( element1, element2 );

            element1.BedSequenceNo = element2.BedSequenceNo;
            element1.BedX += element2.ElementLengthOnBed + gap;

            // Correct the rotation before saving
            if( rotated )
            {
                RecTMElement recRotated = RecTMElement.RotateElement( element1 );
                element1.BedX = recRotated.BedX;
                element1.BedY = recRotated.BedY;
                element1.BedRotation = recRotated.BedRotation;
            }
            SaveElementProduction( element1, false );

            element2.BedSequenceNo = temp.BedSequenceNo;
            element2.BedX -= ( temp.ElementLengthOnBed + gap );

            SaveElementProduction( element2, false );

            return true;
        }

        public bool SwapElements( List<RecTMElement> selElements, RecTMElement current )
        {
            if( selElements == null || selElements.Count == 0 || current == null )
            {
                return false;
            }

            foreach( RecTMElement rec in selElements )
            {
                if( rec.BedX < current.BedX )
                {
                    SwapElements( rec, current );
                }
                else
                {
                    SwapElements( current, rec );
                }
            }
            return true;
        }

        /// <summary>
        /// Move Element
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public bool MoveElement( List<RecTMElement> elements, double x, double y, double z, double rotation )
        {
            if( elements == null || elements.Count == 0 )
            {
                return false;
            }

            foreach( RecTMElement rec in elements )
            {
                Move( rec, x, y, z, rotation );
            }
            return true;
        }

        /// <summary>
        /// Move element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        private bool Move( RecTMElement element, double x, double y, double z, double rotation )
        {
            if( element == null )
            {
                return false;
            }

            RecTMElement temp = new RecTMElement( element );

            temp.BedX += x;
            temp.BedY += y;
            temp.BedZ += z;
            temp.BedRotation += rotation;

            SaveElementProduction( temp, false );

            return true;
        }

        /// <summary>
        /// Rotate
        /// </summary>
        /// <param name="selElements"></param>
        /// <returns></returns>
        public bool Rotate( List<RecTMElement> selElements )
        {
            if( selElements == null || selElements.Count == 0 )
            {
                return false;
            }

            foreach( RecTMElement rec in selElements )
            {
                Rotate( rec );
            }
            return true;
        }

        /// <summary>
        /// Rotate
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool Rotate( RecTMElement element )
        {
            if( element == null )
            {
                return false;
            }

            RecTMElement temp = RecTMElement.RotateElement( element );

            SaveElementProduction( temp, false );
            return true;
        }
    }

    public class RecElement
    {
    }
}
