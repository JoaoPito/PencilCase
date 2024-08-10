using PencilCase.Shared.Models;

namespace PencilCase.Web.StudyGuide.Models;

public class FragmentDropItem
    {
        public String Identifier { get; set; } = String.Empty;
        public FragmentGenerator Generator { get; set; } = new();
        public Fragment? Fragment { get; set; } = null;
    }