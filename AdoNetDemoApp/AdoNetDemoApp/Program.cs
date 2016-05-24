using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.SqlAzure;

namespace AdoNetDemoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var cs = ConfigurationManager.ConnectionStrings["DemoSchoolContext"].ConnectionString;

            GetWithRetry(cs);

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

        private static void GetWithRetry(string connectionString)
        {
            var retryStrategy = new ExponentialBackoff("exp1", 5, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(40),
                TimeSpan.FromSeconds(1));

            var sts = new List<RetryStrategy>() {retryStrategy};
            var manager = new RetryManager(sts, "exp1");
            RetryManager.SetDefault(manager, false);

            //ITransientErrorDetectionStrategy
            //SqlAzureTransientErrorDetectionStrategy
            var retryPolicy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(retryStrategy);

            retryPolicy.Retrying += (sender, eventArgs) =>
            {
                Debugger.Launch();
                Console.WriteLine("Retrying, CurrentRetryCount = {0} , Exception = {1}", eventArgs.CurrentRetryCount, eventArgs.LastException.Message);
            };

            using (var reliableConnection = new ReliableSqlConnection(connectionString, retryPolicy, retryPolicy))
            {
                using (var connection = reliableConnection.Open())
                {
                    var command = new SqlCommand("SELECT * FROM People WHERE Discriminator = 'Student'", connection);

                    retryPolicy.ExecuteAction(() =>
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine(String.Format("{0} - {1} - {2} - {3}", reader[0], reader[1], reader[2], reader["Discriminator"]));
                            }
                        }
                    });
                }
            }
        }
    }
}
