using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StruSoft.Impact.V120.Services.Report
{
    using StruSoft.Impact.V120.DB;
    using StruSoft.Impact.V120.DB.Query;
    using StruSoft.Impact.V120.Planning.Common;
    using StruSoft.Impact.V120.Report.Common;
    using StruSoft.Impact.V120.Services;

    public class DrawingReport
    {
        /// <summary>
        /// Load
        /// </summary>
        /// <param name="reportFilter"></param>
        /// <returns></returns>
        public DrawingReportData Load( ReportFilter reportFilter )
        {
            var data = new DrawingReportData();

            var query = new ImpactQuery()
            {
                Select =
                    {
                        ImpDrawing.Factory,
                        ImpDrawing.Project,
                        ImpDrawing.DrawingName,
                        ImpDrawing.DrawingType,
                        ImpDrawing.DesignedBy,
                        ImpDrawing.Status,
                        ImpDrawing.ApprovedDate,
                        ImpDrawing.CreatedDate,
                        ImpDrawing.Text6,
                    },
                From = { ImpDrawing.As( "T1" ) },
                Join =
                    {
                        Join.Left(
                            ImpElement.As( "T2" ),
                            ImpDrawing.Factory.Equal( ImpElement.Factory ),
                            ImpDrawing.Project.Equal( ImpElement.Project ),
                            ImpDrawing.DrawingName.Equal( ImpElement.DrawingName ) ),
                    },
                Where =
                    {
                        ImpDrawing.Factory.Equal( reportFilter.Factory ),
                        ImpDrawing.Project.Equal( reportFilter.Project ),
                    },
            };

            if( reportFilter.Ranges.Count > 0 )
            {
                var list = new List<Where>();
                foreach( var range in reportFilter.Ranges )
                {
                    if( !string.IsNullOrEmpty( range.From ) && !string.IsNullOrEmpty( range.To ) )
                    {
                        list.Add( ImpDrawing.DrawingName.Between( range.From, range.To ) );
                    }
                }

                if( list.Count > 0 )
                {
                    query.Where.Add( WhereGroup.Or( list.ToArray() ) );
                }
            }

            var statement = query.ToString();

            using( var database = new ImpactDatabase() )
            {
				data.Rows = database.GetAll( statement, column => new DrawingReportRow()
				                {
				                    Factory = column[0].Cast<string>(),
				                    Project = column[1].Cast<string>(),
				                    DrawingName = column[2].Cast<string>(),
				                    DrawingType = column[3].Cast<string>(),
                                    DesignedBy = column[4].Cast<string>(),
                                    Status = column[5].Cast<string>(),
                                    ApprovedDate = column[6].Cast<DateTime?>(),
                                    CreatedDate = column[7].Cast<DateTime?>(),
                                    DrawingText6 = column[8].Cast<string>(),
				                } );
            }

            return data;

        }
    }
}