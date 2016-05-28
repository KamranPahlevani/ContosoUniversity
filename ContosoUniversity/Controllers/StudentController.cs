using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ContosoUniversity.DAL;
using ContosoUniversity.Models;
using PagedList;

namespace ContosoUniversity.Controllers
{
    public class StudentController : Controller
    {
        private SchoolContext db = new SchoolContext();

        // GET: Student
        /// <summary>
        /// This code receives a sortOrder parameter from the query string in the URL.
        /// The query string value is provided by ASP.NET MVC as a parameter to the action method.
        /// The parameter will be a string that's either "Name" or "Date",
        /// optionally followed by an underscore and the string "desc" to specify descending order.
        /// The default sort order is ascending.
        /// 
        /// The first time the Index page is requested, there's no query string.
        /// The students are displayed in ascending order by LastName, which is the default as established by the fall-through case in the switch statement.
        /// When the user clicks a column heading hyperlink, the appropriate sortOrder value is provided in the query string.
        /// 
        /// The two ViewBag variables are used so that the view can configure the column heading hyperlinks with the appropriate query string values
        /// 
        /// These are ternary statements. The first one specifies that if the sortOrder parameter is null or empty,
        /// ViewBag.NameSortParm should be set to "name_desc"; otherwise, it should be set to an empty string.
        /// 
        /// To add paging to the Students Index page, you'll start by installing the PagedList.Mvc NuGet package.
        /// The NuGet PagedList.Mvc package automatically installs the PagedList package as a dependency.
        /// The PagedList package installs a PagedList collection type and extension methods for IQueryable and IEnumerable collections.
        /// The extension methods create a single page of data in a PagedList collection out of your IQueryable or IEnumerable,
        /// and the PagedList collection provides several properties and methods that facilitate paging.
        /// The PagedList.Mvc package installs a paging helper that displays the paging buttons.
        /// 
        /// A ViewBag property provides the view with the current sort order,
        /// because this must be included in the paging links in order to keep the sort order the same while paging
        /// 
        /// Another property, ViewBag.CurrentFilter, provides the view with the current filter string.
        /// This value must be included in the paging links in order to maintain the filter settings during paging,
        /// and it must be restored to the text box when the page is redisplayed. If the search string is changed during paging,
        /// the page has to be reset to 1, because the new filter can result in different data to display.
        /// The search string is changed when a value is entered in the text box and the submit button is pressed.
        /// In that case, the searchString parameter is not null.
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter; 
            }
            ViewBag.CurrentFilter = searchString;

            var students = from s in db.Students
                           select s;

