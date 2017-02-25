// Copyright 2017 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using static System.FormattableString;

namespace StringInterpolation
{
    class ParameterizedSql
    {
        static void Main()
        {
            // Not really going to run anything right now...
            SqlConnection conn = null;
            string name = "Jon";
            int id = 10;
            using (var command = conn.NewSqlCommand($"SELECT * FROM SomeTable WHERE name={name:nvarchar} AND id={id}"))
            {
                Console.WriteLine(command.CommandText);
                var p = command.Parameters[0];
                Console.WriteLine($"{p.ParameterName}: Type={p.SqlDbType}; Value={p.Value}");
                p = command.Parameters[1];
                Console.WriteLine($"{p.ParameterName}: Type={p.SqlDbType}; Value={p.Value}");
            }
        }
    }

    public static class SqlFormattableString
    {
        public static SqlCommand NewSqlCommand(
            this SqlConnection conn, FormattableString formattableString)
        {
            // formattableString.Format will be
            // "SELECT * FROM SomeTable WHERE name={0:nvarchar} AND id={1}"
            // formattableString.GetArguments() will return { "Jon", 10 }
            SqlParameter[] sqlParameters = formattableString.GetArguments()
                .Select((value, position) =>
                    new SqlParameter(Invariant($"@p{position}"), value))
                .ToArray();
            object[] formatArguments = sqlParameters
                .Select(p => new FormatCapturingParameter(p))
                .ToArray();
            string sql = string.Format(formattableString.Format, formatArguments);
            var command = new SqlCommand(sql, conn);
            command.Parameters.AddRange(sqlParameters);
            return command;
        }

        private class FormatCapturingParameter : IFormattable
        {
            private readonly SqlParameter parameter;

            internal FormatCapturingParameter(SqlParameter parameter)
            {
                this.parameter = parameter;
            }

            public string ToString(string format, IFormatProvider formatProvider)
            {
                // format will be "nvarchar" for p0, and null (or "") for p1
                if (!string.IsNullOrEmpty(format))
                {
                    parameter.SqlDbType =
                        (SqlDbType) Enum.Parse(typeof(SqlDbType), format, true);
                }
                return parameter.ParameterName;
            }
        }
    }
}
