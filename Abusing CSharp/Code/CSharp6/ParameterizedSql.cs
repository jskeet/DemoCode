using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSharp6.ParameterizedSql.SqlFormattableString;

namespace CSharp6
{
    class ParameterizedSql
    {
        internal static class SqlFormattableString
        {
            internal static SqlCommand ToSqlCommand(SqlConnection conn, FormattableString formattableString)
            {
                object[] args = formattableString.GetArguments();
                var parameters = args.Select((value, position) => new InterpolatedSqlParameter(value, position)).ToArray();
                string sql = string.Format(formattableString.Format, parameters);
                var command = new SqlCommand(sql, conn);
                foreach (var p in parameters)
                {
                    command.Parameters.Add(p.ToSqlParameter());
                }
                return command;
            }
        }

        class InterpolatedSqlParameter : IFormattable
        {
            private readonly object value;
            private string format;
            internal string Name { get; }

            internal InterpolatedSqlParameter(object value, int position)
            {
                Name = $"@p{position}";
                this.value = value;
            }

            public string ToString(string format, IFormatProvider formatProvider)
            {
                this.format = format;
                return Name;
            }

            internal SqlParameter ToSqlParameter()
            {
                var ret = new SqlParameter(Name, value);
                if (!string.IsNullOrEmpty(format))
                {
                    // TODO: Add more handling such as precision
                    ret.SqlDbType = (SqlDbType)Enum.Parse(typeof(SqlDbType), format, true);
                }
                return ret;
            }
        }

        static void Main()
        {
            // Not really going to run anything right now...
            SqlConnection conn = null;
            string name = "Jon";
            int id = 10;
            using (var command = ToSqlCommand(conn, $"SELECT * FROM SomeTable WHERE name={name:ntext} AND id={id}"))
            {
                Console.WriteLine(command.CommandText);
                var p = command.Parameters[0];
                Console.WriteLine($"{p.ParameterName}: Type={p.SqlDbType}; Value={p.Value}");
                p = command.Parameters[1];
                Console.WriteLine($"{p.ParameterName}: Type={p.SqlDbType}; Value={p.Value}");
            }
        }
    }
}