            if (!string.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.LastName.ToUpper().Contains(searchString.ToUpper()) || s.FirstMidName.ToUpper().Contains(searchString.ToUpper()));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:
                    students = students.OrderBy(s => s.LastName);
                    break;
            }

            int pageSize = 3; 
            int pageNumber = (page ?? 1);
            return View(students.ToPagedList(pageNumber, pageSize));
        }

        // GET: Student/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // GET: Student/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Student/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// (Model binder refers to the ASP.NET MVC functionality that makes it easier for you to work with data submitted by a form;
        /// a model binder converts posted form values to CLR types and passes them to the action method in parameters.
        /// 
        /// Security Note: The ValidateAntiForgeryToken attribute helps prevent cross-site request forgery attacks.
        /// It requires a corresponding Html.AntiForgeryToken() statement in the view, which you'll see later.
        /// 
        /// The Bind attribute protects against over-posting.
        /// For example, suppose the Student entity includes a Secret property that you don't want this web page to update.
        /// 
        /// Even if you don't have a Secret field on the web page, a hacker could use a tool such as fiddler, or write some JavaScript,
        /// to post a Secret form value. Without the Bind attribute limiting the fields that the model binder uses when it creates a Student instance,
        /// the model binder would pick up that Secret form value and use it to update the Student entity instance.
        /// Then whatever value the hacker specified for the Secret form field would be updated in your database.
        /// It's also possible to use the Exclude parameter to blacklist fields you want to exclude.
        /// The reason Include is more secure is that when you add a new property to the entity, the new field is not automatically protected by an Exclude list.
        /// 
        /// Another alternative approach, and one preferred by many, is to use only view models with model binding.
        /// The view model contains only the properties you want to bind. Once the MVC model binder has finished, you copy the view model properties to the entity instance.
        /// 
        /// Handling Transactions
        ///By default the Entity Framework implicitly implements transactions.
        ///In scenarios where you make changes to multiple rows or tables and then call SaveChanges,
        ///the Entity Framework automatically makes sure that either all of your changes succeed or all fail.
        ///If some changes are done first and then an error happens, those changes are automatically rolled back.
        /// </summary>
        /// <param name="student"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "LastName,FirstMidName,EnrollmentDate")] Student student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Students.Add(student);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (DataException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(student);
        }

        // GET: Student/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // POST: Student/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// This code is similar to what you saw in the HttpPost Create method.
        /// However, instead of adding the entity created by the model binder to the entity set,
        /// this code sets a flag on the entity indicating it has been changed. 
        /// When the SaveChanges method is called, the Modified flag causes the Entity Framework to create SQL statements to update the database row.
        /// All columns of the database row will be updated, including those that the user didn't change, and concurrency conflicts are ignored.
        /// 
        /// Entity States and the Attach and SaveChanges Methods
        /// The database context keeps track of whether entities in memory are in sync with their corresponding rows in the database,
        /// and this information determines what happens when you call the SaveChanges method.
        /// For example, when you pass a new entity to the Add method, that entity's state is set to Added.
        /// Then when you call the SaveChanges method, the database context issues a SQL INSERT command.
        /// 
        /// An entity may be in one of the following states:
        /// 1 - Added. The entity does not yet exist in the database. The SaveChanges method must issue an INSERT statement.
        /// 2 - Unchanged. Nothing needs to be done with this entity by the SaveChanges method. When you read an entity from the database, the entity starts out with this status.
        /// 3 - Modified. Some or all of the entity's property values have been modified. The SaveChanges method must issue an UPDATE statement.
        /// 4 - Deleted. The entity has been marked for deletion. The SaveChanges method must issue a DELETE statement. 
        /// 5 - Detached. The entity isn't being tracked by the database context.
        /// 
        /// In a desktop application, state changes are typically set automatically.
        /// In a desktop type of application, you read an entity and make changes to some of its property values.
        /// This causes its entity state to automatically be changed to Modified. Then when you call SaveChanges,
        /// the Entity Framework generates a SQL UPDATE statement that updates only the actual properties that you changed.
        /// 
        /// The disconnected nature of web apps doesn't allow for this continuous sequence.
        /// The DbContext that reads an entity is disposed after a page is rendered.
        /// When the HttpPost Edit action method is called, a new request is made and you have a new instance of the DbContext,
        /// so you have to manually set the entity state to Modified. Then when you call SaveChanges,
        /// the Entity Framework updates all columns of the database row, because the context has no way to know which properties you changed.
        /// 
        /// If you want the SQL Update statement to update only the fields that the user actually changed,
        /// you can save the original values in some way (such as hidden fields) so that they are available when the HttpPost Edit method is called.
        /// Then you can create a Student entity using the original values,
        /// call the Attach method with that original version of the entity,
        /// update the entity's values to the new values, and then call SaveChanges.
        /// For more information, see Entity states and SaveChanges and Local Data in the MSDN Data Developer Center.
        /// </summary>
        /// <param name="student"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,LastName,FirstMidName,EnrollmentDate")] Student student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(student).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (DataException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            } 
            return View(student);
        }

        // GET: Student/Delete/5
        /// <summary>
        /// This code accepts an optional parameter that indicates whether the method was called after a failure to save changes.
        /// This parameter is false when the HttpGet Delete method is called without a previous failure.
        /// When it is called by the HttpPost Delete method in response to a database update error,
        /// the parameter is true and an error message is passed to the view.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="saveChangesError"></param>
        /// <returns></returns>
        public ActionResult Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // POST: Student/Delete/5
        /// <summary>
        /// You have also changed the action method name from DeleteConfirmed to Delete.
        /// The scaffolded code named the HttpPost Delete method DeleteConfirmed to give the HttpPost method a unique signature.
        /// ( The CLR requires overloaded methods to have different method parameters.)
        /// 
        /// As noted, the HttpGet Delete method doesn't delete the data.
        /// Performing a delete operation in response to a GET request 
        /// (or for that matter, performing any edit operation, create operation, or any other operation that changes data) creates a security risk.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                //Student student = db.Students.Find(id);
                //db.Students.Remove(student);

                //If improving performance in a high-volume application is a priority,
                //you could avoid an unnecessary SQL query to retrieve the row by replacing the lines of code that call the Find and Remove methods with the following code:
                Student studentToDelete = new Student() { ID = id };
                db.Entry(studentToDelete).State = EntityState.Deleted;
                db.SaveChanges();
            }
            catch (DataException/* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            } 
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Ensuring that Database Connections Are Not Left Open
        /// To make sure that database connections are properly closed and the resources they hold freed up,
        /// you have to dispose the context instance when you are done with it.
        /// The base Controller class already implements the IDisposable interface,
        /// so this code simply adds an override to the Dispose(bool) method to explicitly dispose the context instance.
        /// </summary>
        /// <param name="disposing"></param>
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
