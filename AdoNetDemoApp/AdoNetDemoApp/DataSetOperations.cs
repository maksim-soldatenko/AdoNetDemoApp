using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetDemoApp
{
    public class DataSetOperations
    {
        private readonly string _connectionString;
        private readonly DataSet _dataSet;

        private string _selectCommand = @"Select * from People where Discriminator = 'Student'
                                          Select * from Enrollments";

        public DataSetOperations(string connectionString)
        {
            _connectionString = connectionString;
            _dataSet = new DataSet();
        }

        public void InitDataSet()
        {
            using (var adapter = GetAdapter())
            {
                adapter.Fill(_dataSet, "Students");
            }
        }

        public void MakeSomeChanges()
        {
            var students = _dataSet.Tables["Students"];

            var vasyl = new List<DataRow>();
            for (int i = 0; i < students.Rows.Count; i++)
            {
                if (students.Rows[i]["LastName"].Equals("Vasyl"))
                {
                    students.Rows[i].Delete();
                }
            }

            //Needs valid UpdateCommand

            using (var adapter = GetAdapter())
            {
                adapter.Update(_dataSet, "People");
            }
        }

        private SqlDataAdapter GetAdapter()
        {
            var adapter = new SqlDataAdapter(_selectCommand, _connectionString);

            adapter.TableMappings.Add("People", "Students");
            adapter.TableMappings.Add("Students1", "Enrollments");

            return adapter;
        }
    }
}
