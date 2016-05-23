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
            using (var adapter = new SqlDataAdapter(_selectCommand, _connectionString))
            {
                adapter.TableMappings.Add("People", "Students");
                adapter.TableMappings.Add("Students1", "Enrollments");

                adapter.Fill(_dataSet, "Students");
            }

        }

        public void MakeSomeChanges()
        {
            var students = _dataSet.Tables["Students"];

            for (int i = 0; i < students.Rows.Count; i++)
            {
                if (students.Rows[i]["LastName"].Equals("Gupalo"))
                {
                    students.Rows[i].Delete(); //Delete
                }
            }

            //Insert
            var newStudent = students.NewRow();
            newStudent["LastName"] = "Komander";
            newStudent["FirstMidName"] = "Vovan";
            newStudent["HireDate"] = DBNull.Value;
            newStudent["EnrollmentDate"] = DateTime.Now;
            newStudent["Discriminator"] = "Student";
            students.Rows.Add(newStudent);

            //Update
            students.Rows[0]["LastName"] = "Snork";

            
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var adapter = GetStudentsAdapter(connection))
                {
                    var i = adapter.Update(_dataSet, "People");
                }
            }
        }

        private SqlDataAdapter GetStudentsAdapter(SqlConnection connection)
        {
            var selectCommand = new SqlCommand("Select * from People where Discriminator = 'Student'");
            selectCommand.Connection = connection;
            var adapter = new SqlDataAdapter(selectCommand);
            adapter.TableMappings.Add("People", "Students");
            
            var insert = new SqlCommand("INSERT INTO PEOPLE VALUES(@lastName, @firstMidName, NULL, @enrollmentDate, 'Student')");
            insert.Parameters.AddRange(new []{LastName(), FirstMidName(), EnrollmentDate()});
            insert.Connection = connection;

            var update = new SqlCommand("UPDATE PEOPLE SET LastName = @lastName, FirstMidName = @FirstMidName, EnrollmentDate = @enrollmentDate WHERE Id = @id");
            update.Parameters.AddRange(new[] {Id(), LastName(), FirstMidName(), EnrollmentDate()});
            update.Connection = connection;

            var delete = new SqlCommand("DELETE FROM PEOPLE WHERE Id = @id");
            delete.Parameters.Add(Id());
            delete.Connection = connection;

            adapter.DeleteCommand = delete;
            adapter.InsertCommand = insert;
            adapter.UpdateCommand = update;

            return adapter;
        }

        public SqlParameter EnrollmentDate()
        {
            var enrollmentDate = new SqlParameter("@enrollmentDate", SqlDbType.DateTime);
            enrollmentDate.SourceColumn = "EnrollmentDate";
            return enrollmentDate;
        }

        public SqlParameter FirstMidName()
        {
            var firstMidName = new SqlParameter("@firstMidName", SqlDbType.NVarChar);
            firstMidName.SourceColumn = "FirstMidName";
            return firstMidName;
        }

        public SqlParameter LastName()
        {
            var lastName = new SqlParameter("@lastName", SqlDbType.NVarChar);
            lastName.SourceColumn = "LastName";
            return lastName;
        }

        public SqlParameter Id()
        {
            var id = new SqlParameter("@id", SqlDbType.Int);
            id.SourceColumn = "Id";
            id.SourceVersion = DataRowVersion.Original;
            return id;
        }
    }
}
