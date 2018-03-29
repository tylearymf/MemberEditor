//-----------------------------------------------------------------------
// <copyright file="StringUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities
{
	using System;
	using System.Text;

    /// <summary>
    /// Not yet documented.
    /// </summary>
	public static class StringUtilities
	{
        /// <summary>
        /// Not yet documented.
        /// </summary>
        /// <param name="bytes">Not yet documented.</param>
        /// <param name="decimals">Not yet documented.</param>
        /// <returns>Not yet documented.</returns>
		public static string NicifyByteSize(int bytes, int decimals = 1)
		{
			StringBuilder builder = new StringBuilder();

			if (bytes < 0)
			{
				builder.Append('-');
				bytes = Math.Abs(bytes);
			}

			int decimalLength = 0;
			string m = null;
			if (bytes > 1000000000)
			{
				builder.Append(bytes / 1000000000);
				bytes -= (bytes / 1000000000) * 1000000000;
				decimalLength = 9;
				m = " GB";
			}
			else if (bytes > 1000000)
			{
				builder.Append(bytes / 1000000);
				bytes -= (bytes / 1000000) * 1000000;
				decimalLength = 6;
				m = " MB";
			}
			else if (bytes > 1000)
			{
				builder.Append(bytes / 1000);
				bytes -= (bytes / 1000) * 1000;
				decimalLength = 3;
				m = " KB";
			}
			else
			{
				builder.Append(bytes);
				decimals = 0;
				decimalLength = 0;
				m = " bytes";
			}

			if (decimals > 0 && decimalLength > 0 && bytes > 0)
			{
				string d = bytes.ToString().PadLeft(decimalLength, '0');
				d = d.Substring(0, decimals < d.Length ? decimals : d.Length).TrimEnd('0');

				if (d.Length > 0)
				{
					builder.Append('.');
					builder.Append(d);
				}
			}

			builder.Append(m);
			return builder.ToString();
		}
	}
}