namespace PencilCase.Web.Models.Responses;

public record SGApiMetadata(string run_id, ICollection<Object> feedback_tokens);