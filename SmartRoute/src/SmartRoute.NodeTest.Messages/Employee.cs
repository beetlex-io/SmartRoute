using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtoBuf;
namespace SmartRoute.NodeTest.Messages
{
    [ProtoContract]
    public class Employee
    {
        [ProtoMember(1)]
        public int EmployeeID
        {
            get;
            set;
        }
        [ProtoMember(2)]
        public string LastName
        {
            get;
            set;
        }
        [ProtoMember(3)]
        public string FirstName
        {
            get;
            set;
        }
        [ProtoMember(4)]
        public string Title
        {
            get;
            set;
        }
        [ProtoMember(5)]
        public string TitleOfCourtesy { get; set; }
        [ProtoMember(6)]
        public DateTime BirthDate { get; set; }
        [ProtoMember(7)]
        public DateTime HireDate { get; set; }
        [ProtoMember(8)]
        public string Address { get; set; }
        [ProtoMember(9)]
        public string City { get; set; }
        [ProtoMember(10)]
        public string Region { get; set; }
        [ProtoMember(11)]
        public string PostalCode { get; set; }
        [ProtoMember(12)]
        public string Country { get; set; }
        [ProtoMember(13)]
        public string HomePhone { get; set; }
        [ProtoMember(14)]
        public string Extension { get; set; }
        [ProtoMember(15)]
        public string Photo { get; set; }
        [ProtoMember(16)]
        public string Notes { get; set; }



        public static Employee GetEmployee()
        {
            Employee result = new Employee();
            result.EmployeeID = 1;
            result.LastName = "Davolio";
            result.FirstName = "Nancy";
            result.Title = "Sales Representative";
            result.TitleOfCourtesy = "MS.";
            result.BirthDate = DateTime.Parse("1968-12-08");
            result.HireDate = DateTime.Parse("1992-05-01");
            result.Address = "507 - 20th Ave. E.Apt. 2A";
            result.City = "Seattle";
            result.Region = "WA";
            result.PostalCode = "98122";
            result.Country = "USA";
            result.HomePhone = "(206) 555-9857";
            result.Extension = "5467";
            result.Photo = "EmpID1.bmp";
            //result.Notes = "Education includes a BA in psychology from Colorado State University.  She also completed &quot;The Art of the Cold Call.&quot;  Nancy is a member of Toastmasters International.";
            result.Notes = "Education includes";
            return result;
        }

        public static List<Employee> GetEmployees(int count)
        {
            List<Employee> result = new List<Employee>();
            for (int i = 0; i < count; i++)
            {
                Employee item = new Employee();
                item.EmployeeID = 1;
                item.LastName = "Davolio";
                item.FirstName = "Nancy";
                item.Title = "Sales Representative";
                item.TitleOfCourtesy = "MS.";
                item.BirthDate = DateTime.Parse("1968-12-08");
                item.HireDate = DateTime.Parse("1992-05-01");
                item.Address = "507 - 20th Ave. E.Apt. 2A";
                item.City = "Seattle";
                item.Region = "WA";
                item.PostalCode = "98122";
                item.Country = "USA";
                item.HomePhone = "(206) 555-9857";
                item.Extension = "5467";
                item.Photo = "EmpID1.bmp";
                item.Notes = "Education includes a BA in psychology from Colorado State University.  She also completed &quot;The Art of the Cold Call.&quot;  Nancy is a member of Toastmasters International.";
                result.Add(item);
            }
            return result;
        }
    }
}
