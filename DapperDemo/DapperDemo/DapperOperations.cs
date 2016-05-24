using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DapperDemo.DomainModel;

namespace DapperDemo
{
    public class DapperOperations
    {
        private readonly string _cs;

        public DapperOperations()
        {
            _cs = ConfigurationManager.ConnectionStrings["DemoSchoolContext"].ConnectionString;
        }

        public void Create()
        {
            var student = new Student() {EnrollmentDate = DateTime.Now, FirstMidName = "Gupalo", LastName = "Vasyl"};

            using (var connection = new SqlConnection(_cs))
            {
                var students =
                    connection.Execute(
                        "INSERT INTO People (LastName, FirstMidName, EnrollmentDate, Discriminator) values (@lastName, @firstMidName, @enrollmentDate, 'Student')",
                        new
                        {
                            lastName = student.LastName,
                            firstMidName = student.FirstMidName,
                            enrollmentDate = student.EnrollmentDate
                        });

                Console.WriteLine("Added {0}", students);
            }
        }

        public void Read()
        {
            ReadAll();
            ReadFiltered("Li");
        }

        public void Update(string lastName)
        {
            using (var connection = new SqlConnection(_cs))
            {
                var student =
                    connection.Query<Student>(
                        "SELECT * FROM PEOPLE WHERE Discriminator = 'Student' AND LastName LIKE @lastName",
                        new {lastName = String.Format("%{0}%", lastName)}).FirstOrDefault();

                if (student != null)
                {
                    student.EnrollmentDate = DateTime.Now;

                    var upd = connection.Execute("UPDATE People SET EnrollmentDate = @enrollmentDate WHERE Id = @id",
                        new {enrollmentDate = student.EnrollmentDate, id = student.Id});

                    Console.WriteLine("Updated {0}", upd);

                }
            }
        }

        public void Delete(string lastName)
        {
            using (var connection = new SqlConnection(_cs))
            {
                var students =
                    connection.Execute(
                        "DELETE FROM PEOPLE WHERE Discriminator = 'Student' AND LastName LIKE @lastName",
                        new {lastName = String.Format("%{0}%", lastName)});

                Console.WriteLine("Deleted {0}", students);
            }
        }

        private void ReadFiltered(string lastName)
        {
            using (var connection = new SqlConnection(_cs))
            {
                var students =
                    connection.Query<Student>(
                        "SELECT * FROM PEOPLE WHERE Discriminator = 'Student' AND LastName LIKE @lastName",
                        new {lastName = String.Format("%{0}%", lastName)});

                foreach (var student in students)
                {
                    Console.WriteLine(String.Format("{0} - {1} - {2}", student.Id, student.FirstMidName, student.LastName));
                }
            }
        }

        private void ReadAll()
        {
            using (var connection = new SqlConnection(_cs))
            {
                var students = connection.Query<Student>("SELECT * FROM PEOPLE WHERE Discriminator = 'Student'");
                foreach (var student in students)
                {
                    Console.WriteLine(String.Format("{0} - {1} - {2}", student.Id, student.FirstMidName, student.LastName));
                }
            }
        }
    }
}
