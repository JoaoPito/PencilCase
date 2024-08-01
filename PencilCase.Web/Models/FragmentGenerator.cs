using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Text.Json.Serialization;
using PencilCase.Shared.Models.StudyGuide;

namespace PencilCase.Web.Models;

public class FragmentGenerator : Fragment
{
    public FragmentGenerator() : base()
    {
        Endpoint = String.Empty;
        Description = String.Empty;
    }
    public FragmentGenerator(String name, 
        String content, 
        String description,
        String endpoint) : base(name, content)
    {
        Endpoint = endpoint;
        Description = description;
    }

    public string Description { get; set; }
    public string Endpoint { get; set; }
}