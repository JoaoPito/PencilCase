using PencilCase.Shared.Models;

namespace PencilCase.Web.Pages.StudyGuideGenerator.Models;

public class FragmentDropItem
    {
        public String Identifier { get; set; } = String.Empty;
        public int Order { get; set; }
        public FragmentGenerator Generator { get; set; } = new();
        public Fragment? Fragment { get; set; } = null;
    }