using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ContosoUniversity.DAL;
using ContosoUniversity.Models;
using System.Data.Entity.Infrastructure;

namespace ContosoUniversity.Controllers
{
    public class DepartmentController : Controller
    {
        private SchoolContext db = new SchoolContext();

        // GET: Department 
        /// <summary>
        /// Four changes were applied to enable the Entity Framework database query to execute asynchronously:
        /// 1 - The method is marked with the async keyword, which tells the compiler to generate callbacks for parts of the method body
        /// and to automatically create the Task<ActionResult> object that is returned.
        /// 2 - The return type was changed from ActionResult to Task<ActionResult> .
        /// The Task<T> type represents ongoing work with a result of type T.
        /// 3 - The await keyword was applied to the web service call. When the compiler sees this keyword,
        /// behind the scenes it splits the method into two parts. The first part ends with the operation that is started asynchronously.
        /// The second part is put into a callback method that is called when the operation completes.
        /// 4 - The asynchronous version of the ToList extension method was called.
        /// 
        /// Some things to be aware of when you are using asynchronous programming with the Entity Framework:
        /// 1 - The async code is not thread safe. In other words, in other words,
        /// don't try to do multiple operations in parallel using the same context instance.
        /// 2 - If you want to take advantage of the performance benefits of async code,
        /// make sure that any library packages that you're using (such as for paging),
        /// also use async if they call any Entity Framework methods that cause queries to be sent to the database.
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index()
        {
            /// Why is the departments.ToList statement modified but not the departments = db.Departments statement?
            /// The reason is that only statements that cause queries or commands to be sent to the database are executed asynchronously.
            /// The departments = db.Departments statement sets up a query but the query is not executed until the ToList method is called.
            /// Therefore, only the ToList method is executed asynchronously.                       
            var departments = db.Departments.Include(d => d.Administrator);
            return View(await departments.ToListAsync());
        }

        // GET: Department/Details/5
        /// <summary>
        /// In the Details method and the HttpGet Edit and Delete methods,
        /// the Find method is the one that causes a query to be sent to the database,
        /// so that's the method that gets executed asynchronously
        /// 
        /// Performing Raw SQL Queries
        /// The Entity Framework Code First API includes methods that enable you to pass SQL commands directly to the database. You have the following options:
        /// 1 - Use the DbSet.SqlQuery method for queries that return entity types. The returned objects must be of the type expected by the DbSet object,
        /// and they are automatically tracked by the database context unless you turn tracking off. (See the following section about the AsNoTracking method.)
        /// 2 - Use the Database.SqlQuery method for queries that return types that aren't entities. The returned data isn't tracked by the database context,
        /// even if you use this method to retrieve entity types.
        /// 3 - Use the Database.ExecuteSqlCommand for non-query commands.
        /// 
        /// One of the advantages of using the Entity Framework is that it avoids tying your code too closely to a particular method of storing data.
        /// It does this by generating SQL queries and commands for you, which also frees you from having to write them yourself.
        /// But there are exceptional scenarios when you need to run specific SQL queries that you have manually created,
        /// and these methods make it possible for you to handle such exceptions.
        /// 
        /// As is always true when you execute SQL commands in a web application, you must take precautions to protect your site against SQL injection attacks.
        /// One way to do that is to use parameterized queries to make sure that strings submitted by a web page can't be interpreted as SQL commands.
        /// In this tutorial you'll use parameterized queries when integrating user input into a query.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Commenting out original code to show how to use a raw SQL query. 
            //Department department = await db.Departments.FindAsync(id);

            // Create and execute raw SQL query.
            string query = "SELECT * FROM Department WHERE DepartmentID = @p0";
            Department department = await db.Departments.SqlQuery(query, id).SingleOrDefaultAsync();

            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // GET: Department/Create
        /// <summary>
        /// In the Create , HttpPost Edit, and DeleteConfirmed methods,
        /// it is the SaveChanges method call that causes a command to be executed,
        /// not statements such as db.Departments.Add(department) which only cause entities in memory to be modified.
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName");
            return View();
        }

        // POST: Department/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "DepartmentID,Name,Budget,StartDate,InstructorID")] Department department)
        {
            if (ModelState.IsValid)
            {
                db.Departments.Add(department);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName", department.InstructorID);
            return View(department);
        }

        // GET: Department/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = await db.Departments.FindAsync(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName", department.InstructorID);
            return View(department);
        }

