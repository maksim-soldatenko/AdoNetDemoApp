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

                connection.Close();
            }

            MultipleResultSet(cs);

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

        private static void MultipleResultSet(string cs)
        {
            var multiple = @"SELECT * FROM People WHERE Discriminator = 'Student'
                            SELECT * FROM People WHERE Discriminator = 'Instructor'";
            using (var connection = new SqlConnection(cs))
            {
                connection.Open();
                var command = new SqlCommand(multiple, connection);
                command.CommandType = CommandType.Text;
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine(String.Format("{0} - {1} - {2} - {3}", reader[0], reader[1], reader[2], reader["Discriminator"]));
                        }

                        reader.NextResult();
                    }
                }

                connection.Close();
            }
        }
    }
}
