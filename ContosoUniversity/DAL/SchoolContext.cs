using ContosoUniversity.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
namespace ContosoUniversity.DAL
{
    /// <summary>
    /// Lazy, Eager, and Explicit Loading of Related Data :
    /// There are several ways that the Entity Framework can load related data into the navigation properties of an entity:
    /// 
    /// 1 - Lazy loading.
    /// When the entity is first read, related data isn't retrieved.
    /// However, the first time you attempt to access a navigation property,
    /// the data required for that navigation property is automatically retrieved.
    /// This results in multiple queries sent to the database — one for the entity itself and one each time that related data for the entity must be retrieved.
    /// The DbContext class enables lazy loading by default.
    /// 
    /// 2 - Eager loading.
    /// When the entity is read, related data is retrieved along with it.
    /// This typically results in a single join query that retrieves all of the data that's needed.
    /// You specify eager loading by using the Include method.
    /// 
    /// 3 - Explicit loading.
    /// This is similar to lazy loading, except that you explicitly retrieve the related data in code;
    /// it doesn't happen automatically when you access a navigation property.
    /// You load related data manually by getting the object state manager entry for an entity and calling the Collection.
    /// Load method for collections or the Reference.Load method for properties that hold a single entity.
    ///  you'd replace Collection(x => x.Courses) with Reference(x => x.Administrator).
    ///  Typically you'd use explicit loading only when you've turned lazy loading off.
    ///  
    /// Because they don't immediately retrieve the property values, lazy loading and explicit loading are also both known as deferred loading.
    /// 
    /// If you know you need related data for every entity retrieved, eager loading often offers the best performance,
    /// because a single query sent to the database is typically more efficient than separate queries for each entity retrieved.
    /// For example, in the above examples, suppose that each department has ten related courses.
    /// The eager loading example would result in just a single (join) query and a single round trip to the database.
    /// The lazy loading and explicit loading examples would both result in eleven queries and eleven round trips to the database.
    /// The extra round trips to the database are especially detrimental to performance when latency is high.
    /// 
    /// On the other hand, in some scenarios lazy loading is more efficient.
    /// Eager loading might cause a very complex join to be generated, which SQL Server can't process efficiently.
    /// Or if you need to access an entity's navigation properties only for a subset of a set of the entities you're processing,
    /// lazy loading might perform better because eager loading would retrieve more data than you need.
    /// If performance is critical, it's best to test performance both ways in order to make the best choice.
    /// 
    /// Lazy loading can mask code that causes performance problems.
    /// For example, code that doesn't specify eager or explicit loading but processes a high volume of entities
    /// and uses several navigation properties in each iteration might be very inefficient (because of many round trips to the database).
    /// An application that performs well in development using an on premise SQL server might have performance problems
    /// when moved to Windows Azure SQL Database due to the increased latency and lazy loading.
    /// Profiling the database queries with a realistic test load will help you determine if lazy loading is appropriate.
    /// For more information see Demystifying Entity Framework Strategies: Loading Related Data and Using the Entity Framework to Reduce Network Latency to SQL Azure.
    /// 
    /// Disable lazy loading before serialization
    /// If you leave lazy loading enabled during serialization, you can end up querying significantly more data than you intended.
    /// Serialization generally works by accessing each property on an instance of a type. Property access triggers lazy loading,
    /// and those lazy loaded entities are serialized. The serialization process then accesses each property of the lazy-loaded entities,
    /// potentially causing even more lazy loading and serialization. To prevent this run-away chain reaction, turn lazy loading off before you serialize an entity.
    /// 
    /// Serialization can also be complicated by the proxy classes that the Entity Framework uses, as explained in the Advanced Scenarios tutorial.
    /// One way to avoid serialization problems is to serialize data transfer objects (DTOs) instead of entity objects,
    /// as shown in the Using Web API with Entity Framework tutorial.
    /// If you don't use DTOs, you can disable lazy loading and avoid proxy issues by disabling proxy creation.
    /// 
    /// Here are some other ways to disable lazy loading:
    /// 1 - For specific navigation properties, omit the virtual keyword when you declare the property.
    /// 2 - For all navigation properties, set LazyLoadingEnabled to false, put the following code in the constructor of your context class:
    /// this.Configuration.LazyLoadingEnabled = false;
    /// 
    /// Automatic change detection
    /// The Entity Framework determines how an entity has changed (and therefore which updates need to be sent to the database) by comparing
    /// the current values of an entity with the original values. The original values are stored when the entity is queried or attached.
    /// Some of the methods that cause automatic change detection are the following:
    /// DbSet.Find
    /// DbSet.Local
    /// DbSet.Remove
    /// DbSet.Add
    /// DbSet.Attach
    /// DbContext.SaveChanges
    /// DbContext.GetValidationErrors
    /// DbContext.Entry
    /// DbChangeTracker.Entries
    /// If you're tracking a large number of entities and you call one of these methods many times in a loop,
    /// you might get significant performance improvements by temporarily turning off automatic change detection using the AutoDetectChangesEnabled property.
    /// For more information, see Automatically Detecting Changes on MSDN.
    /// 
    /// Automatic validation
    /// When you call the SaveChanges method, by default the Entity Framework validates the data in all properties of all changed entities before updating the database.
    /// If you've updated a large number of entities and you've already validated the data,
    /// this work is unnecessary and you could make the process of saving the changes take less time by temporarily turning off validation.
    /// You can do that using the ValidateOnSaveEnabled property. For more information, see Validation on MSDN.
    /// </summary>
    public class SchoolContext : DbContext
    {
        public SchoolContext()
            : base("SchoolContext")
        { }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<OfficeAssignment> OfficeAssignments { get; set; }
        public DbSet<Person> People { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            ///For the many-to-many relationship between the Instructor and Course entities,
            ///the code specifies the table and column names for the join table.
            ///Code First can configure the many-to-many relationship for you without this code,
            ///but if you don't call it, you will get default names such as InstructorInstructorID for the InstructorID column.
            modelBuilder.Entity<Course>()
                        .HasMany(c => c.Instructors).WithMany(i => i.Courses)
                        .Map(t => t.MapLeftKey("CourseID")
                                   .MapRightKey("InstructorID")
                                   .ToTable("CourseInstructor")
                             );
            /// This code instructs Entity Framework to use stored procedures for insert, update, and delete operations on the Department entity.
            modelBuilder.Entity<Department>()
                        .MapToStoredProcedures()
                        .Property(p => p.RowVersion)
                        .IsConcurrencyToken();
        }
    }
}