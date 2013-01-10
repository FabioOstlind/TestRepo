using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StruSoft.Impact.V120.Services
{
	internal static class Util
	{
		/// <summary>
		/// Right-aligns the project name.
		/// </summary>
		/// <param name="project">The project name to right-align.</param>
		/// <returns>A right-aligned instance of the project string.</returns>
		public static string CorrectProjectName( string project )
		{
			if( string.IsNullOrWhiteSpace( project ) )
				throw new ArgumentNullException( "project" );

			return  project.PadLeft( 12 );			
		}

		/// <summary>
		/// Get the company number from the factory number.
		/// </summary>
		/// <param name="factory"></param>
		/// <returns></returns>
		public static string FactoryToCompany( string factory )
		{
			if( string.IsNullOrWhiteSpace( factory ) )
				throw new ArgumentNullException( "factory" );

			if( factory.Length != 4 )
				throw new ArgumentException( "Invalid format.", "factory" );

			return factory.Substring( 0, 2 ) + "00";
		}
	}
}