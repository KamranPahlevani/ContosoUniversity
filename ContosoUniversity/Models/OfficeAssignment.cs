using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    public class OfficeAssignment
    {
        /// <summary>
        /// The Key Attribute :
        /// 
        /// There's a one-to-zero-or-one relationship between the Instructor and the OfficeAssignment entities.
        /// An office assignment only exists in relation to the instructor it's assigned to,
        /// and therefore its primary key is also its foreign key to the Instructor entity.
        /// But the Entity Framework can't automatically recognize InstructorID as the primary key of this entity 
        /// because its name doesn't follow the ID or classnameID naming convention. Therefore, the Key attribute is used to identify it as the key:
        /// 
        /// You can also use the Key attribute if the entity does have its own primary key but you want to name the property something different than classnameID or ID.
        /// By default EF treats the key as non-database-generated because the column is for an identifying relationship.
        /// -------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// The ForeignKey Attribute :
        /// 
        /// When there is a one-to-zero-or-one relationship or a one-to-one relationship between two entities (such as between OfficeAssignment and Instructor),
        /// EF can't work out which end of the relationship is the principal and which end is dependent.
        /// One-to-one relationships have a reference navigation property in each class to the other class.
        /// The ForeignKey Attribute can be applied to the dependent class to establish the relationship.
        /// If you omit the ForeignKey Attribute, you get the following error when you try to create the migration:
        /// </summary>
        [Key]
        [ForeignKey("Instructor")]
        public int InstructorID { get; set; }

        [StringLength(50)]
        [Display(Name = "Office Location")]
        public string Location { get; set; }

        /// <summary>
        /// You could put a [Required] attribute on the Instructor navigation property to specify that there must be a related instructor,
        /// but you don't have to do that because the InstructorID foreign key (which is also the key to this table) is non-nullable.
        /// </summary>
        public virtual Instructor Instructor { get; set; }
    }
}