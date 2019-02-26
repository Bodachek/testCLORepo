using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLO365.Models
{
    public class MetadataModel
    {
        public List<Technology> Technologies { get; set; }
        public List<Category> Categories { get; set; }
        public List<Audience> Audiences { get; set; }
        public List<string> Sources { get; set; }
        public List<string> Levels { get; set; }
        public List<string> StatusTag { get; set; }
    }

    public class Technology
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public List<object> Subjects { get; set; }
    }

    public class SubCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Security { get; set; }
        public string Image { get; set; }
        public string Type { get; set; }
        public string Technology { get; set; }
        public List<SubCategorySubj1> SubCategories { get; set; }
    }

    public class SubCategorySubj1
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Security { get; set; }
        public string Image { get; set; }
        public string Type { get; set; }
        public string Technology { get; set; }
        public List<SubCategorySubj2> SubCategories { get; set; }
    }

    public class SubCategorySubj2
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Security { get; set; }
        public string Image { get; set; }
        public string Type { get; set; }
        public string Technology { get; set; }
    }

    public class Category
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Security { get; set; }
        public string Image { get; set; }
        public string Type { get; set; }
        public string Technology { get; set; }
        public List<SubCategory> SubCategories { get; set; }
    }

    public class Audience
    {
        public string Name { get; set; }
        public string Image { get; set; }
    }
}
