using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ContosoUniversity.Models
{
    /// <summary>
    /// Concurrency Conflicts
    ///A concurrency conflict occurs when one user displays an entity's data in order to edit it,
    ///and then another user updates the same entity's data before the first user's change is written to the database.
    ///If you don't enable the detection of such conflicts, whoever updates the database last overwrites the other user's changes.
    ///In many applications, this risk is acceptable: if there are few users, or few updates, or if isn't really critical if some changes are overwritten,
    ///the cost of programming for concurrency might outweigh the benefit. In that case, you don't have to configure the application to handle concurrency conflicts.
    ///
    /// 1 - Pessimistic Concurrency (Locking)
    ///If your application does need to prevent accidental data loss in concurrency scenarios,
    ///one way to do that is to use database locks. This is called pessimistic concurrency.
    ///For example, before you read a row from a database, you request a lock for read-only or for update access.
    ///If you lock a row for update access, no other users are allowed to lock the row either for read-only or update access,
    ///because they would get a copy of data that's in the process of being changed. If you lock a row for read-only access,
    ///others can also lock it for read-only access but not for update.
    /// Managing locks has disadvantages. It can be complex to program. It requires significant database management resources,
    /// and it can cause performance problems as the number of users of an application increases. For these reasons,
    /// not all database management systems support pessimistic concurrency. The Entity Framework provides no built-in support for it,
    /// and this tutorial doesn't show you how to implement it.
    /// 
    /// 2 - Optimistic Concurrency
    /// The alternative to pessimistic concurrency is optimistic concurrency. Optimistic concurrency means allowing concurrency conflicts to happen,
    /// and then reacting appropriately if they do. For example, John runs the Departments Edit page,
    /// changes the Budget amount for the English department from $350,000.00 to $0.00.The alternative to pessimistic concurrency is optimistic concurrency.
    /// Optimistic concurrency means allowing concurrency conflicts to happen, and then reacting appropriately if they do. For example, John runs the Departments Edit page,
    /// changes the Budget amount for the English department from $350,000.00 to $0.00.
    /// Before John clicks Save, Jane runs the same page and changes the Start Date field from 9/1/2007 to 8/8/2013.
    /// John clicks Save first and sees his change when the browser returns to the Index page, then Jane clicks Save.
    /// What happens next is determined by how you handle concurrency conflicts. Some of the options include the following:
    /// 2 - 1 - You can keep track of which property a user has modified and update only the corresponding columns in the database.
    /// In the example scenario, no data would be lost, because different properties were updated by the two users.
    /// The next time someone browses the English department, they'll see both John's and Jane's changes — a start date of 8/8/2013 and a budget of Zero dollars.
    /// This method of updating can reduce the number of conflicts that could result in data loss,
    /// but it can't avoid data loss if competing changes are made to the same property of an entity.
    /// Whether the Entity Framework works this way depends on how you implement your update code. It's often not practical in a web application,
    /// because it can require that you maintain large amounts of state in order to keep track of all original property values for an entity as well as new values.
    /// Maintaining large amounts of state can affect application performance because it either requires server resources 
    /// or must be included in the web page itself (for example, in hidden fields) or in a cookie.
    ///  2 - 2 - You can let Jane's change overwrite John's change. The next time someone browses the English department,
    ///  they'll see 8/8/2013 and the restored $350,000.00 value. This is called a Client Wins or Last in Wins scenario.
    ///  (All values from the client take precedence over what's in the data store.) As noted in the introduction to this section,
    ///  if you don't do any coding for concurrency handling, this will happen automatically.
    ///  2 - 3 - You can prevent Jane's change from being updated in the database. Typically, you would display an error message,
    ///  show her the current state of the data, and allow her to reapply her changes if she still wants to make them.
    ///  This is called a Store Wins scenario. (The data-store values take precedence over the values submitted by the client.)
    ///  You'll implement the Store Wins scenario in this tutorial. This method ensures that no changes are overwritten without a user being alerted to what's happening.
    ///  
    /// Detecting Concurrency Conflicts
    /// You can resolve conflicts by handling OptimisticConcurrencyException exceptions that the Entity Framework throws.
    /// In order to know when to throw these exceptions, the Entity Framework must be able to detect conflicts. Therefore,
    /// you must configure the database and the data model appropriately. Some options for enabling conflict detection include the following:
    /// 1 - In the database table, include a tracking column that can be used to determine when a row has been changed.
    /// You can then configure the Entity Framework to include that column in the Where clause of SQL Update or Delete commands.
    /// The data type of the tracking column is typically rowversion. The rowversion value is a sequential number that's incremented each time the row is updated.
    /// In an Update or Delete command, the Where clause includes the original value of the tracking column (the original row version) .
    /// If the row being updated has been changed by another user, the value in the rowversion column is different than the original value,
    /// so the Update or Delete statement can't find the row to update because of the Where clause.
    /// When the Entity Framework finds that no rows have been updated by the Update or Delete command (that is, when the number of affected rows is zero),
    /// it interprets that as a concurrency conflict.
    /// 2 - Configure the Entity Framework to include the original values of every column in the table in the Where clause of Update and Delete commands.
    /// As in the first option, if anything in the row has changed since the row was first read,
    /// the Where clause won't return a row to update, which the Entity Framework interprets as a concurrency conflict.
    /// For database tables that have many columns, this approach can result in very large Where clauses,
    /// and can require that you maintain large amounts of state. As noted earlier, maintaining large amounts of state can affect application performance.
    /// Therefore this approach is generally not recommended, and it isn't the method used in this tutorial.
    /// If you do want to implement this approach to concurrency, you have to mark all non-primary-key properties in 
    /// the entity you want to track concurrency for by adding the ConcurrencyCheck attribute to them.
    /// That change enables the Entity Framework to include all columns in the SQL WHERE clause of UPDATE statements.
    /// </summary>
    public class Department
    {
        public int DepartmentID { get; set; }
        [StringLength(50, MinimumLength = 3)]        
        public string Name { get; set; }

        /// The Column Attribute : 
        /// 
        /// Earlier you used the Column attribute to change column name mapping.
        /// In the code for the Department entity, the Column attribute is being used to change SQL data type mapping
        /// so that the column will be defined using the SQL Server money type in the database:
        /// 
        /// Column mapping is generally not required, because the Entity Framework usually chooses the appropriate SQL Server data type based on the CLR type that you define for the property.
        /// The CLR decimal type maps to a SQL Server decimal type.
        /// But in this case you know that the column will be holding currency amounts, and the money data type is more appropriate for that.
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal Budget { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Note By convention, the Entity Framework enables cascade delete for non-nullable foreign keys and for many-to-many relationships.
        /// This can result in circular cascade delete rules, which will cause an exception when you try to add a migration.
        /// For example, if you didn't define the Department.InstructorID property as nullable, you'd get the following exception message:
        /// "The referential relationship will result in a cyclical reference that's not allowed.
        /// " If your business rules required InstructorID property to be non-nullable, you would have to use the following fluent API statement to disable cascade delete on the relationship:
        /// modelBuilder.Entity().HasRequired(d => d.Administrator).WithMany().WillCascadeOnDelete(false);
        /// </summary>
        public int? InstructorID { get; set; }

        /// <summary>
        /// The Timestamp attribute specifies that this column will be included in the Where clause of Update and Delete commands sent to the database.
        /// The attribute is called Timestamp because previous versions of SQL Server used a SQL timestamp data type before the SQL rowversion replaced it.
        /// The .Net type for rowversion is a byte array.
        /// If you prefer to use the fluent API, you can use the IsConcurrencyToken method to specify the tracking property
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; }
        public virtual Instructor Administrator { get; set; }
        public virtual ICollection<Course> Courses { get; set; }
    }
}