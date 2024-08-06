using PencilCase.Shared.Models;

namespace PencilCase.Web.StudyGuide.Models;

public class FragmentDropItem
    {
        public String Identifier { get; set; }
        public FragmentGenerator Generator { get; set; }
        public Fragment? Fragment { get; set; } = null;
    }