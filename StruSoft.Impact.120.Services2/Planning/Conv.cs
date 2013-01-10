using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
/*************************************************************************
* Author: Theib Sawaf, Strusoft 2012-01
*************************************************************************/
namespace StruSoft.Impact.V120.Services
{
	/// <summary>
	/// A Converter that converts values to SQL syntax form 
	/// </summary>

	public class Conv
	{
		/// <summary>
		/// This paramater contains the string that shuld be used when constructing
		/// SQL statements serching for columns containing empty strings. Every DBMS
		/// cannot handle empty strings, e.g. Oracle that use a blank instead.
		/// This parameter must be initialized.
		/// As a consequence of this, an application may only work with ONE BDMS.
		/// </summary>
		private static string emptyStringSubstitute = "";
		/// <summary>
		/// Please don't use!
		/// </summary>
		private Conv () 
		{
		}//end constructor	
		/****************************************************************************
		* Replaces a * with a % sign. Used to allow users to enter a star sign instead
		* of a % sign when searching for for instance a Product Name.
		* @param sql A string with an optional * sign.
		* @return A string with an any * signs replaced by % signs.
		*****************************************************************************/
		public static string Sql_replace_star_with_percent(string sql)
		{
			return sql.Replace('*', '%');
		}//end sql_replace_star_with_percent
		/****************************************************************************
		* Converts an Object array to a string for an IN clause inclusive ( and ,
		* @param values
		*        Values we want to put in a SQL IN WhereClause.
		* @return The values converted to an SQL IN string like ('kalle', 'olle')
		*****************************************************************************/
		public static string Sql (Object[] values) {
			if (values == null || values.Length == 0) {
				return "(" + Conv.Sql("") + ")";
			}//end if

			StringBuilder retVal = new StringBuilder("(");
			for (int i = 0; i < values.Length; i++) 
			{
				retVal.Append(Conv.Sql((Object)values[i]));
				if (i < values.Length - 1)
				{
					retVal.Append(",");
				}
			}//end if

			// Remove ending , and add )
			int len = retVal.Length;
			retVal.Append(")");
			return retVal.ToString();
		}//end sql

		/****************************************************************************
		* Converts an Object to its SQL form.
		* @param value A value we want to put in a SQL WhereClause.
		* @return The value converted to SQL syntax form.
		*****************************************************************************/
		public static string Sql(Object value)
		{
			if (value == null) {
				return "NULL";  // Used by Analyzer/FactInventory
			}else if (value is string) {
				return Sql ((string)value);
			//}else if (value is CodedValue) {
			//  return sql ((CodedValue)value);
			}else if (value is int) {
				return Sql(((int)value));
			}else if (value is long) {
				return Sql(((long)value));
			}else if (value is Double) {
				return Sql(((Double)value));
			}else if (value is float) {
				return Sql(((float)value));
			}else if (value is Boolean) {
				return Sql(((Boolean)value));
			}else if (value is char) {
				return Sql(((char)value));
			}
			//else if (value is Duration) {
			//  return sql ((Duration)value);
			//}else if (value is Week) {
			//  return sql ((Week)value);
		  else if (value is DateTime) {
				return Sql((DateTime)value, true, true);
			}
			//else if (value is Percentage)
			//{
			//  return sql ((Percentage)value);
			//}//end if

			return Sql ((string)value);  // Emergency exit
		}//end sql

		/// <summary>
		///Converts a string to its SQL form ("Dante's Peak" to "'Dante''s Peak'"). 
		/// </summary>
		/// <param name="value">value A value we want to put in a SQL WhereClause.</param>
		/// <returns>The value converted to SQL syntax form.</returns>
		public static string Sql(string value)
		{
			string retVal = null;

			if (value == null)
			{
				value = emptyStringSubstitute;
			}
			else
			{
				// Never trim strings here !!!
				//value = value.Trim();
				if (value.Length == 0)
				{
					value = emptyStringSubstitute;
				}//end if
			}//end if

			StringBuilder buf = new StringBuilder();
			if (value.IndexOf("'") < 0)
			{
				retVal = buf.Append("'").Append(value).Append("'").ToString();
			}
			else
			{
				retVal = buf.Append("'").Append(doubleSingleQuotes(value)).Append("'").ToString();
			}//end if
			return retVal;
		}//end sql
		/// <summary>
		/// Converts a boolean to its SQL form (true to "1").
		/// </summary>
		/// <param name="value">value A value we want to put in a SQL WhereClause.</param>
		/// <returns>The value converted to SQL syntax form.</returns>
		public static string Sql(bool value)
		{
			return value ? "1" : "0";
		}//end sql
		/// <summary>
		/// Converts a char to its SQL form ('a' to "'a'")
		/// </summary>
		/// <param name="value">value A value we want to put in a SQL WhereClause.</param>
		/// <returns>The value converted to SQL syntax form.</returns>
		public static string Sql(char value)
		{
			if (value == '\'')
			{
				return "''''";
			}
			else
			{
				return "'" + value + "'";
			}//end if
		}//end sql
		/// <summary>
		/// Converts an int to its SQL form (45 to "45").
		/// </summary>
		/// <param name="value">value A value we want to put in a SQL WhereClause.</param>
		/// <returns>The value converted to SQL syntax form.</returns>
		public static string Sql(int value)
		{
			return value.ToString();
		}//end sql
		/// <summary>
		/// Converts a long to its SQL form (45 to "45").
		/// </summary>
		/// <param name="value">value A value we want to put in a SQL WhereClause.</param>
		/// <returns>The value converted to SQL syntax form.</returns>
		public static string Sql(long value)
		{
			return value.ToString();
		}//end sql
		/// <summary>
		/// Converts a float to its SQL form (45.2 to "45.2").
		/// </summary>
		/// <param name="value">value A value we want to put in a SQL WhereClause.</param>
		/// <returns>The value converted to SQL syntax form.</returns>
		public static string Sql(float value)
		{
			return value.ToString();
		}//end sql

