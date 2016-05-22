namespace ContosoUniversity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        /// <summary>
        /// Migrations calls the Up method to implement the data model changes for a migration.
        /// When you enter a command to roll back the update, Migrations calls the Down method.
        /// 
        /// This is the initial migration that was created when you entered the add-migration InitialCreate command.
        /// The parameter (InitialCreate in the example) is used for the file name and can be whatever you want;
        /// you typically choose a word or phrase that summarizes what is being done in the migration.
        /// For example, you might name a later migration "AddDepartmentTable".
        /// 
        /// If you created the initial migration when the database already exists,
        /// the database creation code is generated but it doesn't have to run because the database already matches the data model.
        /// When you deploy the app to another environment where the database doesn't exist yet, this code will run to create your database,
        /// so it's a good idea to test it first. That's why you changed the name of the database in the connection string earlier -- 
        /// so that migrations can create a new one from scratch.
        /// 
        /// The update-database command runs the Up method to create the database and then it runs the Seed method to populate the database.
        /// The same process will run automatically in production after you deploy the application, as you'll see in the following section.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.Course",
                c => new
                    {
                        CourseID = c.Int(nullable: false),
                        Title = c.String(),
                        Credits = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CourseID);
            
            CreateTable(
                "dbo.Enrollment",
                c => new
                    {
                        EnrollmentID = c.Int(nullable: false, identity: true),
                        CourseID = c.Int(nullable: false),
                        StudentID = c.Int(nullable: false),
                        Grade = c.Int(),
                    })
                .PrimaryKey(t => t.EnrollmentID)
                .ForeignKey("dbo.Course", t => t.CourseID, cascadeDelete: true)
                .ForeignKey("dbo.Student", t => t.StudentID, cascadeDelete: true)
                .Index(t => t.CourseID)
                .Index(t => t.StudentID);
            
            CreateTable(
                "dbo.Student",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        LastName = c.String(),
                        FirstMidName = c.String(),
                        EnrollmentDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Enrollment", "StudentID", "dbo.Student");
            DropForeignKey("dbo.Enrollment", "CourseID", "dbo.Course");
            DropIndex("dbo.Enrollment", new[] { "StudentID" });
            DropIndex("dbo.Enrollment", new[] { "CourseID" });
            DropTable("dbo.Student");
            DropTable("dbo.Enrollment");
            DropTable("dbo.Course");
        }
    }
}
