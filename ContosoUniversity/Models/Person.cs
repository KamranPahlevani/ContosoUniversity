using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    /// <summary>
    /// There are several ways this inheritance structure could be represented in the database.
    /// You could have a Person table that includes information about both students and instructors in a single table.
    /// Some of the columns could apply only to instructors (HireDate), some only to students (EnrollmentDate),
    /// some to both (LastName, FirstName). Typically, you'd have a discriminator column to indicate which type each row represents.
    /// For example, the discriminator column might have "Instructor" for instructors and "Student" for students.
    /// This pattern of generating an entity inheritance structure from a single database table is called table-per-hierarchy (TPH) inheritance.
    /// 
    /// An alternative is to make the database look more like the inheritance structure.
    /// For example, you could have only the name fields in the Person table and have separate Instructor and Student tables with the date fields.
    /// This pattern of making a database table for each entity class is called table per type (TPT) inheritance.
    /// 
    /// Yet another option is to map all non-abstract types to individual tables. All properties of a class, including inherited properties,
    /// map to columns of the corresponding table. This pattern is called Table-per-Concrete Class (TPC) inheritance.
    /// If you implemented TPC inheritance for the Person, Student, and Instructor classes as shown earlier,
    /// the Student and Instructor tables would look no different after implementing inheritance than they did before.
    /// 
    /// TPC and TPH inheritance patterns generally deliver better performance in the Entity Framework than TPT inheritance patterns,
    /// because TPT patterns can result in complex join queries.
    /// </summary>
    public abstract class Person
    {
        public int ID { get; set; }
        /// <summary>
        /// The StringLengthAttribute :
        /// 
        /// You can also specify data validation rules and validation error messages using attributes.
        /// The StringLength attribute sets the maximum length in the database and provides client side and server side validation for ASP.NET MVC.
        /// You can also specify the minimum string length in this attribute, but the minimum value has no impact on the database schema.
        /// 
        /// Suppose you want to ensure that users don't enter more than 50 characters for a name.
        /// To add this limitation, add StringLength attributes to the LastName and FirstMidName properties
        /// 
        /// The StringLength attribute won't prevent a user from entering white space for a name.
        /// You can use the RegularExpression attribute to apply restrictions to the input.
        /// For example the following code requires the first character to be upper case and the remaining characters to be alphabetical
        /// 
        /// The MaxLength attribute provides similar functionality to the StringLength attribute but doesn't provide client side validation.
        /// 
        /// After Add this Validation Run the application and click the Students tab. You get the following error:
        /// The model backing the 'SchoolContext' context has changed since the database was created.
        /// Consider using Code First Migrations to update the database (http://go.microsoft.com/fwlink/?LinkId=238269).
        /// 
        /// The database model has changed in a way that requires a change in the database schema, and Entity Framework detected that.
        /// You'll use migrations to update the schema without losing any data that you added to the database by using the UI.
        /// If you changed data that was created by the Seed method,
        /// that will be changed back to its original state because of the AddOrUpdate method that you're using in the Seed method.
        /// (AddOrUpdate is equivalent to an "upsert" operation from database terminology.)
        /// 
        /// In the Package Manager Console (PMC), enter the following commands:
        /// add-migration MaxLengthOnNames 
        /// update-database
        /// ---------------------------------------------------------------------------------------------------------------------------------
        /// The Required Attribute :
        /// 
        /// The Required attribute makes the name properties required fields.
        /// The Required attribute is not needed for value types such as DateTime, int, double, and float.
        /// Value types cannot be assigned a null value, so they are inherently treated as required fields.
        /// You could remove the Required attribute and replace it with a minimum length parameter for the StringLength attribute:
        /// </summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]

        /// The Column Attribute :
        /// 
        /// You can also use attributes to control how your classes and properties are mapped to the database.
        /// Suppose you had used the name FirstMidName for the first-name field because the field might also contain a middle name.
        /// But you want the database column to be named FirstName, because users who will be writing ad-hoc queries against the database are accustomed to that name.
        /// To make this mapping, you can use the Column attribute.
        /// 
        /// The Column attribute specifies that when the database is created, the column of the Student table that maps to the FirstMidName property will be named FirstName.
        /// In other words, when your code refers to Student.FirstMidName, the data will come from or be updated in the FirstName column of the Student table.
        /// If you don't specify column names, they are given the same name as the property name.
        /// 
        /// The Column attribute specifies that when the database is created,
        /// the column of the Student table that maps to the FirstMidName property will be named FirstName.
        /// In other words, when your code refers to Student.FirstMidName, the data will come from or be updated in the FirstName column of the Student table.
        /// If you don't specify column names, they are given the same name as the property name.
        ///
        ///The addition of the Column attribute changes the model backing the SchoolContext,
        ///so it won't match the database. Enter the following commands in the PMC to create another migration:
        /// add-migration ColumnFirstName update-database
        public string LastName { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        [Column("FirstName")]
        [Display(Name = "First Name")]
        public string FirstMidName { get; set; }
        [Display(Name = "Full Name")]
        public string FullName
        {
            get
            {
                return LastName + ", " + FirstMidName;
            }
        }
    }
}