		/// <summary>
		/// Converts a double to its SQL form (45.2 to "45.2").
		/// </summary>
		/// <param name="value">value A value we want to put in a SQL WhereClause.</param>
		/// <returns>The value converted to SQL syntax form.</returns>
		public static string Sql(double value)
		{
			return value.ToString();
		}//end sql
		/// <summary>
		/// Converts a Date to its SQL form ("'1997-11-30 10.20.30'"). Uses both the date and time parts.
		/// </summary>
		/// <param name="value">value A value we want to put in a SQL WhereClause.</param>
		/// <returns>The value converted to SQL syntax form.</returns>
		public static string Sql(DateTime value) 
		{
			return Sql (value, true, true);
		}//end sql

		/// <summary>
		/// Converts a Date to its SQL form ("'1997-11-30 10.20.30'").
		/// </summary>
		/// <param name="value">value A value we want to put in a SQL WhereClause.</param>
		/// <param name="value">useDatePart Set true if you want the Date's Date part ("'1997-11-30'").</param>
		/// <param name="value">useTimePart Set true if you want the Date's Time part ("'10.20.30'").</param>
		/// <returns>The value converted to SQL syntax form.</returns>
		public static string Sql(DateTime value, bool useDatePart, bool useTimePart) 
		{
			string        retVal = "NULL";
			StringBuilder sb     = new StringBuilder();
			if (value != null) 
			{
				if (useDatePart && useTimePart) 
				{
					retVal = sb.Append("{ts '").Append(value.ToString()).Append("'}").ToString();
				}
				else if (useDatePart) 
				{
					retVal = sb.Append("{d '").Append(value.ToString()).Append("'}").ToString();
				}
				else if (useTimePart) 
				{
					retVal = sb.Append("{t '").Append(value.ToString()).Append("'}").ToString();
				}//end if
			}//end if
			return retVal;
		}//end sql		
		/// <summary>
		/// Returns the entered string where all occations of single quote (') are
		/// duplicated (') + ('). The reason for this is that single quotes are
		/// delimiters when using strings in SQL statements. If we would like to store
		/// a string that contains single quotes, we must double them.
		/// </summary>
		/// <param name="value">a value we want to put in a SQL WhereClause.</param>
		/// <returns>The value converted to SQL syntax form.</returns>
		private static string doubleSingleQuotes(string aString)
		{
			StringBuilder buf = new StringBuilder(aString);
			char singleQuote = '\'';
			for (int i = buf.Length - 1; i >= 0; i--)
			{
				if (buf[i] == singleQuote)
				{
					buf.Insert(i, singleQuote);
				}//end if
			}//end for
			return buf.ToString();
		}//end doubleSingleQuotes
		/// <summary>
		/// Returns the entered string where all occations of single quote (') are
		/// duplicated (') + ('). The reason for this is that single quotes are
		/// delimiters when using strings in SQL statements. If we would like to store
		/// a string that contains single quotes, we must double them.
		/// </summary>
		/// <param name="value">a value we want to put in a SQL WhereClause.</param>
		/// <returns>The value converted to SQL syntax form.</returns>
		public static string doubleDoubleQuotes(string aString)
		{
			StringBuilder buf = new StringBuilder(aString);
			char doubleQuote = '\"';
			for (int i = buf.Length - 1; i >= 0; i--)
			{
				if (buf[i] == doubleQuote)
				{
					buf.Insert(i, doubleQuote);
				}//end if
			}//end for
			return buf.ToString();
		}//end doubleSingleQuotes
		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static Double GetAsDouble(Object obj)
		{
			if (obj is DBNull)
			{
				return 0;
			}
			return (Double)obj;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static int GetAsInt(Object obj)
		{
			if (obj is DBNull)
			{
				return 0;
			}
			return (int)obj;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string GetAsStr(Object obj)
		{
			if (obj is DBNull)
			{
				return "";
			}
			return (string)obj;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static DateTime GetAsDateTime(Object obj)
		{
			if (obj is DBNull || obj == null || !(obj is DateTime))
			{
				return new DateTime(1970, 1, 1);
			}
			return (DateTime)obj;
		}
	}
}