        // POST: Department/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        /// The view will store the original RowVersion value in a hidden field. When the model binder creates the department instance,
        /// that object will have the original RowVersion property value and the new values for the other properties,
        /// as entered by the user on the Edit page. Then when the Entity Framework creates a SQL UPDATE command,
        /// that command will include a WHERE clause that looks for a row that has the original RowVersion value.
        /// If no rows are affected by the UPDATE command (no rows have the original RowVersion value),
        /// the Entity Framework throws a DbUpdateConcurrencyException exception, and the code in the catch block gets the affected Department entity from the exception object. 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "DepartmentID, Name, Budget, StartDate, RowVersion, InstructorID")] Department department)
        {
            try
            {
                if (ModelState.IsValid) 
                { 
                    ValidateOneAdministratorAssignmentPerInstructor(department); 
                }

                if (ModelState.IsValid)
                {
                    db.Entry(department).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                /// This object has the new values entered by the user in its Entity property
                var clientValues = (Department)entry.Entity;
                var databaseEntry = entry.GetDatabaseValues();

                /// The GetDatabaseValues method returns null if someone has deleted the row from the database; otherwise,
                /// you have to cast the object returned to the Department class in order to access the Department properties.
                if (databaseEntry == null)
                {
                    ModelState.AddModelError(string.Empty, "Unable to save changes. The department was deleted by another user.");
                }
                else
                {
                    var databaseValues = (Department)databaseEntry.ToObject();
                    if (databaseValues.Name != clientValues.Name)
                        ModelState.AddModelError("Name", "Current value: " + databaseValues.Name);
                    if (databaseValues.Budget != clientValues.Budget)
                        ModelState.AddModelError("Budget", "Current value: " + String.Format("{0:c}", databaseValues.Budget));
                    if (databaseValues.StartDate != clientValues.StartDate)
                        ModelState.AddModelError("StartDate", "Current value: " + String.Format("{0:d}", databaseValues.StartDate));
                    if (databaseValues.InstructorID != clientValues.InstructorID)
                        ModelState.AddModelError("InstructorID", "Current value: " + db.Instructors.Find(databaseValues.InstructorID).FullName);
                    ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                    + "was modified by another user after you got the original value. The "
                    + "edit operation was canceled and the current values in the database "
                    + "have been displayed. If you still want to edit this record, click "
                    + "the Save button again. Otherwise click the Back to List hyperlink.");
                    /// Finally, the code sets the RowVersion value of the Department object to the new value retrieved from the database.
                    /// This new RowVersion value will be stored in the hidden field when the Edit page is redisplayed,
                    /// and the next time the user clicks Save, only concurrency errors that happen since the redisplay of the Edit page will be caught.
                    department.RowVersion = databaseValues.RowVersion;
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError(string.Empty, "Unable to save changes. Try again, and if the problem persists contact your system administrator.");
            }
            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName", department.InstructorID);
            return View(department);
        }

        // GET: Department/Delete/5
        /// <summary>
        /// For the Delete page, the Entity Framework detects concurrency conflicts caused by someone else editing the department in a similar manner.
        /// When the HttpGet Delete method displays the confirmation view, the view includes the original RowVersion value in a hidden field.
        /// That value is then available to the HttpPost Delete method that's called when the user confirms the deletion.
        /// When the Entity Framework creates the SQL DELETE command, it includes a WHERE clause with the original RowVersion value.
        /// If the command results in zero rows affected (meaning the row was changed after the Delete confirmation page was displayed),
        /// a concurrency exception is thrown, and the HttpGet Delete method is called with an error flag set to true
        /// in order to redisplay the confirmation page with an error message.It's also possible that zero rows were affected
        /// because the row was deleted by another user, so in that case a different error message is displayed.
        /// 
        /// The method accepts an optional parameter that indicates whether the page is being redisplayed after a concurrency error.
        /// If this flag is true, an error message is sent to the view using a ViewBag property.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="concurrencyError"></param>
        /// <returns></returns>
        public ActionResult Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            if (concurrencyError.GetValueOrDefault())
            {
                if (department == null)
                {
                    ViewBag.ConcurrencyErrorMessage = "The record you attempted to delete "
                    + "was deleted by another user after you got the original values. "
                    + "Click the Back to List hyperlink.";
                }
                else
                {
                    ViewBag.ConcurrencyErrorMessage = "The record you attempted to delete "
                    + "was modified by another user after you got the original values. "
                    + "The delete operation was canceled and the current values in the "
                    + "database have been displayed. If you still want to delete this "
                    + "record, click the Delete button again. Otherwise "
                    + "click the Back to List hyperlink.";
                }
            }
            return View(department);
        }


        // POST: Department/Delete/5
        /// <summary>
        /// In the scaffolded code that you just replaced, this method accepted only a record ID
        /// You've changed this parameter to a Department entity instance created by the model binder.
        /// This gives you access to the RowVersion property value in addition to the record key.
        /// You have also changed the action method name from DeleteConfirmed to Delete.
        /// The scaffolded code named the HttpPost Delete method DeleteConfirmed to give the HttpPost method a unique signature.
        /// ( The CLR requires overloaded methods to have different method parameters.) Now that the signatures are unique,
        /// you can stick with the MVC convention and use the same name for the HttpPost and HttpGet delete methods.
        /// </summary>
        /// <param name="department"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Department department)
        {
            try
            {
                db.Entry(department).State = EntityState.Deleted;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                return RedirectToAction("Delete", new { concurrencyError = true });
            }
            catch (DataException /* dex */)
            {
                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                return View(department);
            }
        }

        /// <summary>
        /// No-Tracking Queries
        /// When a database context retrieves table rows and creates entity objects that represent them,
        /// by default it keeps track of whether the entities in memory are in sync with what's in the database.
        /// The data in memory acts as a cache and is used when you update an entity.
        /// This caching is often unnecessary in a web application because context instances are typically short-lived (a new one is created and disposed for each request)
        /// and the context that reads an entity is typically disposed before that entity is used again.
        /// 
        /// You can disable tracking of entity objects in memory by using the AsNoTracking method.
        /// Typical scenarios in which you might want to do that include the following:
        /// </summary>
        /// <param name="department"></param>
        private void ValidateOneAdministratorAssignmentPerInstructor(Department department)
        {
            if (department.InstructorID != null)
            {
                Department duplicateDepartment = db.Departments
                                                   .Include("Administrator")
                                                   .Where(d => d.InstructorID == department.InstructorID)
                                                   .AsNoTracking()
                                                   .FirstOrDefault();

                if (duplicateDepartment != null && duplicateDepartment.DepartmentID != department.DepartmentID)
                {
                    string errorMessage = String.Format(
                    "Instructor {0} {1} is already administrator of the {2} department.",
                    duplicateDepartment.Administrator.FirstMidName,
                    duplicateDepartment.Administrator.LastName,
                    duplicateDepartment.Name);
                    ModelState.AddModelError(string.Empty, errorMessage);
                }
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
