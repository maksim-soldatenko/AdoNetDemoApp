using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetDemoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var cs = ConfigurationManager.ConnectionStrings["DemoSchoolContext"].ConnectionString;

            using (var connection = new SqlConnection(cs))
            {
                connection.Open();
                var command = new SqlCommand("Insert into People values ('Gupalo','Vasyl',null,'10.25.2015','Student')", connection);
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();

                command.CommandText = "Select * from People";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(String.Format("{0} - {1} - {2}", reader[0], reader[1], reader[2]));
                    }
                }

                connection.Close();
            }

            Console.WriteLine("Make the same with DataSet? Y/N");
            var ans = Console.ReadLine();
            if (ans.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
            {
                var dataset = new DataSetOperations(cs);
                dataset.InitDataSet();
                dataset.MakeSomeChanges();
            }

            Console.ReadKey();
        }
    }
}
