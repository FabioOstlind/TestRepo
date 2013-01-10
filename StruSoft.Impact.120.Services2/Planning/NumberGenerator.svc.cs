using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using StruSoft.Impact.V120.Planning.Common;
using StruSoft.Impact.V120.DB;
using StruSoft.Impact.V120.DB.Query;

namespace StruSoft.Impact.V120.Services
{
    using System.Linq;

    using StruSoft.Impact.Settings;

    /// <summary>
	/// Business logic service.
	/// Retrievs the next sequence value from the number generator object
	/// </summary>
	public partial class ProjectManager : INumberGenerator
	{
		/// <summary>
		/// Returns the next sequence from the number generator object
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="project"></param>
		/// <param name="variable"></param>
		/// <returns></returns>
		public int GetNextNumber( string factory, string project, string variable )
		{
			RecNumberGenerator rec = LoadRecord( factory, project, variable );
			if( rec == null )
			{
				throw new Exception( "Variable " + variable + " Not found for Factory " + factory + ", Project " + project );
			}
			// Check bounds
			int lower = rec.LowerBound;
			int upper = rec.UpperBound;
			if( upper < lower )
			{
				throw new Exception
						 ( "Error in NumberGenerator '" + variable + "'. Upper bound ("
							+ upper + ") is below the Lower bound ("
							+ lower + ").", null );
			}//end if

			// Get a new number
			int next = rec.NextNumber;
			if( next > upper || next < lower )
			{
				next = lower;
			}//end if

			// Calculate the new next number to be stored in db
			int newNext = next + 1;
			if( newNext > upper || newNext < lower )
			{
				newNext = lower;
			}//end if

			// Store the new number
			SetNextNumber( rec, newNext );
			// Return the number
			return newNext;
		}//end generate
		/// <summary>
		/// Returns the next sequence from the number generator object
		/// </summary>
		/// <param name="rec"></param>
		/// <param name="next"></param>
		private void SetNextNumber( RecNumberGenerator rec, int next )
		{
			using( ImpactDatabase database = new ImpactDatabase() )
			{
				ImpactUpdate update = new ImpactUpdate( ImpNumGen.Instance )
				{
					Columns =
					{
						{ ImpNumGen.NextNumber, next },
						{ ImpNumGen.ChangedDate, DateTime.Now }
					},
					Where =
					{
						ImpNumGen.Factory.Equal( rec.Factory ),
						ImpNumGen.Project.Equal( rec.Project ),
						ImpNumGen.Variable.Equal( rec.Variable ),
					}
				};

				string statement = update.ToString();

				database.ExecuteNonQuery( statement );
			}
		}
		/// <summary>
		/// Loads the required number generator object
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="project"></param>
		/// <param name="variable"></param>
		/// <returns></returns>
		private RecNumberGenerator LoadRecord( string factory, string project, string variable )
		{
			RecNumberGenerator record = null;
			using( ImpactDatabase database = new ImpactDatabase() )
			{
				ImpactQuery query = new ImpactQuery()
				{
					From = { ImpNumGen.As( "T1" ) },
					Where =
					{
						ImpNumGen.Factory.Equal( factory ),
						ImpNumGen.Project.Equal( project ),
						ImpNumGen.Variable.Equal( variable ),
					},
				};

				string statement = query.ToString();

				record = database.GetFirst( statement, column => new RecNumberGenerator()
				{
					Factory = DataConverter.Cast<string>( column["FACTORY"] ),
					Project = DataConverter.Cast<string>( column["PROJECT"] ),
					Variable = DataConverter.Cast<string>( column["Variable"] ),
					Description = DataConverter.Cast<string>( column["Description"] ),
					UpperBound = DataConverter.Cast<int>( column["Upper_Bound"] ),
					LowerBound = DataConverter.Cast<int>( column["Lower_Bound"] ),
					NextNumber = DataConverter.Cast<int>( column["Next_Number"] ),
					ChangedDate = DataConverter.Cast<DateTime>( column["Changed_Date"] )
				} );
			}

			return record;
		}
		/// <summary>
		/// Make sure that the current project creates its number gen keys
		/// </summary>
		/// <param name="Factory"></param>
		/// <param name="Project"></param>
		/// <param name="variableName"></param>
		public void VerifyProject( String factory, String project )
		{
			// Get copmany id
			string company = GetCompany( factory );

			Type type = typeof( RecNumberGenerator ); // Get type pointer
			FieldInfo[] fields = type.GetFields(); // Obtain all fields
			foreach( var field in fields ) // Loop through fields
			{
				string name = field.Name; // Get string name
				if( field.IsStatic && name.Substring( 0, 3 ) == "NG_" ) // See if it is a string.
				{
					Verify( factory, project, (String)field.GetValue( null ) );
				}
				else if( field.IsStatic && name.Substring( 0, 7 ) == "CMP_NG_" )
				{
					Verify( company, company, (String)field.GetValue( null ) );
				}
			}
		}
		/// <summary>
		/// Returns the Number Generator's next number.
		/// </summary>
		public void VerifyAll()
		{
			List<RecNumberGenerator> list = GetAllProjects();

			foreach( RecNumberGenerator numGen in list )
			{
				VerifyProject( numGen.Factory, numGen.Project );
			}
		}
		static public string GetCompany( string factory )
		{
			RecNumberGenerator record = null;
			using( ImpactDatabase database = new ImpactDatabase() )
			{
				ImpactQuery query = new ImpactQuery()
				{
					From = { ImpFactory.As( "T1" ) },
					Where =
					{
						ImpFactory.Factory.Equal( factory )
					},
				};

				string statement = query.ToString();

				record = database.GetFirst( statement, column => new RecNumberGenerator()
				{
					Factory = DataConverter.Cast<string>( column["COMPANY"] ),
				} );
			}
			return record.Factory;// In fact this is Company!!

		}
		/// <summary>
		/// Creates all the needed number generator objects
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="project"></param>
		/// <param name="variableName"></param>
        public void Verify( string factory, string project, string variableName )
		{
            const string Description = "Initial value by system";
		    var selectQuery = new ImpactQuery()
		        {
		            From = { ImpNumGen.As( "T1" ) },
		            Where =
		                {
		                    ImpNumGen.Factory.Equal( factory ),
		                    ImpNumGen.Project.Equal( project ),
		                    ImpNumGen.Variable.Equal( variableName ),
		                }
		        };
		    var insert = new ImpactInsert( ImpNumGen.Instance )
		        {
		            Columns =
		                {
		                    { ImpNumGen.Factory, factory },
		                    { ImpNumGen.Project, project },
		                    { ImpNumGen.Variable, variableName },
		                    { ImpNumGen.Description, Description },
		                    { ImpNumGen.UpperBound, RecNumberGenerator.UPPER_BOUND },
		                    { ImpNumGen.LowerBound, RecNumberGenerator.LOWER_BOUND },
		                    { ImpNumGen.NextNumber, RecNumberGenerator.LOWER_BOUND + 1 },
		                    { ImpNumGen.ChangedDate, DateTime.Now },
		                }
		        };
		    switch( ImpactDatabase.DataSource )
		    {
		        case DataSource.SqlServer:
		        case DataSource.SqlServerExpress:
		        {
                    using( ImpactDatabase database = new ImpactDatabase() )
                    {
                        var statement = string.Format( "IF NOT EXISTS ( {0} ) {1}", selectQuery.ToString(), insert.ToString() );

                        database.ExecuteNonQuery( statement );
                    }
                    break;
                }
                case DataSource.Ingres92:
                case DataSource.Ingres100:
                {
		            List<RecNumberGenerator> result;
		            using( var database = new ImpactDatabase() )
		            {
		                result = database.GetAll( selectQuery.ToString(), ParseNumGen ).ToList();
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

        /// <summary>
        /// ParseNumGen
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        private static RecNumberGenerator ParseNumGen( DbDataReader column )
        {
            return new RecNumberGenerator()
             {
                 // Project is required since Cast Planning works with many projects at a time
                 Factory = column[0].Cast<string>(),
                 Project = column[1].Cast<string>(),
                 Variable = column[2].Cast<string>(),
             };
        }

        /// <summary>
		/// Helper
		/// </summary>
		/// <returns>All the existing projects</returns>
		private List<RecNumberGenerator> GetAllProjects()
		{
			List<RecNumberGenerator> list = new List<RecNumberGenerator>();

			using( ImpactDatabase database = new ImpactDatabase() )
			{
				ImpactQuery query = new ImpactQuery() { From = { ImpProject.As( "T1" ) } };
				string statement = query.ToString();

				list = database.GetAll( statement, column => new RecNumberGenerator()
				{
					Factory = DataConverter.Cast<string>( column["FACTORY"] ),
					Project = DataConverter.Cast<string>( column["PROJECT"] )
				} );
			}
			return list;
		}
	}
}
