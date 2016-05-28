using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ContosoUniversity.Models
{
    public class Instructor:Person
    {
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Hire Date")]
        public DateTime HireDate { get; set; }

        /// <summary>
        /// The Courses and OfficeAssignment properties are navigation properties.
        /// As was explained earlier, they are typically defined as virtual so that they can take advantage of an Entity
        /// 
        /// Framework feature called lazy loading. In addition, if a navigation property can hold multiple entities,
        /// its type must implement the ICollection<T> Interface. For example IList<T> qualifies but not IEnumerable<T> because IEnumerable<T> doesn't implement Add.
        /// </summary>
        public virtual ICollection<Course> Courses { get; set; }

        /// <summary>
        /// The Instructor entity has a nullable OfficeAssignment navigation property (because an instructor might not have an office assignment),
        /// and the OfficeAssignment entity has a non-nullable Instructor navigation property (because an office assignment can't exist without an instructor -- InstructorID is non-nullable).
        /// When an Instructor entity has a related OfficeAssignment entity, each entity will have a reference to the other one in its navigation property.
        /// 
        /// The following code provides an example of how you could have used fluent API instead of attributes to specify the relationship between the Instructor and OfficeAssignment entities:
        /// modelBuilder.Entity<Instructor>()
        ///             .HasOptional(p => p.OfficeAssignment)
        ///             .WithRequired(p => p.Instructor);
        /// </summary>
        public virtual OfficeAssignment OfficeAssignment { get; set; }
    }
}